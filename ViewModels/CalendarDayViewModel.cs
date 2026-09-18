using System;
using System.Windows.Media;
using LearningReminder.Resources;
using LearningReminder.Services;

namespace LearningReminder.ViewModels
{
    /// <summary>
    /// 日历里的一格（一天）：显示日期与当天完成度。
    /// </summary>
    public sealed class CalendarDayViewModel : ObservableObject
    {
        private bool _isSelected;

        /// <summary>构造一格日历。</summary>
        public CalendarDayViewModel(DateTime date, bool isCurrentMonth, bool isSelected)
        {
            Date = date;
            IsCurrentMonth = isCurrentMonth;
            _isSelected = isSelected;
            RefreshStats();
        }

        /// <summary>这一格对应的日期</summary>
        public DateTime Date { get; }

        /// <summary>是否属于当前展示的月份</summary>
        public bool IsCurrentMonth { get; }

        /// <summary>日期数字</summary>
        public string DayText => Date.Day.ToString();

        /// <summary>是否今天</summary>
        public bool IsToday => Date.Date == DateTime.Today;

        /// <summary>是否被选中</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetField(ref _isSelected, value))
                {
                    RefreshStats();
                }
            }
        }

        /// <summary>当天完成情况提示</summary>
        public string TooltipText { get; private set; } = string.Empty;

        /// <summary>格子背景</summary>
        public Brush DayBackground { get; private set; } = Palette.Surface;

        /// <summary>日期文字颜色</summary>
        public Brush DayForeground { get; private set; } = Palette.Text;

        /// <summary>完成度圆点颜色</summary>
        public Brush IndicatorBrush { get; private set; } = Palette.Border;

        /// <summary>是否显示完成度圆点</summary>
        public bool ShowIndicator { get; private set; }

        /// <summary>格子边框</summary>
        public Brush DayBorder { get; private set; } = Palette.Surface;

        /// <summary>格子边框粗细</summary>
        public double DayBorderThickness { get; private set; }

        /// <summary>重新计算当天的完成度与配色。</summary>
        public void RefreshStats()
        {
            DayProgress progress = DailyStats.CountFor(Date);
            TooltipText = progress.HasTask
                ? string.Format(AppStrings.CalendarDayTooltipFormat, Date.ToString(AppConstants.DateFormat), progress.Done, progress.Total)
                : string.Format(AppStrings.CalendarDayNoTask, Date.ToString(AppConstants.DateFormat));

            if (IsSelected)
            {
                DayBackground = Palette.Primary;
                DayForeground = Palette.Surface;
                IndicatorBrush = Palette.Surface;
                ShowIndicator = progress.HasTask;
                DayBorder = Palette.Primary;
                DayBorderThickness = 0;
            }
            else
            {
                DayBackground = progress.Percent >= 100
                    ? Palette.SuccessSoft
                    : (progress.Done > 0 ? Palette.WarnSoft : Palette.Surface);
                DayForeground = IsCurrentMonth ? Palette.Text : Palette.Disabled;
                IndicatorBrush = progress.Percent >= 100
                    ? Palette.Success
                    : (progress.Done > 0 ? Palette.Warn : Palette.Border);
                ShowIndicator = progress.HasTask;
                // 今天用主色描边标识
                DayBorder = IsToday ? Palette.Primary : Palette.Surface;
                DayBorderThickness = IsToday ? 1.4 : 0;
            }

            OnPropertyChanged(nameof(TooltipText));
            OnPropertyChanged(nameof(DayBackground));
            OnPropertyChanged(nameof(DayForeground));
            OnPropertyChanged(nameof(IndicatorBrush));
            OnPropertyChanged(nameof(ShowIndicator));
            OnPropertyChanged(nameof(DayBorder));
            OnPropertyChanged(nameof(DayBorderThickness));
        }
    }
}