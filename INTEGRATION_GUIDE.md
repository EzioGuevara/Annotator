# DZ_ROISelector 集成指南

## 概述

本文档说明了如何将 DZ_ROISelector 控件集成到 Annotator 项目中。

## 集成完成情况

### ✅ 已完成的工作

1. **项目结构调整**
   - 将 DZ_ROISelector 添加到 Annotator.sln 解决方案
   - 在 Annotator 项目中添加对 DZ_ROISelector 的项目引用
   - 统一 .NET Framework 版本为 4.8

2. **MainForm 重构**
   - 用 DZ_ROISelector 控件替换原有的 PictureBox (picBox)
   - 移除了所有手动绘制和鼠标交互代码
   - 实现了与 DZ_ROISelector 的事件集成

3. **功能映射**
   - **图像加载**: 使用 `_roiSelector.SetImage()`
   - **ROI 创建**: 通过 DZ_ROISelector 的交互式绘制
   - **ROI 编辑**: 支持拖动和 8 锚点调整大小
   - **ROI 删除**: Delete 键删除选中的 ROI
   - **数据保存**: 自动转换为原始图像坐标保存
   - **数据加载**: 从 XML 加载并转换为 ROI

4. **备份文件**
   - `MainForm_Original.cs.bak`: 原始 MainForm.cs 的备份
   - `MainForm_Integrated.cs`: 集成版本的源文件

## 核心改进

### 1. 自动坐标转换
DZ_ROISelector 内部自动处理显示坐标和原始图像坐标的转换:
- 保存时使用原始图像坐标
- 显示时自动缩放到控件大小
- 无需手动计算 ratio、offset 等参数

### 2. 事件驱动架构
```csharp
// ROI 创建事件
_roiSelector.RoiCreated += RoiSelector_RoiCreated;

// ROI 修改事件
_roiSelector.RoiModified += RoiSelector_RoiModified;

// ROI 删除事件
_roiSelector.RoiDeleted += RoiSelector_RoiDeleted;

// ROI 选择事件
_roiSelector.RoiSelected += RoiSelector_RoiSelected;
```

### 3. 简化的 API
```csharp
// 设置图像
_roiSelector.SetImage(loadedImage);

// 添加 ROI
_roiSelector.AddRoi(new RoiData { ... });

// 获取所有 ROI
var rois = _roiSelector.GetAllRois();

// 清除所有 ROI
_roiSelector.ClearAllRois();

// 删除选中的 ROI
_roiSelector.DeleteSelectedRoi();
```

## 与原 Annotator 的兼容性

### 保留的功能
- ✅ XML 格式的标注文件保存/加载
- ✅ 文本格式导出
- ✅ 图像文件夹浏览
- ✅ 上一张/下一张图像导航
- ✅ 键盘快捷键 (Delete 删除)
- ✅ 设置对话框

### 移除的代码
- ❌ 手动绘制逻辑 (picBox_Paint)
- ❌ 鼠标事件处理 (picBox_MouseMove, MouseDown, MouseUp)
- ❌ BRectangle 的手动编辑逻辑
- ❌ 手动坐标转换计算

### 数据兼容性
- ✅ 完全兼容原有的 Annotations.xml 格式
- ✅ 可以加载旧版本创建的标注文件
- ✅ 保存的文件可以被旧版本读取

## DZ_ROISelector 的优势

1. **更强大的交互**
   - 8 个锚点精确调整大小
   - 流畅的拖动体验
   - 自动吸附边界

2. **更好的视觉效果**
   - 双缓冲绘制,无闪烁
   - 高亮显示选中的 ROI
   - 清晰的锚点指示

3. **可扩展性**
   - 支持多种 ROI 类型
   - 每种类型可设置不同颜色
   - 可限制每种类型的数量

4. **自动化处理**
   - 自动坐标转换
   - 自动图像缩放
   - 自动边界检查

## 使用说明

### 基本操作
1. **选择图像文件夹**: 点击 "Browse..." 按钮
2. **创建 ROI**: 在图像上拖动鼠标创建矩形
3. **编辑 ROI**: 
   - 拖动 ROI 移动位置
   - 拖动 8 个锚点调整大小
4. **删除 ROI**: 选中后按 Delete 键
5. **保存标注**: 点击工具栏的 "Save" 按钮

### 快捷键
- `Delete`: 删除选中的 ROI
- `Previous/Next`: 切换图像

## 技术细节

### ROI 数据流

```
加载图像
    ↓
LoadFile() → _roiSelector.SetImage()
    ↓
LoadImageRectangles()
    ↓
从 AnnotationList 获取 BRectangle 列表
    ↓
转换为 RoiData 并添加到 ROISelector
    ↓
用户交互 (创建/编辑/删除)
    ↓
触发事件 → SaveImageRectangles()
    ↓
从 ROISelector 获取 RoiData 列表
    ↓
转换为 BRectangle 并保存到 AnnotationList
```

### 坐标系统

DZ_ROISelector 维护两套坐标系:
1. **原始图像坐标**: 用于数据保存,与图像实际像素对应
2. **显示坐标**: 用于界面显示,根据控件大小自动缩放

所有坐标转换由 DZ_ROISelector 内部自动处理。

## 未来扩展

### 可能的增强功能
1. **多类型 ROI 支持**
   ```csharp
   _roiSelector.RegisterRoiType(new RoiType 
   { 
       TypeId = 1, 
       TypeName = "Defect", 
       Color = Color.Red,
       MaxCount = 10
   });
   ```

2. **ROI 图像裁剪**
   ```csharp
   var croppedImage = _roiSelector.CropRoiImage(roiId);
   ```

3. **批量操作**
   - 批量删除
   - 批量修改属性
   - ROI 复制/粘贴

## 故障排除

### 常见问题

**Q: 编译错误 "找不到 DZ_ROISelector"**
A: 确保 Annotator.csproj 中包含了项目引用:
```xml
<ProjectReference Include="..\DZ_ROISelector\DZ_ROISelector.csproj">
  <Project>{a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d}</Project>
  <Name>DZ_ROISelector</Name>
</ProjectReference>
```

**Q: ROI 位置不准确**
A: DZ_ROISelector 自动处理坐标转换,确保使用 `OriginalRect` 属性获取原始坐标。

**Q: 如何恢复原版本**
A: 使用备份文件:
```powershell
Copy-Item MainForm_Original.cs.bak MainForm.cs -Force
```

## 总结

DZ_ROISelector 的集成大大简化了 Annotator 的代码,同时提供了更强大和流畅的用户体验。所有原有功能都得到保留,并且数据格式完全兼容。
