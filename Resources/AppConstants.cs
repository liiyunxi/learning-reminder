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

        /// <summary>「稍后再说」默认推迟时长（分钟）</summary>
        public const int DefaultSnoozeMinutes = 15;

        /// <summary>「稍后再说」推迟时长下限（分钟）</summary>
        public const int MinSnoozeMinutes = 1;

        /// <summary>「稍后再说」推迟时长上限（分钟）</summary>
        public const int MaxSnoozeMinutes = 180;

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

        /// <summary>学习计时显示格式（时:分:秒）</summary>
        public const string TimerFormat = @"hh\:mm\:ss";

        /// <summary>保存的历史记录保留天数</summary>
        public const int HistoryKeepDays = 60;

        /// <summary>按 Enter/空格 判定为"已完成"等处理的提示间隔下限，避免重复触发（秒）</summary>
        public const int DuplicateAnswerGuardSeconds = 3;

        // ---------- 数据与隐私 ----------
        /// <summary>数据备份目录名（位于数据目录下）</summary>
        public const string BackupFolderName = "backups";

        /// <summary>备份文件前缀（按天备份，如 data-20260919.bak）</summary>
        public const string BackupFilePrefix = "data-";

        /// <summary>备份文件后缀</summary>
        public const string BackupFileSuffix = ".bak";

        /// <summary>最多保留的备份份数</summary>
        public const int BackupKeepCount = 10;

        // ---------- 免打扰 / 每日目标 ----------
        /// <summary>默认免打扰开始时间</summary>
        public const string DefaultDndStart = "22:30";

        /// <summary>默认免打扰结束时间</summary>
        public const string DefaultDndEnd = "07:30";

        /// <summary>每日目标任务数默认值</summary>
        public const int DefaultDailyGoalCount = 2;

        /// <summary>每日目标任务数的最大值</summary>
        public const int MaxDailyGoalCount = 20;

        // ---------- 通知聚合 ----------
        /// <summary>通知聚合窗口（秒）：窗口内产生的多条待确认合并为一条汇总通知</summary>
        public const int NotificationAggregateSeconds = 2;

        // ---------- 学习时长 ----------
        /// <summary>一分钟的秒数</summary>
        public const int SecondsPerMinute = 60;

        /// <summary>一小时的分钟数</summary>
        public const int MinutesPerHour = 60;

        // ---------- 更新 / 分发 ----------
        /// <summary>最新发行版查询接口</summary>
        public const string UpdateLatestReleaseApi =
            "https://api.github.com/repos/liiyunxi/learning-reminder/releases/latest";

        /// <summary>项目主页</summary>
        public const string RepositoryUrl = "https://github.com/liiyunxi/learning-reminder";

        /// <summary>发现新版本后引导前往的下载地址</summary>
        public const string DownloadPageUrl =
            "https://github.com/liiyunxi/learning-reminder/releases/latest";

        /// <summary>启动后延迟多久检查更新（秒），避免与启动任务抢资源</summary>
        public const int UpdateCheckDelaySeconds = 8;

        // ---------- 统计 / 成就 ----------
        /// <summary>成就"早起"的判定小时：完成时间早于该小时算早起打卡</summary>
        public const int EarlyBirdHour = 9;

        /// <summary>统计页"近 7 天"趋势的天数</summary>
        public const int TrendDays = 7;
    }
}