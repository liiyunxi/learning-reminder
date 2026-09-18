using System;
using LearningReminder.Models;

namespace LearningReminder.Services
{
    /// <summary>
    /// 任务状态同步：让"当天完成标记"与里程碑勾选结果始终一致。
    /// 例如用户直接在清单里勾完最后一个里程碑，或给已完成的任务新增了里程碑。
    /// </summary>
    public static class TaskStateSync
    {
        /// <summary>按当前里程碑勾选情况，校正当天的完成状态。</summary>
        public static void SyncMilestoneCompletion(LearningTask task, DateTime now)
        {
            if (task.Mode != CheckMode.Milestone)
            {
                return;
            }

            DailyRecord today = DataStore.Instance.Today;
            TaskProgress progress = today.GetOrCreate(task.Id);
            bool finished = task.IsMilestoneFinished();

            if (finished && !progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = now;
                SchedulePlanner.Clear(progress);
                return;
            }

            if (!finished && progress.IsCompleted)
            {
                // 新增了未完成的里程碑：恢复提醒
                progress.IsCompleted = false;
                progress.CompletedAt = null;
                SchedulePlanner.ApplyRepeat(task, progress, now);
            }
        }
    }
}