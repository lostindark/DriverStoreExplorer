using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests
{
    [TestClass]
    public class DriverStoreEntryTests
    {
        [TestMethod]
        public void ExportedValuesLineUpWithTheHeader()
        {
            var extensionId = new Guid("1d1b2b1c-7f6e-4d8a-9c3f-1a2b3c4d5e6f");
            var entry = new DriverStoreEntry
            {
                BootCritical = true,
                DriverExtensionId = extensionId,
            };

            string[] names = DriverStoreEntry.GetFieldNames();
            string[] values = entry.GetFieldValues();

            Assert.AreEqual(names.Length, values.Length);
            CollectionAssert.DoesNotContain(names, "Driver Files");
            Assert.AreEqual(bool.TrueString, values[Array.IndexOf(names, "Boot Critical")]);
            Assert.AreEqual(extensionId.ToString(), values[Array.IndexOf(names, "Extension Id")]);

            // Not every backend fills these in.
            values = new DriverStoreEntry().GetFieldValues();
            Assert.AreEqual(string.Empty, values[Array.IndexOf(names, "Boot Critical")]);
            Assert.AreEqual(string.Empty, values[Array.IndexOf(names, "Extension Id")]);
        }

        [TestMethod]
        public void InstallDateIsTruncatedToTheDayItDisplays()
        {
            var morning = new DriverStoreEntry { DriverInfName = "morning", InstallDate = new DateTime(2026, 10, 6, 8, 15, 1) };
            var evening = new DriverStoreEntry { DriverInfName = "evening", InstallDate = new DateTime(2026, 10, 6, 23, 45, 59) };
            var nextDay = new DriverStoreEntry { DriverInfName = "nextDay", InstallDate = new DateTime(2026, 10, 7, 0, 0, 0) };

            Assert.AreEqual(new DateTime(2026, 10, 6), morning.InstallDate);
            Assert.AreEqual(new DateTime(2026, 10, 6), evening.InstallDate);
            Assert.AreEqual(new DateTime(2026, 10, 7), nextDay.InstallDate);

            // ObjectListView falls through to the secondary column only when this compare returns
            // 0, and OrderBy keeps ties in input order, so "evening" staying ahead of "morning"
            // is the order a reader now gets instead of the raw 23:45 sorting after 08:15.
            CollectionAssert.AreEqual(
                new[] { "evening", "morning", "nextDay" },
                new List<DriverStoreEntry> { evening, nextDay, morning }
                    .OrderBy(e => e.InstallDate)
                    .Select(e => e.DriverInfName)
                    .ToArray());
        }

        [TestMethod]
        public void InstallDateKeepsNull()
        {
            var entry = new DriverStoreEntry { InstallDate = null };
            Assert.IsNull(entry.InstallDate);

            entry.InstallDate = new DateTime(2026, 10, 6, 8, 15, 1);
            Assert.AreEqual(new DateTime(2026, 10, 6), entry.InstallDate);

            entry.InstallDate = null;
            Assert.IsNull(entry.InstallDate);
        }

        [TestMethod]
        [DataRow(-1L, "")]
        [DataRow(0L, "0 KB")]
        [DataRow(1L, "1 KB")]
        [DataRow(2048L, "2 KB")]
        public void UnknownAndEmptySizesAreNotShownAsOneKilobyte(long size, string expected)
        {
            Assert.AreEqual(expected, DriverStoreEntry.GetBytesReadable(size));
        }

        [TestMethod]
        public void DriverDateIsTruncatedToTheDayItDisplays()
        {
            var entry = new DriverStoreEntry { DriverDate = new DateTime(2026, 10, 6, 23, 45, 59) };
            Assert.AreEqual(new DateTime(2026, 10, 6), entry.DriverDate);

            entry.DriverDate = default(DateTime);
            Assert.AreEqual(default(DateTime), entry.DriverDate);

            entry.SetDriverDateAndVersion("2026-10-06 31.0.15.3623");
            Assert.AreEqual(new DateTime(2026, 10, 6), entry.DriverDate);
        }
    }
}
