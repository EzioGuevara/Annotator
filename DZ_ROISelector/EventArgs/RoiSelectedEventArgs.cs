using System;
using DZ_ROISelector.Models;

namespace DZ_ROISelector.EventArgs
{
    /// <summary>
    /// ROI 选中事件参数
    /// </summary>
    public class RoiSelectedEventArgs : System.EventArgs
    {
        /// <summary>
        /// ROI 数据
        /// </summary>
        public RoiData RoiData { get; set; }

        public RoiSelectedEventArgs(RoiData roiData)
        {
            RoiData = roiData;
        }
    }
}
