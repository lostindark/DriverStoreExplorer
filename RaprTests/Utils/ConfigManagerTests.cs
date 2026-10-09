using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests.Utils
{
    [TestClass]
    public class ConfigManagerTests
    {
        private static readonly Func<Func<(ConfigManager.ConfigManagerResult Result, int Length)>,
            Func<byte[], ConfigManager.ConfigManagerResult>, string[]> GetDeviceIds =
            (Func<Func<(ConfigManager.ConfigManagerResult Result, int Length)>,
                Func<byte[], ConfigManager.ConfigManagerResult>, string[]>)typeof(ConfigManager)
                .GetMethod("GetDeviceIds", BindingFlags.Static | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<Func<(ConfigManager.ConfigManagerResult Result, int Length)>,
                    Func<byte[], ConfigManager.ConfigManagerResult>, string[]>));

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

        [TestMethod]
        public void DeviceListRetriesWithUpdatedSize()
        {
            int sizeQueries = 0;
            int listQueries = 0;

            var ids = GetDeviceIds(
                () => (ConfigManager.ConfigManagerResult.Success, ++sizeQueries == 1 ? 2 : 3),
                buffer =>
                {
                    if (++listQueries == 1)
                    {
                        return ConfigManager.ConfigManagerResult.BufferSmall;
                    }

                    Encoding.Unicode.GetBytes("X\0\0").CopyTo(buffer, 0);
                    return ConfigManager.ConfigManagerResult.Success;
                });

            CollectionAssert.AreEqual(new[] { "X" }, ids);
            Assert.AreEqual(2, sizeQueries);
            Assert.AreEqual(2, listQueries);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void DeviceListFailuresAreNotReportedAsEmpty(bool failSizeQuery)
        {
            int listQueries = 0;

            var exception = Assert.ThrowsExactly<Win32Exception>(() => GetDeviceIds(
                () => (failSizeQuery ? ConfigManager.ConfigManagerResult.AccessDenied : ConfigManager.ConfigManagerResult.Success, 1),
                buffer =>
                {
                    listQueries++;
                    return ConfigManager.ConfigManagerResult.AccessDenied;
                }));

            Assert.AreEqual(5, exception.NativeErrorCode);
            Assert.AreEqual(failSizeQuery ? 0 : 1, listQueries);
        }

        [TestMethod]
        public void DeviceListStopsRetryingWhenTheSnapshotNeverFits()
        {
            int listQueries = 0;

            Assert.ThrowsExactly<Win32Exception>(() => GetDeviceIds(
                () => (ConfigManager.ConfigManagerResult.Success, 1),
                buffer =>
                {
                    listQueries++;
                    return ConfigManager.ConfigManagerResult.BufferSmall;
                }));

            Assert.AreEqual(3, listQueries);
        }

        [TestMethod]
        public void SuccessfulEmptyDeviceListRemainsValid()
        {
            var ids = GetDeviceIds(
                () => (ConfigManager.ConfigManagerResult.Success, 1),
                buffer => ConfigManager.ConfigManagerResult.Success);

            Assert.AreEqual(0, ids.Length);
        }
    }
}
