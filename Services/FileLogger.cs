using System;
using System.IO;
using System.Text;

namespace LearningReminder.Services
{
    /// <summary>
    /// 极简文件日志：只记录异常与关键事件，方便长期后台运行时排查问题。
    /// 文件超过上限会自动截断，避免无限增长。
    /// </summary>
    public static class FileLogger
    {
        private const long MaxLogBytes = 512 * 1024;

        private static readonly object SyncRoot = new object();

        /// <summary>记录一条信息</summary>
        public static void Info(string message)
        {
            Write("INFO", message);
        }

        /// <summary>记录一条异常（含堆栈）</summary>
        public static void Error(string message, Exception? exception = null)
        {
            string text = exception == null
                ? message
                : message + Environment.NewLine + exception;
            Write("ERROR", text);
        }

        private static void Write(string level, string message)
        {
            try
            {
                lock (SyncRoot)
                {
                    string path = AppPaths.LogFile;
                    TrimIfTooLarge(path);

                    string line = string.Format(
                        "[{0:yyyy-MM-dd HH:mm:ss}] [{1}] {2}{3}",
                        DateTime.Now,
                        level,
                        message,
                        Environment.NewLine);
                    File.AppendAllText(path, line, Encoding.UTF8);
                }
            }
            catch
            {
                // 日志写入失败时不得影响主流程
            }
        }

        private static void TrimIfTooLarge(string path)
        {
            FileInfo info = new FileInfo(path);
            if (!info.Exists || info.Length <= MaxLogBytes)
            {
                return;
            }

            // 保留后半段日志，丢弃最早的记录
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            int keepFrom = lines.Length / 2;
            StringBuilder builder = new StringBuilder();
            for (int i = keepFrom; i < lines.Length; i++)
            {
                builder.AppendLine(lines[i]);
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }
    }
}