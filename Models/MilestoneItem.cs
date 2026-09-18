using System;

namespace LearningReminder.Models
{
    /// <summary>
    /// 里程碑（学习任务下的小目标），完成状态跨天保留。
    /// </summary>
    public class MilestoneItem
    {
        /// <summary>唯一标识</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>小目标描述，例如"看完 string 常用命令"</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>是否已完成</summary>
        public bool Done { get; set; }

        /// <summary>完成时间</summary>
        public DateTime? DoneAt { get; set; }
    }
}