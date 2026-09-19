using System.Collections.Generic;

namespace LearningReminder.Models
{
    /// <summary>
    /// 落盘的全部数据（单文件 JSON）。
    /// </summary>
    public class AppData
    {
        /// <summary>数据版本，便于后续结构升级</summary>
        public int Version { get; set; } = 1;

        /// <summary>应用设置</summary>
        public AppSettings Settings { get; set; } = new AppSettings();

        /// <summary>任务列表</summary>
        public List<LearningTask> Tasks { get; set; } = new List<LearningTask>();

        /// <summary>每日记录（按日期倒序保留最近若干天）</summary>
        public List<DailyRecord> Records { get; set; } = new List<DailyRecord>();

        /// <summary>用户保存的任务模板（内置模板在代码中定义）</summary>
        public List<TaskTemplate> Templates { get; set; } = new List<TaskTemplate>();
    }
}