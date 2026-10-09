using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests.Utils
{
    [TestClass]
    public class ConfigManagerTests
    {
        [TestMethod]
        public void FillDeviceInfoMatchesActiveExtensionWithoutExtensionGuid()
        {
            var extension = new DriverStoreEntry
            {
                DriverPublishedName = "oem42.inf",
                DriverDate = new DateTime(2026, 1, 1),
                DriverVersion = new Version(2, 0)
            };
            var unrelated = new DriverStoreEntry { DriverPublishedName = "oem43.inf" };
            var devices = new List<DeviceDriverInfo>
            {
                new DeviceDriverInfo("disconnected", "Disconnected device", "oem1.inf",
                    new DateTime(2025, 1, 1), new Version(1, 0), false, new[] { "oem42.inf" }),
                new DeviceDriverInfo("active", "Active device", "oem1.inf",
                    new DateTime(2025, 1, 1), new Version(1, 0), true, new[] { "OEM42.INF" })
            };

            ConfigManager.FillDeviceInfo(new List<DriverStoreEntry> { extension, unrelated }, devices);

            Assert.AreEqual("active", extension.DeviceId);
            Assert.AreEqual("Active device", extension.DeviceName);
            Assert.AreEqual(true, extension.DevicePresent);
            Assert.IsNull(unrelated.DeviceId);
            Assert.IsNull(unrelated.DeviceName);
            Assert.IsNull(unrelated.DevicePresent);
        }
    }
}
