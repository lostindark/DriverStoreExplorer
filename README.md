<div align="center">

<img src="./Rapr/icon.ico" alt="logo" width="128">

# Driver Store Explorer (RAPR)

</div>

🌏: [**English**](/) [한국어](/README_KO.md)

---

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)

## ⚠️ Warning
**Driver Store Explorer modifies the Windows driver store. Improper use can cause system malfunction, prevent Windows from booting, or result in loss of device functionality. Know the risks before proceeding. Always back up your drivers before deleting anything.**

---

## What is Driver Store Explorer?

Driver Store Explorer (RAPR) is a powerful tool for viewing, managing, and cleaning up the Windows [DriverStore](https://msdn.microsoft.com/en-us/library/ff544868(VS.85).aspx). It is intended for advanced users and administrators. If you are not familiar with Windows internals, use of this tool is discouraged.

---


## Features

### Core Operations
* **Browse & List**: View all third-party driver packages with detailed metadata (size, version, date etc.)
* **Add/Install**: Install new driver packages with optional automatic device installation
* **Remove/Delete**: Delete single or multiple drivers with force deletion for in-use drivers  
	_Note: The exact implications of removing or deleting drivers depend on system state and usage. Exercise caution, as removal may affect device functionality or require reinstallation of drivers for certain hardware._
* **Export**: Backup selected or all drivers to organized folder structures
* **Smart Cleanup**: Automatically identify and select old/unused driver versions

### Advanced Capabilities
* **Multiple APIs**: Native Windows API, DISM, or PnPUtil backend support with auto-detection
* **Online & Offline**: Work with local machine or offline Windows image driver stores
* **Device Association**: View connected devices and their presence status
* **Batch Operations**: Multi-select for bulk operations with progress tracking
* **Real-time Search**: Live filtering and CSV export for analysis

### User Interface
* **Multi-language**: 20+ languages with RTL support
* **Customizable**: Sortable columns, grouping, and flexible layout
* **Visual Feedback**: Color coding, selection highlighting, and detailed logging

---

## Understanding Driver Status and Removal Options

- **Old drivers:** Drivers are considered as "old" when newer versions exist on the system. Removing these can help free up space and reduce clutter, but may impact compatibility with certain devices or configurations. Consider backing up drivers before removal. The "Select Old Driver(s)" can automatically identify old drivers, though results may vary.
- **Grayed Device Names:** Drivers shown with device names in gray are associated with devices that are not currently connected (such as cameras, phones, or external drives). If you remove these drivers, you will need to reinstall them if you reconnect the device in the future.
- **Force Deletion:** Use this option if you need to delete a driver that is currently in use. Note: This option may not work for print drivers.
---

## Screenshot
![Screenshot of DriverStoreExplorer](https://github.com/user-attachments/assets/2d7df896-494d-4bcd-b064-5f05696cd0d3)

---

## Installation

### Requirements
- Windows 7 or newer
- .NET Framework 4.6.2 or newer
- Administrator privileges

### Option 1: Download Pre-built Binary (Recommended)
1. Go to the [latest release page](https://github.com/lostindark/DriverStoreExplorer/releases/latest)
2. Download the latest ZIP archive
3. Extract the files to a folder of your choice
4. Run `Rapr.exe`

### Option 2: Install via Winget (Recommended)
```powershell
winget install lostindark.DriverStoreExplorer
```
After installation, launch the tool by running:
```powershell
rapr
```

### Option 3: Build from Source
1. Clone or download this repository
2. Open `Rapr.sln` in Visual Studio 2022
3. Build the solution (Build → Build Solution or Ctrl+Shift+B)
4. Run the executable from the output directory

---

## Project History
Originally hosted at https://driverstoreexplorer.codeplex.com/.

## Credits
- [ObjectListView](http://objectlistview.sourceforge.net/)
- [Managed DismApi Wrapper](https://github.com/jeffkl/ManagedDism)
- [FlexibleMessageBox](https://www.codeproject.com/Articles/601900/FlexibleMessageBox-A-Flexible-Replacement-for-the)
- [Resource Embedder](https://github.com/0xced/resource-embedder)
- [PortableSettingsProvider](https://github.com/bluegrams/SettingsProviders)
- [Strong Namer](https://github.com/dsplaisted/strongnamer)

## Sponsors
Free code signing on Windows provided by [SignPath.io], certificate by [SignPath Foundation].

[SignPath.io]: https://signpath.io
[SignPath Foundation]: https://signpath.org
