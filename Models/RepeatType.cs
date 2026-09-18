using System;

namespace LearningReminder.Models
{
    /// <summary>
    /// 重复方式。
    /// </summary>
    public enum RepeatType
    {
        /// <summary>每天</summary>
        Daily = 0,

        /// <summary>每周（固定一个星期几）</summary>
        Weekly = 1,

        /// <summary>每月（固定一个日期）</summary>
        Monthly = 2,

        /// <summary>自定义：选择每周的哪几天执行</summary>
        Custom = 3
    }
}