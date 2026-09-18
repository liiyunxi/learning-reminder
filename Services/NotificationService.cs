using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using CommunityToolkit.WinUI.Notifications;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 通知服务：负责把"该确认进度了"送到用户面前。
    /// 主通道是 Windows 系统通知（带操作按钮），不可用时退回托盘气泡；
    /// 若用户没有应答，则在延迟后弹出确认卡片，保证一定有可点的按钮。
    /// </summary>
    public sealed class NotificationService
    {
        private static readonly Lazy<NotificationService> LazyInstance =
            new Lazy<NotificationService>(() => new NotificationService());

        private NotifyIconHost? _tray;
        private DispatcherTimer? _cardDelayTimer;
        private SynchronizationContext? _uiContext;
        private bool _toastAvailable = true;

        private NotificationService()
        {
        }

        /// <summary>全局唯一实例</summary>
        public static NotificationService Instance => LazyInstance.Value;

        /// <summary>请求显示确认卡片（界面层订阅）</summary>
        public event EventHandler<PendingCheckIn>? CheckCardRequested;

        /// <summary>绑定托盘宿主，用于气泡回落与气泡点击回调。</summary>
        public void AttachTray(NotifyIconHost tray)
        {
            _tray = tray;
            _tray.BalloonClicked += OnBalloonClicked;
        }

        /// <summary>注册系统通知通道（必须在 UI 线程调用）。</summary>
        public void RegisterToastChannel()
        {
            _uiContext = SynchronizationContext.Current;
            try
            {
                ToastNotificationManagerCompat.OnActivated += OnToastActivated;
            }
            catch (Exception ex)
            {
                _toastAvailable = false;
                FileLogger.Error("注册系统通知通道失败，将使用托盘气泡", ex);
            }
        }

        /// <summary>
        /// 提示一项待确认的检查。
        /// </summary>
        public void NotifyCheckIn(PendingCheckIn pending)
        {
            bool toastShown = TryShowToast(pending);
            if (!toastShown)
            {
                ShowBalloon(pending);
            }

            ScheduleCardFallback();
        }

        /// <summary>直接显示确认卡片（点击通知或托盘气泡、托盘菜单入口时调用）。</summary>
        public void RequestCheckCard(PendingCheckIn pending)
        {
            CheckCardRequested?.Invoke(this, pending);
        }

        /// <summary>隐藏到托盘时给一次提示，避免用户找不到托盘图标。</summary>
        public void NotifyHiddenToTray()
        {
            _tray?.ShowBalloon(AppConstants.AppName, AppStrings.TrayHiddenHint);
        }

        private bool TryShowToast(PendingCheckIn pending)
        {
            if (!_toastAvailable)
            {
                return false;
            }

            try
            {
                string body = pending.Mode == Models.CheckMode.Milestone
                    ? string.Format(AppStrings.ToastBodyMilestoneFormat, pending.DoneCount, pending.TotalCount)
                    : AppStrings.ToastBodyInterval;

                ToastContentBuilder builder = new ToastContentBuilder()
                    .AddArgument(ToastArgumentKeys.Action, ToastActions.Open)
                    .AddArgument(ToastArgumentKeys.TaskId, pending.TaskId)
                    .AddText(pending.Title)
                    .AddText(body);

                // 任务填了学习链接时，通知上直接给一个"立即学习"入口
                if (!string.IsNullOrWhiteSpace(pending.LinkUrl))
                {
                    builder.AddButton(BuildButton(AppStrings.ButtonLearnNow, ToastActions.Learn, pending.TaskId));
                }

                builder
                    .AddButton(BuildButton(AppStrings.AnswerDone, ToastActions.Done, pending.TaskId))
                    .AddButton(BuildButton(AppStrings.AnswerNotYet, ToastActions.NotYet, pending.TaskId))
                    .AddButton(BuildButton(
                        string.Format(AppStrings.AnswerSnoozeFormat, DataStore.Instance.Data.Settings.SnoozeMinutes),
                        ToastActions.Snooze,
                        pending.TaskId))
                    .Show();

                return true;
            }
            catch (Exception ex)
            {
                // 系统通知在未注册/被策略禁用时会失败，降级到托盘气泡
                _toastAvailable = false;
                FileLogger.Error("显示系统通知失败，改用托盘气泡", ex);
                return false;
            }
        }

        private static ToastButton BuildButton(string content, string action, string taskId)
        {
            return new ToastButton()
                .SetContent(content)
                .AddArgument(ToastArgumentKeys.Action, action)
                .AddArgument(ToastArgumentKeys.TaskId, taskId);
        }

        private void ShowBalloon(PendingCheckIn pending)
        {
            if (_tray == null)
            {
                return;
            }

            // 气泡不支持按钮，点击气泡时直接打开确认卡片
            _tray.ShowBalloon(
                AppStrings.CheckCardTitle,
                string.Format("{0}{1}{2}", pending.Title, Environment.NewLine, AppStrings.ToastBodyPlain));
        }

        private void OnBalloonClicked(object? sender, EventArgs e)
        {
            // 托盘气泡无法携带任务信息，这里按"最新一条待确认"处理
            IReadOnlyList<PendingCheckIn> pending = CheckInService.Instance.Pending;
            if (pending.Count > 0)
            {
                RequestCheckCard(pending[pending.Count - 1]);
            }
        }

        private void ScheduleCardFallback()
        {
            if (!DataStore.Instance.Data.Settings.AutoShowCheckCard)
            {
                return;
            }

            _cardDelayTimer?.Stop();
            _cardDelayTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(AppConstants.CheckCardDelaySeconds)
            };

            _cardDelayTimer.Tick += (sender, args) =>
            {
                _cardDelayTimer?.Stop();

                // 到点时仍未应答的，全部弹确认卡片（避免通知被忽略后无处可点）
                foreach (PendingCheckIn item in CheckInService.Instance.Pending)
                {
                    RequestCheckCard(item);
                }
            };
            _cardDelayTimer.Start();
        }

        /// <summary>打开任务的学习链接（可从通知按钮、确认卡片触发）。</summary>
        public void OpenLearningLink(string taskId)
        {
            Models.LearningTask? task = DataStore.Instance.FindTask(taskId);
            if (task == null || !task.HasLink)
            {
                return;
            }

            if (!LinkLauncher.TryOpen(task.LinkUrl))
            {
                _tray?.ShowBalloon(AppStrings.ButtonLearnNow, AppStrings.LinkOpenFailed);
            }
        }

        private void OnToastActivated(ToastNotificationActivatedEventArgsCompat args)
        {
            string action = QueryStringHelper.GetValue(args.Argument, ToastArgumentKeys.Action);
            string taskId = QueryStringHelper.GetValue(args.Argument, ToastArgumentKeys.TaskId);
            DispatchAction(action, taskId);
        }

        /// <summary>把通知上的操作应用到业务层（切回 UI 线程执行）。</summary>
        public void DispatchAction(string action, string taskId)
        {
            if (string.IsNullOrEmpty(action))
            {
                return;
            }

            void Execute()
            {
                DateTime now = DateTime.Now;
                switch (action)
                {
                    case ToastActions.Done:
                        CheckInService.Instance.Complete(taskId, now);
                        break;
                    case ToastActions.NotYet:
                        CheckInService.Instance.NotYet(taskId, now);
                        break;
                    case ToastActions.Snooze:
                        CheckInService.Instance.Snooze(taskId, now, markAuto: false);
                        break;
                    case ToastActions.Learn:
                        OpenLearningLink(taskId);
                        break;
                    case ToastActions.Open:
                        PendingCheckIn? pending = CheckInService.Instance.Find(taskId);
                        if (pending != null)
                        {
                            RequestCheckCard(pending);
                        }

                        break;
                    default:
                        FileLogger.Info("收到未知的通知操作：" + action);
                        break;
                }
            }

            if (_uiContext != null)
            {
                _uiContext.Post(_ => Execute(), null);
            }
            else
            {
                Execute();
            }
        }
    }
}