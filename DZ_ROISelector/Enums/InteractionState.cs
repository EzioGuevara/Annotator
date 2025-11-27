namespace DZ_ROISelector.Enums
{
    /// <summary>
    /// 鼠标交互状态
    /// </summary>
    public enum InteractionState
    {
        /// <summary>
        /// 无操作
        /// </summary>
        None,

        /// <summary>
        /// 正在创建新框
        /// </summary>
        Creating,

        /// <summary>
        /// 选中框（悬停）
        /// </summary>
        Hovering,

        /// <summary>
        /// 正在拖动框
        /// </summary>
        Dragging,

        /// <summary>
        /// 正在调整框大小
        /// </summary>
        Resizing
    }
}
