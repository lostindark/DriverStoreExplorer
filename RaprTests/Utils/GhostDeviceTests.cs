using System;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests.Utils
{
    [TestClass]
    public class GhostDeviceTests
    {
        private static readonly Func<ConfigManager.ConfigManagerResult, bool?> PresenceFromStatus =
            (Func<ConfigManager.ConfigManagerResult, bool?>)typeof(ConfigManager)
                .GetMethod("PresenceFromStatus", BindingFlags.Static | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<ConfigManager.ConfigManagerResult, bool?>));

        [TestMethod]
        [DataRow(ConfigManager.ConfigManagerResult.NoSuchDevnode, true)]
        [DataRow(ConfigManager.ConfigManagerResult.Success, false)]
        [DataRow(ConfigManager.ConfigManagerResult.AccessDenied, false)]
        [DataRow(ConfigManager.ConfigManagerResult.Failure, false)]
        public void OnlyDevnodesKnownToBeAbsentAreGhosts(ConfigManager.ConfigManagerResult statusResult, bool expected)
        {
            Assert.AreEqual(expected, GhostDevice.IsGhost(PresenceFromStatus(statusResult)));
        }

        [TestMethod]
        [DataRow(@"USB\VID_046D&PID_C52B\5&2B7E0C4&0&2", true)]
        [DataRow(@"PCI\VEN_8086&DEV_15F3&SUBSYS_00008086&REV_03\3&11583659&0&E8", true)]
        [DataRow(@"HID\VID_046D&PID_C52B&MI_00\7&1A2B3C4D&0&0000", true)]
        [DataRow(@"BTHENUM\{0000110B-0000-1000-8000-00805F9B34FB}_LOCALMFG&0002\8&1&0", true)]
        [DataRow(@"ROOT\NET\0000", false)]
        [DataRow(@"root\SYSTEM\0001", false)]
        [DataRow(@"SWD\MMDEVAPI\{0.0.0.00000000}.{8D4B4E2C}", false)]
        [DataRow(@"HTREE\ROOT\0", false)]
        [DataRow(@"SW\{EEAB7790-C514-11D1-B42B-00805FC1270E}\ASYNCMAC", false)]
        [DataRow(@"UMB\UMB\1&841921D&0&PRINTQUEUE", false)]
        [DataRow(@"{5D624F94-8850-40C3-A3FA-A4FD2080BAF3}\VWIFIMP_WFD\5&1&0", false)]
        [DataRow("", false)]
        [DataRow(null, false)]
        public void SoftwareEnumeratedDevicesAreNotSelectedByDefault(string instanceId, bool expected)
        {
            Assert.AreEqual(expected, GhostDevice.IsSelectedByDefault(instanceId));
        }
    }
}
