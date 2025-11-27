using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DZ_ROISelector.Utils
{
    /// <summary>
    /// 图像缓存类，用于性能优化
    /// </summary>
    public class ImageCache : IDisposable
    {
        private Image _originalImage;
        private Bitmap _scaledImage;
        private Size _lastScaledSize;

        /// <summary>
        /// 原始图像
        /// </summary>
        public Image OriginalImage
        {
            get { return _originalImage; }
            set
            {
                if (_originalImage != value)
                {
                    _originalImage = value;
                    InvalidateCache();
                }
            }
        }

        /// <summary>
        /// 获取缩放后的图像
        /// </summary>
        public Bitmap GetScaledImage(Size targetSize)
        {
            if (_originalImage == null)
                return null;

            // 如果尺寸未变化且缓存存在，直接返回缓存
            if (_scaledImage != null && _lastScaledSize == targetSize)
                return _scaledImage;

            // 释放旧缓存
            if (_scaledImage != null)
            {
                _scaledImage.Dispose();
                _scaledImage = null;
            }

            // 创建新的缩放图像
            if (targetSize.Width > 0 && targetSize.Height > 0)
            {
                _scaledImage = new Bitmap(targetSize.Width, targetSize.Height);
                using (Graphics g = Graphics.FromImage(_scaledImage))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawImage(_originalImage, 0, 0, targetSize.Width, targetSize.Height);
                }
                _lastScaledSize = targetSize;
            }

            return _scaledImage;
        }

        /// <summary>
        /// 使缓存失效
        /// </summary>
        public void InvalidateCache()
        {
            if (_scaledImage != null)
            {
                _scaledImage.Dispose();
                _scaledImage = null;
            }
            _lastScaledSize = Size.Empty;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_scaledImage != null)
            {
                _scaledImage.Dispose();
                _scaledImage = null;
            }
        }
    }
}
