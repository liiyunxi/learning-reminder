using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 新版本信息。
    /// </summary>
    public sealed class UpdateInfo
    {
        /// <summary>新版本号（如 0.3.0，已去掉 v 前缀）</summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>发行版页面地址</summary>
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新检查：查询 GitHub 最新发行版并与当前版本比较。
    /// 只在"发现有新版本"时给出提示与下载引导，不自动下载替换
    /// （安装包未做代码签名，静默替换风险高，保持用户可控）。
    /// </summary>
    public static class UpdateService
    {
        private static readonly HttpClient Client = CreateClient();

        /// <summary>当前应用版本（如 0.3.0）。</summary>
        public static string CurrentVersion
        {
            get
            {
                Version? version = typeof(UpdateService).Assembly.GetName().Version;
                return version == null
                    ? "0.0.0"
                    : string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            }
        }

        /// <summary>查询最新发行版；网络失败返回 null。</summary>
        public static async Task<UpdateInfo?> CheckAsync()
        {
            try
            {
                string json = await Client.GetStringAsync(AppConstants.UpdateLatestReleaseApi).ConfigureAwait(false);
                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;

                string tag = root.TryGetProperty("tag_name", out JsonElement tagElement)
                    ? tagElement.GetString() ?? string.Empty
                    : string.Empty;
                string url = root.TryGetProperty("html_url", out JsonElement urlElement)
                    ? urlElement.GetString() ?? AppConstants.DownloadPageUrl
                    : AppConstants.DownloadPageUrl;

                return new UpdateInfo
                {
                    Version = tag.TrimStart('v', 'V'),
                    Url = url
                };
            }
            catch (Exception ex)
            {
                FileLogger.Error("检查新版本失败", ex);
                return null;
            }
        }

        /// <summary>latest 比 current 新时返回 true。</summary>
        public static bool IsNewer(string latest, string current)
        {
            return Version.TryParse(Normalize(latest), out Version? latestVersion)
                && Version.TryParse(Normalize(current), out Version? currentVersion)
                && latestVersion > currentVersion;
        }

        private static string Normalize(string value)
        {
            // Version.Parse 需要至少两段（如 0.3），单段版本号补一段
            return value.Contains('.') ? value : value + ".0";
        }

        private static HttpClient CreateClient()
        {
            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("LearningReminder/" + CurrentVersion);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            return client;
        }
    }
}