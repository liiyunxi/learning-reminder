using System;
using LearningReminder.Models;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 排期规则（纯计算，不依赖界面与存储）：
    /// 决定"下一次什么时候问"。
    /// </summary>
    public static class SchedulePlanner
    {
        /// <summary>
        /// 当天首次排期：
        /// 定时询问模式以当天起始小时为基准顺延一个间隔；里程碑模式优先使用每日核对时间。
        /// </summary>
        public static void ApplyInitial(LearningTask task, TaskProgress progress, DateTime now)
        {
            if (!task.ShouldRunOn(now.Date))
            {
                // 今天不是执行日：不给排期，也不打扰
                progress.NextCheckAt = null;
                return;
            }

            if (task.Mode == CheckMode.Milestone)
            {
                if (TryGetDailyReminderToday(task.DailyReminderTime, out DateTime reminder))
                {
                    progress.NextCheckAt = reminder;
                    progress.DailyReminderRaised = false;
                }
                else
                {
                    // 未设置每日核对时间：里程碑任务不主动打扰，只按手动检查推进
                    progress.NextCheckAt = null;
                }

                return;
            }

            DateTime dayStart = now.Date.AddHours(AppConstants.DayStartHour);
            DateTime candidate = dayStart.AddMinutes(EffectiveInterval(task, progress));
            progress.NextCheckAt = candidate > now ? candidate : now.AddMinutes(EffectiveInterval(task, progress));
        }

        /// <summary>应答之后重新排期：继续追问则按有效间隔顺延。</summary>
        public static void ApplyRepeat(LearningTask task, TaskProgress progress, DateTime now)
        {
            progress.NextCheckAt = now.AddMinutes(EffectiveInterval(task, progress));
            progress.SnoozeUntil = null;
        }

        /// <summary>
        /// 有效询问间隔（间隔自适应）：连续"还没完成"时逐步放缓（×1.5、×2），
        /// 避免越问越频繁造成打扰，最长不超过 24 小时。
        /// </summary>
        public static int EffectiveInterval(LearningTask task, TaskProgress progress)
        {
            int baseMinutes = NormalizeInterval(task.IntervalMinutes);
            double factor = progress.NotYetStreak >= 2 ? 2.0 : (progress.NotYetStreak == 1 ? 1.5 : 1.0);
            int minutes = (int)Math.Round(baseMinutes * factor);
            return minutes > AppConstants.MaxIntervalMinutes ? AppConstants.MaxIntervalMinutes : minutes;
        }

        /// <summary>推迟到指定分钟数之后再问。</summary>
        public static void ApplySnooze(TaskProgress progress, DateTime now, int minutes)
        {
            DateTime target = now.AddMinutes(minutes < 1 ? AppConstants.DefaultSnoozeMinutes : minutes);
            progress.NextCheckAt = target;
            progress.SnoozeUntil = target;
        }

        /// <summary>任务结束（已完成或里程碑全部勾完）后清除排期。</summary>
        public static void Clear(TaskProgress progress)
        {
            progress.NextCheckAt = null;
            progress.SnoozeUntil = null;
        }

        /// <summary>
        /// 判断此刻是否应该发起询问。
        /// </summary>
        public static bool IsDue(LearningTask task, TaskProgress progress, DateTime now)
        {
            if (!task.Enabled || !task.ShouldRunOn(now.Date) || IsTaskFinished(task, progress))
            {
                return false;
            }

            // 免打扰时段内不打扰：到点的排期直接跳过（不补发）
            if (QuietHours.IsQuiet(now))
            {
                return false;
            }

            // 里程碑模式：未触发过每日核对，且已到核对时间
            if (task.Mode == CheckMode.Milestone
                && !progress.DailyReminderRaised
                && progress.NextCheckAt == null
                && TryGetDailyReminderToday(task.DailyReminderTime, out DateTime reminder)
                && now >= reminder)
            {
                return true;
            }

            if (progress.NextCheckAt == null)
            {
                return false;
            }

            return now >= progress.NextCheckAt.Value;
        }

        /// <summary>任务是否已经结束：定时询问看当天是否完成，里程碑看是否全部勾完。</summary>
        public static bool IsTaskFinished(LearningTask task, TaskProgress progress)
        {
            if (task.Mode == CheckMode.Milestone)
            {
                return task.IsMilestoneFinished();
            }

            return progress.IsCompleted;
        }

        /// <summary>解析每日核对时间。</summary>
        public static bool TryGetDailyReminderToday(string dailyReminderTime, out DateTime reminder)
        {
            reminder = default;

            if (string.IsNullOrWhiteSpace(dailyReminderTime))
            {
                return false;
            }

            if (!TimeSpan.TryParseExact(dailyReminderTime.Trim(), @"hh\:mm", null, out TimeSpan time))
            {
                return false;
            }

            reminder = DateTime.Today.Add(time);
            return true;
        }

        /// <summary>把间隔限制在合理范围内。</summary>
        public static int NormalizeInterval(int minutes)
        {
            if (minutes < AppConstants.MinIntervalMinutes)
            {
                return AppConstants.DefaultIntervalMinutes;
            }

            return minutes > AppConstants.MaxIntervalMinutes ? AppConstants.MaxIntervalMinutes : minutes;
        }
    }
}