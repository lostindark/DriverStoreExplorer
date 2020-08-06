using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Win32;

using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Rapr.Utils
{
    public static class SetupAPI
    {
        public static List<DeviceDriverInfo> GetDeviceDriverInfo()
        {
            List<DeviceDriverInfo> deviceDriverInfos = new List<DeviceDriverInfo>();

            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(
                IntPtr.Zero,
                null,
                IntPtr.Zero,
                DIGCF.ALLCLASSES);

            if (deviceInfoSet == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            try
            {
                SP_DEVINFO_DATA deviceInfo = new SP_DEVINFO_DATA();
                deviceInfo.cbSize = Marshal.SizeOf(deviceInfo);

                for (int i = 0; NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfo); i++)
                {
                    try
                    {
                        deviceDriverInfos.Add(new DeviceDriverInfo
                        (
                            TryGetDeviceRegistryProperty(deviceInfoSet, deviceInfo, DeviceRegistryProperty.SPDRP_FRIENDLYNAME)
                                ?? TryGetDeviceRegistryProperty(deviceInfoSet, deviceInfo, DeviceRegistryProperty.SPDRP_DEVICEDESC),
                            GetDriverInf(deviceInfoSet, deviceInfo),
                            TryGetDevicePropertyInfo<DateTime>(deviceInfoSet, deviceInfo, DeviceHelper.DEVPKEY_Device_DriverDate),
                            TryGetDevicePropertyInfo<Version>(deviceInfoSet, deviceInfo, DeviceHelper.DEVPKEY_Device_DriverVersion),
                            TryGetDevicePropertyInfo<bool?>(deviceInfoSet, deviceInfo, DeviceHelper.DEVPKEY_Device_IsPresent)
                        ));
                    }
                    catch (Win32Exception)
                    {
                    }
                }

                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != ERROR_NO_MORE_ITEMS)
                {
                    throw new Win32Exception(lastWin32Error);
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return deviceDriverInfos;
        }

        public static string GetDriverSignerInfo(string driverStoreFilename)
        {
            const int ERROR_AUTHENTICODE_TRUSTED_PUBLISHER = unchecked((int)0xe0000241);
            const int ERROR_AUTHENTICODE_TRUST_NOT_ESTABLISHED = unchecked((int)0xe0000242);

            SP_INF_SIGNER_INFO signerInfo = new SP_INF_SIGNER_INFO
            {
                cbSize = Marshal.SizeOf(typeof(SP_INF_SIGNER_INFO))
            };

            if ((NativeMethods.SetupVerifyInfFile(driverStoreFilename, IntPtr.Zero, ref signerInfo)
                || Marshal.GetLastWin32Error() == ERROR_AUTHENTICODE_TRUSTED_PUBLISHER
                || Marshal.GetLastWin32Error() == ERROR_AUTHENTICODE_TRUST_NOT_ESTABLISHED)
                && !string.IsNullOrEmpty(signerInfo.DigitalSigner))
            {
                return signerInfo.DigitalSigner;
            }
            else
            {
                return string.Empty;
            }
        }

        public static void AddDriver(string infFullPath, bool install)
        {
            bool result;

            if (install)
            {
                result = NativeMethods.DiInstallDriver(IntPtr.Zero, infFullPath, 0, out _);
            }
            else
            {
                StringBuilder destInfFullPath = new StringBuilder(MAX_PATH);
                uint requiredSize = 0;

                result = NativeMethods.SetupCopyOEMInf(
                    infFullPath,
                    null,
                    OemSourceMediaType.SPOST_PATH,
                    OemCopyStyle.NONE,
                    destInfFullPath,
                    (uint)destInfFullPath.Capacity,
                    ref requiredSize,
                    IntPtr.Zero);
            }

            if (!result)
            {
                throw new Win32Exception();
            }
        }

        public static void DeleteDriver(DriverStoreEntry driverStoreEntry, bool forceDelete)
        {
            if (driverStoreEntry == null)
            {
                throw new ArgumentNullException(nameof(driverStoreEntry));
            }

            if (forceDelete
                && MethodExists("newdev.dll", "DiUninstallDriverW")
                && !NativeMethods.DiUninstallDriver(
                        IntPtr.Zero,
                        Path.Combine(driverStoreEntry.DriverFolderLocation, driverStoreEntry.DriverInfName),
                        DIURFLAG.NO_REMOVE_INF,
                        out _))
            {
                throw new Win32Exception();
            }

            if (!NativeMethods.SetupUninstallOEMInf(
                driverStoreEntry.DriverPublishedName,
                forceDelete ? SetupUOInfFlags.SUOI_FORCEDELETE : SetupUOInfFlags.NONE,
                IntPtr.Zero))
            {
                throw new Win32Exception();
            }
        }

        public static bool MethodExists(string libraryName, string methodName)
        {
            var libraryPtr = NativeMethods.LoadLibrary(libraryName);

            if (libraryPtr != UIntPtr.Zero)
            {
                var procPtr = NativeMethods.GetProcAddress(libraryPtr, methodName);
                return procPtr != UIntPtr.Zero;
            }

            return false;
        }

        private static string GetDriverInf(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfo)
        {
            // Get Currently Installed Driver.
            SP_DEVINSTALL_PARAMS deviceInstallParams = new SP_DEVINSTALL_PARAMS
            {
                cbSize = Marshal.SizeOf(typeof(SP_DEVINSTALL_PARAMS)),
                FlagsEx = DI_FLAGS.ALLOWEXCLUDEDDRVS | DI_FLAGS.INSTALLEDDRIVER
            };

            if (!NativeMethods.SetupDiSetDeviceInstallParams(deviceInfoSet, ref deviceInfo, ref deviceInstallParams))
            {
                throw new Win32Exception();
            }

            if (!NativeMethods.SetupDiBuildDriverInfoList(deviceInfoSet, ref deviceInfo, SPDIT.COMPATDRIVER))
            {
                throw new Win32Exception();
            }

            try
            {
                if (Environment.Is64BitProcess)
                {
                    SP_DRVINFO_DATA drvInfo = new SP_DRVINFO_DATA
                    {
                        cbSize = Marshal.SizeOf(typeof(SP_DRVINFO_DATA))
                    };

                    if (NativeMethods.SetupDiEnumDriverInfo(deviceInfoSet, ref deviceInfo, SPDIT.COMPATDRIVER, 0, ref drvInfo))
                    {
                        SP_DRVINFO_DETAIL_DATA driverInfoDetailData = new SP_DRVINFO_DETAIL_DATA
                        {
                            cbSize = Marshal.SizeOf(typeof(SP_DRVINFO_DETAIL_DATA))
                        };

                        if (NativeMethods.SetupDiGetDriverInfoDetail(deviceInfoSet, ref deviceInfo, ref drvInfo, ref driverInfoDetailData, Marshal.SizeOf(driverInfoDetailData), out _)
                            || Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
                        {
                            return driverInfoDetailData.InfFileName;
                        }
                        else
                        {
                            throw new Win32Exception();
                        }
                    }
                    else
                    {
                        int lastWin32Error = Marshal.GetLastWin32Error();
                        if (lastWin32Error != ERROR_NO_MORE_ITEMS)
                        {
                            throw new Win32Exception(lastWin32Error);
                        }
                    }
                }
                else
                {
                    SP_DRVINFO_DATA32 drvInfo = new SP_DRVINFO_DATA32
                    {
                        cbSize = Marshal.SizeOf(typeof(SP_DRVINFO_DATA32))
                    };

                    if (NativeMethods.SetupDiEnumDriverInfo32(deviceInfoSet, ref deviceInfo, SPDIT.COMPATDRIVER, 0, ref drvInfo))
                    {
                        SP_DRVINFO_DETAIL_DATA32 driverInfoDetailData = new SP_DRVINFO_DETAIL_DATA32
                        {
                            cbSize = Marshal.SizeOf(typeof(SP_DRVINFO_DETAIL_DATA32))
                        };

                        if (NativeMethods.SetupDiGetDriverInfoDetail32(deviceInfoSet, ref deviceInfo, ref drvInfo, ref driverInfoDetailData, Marshal.SizeOf(driverInfoDetailData), out _)
                            || Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
                        {
                            return driverInfoDetailData.InfFileName;
                        }
                        else
                        {
                            throw new Win32Exception();
                        }
                    }
                    else
                    {
                        int lastWin32Error = Marshal.GetLastWin32Error();
                        if (lastWin32Error != ERROR_NO_MORE_ITEMS)
                        {
                            throw new Win32Exception(lastWin32Error);
                        }
                    }
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDriverInfoList(deviceInfoSet, ref deviceInfo, SPDIT.COMPATDRIVER);
            }

            return null;
        }

        private static string TryGetDeviceRegistryProperty(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfo, DeviceRegistryProperty property)
        {
            try
            {
                return GetDeviceRegistryProperty(deviceInfoSet, deviceInfo, property);
            }
            catch (Win32Exception)
            {
            }

            return null;
        }

        private static string GetDeviceRegistryProperty(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfo, DeviceRegistryProperty property)
        {
            const int BufferSize = 2048;
            IntPtr propertyBufferPtr = Marshal.AllocHGlobal(BufferSize);

            try
            {
                if (NativeMethods.SetupDiGetDeviceRegistryProperty(
                    deviceInfoSet,
                    ref deviceInfo,
                    property,
                    out RegistryValueKind valueKind,
                    propertyBufferPtr,
                    BufferSize,
                    out uint propertySize))
                {
                    if (propertySize > 0 && valueKind == RegistryValueKind.String)
                    {
                        return Marshal.PtrToStringUni(propertyBufferPtr);
                    }
                }
                else
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(propertyBufferPtr);
            }

            return null;
        }

        private static T TryGetDevicePropertyInfo<T>(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfo, DevPropKey propertyKey)
        {
            try
            {
                return GetDevicePropertyInfo<T>(deviceInfoSet, deviceInfo, propertyKey);
            }
            catch (Win32Exception)
            {
            }

            return default(T);
        }

        private static T GetDevicePropertyInfo<T>(
            IntPtr deviceInfoSet,
            SP_DEVINFO_DATA deviceInfo,
            DevPropKey propertyKey)
        {
            const int BufferSize = 2048;
            IntPtr propertyBufferPtr = Marshal.AllocHGlobal(BufferSize);

            try
            {
                if (NativeMethods.SetupDiGetDeviceProperty(
                    deviceInfoSet,
                    ref deviceInfo,
                    propertyKey,
                    out DevPropType propertyType,
                    propertyBufferPtr,
                    BufferSize,
                    out uint propertySize,
                    0))
                {
                    if (propertySize > 0)
                    {
                        return DeviceHelper.ConvertPropToType<T>(propertyBufferPtr, propertyType);
                    }
                }
                else
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(propertyBufferPtr);
            }

            return default(T);
        }

        internal const int LINE_LEN = 256;
        internal const int MAX_PATH = 260;

        internal const int ERROR_INSUFFICIENT_BUFFER = 122;
        internal const int ERROR_NO_MORE_ITEMS = 259;

        //
        // Flags controlling what is included in the device information set built
        // by SetupDiGetClassDevs
        //
        internal enum DIGCF
        {
            DEFAULT = 0x00000001,  // only valid with DIGCF_DEVICEINTERFACE
            PRESENT = 0x00000002,
            ALLCLASSES = 0x00000004,
            PROFILE = 0x00000008,
            DEVICEINTERFACE = 0x00000010,
        }

        //
        // Ordinal values distinguishing between class drivers and
        // device drivers.
        // (Passed in 'DriverType' parameter of driver information list APIs)
        //
        internal enum SPDIT
        {
            NODRIVER = 0x00000000,
            CLASSDRIVER = 0x00000001,
            COMPATDRIVER = 0x00000002,
        }

        /// <summary>
        /// Flags for SetupDiGetDeviceRegistryProperty().
        /// </summary>
        internal enum DeviceRegistryProperty : uint
        {
            SPDRP_DEVICEDESC = 0x00000000, // DeviceDesc (R/W)
            SPDRP_HARDWAREID = 0x00000001, // HardwareID (R/W)
            SPDRP_COMPATIBLEIDS = 0x00000002, // CompatibleIDs (R/W)
            SPDRP_UNUSED0 = 0x00000003, // unused
            SPDRP_SERVICE = 0x00000004, // Service (R/W)
            SPDRP_UNUSED1 = 0x00000005, // unused
            SPDRP_UNUSED2 = 0x00000006, // unused
            SPDRP_CLASS = 0x00000007, // Class (R--tied to ClassGUID)
            SPDRP_CLASSGUID = 0x00000008, // ClassGUID (R/W)
            SPDRP_DRIVER = 0x00000009, // Driver (R/W)
            SPDRP_CONFIGFLAGS = 0x0000000A, // ConfigFlags (R/W)
            SPDRP_MFG = 0x0000000B, // Mfg (R/W)
            SPDRP_FRIENDLYNAME = 0x0000000C, // FriendlyName (R/W)
            SPDRP_LOCATION_INFORMATION = 0x0000000D, // LocationInformation (R/W)
            SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E, // PhysicalDeviceObjectName (R)
            SPDRP_CAPABILITIES = 0x0000000F, // Capabilities (R)
            SPDRP_UI_NUMBER = 0x00000010, // UiNumber (R)
            SPDRP_UPPERFILTERS = 0x00000011, // UpperFilters (R/W)
            SPDRP_LOWERFILTERS = 0x00000012, // LowerFilters (R/W)
            SPDRP_BUSTYPEGUID = 0x00000013, // BusTypeGUID (R)
            SPDRP_LEGACYBUSTYPE = 0x00000014, // LegacyBusType (R)
            SPDRP_BUSNUMBER = 0x00000015, // BusNumber (R)
            SPDRP_ENUMERATOR_NAME = 0x00000016, // Enumerator Name (R)
            SPDRP_SECURITY = 0x00000017, // Security (R/W, binary form)
            SPDRP_SECURITY_SDS = 0x00000018, // Security (W, SDS form)
            SPDRP_DEVTYPE = 0x00000019, // Device Type (R/W)
            SPDRP_EXCLUSIVE = 0x0000001A, // Device is exclusive-access (R/W)
            SPDRP_CHARACTERISTICS = 0x0000001B, // Device Characteristics (R/W)
            SPDRP_ADDRESS = 0x0000001C, // Device Address (R)
            SPDRP_UI_NUMBER_DESC_FORMAT = 0X0000001D, // UiNumberDescFormat (R/W)
            SPDRP_DEVICE_POWER_DATA = 0x0000001E, // Device Power Data (R)
            SPDRP_REMOVAL_POLICY = 0x0000001F, // Removal Policy (R)
            SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000020, // Hardware Removal Policy (R)
            SPDRP_REMOVAL_POLICY_OVERRIDE = 0x00000021, // Removal Policy Override (RW)
            SPDRP_INSTALL_STATE = 0x00000022, // Device Install State (R)
            SPDRP_LOCATION_PATHS = 0x00000023, // Device Location Paths (R)
            SPDRP_BASE_CONTAINERID = 0x00000024  // Base ContainerID (R)
        }

        [Flags]
        internal enum DI_FLAGS
        {
            INSTALLEDDRIVER = (0x04000000),
            ALLOWEXCLUDEDDRVS = (0x00000800)
        }

        /// <summary>
        /// Flags for DiUninstallDriver
        /// </summary>
        [Flags]
        internal enum DIURFLAG
        {
            NO_REMOVE_INF = 0x00000001, // Do not remove inf from the system
            UNCONFIGURE_INF = 0x00000002, // Unconfigure inf, if possible
        }

        [Flags]
        internal enum SetupUOInfFlags : uint
        {
            NONE = 0x0000,
            SUOI_FORCEDELETE = 0x0001
        };

        /// <summary>
        /// Driver media type
        /// </summary>
        internal enum OemSourceMediaType
        {
            SPOST_NONE = 0,
            SPOST_PATH = 1,
            SPOST_URL = 2,
            SPOST_MAX = 3
        }

        /// <summary>
        /// Driver file copy style
        /// </summary>
        internal enum OemCopyStyle
        {
            NONE = 0x0,
            SP_COPY_DELETESOURCE = 0x0000001,   // delete source file on successful copy
            SP_COPY_REPLACEONLY = 0x0000002,   // copy only if target file already present
            SP_COPY_NEWER = 0x0000004,   // copy only if source newer than or same as target
            SP_COPY_NEWER_OR_SAME = SP_COPY_NEWER,
            SP_COPY_NOOVERWRITE = 0x0000008,   // copy only if target doesn't exist
            SP_COPY_NODECOMP = 0x0000010,   // don't decompress source file while copying
            SP_COPY_LANGUAGEAWARE = 0x0000020,   // don't overwrite file of different language
            SP_COPY_SOURCE_ABSOLUTE = 0x0000040,   // SourceFile is a full source path
            SP_COPY_SOURCEPATH_ABSOLUTE = 0x0000080,   // SourcePathRoot is the full path
            SP_COPY_IN_USE_NEEDS_REBOOT = 0x0000100,   // System needs reboot if file in use
            SP_COPY_FORCE_IN_USE = 0x0000200,   // Force target-in-use behavior
            SP_COPY_NOSKIP = 0x0000400,   // Skip is disallowed for this file or section
            SP_FLAG_CABINETCONTINUATION = 0x0000800,   // Used with need media notification
            SP_COPY_FORCE_NOOVERWRITE = 0x0001000,   // like NOOVERWRITE but no callback nofitication
            SP_COPY_FORCE_NEWER = 0x0002000,   // like NEWER but no callback nofitication
            SP_COPY_WARNIFSKIP = 0x0004000,   // system critical file: warn if user tries to skip
            SP_COPY_NOBROWSE = 0x0008000,   // Browsing is disallowed for this file or section
            SP_COPY_NEWER_ONLY = 0x0010000,   // copy only if source file newer than target
            SP_COPY_SOURCE_SIS_MASTER = 0x0020000,   // source is single-instance store master
            SP_COPY_OEMINF_CATALOG_ONLY = 0x0040000,   // (SetupCopyOEMInf only) don't copy INF--just catalog
            SP_COPY_REPLACE_BOOT_FILE = 0x0080000,   // file must be present upon reboot (i.e., it's needed by the loader), this flag implies a reboot
            SP_COPY_NOPRUNE = 0x0100000   // never prune this file
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SP_DEVINSTALL_PARAMS
        {
            public int cbSize;
            public int Flags;
            public DI_FLAGS FlagsEx;
            public IntPtr hwndParent;
            public IntPtr InstallMsgHandler;
            public IntPtr InstallMsgHandlerContext;
            public IntPtr FileQueue;
            public UIntPtr ClassInstallReserved;
            public int Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string DriverPath;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SP_DRVINFO_DATA
        {
            public int cbSize;
            public int DriverType;
            private IntPtr Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string Description;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string MfgName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string ProviderName;
            public FILETIME DriverDate;
            public long DriverVersion;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SP_DRVINFO_DETAIL_DATA
        {
            public int cbSize;
            public FILETIME InfDate;
            public int CompatIDsOffset;
            public int CompatIDsLength;
            public IntPtr Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string SectionName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string InfFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string DrvDescription;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)] public string HardwareID;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        internal struct SP_DRVINFO_DATA32
        {
            public int cbSize;
            public int DriverType;
            private IntPtr Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string Description;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string MfgName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string ProviderName;
            public FILETIME DriverDate;
            public long DriverVersion;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        internal struct SP_DRVINFO_DETAIL_DATA32
        {
            public int cbSize;
            public FILETIME InfDate;
            public int CompatIDsOffset;
            public int CompatIDsLength;
            public IntPtr Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string SectionName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string InfFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LINE_LEN)] public string DrvDescription;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)] public string HardwareID;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            private IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SP_INF_SIGNER_INFO
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string CatalogFile;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string DigitalSigner;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string DigitalSignerVersion;
            public int SignerScore;
        };

        /// <summary>
        /// The managed interop layer to setupapi.dll
        /// </summary>
        internal static class NativeMethods
        {
            [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern UIntPtr LoadLibrary(string lpFileName);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "GetProcAddress only comes in an ANSI flavor.")]
            [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
            internal static extern UIntPtr GetProcAddress(UIntPtr hModule, string lpProcName);

            [DllImport("newdev.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool DiInstallDriver(
                [In] IntPtr hwndParent,
                [In] string infPath,
                [In] uint flags,
                [Out] out bool needReboot
            );

            [DllImport("newdev.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool DiUninstallDriver(
                [In] IntPtr hwndParent,
                [In] string infPath,
                [In] DIURFLAG flags,
                [Out] out bool needReboot
            );

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern IntPtr SetupDiGetClassDevs(
               IntPtr classGuid,
               string enumerator,
               IntPtr hwndParent,
               DIGCF flags);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupDiEnumDeviceInfo(
                IntPtr deviceInfoSet,
                int memberIndex,
                ref SP_DEVINFO_DATA deviceInfoData);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupDiSetDeviceInstallParams(
                IntPtr deviceInfoSet,
                ref SP_DEVINFO_DATA deviceInfoData,
                ref SP_DEVINSTALL_PARAMS deviceInstallParams);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetupDiEnumDriverInfo(
                IntPtr lpInfoSet,
                ref SP_DEVINFO_DATA deviceInfoData,
                SPDIT driverType,
                int memberIndex,
                ref SP_DRVINFO_DATA driverInfoData);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupDiGetDriverInfoDetail(
               IntPtr deviceInfoSet,
               ref SP_DEVINFO_DATA deviceInfoData,
               ref SP_DRVINFO_DATA driverInfoData,
               ref SP_DRVINFO_DETAIL_DATA driverInfoDetailData,
               int driverInfoDetailDataSize,
               out int requiredSize);

            [DllImport("setupapi.dll", EntryPoint = "SetupDiEnumDriverInfo", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetupDiEnumDriverInfo32(
                IntPtr lpInfoSet,
                ref SP_DEVINFO_DATA deviceInfoData,
                SPDIT driverType,
                int memberIndex,
                ref SP_DRVINFO_DATA32 driverInfoData);

            [DllImport("setupapi.dll", EntryPoint = "SetupDiGetDriverInfoDetail", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupDiGetDriverInfoDetail32(
               IntPtr deviceInfoSet,
               ref SP_DEVINFO_DATA deviceInfoData,
               ref SP_DRVINFO_DATA32 driverInfoData,
               ref SP_DRVINFO_DETAIL_DATA32 driverInfoDetailData,
               int driverInfoDetailDataSize,
               out int requiredSize);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupDiGetDeviceRegistryProperty(
                IntPtr deviceInfoSet,
                ref SP_DEVINFO_DATA deviceInfoData,
                DeviceRegistryProperty property,
                out RegistryValueKind dataType,
                IntPtr propertyBuffer,
                uint propertyBufferSize,
                out uint requiredSize);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupDiGetDeviceProperty(
                IntPtr deviceInfoSet,
                ref SP_DEVINFO_DATA deviceInfoData,
                DevPropKey propertyKey,
                out DevPropType propertyType,
                IntPtr propertyBuffer,
                int propertyBufferSize,
                out uint requiredSize,
                uint flags);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupDiBuildDriverInfoList(
                IntPtr deviceInfoSet,
                ref SP_DEVINFO_DATA deviceInfoData,
                SPDIT driverType);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupDiDestroyDriverInfoList(
                IntPtr deviceInfoSet,
                ref SP_DEVINFO_DATA deviceInfoData,
                SPDIT driverType);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool SetupVerifyInfFile(
                string infName,
                IntPtr altPlatformInfo,
                ref SP_INF_SIGNER_INFO infSignerInfo);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetupCopyOEMInf(
                [MarshalAs(UnmanagedType.LPWStr)] string infName,
                [MarshalAs(UnmanagedType.LPWStr)] string sourceMediaLocation,
                OemSourceMediaType sourceMediaType,
                OemCopyStyle copyStyle,
                [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder destInfFullPath,
                uint destInfFullPathSize,
                ref uint desInfFullPathReqSize,
                IntPtr destInfFilename);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetupUninstallOEMInf(
                [MarshalAs(UnmanagedType.LPWStr)] string infName,
                SetupUOInfFlags flags,
                IntPtr reserved);
        }
    }
}
