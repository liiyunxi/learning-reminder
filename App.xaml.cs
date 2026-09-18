using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LearningReminder.Resources;
using LearningReminder.Services;
using LearningReminder.Views;

namespace LearningReminder
{
    /// <summary>
    /// 应用入口：常驻托盘运行，主窗口只是"查看与编辑"的入口。
    /// </summary>
    public partial class App : Application
    {
        private SingleInstanceService? _singleInstance;
        private NotifyIconHost? _tray;
        private SchedulerService? _scheduler;
        private MainWindow? _mainWindow;
        private bool _exiting;

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            RegisterGlobalExceptionHandlers();

            // 单实例：已经在运行时，把本次启动参数转发过去后立即退出
            _singleInstance = new SingleInstanceService();
            if (!_singleInstance.TryAcquirePrimary())
            {
                _singleInstance.SendToPrimary(string.Join(" ", e.Args));
                Shutdown();
                return;
            }

            _singleInstance.ArgumentsReceived += OnArgumentsReceived;
            _singleInstance.StartListening();

            DataStore.Instance.Load();

            _tray = new NotifyIconHost();
            SyncAutoStartSetting();
            NotificationService.Instance.AttachTray(_tray);
            NotificationService.Instance.RegisterToastChannel();
            NotificationService.Instance.CheckCardRequested += OnCheckCardRequested;
            WireTrayEvents();

            _scheduler = new SchedulerService();
            _scheduler.CheckInRaised += OnCheckInRaised;
            CheckInService.Instance.PendingChanged += OnPendingChanged;
            DataStore.Instance.Changed += OnDataChanged;

            _scheduler.Start();

            // 先把托盘图标与提示刷新到位，避免出现"进程在跑但托盘里看不到"的情况
            RefreshTrayState();

            if (AppStartupOptions.IsMinimizedStart(e.Args))
            {
                // 开机自启场景：静默驻留托盘，给一次轻提示即可
                _tray.ShowBalloon(AppConstants.AppName, AppStrings.TrayStartedSilently);
            }
            else
            {
                ShowMainWindow();
            }

            // 由历史通知直接激活启动时，尝试解析通知按钮上的动作
            if (AppStartupOptions.TryParseAnswer(string.Join(" ", e.Args), out string taskId, out string action))
            {
                NotificationService.Instance.DispatchAction(action, taskId);
            }

            FileLogger.Info("应用启动完成");
        }

        /// <inheritdoc />
        protected override void OnExit(ExitEventArgs e)
        {
            DataStore.Instance.Save();
            _singleInstance?.Dispose();
            FileLogger.Info("应用退出");
            base.OnExit(e);
        }

        /// <summary>调度器（主界面订阅其心跳来刷新倒计时）。</summary>
        public SchedulerService? Scheduler => _scheduler;

        /// <summary>是否正在退出（主窗口据此决定"关闭即隐藏"还是真正关闭）。</summary>
        public bool IsExiting => _exiting;

        /// <summary>对单个任务立即发起询问。</summary>
        public void RaiseCheckNow(Models.LearningTask task)
        {
            _scheduler?.RaiseNow(task, DateTime.Now);
        }

        /// <summary>对今天所有未结束的任务立即发起询问。</summary>
        public void RaiseAllChecksNow()
        {
            RaiseAllChecks();
        }

        /// <summary>同步托盘菜单里的开机自启勾选状态。</summary>
        public void SyncAutoStartMenu()
        {
            if (_tray != null)
            {
                _tray.AutoStartItem.Checked = DataStore.Instance.Data.Settings.AutoStart;
            }
        }

        /// <summary>显示并激活主窗口。</summary>
        private void ShowMainWindow()
        {
            _mainWindow ??= new MainWindow();
            _mainWindow.ShowAndActivate();
        }

        private void ExitApplication()
        {
            if (_exiting)
            {
                return;
            }

            _exiting = true;
            _scheduler?.Stop();
            _tray?.Dispose();
            DataStore.Instance.Save();
            Shutdown();
        }

        private void WireTrayEvents()
        {
            if (_tray == null)
            {
                return;
            }

            _tray.OpenRequested += (sender, args) => ShowMainWindow();
            _tray.CheckAllRequested += (sender, args) => RaiseAllChecks();
            _tray.ExitRequested += (sender, args) => ExitApplication();
            _tray.AutoStartToggled += (sender, enabled) =>
            {
                DataStore.Instance.Data.Settings.AutoStart = enabled;
                AutoStartService.SetEnabled(enabled);
                DataStore.Instance.SaveAndNotify();
            };
        }

        /// <summary>托盘菜单与主界面的「立即检查」：对今天所有未结束的任务立刻发起询问。</summary>
        private void RaiseAllChecks()
        {
            if (_scheduler == null)
            {
                return;
            }

            DateTime now = DateTime.Now;
            int raised = 0;
            foreach (Models.LearningTask task in DataStore.Instance.Data.Tasks.ToArray())
            {
                if (!task.Enabled
                    || !task.ShouldRunOn(now.Date)
                    || CheckInService.Instance.Find(task.Id) != null)
                {
                    continue;
                }

                Models.TaskProgress progress = DataStore.Instance.Today.GetOrCreate(task.Id);
                if (SchedulePlanner.IsTaskFinished(task, progress))
                {
                    continue;
                }

                _scheduler.RaiseNow(task, now);
                raised++;
            }

            if (raised == 0 && _tray != null)
            {
                _tray.ShowBalloon(AppConstants.AppName, AppStrings.NothingToCheck);
            }
        }

        private void OnCheckInRaised(object? sender, PendingCheckIn pending)
        {
            NotificationService.Instance.NotifyCheckIn(pending);
        }

        private void OnCheckCardRequested(object? sender, PendingCheckIn pending)
        {
            CheckInWindow.ShowFor(pending);
        }

        private void OnPendingChanged(object? sender, EventArgs e)
        {
            RefreshTrayState();
        }

        private void OnDataChanged(object? sender, EventArgs e)
        {
            RefreshTrayState();
        }

        private void OnArgumentsReceived(object? sender, string arguments)
        {
            // 管道回调发生在线程池线程，统一切回 UI 线程处理
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (AppStartupOptions.TryParseAnswer(arguments, out string taskId, out string action))
                {
                    NotificationService.Instance.DispatchAction(action, taskId);
                    return;
                }

                ShowMainWindow();
            }));
        }

        /// <summary>托盘图标与悬停提示：有待确认项 → 橙色；今日全部完成 → 绿色。</summary>
        private void RefreshTrayState()
        {
            if (_tray == null)
            {
                return;
            }

            int pendingCount = CheckInService.Instance.Pending.Count;
            if (pendingCount > 0)
            {
                _tray.SetIcon(TrayIconFactory.PendingIcon);
                _tray.SetTooltip(string.Format(
                    AppStrings.TrayTipPendingFormat,
                    AppConstants.AppName,
                    pendingCount));
                return;
            }

            (int done, int total) = CountTodayProgress();
            _tray.SetIcon(total > 0 && done >= total
                ? TrayIconFactory.DoneIcon
                : TrayIconFactory.IdleIcon);
            _tray.SetTooltip(string.Format(
                AppStrings.TrayTipIdleFormat,
                AppConstants.AppName,
                done,
                total));
        }

        /// <summary>统计今日完成情况。</summary>
        private static (int Done, int Total) CountTodayProgress()
        {
            DayProgress progress = DailyStats.CountFor(DateTime.Today);
            return (progress.Done, progress.Total);
        }

        /// <summary>把数据里的自启配置和注册表实际状态对齐。</summary>
        private void SyncAutoStartSetting()
        {
            bool actuallyEnabled = AutoStartService.IsEnabled();
            Models.AppSettings settings = DataStore.Instance.Data.Settings;

            if (settings.AutoStart != actuallyEnabled)
            {
                // 以注册表为准（例如用户手工删除或系统重装后）
                settings.AutoStart = actuallyEnabled;
            }

            if (_tray != null)
            {
                _tray.AutoStartItem.Checked = actuallyEnabled;
            }
        }

        private void RegisterGlobalExceptionHandlers()
        {
            DispatcherUnhandledException += (sender, args) =>
            {
                FileLogger.Error("界面线程未处理异常", args.Exception);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                FileLogger.Error("应用域未处理异常", args.ExceptionObject as Exception);
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                FileLogger.Error("后台任务未观察异常", args.Exception);
                args.SetObserved();
            };
        }
    }
}