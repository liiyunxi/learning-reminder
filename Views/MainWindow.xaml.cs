using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LearningReminder.Models;
using LearningReminder.Resources;
using LearningReminder.Services;
using LearningReminder.ViewModels;

namespace LearningReminder.Views
{
    /// <summary>
    /// 主窗口：查看今日任务、增删改任务、按日历查看历史记录。
    /// 关闭窗口只是隐藏，应用仍驻留托盘继续提醒。
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<TaskCardViewModel> _taskCards =
            new ObservableCollection<TaskCardViewModel>();

        private readonly ObservableCollection<RecordRowViewModel> _records =
            new ObservableCollection<RecordRowViewModel>();

        private readonly ObservableCollection<CalendarDayViewModel> _calendarDays =
            new ObservableCollection<CalendarDayViewModel>();

        private readonly ObservableCollection<BackfillRowViewModel> _dayTasks =
            new ObservableCollection<BackfillRowViewModel>();

        private readonly ObservableCollection<TrendBarViewModel> _trendBars =
            new ObservableCollection<TrendBarViewModel>();

        private readonly ObservableCollection<Achievement> _achievements =
            new ObservableCollection<Achievement>();

        private DateTime _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime _selectedDate = DateTime.Today;
        private bool _suppressEvents = true;
        private string _activeTag = string.Empty;
        private bool _buildingTagFilters;

        /// <summary>构造主窗口。</summary>
        public MainWindow()
        {
            InitializeComponent();

            TaskList.ItemsSource = _taskCards;
            RecordList.ItemsSource = _records;
            CalendarDays.ItemsSource = _calendarDays;
            DayTaskList.ItemsSource = _dayTasks;
            TrendBars.ItemsSource = _trendBars;
            AchievementList.ItemsSource = _achievements;
            DataPathText.Text = string.Format(AppStrings.DataPathLabelFormat, AppPaths.DataDirectory);

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;
            Closing += OnWindowClosing;
            StateChanged += OnWindowStateChanged;
        }

        /// <summary>显示并置于前台（托盘入口调用）。</summary>
        public void ShowAndActivate()
        {
            if (!IsVisible)
            {
                Show();
            }

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Activate();

            // 置顶再取消，确保窗口真的浮到最前
            Topmost = true;
            Topmost = false;
        }

        private App? Host => Application.Current as App;

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.Changed += OnDataChanged;
            DataStore.Instance.DayRolledOver += OnDayRolledOver;
            CheckInService.Instance.PendingChanged += OnPendingChanged;

            SchedulerService? scheduler = Host?.Scheduler;
            if (scheduler != null)
            {
                scheduler.Ticked += OnSchedulerTicked;
            }

            _selectedDate = DateTime.Today;
            _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            BuildCalendar();

            SyncTaskList(forceRebuild: true);
            SyncRecords();
            UpdateHeader();
            UpdatePendingBanner();
            SyncAutoStartCheck();
            BuildTagFilters();
            RefreshStats();

            if (Host?.UpdateInfo is UpdateInfo info)
            {
                ShowUpdate(info);
            }

            if (Host != null)
            {
                Host.UpdateFound += OnUpdateFound;
            }

            _suppressEvents = false;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            DataStore.Instance.Changed -= OnDataChanged;
            DataStore.Instance.DayRolledOver -= OnDayRolledOver;
            CheckInService.Instance.PendingChanged -= OnPendingChanged;

            if (Host != null)
            {
                Host.UpdateFound -= OnUpdateFound;
            }

            SchedulerService? scheduler = Host?.Scheduler;
            if (scheduler != null)
            {
                scheduler.Ticked -= OnSchedulerTicked;
            }
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Host?.IsExiting ?? false)
            {
                return;
            }

            // 关闭按钮＝隐藏到托盘，保证后台提醒不中断
            e.Cancel = true;
            Hide();
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            bool maximized = WindowState == WindowState.Maximized;
            MaximizeIcon.Visibility = maximized ? Visibility.Collapsed : Visibility.Visible;
            RestoreIcon.Visibility = maximized ? Visibility.Visible : Visibility.Collapsed;
        }

        // ---------------- 数据同步 ----------------

        private void OnDataChanged(object? sender, EventArgs e)
        {
            BuildTagFilters();
            SyncTaskList(forceRebuild: false);
            RefreshCalendarStats();
            SyncRecords();
            UpdateHeader();
            UpdatePendingBanner();
            RefreshStats();
        }

        private void OnDayRolledOver(object? sender, EventArgs e)
        {
            _selectedDate = DateTime.Today;
            _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            BuildCalendar();
            BuildTagFilters();
            SyncTaskList(forceRebuild: true);
            SyncRecords();
            UpdateHeader();
            RefreshStats();
        }

        private void OnPendingChanged(object? sender, EventArgs e)
        {
            UpdatePendingBanner();
        }

        private void OnSchedulerTicked(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            foreach (TaskCardViewModel card in _taskCards)
            {
                card.RefreshCountdown(now);
            }
        }

        /// <summary>
        /// 同步任务卡片：任务集合没有变化时只刷新文案，避免无谓地重建界面元素。
        /// </summary>
        private void SyncTaskList(bool forceRebuild)
        {
            List<LearningTask> tasks = new List<LearningTask>();
            List<string> skipped = new List<string>();
            DateTime today = DateTime.Today;

            foreach (LearningTask task in DataStore.Instance.Data.Tasks)
            {
                if (_activeTag.Length > 0
                    && !string.Equals(task.Tag?.Trim() ?? string.Empty, _activeTag, StringComparison.Ordinal))
                {
                    // 与当前分组筛选不符的任务不展示
                    continue;
                }

                // 今天不执行的任务不进今日清单，只在底部说明
                if (task.ShouldRunOn(today))
                {
                    tasks.Add(task);
                }
                else
                {
                    skipped.Add(string.Format("{0}（{1}）", task.Title, RepeatText.Describe(task.Repeat)));
                }
            }

            bool sameShape = !forceRebuild && _taskCards.Count == tasks.Count;
            if (sameShape)
            {
                for (int index = 0; index < tasks.Count; index++)
                {
                    if (_taskCards[index].Task.Id != tasks[index].Id)
                    {
                        sameShape = false;
                        break;
                    }
                }
            }

            if (!sameShape)
            {
                _taskCards.Clear();
                foreach (LearningTask task in tasks)
                {
                    _taskCards.Add(new TaskCardViewModel(task, OnMilestoneToggled));
                }
            }
            else
            {
                foreach (TaskCardViewModel card in _taskCards)
                {
                    card.Refresh();
                }
            }

            EmptyToday.Text = _activeTag.Length == 0
                ? AppStrings.LabelEmptyToday
                : string.Format(AppStrings.LabelEmptyTaggedFormat, _activeTag);
            EmptyToday.Visibility = _taskCards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            UpdateSkippedHint(skipped);
        }

        private void UpdateSkippedHint(List<string> skipped)
        {
            if (skipped.Count == 0)
            {
                SkippedTodayText.Visibility = Visibility.Collapsed;
                return;
            }

            SkippedTodayText.Text = string.Format(
                AppStrings.LabelSkippedTodayFormat,
                skipped.Count,
                string.Join(AppStrings.RepeatWeekdayJoiner, skipped));
            SkippedTodayText.Visibility = Visibility.Visible;
        }

        /// <summary>只显示选中日期那天的检查流水。</summary>
        private void SyncRecords()
        {
            _records.Clear();

            DailyRecord? record = DataStore.Instance.FindRecord(_selectedDate.ToString(AppConstants.DateFormat));
            if (record != null)
            {
                for (int index = record.Logs.Count - 1; index >= 0; index--)
                {
                    _records.Add(new RecordRowViewModel(record.Logs[index], showDate: false));
                }
            }

            CultureInfo culture = CultureInfo.GetCultureInfo(AppConstants.CultureName);
            SelectedDayText.Text = string.Format(
                AppStrings.CalendarSelectedFormat,
                _selectedDate.ToString(AppConstants.LongDateFormat, culture));

            EmptyHistory.Visibility = _records.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            SyncDayTasks();
        }

        /// <summary>刷新"当天任务"列表：过去日期提供补打卡入口。</summary>
        private void SyncDayTasks()
        {
            _dayTasks.Clear();

            DateTime day = _selectedDate.Date;
            bool canBackfillDay = day < DateTime.Today;
            string dateKey = day.ToString(AppConstants.DateFormat);
            DailyRecord? record = DataStore.Instance.FindRecord(dateKey);

            foreach (LearningTask task in DataStore.Instance.Data.Tasks)
            {
                if (!task.Enabled || task.CreatedAt.Date > day || !task.ShouldRunOn(day))
                {
                    continue;
                }

                TaskProgress? progress = record?.Find(task.Id);
                bool completed = progress?.IsCompleted == true;
                _dayTasks.Add(new BackfillRowViewModel(task.Id, task.Title, completed, canBackfillDay && !completed));
            }

            DayTaskPanel.Visibility = _dayTasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>按当前任务动态生成分组筛选按钮（"全部" + 各分组）。</summary>
        private void BuildTagFilters()
        {
            _buildingTagFilters = true;
            try
            {
                TagFilterPanel.Children.Clear();

                List<string> tags = new List<string>();
                foreach (LearningTask task in DataStore.Instance.Data.Tasks)
                {
                    string tag = task.Tag?.Trim() ?? string.Empty;
                    if (tag.Length > 0 && !tags.Contains(tag))
                    {
                        tags.Add(tag);
                    }
                }

                tags.Sort(StringComparer.CurrentCulture);

                if (_activeTag.Length > 0 && !tags.Contains(_activeTag))
                {
                    // 当前筛选的分组已不存在，回到"全部"
                    _activeTag = string.Empty;
                }

                AddTagFilterButton(AppStrings.TagFilterAll, string.Empty, _activeTag.Length == 0);
                foreach (string tag in tags)
                {
                    AddTagFilterButton(tag, tag, string.Equals(_activeTag, tag, StringComparison.Ordinal));
                }

                TagFilterPanel.Visibility = tags.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            finally
            {
                _buildingTagFilters = false;
            }
        }

        private void AddTagFilterButton(string text, string tag, bool selected)
        {
            RadioButton button = new RadioButton
            {
                Content = text,
                GroupName = "TagFilter",
                IsChecked = selected,
                Style = (Style)FindResource("Pill.Radio"),
                Tag = tag
            };
            button.Checked += OnTagFilterChecked;
            TagFilterPanel.Children.Add(button);
        }

        private void OnTagFilterChecked(object sender, RoutedEventArgs e)
        {
            if (_buildingTagFilters)
            {
                return;
            }

            if ((sender as FrameworkElement)?.Tag is not string tag || _activeTag == tag)
            {
                return;
            }

            _activeTag = tag;
            SyncTaskList(forceRebuild: true);
        }

        /// <summary>刷新统计页：连续天数、每日目标、近 7 天趋势、成就。</summary>
        private void RefreshStats()
        {
            int streak = StatsService.CurrentStreak();
            StreakText.Text = streak > 0
                ? string.Format(AppStrings.StatsStreakFormat, streak)
                : AppStrings.StatsStreakEmpty;
            StreakHintText.Text = string.Format(AppStrings.StatsMaxStreakFormat, StatsService.MaxStreak());

            int goal = DataStore.Instance.Data.Settings.DailyGoalCount;
            DayProgress today = DailyStats.CountFor(DateTime.Today);
            if (goal <= 0)
            {
                GoalText.Text = AppStrings.StatsGoalOff;
                GoalProgress.Value = 0;
            }
            else
            {
                GoalText.Text = today.Done >= goal
                    ? string.Format(AppStrings.StatsGoalFormat, today.Done, goal) + " · " + AppStrings.StatsGoalDone
                    : string.Format(AppStrings.StatsGoalFormat, today.Done, goal);
                GoalProgress.Value = Math.Min(100, today.Done * 100.0 / goal);
            }

            DateTime from = DateTime.Today.AddDays(-(AppConstants.TrendDays - 1));
            List<DayStat> trend = StatsService.DailyStatsRange(from, DateTime.Today);
            TrendTitleText.Text = AppStrings.StatsWeekTitle;
            _trendBars.Clear();
            foreach (DayStat stat in trend)
            {
                _trendBars.Add(new TrendBarViewModel(stat));
            }

            RangeSummary week = StatsService.Summarize(trend);
            WeekSummaryText.Text = string.Format(
                AppStrings.StatsRangeFormat,
                week.Done,
                week.Total,
                week.Percent,
                week.FullDays)
                + " · "
                + string.Format(AppStrings.StatsStudyFormat, StatsService.FormatStudyDuration(week.StudySeconds));

            DateTime monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            RangeSummary month = StatsService.Summarize(StatsService.DailyStatsRange(monthStart, DateTime.Today));
            MonthSummaryText.Text = AppStrings.StatsMonthTitle
                + "："
                + string.Format(AppStrings.StatsRangeFormat, month.Done, month.Total, month.Percent, month.FullDays)
                + " · "
                + string.Format(AppStrings.StatsStudyFormat, StatsService.FormatStudyDuration(month.StudySeconds));

            _achievements.Clear();
            foreach (Achievement item in StatsService.EvaluateAchievements())
            {
                _achievements.Add(item);
            }
        }

        /// <summary>展示"发现新版本"提示条。</summary>
        public void ShowUpdate(UpdateInfo info)
        {
            UpdateBannerText.Text = string.Format(AppStrings.UpdateBannerFormat, info.Version);
            UpdateBanner.Visibility = Visibility.Visible;
        }

        private void OnUpdateFound(object? sender, UpdateInfo info)
        {
            ShowUpdate(info);
        }

        private void UpdateHeader()
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(AppConstants.CultureName);
            TodaySubtitle.Text = DateTime.Now.ToString(AppConstants.LongDateFormat, culture);

            DayProgress progress = DailyStats.CountFor(DateTime.Today);
            ProgressText.Text = string.Format(
                AppStrings.TodayProgressFormat,
                progress.Done,
                progress.Total,
                progress.Percent);
            TodayProgress.Value = progress.Percent;
        }

        private void UpdatePendingBanner()
        {
            int pendingCount = CheckInService.Instance.Pending.Count;
            if (pendingCount == 0)
            {
                PendingBanner.Visibility = Visibility.Collapsed;
                return;
            }

            PendingBannerText.Text = string.Format(AppStrings.LabelPendingBannerFormat, pendingCount);
            PendingBanner.Visibility = Visibility.Visible;
        }

        private void SyncAutoStartCheck()
        {
            _suppressEvents = true;
            AutoStartCheck.IsChecked = DataStore.Instance.Data.Settings.AutoStart;
            _suppressEvents = false;
        }

        // ---------------- 日历 ----------------

        /// <summary>构建当前展示月份的 6×7 日历格（周一为一周的第一天）。</summary>
        private void BuildCalendar()
        {
            _calendarDays.Clear();

            int offset = ((int)_displayMonth.DayOfWeek + 6) % 7;
            DateTime start = _displayMonth.AddDays(-offset);
            for (int index = 0; index < 42; index++)
            {
                DateTime date = start.AddDays(index);
                _calendarDays.Add(new CalendarDayViewModel(
                    date,
                    isCurrentMonth: date.Month == _displayMonth.Month && date.Year == _displayMonth.Year,
                    isSelected: date.Date == _selectedDate.Date));
            }

            CalendarMonthText.Text = string.Format(
                AppStrings.CalendarMonthFormat,
                _displayMonth.Year,
                _displayMonth.Month);
        }

        private void RefreshCalendarStats()
        {
            foreach (CalendarDayViewModel day in _calendarDays)
            {
                day.RefreshStats();
            }
        }

        private void SelectDate(DateTime date)
        {
            _selectedDate = date.Date;
            DateTime targetMonth = new DateTime(date.Year, date.Month, 1);

            if (targetMonth != _displayMonth)
            {
                _displayMonth = targetMonth;
                BuildCalendar();
            }
            else
            {
                foreach (CalendarDayViewModel day in _calendarDays)
                {
                    day.IsSelected = day.Date.Date == _selectedDate;
                }
            }

            SyncRecords();
        }

        private void OnCalendarDayClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CalendarDayViewModel cell)
            {
                SelectDate(cell.Date);
            }
        }

        private void OnPrevMonthClick(object sender, RoutedEventArgs e)
        {
            _displayMonth = _displayMonth.AddMonths(-1);
            BuildCalendar();
        }

        private void OnNextMonthClick(object sender, RoutedEventArgs e)
        {
            _displayMonth = _displayMonth.AddMonths(1);
            BuildCalendar();
        }

        private void OnTodayClick(object sender, RoutedEventArgs e)
        {
            SelectDate(DateTime.Today);
        }

        // ---------------- 交互 ----------------

        private void OnMilestoneToggled(LearningTask task)
        {
            // 勾选里程碑后立即落盘，并同步"当天是否已完成"的状态
            TaskStateSync.SyncMilestoneCompletion(task, DateTime.Now);
            DataStore.Instance.SaveAndNotify();
        }

        private void OnNewTaskClick(object sender, RoutedEventArgs e)
        {
            TaskEditWindow dialog = new TaskEditWindow(null) { Owner = this };
            if (dialog.ShowDialog() != true || dialog.Result == null)
            {
                return;
            }

            DataStore.Instance.Data.Tasks.Add(dialog.Result);
            TaskStateSync.SyncMilestoneCompletion(dialog.Result, DateTime.Now);
            DataStore.Instance.SaveAndNotify();
            SyncTaskList(forceRebuild: true);
        }

        private void OnEditTaskClick(object sender, RoutedEventArgs e)
        {
            TaskCardViewModel? card = GetCardFromSender(sender);
            if (card == null)
            {
                return;
            }

            TaskEditWindow dialog = new TaskEditWindow(card.Task) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            TaskStateSync.SyncMilestoneCompletion(card.Task, DateTime.Now);
            DataStore.Instance.SaveAndNotify();
            SyncTaskList(forceRebuild: true);
        }

        private void OnDeleteTaskClick(object sender, RoutedEventArgs e)
        {
            TaskCardViewModel? card = GetCardFromSender(sender);
            if (card == null)
            {
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                this,
                string.Format(AppStrings.ConfirmDeleteMessageFormat, card.Title),
                AppStrings.ConfirmDeleteTitle,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (confirm != MessageBoxResult.OK)
            {
                return;
            }

            CheckInService.Instance.RemovePending(card.Task.Id);
            DataStore.Instance.RemoveTask(card.Task.Id);
            SyncTaskList(forceRebuild: true);
        }

        private void OnCheckNowClick(object sender, RoutedEventArgs e)
        {
            TaskCardViewModel? card = GetCardFromSender(sender);
            if (card == null)
            {
                return;
            }

            Host?.RaiseCheckNow(card.Task);
        }

        /// <summary>停用 / 启用任务：停用后不再提醒，随时可从卡片恢复。</summary>
        private void OnToggleEnabledClick(object sender, RoutedEventArgs e)
        {
            TaskCardViewModel? card = GetCardFromSender(sender);
            if (card == null)
            {
                return;
            }

            bool enabled = !card.Task.Enabled;
            card.Task.Enabled = enabled;

            if (enabled)
            {
                // 重新启用：按当前时刻恢复排期（今天已完成的任务不再排期）
                TaskProgress progress = DataStore.Instance.Today.GetOrCreate(card.Task.Id);
                if (!SchedulePlanner.IsTaskFinished(card.Task, progress))
                {
                    SchedulePlanner.ApplyInitial(card.Task, progress, DateTime.Now);
                }
            }
            else
            {
                // 停用后立即撤下待确认项，避免继续询问
                CheckInService.Instance.RemovePending(card.Task.Id);
            }

            DataStore.Instance.SaveAndNotify();
            SyncTaskList(forceRebuild: true);
        }

        /// <summary>开始 / 结束学习计时（时长累计到当天）。</summary>
        private void OnToggleStudyClick(object sender, RoutedEventArgs e)
        {
            TaskCardViewModel? card = GetCardFromSender(sender);
            if (card == null)
            {
                return;
            }

            StudySessionService.Toggle(card.Task, DateTime.Now);
            SyncTaskList(forceRebuild: true);
        }

        /// <summary>补打卡：把选中的过去日期补记为完成。</summary>
        private void OnBackfillClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not BackfillRowViewModel row)
            {
                return;
            }

            CheckInService.Instance.Backfill(
                row.TaskId,
                _selectedDate.ToString(AppConstants.DateFormat),
                DateTime.Now);
        }

        /// <summary>打开设置窗口。</summary>
        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow dialog = new SettingsWindow { Owner = this };
            dialog.ShowDialog();

            // 设置可能影响界面口径：每日目标、进度等
            RefreshStats();
            UpdateHeader();
        }

        /// <summary>打开新版本下载页。</summary>
        private void OnGoDownloadClick(object sender, RoutedEventArgs e)
        {
            string url = Host?.UpdateInfo?.Url ?? AppConstants.DownloadPageUrl;
            LinkLauncher.TryOpen(url);
        }

        /// <summary>关闭"发现新版本"提示条。</summary>
        private void OnDismissUpdateClick(object sender, RoutedEventArgs e)
        {
            UpdateBanner.Visibility = Visibility.Collapsed;
        }

        private void OnOpenLinkClick(object sender, RoutedEventArgs e)
        {
            TaskCardViewModel? card = GetCardFromSender(sender);
            if (card == null)
            {
                return;
            }

            if (!LinkLauncher.TryOpen(card.Task.LinkUrl))
            {
                MessageBox.Show(
                    this,
                    AppStrings.LinkOpenFailed,
                    AppStrings.ButtonLearnNow,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void OnCheckAllClick(object sender, RoutedEventArgs e)
        {
            Host?.RaiseAllChecksNow();
        }

        private void OnHideToTrayClick(object sender, RoutedEventArgs e)
        {
            Hide();
            NotificationService.Instance.NotifyHiddenToTray();
        }

        private void OnPendingBannerClick(object sender, MouseButtonEventArgs e)
        {
            IReadOnlyList<PendingCheckIn> pending = CheckInService.Instance.Pending;
            if (pending.Count > 0)
            {
                NotificationService.Instance.RequestCheckCard(pending[pending.Count - 1]);
            }
        }

        private void OnAutoStartChanged(object sender, RoutedEventArgs e)
        {
            if (_suppressEvents)
            {
                return;
            }

            bool enabled = AutoStartCheck.IsChecked == true;
            DataStore.Instance.Data.Settings.AutoStart = enabled;
            AutoStartService.SetEnabled(enabled);
            DataStore.Instance.Save();
            Host?.SyncAutoStartMenu();
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static TaskCardViewModel? GetCardFromSender(object sender)
        {
            return (sender as FrameworkElement)?.DataContext as TaskCardViewModel;
        }
    }
}