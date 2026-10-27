using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests
{
    /// <summary>
    /// Runs the real driver store backends against the machine running the tests.
    /// These catch a Windows update breaking an API (drvstore.dll is undocumented),
    /// and are skipped when not running elevated on Windows.
    /// </summary>
    [TestClass]
    [TestCategory("Smoke")]
    public class DriverStoreSmokeTests
    {
        [TestMethod]
        public void NativeDriverStoreListsPackages()
        {
            RequireElevatedWindows();
            if (!DSEFormHelper.IsNativeDriverStoreSupported)
            {
                Assert.Inconclusive("The native driver store needs Windows 8 or later.");
            }

            AssertPackagesLookValid(new NativeDriverStore().EnumeratePackages());
        }

        [TestMethod]
        public void DismListsPackages()
        {
            RequireElevatedWindows();
            if (!DismUtil.IsDismAvailable)
            {
                Assert.Inconclusive("DISM is not available.");
            }

            AssertPackagesLookValid(new DismUtil().EnumeratePackages());
        }

        [TestMethod]
        public void DeviceListIsNotEmpty()
        {
            RequireWindows();

            Assert.IsNotEmpty(ConfigManager.GetDeviceDriverInfo());
        }

        // A clean CI machine may have no third-party packages, so only check what is listed.
        private static void AssertPackagesLookValid(List<DriverStoreEntry> entries)
        {
            Assert.IsNotNull(entries);

            foreach (var entry in entries)
            {
                StringAssert.StartsWith(entry.DriverPublishedName, "oem", StringComparison.OrdinalIgnoreCase);
                StringAssert.EndsWith(entry.DriverPublishedName, ".inf", StringComparison.OrdinalIgnoreCase);
                Assert.IsFalse(string.IsNullOrEmpty(entry.DriverInfName), entry.DriverPublishedName);
                Assert.IsTrue(Directory.Exists(entry.DriverFolderLocation), entry.DriverFolderLocation);
            }
        }

        private static void RequireWindows()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                Assert.Inconclusive("Needs Windows.");
            }
        }

        private static void RequireElevatedWindows()
        {
            RequireWindows();
            if (!DSEFormHelper.IsRunAsAdmin)
            {
                Assert.Inconclusive("Needs an elevated process.");
            }
        }
    }
}
