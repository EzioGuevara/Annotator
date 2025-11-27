using System;
using DZ_ROISelector.Models;

namespace DZ_ROISelector.EventArgs
{
    /// <summary>
    /// ROI 修改事件参数
    /// </summary>
    public class RoiModifiedEventArgs : System.EventArgs
    {
        /// <summary>
        /// ROI 数据
        /// </summary>
        public RoiData RoiData { get; set; }

        public RoiModifiedEventArgs(RoiData roiData)
        {
            RoiData = roiData;
        }
    }
}
