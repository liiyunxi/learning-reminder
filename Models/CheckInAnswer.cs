namespace LearningReminder.Models
{
    /// <summary>
    /// 一次检查的应答结果，用于历史记录统计。
    /// </summary>
    public enum CheckInAnswer
    {
        /// <summary>已完成</summary>
        Completed = 0,

        /// <summary>仍未完成</summary>
        NotYet = 1,

        /// <summary>主动推迟</summary>
        Snoozed = 2,

        /// <summary>未应答，自动推迟</summary>
        AutoSnoozed = 3,

        /// <summary>里程碑模式下更新了部分进度</summary>
        Partial = 4
    }
}