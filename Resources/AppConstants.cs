namespace LearningReminder.Resources
{
    /// <summary>
    /// 应用级常量：默认值、文件名、注册表键、时间格式等。
    /// 统一收口，避免散落的硬编码字符串与魔法数字。
    /// </summary>
    public static class AppConstants
    {
        /// <summary>应用显示名称（托盘提示、通知标题、窗口标题统一使用）</summary>
        public const string AppName = "学习计划";

        /// <summary>数据目录名（位于 %AppData% 下）</summary>
        public const string AppDataFolderName = "LearningReminder";

        /// <summary>主数据文件名</summary>
        public const string DataFileName = "data.json";

        /// <summary>错误日志文件名</summary>
        public const string LogFileName = "log.txt";

        /// <summary>单实例互斥体名称</summary>
        public const string SingleInstanceMutexName = "LearningReminder.SingleInstance.Mutex";

        /// <summary>单实例命名管道名称（用于把二次启动的参数转发给已运行实例）</summary>
        public const string SingleInstancePipeName = "LearningReminder.SingleInstance.Pipe";

        /// <summary>开机自启注册表项（HKCU 下的 Run 键）</summary>
        public const string AutoRunRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>开机自启注册表值名称</summary>
        public const string AutoRunValueName = "LearningReminder";

        /// <summary>系统通知的应用标识（Toast 需要，卸载时会清理）</summary>
        public const string ToastAumid = "Trae.LearningReminder";

        /// <summary>调度器心跳间隔（毫秒），1 秒足够精确且开销极低</summary>
        public const int SchedulerTickMilliseconds = 1000;

        /// <summary>默认检查间隔（分钟）：每小时询问一次</summary>
        public const int DefaultIntervalMinutes = 60;

        /// <summary>最小允许的检查间隔（分钟）</summary>
        public const int MinIntervalMinutes = 5;

        /// <summary>最大允许的检查间隔（分钟）</summary>
        public const int MaxIntervalMinutes = 24 * 60;

        /// <summary>"稍后再说"默认推迟时长（分钟）</summary>
        public const int DefaultSnoozeMinutes = 15;

        /// <summary>待确认项在内存中的最大等待时长（秒），超时按自动推迟处理</summary>
        public const int PendingTimeoutSeconds = 300;

        /// <summary>确认卡片自动关闭倒计时（秒）</summary>
        public const int CheckCardAutoCloseSeconds = 90;

        /// <summary>首次弹卡片前的延迟（秒）：先给系统通知留出被点击的时间</summary>
        public const int CheckCardDelaySeconds = 20;

        /// <summary>里程碑模式默认的每日核对提醒时间</summary>
        public const string DefaultDailyReminderTime = "21:00";

        /// <summary>用户输入链接缺少协议时补全的协议</summary>
        public const string DefaultUrlScheme = "https://";

        /// <summary>每月重复的日期取值范围</summary>
        public const int MinMonthDay = 1;

        /// <summary>每月重复的日期取值范围上限</summary>
        public const int MaxMonthDay = 31;

        /// <summary>当天计划起始小时：跨天重置后，首次检查时间以此为基准</summary>
        public const int DayStartHour = 8;

        /// <summary>日期键格式</summary>
        public const string DateFormat = "yyyy-MM-dd";

        /// <summary>界面展示用的长日期格式</summary>
        public const string LongDateFormat = "yyyy年M月d日 dddd";

        /// <summary>界面日期使用的区域</summary>
        public const string CultureName = "zh-CN";

        /// <summary>时间键格式（每日提醒时间）</summary>
        public const string TimeFormat = "HH:mm";

        /// <summary>保存的历史记录保留天数</summary>
        public const int HistoryKeepDays = 60;

        /// <summary>按 Enter/空格 判定为"已完成"等处理的提示间隔下限，避免重复触发（秒）</summary>
        public const int DuplicateAnswerGuardSeconds = 3;
    }
}