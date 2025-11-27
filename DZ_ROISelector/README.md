# DZ_ROISelector - ROI 选择器控件

一个功能强大的 WinForms 用户控件，用于在图像上进行 ROI（感兴趣区域）的选择、标注和管理。

## 功能特性

### 核心功能
- ✅ **多类型 ROI 支持**：支持注册多种 ROI 类型，每种类型可设置不同颜色
- ✅ **数量限制**：可为每种 ROI 类型设置最大数量限制
- ✅ **交互式创建**：鼠标拖拽创建矩形 ROI
- ✅ **拖动和调整**：支持拖动移动和 8 个锚点调整大小
- ✅ **坐标转换**：自动处理原始图像坐标和显示坐标的转换
- ✅ **图像自适应**：图像自动缩放以适应控件大小，保持宽高比
- ✅ **双缓冲绘制**：流畅的绘制性能，无闪烁

### 高级功能
- ✅ **配置/显示模式**：支持配置模式（可编辑）和显示模式（只读）
- ✅ **图像裁剪**：可获取 ROI 区域的裁剪图像
- ✅ **批量操作**：支持批量更新、清空等操作
- ✅ **事件系统**：提供创建、修改、选中、删除等事件
- ✅ **键盘快捷键**：Delete 删除、Escape 取消
- ✅ **可见性控制**：可按类型控制 ROI 的显示/隐藏

## 快速开始

### 1. 基本使用

```csharp
// 创建控件实例
var roiSelector = new ROISelector();
roiSelector.Dock = DockStyle.Fill;
this.Controls.Add(roiSelector);

// 注册 ROI 类型
roiSelector.RegisterRoiType("缺陷", Color.Red, "产品缺陷区域");
roiSelector.RegisterRoiType("标签", Color.Green, "产品标签区域");
roiSelector.RegisterRoiType("二维码", Color.Blue, "二维码区域");

// 设置类型的最大数量（可选）
roiSelector.SetMaxCount("二维码", 1);  // 二维码只能有一个

// 加载图像
roiSelector.SourceImage = Image.FromFile("test.jpg");
```

### 2. 创建 ROI

```csharp
// 方式1：用户交互创建
private void btnCreateDefect_Click(object sender, EventArgs e)
{
    roiSelector.StartCreating("缺陷");
    // 用户在图像上拖拽鼠标创建矩形
}

// 方式2：代码创建
roiSelector.AddRoi("标签", new Rectangle(100, 100, 200, 150));
```

### 3. 获取 ROI 数据

```csharp
// 获取所有 ROI
List<RoiData> allRois = roiSelector.GetAllRois();
foreach (var roi in allRois)
{
    Console.WriteLine($"类型: {roi.TypeName}, 位置: {roi.OriginalRect}");
}

// 按类型获取
List<RoiData> defects = roiSelector.GetRoisByType("缺陷");

// 获取 ROI 裁剪图像
Bitmap roiImage = roiSelector.GetRoiImage(0);
if (roiImage != null)
{
    roiImage.Save("roi_0.jpg");
}

// 批量获取所有 ROI 图像
Dictionary<string, List<Bitmap>> allImages = roiSelector.GetAllRoiImages();
```

### 4. 事件处理

```csharp
// ROI 创建事件
roiSelector.RoiCreated += (s, e) =>
{
    Console.WriteLine($"创建了 {e.RoiData.TypeName} ROI");
    Console.WriteLine($"原始坐标: {e.RoiData.OriginalRect}");
};

// ROI 修改事件
roiSelector.RoiModified += (s, e) =>
{
    Console.WriteLine($"修改了 ROI {e.RoiData.Index}");
};

// ROI 选中事件
roiSelector.RoiSelected += (s, e) =>
{
    Console.WriteLine($"选中了 ROI {e.RoiData.Index}");
};

// ROI 删除事件
roiSelector.RoiDeleted += (s, e) =>
{
    Console.WriteLine($"删除了 {e.RoiData.TypeName} ROI");
};
```

## 属性说明

### 控件属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Mode` | `ControlMode` | `Configuration` | 工作模式：Configuration（可编辑）或 Display（只读） |
| `ShowLabels` | `bool` | `true` | 是否显示 ROI 类型标签 |
| `ShowHandles` | `bool` | `true` | 是否显示调整锚点（仅配置模式） |
| `MinRoiSize` | `int` | `10` | 最小 ROI 尺寸（像素） |
| `SourceImage` | `Image` | `null` | 原始图像 |

### RoiData 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Index` | `int` | ROI 索引 |
| `TypeName` | `string` | ROI 类型名称 |
| `Color` | `Color` | 显示颜色 |
| `OriginalRect` | `Rectangle` | 原始图像坐标系中的矩形 |
| `DisplayRect` | `Rectangle` | 显示坐标系中的矩形 |
| `IsVisible` | `bool` | 是否可见 |
| `Tag` | `object` | 附加数据 |

## API 参考

### 类型管理

```csharp
// 注册 ROI 类型
void RegisterRoiType(string typeName, Color color, string description = "")

// 设置类型最大数量
void SetMaxCount(string typeName, int maxCount)

// 设置类型可见性
void SetTypeVisible(string typeName, bool visible)
```

### ROI 操作

```csharp
// 开始创建 ROI
void StartCreating(string typeName)

// 取消创建
void CancelCreating()

// 添加 ROI
void AddRoi(string typeName, Rectangle rect)

// 删除 ROI
void RemoveRoi(int index)

// 清空 ROI
void ClearRois(string typeName = null)

// 更新 ROI
void UpdateRoi(int index, Rectangle rect)

// 高亮 ROI
void HighlightRoi(int index, bool highlight)
```

### 数据获取

```csharp
// 获取所有 ROI
List<RoiData> GetAllRois()

// 按类型获取 ROI
List<RoiData> GetRoisByType(string typeName)

// 获取指定 ROI
RoiData GetRoi(int index)

// 获取 ROI 图像
Bitmap GetRoiImage(int index)

// 批量获取 ROI 图像
Dictionary<string, List<Bitmap>> GetAllRoiImages()
```

### 批量更新

```csharp
// 开始批量更新（暂停刷新）
void BeginUpdate()

// 结束批量更新（恢复刷新）
void EndUpdate()
```

## 使用示例

### 示例 1：缺陷检测标注工具

```csharp
public class DefectAnnotationForm : Form
{
    private ROISelector roiSelector;
    private ListBox lstRois;
    
    public DefectAnnotationForm()
    {
        InitializeComponent();
        
        // 初始化控件
        roiSelector = new ROISelector();
        roiSelector.Dock = DockStyle.Fill;
        
        // 注册缺陷类型
        roiSelector.RegisterRoiType("划痕", Color.Red);
        roiSelector.RegisterRoiType("污点", Color.Orange);
        roiSelector.RegisterRoiType("凹陷", Color.Yellow);
        
        // 监听事件
        roiSelector.RoiCreated += OnRoiCreated;
        roiSelector.RoiDeleted += OnRoiDeleted;
    }
    
    private void btnLoadImage_Click(object sender, EventArgs e)
    {
        using (var ofd = new OpenFileDialog())
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                roiSelector.SourceImage = Image.FromFile(ofd.FileName);
            }
        }
    }
    
    private void btnCreateScratch_Click(object sender, EventArgs e)
    {
        roiSelector.StartCreating("划痕");
    }
    
    private void btnSave_Click(object sender, EventArgs e)
    {
        var rois = roiSelector.GetAllRois();
        // 保存 ROI 数据到文件或数据库
        SaveRoisToFile(rois, "annotations.json");
    }
    
    private void OnRoiCreated(object sender, RoiCreatedEventArgs e)
    {
        lstRois.Items.Add($"{e.RoiData.TypeName} - {e.RoiData.OriginalRect}");
    }
}
```

### 示例 2：图像裁剪工具

```csharp
public class ImageCropperForm : Form
{
    private ROISelector roiSelector;
    
    public ImageCropperForm()
    {
        roiSelector = new ROISelector();
        roiSelector.RegisterRoiType("裁剪区域", Color.Blue);
        roiSelector.SetMaxCount("裁剪区域", 1);  // 只允许一个裁剪区域
        
        roiSelector.RoiCreated += (s, e) =>
        {
            // 自动裁剪并保存
            Bitmap croppedImage = roiSelector.GetRoiImage(0);
            if (croppedImage != null)
            {
                croppedImage.Save("cropped.jpg");
                MessageBox.Show("裁剪完成！");
            }
        };
    }
}
```

### 示例 3：多区域分析

```csharp
// 批量创建 ROI
roiSelector.BeginUpdate();
for (int i = 0; i < detectedRegions.Count; i++)
{
    roiSelector.AddRoi("检测区域", detectedRegions[i]);
}
roiSelector.EndUpdate();

// 批量获取并分析
var images = roiSelector.GetAllRoiImages();
foreach (var kvp in images)
{
    string typeName = kvp.Key;
    List<Bitmap> regionImages = kvp.Value;
    
    foreach (var img in regionImages)
    {
        // 对每个区域进行分析
        AnalyzeRegion(img);
    }
}
```

## 键盘快捷键

- **Delete**：删除选中的 ROI
- **Escape**：取消当前操作或取消选中

## 注意事项

1. **坐标系统**：控件内部自动处理坐标转换，`GetAllRois()` 返回的 `OriginalRect` 是原始图像坐标
2. **内存管理**：使用 `GetRoiImage()` 获取的 Bitmap 需要手动释放
3. **线程安全**：控件操作应在 UI 线程中进行
4. **性能优化**：批量操作时使用 `BeginUpdate()`/`EndUpdate()` 提高性能

## 技术架构

### 核心组件

- **ROISelector**：主控件类
- **RoiRectangle**：ROI 矩形类，包含绘制和交互逻辑
- **CoordinateTransform**：坐标转换工具
- **ImageCache**：图像缓存管理
- **DrawingHelper**：绘图辅助工具

### 设计模式

- 事件驱动架构
- 双缓冲绘制
- 坐标系统抽象
- 类型注册机制

## 版本历史

### v1.0.0 (2025-01-28)
- 初始版本发布
- 支持基本的 ROI 创建、编辑、删除功能
- 实现坐标转换和图像自适应
- 提供完整的事件系统

## 许可证

本项目采用 MIT 许可证。

## 联系方式

如有问题或建议，请联系开发团队。
