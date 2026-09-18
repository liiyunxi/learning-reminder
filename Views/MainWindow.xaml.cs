using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
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

        private DateTime _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private DateTime _selectedDate = DateTime.Today;
        private bool _suppressEvents = true;

        /// <summary>构造主窗口。</summary>
        public MainWindow()
        {
            InitializeComponent();

            TaskList.ItemsSource = _taskCards;
            RecordList.ItemsSource = _records;
            CalendarDays.ItemsSource = _calendarDays;
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
            UpdateBanner();
            SyncAutoStartCheck();
            _suppressEvents = false;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            DataStore.Instance.Changed -= OnDataChanged;
            DataStore.Instance.DayRolledOver -= OnDayRolledOver;
            CheckInService.Instance.PendingChanged -= OnPendingChanged;

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
            SyncTaskList(forceRebuild: false);
            RefreshCalendarStats();
            SyncRecords();
            UpdateHeader();
            UpdateBanner();
        }

        private void OnDayRolledOver(object? sender, EventArgs e)
        {
            _selectedDate = DateTime.Today;
            _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            BuildCalendar();
            SyncTaskList(forceRebuild: true);
            SyncRecords();
            UpdateHeader();
        }

        private void OnPendingChanged(object? sender, EventArgs e)
        {
            UpdateBanner();
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

        private void UpdateBanner()
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