using System;
using System.Collections.Generic;
using LearningReminder.Models;

namespace LearningReminder.Services
{
    /// <summary>
    /// 待确认项：一次已经发出、但用户尚未回答的询问。
    /// </summary>
    public sealed class PendingCheckIn
    {
        /// <summary>任务标识</summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>任务名称</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>学习链接（可用于通知上的「立即学习」按钮）</summary>
        public string LinkUrl { get; set; } = string.Empty;

        /// <summary>检查方式</summary>
        public CheckMode Mode { get; set; }

        /// <summary>发出时间</summary>
        public DateTime RaisedAt { get; set; } = DateTime.Now;

        /// <summary>里程碑已完成数量（里程碑模式）</summary>
        public int DoneCount { get; set; }

        /// <summary>里程碑总数（里程碑模式）</summary>
        public int TotalCount { get; set; }
    }
}