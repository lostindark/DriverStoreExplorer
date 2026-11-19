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

        private delegate bool RemoveGhostDevice(
            string deviceId,
            Func<string, (ConfigManager.ConfigManagerResult Result, uint DevInst)> locate,
            Func<uint, ConfigManager.ConfigManagerResult> getStatus,
            Func<uint, ConfigManager.ConfigManagerResult> uninstall);

        private static readonly RemoveGhostDevice TryRemove =
            (RemoveGhostDevice)typeof(ConfigManager)
                .GetMethod(
                    "TryRemoveGhostDevice",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(string),
                        typeof(Func<string, (ConfigManager.ConfigManagerResult Result, uint DevInst)>),
                        typeof(Func<uint, ConfigManager.ConfigManagerResult>),
                        typeof(Func<uint, ConfigManager.ConfigManagerResult>),
                    },
                    null)
                .CreateDelegate(typeof(RemoveGhostDevice));

        [TestMethod]
        [DataRow(ConfigManager.ConfigManagerResult.Success, ConfigManager.ConfigManagerResult.NoSuchDevnode, ConfigManager.ConfigManagerResult.Success, true, true)]
        [DataRow(ConfigManager.ConfigManagerResult.Success, ConfigManager.ConfigManagerResult.NoSuchDevnode, ConfigManager.ConfigManagerResult.RemoveVetoed, true, false)]
        [DataRow(ConfigManager.ConfigManagerResult.Success, ConfigManager.ConfigManagerResult.Success, ConfigManager.ConfigManagerResult.Success, false, false)]
        [DataRow(ConfigManager.ConfigManagerResult.Success, ConfigManager.ConfigManagerResult.AccessDenied, ConfigManager.ConfigManagerResult.Success, false, false)]
        [DataRow(ConfigManager.ConfigManagerResult.NoSuchDevnode, ConfigManager.ConfigManagerResult.NoSuchDevnode, ConfigManager.ConfigManagerResult.Success, false, false)]
        [DataRow(ConfigManager.ConfigManagerResult.InvalidDeviceId, ConfigManager.ConfigManagerResult.NoSuchDevnode, ConfigManager.ConfigManagerResult.Success, false, false)]
        public void OnlyDevicesStillAbsentAreUninstalled(
            ConfigManager.ConfigManagerResult locateResult,
            ConfigManager.ConfigManagerResult statusResult,
            ConfigManager.ConfigManagerResult uninstallResult,
            bool expectUninstall,
            bool expectedResult)
        {
            const uint devInst = 42;
            uint? uninstalled = null;

            bool result = TryRemove(
                @"USB\VID_0781&PID_5581\1",
                id => (locateResult, devInst),
                inst => statusResult,
                inst =>
                {
                    uninstalled = inst;
                    return uninstallResult;
                });

            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectUninstall ? devInst : (uint?)null, uninstalled);
        }

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
        [DataRow(null, false)]
        [DataRow(5, true)]
        [DataRow(29, true)]
        [DataRow(31, false)]
        [DataRow(400, false)]
        public void OnlyDevicesUsedInsideTheWindowAreRecent(int? daysAgo, bool expected)
        {
            var now = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);
            DateTime? lastUsed = daysAgo.HasValue ? now.AddDays(-daysAgo.Value) : (DateTime?)null;

            Assert.AreEqual(expected, GhostDevice.IsUsedWithin(lastUsed, now, 30));
        }

        [TestMethod]
        public void LastUsedIsTheLaterOfArrivalAndRemoval()
        {
            var early = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var late = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.AreEqual(late, GhostDevice.LatestOf(early, late));
            Assert.AreEqual(late, GhostDevice.LatestOf(late, early));
            Assert.AreEqual(early, GhostDevice.LatestOf(early, null));
            Assert.AreEqual(late, GhostDevice.LatestOf(null, late));
            Assert.IsNull(GhostDevice.LatestOf(null, null));
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
