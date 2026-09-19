using System;
using System.Collections.Generic;
using System.Globalization;
using LearningReminder.Models;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 检查（询问）服务：维护待确认队列，处理用户的四种应答，并写入记录。
    /// 仅在 UI 线程调用。
    /// </summary>
    public sealed class CheckInService
    {
        private static readonly Lazy<CheckInService> LazyInstance =
            new Lazy<CheckInService>(() => new CheckInService());

        private readonly List<PendingCheckIn> _pending = new List<PendingCheckIn>();

        private CheckInService()
        {
        }

        /// <summary>全局唯一实例</summary>
        public static CheckInService Instance => LazyInstance.Value;

        /// <summary>待确认队列发生变化</summary>
        public event EventHandler? PendingChanged;

        /// <summary>当前待确认列表</summary>
        public IReadOnlyList<PendingCheckIn> Pending => _pending;

        /// <summary>是否存在待确认项</summary>
        public bool HasPending => _pending.Count > 0;

        /// <summary>取指定任务的待确认项。</summary>
        public PendingCheckIn? Find(string taskId)
        {
            foreach (PendingCheckIn item in _pending)
            {
                if (item.TaskId == taskId)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 发起一次询问：记录询问时间、加入待确认队列、清除排期（等待应答后重新排期）。
        /// </summary>
        public PendingCheckIn Raise(LearningTask task, DateTime now)
        {
            DailyRecord today = DataStore.Instance.Today;
            TaskProgress progress = today.GetOrCreate(task.Id);
            progress.LastCheckAt = now;
            progress.NextCheckAt = null;
            progress.SnoozeUntil = null;

            if (task.Mode == CheckMode.Milestone)
            {
                progress.DailyReminderRaised = true;
            }

            PendingCheckIn? exist = Find(task.Id);
            if (exist != null)
            {
                // 同一任务已有待确认项时不再重复入队，仅刷新时间与链接
                exist.RaisedAt = now;
                exist.LinkUrl = task.LinkUrl;
                PendingChanged?.Invoke(this, EventArgs.Empty);
                return exist;
            }

            PendingCheckIn pending = new PendingCheckIn
            {
                TaskId = task.Id,
                Title = task.Title,
                LinkUrl = task.LinkUrl,
                Mode = task.Mode,
                RaisedAt = now,
                DoneCount = task.DoneMilestoneCount(),
                TotalCount = task.Milestones.Count
            };
            _pending.Add(pending);

            DataStore.Instance.SaveAndNotify();
            PendingChanged?.Invoke(this, EventArgs.Empty);
            return pending;
        }

        /// <summary>用户点了「已完成」。</summary>
        public void Complete(string taskId, DateTime now)
        {
            LearningTask? task = DataStore.Instance.FindTask(taskId);
            if (task == null)
            {
                RemovePending(taskId);
                return;
            }

            DailyRecord today = DataStore.Instance.Today;
            TaskProgress progress = today.GetOrCreate(taskId);

            // 里程碑模式下「已完成」表示所有小目标都完成了，避免与勾选状态自相矛盾
            if (task.Mode == CheckMode.Milestone)
            {
                foreach (MilestoneItem item in task.Milestones)
                {
                    MarkMilestone(item, done: true, now);
                }
            }

            progress.IsCompleted = true;
            progress.CompletedAt = now;
            progress.NotYetStreak = 0;
            StudySessionService.Stop(progress, now);
            SchedulePlanner.Clear(progress);
            AddLog(today, task, CheckInAnswer.Completed, string.Empty);
            RemovePending(taskId);
            FileLogger.Info("任务完成：" + task.Title);
            DataStore.Instance.SaveAndNotify();
        }

        /// <summary>用户点了「还没完成」，按间隔继续追问。</summary>
        public void NotYet(string taskId, DateTime now)
        {
            LearningTask? task = DataStore.Instance.FindTask(taskId);
            if (task == null)
            {
                RemovePending(taskId);
                return;
            }

            DailyRecord today = DataStore.Instance.Today;
            TaskProgress progress = today.GetOrCreate(taskId);
            progress.NotYetStreak++;
            SchedulePlanner.ApplyRepeat(task, progress, now);

            AddLog(today, task, CheckInAnswer.NotYet, string.Empty);
            RemovePending(taskId);
            DataStore.Instance.SaveAndNotify();
        }

        /// <summary>推迟询问（markAuto 表示因为没人应答而自动推迟）。</summary>
        public void Snooze(string taskId, DateTime now, bool markAuto)
        {
            LearningTask? task = DataStore.Instance.FindTask(taskId);
            if (task == null)
            {
                RemovePending(taskId);
                return;
            }

            DailyRecord today = DataStore.Instance.Today;
            TaskProgress progress = today.GetOrCreate(taskId);
            int minutes = DataStore.Instance.Data.Settings.SnoozeMinutes;
            SchedulePlanner.ApplySnooze(progress, now, minutes);

            AddLog(
                today,
                task,
                markAuto ? CheckInAnswer.AutoSnoozed : CheckInAnswer.Snoozed,
                string.Format("{0} 分钟", minutes));
            RemovePending(taskId);
            DataStore.Instance.SaveAndNotify();
        }

        /// <summary>
        /// 应用确认卡片上勾选的里程碑；全部勾完视为完成，否则按间隔继续追问。
        /// </summary>
        public void ApplyMilestones(string taskId, IEnumerable<string> doneMilestoneIds, DateTime now)
        {
            LearningTask? task = DataStore.Instance.FindTask(taskId);
            if (task == null)
            {
                RemovePending(taskId);
                return;
            }

            HashSet<string> selected = new HashSet<string>(doneMilestoneIds);
            foreach (MilestoneItem item in task.Milestones)
            {
                MarkMilestone(item, selected.Contains(item.Id), now);
            }

            DailyRecord today = DataStore.Instance.Today;
            TaskProgress progress = today.GetOrCreate(taskId);
            bool finished = task.IsMilestoneFinished();

            if (finished)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = now;
                progress.NotYetStreak = 0;
                StudySessionService.Stop(progress, now);
                SchedulePlanner.Clear(progress);
                AddLog(today, task, CheckInAnswer.Completed, BuildMilestoneDetail(task));
            }
            else
            {
                SchedulePlanner.ApplyRepeat(task, progress, now);
                AddLog(today, task, CheckInAnswer.Partial, BuildMilestoneDetail(task));
            }

            RemovePending(taskId);
            DataStore.Instance.SaveAndNotify();
        }

        /// <summary>处理超时未应答的待确认项，按自动推迟处理。</summary>
        public void ExpireTimeout(DateTime now)
        {
            List<PendingCheckIn> expired = new List<PendingCheckIn>();
            foreach (PendingCheckIn item in _pending)
            {
                if ((now - item.RaisedAt).TotalSeconds >= AppConstants.PendingTimeoutSeconds)
                {
                    expired.Add(item);
                }
            }

            foreach (PendingCheckIn item in expired)
            {
                Snooze(item.TaskId, now, markAuto: true);
            }
        }

        /// <summary>补打卡：把过去某天未完成的任务补记为已完成。</summary>
        public void Backfill(string taskId, string dateKey, DateTime now)
        {
            LearningTask? task = DataStore.Instance.FindTask(taskId);
            DailyRecord? record = DataStore.Instance.FindRecord(dateKey);
            if (task == null || record == null)
            {
                return;
            }

            TaskProgress progress = record.GetOrCreate(taskId);
            if (progress.IsCompleted)
            {
                return;
            }

            if (task.Mode == CheckMode.Milestone)
            {
                // 与「已完成」口径一致：里程碑模式下把全部小目标标记完成
                foreach (MilestoneItem item in task.Milestones)
                {
                    MarkMilestone(item, done: true, now);
                }
            }

            progress.IsCompleted = true;
            progress.CompletedAt = ResolveBackfillTime(dateKey, now);
            progress.NotYetStreak = 0;
            SchedulePlanner.Clear(progress);

            record.Logs.Add(new CheckInLogEntry
            {
                Time = now,
                TaskId = task.Id,
                TaskTitle = task.Title,
                Answer = CheckInAnswer.Backfill,
                Detail = string.Empty
            });

            FileLogger.Info("补打卡：" + task.Title + " @ " + dateKey);
            DataStore.Instance.SaveAndNotify();
        }

        /// <summary>清空全部待确认项（导入数据后调用）。</summary>
        public void ClearAll()
        {
            if (_pending.Count == 0)
            {
                return;
            }

            _pending.Clear();
            PendingChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>真实任务被删除或停用时，清理对应待确认项。</summary>
        public void RemovePending(string taskId)
        {
            int removed = _pending.RemoveAll(item => item.TaskId == taskId);
            if (removed > 0)
            {
                PendingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private static void MarkMilestone(MilestoneItem item, bool done, DateTime now)
        {
            if (item.Done == done)
            {
                return;
            }

            item.Done = done;
            item.DoneAt = done ? now : null;
        }

        /// <summary>补打卡的完成时间：取目标日期的中午，避免显示为操作时间。</summary>
        private static DateTime ResolveBackfillTime(string dateKey, DateTime now)
        {
            if (DateTime.TryParseExact(
                dateKey,
                AppConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime day))
            {
                return day.AddHours(12);
            }

            return now;
        }

        private static string BuildMilestoneDetail(LearningTask task)
        {
            return string.Format(
                AppStrings.LogAnswerPartialFormat,
                task.DoneMilestoneCount(),
                task.Milestones.Count);
        }

        private static void AddLog(DailyRecord today, LearningTask task, CheckInAnswer answer, string detail)
        {
            today.Logs.Add(new CheckInLogEntry
            {
                Time = DateTime.Now,
                TaskId = task.Id,
                TaskTitle = task.Title,
                Answer = answer,
                Detail = detail
            });
        }
    }
}