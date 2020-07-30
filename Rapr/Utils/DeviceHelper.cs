using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rapr.Utils
{
    /// <summary>
    /// The DEVPROPTYPE data type represents the property-data-type identifier that specifies the data type of a device property value in the unified device property model.
    /// </summary>
    internal enum DevPropType : uint
    {
        TYPEMOD_ARRAY = 0x00001000,  // array of fixed-sized data elements
        TYPEMOD_LIST = 0x00002000,  // list of variable-sized data elements

        //
        // Property data types.
        //
        Empty = 0x00000000, // nothing, no property data
        Null = 0x00000001, // null property data
        Sbyte = 0x00000002, // 8-bit signed int (sbyte)
        Byte = 0x00000003, // 8-bit unsigned int (byte)
        Int16 = 0x00000004, // 16-bit signed int (short)
        Uint16 = 0x00000005, // 16-bit unsigned int (ushort)
        Int32 = 0x00000006, // 32-bit signed int (long)
        Uint32 = 0x00000007, // 32-bit unsigned int (ulong)
        Int64 = 0x00000008, // 64-bit signed int (long64)
        Uint64 = 0x00000009, // 64-bit unsigned int (ulong64)
        Float = 0x0000000a, // 32-bit floating-point (float)
        Double = 0x0000000b, // 64-bit floating-point (double)
        Decimal = 0x0000000c, // 128-bit data (decimal)
        Guid = 0x0000000d, // 128-bit unique identifier (guid)
        Currency = 0x0000000e, // 64 bit signed int currency value (currency)
        Date = 0x0000000f, // date (date)
        FileTime = 0x00000010, // file time (filetime)
        Boolean = 0x00000011, // 8-bit boolean (devprop_boolean)
        String = 0x00000012, // null-terminated string
        StringList = (String | TYPEMOD_LIST), // multi-sz string list
        SecurityDescriptor = 0x00000013, // self-relative binary security_descriptor
        SecurityDescriptorString = 0x00000014, // security descriptor string (sddl format)
        Devpropkey = 0x00000015, // device property key (devpropkey)
        Devproptype = 0x00000016, // device property type (devproptype)
        Binary = (Byte | TYPEMOD_ARRAY), // custom binary data
        Error = 0x00000017, // 32-bit win32 system error code
        Ntstatus = 0x00000018, // 32-bit ntstatus code
        StringIndirect = 0x00000019, // string resource (@[path\]<dllname>,-<strid>)
    }

    /// <summary>
    /// The DEVPROPKEY structure represents a device property key for a device property in the unified device property model.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct DevPropKey
    {
        public Guid fmtid;
        public uint pid;

        public DevPropKey(Guid fmtid, uint pid)
        {
            this.fmtid = fmtid;
            this.pid = pid;
        }

        public DevPropKey(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k, uint pid)
        {
            this.fmtid = new Guid(a, b, c, d, e, f, g, h, i, j, k);
            this.pid = pid;
        }
    }

    public static class DeviceHelper
    {
        //
        // Device properties
        // These DEVPKEYs correspond to the SetupAPI SPDRP_XXX device properties.
        //
        internal static readonly DevPropKey DEVPKEY_Device_DeviceDesc =               new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 2);     // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_HardwareIds =              new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 3);     // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_CompatibleIds =            new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 4);     // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_Service =                  new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 6);     // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_Class =                    new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 9);     // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_ClassGuid =                new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 10);    // DEVPROP_TYPE_GUID
        internal static readonly DevPropKey DEVPKEY_Device_Driver =                   new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 11);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_ConfigFlags =              new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 12);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_Manufacturer =             new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 13);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_FriendlyName =             new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 14);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_LocationInfo =             new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 15);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_PDOName =                  new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 16);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_Capabilities =             new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 17);    // DEVPROP_TYPE_UNINT32
        internal static readonly DevPropKey DEVPKEY_Device_UINumber =                 new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 18);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_UpperFilters =             new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 19);    // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_LowerFilters =             new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 20);    // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_BusTypeGuid =              new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 21);    // DEVPROP_TYPE_GUID
        internal static readonly DevPropKey DEVPKEY_Device_LegacyBusType =            new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 22);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_BusNumber =                new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 23);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_EnumeratorName =           new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 24);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_Security =                 new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 25);    // DEVPROP_TYPE_SECURITY_DESCRIPTOR
        internal static readonly DevPropKey DEVPKEY_Device_SecuritySDS =              new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 26);    // DEVPROP_TYPE_SECURITY_DESCRIPTOR_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DevType =                  new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 27);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_Exclusive =                new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 28);    // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_Device_Characteristics =          new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 29);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_Address =                  new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 30);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_UINumberDescFormat =       new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 31);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_PowerData =                new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 32);    // DEVPROP_TYPE_BINARY
        internal static readonly DevPropKey DEVPKEY_Device_RemovalPolicy =            new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 33);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_RemovalPolicyDefault =     new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 34);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_RemovalPolicyOverride =    new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 35);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_InstallState =             new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 36);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_LocationPaths =            new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 37);    // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_BaseContainerId =          new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 38);    // DEVPROP_TYPE_GUID
        internal static readonly DevPropKey DEVPKEY_Device_DebuggerSafe =             new DevPropKey(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0, 39);    // DEVPROP_TYPE_UINT32

        //
        // Device properties
        // These DEVPKEYs correspond to a device's status and problem code.
        //
        internal static readonly DevPropKey DEVPKEY_Device_DevNodeStatus =            new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 2);     // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_ProblemCode =              new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 3);     // DEVPROP_TYPE_UINT32

        //
        // Device properties
        // These DEVPKEYs correspond to a device's relations.
        //
        internal static readonly DevPropKey DEVPKEY_Device_EjectionRelations =        new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 4);     // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_RemovalRelations =         new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 5);     // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_PowerRelations =           new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 6);     // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_BusRelations =             new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 7);     // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_Parent =                   new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 8);     // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_Children =                 new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 9);     // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_Siblings =                 new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 10);    // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_TransportRelations =       new DevPropKey(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7, 11);    // DEVPROP_TYPE_STRING_LIST

        //
        // Other Device properties
        // These DEVPKEYs are set for the corresponding types of root-enumerated devices.     ;comment
        //
        internal static readonly DevPropKey DEVPKEY_Device_Reported =                 new DevPropKey(0x80497100, 0x8c73, 0x48b9, 0xaa, 0xd9, 0xce, 0x38, 0x7e, 0x19, 0xc5, 0x6e, 2);     // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_Device_Legacy =                   new DevPropKey(0x80497100, 0x8c73, 0x48b9, 0xaa, 0xd9, 0xce, 0x38, 0x7e, 0x19, 0xc5, 0x6e, 3);     // DEVPROP_TYPE_BOOLEAN

        //
        // Device Container Id
        //
        internal static readonly DevPropKey DEVPKEY_Device_ContainerId =              new DevPropKey(0x8c7ed206, 0x3f8a, 0x4827, 0xb3, 0xab, 0xae, 0x9e, 0x1f, 0xae, 0xfc, 0x6c, 2);     // DEVPROP_TYPE_GUID
        internal static readonly DevPropKey DEVPKEY_Device_InLocalMachineContainer =   new DevPropKey(0x8c7ed206, 0x3f8a, 0x4827, 0xb3, 0xab, 0xae, 0x9e, 0x1f, 0xae, 0xfc, 0x6c, 4);     // DEVPROP_TYPE_BOOLEAN

        //
        // Device Experience related Keys
        //
        internal static readonly DevPropKey DEVPKEY_Device_ModelId =                  new DevPropKey(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b, 2);     // DEVPROP_TYPE_GUID
        internal static readonly DevPropKey DEVPKEY_Device_FriendlyNameAttributes =   new DevPropKey(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b, 3);     // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_ManufacturerAttributes =   new DevPropKey(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b, 4);     // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_PresenceNotForDevice =     new DevPropKey(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b, 5);     // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_Device_SignalStrength =           new DevPropKey(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b, 6);     // DEVPROP_TYPE_INT32
        internal static readonly DevPropKey DEVPKEY_Device_IsAssociateableByUserAction =   new DevPropKey(0x80d81ea6, 0x7473, 0x4b0c, 0x82, 0x16, 0xef, 0xc1, 0x1a, 0x2c, 0x4c, 0x8b, 7);// DEVPROP_TYPE_BOOLEAN

        //
        // Other Device properties
        //
        internal static readonly DevPropKey DEVPKEY_Numa_Proximity_Domain =           new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 1);     // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_DHP_Rebalance_Policy =     new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 2);     // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_Numa_Node =                new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 3);     // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_BusReportedDeviceDesc =    new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 4);     // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_IsPresent =                new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 5);     // DEVPROP_TYPE_BOOL
        internal static readonly DevPropKey DEVPKEY_Device_HasProblem =               new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 6);     // DEVPROP_TYPE_BOOL
        internal static readonly DevPropKey DEVPKEY_Device_ConfigurationId =          new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 7);     // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_ReportedDeviceIdsHash =    new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 8);     // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_PhysicalDeviceLocation =   new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 9);     // DEVPROP_TYPE_BINARY
        internal static readonly DevPropKey DEVPKEY_Device_BiosDeviceName =           new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 10);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DriverProblemDesc =        new DevPropKey(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2, 11);    // DEVPROP_TYPE_STRING


        //
        // Device Session Id
        //
        internal static readonly DevPropKey DEVPKEY_Device_SessionId =                new DevPropKey(0x83da6326, 0x97a6, 0x4088, 0x94, 0x53, 0xa1, 0x92, 0x3f, 0x57, 0x3b, 0x29, 6);     // DEVPROP_TYPE_UINT32

        //
        // Device activity timestamp properties
        //
        internal static readonly DevPropKey DEVPKEY_Device_InstallDate =              new DevPropKey(0x83da6326, 0x97a6, 0x4088, 0x94, 0x53, 0xa1, 0x92, 0x3f, 0x57, 0x3b, 0x29, 100);   // DEVPROP_TYPE_FILETIME
        internal static readonly DevPropKey DEVPKEY_Device_FirstInstallDate =         new DevPropKey(0x83da6326, 0x97a6, 0x4088, 0x94, 0x53, 0xa1, 0x92, 0x3f, 0x57, 0x3b, 0x29, 101);   // DEVPROP_TYPE_FILETIME
        internal static readonly DevPropKey DEVPKEY_Device_LastArrivalDate =          new DevPropKey(0x83da6326, 0x97a6, 0x4088, 0x94, 0x53, 0xa1, 0x92, 0x3f, 0x57, 0x3b, 0x29, 102);   // DEVPROP_TYPE_FILETIME
        internal static readonly DevPropKey DEVPKEY_Device_LastRemovalDate =          new DevPropKey(0x83da6326, 0x97a6, 0x4088, 0x94, 0x53, 0xa1, 0x92, 0x3f, 0x57, 0x3b, 0x29, 103);   // DEVPROP_TYPE_FILETIME


        //
        // Device driver properties
        //
        internal static readonly DevPropKey DEVPKEY_Device_DriverDate =               new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 2);      // DEVPROP_TYPE_FILETIME
        internal static readonly DevPropKey DEVPKEY_Device_DriverVersion =            new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 3);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DriverDesc =               new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 4);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DriverInfPath =            new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 5);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DriverInfSection =         new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 6);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DriverInfSectionExt =      new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 7);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_MatchingDeviceId =         new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 8);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DriverProvider =           new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 9);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DriverPropPageProvider =   new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 10);     // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DriverCoInstallers =       new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 11);     // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_Device_ResourcePickerTags =       new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 12);     // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_ResourcePickerExceptions =   new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 13);   // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_Device_DriverRank =               new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 14);     // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_Device_DriverLogoLevel =          new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 15);     // DEVPROP_TYPE_UINT32

        //
        // Device properties
        // These DEVPKEYs may be set by the driver package installed for a device.
        //
        internal static readonly DevPropKey DEVPKEY_Device_NoConnectSound =           new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 17);     // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_Device_GenericDriverInstalled =   new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 18);     // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_Device_AdditionalSoftwareRequested =   new DevPropKey(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6, 19); //DEVPROP_TYPE_BOOLEAN

        //
        // Device safe-removal properties
        //
        internal static readonly DevPropKey DEVPKEY_Device_SafeRemovalRequired =      new DevPropKey(0xafd97640,  0x86a3, 0x4210, 0xb6, 0x7c, 0x28, 0x9c, 0x41, 0xaa, 0xbe, 0x55, 2);    // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_Device_SafeRemovalRequiredOverride =   new DevPropKey(0xafd97640,  0x86a3, 0x4210, 0xb6, 0x7c, 0x28, 0x9c, 0x41, 0xaa, 0xbe, 0x55, 3); // DEVPROP_TYPE_BOOLEAN

        //                                                                            
        // Device setup class properties                                              
        //                                                                            
        internal static readonly DevPropKey DEVPKEY_DeviceClass_Name = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 2);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DeviceClass_ClassName = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 3);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DeviceClass_Icon = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 4);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DeviceClass_ClassInstaller = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 5);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DeviceClass_PropPageProvider = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 6);      // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DeviceClass_NoInstallClass = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 7);      // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DeviceClass_NoDisplayClass = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 8);      // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DeviceClass_SilentInstall = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 9);      // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DeviceClass_NoUseClass = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 10);     // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DeviceClass_DefaultService = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 11);     // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DeviceClass_IconPath = new DevPropKey(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66, 12);     // DEVPROP_TYPE_STRING_LIST

        //
        // Driver database - driver package properties
        //
        internal static readonly DevPropKey DEVPKEY_DriverPackage_DriverInfName =           new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 2);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DriverPackage_OriginalInfName =         new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 3);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DriverPackage_SourceMediaPath =         new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 4);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DriverPackage_Inbox =                   new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 5);    // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_Published =               new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 6);    // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_SignerName =              new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 7);    // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DriverPackage_SignerScore =             new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 8);    // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_DriverPackage_F6 =                      new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 9);    // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ProcessorArchitecture =   new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 10);   // DEVPROP_TYPE_UINT16
        internal static readonly DevPropKey DEVPKEY_DriverPackage_Locale =                  new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 11);   // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ProviderName =            new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 12);   // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ClassGuid =               new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 13);   // DEVPROP_TYPE_GUID
        internal static readonly DevPropKey DEVPKEY_DriverPackage_DriverDate =              new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 14);   // DEVPROP_TYPE_FILETIME
        internal static readonly DevPropKey DEVPKEY_DriverPackage_DriverVersion =           new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 15);   // DEVPROP_TYPE_UINT64
        internal static readonly DevPropKey DEVPKEY_DriverPackage_BootCritical =            new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 16);   // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ConfigurableFlags =       new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 17);   // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_DriverPackage_Configurable =            new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 18);   // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ConfigurableOverride =    new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 19);   // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ExtensionId =             new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 20);   // DEVPROP_TYPE_GUID
        internal static readonly DevPropKey DEVPKEY_DriverPackage_StatusFlags =             new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 21);   // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_DriverPackage_LockLevel =               new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 22);   // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ProductName =             new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 23);   // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DriverPackage_TargetComputerIds =       new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 24);   // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_DriverPackage_SystemCritical =          new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 25);   // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ImportDate =              new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 26);   // DEVPROP_TYPE_FILETIME
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ClassVersion =            new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 27);   // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_DriverPackage_NeedsReconfig =           new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 28);   // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_Configurations =          new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 29);   // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ConfigurationScopes =     new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 30);   // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_DriverPackage_RelatedDriverPackages =   new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 31);   // DEVPROP_TYPE_STRING_LIST
        internal static readonly DevPropKey DEVPKEY_DriverPackage_DriverPackageId =         new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 32);   // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DriverPackage_FamilyId =                new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 33);   // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DriverPackage_ExtensionContractIds =    new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 34);   // DEVPROP_TYPE_GUID | DEVPROP_TYPEMOD_ARRAY
        internal static readonly DevPropKey DEVPKEY_DriverPackage_FileSize =                new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 35);   // DEVPROP_TYPE_UINT64
        internal static readonly DevPropKey DEVPKEY_DriverPackage_EffectiveFileSize =       new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 36);   // DEVPROP_TYPE_UINT64
        internal static readonly DevPropKey DEVPKEY_DriverPackage_CatalogFile =             new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 37);   // DEVPROP_TYPE_STRING
        internal static readonly DevPropKey DEVPKEY_DriverPackage_Integrated =              new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 38);   // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_PrimitiveFlags =          new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 39);   // DEVPROP_TYPE_UINT32
        internal static readonly DevPropKey DEVPKEY_DriverPackage_Primitive =               new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 40);   // DEVPROP_TYPE_BOOLEAN
        internal static readonly DevPropKey DEVPKEY_DriverPackage_Invalidated =             new DevPropKey(0x8163eb01, 0x142c, 0x4f7a, 0x94, 0xe1, 0xa2, 0x74, 0xcc, 0x47, 0xdb, 0xba, 41);   // DEVPROP_TYPE_BOOLEAN

       internal static T ConvertPropToType<T>(IntPtr propertyBufferPtr, DevPropType propertyType)
        {
            if ((propertyType == DevPropType.String || propertyType == DevPropType.StringIndirect) && typeof(T) == typeof(string))
            {
                return (T)(object)Marshal.PtrToStringUni(propertyBufferPtr);
            }
            else if (propertyType == DevPropType.FileTime && typeof(T) == typeof(DateTime))
            {
                var time = (System.Runtime.InteropServices.ComTypes.FILETIME)Marshal.PtrToStructure(propertyBufferPtr, typeof(System.Runtime.InteropServices.ComTypes.FILETIME));
                ulong high = (ulong)time.dwHighDateTime;
                uint low = (uint)time.dwLowDateTime;
                long fileTime = (long)((high << 32) + low);
                return (T)(object)DateTime.FromFileTimeUtc(fileTime);
            }
            else if (propertyType == DevPropType.Uint64 && typeof(T) == typeof(ulong))
            {
                return (T)(object)(ulong)Marshal.ReadInt64(propertyBufferPtr);
            }
            else if (propertyType == DevPropType.Uint64 && typeof(T) == typeof(Version))
            {
                ulong driverVersion = (ulong)Marshal.ReadInt64(propertyBufferPtr);
                return (T)(object)new Version(
                    (int)((driverVersion >> 48) & 0xFFFF),
                    (int)((driverVersion >> 32) & 0xFFFF),
                    (int)((driverVersion >> 16) & 0xFFFF),
                    (int)((driverVersion >> 0) & 0xFFFF));
            }
            else if (propertyType == DevPropType.String && typeof(T) == typeof(Version))
            {
                return (T)(object)Version.Parse(Marshal.PtrToStringUni(propertyBufferPtr));
            }
            else if (propertyType == DevPropType.Uint32 && typeof(T) == typeof(uint))
            {
                return (T)(object)(uint)Marshal.ReadInt32(propertyBufferPtr);
            }
            else if (propertyType == DevPropType.Guid && typeof(T) == typeof(Guid))
            {
                return (T)Marshal.PtrToStructure(propertyBufferPtr, typeof(Guid));
            }
            else if (propertyType == DevPropType.Boolean && (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?)))
            {
                return (T)(object)(Marshal.ReadByte(propertyBufferPtr) != 0);
            }
            else if (propertyType == DevPropType.StringList && typeof(T) == typeof(IList<string>))
            {
                IntPtr curStringPos = propertyBufferPtr;
                List<string> strings = new List<string>();

                while (true)
                {
                    string curStr = Marshal.PtrToStringUni(curStringPos);
                    if (string.IsNullOrEmpty(curStr))
                    {
                        break;
                    }

                    strings.Add(curStr);
                    curStringPos = new IntPtr(curStringPos.ToInt64() + ((curStr.Length + 1) * sizeof(char)));
                }

                return (T)(object)strings;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
