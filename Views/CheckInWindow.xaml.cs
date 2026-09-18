using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LearningReminder.Models;
using LearningReminder.Resources;
using LearningReminder.Services;
using LearningReminder.ViewModels;

namespace LearningReminder.Views
{
    /// <summary>
    /// 进度确认卡片：不抢焦点、不占任务栏，出现在屏幕右下角，带操作按钮。
    /// 超时无人应答按「稍后再说」处理。
    /// </summary>
    public partial class CheckInWindow : Window
    {
        private static readonly List<CheckInWindow> OpenWindows = new List<CheckInWindow>();

        private readonly DispatcherTimer _countdownTimer;
        private string _taskId = string.Empty;
        private int _remainSeconds = AppConstants.CheckCardAutoCloseSeconds;

        private CheckInWindow(PendingCheckIn pending)
        {
            InitializeComponent();

            _countdownTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += OnCountdownTick;

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;

            Bind(pending);
        }

        /// <summary>显示指定任务的确认卡片（同一任务只保留一个卡片）。</summary>
        public static void ShowFor(PendingCheckIn pending)
        {
            foreach (CheckInWindow window in OpenWindows)
            {
                if (window._taskId == pending.TaskId)
                {
                    window.BringToFront();
                    return;
                }
            }

            CheckInWindow created = new CheckInWindow(pending);
            OpenWindows.Add(created);
            created.Show();
        }

        private void Bind(PendingCheckIn pending)
        {
            _taskId = pending.TaskId;
            _remainSeconds = AppConstants.CheckCardAutoCloseSeconds;

            LearningTask? task = DataStore.Instance.FindTask(pending.TaskId);
            TaskTitleText.Text = pending.Title;

            bool milestoneMode = task != null && task.Mode == CheckMode.Milestone;
            if (milestoneMode && task != null)
            {
                HintText.Text = AppStrings.CheckCardHintMilestone;
                MilestoneList.ItemsSource = BuildMilestoneRows(task);
                MilestoneList.Visibility = Visibility.Visible;
                ProgressText.Visibility = Visibility.Visible;
                ProgressText.Text = string.Format(
                    AppStrings.MilestoneProgressFormat,
                    task.DoneMilestoneCount(),
                    task.Milestones.Count);

                PrimaryButton.Content = AppStrings.ButtonSaveProgress;
                SecondaryButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                HintText.Text = AppStrings.CheckCardHintInterval;
                PrimaryButton.Content = AppStrings.AnswerDone;
                SecondaryButton.Content = AppStrings.AnswerNotYet;
            }

            SnoozeButton.Content = string.Format(
                AppStrings.AnswerSnoozeFormat,
                DataStore.Instance.Data.Settings.SnoozeMinutes);

            // 有学习链接时，卡片上给一个直达网页的入口
            if (task != null && task.HasLink)
            {
                LearnButton.Content = AppStrings.ButtonLearnNow;
                LearnButton.Visibility = Visibility.Visible;
            }

            UpdateAutoCloseText();
        }

        private static List<MilestoneRowViewModel> BuildMilestoneRows(LearningTask task)
        {
            List<MilestoneRowViewModel> rows = new List<MilestoneRowViewModel>();
            foreach (MilestoneItem item in task.Milestones)
            {
                rows.Add(new MilestoneRowViewModel(item, OnMilestoneToggled));
            }

            return rows;
        }

        private static void OnMilestoneToggled()
        {
            // 勾选即落盘，避免用户直接关掉卡片时丢失进度
            DataStore.Instance.Save();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            MoveToCorner();
            PlayFadeIn();
            _countdownTimer.Start();
            CheckInService.Instance.PendingChanged += OnPendingChanged;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            _countdownTimer.Stop();
            CheckInService.Instance.PendingChanged -= OnPendingChanged;
            OpenWindows.Remove(this);
        }

        /// <summary>停靠到工作区右下角。</summary>
        private void MoveToCorner()
        {
            Rect workArea = SystemParameters.WorkArea;
            Left = workArea.Right - ActualWidth - 8;
            Top = workArea.Bottom - ActualHeight - 8;
        }

        private void PlayFadeIn()
        {
            DoubleAnimation animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
            BeginAnimation(OpacityProperty, animation);
        }

        private void BringToFront()
        {
            MoveToCorner();
            _remainSeconds = AppConstants.CheckCardAutoCloseSeconds;
            UpdateAutoCloseText();
        }

        private void OnPendingChanged(object? sender, EventArgs e)
        {
            // 已经通过通知按钮应答过，卡片自动收起
            if (CheckInService.Instance.Find(_taskId) == null)
            {
                Close();
            }
        }

        private void OnCountdownTick(object? sender, EventArgs e)
        {
            _remainSeconds--;
            UpdateAutoCloseText();

            if (_remainSeconds <= 0)
            {
                _countdownTimer.Stop();
                CheckInService.Instance.Snooze(_taskId, DateTime.Now, markAuto: true);
                Close();
            }
        }

        private void UpdateAutoCloseText()
        {
            AutoCloseText.Text = string.Format(AppStrings.CheckCardAutoCloseFormat, _remainSeconds);
        }

        private void OnPrimaryClick(object sender, RoutedEventArgs e)
        {
            LearningTask? task = DataStore.Instance.FindTask(_taskId);
            DateTime now = DateTime.Now;

            if (task != null && task.Mode == CheckMode.Milestone)
            {
                CheckInService.Instance.ApplyMilestones(_taskId, CollectDoneMilestoneIds(), now);
            }
            else
            {
                CheckInService.Instance.Complete(_taskId, now);
            }

            Close();
        }

        private void OnSecondaryClick(object sender, RoutedEventArgs e)
        {
            CheckInService.Instance.NotYet(_taskId, DateTime.Now);
            Close();
        }

        private void OnSnoozeClick(object sender, RoutedEventArgs e)
        {
            CheckInService.Instance.Snooze(_taskId, DateTime.Now, markAuto: false);
            Close();
        }

        private void OnLearnClick(object sender, RoutedEventArgs e)
        {
            // 打开链接但不关闭卡片，方便学完回来继续打卡
            _remainSeconds = AppConstants.CheckCardAutoCloseSeconds;
            UpdateAutoCloseText();
            NotificationService.Instance.OpenLearningLink(_taskId);
        }

        private List<string> CollectDoneMilestoneIds()
        {
            List<string> ids = new List<string>();
            if (MilestoneList.ItemsSource is IEnumerable<MilestoneRowViewModel> rows)
            {
                foreach (MilestoneRowViewModel row in rows)
                {
                    if (row.IsDone)
                    {
                        ids.Add(row.Id);
                    }
                }
            }

            return ids;
        }
    }
}