using System;
using System.Collections.Generic;
using LearningReminder.Resources;

namespace LearningReminder.Models
{
    /// <summary>
    /// 任务模板：新建任务时可一键套用的预设（内置模板 + 用户自建模板）。
    /// </summary>
    public class TaskTemplate
    {
        /// <summary>唯一标识</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>模板名称（选择器上显示）</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>任务标题</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>检查方式</summary>
        public CheckMode Mode { get; set; } = CheckMode.Interval;

        /// <summary>询问间隔（分钟，定时询问模式）</summary>
        public int IntervalMinutes { get; set; } = AppConstants.DefaultIntervalMinutes;

        /// <summary>每日核对时间（里程碑模式）</summary>
        public string DailyReminderTime { get; set; } = string.Empty;

        /// <summary>里程碑清单（里程碑模式）</summary>
        public List<string> Milestones { get; set; } = new List<string>();

        /// <summary>分组标签</summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>是否内置模板（内置模板不可删除）</summary>
        public bool BuiltIn { get; set; }
    }
}