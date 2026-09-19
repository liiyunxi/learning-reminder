using System.Collections.Generic;
using LearningReminder.Models;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 内置任务模板库：给"新建任务"提供常见学习场景的预设（不可删除）。
    /// </summary>
    public static class TaskTemplateLibrary
    {
        private static List<TaskTemplate>? _builtIn;

        /// <summary>内置模板。</summary>
        public static IReadOnlyList<TaskTemplate> BuiltIn => _builtIn ??= CreateBuiltIn();

        private static List<TaskTemplate> CreateBuiltIn()
        {
            return new List<TaskTemplate>
            {
                new TaskTemplate
                {
                    Name = AppStrings.TemplateNameVocab,
                    Title = AppStrings.TemplateTitleVocab,
                    Mode = CheckMode.Interval,
                    IntervalMinutes = AppConstants.DefaultIntervalMinutes,
                    Tag = AppStrings.TagEnglish,
                    BuiltIn = true
                },
                new TaskTemplate
                {
                    Name = AppStrings.TemplateNameCourse,
                    Title = AppStrings.TemplateTitleCourse,
                    Mode = CheckMode.Interval,
                    IntervalMinutes = AppConstants.DefaultIntervalMinutes,
                    Tag = AppStrings.TagCourse,
                    BuiltIn = true
                },
                new TaskTemplate
                {
                    Name = AppStrings.TemplateNameReading,
                    Title = AppStrings.TemplateTitleReading,
                    Mode = CheckMode.Interval,
                    IntervalMinutes = AppConstants.DefaultIntervalMinutes,
                    Tag = AppStrings.TagReading,
                    BuiltIn = true
                },
                new TaskTemplate
                {
                    Name = AppStrings.TemplateNameCoding,
                    Title = AppStrings.TemplateTitleCoding,
                    Mode = CheckMode.Milestone,
                    DailyReminderTime = AppConstants.DefaultDailyReminderTime,
                    Tag = AppStrings.TagCoding,
                    Milestones = new List<string>
                    {
                        AppStrings.TemplateCodingMilestone1,
                        AppStrings.TemplateCodingMilestone2,
                        AppStrings.TemplateCodingMilestone3
                    },
                    BuiltIn = true
                }
            };
        }
    }
}