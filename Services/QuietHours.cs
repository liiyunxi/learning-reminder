using System;
using LearningReminder.Models;

namespace LearningReminder.Services
{
    /// <summary>
    /// 免打扰时段判断（纯计算）：
    /// 时段内不发起任何询问与提醒，到点的排期直接跳过（不补发）。
    /// </summary>
    public static class QuietHours
    {
        /// <summary>按当前设置判断此刻是否处于免打扰时段。</summary>
        public static bool IsQuiet(DateTime now)
        {
            return IsQuiet(now, DataStore.Instance.Data.Settings);
        }

        /// <summary>按指定设置判断此刻是否处于免打扰时段。</summary>
        public static bool IsQuiet(DateTime now, AppSettings settings)
        {
            if (!settings.DndEnabled)
            {
                return false;
            }

            if (!TryParseRange(settings.DndStart, settings.DndEnd, out TimeSpan start, out TimeSpan end))
            {
                return false;
            }

            if (start == end)
            {
                return false;
            }

            TimeSpan time = now.TimeOfDay;
            return start < end
                ? time >= start && time < end     // 同日时段，如 12:00-14:00
                : time >= start || time < end;    // 跨天时段，如 22:30-次日 07:30
        }

        /// <summary>解析免打扰起止时间（HH:mm）。</summary>
        public static bool TryParseRange(string start, string end, out TimeSpan startTime, out TimeSpan endTime)
        {
            startTime = default;
            endTime = default;
            return TimeSpan.TryParseExact(start.Trim(), @"hh\:mm", null, out startTime)
                && TimeSpan.TryParseExact(end.Trim(), @"hh\:mm", null, out endTime);
        }
    }
}