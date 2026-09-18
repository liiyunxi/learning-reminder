using System;
using LearningReminder.Models;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 每日完成情况统计（今天与历史日期共用同一套口径，保证界面与日历一致）。
    /// </summary>
    public static class DailyStats
    {
        /// <summary>
        /// 统计指定日期的完成情况：只统计"那天应该执行、且那天之前已创建"的任务。
        /// </summary>
        public static DayProgress CountFor(DateTime date)
        {
            DataStore store = DataStore.Instance;
            DailyRecord? record = store.FindRecord(date.ToString(AppConstants.DateFormat));

            int done = 0;
            int total = 0;
            foreach (LearningTask task in store.Data.Tasks)
            {
                if (!task.Enabled
                    || task.CreatedAt.Date > date.Date
                    || !task.ShouldRunOn(date))
                {
                    continue;
                }

                total++;
                TaskProgress? progress = record?.Find(task.Id);
                if (progress != null && progress.IsCompleted)
                {
                    done++;
                }
            }

            return new DayProgress(done, total);
        }
    }

    /// <summary>
    /// 某一天的完成情况。
    /// </summary>
    public sealed class DayProgress
    {
        /// <summary>构造统计结果。</summary>
        public DayProgress(int done, int total)
        {
            Done = done;
            Total = total;
        }

        /// <summary>已完成数量</summary>
        public int Done { get; }

        /// <summary>当天应执行的任务总数</summary>
        public int Total { get; }

        /// <summary>完成百分比（0-100）</summary>
        public int Percent => Total == 0 ? 0 : (int)Math.Round(Done * 100.0 / Total);

        /// <summary>当天是否有任务安排</summary>
        public bool HasTask => Total > 0;
    }
}