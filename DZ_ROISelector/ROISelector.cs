using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DZ_ROISelector.Enums;
using DZ_ROISelector.Models;
using DZ_ROISelector.Utils;

namespace DZ_ROISelector
{
    /// <summary>
    /// ROI 选择器用户控件
    /// </summary>
    public partial class ROISelector : UserControl
    {
        #region 私有字段

        private readonly Dictionary<string, RoiType> _roiTypes;
        private readonly List<RoiRectangle> _rectangles;
        private readonly CoordinateTransform _transform;
        private readonly ImageCache _imageCache;

        private InteractionState _currentState;
        private RoiRectangle _selectedRectangle;
        private RoiRectangle _creatingRectangle;
        private AnchorType _currentAnchor;
        private Point _lastMousePosition;
        private string _creatingTypeName;
        private bool _isUpdating;

        #endregion

        #region 公共属性

        /// <summary>
        /// 控件工作模式
        /// </summary>
        [Category("ROI设置")]
        [Description("控件工作模式：Configuration(配置模式) 或 Display(显示模式)")]
        [DefaultValue(ControlMode.Configuration)]
        public ControlMode Mode { get; set; }

        /// <summary>
        /// 是否显示标签
        /// </summary>
        [Category("ROI设置")]
        [Description("是否显示ROI类型标签")]
        [DefaultValue(true)]
        public bool ShowLabels { get; set; }

        /// <summary>
        /// 是否显示调整锚点
        /// </summary>
        [Category("ROI设置")]
        [Description("是否显示调整锚点（仅配置模式有效）")]
        [DefaultValue(true)]
        public bool ShowHandles { get; set; }

        /// <summary>
        /// 是否启用右键菜单
        /// </summary>
        [Category("ROI设置")]
        [Description("是否启用右键菜单")]
        [DefaultValue(false)]
        public bool EnableContextMenu { get; set; }

        /// <summary>
        /// 最小ROI尺寸
        /// </summary>
        [Category("ROI设置")]
        [Description("最小ROI尺寸（像素）")]
        [DefaultValue(10)]
        public int MinRoiSize { get; set; }

        /// <summary>
        /// 原始图像
        /// </summary>
        [Browsable(false)]
        public Image SourceImage
        {
            get { return _imageCache.OriginalImage; }
            set
            {
                _imageCache.OriginalImage = value;
                UpdateTransform();
                Invalidate();
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// ROI 创建完成事件
        /// </summary>
        [Category("ROI事件")]
        [Description("ROI创建完成时触发")]
        public event EventHandler<DZ_ROISelector.EventArgs.RoiCreatedEventArgs> RoiCreated;

        /// <summary>
        /// ROI 修改事件
        /// </summary>
        [Category("ROI事件")]
        [Description("ROI被修改时触发")]
        public event EventHandler<DZ_ROISelector.EventArgs.RoiModifiedEventArgs> RoiModified;

        /// <summary>
        /// ROI 选中事件
        /// </summary>
        [Category("ROI事件")]
        [Description("ROI被选中时触发")]
        public event EventHandler<DZ_ROISelector.EventArgs.RoiSelectedEventArgs> RoiSelected;

        /// <summary>
        /// ROI 删除事件
        /// </summary>
        [Category("ROI事件")]
        [Description("ROI被删除时触发")]
        public event EventHandler<DZ_ROISelector.EventArgs.RoiDeletedEventArgs> RoiDeleted;

        #endregion

        #region 构造函数

        public ROISelector()
        {
            // 先初始化关键字段，避免 InitializeComponent 调用期间访问尚未初始化的依赖项（例如 OnResize -> UpdateTransform -> SourceImage）
            _roiTypes = new Dictionary<string, RoiType>();
            _rectangles = new List<RoiRectangle>();
            _transform = new CoordinateTransform();
            _imageCache = new ImageCache();

            InitializeComponent();

            Mode = ControlMode.Configuration;
            ShowLabels = true;
            ShowHandles = true;
            EnableContextMenu = false;
            MinRoiSize = 10;

            _currentState = InteractionState.None;

            // 启用双缓冲
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }

        #endregion

        #region 公共方法 - 类型管理

        /// <summary>
        /// 注册ROI类型
        /// </summary>
        public void RegisterRoiType(string typeName, Color color, string description = "")
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException("类型名称不能为空", nameof(typeName));

            if (_roiTypes.ContainsKey(typeName))
            {
                _roiTypes[typeName].Color = color;
                _roiTypes[typeName].Description = description;
            }
            else
            {
                _roiTypes[typeName] = new RoiType(typeName, color, description);
            }
        }

        /// <summary>
        /// 设置类型的最大数量
        /// </summary>
        public void SetMaxCount(string typeName, int maxCount)
        {
            if (_roiTypes.ContainsKey(typeName))
            {
                _roiTypes[typeName].MaxCount = maxCount;
            }
        }

        /// <summary>
        /// 设置类型可见性
        /// </summary>
        public void SetTypeVisible(string typeName, bool visible)
        {
            if (_roiTypes.ContainsKey(typeName))
            {
                _roiTypes[typeName].IsVisible = visible;
                foreach (var rect in _rectangles.Where(r => r.TypeName == typeName))
                {
                    rect.IsVisible = visible;
                }
                Invalidate();
            }
        }

        #endregion

        #region 公共方法 - ROI操作

        /// <summary>
        /// 开始创建指定类型的ROI
        /// </summary>
        public void StartCreating(string typeName)
        {
            if (!_roiTypes.ContainsKey(typeName))
                throw new ArgumentException($"未注册的ROI类型: {typeName}", nameof(typeName));

            var roiType = _roiTypes[typeName];
            
            // 检查是否可以创建
            if (!roiType.CanCreate())
            {
                MessageBox.Show($"'{typeName}' 类型的ROI已达到最大数量限制 ({roiType.MaxCount})",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 如果达到最大数量，删除旧的
            if (roiType.MaxCount > 0)
            {
                var existingRois = _rectangles.Where(r => r.TypeName == typeName).ToList();
                while (existingRois.Count >= roiType.MaxCount)
                {
                    var toRemove = existingRois[0];
                    _rectangles.Remove(toRemove);
                    roiType.CurrentCount--;
                    existingRois.RemoveAt(0);
                }
            }

            _creatingTypeName = typeName;
            _currentState = InteractionState.Creating;
            Cursor = Cursors.Cross;
        }

        /// <summary>
        /// 取消创建
        /// </summary>
        public void CancelCreating()
        {
            _creatingTypeName = null;
            _creatingRectangle = null;
            _currentState = InteractionState.None;
            Cursor = Cursors.Default;
            Invalidate();
        }

        /// <summary>
        /// 添加ROI（代码方式）
        /// </summary>
        public void AddRoi(string typeName, Rectangle rect)
        {
            if (!_roiTypes.ContainsKey(typeName))
                throw new ArgumentException($"未注册的ROI类型: {typeName}", nameof(typeName));

            var roiType = _roiTypes[typeName];
            if (!roiType.CanCreate())
                return;

            // 转换为显示坐标
            RectangleF displayRect = _transform.OriginalToDisplay(rect);
            
            var roiRect = new RoiRectangle(
                displayRect.X, displayRect.Y, displayRect.Width, displayRect.Height,
                typeName, roiType.Color);

            _rectangles.Add(roiRect);
            roiType.CurrentCount++;

            OnRoiCreated(roiRect);
            Invalidate();
        }

        /// <summary>
        /// 删除ROI
        /// </summary>
        public void RemoveRoi(int index)
        {
            if (index >= 0 && index < _rectangles.Count)
            {
                var rect = _rectangles[index];
                _rectangles.RemoveAt(index);

                if (_roiTypes.ContainsKey(rect.TypeName))
                {
                    _roiTypes[rect.TypeName].CurrentCount--;
                }

                OnRoiDeleted(rect);
                Invalidate();
            }
        }

        /// <summary>
        /// 清空ROI
        /// </summary>
        public void ClearRois(string typeName = null)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                _rectangles.Clear();
                foreach (var type in _roiTypes.Values)
                {
                    type.CurrentCount = 0;
                }
            }
            else
            {
                _rectangles.RemoveAll(r => r.TypeName == typeName);
                if (_roiTypes.ContainsKey(typeName))
                {
                    _roiTypes[typeName].CurrentCount = 0;
                }
            }
            Invalidate();
        }

        /// <summary>
        /// 更新ROI
        /// </summary>
        public void UpdateRoi(int index, Rectangle rect)
        {
            if (index >= 0 && index < _rectangles.Count)
            {
                RectangleF displayRect = _transform.OriginalToDisplay(rect);
                var roiRect = _rectangles[index];
                roiRect.X = displayRect.X;
                roiRect.Y = displayRect.Y;
                roiRect.Width = displayRect.Width;
                roiRect.Height = displayRect.Height;

                OnRoiModified(roiRect);
                Invalidate();
            }
        }

        /// <summary>
        /// 高亮显示ROI
        /// </summary>
        public void HighlightRoi(int index, bool highlight)
        {
            if (index >= 0 && index < _rectangles.Count)
            {
                _rectangles[index].IsSelected = highlight;
                Invalidate();
            }
        }

        #endregion

        #region 公共方法 - 数据获取

        /// <summary>
        /// 获取所有ROI数据
        /// </summary>
        public List<RoiData> GetAllRois()
        {
            var result = new List<RoiData>();
            for (int i = 0; i < _rectangles.Count; i++)
            {
                result.Add(CreateRoiData(i, _rectangles[i]));
            }
            return result;
        }

        /// <summary>
        /// 按类型获取ROI数据
        /// </summary>
        public List<RoiData> GetRoisByType(string typeName)
        {
            var result = new List<RoiData>();
            for (int i = 0; i < _rectangles.Count; i++)
            {
                if (_rectangles[i].TypeName == typeName)
                {
                    result.Add(CreateRoiData(i, _rectangles[i]));
                }
            }
            return result;
        }

        /// <summary>
        /// 获取指定ROI数据
        /// </summary>
        public RoiData GetRoi(int index)
        {
            if (index >= 0 && index < _rectangles.Count)
            {
                return CreateRoiData(index, _rectangles[index]);
            }
            return null;
        }

        /// <summary>
        /// 获取ROI裁剪图像
        /// </summary>
        public Bitmap GetRoiImage(int index)
        {
            if (index < 0 || index >= _rectangles.Count || SourceImage == null)
                return null;

            var rect = _rectangles[index];
            Rectangle originalRect = _transform.DisplayToOriginal(rect.ToRectangle());
            return DrawingHelper.CropImage(SourceImage, originalRect);
        }

        /// <summary>
        /// 按类型批量获取ROI图像
        /// </summary>
        public Dictionary<string, List<Bitmap>> GetAllRoiImages()
        {
            var result = new Dictionary<string, List<Bitmap>>();
            
            for (int i = 0; i < _rectangles.Count; i++)
            {
                var rect = _rectangles[i];
                if (!result.ContainsKey(rect.TypeName))
                {
                    result[rect.TypeName] = new List<Bitmap>();
                }
                
                var image = GetRoiImage(i);
                if (image != null)
                {
                    result[rect.TypeName].Add(image);
                }
            }
            
            return result;
        }

        #endregion

        #region 公共方法 - 批量更新

        /// <summary>
        /// 开始批量更新（暂停刷新）
        /// </summary>
        public void BeginUpdate()
        {
            _isUpdating = true;
        }

        /// <summary>
        /// 结束批量更新（恢复刷新）
        /// </summary>
        public void EndUpdate()
        {
            _isUpdating = false;
            Invalidate();
        }

        #endregion

        #region 私有方法

        private RoiData CreateRoiData(int index, RoiRectangle rect)
        {
            Rectangle originalRect = _transform.DisplayToOriginal(rect.ToRectangle());
            return new RoiData(index, rect.TypeName, rect.Color, originalRect, rect.ToRectangle())
            {
                IsVisible = rect.IsVisible,
                Tag = rect.Tag
            };
        }

        private void UpdateTransform()
        {
            if (SourceImage != null)
            {
                _transform.Calculate(SourceImage.Size, ClientSize);
            }
        }

        private RoiRectangle GetRectangleAt(Point point)
        {
            // 从后往前查找（后添加的在上层）
            for (int i = _rectangles.Count - 1; i >= 0; i--)
            {
                if (_rectangles[i].IsVisible && _rectangles[i].Contains(point))
                {
                    return _rectangles[i];
                }
            }
            return null;
        }

        #endregion

        #region 事件触发

        protected virtual void OnRoiCreated(RoiRectangle rect)
        {
            RoiCreated?.Invoke(this, new DZ_ROISelector.EventArgs.RoiCreatedEventArgs(CreateRoiData(_rectangles.IndexOf(rect), rect)));
        }

        protected virtual void OnRoiModified(RoiRectangle rect)
        {
            RoiModified?.Invoke(this, new DZ_ROISelector.EventArgs.RoiModifiedEventArgs(CreateRoiData(_rectangles.IndexOf(rect), rect)));
        }

        protected virtual void OnRoiSelected(RoiRectangle rect)
        {
            RoiSelected?.Invoke(this, new DZ_ROISelector.EventArgs.RoiSelectedEventArgs(CreateRoiData(_rectangles.IndexOf(rect), rect)));
        }

        protected virtual void OnRoiDeleted(RoiRectangle rect)
        {
            RoiDeleted?.Invoke(this, new DZ_ROISelector.EventArgs.RoiDeletedEventArgs(CreateRoiData(-1, rect)));
        }

        #endregion

        #region 重写方法

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            UpdateTransform();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _imageCache?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 事件处理 - 绘制

        private void ROISelector_Paint(object sender, PaintEventArgs e)
        {
            if (_isUpdating)
                return;

            Graphics g = e.Graphics;

            // 根据模式设置绘制质量
            if (Mode == ControlMode.Display)
            {
                DrawingHelper.SetFastRendering(g);
            }
            else
            {
                DrawingHelper.SetHighQuality(g);
            }

            // 绘制图像
            if (SourceImage != null)
            {
                Size scaledSize = _transform.GetScaledImageSize();
                Bitmap scaledImage = _imageCache.GetScaledImage(scaledSize);
                
                if (scaledImage != null)
                {
                    g.DrawImage(scaledImage, _transform.OffsetX, _transform.OffsetY);
                }
            }

            // 绘制所有ROI
            bool showHandles = ShowHandles && Mode == ControlMode.Configuration;
            foreach (var rect in _rectangles)
            {
                if (rect.IsVisible)
                {
                    rect.Draw(g, showHandles, ShowLabels);
                }
            }

            // 绘制正在创建的ROI
            if (_creatingRectangle != null)
            {
                _creatingRectangle.Draw(g, showHandles, ShowLabels);
            }
        }

        #endregion

        #region 事件处理 - 鼠标

        private void ROISelector_MouseDown(object sender, MouseEventArgs e)
        {
            if (Mode != ControlMode.Configuration)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (_currentState == InteractionState.Creating)
                {
                    // 开始创建新ROI
                    if (_roiTypes.ContainsKey(_creatingTypeName))
                    {
                        var roiType = _roiTypes[_creatingTypeName];
                        _creatingRectangle = new RoiRectangle(
                            e.X, e.Y, 0, 0,
                            _creatingTypeName, roiType.Color);
                        _lastMousePosition = e.Location;
                    }
                }
                else
                {
                    // 选择或开始编辑ROI
                    _selectedRectangle = GetRectangleAt(e.Location);
                    
                    if (_selectedRectangle != null)
                    {
                        // 取消其他选中状态
                        foreach (var rect in _rectangles)
                        {
                            rect.IsSelected = (rect == _selectedRectangle);
                        }

                        _currentAnchor = _selectedRectangle.GetHitAnchor(e.X, e.Y);
                        
                        if (_currentAnchor == AnchorType.None)
                        {
                            _currentState = InteractionState.Dragging;
                        }
                        else
                        {
                            _currentState = InteractionState.Resizing;
                        }

                        _lastMousePosition = e.Location;
                        OnRoiSelected(_selectedRectangle);
                    }
                    else
                    {
                        // 取消所有选中
                        foreach (var rect in _rectangles)
                        {
                            rect.IsSelected = false;
                        }
                    }
                    
                    Invalidate();
                }
            }
        }

        private void ROISelector_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mode != ControlMode.Configuration)
                return;

            int dx = e.X - _lastMousePosition.X;
            int dy = e.Y - _lastMousePosition.Y;

            switch (_currentState)
            {
                case InteractionState.Creating:
                    if (_creatingRectangle != null && e.Button == MouseButtons.Left)
                    {
                        // 更新创建中的ROI尺寸
                        _creatingRectangle.Width = e.X - _creatingRectangle.X;
                        _creatingRectangle.Height = e.Y - _creatingRectangle.Y;
                        Invalidate();
                    }
                    break;

                case InteractionState.Dragging:
                    if (_selectedRectangle != null && e.Button == MouseButtons.Left)
                    {
                        _selectedRectangle.Resize(AnchorType.None, dx, dy);
                        
                        // 限制在图像范围内
                        Rectangle imageRect = _transform.GetImageDisplayRect();
                        _selectedRectangle.ClampToBounds(
                            imageRect.Left, imageRect.Top,
                            imageRect.Right, imageRect.Bottom);
                        
                        _lastMousePosition = e.Location;
                        Invalidate();
                    }
                    break;

                case InteractionState.Resizing:
                    if (_selectedRectangle != null && e.Button == MouseButtons.Left)
                    {
                        _selectedRectangle.Resize(_currentAnchor, dx, dy);
                        
                        // 限制在图像范围内
                        Rectangle imageRect = _transform.GetImageDisplayRect();
                        _selectedRectangle.ClampToBounds(
                            imageRect.Left, imageRect.Top,
                            imageRect.Right, imageRect.Bottom);
                        
                        _lastMousePosition = e.Location;
                        Invalidate();
                    }
                    break;

                case InteractionState.Hovering:
                case InteractionState.None:
                    // 更新光标
                    var hoverRect = GetRectangleAt(e.Location);
                    if (hoverRect != null)
                    {
                        AnchorType anchor = hoverRect.GetHitAnchor(e.X, e.Y);
                        Cursor = RoiRectangle.GetCursor(anchor);
                        _currentState = InteractionState.Hovering;
                    }
                    else
                    {
                        Cursor = _currentState == InteractionState.Creating ? Cursors.Cross : Cursors.Default;
                        if (_currentState == InteractionState.Hovering)
                        {
                            _currentState = InteractionState.None;
                        }
                    }
                    break;
            }
        }

        private void ROISelector_MouseUp(object sender, MouseEventArgs e)
        {
            if (Mode != ControlMode.Configuration)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (_currentState == InteractionState.Creating && _creatingRectangle != null)
                {
                    // 完成创建
                    _creatingRectangle.NormalizeRect();
                    
                    // 检查尺寸是否满足最小要求
                    if (_creatingRectangle.Width >= MinRoiSize && _creatingRectangle.Height >= MinRoiSize)
                    {
                        _rectangles.Add(_creatingRectangle);
                        
                        if (_roiTypes.ContainsKey(_creatingTypeName))
                        {
                            _roiTypes[_creatingTypeName].CurrentCount++;
                        }
                        
                        OnRoiCreated(_creatingRectangle);
                    }
                    
                    _creatingRectangle = null;
                    _creatingTypeName = null;
                    _currentState = InteractionState.None;
                    Cursor = Cursors.Default;
                    Invalidate();
                }
                else if (_currentState == InteractionState.Dragging || _currentState == InteractionState.Resizing)
                {
                    // 完成拖动或调整
                    if (_selectedRectangle != null)
                    {
                        OnRoiModified(_selectedRectangle);
                    }
                    
                    _currentState = InteractionState.None;
                    Cursor = Cursors.Default;
                }
            }
        }

        #endregion

        #region 事件处理 - 键盘

        private void ROISelector_KeyDown(object sender, KeyEventArgs e)
        {
            if (Mode != ControlMode.Configuration)
                return;

            if (e.KeyCode == Keys.Delete && _selectedRectangle != null)
            {
                // 删除选中的ROI
                int index = _rectangles.IndexOf(_selectedRectangle);
                if (index >= 0)
                {
                    RemoveRoi(index);
                    _selectedRectangle = null;
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // 取消当前操作
                if (_currentState == InteractionState.Creating)
                {
                    CancelCreating();
                }
                else
                {
                    // 取消选中
                    foreach (var rect in _rectangles)
                    {
                        rect.IsSelected = false;
                    }
                    _selectedRectangle = null;
                    Invalidate();
                }
                e.Handled = true;
            }
        }

        #endregion
    }
}
