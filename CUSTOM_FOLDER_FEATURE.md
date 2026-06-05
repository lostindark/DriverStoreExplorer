# Custom Folder Driver Store Feature - Implementation Guide

## Overview

The Driver Store Explorer has been enhanced with the ability to select **any folder as a custom driver store**. This allows you to:

- ✅ Create a folder to hold driver packages
- ✅ Add drivers from your system to this folder
- ✅ Remove drivers from this folder
- ✅ Export drivers from this folder
- ✅ Use this folder with Windows setup: `setup.exe /drivers <your folder>`

This is perfect for offline Windows installations where you want to pre-stage specific drivers.

## Implementation Details

### New Components Added

1. **CustomFolderDriverStore.cs** (`Rapr/Utils/`)
   - Implements `IDriverStore` interface
   - Manages driver packages in any user-selected folder
   - Supports enumeration, addition, deletion, and export of drivers

2. **DriverStoreType Update**
   - Added `CustomFolder` enum value to `DriverStoreType`

3. **DriverStoreFactory Update**
   - Added `CreateCustomFolderDriverStore(string folderPath)` method

4. **UI Updates - ChooseDriverStore Dialog**
   - New radio button: "Custom Driver Folder"
   - Same browse and path validation as offline stores
   - Folder selection validation (must be accessible)

## How to Use

### From the Application

1. **Launch Driver Store Explorer** (Rapr.exe)
2. **Click "Choose Driver Store"** from the menu (Tools or similar menu depending on version)
3. **Select "Custom Driver Folder"** radio button
4. **Click the browse button (...)** to select or create a folder
5. **Click OK** to load the custom folder

### Working with Your Custom Driver Store

Once a custom folder is selected, you can:

- **View drivers** in the folder (all .inf files are scanned)
- **Add drivers**: Click "Add Driver", select driver .inf files
- **Delete drivers**: Select drivers, check boxes, click "Delete Driver"
- **Export drivers**: Select drivers, click "Export Drivers", choose destination

### Preparing Drivers for Windows Installation

1. **Create a folder** (e.g., `C:\MyDrivers`)
2. **Open Driver Store Explorer** with "Custom Driver Folder" mode pointing to that folder
3. **Add your drivers** to the folder using the application
4. **Use with Windows Setup**:
   ```
   setup.exe /drivers C:\MyDrivers
   ```

Or during a Windows unattended installation:
```xml
<DriverPaths>
    <DriverPath priority="1">C:\MyDrivers</DriverPath>
</DriverPaths>
```

## Folder Structure

Your custom driver folder can have any structure. The application will:
- Scan recursively for all `.inf` files
- Extract driver information from the INF files
- Organize drivers by folder for management

**Example structure after adding drivers:**
```
C:\MyDrivers\
├── NetworkDriver_custom\
│   ├── netdriver.inf
│   ├── netdriver.sys
│   └── netdriver.cat
├── AudioDriver_custom\
│   ├── audiodriver.inf
│   ├── audiodriver.sys
│   └── audiodriver.cat
└── VideoDriver_custom\
    ├── videodriver.inf
    ├── videodriver.sys
    └── videodriver.cat
```

## Features & Limitations

### ✅ Supported Operations
- Browse and select any folder
- Enumerate drivers in the folder
- View driver details (version, date, class, provider)
- Delete drivers by removing their folders
- Add drivers by copying INF and related files
- Export drivers to another location
- Search and filter drivers

### ⚠️ Limitations (by design)
- Cannot install drivers from custom folder (install-on-demand not supported)
- Device information not available (since drivers aren't installed)
- No boot critical flag management
- Signer information limited to metadata in INF

## Technical Notes

### Data Model

When you select a custom folder, the application:
1. Scans for all `.inf` files recursively
2. Parses each INF to extract:
   - Driver name and version
   - Driver date
   - Size calculation
   - Class (defaults to "Unknown")
   - Provider (defaults to "Custom Folder")

3. Creates `DriverStoreEntry` objects for display and management

### IDriverStore Interface Implementation

The `CustomFolderDriverStore` class fully implements the `IDriverStore` interface:

```csharp
public class CustomFolderDriverStore : IDriverStore
{
    public DriverStoreType Type { get; }           // Returns DriverStoreType.CustomFolder
    public string OfflineStoreLocation { get; }    // Returns the folder path
    public bool SupportAddInstall { get; }         // Returns false
    public bool SupportForceDeletion { get; }      // Returns true
    public bool SupportDeviceNameColumn { get; }   // Returns false
    public bool SupportExportDriver { get; }       // Returns true
    public bool SupportExportAllDrivers { get; }   // Returns true
    
    public List<DriverStoreEntry> EnumeratePackages();
    public bool DeleteDriver(DriverStoreEntry entry, bool forceDelete);
    public bool AddDriver(string infFullPath, bool install);
    public bool ExportDriver(DriverStoreEntry entry, string destinationPath);
    public bool ExportAllDrivers(string destinationPath);
}
```

## Testing Recommendations

1. **Create a test folder** with some driver packages
2. **Select it in the app** and verify drivers appear
3. **Add a driver** and confirm it copies correctly
4. **Delete a driver** and verify the folder is removed
5. **Export drivers** and check the destination folder
6. **Use the exported drivers** with Windows setup

## Future Enhancements

Potential improvements:
- Driver validation (signature checking)
- Duplicate driver detection
- Driver dependency analysis
- Batch import from directories
- Driver version comparisons
- Support for driver categories/tags

---

**Version**: 1.0  
**Date**: June 2026  
**Status**: Implemented and tested
