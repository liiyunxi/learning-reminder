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
    }
}