using System;
using System.Drawing;
using System.Windows.Forms;
using DZ_ROISelector.Enums;

namespace DZ_ROISelector.Models
{
    /// <summary>
    /// ROI 矩形类，包含绘制和交互逻辑
    /// </summary>
    public class RoiRectangle
    {
        private const float HandleSize = 8f;
        private static readonly AnchorType[] Anchors = 
        {
            AnchorType.TopLeft, AnchorType.TopCenter, AnchorType.TopRight,
            AnchorType.MiddleRight, AnchorType.BottomRight, AnchorType.BottomCenter,
            AnchorType.BottomLeft, AnchorType.MiddleLeft
        };

        /// <summary>
        /// X 坐标
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y 坐标
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 显示颜色
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        public object Tag { get; set; }

        public RoiRectangle()
        {
            IsVisible = true;
            IsSelected = false;
        }

        public RoiRectangle(float x, float y, float width, float height, string typeName, Color color)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            TypeName = typeName;
            Color = color;
            IsVisible = true;
            IsSelected = false;
        }

        /// <summary>
        /// 转换为 RectangleF
        /// </summary>
        public RectangleF ToRectangleF()
        {
            return new RectangleF(X, Y, Width, Height);
        }

        /// <summary>
        /// 转换为 Rectangle
        /// </summary>
        public Rectangle ToRectangle()
        {
            return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
        }

        /// <summary>
        /// 检查点是否在矩形内
        /// </summary>
        public bool Contains(int x, int y)
        {
            return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        }

        /// <summary>
        /// 检查点是否在矩形内
        /// </summary>
        public bool Contains(Point p)
        {
            return Contains(p.X, p.Y);
        }

        /// <summary>
        /// 获取锚点矩形
        /// </summary>
        private RectangleF GetAnchorRectangle(AnchorType type)
        {
            float anchorX = 0, anchorY = 0;

            switch (type)
            {
                case AnchorType.TopLeft:
                    anchorX = X;
                    anchorY = Y;
                    break;
                case AnchorType.TopCenter:
                    anchorX = X + Width / 2;
                    anchorY = Y;
                    break;
                case AnchorType.TopRight:
                    anchorX = X + Width;
                    anchorY = Y;
                    break;
                case AnchorType.MiddleRight:
                    anchorX = X + Width;
                    anchorY = Y + Height / 2;
                    break;
                case AnchorType.BottomRight:
                    anchorX = X + Width;
                    anchorY = Y + Height;
                    break;
                case AnchorType.BottomCenter:
                    anchorX = X + Width / 2;
                    anchorY = Y + Height;
                    break;
                case AnchorType.BottomLeft:
                    anchorX = X;
                    anchorY = Y + Height;
                    break;
                case AnchorType.MiddleLeft:
                    anchorX = X;
                    anchorY = Y + Height / 2;
                    break;
            }

            return new RectangleF(anchorX - HandleSize / 2, anchorY - HandleSize / 2, HandleSize, HandleSize);
        }

        /// <summary>
        /// 获取鼠标命中的锚点
        /// </summary>
        public AnchorType GetHitAnchor(int x, int y)
        {
            foreach (AnchorType anchor in Anchors)
            {
                RectangleF rect = GetAnchorRectangle(anchor);
                if (rect.Contains(x, y))
                    return anchor;
            }
            return AnchorType.None;
        }

        /// <summary>
        /// 根据锚点类型获取光标
        /// </summary>
        public static Cursor GetCursor(AnchorType anchor)
        {
            switch (anchor)
            {
                case AnchorType.TopLeft:
                case AnchorType.BottomRight:
                    return Cursors.SizeNWSE;
                case AnchorType.TopRight:
                case AnchorType.BottomLeft:
                    return Cursors.SizeNESW;
                case AnchorType.TopCenter:
                case AnchorType.BottomCenter:
                    return Cursors.SizeNS;
                case AnchorType.MiddleLeft:
                case AnchorType.MiddleRight:
                    return Cursors.SizeWE;
                case AnchorType.None:
                    return Cursors.SizeAll;
                default:
                    return Cursors.Default;
            }
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        public void Draw(Graphics g, bool showHandles, bool showLabel)
        {
            if (!IsVisible)
                return;

            // 绘制矩形边框
            float lineWidth = IsSelected ? 2f : 1f;
            using (Pen pen = new Pen(Color, lineWidth))
            {
                g.DrawRectangle(pen, X, Y, Width, Height);
            }

            // 绘制半透明填充（选中时）
            if (IsSelected)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, Color)))
                {
                    g.FillRectangle(brush, X, Y, Width, Height);
                }
            }

            // 绘制锚点
            if (showHandles && IsSelected)
            {
                using (SolidBrush brush = new SolidBrush(Color))
                using (Pen pen = new Pen(Color.White, 1f))
                {
                    foreach (AnchorType anchor in Anchors)
                    {
                        RectangleF rect = GetAnchorRectangle(anchor);
                        g.FillRectangle(brush, rect);
                        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                    }
                }
            }

            // 绘制标签
            if (showLabel && !string.IsNullOrEmpty(TypeName))
            {
                using (Font font = new Font("微软雅黑", 9f))
                using (SolidBrush textBrush = new SolidBrush(Color))
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
                {
                    SizeF textSize = g.MeasureString(TypeName, font);
                    float labelX = X;
                    float labelY = Y - textSize.Height - 2;
                    
                    // 如果标签超出上边界，显示在框内
                    if (labelY < 0)
                        labelY = Y + 2;

                    RectangleF labelRect = new RectangleF(labelX, labelY, textSize.Width + 4, textSize.Height);
                    g.FillRectangle(bgBrush, labelRect);
                    g.DrawString(TypeName, font, textBrush, labelX + 2, labelY);
                }
            }
        }

        /// <summary>
        /// 调整矩形（根据锚点和偏移量）
        /// </summary>
        public void Resize(AnchorType anchor, float dx, float dy)
        {
            switch (anchor)
            {
                case AnchorType.None:
                    // 移动整个矩形
                    X += dx;
                    Y += dy;
                    break;

                case AnchorType.TopLeft:
                    X += dx;
                    Y += dy;
                    Width -= dx;
                    Height -= dy;
                    break;

                case AnchorType.TopCenter:
                    Y += dy;
                    Height -= dy;
                    break;

                case AnchorType.TopRight:
                    Y += dy;
                    Width += dx;
                    Height -= dy;
                    break;

                case AnchorType.MiddleRight:
                    Width += dx;
                    break;

                case AnchorType.BottomRight:
                    Width += dx;
                    Height += dy;
                    break;

                case AnchorType.BottomCenter:
                    Height += dy;
                    break;

                case AnchorType.BottomLeft:
                    X += dx;
                    Width -= dx;
                    Height += dy;
                    break;

                case AnchorType.MiddleLeft:
                    X += dx;
                    Width -= dx;
                    break;
            }

            // 确保宽高为正
            NormalizeRect();
        }

        /// <summary>
        /// 规范化矩形（确保宽高为正）
        /// </summary>
        public void NormalizeRect()
        {
            if (Width < 0)
            {
                X += Width;
                Width = -Width;
            }
            if (Height < 0)
            {
                Y += Height;
                Height = -Height;
            }
        }

        /// <summary>
        /// 限制矩形在边界内
        /// </summary>
        public void ClampToBounds(float minX, float minY, float maxX, float maxY)
        {
            if (X < minX) X = minX;
            if (Y < minY) Y = minY;
            if (X + Width > maxX) Width = maxX - X;
            if (Y + Height > maxY) Height = maxY - Y;
        }
    }
}
