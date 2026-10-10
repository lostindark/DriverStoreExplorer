using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Rapr.Utils
{
    public static class ConfigManager
    {
        public static List<DriverStoreEntry> FillDeviceInfo(List<DriverStoreEntry> driverStoreEntries)
        {
            var devicesInfo = ConfigManager.GetDeviceDriverInfo();
            return FillDeviceInfo(driverStoreEntries, devicesInfo);
        }

        public static List<DriverStoreEntry> FillDeviceInfo(List<DriverStoreEntry> driverStoreEntries, List<DeviceDriverInfo> devicesInfo)
        {
            foreach (var driverStoreEntry in driverStoreEntries)
            {
                var deviceInfo = devicesInfo.OrderByDescending(d => d.IsPresent)?.FirstOrDefault(e =>
                    (!string.IsNullOrEmpty(driverStoreEntry.DriverPublishedName)
                        && string.Equals(e.DriverInf, driverStoreEntry.DriverPublishedName, StringComparison.OrdinalIgnoreCase))
                    || e.ExtendedInfs?.Any(extInf => string.Equals(extInf, driverStoreEntry.DriverPublishedName, StringComparison.OrdinalIgnoreCase)) == true);

                driverStoreEntry.DeviceId = deviceInfo?.DeviceId;
                driverStoreEntry.DeviceName = deviceInfo?.DeviceName;
                driverStoreEntry.DevicePresent = deviceInfo?.IsPresent;
            }

            return driverStoreEntries;
        }

        public static List<DeviceDriverInfo> GetDeviceDriverInfo()
        {
            List<DeviceDriverInfo> deviceDriverInfos = new List<DeviceDriverInfo>();

            var deviceIds = GetDeviceIds(
                () =>
                {
                    int length = 0;
                    var result = NativeMethods.CM_Get_Device_ID_List_Size(ref length, null, CM_GETIDLIST_FILTER.NONE);
                    return (result, length);
                },
                buffer => NativeMethods.CM_Get_Device_ID_List(null, buffer, buffer.Length / sizeof(char), CM_GETIDLIST_FILTER.NONE));

            foreach (var deviceId in deviceIds)
            {
                uint devInst = 0;
                var locateResult = NativeMethods.CM_Locate_DevNode(
                    ref devInst,
                    deviceId,
                    CM_LOCATE_DEVNODE_FLAG.CM_LOCATE_DEVNODE_PHANTOM);
                if (locateResult == ConfigManagerResult.NoSuchDevnode)
                {
                    continue;
                }

                if (locateResult != ConfigManagerResult.Success)
                {
                    throw CreateConfigManagerException(locateResult);
                }

                deviceDriverInfos.Add(new DeviceDriverInfo(
                    deviceId,
                    GetDevNodeProperty<string>(devInst, DeviceHelper.DEVPKEY_Device_FriendlyName)
                        ?? GetDevNodeProperty<string>(devInst, DeviceHelper.DEVPKEY_Device_DeviceDesc),
                    GetDevNodeProperty<string>(devInst, DeviceHelper.DEVPKEY_Device_DriverInfPath),
                    GetDevNodeProperty<DateTime>(devInst, DeviceHelper.DEVPKEY_Device_DriverDate),
                    GetDevNodeProperty<Version>(devInst, DeviceHelper.DEVPKEY_Device_DriverVersion),
                    IsDevicePresent(devInst),
                    GetDevNodeProperty<string[]>(devInst, DeviceHelper.DEVPKEY_Device_DriverExtendedInfs)));
            }

            return deviceDriverInfos;
        }

        private static string[] GetDeviceIds(
            Func<(ConfigManagerResult Result, int Length)> getSize,
            Func<byte[], ConfigManagerResult> getList)
        {
            ConfigManagerResult result = ConfigManagerResult.Failure;

            // A device can arrive between querying the list size and retrieving it.
            for (int attempt = 0; attempt < 3; attempt++)
            {
                var size = getSize();
                result = size.Result;
                if (result != ConfigManagerResult.Success)
                {
                    break;
                }

                byte[] buffer = new byte[checked(size.Length * sizeof(char))];
                result = getList(buffer);
                if (result == ConfigManagerResult.Success)
                {
                    return Encoding.Unicode.GetString(buffer).Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                }

                if (result != ConfigManagerResult.BufferSmall)
                {
                    break;
                }
            }

            throw new Win32Exception((int)NativeMethods.CM_MapCrToWin32Err(result, 31));
        }

        private static bool? IsDevicePresent(uint devInst)
        {
            ConfigManagerResult result = NativeMethods.CM_Get_DevNode_Status(out _, out _, devInst, 0);

            if (result == ConfigManagerResult.Success)
            {
                return true;
            }
            else if (result == ConfigManagerResult.NoSuchDevnode)
            {
                return false;
            }
            else
            {
                return null;
            }
        }

        internal static T GetDevNodeProperty<T>(uint devInst, DevPropKey propertyKey)
        {
            return ReadDevNodeProperty<T>((buffer, size) =>
            {
                uint requiredSize = size;
                var result = NativeMethods.CM_Get_DevNode_Property(devInst, ref propertyKey,
                    out DevPropType propertyType, buffer, ref requiredSize, 0);
                return (result, (uint)propertyType, requiredSize);
            });
        }

        private static T ReadDevNodeProperty<T>(Func<IntPtr, uint, (ConfigManagerResult Result, uint Type, uint Size)> read)
        {
            uint size = 2048;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                IntPtr buffer = Marshal.AllocHGlobal(checked((int)size));
                try
                {
                    var property = read(buffer, size);
                    if (property.Result == ConfigManagerResult.Success)
                    {
                        return property.Size > 0 ? DeviceHelper.ConvertPropToType<T>(buffer, (DevPropType)property.Type) : default;
                    }

                    if (property.Result == ConfigManagerResult.NoSuchValue)
                    {
                        return default;
                    }

                    if (property.Result != ConfigManagerResult.BufferSmall)
                    {
                        throw CreateConfigManagerException(property.Result);
                    }

                    size = Math.Max(property.Size, checked(size * 2));
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }

            throw CreateConfigManagerException(ConfigManagerResult.BufferSmall);
        }

        private static Win32Exception CreateConfigManagerException(ConfigManagerResult result)
        {
            return new Win32Exception((int)NativeMethods.CM_MapCrToWin32Err(result, 31));
        }

        internal static T GetClassProperty<T>(Guid classGuid, DevPropKey propertyKey)
        {
            const int bufferSize = 2048;
            IntPtr propertyBufferPtr = Marshal.AllocHGlobal(bufferSize);
            uint propertySize = bufferSize;

            try
            {
                if (NativeMethods.CM_Get_Class_Property(
                    ref classGuid,
                    ref propertyKey,
                    out DevPropType propertyType,
                    propertyBufferPtr,
                    ref propertySize,
                    0) == 0)
                {
                    if (propertySize > 0)
                    {
                        return DeviceHelper.ConvertPropToType<T>(propertyBufferPtr, propertyType);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(propertyBufferPtr);
            }

            return default(T);
        }

        //
        // Flags for CM_Get_Device_ID_List, CM_Get_Device_ID_List_Size
        //
        [Flags]
        internal enum CM_GETIDLIST_FILTER : uint
        {
            ENUMERATOR = 0x00000001,
            SERVICE = 0x00000002,
            EJECTRELATIONS = 0x00000004,
            REMOVALRELATIONS = 0x00000008,
            POWERRELATIONS = 0x00000010,
            BUSRELATIONS = 0x00000020,
            NONE = 0x00000000,
            DONOTGENERATE = 0x10000040,
            TRANSPORTRELATIONS = 0x00000080,
            PRESENT = 0x00000100,
            CLASS = 0x00000200,
            BITS = 0x100003FF,
        }

        //
        // Flags for CM_Locate_DevNode
        //
        internal enum CM_LOCATE_DEVNODE_FLAG : uint
        {
            CM_LOCATE_DEVNODE_NORMAL = 0x00000000,
            CM_LOCATE_DEVNODE_PHANTOM = 0x00000001,
            CM_LOCATE_DEVNODE_CANCELREMOVE = 0x00000002,
            CM_LOCATE_DEVNODE_NOVALIDATION = 0x00000004,
            CM_LOCATE_DEVNODE_BITS = 0x00000007,
        }

#pragma warning disable CA1028 // Enum Storage should be Int32
        public enum ConfigManagerResult : uint
#pragma warning restore CA1028 // Enum Storage should be Int32
        {
            Success = 0x00000000,
            Default = 0x00000001,
            OutOfMemory = 0x00000002,
            InvalidPointer = 0x00000003,
            InvalidFlag = 0x00000004,
            InvalidDevnode = 0x00000005,
            InvalidDevinst = InvalidDevnode,
            InvalidResDes = 0x00000006,
            InvalidLogConf = 0x00000007,
            InvalidArbitrator = 0x00000008,
            InvalidNodelist = 0x00000009,
            DevnodeHasReqs = 0x0000000A,
            DevinstHasReqs = DevnodeHasReqs,
            InvalidResourceid = 0x0000000B,
            NoSuchDevnode = 0x0000000D,
            NoSuchDevinst = NoSuchDevnode,
            NoMoreLogConf = 0x0000000E,
            NoMoreResDes = 0x0000000F,
            AlreadySuchDevnode = 0x00000010,
            AlreadySuchDevinst = AlreadySuchDevnode,
            InvalidRangeList = 0x00000011,
            InvalidRange = 0x00000012,
            Failure = 0x00000013,
            NoSuchLogicalDev = 0x00000014,
            CreateBlocked = 0x00000015,
            RemoveVetoed = 0x00000017,
            ApmVetoed = 0x00000018,
            InvalidLoadType = 0x00000019,
            BufferSmall = 0x0000001A,
            NoArbitrator = 0x0000001B,
            NoRegistryHandle = 0x0000001C,
            RegistryError = 0x0000001D,
            InvalidDeviceId = 0x0000001E,
            InvalidData = 0x0000001F,
            InvalidApi = 0x00000020,
            DevloaderNotReady = 0x00000021,
            NeedRestart = 0x00000022,
            NoMoreHwProfiles = 0x00000023,
            DeviceNotThere = 0x00000024,
            NoSuchValue = 0x00000025,
            WrongType = 0x00000026,
            InvalidPriority = 0x00000027,
            NotDisableable = 0x00000028,
            FreeResources = 0x00000029,
            QueryVetoed = 0x0000002A,
            CantShareIrq = 0x0000002B,
            NoDependent = 0x0000002C,
            SameResources = 0x0000002D,
            NoSuchRegistryKey = 0x0000002E,
            InvalidMachinename = 0x0000002F,   // NT ONLY
            RemoteCommFailure = 0x00000030,   // NT ONLY
            MachineUnavailable = 0x00000031,   // NT ONLY
            NoCmServices = 0x00000032,   // NT ONLY
            AccessDenied = 0x00000033,   // NT ONLY
            CallNotImplemented = 0x00000034,
            InvalidProperty = 0x00000035,
            DeviceInterfaceActive = 0x00000036,
            NoSuchDeviceInterface = 0x00000037,
            InvalidReferenceString = 0x00000038,
            InvalidConflictList = 0x00000039,
            InvalidIndex = 0x0000003A,
            InvalidStructureSize = 0x0000003B
        }

        /// <summary>
        /// The managed interop layer to CfgMgr32.dll
        /// </summary>
        internal static class NativeMethods
        {
            [DllImport("CfgMgr32.dll")]
            internal static extern uint CM_MapCrToWin32Err(ConfigManagerResult result, uint defaultError);

            [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern ConfigManagerResult CM_Get_Class_Property(
                ref Guid classGUID,
                ref DevPropKey propertyKey,
                out DevPropType propertyType,
                IntPtr buffer,
                ref uint bufferSize,
                uint flags);

            [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern ConfigManagerResult CM_Get_Device_ID_List_Size(ref int length, string filter, CM_GETIDLIST_FILTER flags);

            [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern ConfigManagerResult CM_Get_Device_ID_List(string filter, byte[] buffer, int bufferLength, CM_GETIDLIST_FILTER flags);

            [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern ConfigManagerResult CM_Locate_DevNode(ref uint devInst, string deviceID, CM_LOCATE_DEVNODE_FLAG flags);

            [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern ConfigManagerResult CM_Get_DevNode_Property(
                uint devInst,
                ref DevPropKey propertyKey,
                out DevPropType propertyType,
                IntPtr buffer,
                ref uint bufferSize,
                uint flags);

            [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern ConfigManagerResult CM_Get_DevNode_Status(
              out uint status,
              out uint problemNumber,
              uint devInst,
              uint ulFlags);
        }
    }
}
