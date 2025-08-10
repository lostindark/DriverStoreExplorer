Driver Store Explorer [RAPR]
===================================================

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)

## Overview
Driver Store Explorer [RAPR] makes it easier to deal with Windows [driver store](https://msdn.microsoft.com/en-us/library/ff544868(VS.85).aspx). Supported operations include list/add/install/delete/export third-party driver packages.

## Features

### Core Operations
* **Browse & List**: View all third-party driver packages with detailed metadata (size, version, date etc.)
* **Add/Install**: Install new driver packages with optional automatic device installation
* **Remove/Delete**: Delete single or multiple drivers with force deletion for in-use drivers
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

## Screenshots
![Screenshot of DriverStoreExplorer](https://github.com/user-attachments/assets/2d7df896-494d-4bcd-b064-5f05696cd0d3)

## Installation

### Requirements
* .NET Framework 4.6.2 or newer
* Windows 7 or newer
* Administrator privileges (for driver store operations)

### Option 1: Download Pre-built Binary (Recommended)
1. Go to the [latest release page](https://github.com/lostindark/DriverStoreExplorer/releases/latest)
2. Download the latest ZIP archive
3. Extract the files (if downloaded as ZIP) to a folder of your choice
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
4. Navigate to the output directory and run the executable


## History
The project was originally hosted on https://driverstoreexplorer.codeplex.com/.

## Credits
* [ObjectListView](http://objectlistview.sourceforge.net/)
* [Managed DismApi Wrapper](https://github.com/jeffkl/ManagedDism)
* [FlexibleMessageBox](https://www.codeproject.com/Articles/601900/FlexibleMessageBox-A-Flexible-Replacement-for-the)
* [Resource Embedder](https://github.com/0xced/resource-embedder)
* [PortableSettingsProvider](https://github.com/bluegrams/SettingsProviders)
* [Strong Namer](https://github.com/dsplaisted/strongnamer)

## Sponsors
Free code signing on Windows provided by [SignPath.io], certificate by [SignPath Foundation].

[SignPath.io]: https://signpath.io
[SignPath Foundation]: https://signpath.org
