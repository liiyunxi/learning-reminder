namespace LearningReminder.Models
{
    /// <summary>
    /// 任务的检查方式。
    /// </summary>
    public enum CheckMode
    {
        /// <summary>定时询问：按固定间隔反复询问进度，直到当天标记完成。</summary>
        Interval = 0,

        /// <summary>里程碑：任务被拆成多个小目标，逐个勾选，全部完成即结束（可跨天）。</summary>
        Milestone = 1
    }
}