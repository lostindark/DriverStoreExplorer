Driver Store Explorer [RAPR]
===================================================

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)

### Overview
--------
Driver Store Explorer [RAPR] makes it easier to deal with Windows [driver store](https://msdn.microsoft.com/en-us/library/ff544868(VS.85).aspx). Supported operations include list/add/install/delete third-party driver packages.

### Features:
* Support online (local machine) and offline driver store.
* Enumerate / list all third-party driver packages in the driver store. Showing device associated with drivers. Export the driver package list as CSV.
* Add a driver package to the driver store.
* Delete one or multiple driver packages from the store.
* Detect old and not used driver packages (best effort).
* Full-fledged GUI Supports grouping / sorting on any column. Supports re-arranging of / selecting specific columns.

### Screenshots:
![Screenshot of DriverStoreExplorer](https://github.com/lostindark/DriverStoreExplorer/raw/master/Screenshots/Screenshot.png "Screenshot of Driver Store Explorer")

### Requirements: 
This tool requires:
* .NET Framework 4.5 or newer
* Windows 7 or newer

To build the code yourself, open Rapr.sln in Visual Studio 2019. Visual Studio 2017 may also work but it is not guaranteed.

### Releases:
Download the latest version here: https://github.com/lostindark/DriverStoreExplorer/releases/latest.

### History:
The project was originally hosted on https://driverstoreexplorer.codeplex.com/.

### Credits:
This tool uses the excellent [ObjectListView](http://objectlistview.sourceforge.net/cs/index.html).
