using System;
using System.Linq;
using System.Windows.Threading;
using LearningReminder.Models;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 调度器：每秒心跳一次，负责当天首次排期、到点发起询问、超时未应答的自动推迟。
    /// 运行在 UI 线程，逻辑很轻（任务数量级为个位数），CPU 与内存开销可忽略。
    /// </summary>
    public sealed class SchedulerService
    {
        private readonly DispatcherTimer _timer;

        /// <summary>内部时钟滴答（界面用它刷新倒计时文本）</summary>
        public event EventHandler? Ticked;

        /// <summary>发起了一次新的询问（通知服务订阅此事件来弹通知）</summary>
        public event EventHandler<PendingCheckIn>? CheckInRaised;

        /// <summary>构造调度器。</summary>
        public SchedulerService()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(AppConstants.SchedulerTickMilliseconds)
            };
            _timer.Tick += OnTick;
        }

        /// <summary>启动调度。</summary>
        public void Start()
        {
            // 启动时先为今天的任务排期，避免等待第一个心跳
            PrepareToday(DateTime.Now);
            _timer.Start();
        }

        /// <summary>停止调度。</summary>
        public void Stop()
        {
            _timer.Stop();
        }

        /// <summary>立即为指定任务发起询问（手动"立即检查"与托盘菜单使用）。</summary>
        public void RaiseNow(LearningTask task, DateTime now)
        {
            PendingCheckIn pending = CheckInService.Instance.Raise(task, now);
            CheckInRaised?.Invoke(this, pending);
        }

        private void OnTick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            // EnsureToday 在跨天时会重建记录，排期逻辑随之重新初始化
            DataStore.Instance.EnsureToday();
            PrepareToday(now);

            LearningTask[] tasks = DataStore.Instance.Data.Tasks.ToArray();
            foreach (LearningTask task in tasks)
            {
                TryRaiseDueTask(task, now);
            }

            CheckInService.Instance.ExpireTimeout(now);
            Ticked?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>为今天尚未排期的任务安排第一次询问。</summary>
        private static void PrepareToday(DateTime now)
        {
            DailyRecord today = DataStore.Instance.Today;
            foreach (LearningTask task in DataStore.Instance.Data.Tasks)
            {
                if (!task.Enabled)
                {
                    continue;
                }

                TaskProgress progress = today.GetOrCreate(task.Id);
                if (progress.Initialized)
                {
                    continue;
                }

                SchedulePlanner.ApplyInitial(task, progress, now);
                progress.Initialized = true;
            }
        }

        private void TryRaiseDueTask(LearningTask task, DateTime now)
        {
            if (CheckInService.Instance.Find(task.Id) != null)
            {
                // 上一轮询问还没回答，先不追问
                return;
            }

            DailyRecord today = DataStore.Instance.Today;
            TaskProgress progress = today.GetOrCreate(task.Id);
            if (!SchedulePlanner.IsDue(task, progress, now))
            {
                return;
            }

            PendingCheckIn pending = CheckInService.Instance.Raise(task, now);
            FileLogger.Info("发起检查：" + task.Title);
            CheckInRaised?.Invoke(this, pending);
        }
    }
}