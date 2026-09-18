using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace LearningReminder.Services
{
    /// <summary>
    /// 应用标识与托盘图标：圆角底 + 摊开的书 + 完成对勾
    /// （学习为宗旨，自律为目标）。运行时绘制，避免携带图片资源。
    /// </summary>
    public static class TrayIconFactory
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr handle);

        private const string StartColorHex = "#4F46E5";
        private const string EndColorHex = "#7C3AED";
        private const string PageColorHex = "#FFFFFF";
        private const string PageShadeHex = "#D9DEF0";
        private const string BadgeColorHex = "#10B981";

        // 图标随进程常驻，只创建一次，避免每次刷新都新建 GDI 对象
        private static Icon? _idleIcon;
        private static Icon? _pendingIcon;
        private static Icon? _doneIcon;

        /// <summary>常规状态</summary>
        public static Icon IdleIcon => _idleIcon ??= Create("#4F46E5", "#7C3AED");

        /// <summary>有待确认项（橙色系）</summary>
        public static Icon PendingIcon => _pendingIcon ??= Create("#F59E0B", "#EA580C");

        /// <summary>今日任务全部完成（绿色系）</summary>
        public static Icon DoneIcon => _doneIcon ??= Create("#10B981", "#059669");

        /// <summary>
        /// 绘制图标：小尺寸用简化画法保证在 16px 下仍清晰。
        /// </summary>
        private static Icon Create(string startHex, string endHex)
        {
            const int size = 32;
            using Bitmap bitmap = new Bitmap(size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);
                // 托盘通常按 16-24px 渲染，用简化画法更清晰
                DrawLogo(graphics, size, startHex, endHex, compact: true);
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
        /// 在指定画布上按 32×32 的设计稿等比绘制（供托盘图标与图标文件共用）。
        /// </summary>
        public static void DrawLogo(Graphics graphics, int size, string startHex, string endHex, bool compact)
        {
            // 设计稿坐标系为 32×32，这里统一缩放到目标尺寸
            float scale = size / 32f;

            using (GraphicsPath background = CreateRoundedRect(
                       new RectangleF(0, 0, size, size),
                       7 * scale))
            using (LinearGradientBrush brush = new LinearGradientBrush(
                       new RectangleF(0, 0, size, size),
                       ColorTranslator.FromHtml(startHex),
                       ColorTranslator.FromHtml(endHex),
                       45f))
            {
                graphics.FillPath(brush, background);
            }

            Color pageColor = ColorTranslator.FromHtml(PageColorHex);
            Color shadeColor = ColorTranslator.FromHtml(PageShadeHex);
            Color badgeColor = ColorTranslator.FromHtml(BadgeColorHex);

            // 摊开的书：左右两页
            using (SolidBrush white = new SolidBrush(pageColor))
            using (SolidBrush shade = new SolidBrush(shadeColor))
            {
                graphics.FillPolygon(white, ScalePoints(new[]
                {
                    new PointF(4f, 8.2f), new PointF(11f, 10.6f), new PointF(11f, 19.6f), new PointF(4f, 17.2f)
                }, scale));
                graphics.FillPolygon(shade, ScalePoints(new[]
                {
                    new PointF(13f, 10.6f), new PointF(20f, 8.2f), new PointF(20f, 17.2f), new PointF(13f, 19.6f)
                }, scale));
            }

            if (!compact)
            {
                // 完成徽章：绿环 + 白底 + 对勾
                using SolidBrush badge = new SolidBrush(badgeColor);
                graphics.FillEllipse(badge, ToRect(18.6f, 18.6f, 4.6f, scale));

                using SolidBrush white = new SolidBrush(pageColor);
                graphics.FillEllipse(white, ToRect(18.6f, 18.6f, 3.4f, scale));

                using Pen mark = new Pen(badgeColor, Math.Max(1f, 1.7f * scale))
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };
                graphics.DrawLines(mark, ScalePoints(new[]
                {
                    new PointF(16.6f, 18.5f), new PointF(18f, 19.9f), new PointF(20.7f, 16.9f)
                }, scale));
            }
            else
            {
                // 小尺寸：把对勾画粗，直接压在书页上，保证 16px 也能看出"完成"
                using Pen mark = new Pen(badgeColor, Math.Max(1.7f, 3f * scale))
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };
                graphics.DrawLines(mark, ScalePoints(new[]
                {
                    new PointF(12.6f, 14.4f), new PointF(16f, 17.8f), new PointF(22f, 9.6f)
                }, scale));
            }
        }

        private static PointF[] ScalePoints(PointF[] points, float scale)
        {
            PointF[] result = new PointF[points.Length];
            for (int index = 0; index < points.Length; index++)
            {
                result[index] = new PointF(points[index].X * scale, points[index].Y * scale);
            }

            return result;
        }

        private static RectangleF ToRect(float centerX, float centerY, float radius, float scale)
        {
            float diameter = radius * 2 * scale;
            return new RectangleF((centerX - radius) * scale, (centerY - radius) * scale, diameter, diameter);
        }

        /// <summary>构造圆角矩形路径。</summary>
        private static GraphicsPath CreateRoundedRect(RectangleF bounds, float radius)
        {
            float diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}