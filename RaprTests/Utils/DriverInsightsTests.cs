using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Controls;
using Rapr.Utils;

namespace Rapr.Tests.Utils
{
    [TestClass]
    public class DriverInsightsTests
    {
        private static DriverStoreEntry Package(string publishedName, string version, string deviceName = null, string infName = "net.inf") =>
            new DriverStoreEntry
            {
                DriverPublishedName = publishedName,
                DriverInfName = infName,
                DriverClass = "Net",
                DriverPkgProvider = "Contoso",
                DriverVersion = new Version(version),
                DriverDate = new DateTime(2020, 1, 1).AddDays(int.Parse(version.Split('.')[0])),
                DeviceName = deviceName,
                DevicePresent = deviceName == null ? (bool?)null : true,
            };

        [TestMethod]
        public void GroupVersionsSortsNewestFirstAndSkipsSingleVersions()
        {
            var entries = new List<DriverStoreEntry>
            {
                Package("oem1.inf", "1.0"),
                Package("oem3.inf", "3.0"),
                Package("oem2.inf", "2.0"),
                Package("oem4.inf", "1.0", infName: "other.inf"),
            };

            var groups = DriverInsights.GroupVersions(entries);

            Assert.AreEqual(1, groups.Count);
            CollectionAssert.AreEqual(
                new[] { "oem3.inf", "oem2.inf", "oem1.inf" },
                groups[0].SelectMany(version => version).Select(e => e.DriverPublishedName).ToArray());
        }

        [TestMethod]
        public void OldVersionIsOnlyAnOlderPackageThatNoDeviceUses()
        {
            var newest = Package("oem3.inf", "3.0", deviceName: "Contoso Adapter");
            var unused = Package("oem1.inf", "1.0");
            var used = Package("oem2.inf", "2.0", deviceName: "Contoso Adapter #2");
            var insights = new DriverInsights(new[] { newest, unused, used }, devicesKnown: true);

            Assert.IsTrue(insights.IsOldVersion(unused));
            Assert.IsFalse(insights.IsOldVersion(used));
            Assert.IsFalse(insights.IsOldVersion(newest));
        }

        [TestMethod]
        public void SameVersionWithADeviceIsNotOld()
        {
            // Select Old Driver(s) skips a version when any of its packages has a device.
            var newest = Package("oem3.inf", "3.0");
            var copy1 = Package("oem1.inf", "1.0");
            var copy2 = Package("oem2.inf", "1.0", deviceName: "Contoso Adapter");
            var insights = new DriverInsights(new[] { newest, copy1, copy2 }, devicesKnown: true);

            Assert.IsFalse(insights.IsOldVersion(copy1));
            Assert.IsFalse(insights.IsOldVersion(copy2));
        }

        [TestMethod]
        public void NothingIsOldOrUnusedWhenDevicesAreUnknown()
        {
            var newest = Package("oem2.inf", "2.0");
            var older = Package("oem1.inf", "1.0");
            var insights = new DriverInsights(new[] { newest, older }, devicesKnown: false);

            Assert.IsFalse(insights.IsOldVersion(older));
            Assert.IsFalse(insights.HasNoDevice(older));
            Assert.AreSame(newest, insights.GetNewerVersion(older));
        }

        [TestMethod]
        public void NewerVersionPrefersAPackageInUse()
        {
            var newestUnused = Package("oem3.inf", "3.0");
            var newestUsed = Package("oem4.inf", "3.0", deviceName: "Contoso Adapter");
            var older = Package("oem1.inf", "1.0");
            var insights = new DriverInsights(new[] { newestUnused, newestUsed, older }, devicesKnown: true);

            Assert.AreSame(newestUsed, insights.GetNewerVersion(older));
            Assert.IsNull(insights.GetNewerVersion(newestUnused));
        }

        [TestMethod]
        public void NewerVersionPrefersAConnectedDevice()
        {
            var newestDisconnected = Package("oem3.inf", "3.0", deviceName: "Contoso Adapter");
            newestDisconnected.DevicePresent = false;
            var newestConnected = Package("oem4.inf", "3.0", deviceName: "Contoso Adapter #2");
            var older = Package("oem1.inf", "1.0");
            var insights = new DriverInsights(new[] { newestDisconnected, newestConnected, older }, devicesKnown: true);

            Assert.AreSame(newestConnected, insights.GetNewerVersion(older));
            StringAssert.Contains(DriverDetailsPanel.GetNotes(older, insights)[0], "Contoso Adapter #2");
        }

        [TestMethod]
        public void NotesNameTheNewerVersionAndItsDevice()
        {
            var newest = Package("oem2.inf", "2.0", deviceName: "Contoso Adapter");
            var older = Package("oem1.inf", "1.0");
            var insights = new DriverInsights(new[] { newest, older }, devicesKnown: true);

            IList<string> notes = DriverDetailsPanel.GetNotes(older, insights);

            Assert.AreEqual(2, notes.Count);
            StringAssert.Contains(notes[0], "2.0");
            StringAssert.Contains(notes[0], "Contoso Adapter");
        }

        [TestMethod]
        public void NotesShowTheDateWhenOnlyTheDateIsNewer()
        {
            var newest = Package("oem2.inf", "2.0");
            newest.DriverDate = new DateTime(2024, 5, 6);
            var older = Package("oem1.inf", "2.0");
            older.DriverDate = new DateTime(2023, 1, 2);
            var insights = new DriverInsights(new[] { newest, older }, devicesKnown: true);

            IList<string> notes = DriverDetailsPanel.GetNotes(older, insights);

            StringAssert.Contains(notes[0], "2.0 (" + newest.DriverDate.ToString("d", System.Globalization.CultureInfo.CurrentCulture) + ")");
        }

        [TestMethod]
        public void NotesMentionBootCritical()
        {
            var entry = Package("oem1.inf", "1.0", deviceName: "Contoso Adapter");
            entry.BootCritical = true;

            IList<string> notes = DriverDetailsPanel.GetNotes(entry, new DriverInsights(new[] { entry }, devicesKnown: true));

            Assert.AreEqual(2, notes.Count);
            StringAssert.Contains(notes[0], "Contoso Adapter");
        }
    }
}
