namespace LearningReminder.Resources
{
    /// <summary>
    /// 界面与通知文案常量：所有面向用户的文本集中在此，避免硬编码在业务代码里。
    /// </summary>
    public static class AppStrings
    {
        // ---------- 窗口标题 ----------
        public const string MainWindowTitle = "学习计划";
        public const string TaskEditWindowTitleNew = "新建任务";
        public const string TaskEditWindowTitleEdit = "编辑任务";
        public const string CheckCardTitle = "进度确认";

        // ---------- 主界面 ----------
        public const string TabToday = "今日任务";
        public const string TabHistory = "记录";
        public const string ButtonNewTask = "新建任务";
        public const string ButtonCheckAll = "检查全部任务";
        public const string ButtonHideToTray = "隐藏到托盘";
        public const string ButtonClose = "关闭";
        public const string ButtonSettings = "设置";
        public const string TagFilterAll = "全部";
        public const string UpdateBannerFormat = "发现新版本 v{0}";
        public const string ButtonGoDownload = "前往下载";
        public const string LabelAutoStart = "开机自启";
        public const string TodayProgressFormat = "今日完成 {0}/{1}（{2}%）";
        public const string DataPathLabelFormat = "数据目录：{0}";
        public const string LabelEmptyToday = "还没有任务，点「新建任务」添加一条学习计划吧";
        public const string LabelEmptyTaggedFormat = "分组「{0}」下没有今天要执行的任务";
        public const string LabelEmptyHistory = "这一天没有检查记录";
        public const string LabelSkippedTodayFormat = "另有 {0} 个任务今天不执行：{1}";
        public const string LabelPendingBannerFormat = "有 {0} 项等待你确认，点这里立即处理";
        public const string ButtonCheckNow = "立即检查";
        public const string ButtonEdit = "编辑";
        public const string ButtonDelete = "删除";
        public const string ButtonDisable = "停用";
        public const string ButtonEnable = "启用";
        public const string ButtonLearnNow = "立即学习";
        public const string ButtonOpenLink = "打开链接";
        public const string ModeIntervalLabel = "定时询问";
        public const string ModeMilestoneLabel = "里程碑";
        public const string IntervalLabelFormat = "每 {0} 分钟询问一次";
        public const string MilestoneProgressFormat = "已完成 {0}/{1}";
        public const string DailyReminderLabelFormat = "每日 {0} 核对";
        public const string DailyReminderOff = "不主动提醒";
        public const string CountdownFormat = "下次检查 {0}（{1} 分钟后）";
        public const string CountdownSoon = "下次检查不到 1 分钟";
        public const string CountdownPending = "等待你确认进度";
        public const string TooltipFinished = "今日已完成";
        public const string StatusCompletedToday = "今日已完成";
        public const string StatusMilestoneFinished = "里程碑已全部完成";
        public const string StatusDisabled = "已停用";

        // ---------- 任务编辑 ----------
        public const string FieldTitle = "任务名称";
        public const string FieldTitleHint = "例如：学习 Redis 数据类型";
        public const string FieldLink = "学习链接（可选）";
        public const string FieldLinkHint = "填了链接后，提醒时会多一个「立即学习」按钮，点开直达网页";
        public const string FieldTag = "分组标签（可选）";
        public const string FieldTagHint = "例如：英语、编程；设置后可在主界面按分组筛选";
        public const string LabelTemplateSection = "从模板新建";
        public const string ButtonSaveAsTemplate = "保存为模板";
        public const string ButtonDeleteTemplate = "×";
        public const string ValidationTemplateNameRequired = "请先填写任务名称，再保存为模板";
        public const string FieldRepeat = "重复";
        public const string FieldRepeatWeekday = "星期几执行";
        public const string FieldRepeatMonthDay = "每月几号执行";
        public const string FieldRepeatCustom = "哪几天执行（可多选）";
        public const string FieldRepeatMonthHint = "当月没有该日期时（例如 2 月没有 31 号）顺延到当月最后一天";
        public const string ButtonWorkdays = "工作日";
        public const string FieldMode = "检查方式";
        public const string ModeIntervalDescription = "按固定间隔反复询问进度，直到你标记完成为止；连续未完成时会自动放缓节奏";
        public const string ModeMilestoneDescription = "把任务拆成多个小目标，逐个勾选，全部完成即结束";
        public const string FieldInterval = "询问间隔（分钟）";
        public const string FieldDailyReminder = "每日核对时间";
        public const string FieldMilestones = "里程碑清单（每行一条）";
        public const string FieldMilestonesHint = "每行一条，例如：\r\n看完 string 常用命令\r\n学完 hash 与 zset";
        public const string ButtonSave = "保存";
        public const string ButtonSaveProgress = "保存进度";
        public const string ButtonCancel = "取消";
        public const string ValidationTitleRequired = "请填写任务名称";
        public const string ValidationIntervalRangeFormat = "询问间隔需在 {0} - {1} 分钟之间";
        public const string ValidationMilestoneRequired = "里程碑模式至少需要一条里程碑";
        public const string ValidationTimeFormat = "每日核对时间格式应为 HH:mm，例如 21:00";
        public const string ValidationLinkFormat = "学习链接格式不正确，请填写 http 或 https 开头的网址";
        public const string ValidationWeekdayRequired = "请至少选择一天执行";
        public const string ValidationMonthDayRangeFormat = "每月日期需在 {0} - {1} 号之间";

        // ---------- 重复规则文案 ----------
        public const string RepeatDaily = "每天";
        public const string RepeatWeekly = "每周";
        public const string RepeatMonthly = "每月";
        public const string RepeatCustom = "自定义";
        public const string RepeatWeeklyFormat = "每周{0}";
        public const string RepeatCustomFormat = "每周{0}";
        public const string RepeatMonthlyFormat = "每月 {0} 日";
        public const string RepeatWeekdayJoiner = "、";
        public const string WeekdaySunday = "周日";
        public const string WeekdayMonday = "周一";
        public const string WeekdayTuesday = "周二";
        public const string WeekdayWednesday = "周三";
        public const string WeekdayThursday = "周四";
        public const string WeekdayFriday = "周五";
        public const string WeekdaySaturday = "周六";

        // ---------- 记录（日历 + 列表） ----------
        public const string CalendarMonthFormat = "{0} 年 {1} 月";
        public const string CalendarPrevMonth = "上个月";
        public const string CalendarNextMonth = "下个月";
        public const string CalendarToday = "回到今天";
        public const string CalendarDayTooltipFormat = "{0}：完成 {1}/{2}";
        public const string CalendarDayNoTask = "{0}：没有安排任务";
        public const string CalendarSelectedFormat = "{0} · 检查记录";
        public const string CalendarLegendAllDone = "当天全部完成";
        public const string CalendarLegendPartial = "部分完成";
        public const string CalendarLegendNone = "未完成";

        // ---------- 确认卡片 / 通知 ----------
        public const string AnswerDone = "已完成";
        public const string AnswerNotYet = "还没完成";
        public const string AnswerSnoozeFormat = "{0} 分钟后再问";
        public const string CheckCardHintInterval = "该任务按间隔询问进度，完成后今天不再打扰";
        public const string CheckCardHintMilestone = "勾选已完成的小目标，全部勾完任务即结束";
        public const string CheckCardAutoCloseFormat = "{0} 秒后自动按「稍后再说」处理";
        public const string ToastBodyInterval = "该确认一下进度了，完成了吗？";
        public const string ToastBodyMilestoneFormat = "核对里程碑进度：已完成 {0}/{1}";
        public const string ToastBodyPlain = "点此确认进度";

        // ---------- 托盘 ----------
        public const string TrayTipIdleFormat = "{0}今日 {1}/{2} 完成";
        public const string TrayTipPendingFormat = "{0}待确认 {1} 项";
        public const string TrayMenuOpen = "打开主界面";
        public const string TrayMenuCheckNow = "立即检查全部";
        public const string TrayMenuAutoStart = "开机自启";
        public const string TrayMenuExit = "退出";
        public const string TrayStartedSilently = "已在后台运行，任务到点会提醒你";
        public const string TrayHiddenHint = "已最小化到托盘：图标在任务栏右下角，若看不到就点 ^ 展开隐藏图标";
        public const string LinkOpenFailed = "链接打开失败，请检查网址是否有效";
        public const string NothingToCheck = "当前没有需要检查的任务";

        // ---------- 其它 ----------
        public const string ConfirmDeleteTitle = "删除任务";
        public const string ConfirmDeleteMessageFormat = "确定删除任务「{0}」吗？历史记录会保留。";
        public const string LogAnswerCompleted = "标记完成";
        public const string LogAnswerNotYet = "仍未完成";
        public const string LogAnswerSnoozed = "稍后再说";
        public const string LogAnswerAutoSnoozed = "未应答，自动推迟";
        public const string LogAnswerPartialFormat = "进度 {0}/{1}";
        public const string LogAnswerBackfill = "补打卡";

        // ---------- 学习计时 / 补打卡（阶段三：进度确认） ----------
        public const string ButtonStartStudy = "开始学习";
        public const string ButtonStopStudy = "结束学习";
        public const string StudyRunningFormat = "学习中 {0}";
        public const string StudyTodayFormat = "今日学习 {0}";
        public const string ButtonBackfill = "补打卡";
        public const string LabelDayTasks = "当天任务";
        public const string BackfillDoneText = "已完成";
        public const string BackfillTodoText = "未完成";
        public const string BackfillHint = "漏打卡的日子可以事后补记";

        // ---------- 模板与标签（阶段三：任务管理） ----------
        public const string TemplateNameVocab = "背单词";
        public const string TemplateTitleVocab = "背诵 30 个单词";
        public const string TemplateNameCourse = "看视频课";
        public const string TemplateTitleCourse = "看一节视频课";
        public const string TemplateNameReading = "读书";
        public const string TemplateTitleReading = "阅读 30 分钟";
        public const string TemplateNameCoding = "编程练习";
        public const string TemplateTitleCoding = "完成一道算法题";
        public const string TemplateCodingMilestone1 = "读题并想出思路";
        public const string TemplateCodingMilestone2 = "写出代码并通过测试";
        public const string TemplateCodingMilestone3 = "整理错题笔记";
        public const string TagEnglish = "英语";
        public const string TagCourse = "课程";
        public const string TagReading = "阅读";
        public const string TagCoding = "编程";

        // ---------- 统计 / 成就（阶段三：统计报告 / 目标激励） ----------
        public const string TabStats = "统计";
        public const string StatsStreakTitle = "连续完成";
        public const string StatsStreakFormat = "{0} 天";
        public const string StatsStreakEmpty = "还没有连续记录，从今天开始吧";
        public const string StatsMaxStreakFormat = "历史最长连续 {0} 天";
        public const string TrendDayLabelFormat = "{0}/{1}";
        public const string StatsGoalTitle = "每日目标";
        public const string StatsGoalFormat = "今日完成 {0}/{1} 个任务";
        public const string StatsGoalDone = "今日目标已达成";
        public const string StatsGoalOff = "未设置每日目标（可在设置里开启）";
        public const string StatsWeekTitle = "近 7 天";
        public const string StatsMonthTitle = "本月";
        public const string StatsRangeFormat = "完成 {0}/{1}（{2}%） · 全勤 {3} 天";
        public const string StatsStudyFormat = "学习时长 {0}";
        public const string StatsAchievementTitle = "成就";
        public const string AchvProgressFormat = "{0}/{1}";
        public const string AchvFirstDone = "初次打卡";
        public const string AchvFirstDoneDesc = "完成第一个任务";
        public const string AchvStreak3 = "三日连击";
        public const string AchvStreak3Desc = "连续 3 天全部完成";
        public const string AchvStreak7 = "七日连击";
        public const string AchvStreak7Desc = "连续 7 天全部完成";
        public const string AchvStreak15 = "半月坚持";
        public const string AchvStreak15Desc = "连续 15 天全部完成";
        public const string AchvDone50 = "小有所成";
        public const string AchvDone50Desc = "累计完成 50 次任务";
        public const string AchvDone200 = "持之以恒";
        public const string AchvDone200Desc = "累计完成 200 次任务";
        public const string AchvStudy10h = "十小时";
        public const string AchvStudy10hDesc = "累计学习 10 小时";
        public const string AchvStudy50h = "五十小时";
        public const string AchvStudy50hDesc = "累计学习 50 小时";
        public const string AchvMilestone10 = "里程碑猎手";
        public const string AchvMilestone10Desc = "完成 10 个里程碑";
        public const string AchvEarlyBird = "早起之星";
        public const string AchvEarlyBirdDesc = "上午 9 点前完成 5 次";
        public const string DurationHourMinuteFormat = "{0} 小时 {1} 分";
        public const string DurationMinuteFormat = "{0} 分钟";
        public const string DurationUnderMinuteFormat = "{0} 秒";

        // ---------- 设置（阶段三：数据 / 更新 / 快捷键） ----------
        public const string SettingsWindowTitle = "设置";
        public const string SectionReminder = "提醒";
        public const string LabelDnd = "免打扰时段";
        public const string LabelDndRange = "至";
        public const string DndHint = "时段内不发起任何提醒；里程碑的每日核对时间若落在时段内，当天将不提醒";
        public const string LabelSnoozeMinutes = "「稍后再说」推迟时长（分钟）";
        public const string LabelDefaultInterval = "新建任务默认询问间隔（分钟）";
        public const string LabelAutoShowCard = "系统通知未应答时自动弹出确认卡片";
        public const string SectionGoal = "目标";
        public const string LabelDailyGoal = "每日目标任务数（0 表示不设目标）";
        public const string SectionDataPrivacy = "数据与隐私";
        public const string LabelEncryptData = "加密本地数据文件（仅当前 Windows 账户可读）";
        public const string ButtonOpenDataDir = "打开数据目录";
        public const string ButtonExportData = "导出数据";
        public const string ButtonImportData = "导入数据";
        public const string ButtonBackupNow = "立即备份";
        public const string BackupHintFormat = "每天自动备份到数据目录的 backups 子目录，最多保留 {0} 份";
        public const string ExportDialogTitle = "导出数据";
        public const string ImportDialogTitle = "导入数据";
        public const string ExportSuccess = "数据已导出";
        public const string ExportFailed = "导出失败，请查看日志";
        public const string ImportConfirmTitle = "导入数据";
        public const string ImportConfirmMessage = "导入将替换当前全部任务与记录（现有数据会先自动备份）。是否继续？";
        public const string ImportSuccess = "导入完成";
        public const string ImportFailed = "导入失败：文件不是有效的数据备份";
        public const string BackupDone = "已创建备份";
        public const string SectionAbout = "关于";
        public const string LabelVersionFormat = "当前版本 v{0}";
        public const string LabelCheckUpdateOnStart = "启动时自动检查新版本";
        public const string ButtonCheckUpdate = "检查更新";
        public const string DataFileFilter = "学习计划数据 (*.json)|*.json|所有文件 (*.*)|*.*";
        public const string ExportFileNamePrefix = "学习计划-导出-";
        public const string UpdateChecking = "正在检查…";
        public const string UpdateLatestFormat = "已是最新版本（v{0}）";
        public const string UpdateFailed = "检查失败，请稍后再试";
        public const string UpdateAvailableFormat = "发现新版本 v{0}，可前往下载";
        public const string HotkeyHint = "全局快捷键：Ctrl+Alt+L 打开主界面，Ctrl+Alt+K 立即检查全部";
        public const string ValidationNumberRangeFormat = "{0} 需在 {1} - {2} 之间";
        public const string ValidationDndRange = "免打扰时间格式应为 HH:mm，例如 22:30";

        // ---------- 托盘状态总览 / 通知聚合 ----------
        public const string TrayOverviewFormat = "今日完成 {0}/{1}";
        public const string TrayOverviewItemDonePrefix = "√ ";
        public const string TrayOverviewItemTodoPrefix = "○ ";
        public const string ToastSummaryTitleFormat = "有 {0} 项待确认";
        public const string ToastSummaryBody = "点此打开确认卡片处理";
    }
}