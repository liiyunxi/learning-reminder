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
        public const string LabelAutoStart = "开机自启";
        public const string TodayProgressFormat = "今日完成 {0}/{1}（{2}%）";
        public const string DataPathLabelFormat = "数据目录：{0}";
        public const string LabelEmptyToday = "还没有任务，点「新建任务」添加一条学习计划吧";
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
        public const string FieldRepeat = "重复";
        public const string FieldRepeatWeekday = "星期几执行";
        public const string FieldRepeatMonthDay = "每月几号执行";
        public const string FieldRepeatCustom = "哪几天执行（可多选）";
        public const string FieldRepeatMonthHint = "当月没有该日期时（例如 2 月没有 31 号）顺延到当月最后一天";
        public const string ButtonWorkdays = "工作日";
        public const string FieldMode = "检查方式";
        public const string ModeIntervalDescription = "按固定间隔反复询问进度，直到你标记完成为止";
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
    }
}