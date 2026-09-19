using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LearningReminder.Models;
using LearningReminder.Resources;
using LearningReminder.Services;

namespace LearningReminder.Views
{
    /// <summary>
    /// 新建 / 编辑任务的对话框。
    /// </summary>
    public partial class TaskEditWindow : Window
    {
        private readonly LearningTask? _editing;
        private bool _loading = true;

        /// <summary>构造对话框；传入任务表示编辑，传 null 表示新建。</summary>
        public TaskEditWindow(LearningTask? task)
        {
            InitializeComponent();

            _editing = task;
            Title = task == null ? AppStrings.TaskEditWindowTitleNew : AppStrings.TaskEditWindowTitleEdit;
            TitleBarText.Text = Title;
            ModeHint.Text = AppStrings.ModeIntervalDescription;
            MilestoneHint.Text = AppStrings.FieldMilestonesHint;

            if (task == null)
            {
                ModeIntervalRadio.IsChecked = true;
                RepeatDailyRadio.IsChecked = true;
                IntervalBox.Text = SchedulePlanner
                    .NormalizeInterval(DataStore.Instance.Data.Settings.DefaultIntervalMinutes)
                    .ToString();
                DailyTimeBox.Text = AppConstants.DefaultDailyReminderTime;
                MonthDayBox.Text = AppConstants.MinMonthDay.ToString();
            }
            else
            {
                FillFromTask(task);
            }

            _loading = false;
            UpdateModePanels();
            UpdateRepeatPanels();
            BuildTemplates();
        }

        /// <summary>保存后的任务（取消时为 null）。</summary>
        public LearningTask? Result { get; private set; }

        /// <summary>每周（单选）与自定义（多选）的星期按钮，顺序为周一到周日。</summary>
        private DayOfWeek[] WeekdayOrder => new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        private void FillFromTask(LearningTask task)
        {
            TitleBox.Text = task.Title;
            LinkBox.Text = task.LinkUrl;
            TagBox.Text = task.Tag;
            IntervalBox.Text = task.IntervalMinutes.ToString();

            bool milestoneMode = task.Mode == CheckMode.Milestone;
            ModeMilestoneRadio.IsChecked = milestoneMode;
            ModeIntervalRadio.IsChecked = !milestoneMode;

            bool noReminder = string.IsNullOrWhiteSpace(task.DailyReminderTime);
            NoReminderCheck.IsChecked = noReminder;
            DailyTimeBox.Text = noReminder ? AppConstants.DefaultDailyReminderTime : task.DailyReminderTime;
            DailyTimeBox.IsEnabled = !noReminder;

            List<string> lines = new List<string>();
            foreach (MilestoneItem item in task.Milestones)
            {
                lines.Add(item.Title);
            }

            MilestoneBox.Text = string.Join(Environment.NewLine, lines);

            FillRepeat(task.Repeat);
        }

        private void FillRepeat(RepeatRule? rule)
        {
            rule ??= RepeatRule.CreateDaily();

            SetRepeatTypeRadio(rule.Type);
            MonthDayBox.Text = rule.DayOfMonth.ToString();

            DayOfWeek[] order = WeekdayOrder;
            RadioButton[] weeklyButtons = { WeeklyMon, WeeklyTue, WeeklyWed, WeeklyThu, WeeklyFri, WeeklySat, WeeklySun };
            CheckBox[] customBoxes = { CustomMon, CustomTue, CustomWed, CustomThu, CustomFri, CustomSat, CustomSun };

            for (int index = 0; index < order.Length; index++)
            {
                DayOfWeek day = order[index];
                bool selected = rule.Weekdays.Contains(day);
                weeklyButtons[index].IsChecked = selected;
                customBoxes[index].IsChecked = selected;
            }

            if (rule.Type == RepeatType.Weekly && rule.Weekdays.Count == 0)
            {
                WeeklyMon.IsChecked = true;
            }
        }

        private void SetRepeatTypeRadio(RepeatType type)
        {
            RepeatDailyRadio.IsChecked = type == RepeatType.Daily;
            RepeatWeeklyRadio.IsChecked = type == RepeatType.Weekly;
            RepeatMonthlyRadio.IsChecked = type == RepeatType.Monthly;
            RepeatCustomRadio.IsChecked = type == RepeatType.Custom;
        }

        private void OnRepeatTypeChanged(object sender, RoutedEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            UpdateRepeatPanels();
        }

        /// <summary>按重复方式切换可见字段。</summary>
        private void UpdateRepeatPanels()
        {
            RepeatType type = GetSelectedRepeatType();
            WeeklyPanel.Visibility = type == RepeatType.Weekly ? Visibility.Visible : Visibility.Collapsed;
            MonthlyPanel.Visibility = type == RepeatType.Monthly ? Visibility.Visible : Visibility.Collapsed;
            CustomPanel.Visibility = type == RepeatType.Custom ? Visibility.Visible : Visibility.Collapsed;
        }

        private RepeatType GetSelectedRepeatType()
        {
            if (RepeatWeeklyRadio.IsChecked == true)
            {
                return RepeatType.Weekly;
            }

            if (RepeatMonthlyRadio.IsChecked == true)
            {
                return RepeatType.Monthly;
            }

            return RepeatCustomRadio.IsChecked == true ? RepeatType.Custom : RepeatType.Daily;
        }

        private void OnWorkdaysClick(object sender, RoutedEventArgs e)
        {
            // 一键勾选工作日（周一至周五）
            RepeatCustomRadio.IsChecked = true;
            CustomMon.IsChecked = true;
            CustomTue.IsChecked = true;
            CustomWed.IsChecked = true;
            CustomThu.IsChecked = true;
            CustomFri.IsChecked = true;
            CustomSat.IsChecked = false;
            CustomSun.IsChecked = false;
        }

        private void OnModeChanged(object sender, RoutedEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            UpdateModePanels();
        }

        /// <summary>按检查方式切换可见字段。</summary>
        private void UpdateModePanels()
        {
            bool milestoneMode = ModeMilestoneRadio.IsChecked == true;
            MilestonePanel.Visibility = milestoneMode ? Visibility.Visible : Visibility.Collapsed;
            IntervalPanel.Visibility = milestoneMode ? Visibility.Collapsed : Visibility.Visible;
            ModeHint.Text = milestoneMode
                ? AppStrings.ModeMilestoneDescription
                : AppStrings.ModeIntervalDescription;
        }

        private void OnQuickIntervalClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is string minutes)
            {
                IntervalBox.Text = minutes;
            }
        }

        private void OnNoReminderChanged(object sender, RoutedEventArgs e)
        {
            DailyTimeBox.IsEnabled = NoReminderCheck.IsChecked != true;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            string title = TitleBox.Text.Trim();
            if (title.Length == 0)
            {
                ShowError(AppStrings.ValidationTitleRequired);
                return;
            }

            string link = LinkBox.Text.Trim();
            if (link.Length > 0 && !LinkLauncher.IsValid(link))
            {
                ShowError(AppStrings.ValidationLinkFormat);
                return;
            }

            if (!TryReadRepeat(out RepeatRule repeat))
            {
                return;
            }

            bool milestoneMode = ModeMilestoneRadio.IsChecked == true;
            int interval = SchedulePlanner.NormalizeInterval(DataStore.Instance.Data.Settings.DefaultIntervalMinutes);

            if (!milestoneMode && !TryReadInterval(out interval))
            {
                return;
            }

            string dailyTime = string.Empty;
            List<string> milestoneTitles = new List<string>();
            if (milestoneMode)
            {
                if (!TryReadMilestones(milestoneTitles) || !TryReadDailyTime(out dailyTime))
                {
                    return;
                }
            }

            Apply(title, link, repeat, milestoneMode, interval, dailyTime, milestoneTitles);
            DialogResult = true;
        }

        /// <summary>把表单内容写回任务对象（编辑时保留已有里程碑的完成状态）。</summary>
        private void Apply(
            string title,
            string link,
            RepeatRule repeat,
            bool milestoneMode,
            int interval,
            string dailyTime,
            List<string> milestoneTitles)
        {
            LearningTask task = _editing ?? new LearningTask();
            task.Title = title;
            task.LinkUrl = link.Length == 0 ? string.Empty : LinkLauncher.Normalize(link);
            task.Tag = TagBox.Text.Trim();
            task.Repeat = repeat;
            task.Mode = milestoneMode ? CheckMode.Milestone : CheckMode.Interval;
            task.IntervalMinutes = interval;
            task.DailyReminderTime = milestoneMode ? dailyTime : string.Empty;
            // 启用 / 停用由主界面卡片控制，编辑时保持原有状态

            if (milestoneMode)
            {
                MergeMilestones(task, milestoneTitles);
            }

            Result = task;
        }

        /// <summary>读取并校验重复规则。</summary>
        private bool TryReadRepeat(out RepeatRule rule)
        {
            rule = RepeatRule.CreateDaily();
            RepeatType type = GetSelectedRepeatType();
            rule.Type = type;

            if (type == RepeatType.Weekly)
            {
                DayOfWeek? picked = ReadSingleWeekday();
                if (picked == null)
                {
                    ShowError(AppStrings.ValidationWeekdayRequired);
                    return false;
                }

                rule.Weekdays.Add(picked.Value);
                return true;
            }

            if (type == RepeatType.Custom)
            {
                List<DayOfWeek> picked = ReadCheckedWeekdays();
                if (picked.Count == 0)
                {
                    ShowError(AppStrings.ValidationWeekdayRequired);
                    return false;
                }

                rule.Weekdays.AddRange(picked);
                return true;
            }

            if (type == RepeatType.Monthly)
            {
                if (!int.TryParse(MonthDayBox.Text.Trim(), out int day)
                    || day < AppConstants.MinMonthDay
                    || day > AppConstants.MaxMonthDay)
                {
                    ShowError(string.Format(
                        AppStrings.ValidationMonthDayRangeFormat,
                        AppConstants.MinMonthDay,
                        AppConstants.MaxMonthDay));
                    return false;
                }

                rule.DayOfMonth = day;
            }

            return true;
        }

        private DayOfWeek? ReadSingleWeekday()
        {
            DayOfWeek[] order = WeekdayOrder;
            RadioButton[] buttons = { WeeklyMon, WeeklyTue, WeeklyWed, WeeklyThu, WeeklyFri, WeeklySat, WeeklySun };
            for (int index = 0; index < buttons.Length; index++)
            {
                if (buttons[index].IsChecked == true)
                {
                    return order[index];
                }
            }

            return null;
        }

        private List<DayOfWeek> ReadCheckedWeekdays()
        {
            DayOfWeek[] order = WeekdayOrder;
            CheckBox[] boxes = { CustomMon, CustomTue, CustomWed, CustomThu, CustomFri, CustomSat, CustomSun };
            List<DayOfWeek> picked = new List<DayOfWeek>();

            for (int index = 0; index < boxes.Length; index++)
            {
                if (boxes[index].IsChecked == true)
                {
                    picked.Add(order[index]);
                }
            }

            return picked;
        }

        private static void MergeMilestones(LearningTask task, List<string> titles)
        {
            List<MilestoneItem> merged = new List<MilestoneItem>();
            foreach (string title in titles)
            {
                MilestoneItem? existing = task.Milestones.Find(item => item.Title == title);
                if (existing != null)
                {
                    merged.Add(existing);
                    continue;
                }

                merged.Add(new MilestoneItem { Title = title });
            }

            task.Milestones.Clear();
            task.Milestones.AddRange(merged);
        }

        private bool TryReadInterval(out int interval)
        {
            interval = DataStore.Instance.Data.Settings.DefaultIntervalMinutes;
            if (!int.TryParse(IntervalBox.Text.Trim(), out int parsed)
                || parsed < AppConstants.MinIntervalMinutes
                || parsed > AppConstants.MaxIntervalMinutes)
            {
                ShowError(string.Format(
                    AppStrings.ValidationIntervalRangeFormat,
                    AppConstants.MinIntervalMinutes,
                    AppConstants.MaxIntervalMinutes));
                return false;
            }

            interval = parsed;
            return true;
        }

        private bool TryReadMilestones(List<string> titles)
        {
            string[] lines = MilestoneBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.Length > 0 && !titles.Contains(trimmed))
                {
                    titles.Add(trimmed);
                }
            }

            if (titles.Count == 0)
            {
                ShowError(AppStrings.ValidationMilestoneRequired);
                return false;
            }

            return true;
        }

        private bool TryReadDailyTime(out string dailyTime)
        {
            dailyTime = string.Empty;
            if (NoReminderCheck.IsChecked == true)
            {
                return true;
            }

            if (!DateTime.TryParse(DailyTimeBox.Text.Trim(), out DateTime parsed))
            {
                ShowError(AppStrings.ValidationTimeFormat);
                return false;
            }

            dailyTime = parsed.ToString(AppConstants.TimeFormat);
            return true;
        }

        /// <summary>构建模板选择区：内置模板 + 用户保存的模板（用户模板可删除）。</summary>
        private void BuildTemplates()
        {
            TemplatePanel.Children.Clear();

            foreach (TaskTemplate template in TaskTemplateLibrary.BuiltIn)
            {
                TemplatePanel.Children.Add(BuildTemplateChip(template));
            }

            foreach (TaskTemplate template in DataStore.Instance.Data.Templates)
            {
                TemplatePanel.Children.Add(BuildTemplateChip(template));
            }
        }

        private FrameworkElement BuildTemplateChip(TaskTemplate template)
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 8, 8)
            };

            Button applyButton = new Button
            {
                Content = template.Name,
                Style = (Style)FindResource("Button.Ghost"),
                Padding = new Thickness(10, 4, 10, 4),
                FontSize = 12,
                Tag = template
            };
            applyButton.Click += OnTemplateClick;
            panel.Children.Add(applyButton);

            if (!template.BuiltIn)
            {
                Button deleteButton = new Button
                {
                    Content = AppStrings.ButtonDeleteTemplate,
                    Style = (Style)FindResource("Button.Link"),
                    Padding = new Thickness(4, 2, 4, 2),
                    FontSize = 12,
                    Tag = template
                };
                deleteButton.Click += OnDeleteTemplateClick;
                panel.Children.Add(deleteButton);
            }

            return panel;
        }

        /// <summary>套用模板：把模板内容填进表单，重复规则保持当前选择。</summary>
        private void OnTemplateClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not TaskTemplate template)
            {
                return;
            }

            TitleBox.Text = template.Title;
            TagBox.Text = template.Tag;

            bool milestone = template.Mode == CheckMode.Milestone;
            ModeMilestoneRadio.IsChecked = milestone;
            ModeIntervalRadio.IsChecked = !milestone;
            IntervalBox.Text = SchedulePlanner.NormalizeInterval(template.IntervalMinutes).ToString();
            NoReminderCheck.IsChecked = false;
            DailyTimeBox.Text = string.IsNullOrWhiteSpace(template.DailyReminderTime)
                ? AppConstants.DefaultDailyReminderTime
                : template.DailyReminderTime;
            MilestoneBox.Text = string.Join(Environment.NewLine, template.Milestones);

            UpdateModePanels();
        }

        private void OnDeleteTemplateClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not TaskTemplate template)
            {
                return;
            }

            DataStore.Instance.Data.Templates.RemoveAll(item => item.Id == template.Id);
            DataStore.Instance.Save();
            BuildTemplates();
        }

        /// <summary>把当前表单内容另存为模板，供以后一键套用。</summary>
        private void OnSaveTemplateClick(object sender, RoutedEventArgs e)
        {
            string title = TitleBox.Text.Trim();
            if (title.Length == 0)
            {
                ShowError(AppStrings.ValidationTemplateNameRequired);
                return;
            }

            TaskTemplate template = new TaskTemplate
            {
                Name = title,
                Title = title,
                Tag = TagBox.Text.Trim(),
                Mode = ModeMilestoneRadio.IsChecked == true ? CheckMode.Milestone : CheckMode.Interval,
                IntervalMinutes = SchedulePlanner.NormalizeInterval(
                    int.TryParse(IntervalBox.Text.Trim(), out int interval)
                        ? interval
                        : DataStore.Instance.Data.Settings.DefaultIntervalMinutes)
            };

            if (template.Mode == CheckMode.Milestone)
            {
                template.DailyReminderTime = NoReminderCheck.IsChecked == true
                    ? string.Empty
                    : (DateTime.TryParse(DailyTimeBox.Text.Trim(), out DateTime time)
                        ? time.ToString(AppConstants.TimeFormat)
                        : AppConstants.DefaultDailyReminderTime);
                template.Milestones.AddRange(ReadMilestoneLines());
            }

            DataStore.Instance.Data.Templates.Add(template);
            DataStore.Instance.Save();
            BuildTemplates();
        }

        private List<string> ReadMilestoneLines()
        {
            List<string> titles = new List<string>();
            string[] lines = MilestoneBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.Length > 0 && !titles.Contains(trimmed))
                {
                    titles.Add(trimmed);
                }
            }

            return titles;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}