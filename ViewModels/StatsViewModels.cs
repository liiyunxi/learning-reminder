using System;
using System.Windows.Media;
using LearningReminder.Resources;
using LearningReminder.Services;

namespace LearningReminder.ViewModels
{
    /// <summary>统计页的趋势柱。</summary>
    public sealed class TrendBarViewModel
    {
        private const double MaxBarHeight = 104;
        private const double EmptyBarHeight = 3;

        /// <summary>构造趋势柱。</summary>
        public TrendBarViewModel(DayStat stat)
        {
            Label = string.Format(AppStrings.TrendDayLabelFormat, stat.Date.Month, stat.Date.Day);
            Tooltip = string.Format(
                AppStrings.CalendarDayTooltipFormat,
                stat.Date.ToString(AppConstants.DateFormat),
                stat.Done,
                stat.Total);

            if (stat.Total == 0)
            {
                Fill = Palette.Border;
                BarHeight = EmptyBarHeight;
                return;
            }

            Fill = stat.Percent >= 100 ? Palette.Success : Palette.Warn;
            BarHeight = Math.Max(6, MaxBarHeight * stat.Percent / 100.0);
        }

        /// <summary>横轴标签（月/日）</summary>
        public string Label { get; }

        /// <summary>悬停提示</summary>
        public string Tooltip { get; }

        /// <summary>柱体颜色：全完成绿色、部分完成橙色、无任务灰边</summary>
        public Brush Fill { get; }

        /// <summary>柱体高度（按完成百分比换算）</summary>
        public double BarHeight { get; }
    }

    /// <summary>记录页"当天任务"行（含补打卡状态）。</summary>
    public sealed class BackfillRowViewModel
    {
        /// <summary>构造行。</summary>
        public BackfillRowViewModel(string taskId, string title, bool completed, bool canBackfill)
        {
            TaskId = taskId;
            Title = title;
            Completed = completed;
            CanBackfill = canBackfill;
        }

        /// <summary>任务标识</summary>
        public string TaskId { get; }

        /// <summary>任务名称</summary>
        public string Title { get; }

        /// <summary>当天是否已完成</summary>
        public bool Completed { get; }

        /// <summary>是否显示补打卡按钮（过去日期且未完成时显示）</summary>
        public bool CanBackfill { get; }

        /// <summary>状态文案</summary>
        public string StatusText => Completed ? AppStrings.BackfillDoneText : AppStrings.BackfillTodoText;
    }
}