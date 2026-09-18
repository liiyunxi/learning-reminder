using System;
using System.Collections.Generic;

namespace LearningReminder.Models
{
    /// <summary>
    /// 单个任务在某一天的执行状态（定时询问模式主要用于此处的当天完成标记）。
    /// </summary>
    public class TaskProgress
    {
        /// <summary>任务标识</summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>当天是否已完成</summary>
        public bool IsCompleted { get; set; }

        /// <summary>完成时间</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>最近一次询问时间</summary>
        public DateTime? LastCheckAt { get; set; }

        /// <summary>下次询问时间（为空表示尚未排期）</summary>
        public DateTime? NextCheckAt { get; set; }

        /// <summary>推迟到该时间之后再询问</summary>
        public DateTime? SnoozeUntil { get; set; }

        /// <summary>里程碑模式：当天的核对提醒是否已触发过</summary>
        public bool DailyReminderRaised { get; set; }

        /// <summary>当天是否已完成首次排期（避免调度器反复重排）</summary>
        public bool Initialized { get; set; }
    }

    /// <summary>
    /// 某一天的执行记录：包含各任务状态与当天的检查流水。
    /// </summary>
    public class DailyRecord
    {
        /// <summary>日期键（yyyy-MM-dd）</summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>各任务的当天状态</summary>
        public List<TaskProgress> Progress { get; set; } = new List<TaskProgress>();

        /// <summary>当天的检查记录</summary>
        public List<CheckInLogEntry> Logs { get; set; } = new List<CheckInLogEntry>();

        /// <summary>
        /// 取指定任务当天的状态，不存在时创建。
        /// </summary>
        public TaskProgress GetOrCreate(string taskId)
        {
            foreach (TaskProgress item in Progress)
            {
                if (item.TaskId == taskId)
                {
                    return item;
                }
            }

            TaskProgress created = new TaskProgress { TaskId = taskId };
            Progress.Add(created);
            return created;
        }

        /// <summary>
        /// 查找指定任务当天的状态，找不到返回 null。
        /// </summary>
        public TaskProgress? Find(string taskId)
        {
            foreach (TaskProgress item in Progress)
            {
                if (item.TaskId == taskId)
                {
                    return item;
                }
            }

            return null;
        }
    }
}