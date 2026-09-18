using System;

namespace LearningReminder.Models
{
    /// <summary>
    /// 一条检查记录。
    /// </summary>
    public class CheckInLogEntry
    {
        /// <summary>记录时间</summary>
        public DateTime Time { get; set; } = DateTime.Now;

        /// <summary>任务标识</summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>任务名称快照（任务被删除后记录仍可读）</summary>
        public string TaskTitle { get; set; } = string.Empty;

        /// <summary>应答结果</summary>
        public CheckInAnswer Answer { get; set; }

        /// <summary>补充说明，例如"进度 2/4"</summary>
        public string Detail { get; set; } = string.Empty;
    }
}