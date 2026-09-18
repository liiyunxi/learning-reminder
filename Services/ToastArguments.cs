using System;

namespace LearningReminder.Services
{
    /// <summary>
    /// 通知参数键（与 Toast 上的按钮一一对应）。
    /// </summary>
    public static class ToastArgumentKeys
    {
        /// <summary>动作键</summary>
        public const string Action = "action";

        /// <summary>任务标识键</summary>
        public const string TaskId = "taskId";
    }

    /// <summary>
    /// 通知上的动作取值。
    /// </summary>
    public static class ToastActions
    {
        /// <summary>已完成</summary>
        public const string Done = "done";

        /// <summary>还没完成</summary>
        public const string NotYet = "notyet";

        /// <summary>稍后再说</summary>
        public const string Snooze = "snooze";

        /// <summary>打开确认卡片</summary>
        public const string Open = "open";

        /// <summary>打开学习链接</summary>
        public const string Learn = "learn";
    }

    /// <summary>
    /// 解析通知回传的形如 action=done;taskId=xxx 的参数串。
    /// 注意：通知参数的分隔符可能是 &amp;、; 或空格（不同激活方式不一样），这里统一兼容。
    /// </summary>
    public static class QueryStringHelper
    {
        private static readonly char[] Separators = { '&', ';', ' ', '\t', '\r', '\n' };

        /// <summary>取指定键的值，取不到返回空串。</summary>
        public static string GetValue(string query, string key)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            string[] segments = query.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (string segment in segments)
            {
                int separatorIndex = segment.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string name = segment.Substring(0, separatorIndex);
                if (name == key)
                {
                    return segment.Substring(separatorIndex + 1);
                }
            }

            return string.Empty;
        }
    }
}