using System.Windows;
using System.Windows.Media;

namespace LearningReminder.Resources
{
    /// <summary>
    /// 取应用主题色（统一从 App.xaml 的资源里取，避免颜色写两遍）。
    /// </summary>
    public static class Palette
    {
        /// <summary>主色</summary>
        public static Brush Primary => Find("Brush.Primary", "#4F46E5");

        /// <summary>主色浅底</summary>
        public static Brush PrimarySoft => Find("Brush.PrimarySoft", "#E4E4FF");

        /// <summary>成功色</summary>
        public static Brush Success => Find("Brush.Success", "#10B981");

        /// <summary>成功浅底</summary>
        public static Brush SuccessSoft => Find("Brush.SuccessSoft", "#D9F7EC");

        /// <summary>提醒色</summary>
        public static Brush Warn => Find("Brush.Warn", "#F59E0B");

        /// <summary>提醒浅底</summary>
        public static Brush WarnSoft => Find("Brush.WarnSoft", "#FDEBC8");

        /// <summary>卡片底色</summary>
        public static Brush Surface => Find("Brush.Surface", "#FFFFFF");

        /// <summary>次要文字色</summary>
        public static Brush Muted => Find("Brush.Muted", "#5B5B73");

        /// <summary>正文色</summary>
        public static Brush Text => Find("Brush.Text", "#1F2033");

        /// <summary>边框色</summary>
        public static Brush Border => Find("Brush.Border", "#1F2033");

        /// <summary>空白格文字色</summary>
        public static Brush Disabled => Find("Brush.Disabled", "#B9B7AC");

        private static Brush Find(string key, string fallback)
        {
            object? resource = Application.Current?.TryFindResource(key);
            if (resource is Brush brush)
            {
                return brush;
            }

            SolidColorBrush created = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fallback));
            created.Freeze();
            return created;
        }
    }
}