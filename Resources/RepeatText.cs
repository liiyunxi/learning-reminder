using System;
using System.Collections.Generic;
using LearningReminder.Models;

namespace LearningReminder.Resources
{
    /// <summary>
    /// 重复规则与星期的中文文案生成（界面与卡片共用，避免各处拼字符串）。
    /// </summary>
    public static class RepeatText
    {
        /// <summary>星期的完整中文名，例如"周一"。</summary>
        public static string WeekdayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday:
                    return AppStrings.WeekdayMonday;
                case DayOfWeek.Tuesday:
                    return AppStrings.WeekdayTuesday;
                case DayOfWeek.Wednesday:
                    return AppStrings.WeekdayWednesday;
                case DayOfWeek.Thursday:
                    return AppStrings.WeekdayThursday;
                case DayOfWeek.Friday:
                    return AppStrings.WeekdayFriday;
                case DayOfWeek.Saturday:
                    return AppStrings.WeekdaySaturday;
                default:
                    return AppStrings.WeekdaySunday;
            }
        }

        /// <summary>星期的单字，用于"每周一、三、五"这类紧凑文案。</summary>
        public static string WeekdayShortName(DayOfWeek day)
        {
            string full = WeekdayName(day);
            return full.Length > 1 ? full.Substring(1) : full;
        }

        /// <summary>生成重复规则的中文描述，例如"每天"/"每周一"/"每月 15 日"/"每周一、三"。</summary>
        public static string Describe(RepeatRule? rule)
        {
            if (rule == null)
            {
                return AppStrings.RepeatDaily;
            }

            switch (rule.Type)
            {
                case RepeatType.Weekly:
                    return rule.Weekdays.Count > 0
                        ? string.Format(AppStrings.RepeatWeeklyFormat, WeekdayShortName(rule.Weekdays[0]))
                        : AppStrings.RepeatWeekly;

                case RepeatType.Monthly:
                    return string.Format(AppStrings.RepeatMonthlyFormat, rule.DayOfMonth);

                case RepeatType.Custom:
                    return rule.Weekdays.Count > 0
                        ? string.Format(AppStrings.RepeatCustomFormat, JoinWeekdays(rule.Weekdays))
                        : AppStrings.RepeatCustom;

                default:
                    return AppStrings.RepeatDaily;
            }
        }

        /// <summary>把多个星期拼成"一、三、五"。</summary>
        public static string JoinWeekdays(IEnumerable<DayOfWeek> weekdays)
        {
            List<string> names = new List<string>();
            foreach (DayOfWeek day in weekdays)
            {
                names.Add(WeekdayShortName(day));
            }

            return string.Join(AppStrings.RepeatWeekdayJoiner, names);
        }
    }
}