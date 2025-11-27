using System;
using DZ_ROISelector.Models;

namespace DZ_ROISelector.EventArgs
{
    /// <summary>
    /// ROI 创建完成事件参数
    /// </summary>
    public class RoiCreatedEventArgs : System.EventArgs
    {
        /// <summary>
        /// ROI 数据
        /// </summary>
        public RoiData RoiData { get; set; }

        public RoiCreatedEventArgs(RoiData roiData)
        {
            RoiData = roiData;
        }
    }
}
