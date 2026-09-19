using System;
using System.Collections.Generic;
using LearningReminder.Models;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>某一天的统计行（统计页趋势图使用）。</summary>
    public sealed class DayStat
    {
        /// <summary>日期</summary>
        public DateTime Date { get; set; }

        /// <summary>当天完成数</summary>
        public int Done { get; set; }

        /// <summary>当天应完成数</summary>
        public int Total { get; set; }

        /// <summary>完成百分比（0-100）</summary>
        public int Percent { get; set; }

        /// <summary>当天学习时长（秒）</summary>
        public int StudySeconds { get; set; }
    }

    /// <summary>区间汇总。</summary>
    public sealed class RangeSummary
    {
        /// <summary>有任务的天数</summary>
        public int Days { get; set; }

        /// <summary>全部完成的天数</summary>
        public int FullDays { get; set; }

        /// <summary>完成任务次数</summary>
        public int Done { get; set; }

        /// <summary>应完成任务次数</summary>
        public int Total { get; set; }

        /// <summary>学习时长（秒）</summary>
        public int StudySeconds { get; set; }

        /// <summary>完成百分比（0-100）</summary>
        public int Percent => Total == 0 ? 0 : (int)Math.Round(Done * 100.0 / Total);
    }

    /// <summary>一条成就的展示信息。</summary>
    public sealed class Achievement
    {
        /// <summary>成就名称</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>成就说明</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>是否已解锁</summary>
        public bool Unlocked { get; set; }

        /// <summary>进度文案（如 3/7）</summary>
        public string ProgressText { get; set; } = string.Empty;
    }

    /// <summary>
    /// 统计服务：连续天数、区间汇总、成就评估。
    /// 全部基于本地保留的历史记录现算，不额外存冗余数据。
    /// </summary>
    public static class StatsService
    {
        /// <summary>
        /// 连续全完成天数：今天尚未完成不打断连击，没有任务的日子跳过。
        /// </summary>
        public static int CurrentStreak()
        {
            int streak = 0;
            DateTime day = DateTime.Today;
            for (int index = 0; index < AppConstants.HistoryKeepDays; index++, day = day.AddDays(-1))
            {
                DayProgress progress = DailyStats.CountFor(day);
                if (!progress.HasTask)
                {
                    continue;
                }

                if (progress.Percent >= 100)
                {
                    streak++;
                    continue;
                }

                if (day.Date == DateTime.Today)
                {
                    // 今天还在进行中，不算打断
                    continue;
                }

                break;
            }

            return streak;
        }

        /// <summary>历史窗口内最长的连续全完成天数。</summary>
        public static int MaxStreak()
        {
            int best = 0;
            int current = 0;
            DateTime day = DateTime.Today.AddDays(-AppConstants.HistoryKeepDays + 1);
            for (int index = 0; index < AppConstants.HistoryKeepDays; index++, day = day.AddDays(1))
            {
                DayProgress progress = DailyStats.CountFor(day);
                if (!progress.HasTask)
                {
                    continue;
                }

                if (progress.Percent >= 100)
                {
                    current++;
                    if (current > best)
                    {
                        best = current;
                    }
                }
                else if (day.Date != DateTime.Today)
                {
                    current = 0;
                }
            }

            return best;
        }

        /// <summary>按天聚合区间统计（含学习时长）。</summary>
        public static List<DayStat> DailyStatsRange(DateTime from, DateTime to)
        {
            List<DayStat> list = new List<DayStat>();
            for (DateTime day = from.Date; day <= to.Date; day = day.AddDays(1))
            {
                DayProgress progress = DailyStats.CountFor(day);
                list.Add(new DayStat
                {
                    Date = day,
                    Done = progress.Done,
                    Total = progress.Total,
                    Percent = progress.Percent,
                    StudySeconds = StudySecondsOf(day)
                });
            }

            return list;
        }

        /// <summary>汇总一组按天统计。</summary>
        public static RangeSummary Summarize(IReadOnlyList<DayStat> stats)
        {
            RangeSummary summary = new RangeSummary();
            foreach (DayStat stat in stats)
            {
                summary.Done += stat.Done;
                summary.Total += stat.Total;
                summary.StudySeconds += stat.StudySeconds;
                if (stat.Total > 0)
                {
                    summary.Days++;
                    if (stat.Percent >= 100)
                    {
                        summary.FullDays++;
                    }
                }
            }

            return summary;
        }

        /// <summary>某天累计学习时长（秒）。</summary>
        public static int StudySecondsOf(DateTime day)
        {
            DailyRecord? record = DataStore.Instance.FindRecord(day.ToString(AppConstants.DateFormat));
            if (record == null)
            {
                return 0;
            }

            int sum = 0;
            foreach (TaskProgress progress in record.Progress)
            {
                sum += progress.StudySeconds;
            }

            return sum;
        }

        /// <summary>历史窗口内累计完成任务次数。</summary>
        public static int TotalCompletedCount()
        {
            int count = 0;
            foreach (DailyRecord record in DataStore.Instance.Data.Records)
            {
                foreach (TaskProgress progress in record.Progress)
                {
                    if (progress.IsCompleted)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>历史窗口内累计学习时长（秒）。</summary>
        public static int TotalStudySeconds()
        {
            int sum = 0;
            foreach (DailyRecord record in DataStore.Instance.Data.Records)
            {
                foreach (TaskProgress progress in record.Progress)
                {
                    sum += progress.StudySeconds;
                }
            }

            return sum;
        }

        /// <summary>已完成里程碑个数（当前任务清单口径）。</summary>
        public static int DoneMilestones()
        {
            int count = 0;
            foreach (LearningTask task in DataStore.Instance.Data.Tasks)
            {
                foreach (MilestoneItem item in task.Milestones)
                {
                    if (item.Done)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>历史窗口内早于设定时间的完成次数（早起打卡）。</summary>
        public static int EarlyCompletionCount()
        {
            int count = 0;
            foreach (DailyRecord record in DataStore.Instance.Data.Records)
            {
                foreach (CheckInLogEntry entry in record.Logs)
                {
                    if (entry.Answer == CheckInAnswer.Completed && entry.Time.Hour < AppConstants.EarlyBirdHour)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>把秒数格式化为"X 小时 Y 分 / Y 分钟 / X 秒"。</summary>
        public static string FormatStudyDuration(int seconds)
        {
            int minutes = seconds / AppConstants.SecondsPerMinute;
            if (minutes < 1)
            {
                return string.Format(AppStrings.DurationUnderMinuteFormat, seconds);
            }

            int hours = minutes / AppConstants.MinutesPerHour;
            int rest = minutes % AppConstants.MinutesPerHour;
            return hours > 0
                ? string.Format(AppStrings.DurationHourMinuteFormat, hours, rest)
                : string.Format(AppStrings.DurationMinuteFormat, minutes);
        }

        /// <summary>评估全部成就的解锁状态。</summary>
        public static List<Achievement> EvaluateAchievements()
        {
            int completed = TotalCompletedCount();
            int studySeconds = TotalStudySeconds();
            int maxStreak = MaxStreak();
            int milestones = DoneMilestones();
            int early = EarlyCompletionCount();

            return new List<Achievement>
            {
                Build(AppStrings.AchvFirstDone, AppStrings.AchvFirstDoneDesc, completed, 1),
                Build(AppStrings.AchvStreak3, AppStrings.AchvStreak3Desc, maxStreak, 3),
                Build(AppStrings.AchvStreak7, AppStrings.AchvStreak7Desc, maxStreak, 7),
                Build(AppStrings.AchvStreak15, AppStrings.AchvStreak15Desc, maxStreak, 15),
                Build(AppStrings.AchvDone50, AppStrings.AchvDone50Desc, completed, 50),
                Build(AppStrings.AchvDone200, AppStrings.AchvDone200Desc, completed, 200),
                Build(
                    AppStrings.AchvStudy10h,
                    AppStrings.AchvStudy10hDesc,
                    studySeconds,
                    10 * AppConstants.MinutesPerHour * AppConstants.SecondsPerMinute),
                Build(
                    AppStrings.AchvStudy50h,
                    AppStrings.AchvStudy50hDesc,
                    studySeconds,
                    50 * AppConstants.MinutesPerHour * AppConstants.SecondsPerMinute),
                Build(AppStrings.AchvMilestone10, AppStrings.AchvMilestone10Desc, milestones, 10),
                Build(AppStrings.AchvEarlyBird, AppStrings.AchvEarlyBirdDesc, early, 5)
            };
        }

        private static Achievement Build(string name, string description, int current, int target)
        {
            return new Achievement
            {
                Name = name,
                Description = description,
                Unlocked = current >= target,
                ProgressText = string.Format(AppStrings.AchvProgressFormat, Math.Min(current, target), target)
            };
        }
    }
}