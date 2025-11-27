using System.Drawing;

namespace DZ_ROISelector.Models
{
    /// <summary>
    /// ROI 数据输出类
    /// </summary>
    public class RoiData
    {
        /// <summary>
        /// 框索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 显示颜色
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// 原始图像坐标系中的矩形
        /// </summary>
        public Rectangle OriginalRect { get; set; }

        /// <summary>
        /// 显示坐标系中的矩形
        /// </summary>
        public Rectangle DisplayRect { get; set; }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// 附加数据（用户自定义）
        /// </summary>
        public object Tag { get; set; }

        public RoiData()
        {
            IsVisible = true;
        }

        public RoiData(int index, string typeName, Color color, Rectangle originalRect, Rectangle displayRect)
        {
            Index = index;
            TypeName = typeName;
            Color = color;
            OriginalRect = originalRect;
            DisplayRect = displayRect;
            IsVisible = true;
        }
    }
}
