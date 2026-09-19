using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace LearningReminder.Services
{
    /// <summary>
    /// 应用标识与托盘图标：像素风"摊开的书 + 绿色底座"。
    /// 用 16×16 像素网格定义形状，按整数倍缩放绘制，保证任意尺寸下边缘锐利。
    /// </summary>
    public static class TrayIconFactory
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr handle);

        /// <summary>16×16 像素网格：'.' 背景、'W' 左页、'S' 右页、'G' 底座。</summary>
        private static readonly string[] LogoGrid =
        {
            "................",
            "................",
            "................",
            "..WWWWW..SSSSS..",
            "..WWWWW..SSSSS..",
            "..WWWWW..SSSSS..",
            "..WWWWW..SSSSS..",
            "..WWWWW..SSSSS..",
            "..WWWWW..SSSSS..",
            "..WWWWW..SSSSS..",
            "...WWWW..SSSS...",
            "................",
            ".GGGGGGGGGGGGGG.",
            "................",
            "................",
            "................"
        };

        // 图标随进程常驻，只创建一次，避免每次刷新都新建 GDI 对象
        private static Icon? _idleIcon;
        private static Icon? _pendingIcon;
        private static Icon? _doneIcon;

        /// <summary>常规状态（靛蓝底 + 绿色底座）</summary>
        public static Icon IdleIcon => _idleIcon ??= Create("#4F46E5", "#FFFFFF", "#C7D2FE", "#10B981");

        /// <summary>有待确认项（橙色底）</summary>
        public static Icon PendingIcon => _pendingIcon ??= Create("#F59E0B", "#FFFFFF", "#FFE8C7", "#B45309");

        /// <summary>今日任务全部完成（绿色底 + 白色底座）</summary>
        public static Icon DoneIcon => _doneIcon ??= Create("#10B981", "#FFFFFF", "#C8F5E3", "#FFFFFF");

        /// <summary>按网格绘制图标（32px 供托盘使用）。</summary>
        private static Icon Create(string backgroundHex, string pageHex, string shadeHex, string shelfHex)
        {
            const int size = 32;
            using Bitmap bitmap = new Bitmap(size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                DrawPixelLogo(graphics, size, backgroundHex, pageHex, shadeHex, shelfHex);
            }

            // GetHicon 产生的句柄需要显式释放，因此先克隆再销毁原句柄
            IntPtr handle = bitmap.GetHicon();
            try
            {
                using Icon temporary = Icon.FromHandle(handle);
                return (Icon)temporary.Clone();
            }
            finally
            {
                DestroyIcon(handle);
            }
        }

        /// <summary>
        /// 在指定画布上按 16×16 像素网格绘制 logo（托盘图标与窗口标识共用同一口径）。
        /// </summary>
        public static void DrawPixelLogo(
            Graphics graphics,
            int size,
            string backgroundHex,
            string pageHex,
            string shadeHex,
            string shelfHex)
        {
            Color background = ColorTranslator.FromHtml(backgroundHex);
            Color page = ColorTranslator.FromHtml(pageHex);
            Color shade = ColorTranslator.FromHtml(shadeHex);
            Color shelf = ColorTranslator.FromHtml(shelfHex);

            float cell = size / (float)LogoGrid.Length;
            using SolidBrush brush = new SolidBrush(background);
            for (int row = 0; row < LogoGrid.Length; row++)
            {
                string line = LogoGrid[row];
                for (int column = 0; column < line.Length; column++)
                {
                    brush.Color = line[column] switch
                    {
                        'W' => page,
                        'S' => shade,
                        'G' => shelf,
                        _ => background
                    };

                    int x = (int)Math.Round(column * cell);
                    int y = (int)Math.Round(row * cell);
                    int width = (int)Math.Round((column + 1) * cell) - x;
                    int height = (int)Math.Round((row + 1) * cell) - y;
                    graphics.FillRectangle(brush, x, y, width, height);
                }
            }
        }
    }
}