using System;
using System.Collections.Generic;

namespace LearningReminder.Models
{
    /// <summary>
    /// 任务的重复规则：决定哪些日子需要执行与提醒。
    /// </summary>
    public class RepeatRule
    {
        /// <summary>重复方式</summary>
        public RepeatType Type { get; set; } = RepeatType.Daily;

        /// <summary>每周几（Weekly 用第一个；Custom 用全部）</summary>
        public List<DayOfWeek> Weekdays { get; set; } = new List<DayOfWeek>();

        /// <summary>每月几号（1-31）</summary>
        public int DayOfMonth { get; set; } = 1;

        /// <summary>
        /// 指定日期是否要执行该任务。
        /// </summary>
        public bool ShouldRunOn(DateTime date)
        {
            switch (Type)
            {
                case RepeatType.Weekly:
                    return Weekdays.Count > 0 && Weekdays.Contains(date.DayOfWeek);

                case RepeatType.Monthly:
                    // 当月没有该日期时（如 2 月没有 31 号）顺延到当月最后一天
                    int effectiveDay = Math.Min(DayOfMonth, DateTime.DaysInMonth(date.Year, date.Month));
                    return date.Day == effectiveDay;

                case RepeatType.Custom:
                    return Weekdays.Count > 0 && Weekdays.Contains(date.DayOfWeek);

                default:
                    return true;
            }
        }

        /// <summary>创建一个"每天"的默认规则。</summary>
        public static RepeatRule CreateDaily()
        {
            return new RepeatRule { Type = RepeatType.Daily };
        }
    }
}