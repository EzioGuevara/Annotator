using System.Drawing;
using System.Drawing.Drawing2D;

namespace DZ_ROISelector.Utils
{
    /// <summary>
    /// 绘制辅助类
    /// </summary>
    public static class DrawingHelper
    {
        /// <summary>
        /// 创建高质量的 Graphics 对象
        /// </summary>
        public static void SetHighQuality(Graphics g)
        {
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        /// <summary>
        /// 创建快速绘制的 Graphics 对象（用于显示模式）
        /// </summary>
        public static void SetFastRendering(Graphics g)
        {
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.Low;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
        }

        /// <summary>
        /// 绘制带阴影的文本
        /// </summary>
        public static void DrawTextWithShadow(Graphics g, string text, Font font, Brush brush, PointF point)
        {
            // 绘制阴影
            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(128, Color.Black)))
            {
                g.DrawString(text, font, shadowBrush, point.X + 1, point.Y + 1);
            }
            // 绘制文本
            g.DrawString(text, font, brush, point);
        }

        /// <summary>
        /// 绘制带背景的文本
        /// </summary>
        public static void DrawTextWithBackground(Graphics g, string text, Font font, Brush textBrush, Brush bgBrush, PointF point, int padding = 2)
        {
            SizeF textSize = g.MeasureString(text, font);
            RectangleF bgRect = new RectangleF(point.X, point.Y, textSize.Width + padding * 2, textSize.Height);
            g.FillRectangle(bgBrush, bgRect);
            g.DrawString(text, font, textBrush, point.X + padding, point.Y);
        }

        /// <summary>
        /// 裁剪图像区域
        /// </summary>
        public static Bitmap CropImage(Image source, Rectangle cropRect)
        {
            if (source == null)
                return null;

            // 确保裁剪区域在图像范围内
            cropRect.Intersect(new Rectangle(0, 0, source.Width, source.Height));

            if (cropRect.Width <= 0 || cropRect.Height <= 0)
                return null;

            Bitmap croppedImage = new Bitmap(cropRect.Width, cropRect.Height);
            using (Graphics g = Graphics.FromImage(croppedImage))
            {
                g.DrawImage(source, 
                    new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                    cropRect,
                    GraphicsUnit.Pixel);
            }

            return croppedImage;
        }

        /// <summary>
        /// 创建半透明颜色
        /// </summary>
        public static Color GetTransparentColor(Color color, int alpha)
        {
            return Color.FromArgb(alpha, color);
        }
    }
}
