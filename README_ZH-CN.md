<div align="center">

<img src="./Rapr/icon.ico" alt="logo" width="128">

# Driver Store Explorer (RAPR)

</div>

🌏: [English](/) [한국어](/README_KO.md) [**简体中文**](/README_ZH-CN.md) [繁體中文](/README_ZH-TW.md)

---

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)

## ⚠️ 警告
**Driver Store Explorer 会修改 Windows 驱动程序存储。不当使用可能导致系统故障、Windows 无法启动或设备功能丢失。请在操作前充分了解风险。在删除任何驱动程序之前，请务必进行备份。**

---

## 什么是 Driver Store Explorer？

Driver Store Explorer (RAPR) 是一款功能强大的工具，用于查看、管理和清理 Windows [驱动程序存储](https://msdn.microsoft.com/en-us/library/ff544868(VS.85).aspx)。它面向高级用户和系统管理员。如果您不熟悉 Windows 内部机制，不建议使用此工具。

---


## 功能

### 核心操作
* **浏览和列表**：查看所有第三方驱动程序包的详细元数据（大小、版本、日期等）
* **添加/安装**：安装新驱动程序包，支持可选的自动设备安装
* **移除/删除**：删除单个或多个驱动程序，支持对正在使用的驱动程序进行强制删除
	_注意：移除或删除驱动程序的确切影响取决于系统状态和使用情况。请谨慎操作，因为移除可能影响设备功能或需要为某些硬件重新安装驱动程序。_
* **导出**：将选定或所有驱动程序备份到有组织的文件夹结构中
* **智能清理**：自动识别并选择旧的/未使用的驱动程序版本

### 高级功能
* **多种 API**：支持原生 Windows API、DISM 或 PnPUtil 后端，具备自动检测功能
* **在线和离线**：支持本地计算机或离线 Windows 映像驱动程序存储
* **设备关联**：查看已连接设备及其存在状态
* **批量操作**：支持多选批量操作，附带进度跟踪
* **实时搜索**：实时筛选和 CSV 导出，便于分析

### 用户界面
* **多语言**：支持 20 多种语言，包括从右到左的文字
* **可自定义**：可排序的列、分组和灵活的布局
* **视觉反馈**：颜色编码、选择高亮和详细日志记录

---

## 了解驱动程序状态和移除选项

- **旧驱动程序：** 当系统中存在更新版本时，驱动程序被视为"旧"驱动程序。移除这些驱动程序可以帮助释放空间并减少杂乱，但可能会影响与某些设备或配置的兼容性。建议在移除前备份驱动程序。"选择旧驱动程序"功能可以自动识别旧驱动程序，但结果可能有所不同。
- **灰色设备名称：** 设备名称显示为灰色的驱动程序与当前未连接的设备（如相机、手机或外置驱动器）相关联。如果移除这些驱动程序，将来重新连接设备时需要重新安装。
- **强制删除：** 如果需要删除正在使用的驱动程序，请使用此选项。注意：此选项可能不适用于打印驱动程序。
---

## 截图
![Screenshot of DriverStoreExplorer](https://github.com/user-attachments/assets/2d7df896-494d-4bcd-b064-5f05696cd0d3)

---

## 安装

### 系统要求
- Windows 7 或更高版本
- .NET Framework 4.6.2 或更高版本
- 管理员权限

### 方式一：下载预编译版本（推荐）
1. 前往[最新版本页面](https://github.com/lostindark/DriverStoreExplorer/releases/latest)
2. 下载最新的 ZIP 压缩包
3. 将文件解压到您选择的文件夹中
4. 运行 `Rapr.exe`

### 方式二：通过 Winget 安装（推荐）
```powershell
winget install lostindark.DriverStoreExplorer
```
安装后，运行以下命令启动工具：
```powershell
rapr
```

### 方式三：从源代码构建
1. 克隆或下载此仓库
2. 在 Visual Studio 2022 中打开 `Rapr.sln`
3. 构建解决方案（生成 → 生成解决方案 或 Ctrl+Shift+B）
4. 从输出目录运行可执行文件

---

## 项目历史
最初托管于 [https://driverstoreexplorer.codeplex.com/](https://web.archive.org/web/20190417132137/https://archive.codeplex.com/?p=driverstoreexplorer)。

## 致谢
- [ObjectListView](http://objectlistview.sourceforge.net/)
- [Managed DismApi Wrapper](https://github.com/jeffkl/ManagedDism)
- [FlexibleMessageBox](https://www.codeproject.com/Articles/601900/FlexibleMessageBox-A-Flexible-Replacement-for-the)
- [Resource Embedder](https://github.com/0xced/resource-embedder)
- [PortableSettingsProvider](https://github.com/bluegrams/SettingsProviders)
- [Strong Namer](https://github.com/dsplaisted/strongnamer)

## 赞助商
Windows 上的免费代码签名由 [SignPath.io] 提供，证书由 [SignPath Foundation] 提供。

[SignPath.io]: https://signpath.io
[SignPath Foundation]: https://signpath.org
