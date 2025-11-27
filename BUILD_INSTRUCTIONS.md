# Annotator 编译说明

## 问题说明

由于项目使用了 .NET Framework 4.8 和资源文件(包含图片),在使用 `dotnet build` 时可能会遇到 `System.Resources.Extensions` 相关的错误。

## 解决方案

### 方案 1: 使用 Visual Studio 编译 (推荐)

1. 使用 Visual Studio 2019 或更高版本打开 `Annotator.sln`
2. 在解决方案资源管理器中,右键点击解决方案
3. 选择"还原 NuGet 包"
4. 点击"生成" -> "生成解决方案" (或按 Ctrl+Shift+B)

Visual Studio 会自动处理 NuGet 包的还原和编译。

### 方案 2: 使用命令行编译

如果必须使用命令行,请使用 MSBuild:

```powershell
# 使用 Developer Command Prompt for VS 或 Developer PowerShell for VS
cd F:\Develop\Annotator
msbuild Annotator.sln /t:Restore
msbuild Annotator.sln /p:Configuration=Debug
```

### 方案 3: 简化项目配置

如果上述方法都不行,可以移除资源文件的序列化要求:

1. 打开 `Annotator\Annotator.csproj`
2. 删除这一行:
   ```xml
   <GenerateResourceUsePreserializedResources>true</GenerateResourceUsePreserializedResources>
   ```
3. 删除 PackageReference:
   ```xml
   <ItemGroup>
     <PackageReference Include="System.Resources.Extensions" Version="8.0.0" />
   </ItemGroup>
   ```
4. 重新编译

## 集成说明

本项目已成功集成 DZ_ROISelector 控件:

- ✅ DZ_ROISelector 项目已添加到解决方案
- ✅ Annotator 项目已引用 DZ_ROISelector
- ✅ MainForm 已重构使用 ROISelector 控件
- ✅ 保持与原有 XML 标注格式的完全兼容

## 功能特性

### 新增功能
- 8 锚点精确调整 ROI
- 流畅的拖动和缩放
- 自动坐标转换
- 双缓冲绘制
- 事件驱动架构

### 保留功能
- 完全兼容原有的 Annotations.xml 格式
- 支持图像文件夹浏览
- 支持导出标注数据
- 所有原有快捷键和菜单功能

## 文件说明

- `MainForm.cs` - 集成了 ROISelector 的主窗体
- `MainForm_Original.cs.bak` - 原始版本备份
- `MainForm_Integrated.cs` - 集成版本的副本
- `INTEGRATION_GUIDE.md` - 详细的集成指南

## 运行项目

编译成功后,可执行文件位于:
```
Annotator\bin\Debug\Annotator.exe
```

直接运行即可使用增强后的标注工具。
