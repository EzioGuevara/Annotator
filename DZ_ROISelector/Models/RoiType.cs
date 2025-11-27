using System.Drawing;

namespace DZ_ROISelector.Models
{
    /// <summary>
    /// ROI 框类型定义
    /// </summary>
    public class RoiType
    {
        /// <summary>
        /// 类型名称（如：定位、找边、搜索区域、缺陷）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 显示颜色
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 最大允许数量（-1 表示不限制）
        /// </summary>
        public int MaxCount { get; set; }

        /// <summary>
        /// 当前数量
        /// </summary>
        public int CurrentCount { get; set; }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible { get; set; }

        public RoiType()
        {
            MaxCount = -1;
            CurrentCount = 0;
            IsVisible = true;
        }

        public RoiType(string name, Color color, string description = "")
        {
            Name = name;
            Color = color;
            Description = description;
            MaxCount = -1;
            CurrentCount = 0;
            IsVisible = true;
        }

        /// <summary>
        /// 检查是否可以创建新框
        /// </summary>
        public bool CanCreate()
        {
            return MaxCount == -1 || CurrentCount < MaxCount;
        }
    }
}
