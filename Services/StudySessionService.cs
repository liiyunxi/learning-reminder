using System;
using LearningReminder.Models;

namespace LearningReminder.Services
{
    /// <summary>
    /// 学习计时：在任务卡片上"开始学习 / 结束学习"，把本次时长累计到当天。
    /// 计时状态存在当天进度里，跨天不延续（未结束时长的不足一天部分不计入）。
    /// </summary>
    public static class StudySessionService
    {
        /// <summary>切换计时状态，返回切换后是否处于计时中。</summary>
        public static bool Toggle(LearningTask task, DateTime now)
        {
            TaskProgress progress = DataStore.Instance.Today.GetOrCreate(task.Id);
            if (progress.SessionStartedAt == null)
            {
                progress.SessionStartedAt = now;
                DataStore.Instance.SaveAndNotify();
                return true;
            }

            Stop(progress, now);
            DataStore.Instance.SaveAndNotify();
            return false;
        }

        /// <summary>结束计时并把本次时长累加到当天。</summary>
        public static void Stop(TaskProgress progress, DateTime now)
        {
            if (progress.SessionStartedAt == null)
            {
                return;
            }

            double seconds = (now - progress.SessionStartedAt.Value).TotalSeconds;
            if (seconds > 0)
            {
                progress.StudySeconds += (int)Math.Round(seconds);
            }

            progress.SessionStartedAt = null;
        }

        /// <summary>取当前进行中的学习时长（秒），未在计时返回 0。</summary>
        public static int ElapsedSeconds(TaskProgress progress, DateTime now)
        {
            if (progress.SessionStartedAt == null)
            {
                return 0;
            }

            double seconds = (now - progress.SessionStartedAt.Value).TotalSeconds;
            return seconds > 0 ? (int)seconds : 0;
        }
    }
}