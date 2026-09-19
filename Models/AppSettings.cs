namespace LearningReminder.Models
{
    /// <summary>
    /// 应用设置。
    /// </summary>
    public class AppSettings
    {
        /// <summary>开机自启</summary>
        public bool AutoStart { get; set; }

        /// <summary>新建任务时默认的询问间隔（分钟）</summary>
        public int DefaultIntervalMinutes { get; set; } = Resources.AppConstants.DefaultIntervalMinutes;

        /// <summary>「稍后再说」推迟时长（分钟）</summary>
        public int SnoozeMinutes { get; set; } = Resources.AppConstants.DefaultSnoozeMinutes;

        /// <summary>系统通知未被点击时，是否自动弹出确认卡片</summary>
        public bool AutoShowCheckCard { get; set; } = true;

        /// <summary>是否启用免打扰时段</summary>
        public bool DndEnabled { get; set; }

        /// <summary>免打扰开始时间（HH:mm）</summary>
        public string DndStart { get; set; } = Resources.AppConstants.DefaultDndStart;

        /// <summary>免打扰结束时间（HH:mm；早于开始时间表示跨天）</summary>
        public string DndEnd { get; set; } = Resources.AppConstants.DefaultDndEnd;

        /// <summary>每日目标任务数（0 表示不设目标）</summary>
        public int DailyGoalCount { get; set; } = Resources.AppConstants.DefaultDailyGoalCount;

        /// <summary>是否加密本地数据文件（DPAPI，仅当前 Windows 账户可解密）</summary>
        public bool EncryptData { get; set; }

        /// <summary>启动时自动检查新版本</summary>
        public bool CheckUpdateOnStart { get; set; } = true;
    }
}