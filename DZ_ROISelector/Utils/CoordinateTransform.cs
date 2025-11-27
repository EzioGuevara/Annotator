using System;
using System.Drawing;

namespace DZ_ROISelector.Utils
{
    /// <summary>
    /// 坐标转换工具类
    /// </summary>
    public class CoordinateTransform
    {
        /// <summary>
        /// 缩放比例
        /// </summary>
        public float Scale { get; private set; }

        /// <summary>
        /// X 偏移量
        /// </summary>
        public float OffsetX { get; private set; }

        /// <summary>
        /// Y 偏移量
        /// </summary>
        public float OffsetY { get; private set; }

        /// <summary>
        /// 原始图像尺寸
        /// </summary>
        public Size OriginalSize { get; private set; }

        /// <summary>
        /// 显示区域尺寸
        /// </summary>
        public Size DisplaySize { get; private set; }

        public CoordinateTransform()
        {
            Scale = 1.0f;
            OffsetX = 0;
            OffsetY = 0;
        }

        /// <summary>
        /// 计算转换参数
        /// </summary>
        public void Calculate(Size originalSize, Size displaySize)
        {
            OriginalSize = originalSize;
            DisplaySize = displaySize;

            if (originalSize.Width == 0 || originalSize.Height == 0)
            {
                Scale = 1.0f;
                OffsetX = 0;
                OffsetY = 0;
                return;
            }

            // 计算缩放比例（保持宽高比）
            float scaleX = (float)displaySize.Width / originalSize.Width;
            float scaleY = (float)displaySize.Height / originalSize.Height;
            Scale = Math.Min(scaleX, scaleY);

            // 计算居中偏移
            float scaledWidth = originalSize.Width * Scale;
            float scaledHeight = originalSize.Height * Scale;
            OffsetX = (displaySize.Width - scaledWidth) / 2;
            OffsetY = (displaySize.Height - scaledHeight) / 2;
        }

        /// <summary>
        /// 原始坐标转显示坐标
        /// </summary>
        public PointF OriginalToDisplay(PointF point)
        {
            return new PointF(
                point.X * Scale + OffsetX,
                point.Y * Scale + OffsetY
            );
        }

        /// <summary>
        /// 原始坐标转显示坐标
        /// </summary>
        public Point OriginalToDisplay(Point point)
        {
            return new Point(
                (int)(point.X * Scale + OffsetX),
                (int)(point.Y * Scale + OffsetY)
            );
        }

        /// <summary>
        /// 原始矩形转显示矩形
        /// </summary>
        public RectangleF OriginalToDisplay(RectangleF rect)
        {
            return new RectangleF(
                rect.X * Scale + OffsetX,
                rect.Y * Scale + OffsetY,
                rect.Width * Scale,
                rect.Height * Scale
            );
        }

        /// <summary>
        /// 原始矩形转显示矩形
        /// </summary>
        public Rectangle OriginalToDisplay(Rectangle rect)
        {
            return new Rectangle(
                (int)(rect.X * Scale + OffsetX),
                (int)(rect.Y * Scale + OffsetY),
                (int)(rect.Width * Scale),
                (int)(rect.Height * Scale)
            );
        }

        /// <summary>
        /// 显示坐标转原始坐标
        /// </summary>
        public PointF DisplayToOriginal(PointF point)
        {
            return new PointF(
                (point.X - OffsetX) / Scale,
                (point.Y - OffsetY) / Scale
            );
        }

        /// <summary>
        /// 显示坐标转原始坐标
        /// </summary>
        public Point DisplayToOriginal(Point point)
        {
            return new Point(
                (int)((point.X - OffsetX) / Scale),
                (int)((point.Y - OffsetY) / Scale)
            );
        }

        /// <summary>
        /// 显示矩形转原始矩形
        /// </summary>
        public RectangleF DisplayToOriginal(RectangleF rect)
        {
            return new RectangleF(
                (rect.X - OffsetX) / Scale,
                (rect.Y - OffsetY) / Scale,
                rect.Width / Scale,
                rect.Height / Scale
            );
        }

        /// <summary>
        /// 显示矩形转原始矩形
        /// </summary>
        public Rectangle DisplayToOriginal(Rectangle rect)
        {
            return new Rectangle(
                (int)((rect.X - OffsetX) / Scale),
                (int)((rect.Y - OffsetY) / Scale),
                (int)(rect.Width / Scale),
                (int)(rect.Height / Scale)
            );
        }

        /// <summary>
        /// 获取缩放后的图像尺寸
        /// </summary>
        public Size GetScaledImageSize()
        {
            return new Size(
                (int)(OriginalSize.Width * Scale),
                (int)(OriginalSize.Height * Scale)
            );
        }

        /// <summary>
        /// 获取图像在显示区域的矩形
        /// </summary>
        public Rectangle GetImageDisplayRect()
        {
            Size scaledSize = GetScaledImageSize();
            return new Rectangle(
                (int)OffsetX,
                (int)OffsetY,
                scaledSize.Width,
                scaledSize.Height
            );
        }
    }
}
