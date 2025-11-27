using System;
using DZ_ROISelector.Models;

namespace DZ_ROISelector.EventArgs
{
    /// <summary>
    /// ROI 删除事件参数
    /// </summary>
    public class RoiDeletedEventArgs : System.EventArgs
    {
        /// <summary>
        /// ROI 数据
        /// </summary>
        public RoiData RoiData { get; set; }

        public RoiDeletedEventArgs(RoiData roiData)
        {
            RoiData = roiData;
        }
    }
}
