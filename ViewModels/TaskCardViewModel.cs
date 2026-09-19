using System;
using System.Collections.ObjectModel;
using LearningReminder.Models;
using LearningReminder.Resources;
using LearningReminder.Services;

namespace LearningReminder.ViewModels
{
    /// <summary>
    /// 今日任务卡片视图模型。
    /// </summary>
    public sealed class TaskCardViewModel : ObservableObject
    {
        private string _detailText = string.Empty;
        private string _countdownText = string.Empty;
        private string _statusText = string.Empty;
        private bool _showMilestones;

        /// <summary>构造卡片。</summary>
        public TaskCardViewModel(LearningTask task, Action<LearningTask> onMilestoneToggled)
        {
            Task = task;
            foreach (MilestoneItem item in task.Milestones)
            {
                Milestones.Add(new MilestoneRowViewModel(item, () => onMilestoneToggled(task)));
            }

            Refresh();
        }

        /// <summary>对应的任务定义</summary>
        public LearningTask Task { get; }

        /// <summary>任务名称</summary>
        public string Title => Task.Title;

        /// <summary>模式标签</summary>
        public string ModeText => Task.Mode == CheckMode.Interval
            ? AppStrings.ModeIntervalLabel
            : AppStrings.ModeMilestoneLabel;

        /// <summary>重复规则标签，例如"每天"、"每周一、三"</summary>
        public string RepeatLabel => RepeatText.Describe(Task.Repeat);

        /// <summary>是否配置了学习链接</summary>
        public bool HasLink => Task.HasLink;

        /// <summary>是否已停用（停用后不再提醒，可从卡片一键恢复）</summary>
        public bool IsDisabled => !Task.Enabled;

        /// <summary>停用 / 启用按钮的文案</summary>
        public string EnabledButtonText => Task.Enabled ? AppStrings.ButtonDisable : AppStrings.ButtonEnable;

        /// <summary>是否允许发起检查（停用中的任务不允许）</summary>
        public bool CanCheck => Task.Enabled;

        /// <summary>副标题：间隔说明或里程碑进度</summary>
        public string DetailText
        {
            get => _detailText;
            private set => SetField(ref _detailText, value);
        }

        /// <summary>倒计时提示</summary>
        public string CountdownText
        {
            get => _countdownText;
            private set => SetField(ref _countdownText, value);
        }

        /// <summary>状态标签文案，为空时界面隐藏该标签</summary>
        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (SetField(ref _statusText, value))
                {
                    OnPropertyChanged(nameof(HasStatus));
                }
            }
        }

        /// <summary>是否显示状态标签</summary>
        public bool HasStatus => !string.IsNullOrEmpty(_statusText);

        /// <summary>里程碑清单</summary>
        public ObservableCollection<MilestoneRowViewModel> Milestones { get; } =
            new ObservableCollection<MilestoneRowViewModel>();

        /// <summary>是否展示里程碑清单</summary>
        public bool ShowMilestones
        {
            get => _showMilestones;
            private set => SetField(ref _showMilestones, value);
        }

        /// <summary>刷新卡片上的派生信息。</summary>
        public void Refresh()
        {
            ShowMilestones = Task.Mode == CheckMode.Milestone && Milestones.Count > 0;
            DetailText = BuildDetailText();
            StatusText = BuildStatusText();
            CountdownText = BuildCountdownText(DateTime.Now);

            // 启用状态变化时，即使不重建卡片也要刷新按钮与标签
            OnPropertyChanged(nameof(IsDisabled));
            OnPropertyChanged(nameof(EnabledButtonText));
            OnPropertyChanged(nameof(CanCheck));
        }

        /// <summary>每秒调用一次，只更新倒计时文本。</summary>
        public void RefreshCountdown(DateTime now)
        {
            CountdownText = BuildCountdownText(now);
        }

        private string BuildDetailText()
        {
            if (Task.Mode == CheckMode.Milestone)
            {
                return string.Format(
                    AppStrings.MilestoneProgressFormat,
                    Task.DoneMilestoneCount(),
                    Task.Milestones.Count);
            }

            return string.Format(
                AppStrings.IntervalLabelFormat,
                SchedulePlanner.NormalizeInterval(Task.IntervalMinutes));
        }

        private string BuildStatusText()
        {
            if (!Task.Enabled)
            {
                // 停用状态由独立标签展示，避免与"已完成"等信息叠加
                return string.Empty;
            }

            DailyRecord today = DataStore.Instance.Today;
            TaskProgress progress = today.GetOrCreate(Task.Id);

            if (Task.Mode == CheckMode.Milestone)
            {
                return Task.IsMilestoneFinished() ? AppStrings.StatusMilestoneFinished : string.Empty;
            }

            return progress.IsCompleted ? AppStrings.StatusCompletedToday : string.Empty;
        }

        private string BuildCountdownText(DateTime now)
        {
            if (!Task.Enabled)
            {
                // 停用中不显示排期，避免误以为到点还会提醒
                return string.Empty;
            }

            DailyRecord today = DataStore.Instance.Today;
            TaskProgress progress = today.GetOrCreate(Task.Id);

            if (SchedulePlanner.IsTaskFinished(Task, progress))
            {
                return AppStrings.TooltipFinished;
            }

            if (CheckInService.Instance.Find(Task.Id) != null)
            {
                return AppStrings.CountdownPending;
            }

            if (progress.NextCheckAt == null)
            {
                // 里程碑任务未设置每日核对时间：只靠手动检查
                return SchedulePlanner.TryGetDailyReminderToday(Task.DailyReminderTime, out DateTime reminder)
                    ? string.Format(AppStrings.DailyReminderLabelFormat, reminder.ToString(AppConstants.TimeFormat))
                    : AppStrings.DailyReminderOff;
            }

            DateTime next = progress.NextCheckAt.Value;
            TimeSpan remain = next - now;
            if (remain.TotalMinutes < 1)
            {
                return AppStrings.CountdownSoon;
            }

            return string.Format(
                AppStrings.CountdownFormat,
                next.ToString(AppConstants.TimeFormat),
                (int)Math.Ceiling(remain.TotalMinutes));
        }
    }
}