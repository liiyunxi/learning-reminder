using System;
using System.Collections.Generic;
using LearningReminder.Resources;

namespace LearningReminder.Models
{
    /// <summary>
    /// 学习任务定义（长期存在，与"每天"的执行状态分离）。
    /// </summary>
    public class LearningTask
    {
        /// <summary>唯一标识</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>任务名称，例如"学习 Redis 数据类型"</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>学习链接（可选），到点提醒时可一键打开</summary>
        public string LinkUrl { get; set; } = string.Empty;

        /// <summary>检查方式</summary>
        public CheckMode Mode { get; set; } = CheckMode.Interval;

        /// <summary>重复规则：每天 / 每周 / 每月 / 自定义</summary>
        public RepeatRule Repeat { get; set; } = RepeatRule.CreateDaily();

        /// <summary>询问间隔（分钟），两种模式复用于"再次询问"的节奏</summary>
        public int IntervalMinutes { get; set; } = AppConstants.DefaultIntervalMinutes;

        /// <summary>每日核对时间（HH:mm）；仅里程碑模式使用，空串表示不主动提醒</summary>
        public string DailyReminderTime { get; set; } = string.Empty;

        /// <summary>是否启用（停用后不再提醒）</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>里程碑清单</summary>
        public List<MilestoneItem> Milestones { get; set; } = new List<MilestoneItem>();

        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>是否有可打开的学习链接</summary>
        public bool HasLink => !string.IsNullOrWhiteSpace(LinkUrl);

        /// <summary>今天是否要执行（由重复规则决定）。</summary>
        public bool ShouldRunOn(DateTime date)
        {
            return Repeat == null || Repeat.ShouldRunOn(date);
        }

        /// <summary>
        /// 里程碑是否已全部完成（仅里程碑模式有意义，跨天累计）。
        /// </summary>
        public bool IsMilestoneFinished()
        {
            if (Mode != CheckMode.Milestone || Milestones.Count == 0)
            {
                return false;
            }

            // 只要存在未完成项即视为未结束
            foreach (MilestoneItem item in Milestones)
            {
                if (!item.Done)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 已完成的里程碑数量。
        /// </summary>
        public int DoneMilestoneCount()
        {
            int count = 0;
            foreach (MilestoneItem item in Milestones)
            {
                if (item.Done)
                {
                    count++;
                }
            }

            return count;
        }
    }
}