using System;

namespace LearningReminder.Services
{
    /// <summary>
    /// 启动参数定义与解析。
    /// </summary>
    public static class AppStartupOptions
    {
        /// <summary>静默启动（开机自启时使用，直接驻留托盘）</summary>
        public const string MinimizedArgument = "--minimized";

        /// <summary>是否要求静默启动（不显示主窗口）。</summary>
        public static bool IsMinimizedStart(string[] args)
        {
            foreach (string arg in args)
            {
                if (string.Equals(arg, MinimizedArgument, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 尽力从命令行里解析通知按钮回传的动作参数。
        /// 不同激活方式（进程内激活 / 重新拉起进程）参数形态不同，
        /// 这里把空格、分号统一归一化为 &amp; 后按键值对解析，解析不到就当作普通启动。
        /// </summary>
        public static bool TryParseAnswer(string commandLine, out string taskId, out string action)
        {
            taskId = string.Empty;
            action = string.Empty;

            if (string.IsNullOrWhiteSpace(commandLine))
            {
                return false;
            }

            string normalized = commandLine
                .Replace(';', '&')
                .Replace(' ', '&')
                .Replace('\t', '&');

            action = QueryStringHelper.GetValue(normalized, ToastArgumentKeys.Action);
            taskId = QueryStringHelper.GetValue(normalized, ToastArgumentKeys.TaskId);

            return !string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(taskId);
        }
    }
}