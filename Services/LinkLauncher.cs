using System;
using System.Diagnostics;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 学习链接：校验与打开默认浏览器。
    /// </summary>
    public static class LinkLauncher
    {
        /// <summary>把用户输入整理成可用的 URL（缺少协议时补 https://）。</summary>
        public static string Normalize(string url)
        {
            string trimmed = (url ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                return string.Empty;
            }

            if (trimmed.IndexOf("://", StringComparison.Ordinal) < 0)
            {
                trimmed = AppConstants.DefaultUrlScheme + trimmed;
            }

            return trimmed;
        }

        /// <summary>是否是合法的 http/https 链接。</summary>
        public static bool IsValid(string url)
        {
            string normalized = Normalize(url);
            if (normalized.Length == 0)
            {
                return false;
            }

            if (!Uri.TryCreate(normalized, UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        /// <summary>用默认浏览器打开链接，失败时返回 false。</summary>
        public static bool TryOpen(string url)
        {
            string normalized = Normalize(url);
            if (!IsValid(normalized))
            {
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo(normalized) { UseShellExecute = true });
                FileLogger.Info("打开学习链接：" + normalized);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("打开学习链接失败：" + normalized, ex);
                return false;
            }
        }
    }
}