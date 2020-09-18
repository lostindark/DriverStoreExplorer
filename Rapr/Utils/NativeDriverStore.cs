using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Rapr.Utils
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "Interop")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Interop")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Interop")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Interop")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Interop")]
    public class NativeDriverStore : IDriverStore
    {
        public DriverStoreType Type { get; }

        public string OfflineStoreLocation { get; }

        public NativeDriverStore()
        {
            this.Type = DriverStoreType.Online;
        }

        public NativeDriverStore(string imagePath)
        {
            this.Type = DriverStoreType.Offline;
            this.OfflineStoreLocation = imagePath;
        }

        public bool SupportAddInstall => this.Type == DriverStoreType.Online;

        public bool SupportForceDeletion => this.Type == DriverStoreType.Online;

        public bool SupportDeviceNameColumn => this.Type == DriverStoreType.Online;

        public bool SupportExportDriver => true;

        public bool SupportExportAllDrivers => true;

        public List<DriverStoreEntry> EnumeratePackages()
        {
            var ptr = NativeMethods.DriverStoreOpen(null, null, 0, IntPtr.Zero);
            if (ptr == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();
            var devicesInfo = new List<DeviceDriverInfo>();

            try
            {
                {
                    GCHandle handle = GCHandle.Alloc(devicesInfo);
                    try
                    {
                        NativeMethods.DriverStoreEnumObjects(
                            ptr,
                            DriverStoreObjectType.DeviceNode,
                            DRIVERSTORE_LOCK_LEVEL.NONE,
                            EnumDeviceObjects,
                            GCHandle.ToIntPtr(handle));
                    }
                    finally
                    {
                        handle.Free();
                    }
                }

                {
                    GCHandle handle = GCHandle.Alloc(driverStoreEntries);
                    try
                    {
                        NativeMethods.DriverStoreEnum(
                            ptr,
                            DriverStoreEnumFlags.OemOnly,
                            EnumDriverPackages,
                            GCHandle.ToIntPtr(handle));
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
            }
            finally
            {
                NativeMethods.DriverStoreClose(ptr);
            }

            foreach (var driverStoreEntry in driverStoreEntries)
            {
                var deviceInfo = devicesInfo.OrderByDescending(d => d.IsPresent)?.FirstOrDefault(e =>
                    string.Equals(e.DriverInf, driverStoreEntry.DriverPublishedName, StringComparison.OrdinalIgnoreCase)
                    && e.DriverVersion == driverStoreEntry.DriverVersion
                    && e.DriverDate == driverStoreEntry.DriverDate);

                driverStoreEntry.DeviceName = deviceInfo?.DeviceName;
                driverStoreEntry.DevicePresent = deviceInfo?.IsPresent;
            }

            return driverStoreEntries;
        }

        public bool DeleteDriver(DriverStoreEntry driverStoreEntry, bool forceDelete)
        {
            if (driverStoreEntry == null)
            {
                throw new ArgumentNullException(nameof(driverStoreEntry));
            }

            switch (this.Type)
            {
                case DriverStoreType.Online:
                    try
                    {
                        SetupAPI.DeleteDriver(driverStoreEntry, forceDelete);
                    }
                    catch (Win32Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                        return false;
                    }

                    return true;

                case DriverStoreType.Offline:
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException();
            }
        }

        public bool AddDriver(string infFullPath, bool install)
        {
            switch (this.Type)
            {
                case DriverStoreType.Online:
                    try
                    {
                        SetupAPI.AddDriver(infFullPath, install);
                    }
                    catch (Win32Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                        return false;
                    }

                    return true;

                case DriverStoreType.Offline:
                    throw new NotImplementedException();

                default:
                    throw new NotSupportedException();
            }
        }

        internal static bool EnumDeviceObjects(
                IntPtr hDriverStore,
                DriverStoreObjectType ObjectType,
                string ObjectName,
                IntPtr lParam)
        {
            var devicesInfo = (List<DeviceDriverInfo>)GCHandle.FromIntPtr(lParam).Target;

            devicesInfo.Add(new DeviceDriverInfo(
                GetObjectPropertyInfo<string>(hDriverStore, ObjectName, DeviceHelper.DEVPKEY_Device_DriverDesc, ObjectType),
                GetObjectPropertyInfo<string>(hDriverStore, ObjectName, DeviceHelper.DEVPKEY_Device_DriverInfPath, ObjectType),
                GetObjectPropertyInfo<DateTime>(hDriverStore, ObjectName, DeviceHelper.DEVPKEY_Device_DriverDate, ObjectType),
                Version.Parse(GetObjectPropertyInfo<string>(hDriverStore, ObjectName, DeviceHelper.DEVPKEY_Device_DriverVersion, ObjectType)),
                GetObjectPropertyInfo<bool?>(hDriverStore, ObjectName, DeviceHelper.DEVPKEY_Device_IsPresent, ObjectType)));

            return true;
        }

        internal static bool EnumDriverPackages(
            IntPtr driverStoreHandle,
            string driverStoreFilename,
            DriverPackageInfo pDriverPackageInfo,
            IntPtr lParam)
        {
            List<DriverStoreEntry> driverStoreEntries = (List<DriverStoreEntry>)GCHandle.FromIntPtr(lParam).Target;
            var driverClassGuid = GetObjectPropertyInfo<Guid>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_ClassGuid);

            DriverStoreEntry driverStoreEntry = new DriverStoreEntry
            {
                DriverClass = ConfigManager.GetClassProperty<string>(driverClassGuid, DeviceHelper.DEVPKEY_DeviceClass_Name),
                DriverInfName = Path.GetFileName(driverStoreFilename),
                DriverPublishedName = pDriverPackageInfo.PublishedInfName,
                DriverPkgProvider = GetObjectPropertyInfo<string>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_ProviderName),
                DriverSignerName = GetObjectPropertyInfo<string>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_SignerName),
                DriverDate = GetObjectPropertyInfo<DateTime>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_DriverDate),
                DriverVersion = GetObjectPropertyInfo<Version>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_DriverVersion),
                DriverFolderLocation = Path.GetDirectoryName(driverStoreFilename),
                DriverSize = DriverStoreRepository.GetFolderSize(new DirectoryInfo(Path.GetDirectoryName(driverStoreFilename))),
                BootCritical = GetObjectPropertyInfo<bool?>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_BootCritical),
            };

            driverStoreEntries.Add(driverStoreEntry);

            return true;
        }

        internal static T GetObjectPropertyInfo<T>(
            IntPtr driverStoreHandle,
            string objectName,
            DevPropKey propertyKey,
            DriverStoreObjectType objectType = DriverStoreObjectType.DriverPackage)
        {
            const int bufferSize = 2048;
            IntPtr propertyBufferPtr = Marshal.AllocHGlobal(bufferSize);

            if (NativeMethods.DriverStoreGetObjectProperty(
                driverStoreHandle,
                objectType,
                objectName,
                ref propertyKey,
                out DevPropType propertyType,
                propertyBufferPtr,
                bufferSize,
                out uint propertySize,
                DriverStoreSetObjectPropertyFlags.None))
            {
                if (propertySize > 0)
                {
                    return DeviceHelper.ConvertPropToType<T>(propertyBufferPtr, propertyType);
                }
            }

            Marshal.FreeHGlobal(propertyBufferPtr);

            return default;
        }

        public bool ExportDriver(string infName, string destinationPath) => throw new NotImplementedException();

        public bool ExportAllDrivers(string destinationPath) => throw new NotImplementedException();

        // Define other methods and classes here
        private const int MAX_PATH = 260;
        private const int LOCALE_NAME_MAX_LENGTH = 85;

        #region Enums
        /// <summary>
        /// Processor Architecture (must match winnt.h)
        /// </summary>
        public enum ProcessorArchitecture : ushort
        {
            PROCESSOR_ARCHITECTURE_INTEL = 0,
            PROCESSOR_ARCHITECTURE_MIPS = 1,
            PROCESSOR_ARCHITECTURE_ALPHA = 2,
            PROCESSOR_ARCHITECTURE_PPC = 3,
            PROCESSOR_ARCHITECTURE_SHX = 4,
            PROCESSOR_ARCHITECTURE_ARM = 5,
            PROCESSOR_ARCHITECTURE_IA64 = 6,
            PROCESSOR_ARCHITECTURE_ALPHA64 = 7,
            PROCESSOR_ARCHITECTURE_MSIL = 8,
            PROCESSOR_ARCHITECTURE_AMD64 = 9,
            PROCESSOR_ARCHITECTURE_IA32_ON_WIN64 = 10,
            PROCESSOR_ARCHITECTURE_NEUTRAL = 11,
            PROCESSOR_ARCHITECTURE_ARM64 = 12,
            PROCESSOR_ARCHITECTURE_UNKNOWN = 0xFFFF,
        }

        /// <summary>
        /// Flags for Opening the driver store.
        /// </summary>
        [Flags]
        internal enum DriverStoreOpenFlags : uint
        {
            None = 0x00000000,                   // Unknown
            Create = 0x00000001,                    // Create Driver Store if it doesnot exist
            Exclusive = 0x00000002,                 // Open Driver store for exclusive access
        }

        /// <summary>
        /// Flags for Importing the driver in the driver store.
        /// </summary>
        [Flags]
        internal enum DriverStoreImportFlags : uint
        {
            None = 0x00000000,                   // Unknown
            SkipTempCopy = 0x00000001,              // Skip temporary file copy step
            SkipExternalFileCheck = 0x00000002,     // Skip external file presence check
            NoRestorePoint = 0x00000004,            // Do not set a system restore point
            NonInteractive = 0x00000008,            // Enable non-interactive mode to not show any UI dialogs
            Replace = 0x00000020,                   // Replace existing driver package
            Hardlink = 0x00000040,                  // Hardlink files into Driver Store
            PublishSameName = 0x00000100,           // Publish same INF name instead of OEM INF name
            Inbox = 0x00000200,                     // Inbox driver package
            F6 = 0x00000400,                        // F6 driver package
            BaseVersion = 0x00000800,               // Base driver package version
            SystemDefaultLocale = 0x00001000,       // Only import files for system default locale
            SystemCritical = 0x00002000             // System critical driver package
        }

        /// <summary>
        /// Flags for Importing the driver in the driver store.
        /// </summary>
        [Flags]
        internal enum DriverStoreOfflineAddDriverPackageFlags : uint
        {
            None = 0x00000000,                   // Unknown
            SkipInstall = 0x00000001,  //  Add the package to the driver store but skip the installation
            Inbox = 0x00000002,  // driver to be added is an inbox package
            F6 = 0x00000004, //  Add the package to the driver store as if the package was specified through the F6 mechanism
            SkipExternalFilePresenceCheck = 0x00000008, // Don't do presence check of external files in the driver package
            NoTempCopy = 0x00000010, // Do not perform the copy to the temporary directory
            UseHardLinks = 0x00000020, // Use hard links when importing to the driver store
            InstallOnly = 0x00000040, // Only install (reflect) a driver package that is already in the driver store.
            ReplacePackage = 0x00000080, // Replace the driver package if it is already present in the driver store.
            Force = 0x00000100, // Force offline reflection regardless of device class when importing to the driver store.
            BaseVersion = 0x00000200, // Driver package being added is the base version
        }

        [Flags]
        internal enum DriverStorePublishFlags : uint
        {
            None = 0x00000000
        }

        [Flags]
        internal enum DriverStoreConfigureFlags : uint
        {
            None = 0x00000000,                   // Unknown
            Force = 0x00000001,    // Force configuration of non-configurable driver package
            ActiveOnly = 0x00000002,    // Configure already active configurations only
            SourceConfigurations = 0x00010000,    // Source filter supplies configurations
            SourceDeviceIds = 0x00020000,    // Source filter supplies device IDs
            TargetDeviceNodes = 0x00100000,    // Target filter supplies device instance IDs
        }

        [Flags]
        internal enum DriverStoreReflectFlags : uint
        {
            None = 0x00000000,
            FilesOnly = 0x00000001,    // Reflect driver files only
            ActiveDrivers = 0x00000002,    // Reflect previously reflected drivers for published name
            ExternalOnly = 0x00000004,    // Reflect external driver operations only
            Configurations = 0x00000008,    // Reflect driver configurations into driver database
        }

        [Flags]
        internal enum DriverStoreReflectCriticalFlags : uint
        {
            None = 0x00000000,
            Force = 0x00000001,    // Force reflection of non-boot critical driver packages
            Configurations = 0x00000002,    // Reflect driver configurations into driver database
        }

        [Flags]
        internal enum DriverStoreSetObjectPropertyFlags : uint
        {
            None = 0x00000000,
        }

        [Flags]
        public enum DriverPackageEnumFilesFlags
        {
            None = 0,
            Copy = 0x00000001,              // Enumerate copy file operations
            Delete = 0x00000002,            // Enumerate delete file operations
            Rename = 0x00000004,            // Enumerate rename file operations
            Inf = 0x00000010,               // Enumerate driver package INF file
            Catalog = 0x00000020,           // Enumerate catalog file
            Binaries = 0x00000040,          // Enumerate binary files
            CopyInfs = 0x00000080,          // Enumerate copy INF files
            IncludeInfs = 0x00000100,       // Enumerate include INF files
            External = 0x00001000,          // Include external files in enumeration
            UniqueSource = 0x00002000,      // Only return files with unique sources
            UniqueDestination = 0x00004000  // Only return files with unique destinations
        }

        [Flags]
        public enum DriverPackageOpenFlags
        {
            None = 0,
            VersionOnly = 0x00000001,           // Open for version information only
            FilesOnly = 0x00000002,             // Open for file enumeration only
            DefaultLanguage = 0x00000004,       // Open with default language
            LocalizableStrings = 0x00000008,    // Open with localizable strings where applicable
            TargetOSVersion = 0x00000010,       // Open for use with target OS version
            StrictValidation = 0x00000020,      // Open with strict validation
            ClassSchemaOnly = 0x00000040,       // Open for class schema setting enumeration only
            LogTelemetry = 0x00000080,          // Open with telemetry logging
            PrimaryOnly = 0x00000100            // Open only the primary INF
        }

        [Flags]
        internal enum DriverStoreCopyFlags : uint
        {
            None = 0x00000000,                  // Default settings
            External = 0x00000001,              // Include externally included files when possible
            CopyInfs = 0x00000002,              // Include files referenced by copy INF directives
            SkipExistingCopyInfs = 0x00000004,  // Skip copy INFs that already exist in driver store
            SystemDefaultLocale = 0x00000008,   // Only copy files for system default locale
            Hardlink = 0x00000010,              // Hardlink files instead of copying them
        }

        [Flags]
        internal enum DriverStoreEnumFlags : uint
        {
            None = 0x00000000,                  // Default settings
            InboxOnly = 0x00000001,             // Enumerate only inbox driver packages
            OemOnly = 0x00000002,               // Enumerate only OEM driver packages
            PublishedOnly = 0x00000004,         // Enumerate only published driver packages
            Valid = InboxOnly | OemOnly | PublishedOnly,
        }

        [Flags]
        public enum DriverPackageFlags
        {
            None = 0x00000000,
            Inbox = 0x00000001,
            Oem = 0x00000002,
            Published = 0x00000004,
            F6 = 0x00000008,
            BaseVersion = 0x00000010
        }

        public enum DriverStoreObjectType
        {
            DriverDatabase = 0x00000001,
            DriverPackage = 0x00000002,
            DriverInfFile = 0x00000003,
            DriverFile,
            DeviceId,
            DeviceSetupClass,
            DeviceNode,
            DeviceInterfaceClass,
            DeviceInterface,
            DeviceContainer,
            DriverService,
            DriverRegKey,
            DevicePanel,
        }

        internal enum DriverFileOperation : uint
        {
            Copy = 0,
            Delete,
            Rename
        }

        internal enum DriverFileType : uint
        {
            Inf = 0,
            Catalog,
            Binary,
            CopyInf,
            IncludeInf
        }

        [Flags]
        public enum DriverPackageGetPropertyFlags
        {
            None = 0x00000000,                  // Default settings
        }

        [Flags]
        public enum DriverPackageVersionInfoFlags
        {
            None = 0x00000000,
            HAS_DEVICE_DRIVERS = 0x00000001,    // Driver package has device drivers
            PNP_LOCKDOWN = 0x00000002,   // Driver package has PnP lockdown enabled
            FORCE_BOOT_CRITICAL = 0x00000004,   // Force driver package to be boot critical
            FORCE_NOT_BOOT_CRITICAL = 0x00000008,   // Force driver package to NOT be boot critical
            HAS_DEVICE_CLASSES = 0x00000010,    // Driver package has device classes
            PRE_CONFIGURABLE = 0x00000020,    // Driver package has pre-configurable drivers
            HAS_DEVICES = 0x00000040,    // Driver package has devices
            HAS_INTERFACE_CLASSES = 0x00000080,    // Driver package has device interface classes
            HAS_PRIMITIVE_DRIVERS = 0x00000100,    // Driver package has primitive drivers
            HAS_DRIVERS = HAS_DEVICE_DRIVERS | HAS_PRIMITIVE_DRIVERS,
            HAS_CLASSES = HAS_DEVICE_CLASSES | HAS_INTERFACE_CLASSES,
        }

        //
        // Driver Store Update Devices API
        //
        [Flags]
        public enum DriverStoreUpdateDevicesFlags
        {
            FORCE = 0x00000001,    // Force update of devices with applicable driver
            NULL_DRIVER = 0x00000002,    // Update devices with NULL driver
            ALTERNATIVE_DRIVER = 0x00000004,    // Update devices with next best alternative driver
            SOURCE_CONFIGURATIONS = 0x00010000,    // Source filter supplies configurations
            SOURCE_DEVICE_IDS = 0x00020000,    // Source filter supplies device IDs
            SOURCE_DRIVER_NAMES = 0x00040000,    // Source filter supplies driver node names
            TARGET_INSTANCE_IDS = 0x00100000,    // Target filter supplies device instance IDs
            TARGET_DEVICE_IDS = 0x00200000,    // Target filter supplies device IDs
            VALID = FORCE
                | NULL_DRIVER
                | ALTERNATIVE_DRIVER
                | SOURCE_CONFIGURATIONS
                | SOURCE_DEVICE_IDS
                | SOURCE_DRIVER_NAMES
                | TARGET_INSTANCE_IDS
                | TARGET_DEVICE_IDS
        }

        //
        // Driver Store Delete API
        //
        [Flags]
        public enum DriverStoreDeleteFlags
        {
            INBOX = 0x00000001,    // Inbox driver package
            UNCONFIGURE = 0x00000002,    // Unconfigure driver package
            UNCONFIGURE_ONLY = 0x00000004,    // Unconfigure driver package only, without deleting it
            UNCONFIGURE_PRESERVE_STATE = 0x00010000,    // Preserve global state when unconfiguring, used with UNCONFIGURE flag only
            UNCONFIGURE_VALID = UNCONFIGURE_PRESERVE_STATE,
            VALID = INBOX
                | UNCONFIGURE
                | UNCONFIGURE_ONLY
                | UNCONFIGURE_VALID
        }

        public enum DRIVERSTORE_LOCK_LEVEL
        {
            NONE = 0,
            BASIC_PROTECTED,
            RUNTIME_ISOLATED,
            SYSTEM_PROTECTED,
            MAX,
        };

        #endregion Enums

        #region Structs
        /// <summary>
        ///  The DriverFile struct returned by DriverPackageEnumFilesW.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DriverFile
        {
            internal DriverFileOperation Operation;
            internal string ExternalFile;
            internal DriverFileType Type;
            internal uint Flags;
            internal string SourceFile;
            internal string SourcePath;
            internal string DestinationFile;
            internal string DestinationPath;
            internal string ArchiveFile;
            internal string SecurityDescriptor;
            internal string SectionName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DriverPackageInfo
        {
            public ProcessorArchitecture ProcessorArchitecture;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LOCALE_NAME_MAX_LENGTH)] public string LocaleName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string PublishedInfName;
            public DriverPackageFlags Flags;
        };

        /// <summary>
        ///  The struct returned by DriverPackageGetVersionInfo.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DriverPackageVersionInfo
        {
            public uint Size;
            public ProcessorArchitecture Architecture;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LOCALE_NAME_MAX_LENGTH)] public string LocaleName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string ProviderName;
            public System.Runtime.InteropServices.ComTypes.FILETIME DriverDate;
            public ulong DriverVersion;
            public Guid ClassGuid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string ClassName;
            public uint ClassVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string CatalogFile;
            public DriverPackageVersionInfoFlags Flags;
        }

        #endregion Structs

        /// <summary>
        /// The managed interop layer to drvstore.dll
        /// </summary>
        internal static class NativeMethods
        {
            /// <summary>
            /// Open the driver store.
            /// </summary>
            /// <param name="targetSystemPath">The path to the "windows" directory on the image.</param>
            /// <param name="targetBootDrive">The path to the boot drive on the image.</param>
            /// <param name="flags">Flags to Open the driver store.</param>
            /// <param name="transactionHandle">transaction handle</param>
            /// <returns>Handle to the driver store on success. IntPtr.Zero on failure.</returns>
            [DllImport("drvstore.dll", EntryPoint = "DriverStoreOpenW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr DriverStoreOpen(
                string targetSystemPath,
                string targetBootDrive,
                DriverStoreOpenFlags flags,
                IntPtr transactionHandle);

            /// <summary>
            /// Close the driver store.
            /// </summary>
            /// <param name="driverStoreHandle">handle to the driver store.</param>
            /// <returns>True on success. False on failure.</returns>
            [DllImport("drvstore.dll", SetLastError = true)]
            internal static extern bool DriverStoreClose(
                 IntPtr driverStoreHandle);

            public delegate bool EnumDriverPackageDelegate(
                IntPtr driverStoreHandle,
                [MarshalAs(UnmanagedType.LPWStr, SizeConst = 256)]
                string driverStoreFilename,
                DriverPackageInfo driverPackageInfo,
                IntPtr lParam);

            [DllImport("drvstore.dll", EntryPoint = "DriverStoreEnumW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool DriverStoreEnum(
                IntPtr driverStoreHandle,
                DriverStoreEnumFlags flags,
                EnumDriverPackageDelegate CallbackRoutine,
                IntPtr lParam);

            public delegate bool EnumObjectsDelegate(
                IntPtr hDriverStore,
                DriverStoreObjectType ObjectType,
                [MarshalAs(UnmanagedType.LPWStr, SizeConst = MAX_PATH)] string objectName,
                IntPtr lParam);

            [DllImport("drvstore.dll", EntryPoint = "DriverStoreEnumObjectsW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool DriverStoreEnumObjects(
                IntPtr hDriverStore,
                DriverStoreObjectType objectType,
                DRIVERSTORE_LOCK_LEVEL flags,
                EnumObjectsDelegate callbackRoutine,
                IntPtr lParam
            );

            /// <summary>
            /// Import the driver package to the driver store.
            /// </summary>
            /// <param name="driverStoreHandle">handle to the driver store.</param>
            /// <param name="driverPackageFileName">the name of the driver package file.</param>
            /// <param name="processorArchitecture">the processor architecture. </param>
            /// <param name="localeName">the loacle for the package.</param>
            /// <param name="flags">the flags for import.</param>
            /// <param name="driverStoreFileName">the driver store file name buffer.</param>
            /// <param name="driverStoreFileNameSize">the driver store file name size.</param>
            /// <returns>Result code for operation.</returns>
            [DllImport("drvstore.dll", EntryPoint = "DriverStoreImportW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint DriverStoreImport(
                IntPtr driverStoreHandle,
                string driverPackageFileName,
                ProcessorArchitecture processorArchitecture,
                string localeName,
                DriverStoreImportFlags flags,
                StringBuilder driverStoreFileName,
                int driverStoreFileNameSize);

            [DllImport("drvstore.dll", EntryPoint = "DriverStoreOfflineAddDriverPackageW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint DriverStoreOfflineAddDriverPackage(
                string DriverPackageInfPath,
                DriverStoreOfflineAddDriverPackageFlags Flags,
                IntPtr Reserved,
                ushort ProcessorArchitecture,
                string LocaleName,
                StringBuilder DestInfPath,
                ref int cchDestInfPath,
                string TargetSystemRoot,
                string TargetSystemDrive);

            [DllImport("drvstore.dll", EntryPoint = "DriverStoreUpdateDevicesW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint DriverStoreUpdateDevices(
                IntPtr hDriverStore,
                string driverStoreFilename,
                DriverStoreUpdateDevicesFlags flags,
                string sourceFilter,
                string targetFilter);


            [DllImport("drvstore.dll", EntryPoint = "DriverStoreDeleteW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint DriverStoreDelete(
                IntPtr hDriverStore,
                string driverStoreFilename,
                DriverStoreDeleteFlags flags);

            [DllImport("drvstore.dll", EntryPoint = "DriverStoreConfigureW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint DriverStoreConfigure(
                IntPtr hDriverStore,
                string DriverStoreFilename,
                DriverStoreConfigureFlags Flags,
                string SourceFilter,
                string TargetFilter);

            /// <summary>
            /// Reflect the driver and pre-configure it in the driver database.
            /// </summary>
            /// <param name="driverStoreHandle">the handle to the driver store.</param>
            /// <param name="driverStoreFileName">the name of the driver package name in file store.</param>
            /// <param name="flag">the flags for driver reflection.</param>
            /// <param name="filterDeviceId">optional list of device IDs to filter by.</param>
            /// <returns>Result code for operation.</returns>
            [DllImport("drvstore.dll", EntryPoint = "DriverStoreReflectCriticalW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint DriverStoreReflectCritical(
                IntPtr driverStoreHandle,
                string driverStoreFileName,
                DriverStoreReflectCriticalFlags flag,
                string filterDeviceId);

            /// <summary>
            /// Reflect the driver and pre-configure it in the driver database.
            /// </summary>
            /// <param name="driverStoreHandle">the handle to the driver store.</param>
            /// <param name="driverStoreFileName">the name of the driver package name in file store.</param>
            /// <param name="flag">the flags for driver reflection.</param>
            /// <param name="filterDeviceId">optional list of device IDs to filter by.</param>
            /// <returns>Result code for operation.</returns>
            [DllImport("drvstore.dll", EntryPoint = "DriverStoreReflectW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint DriverStoreReflect(
                IntPtr driverStoreHandle,
                string driverStoreFileName,
                DriverStoreReflectFlags flag,
                string filterSectionNames);

            [DllImport("drvstore.dll", EntryPoint = "DriverStorePublishW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint DriverStorePublish(
                IntPtr driverStoreHandle,
                string driverStoreFileName,
                DriverStorePublishFlags flag,
                StringBuilder publishedFileName,
                int publishedFileNameSize,
                ref bool isPublishedFileNameChanged);

            /// <summary>
            /// Set driver store property for a specified object type.
            /// </summary>
            /// <param name="driverStoreHandle">the handle to the driver store.</param>
            /// <param name="objectType">driver store object type.</param>
            /// <param name="objectName">driver store object name corresponding to object type.</param>
            /// <param name="propertyKey">property key to set.</param>
            /// <param name="propertyType">property type corresponding to property key.</param>
            /// <param name="propertyBuffer">buffer containing property data to set.</param>
            /// <param name="propertySize">size of property data in buffer.</param>
            /// <param name="flag">set property flags.</param>
            [DllImport("drvstore.dll", EntryPoint = "DriverStoreSetObjectPropertyW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool DriverStoreSetObjectProperty(
                IntPtr driverStoreHandle,
                DriverStoreObjectType objectType,
                string objectName,
                ref DevPropKey propertyKey,
                DevPropType propertyType,
                ref uint propertyBuffer,
                int propertySize,
                DriverStoreSetObjectPropertyFlags flag);

            [DllImport("drvstore.dll", EntryPoint = "DriverStoreGetObjectPropertyW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool DriverStoreGetObjectProperty(
                IntPtr driverStoreHandle,
                DriverStoreObjectType objectType,
                string objectName,
                ref DevPropKey propertyKey,
                out DevPropType propertyType,
                IntPtr propertyBuffer,
                int bufferSize,
                out uint propertySize,
                DriverStoreSetObjectPropertyFlags flag);

            public delegate bool EnumFilesDelegate(IntPtr driverPackageHandle, IntPtr pDriverFile, IntPtr lParam);

            [DllImport("drvstore.dll", EntryPoint = "DriverPackageEnumFilesW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool DriverPackageEnumFilesW(
                IntPtr driverPackageHandle,
                IntPtr enumContext,
                DriverPackageEnumFilesFlags flags,
                EnumFilesDelegate callbackRoutine,
                IntPtr lParam);

            [DllImport("drvstore.dll", EntryPoint = "DriverPackageOpenW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr DriverPackageOpen(
                string driverPackageFilename,
                ProcessorArchitecture processorArchitecture,
                string localeName,
                DriverPackageOpenFlags flags,
                IntPtr resolveContext);

            [DllImport("drvstore.dll", EntryPoint = "DriverPackageGetVersionInfoW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool DriverPackageGetVersionInfo(
                IntPtr driverPackageHandle,
                IntPtr pVersionInfo);

            // Internal struct so marshaller can get size
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct DevPropTypeInternal
            {
                internal DevPropType PropType;
            };

            [DllImport("drvstore.dll", EntryPoint = "DriverPackageGetPropertyW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool DriverPackageGetProperty(
                IntPtr driverPackageHandle,
                IntPtr enumContext,
                string sectionName,
                IntPtr propertyKey,
                IntPtr propertyType,
                IntPtr propertyBuffer,
                uint bufferSize,
                IntPtr propertySize,
                DriverPackageGetPropertyFlags flags);

            [DllImport("drvstore.dll", EntryPoint = "DriverPackageClose", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern void DriverPackageClose(
                IntPtr driverPackageHandle);

            [DllImport("drvstore.dll", EntryPoint = "DriverStoreCopyW", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern uint DriverStoreCopy(
                IntPtr driverPackageHandle,
                string driverPackageFilename,
                ProcessorArchitecture processorArchitecture,
                IntPtr localeName,
                DriverStoreCopyFlags flags,
                string destinationPath);
        }
    }
}
