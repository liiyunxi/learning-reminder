using System;
using LearningReminder.Models;
using LearningReminder.Resources;

namespace LearningReminder.ViewModels
{
    /// <summary>
    /// 历史记录行。
    /// </summary>
    public sealed class RecordRowViewModel
    {
        /// <summary>构造记录行。</summary>
        public RecordRowViewModel(CheckInLogEntry entry, bool showDate)
        {
            ShowDate = showDate;
            DateText = entry.Time.ToString(AppConstants.DateFormat);
            TimeText = entry.Time.ToString("HH:mm");
            TaskTitle = entry.TaskTitle;
            AnswerText = BuildAnswerText(entry);
        }

        /// <summary>是否展示日期分组标题</summary>
        public bool ShowDate { get; }

        /// <summary>日期文本</summary>
        public string DateText { get; }

        /// <summary>时间文本</summary>
        public string TimeText { get; }

        /// <summary>任务名称</summary>
        public string TaskTitle { get; }

        /// <summary>结果文本</summary>
        public string AnswerText { get; }

        private static string BuildAnswerText(CheckInLogEntry entry)
        {
            switch (entry.Answer)
            {
                case CheckInAnswer.Completed:
                    return AppStrings.LogAnswerCompleted;
                case CheckInAnswer.NotYet:
                    return AppStrings.LogAnswerNotYet;
                case CheckInAnswer.Snoozed:
                    return AppStrings.LogAnswerSnoozed + BuildDetailSuffix(entry.Detail);
                case CheckInAnswer.AutoSnoozed:
                    return AppStrings.LogAnswerAutoSnoozed;
                case CheckInAnswer.Partial:
                    return string.IsNullOrEmpty(entry.Detail) ? AppStrings.LogAnswerNotYet : entry.Detail;
                default:
                    return string.Empty;
            }
        }

        private static string BuildDetailSuffix(string detail)
        {
            return string.IsNullOrEmpty(detail) ? string.Empty : "（" + detail + "）";
        }
    }
}