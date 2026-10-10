using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests.Utils
{
    [TestClass]
    public class LinqExtensionsTests
    {
        [TestMethod]
        public void OrderByColumnWithNonComparableValuesSortsByText()
        {
            var entries = new List<DriverStoreEntry>
            {
                new DriverStoreEntry { DriverPublishedName = "oem2.inf", DriverFiles = new List<string> { "b.sys" } },
                new DriverStoreEntry { DriverPublishedName = "oem3.inf", DriverFiles = null },
                new DriverStoreEntry { DriverPublishedName = "oem1.inf", DriverFiles = new List<string> { "a.sys", "z.dll" } },
            };

            var ascending = entries.OrderByColumnName(nameof(DriverStoreEntry.DriverFiles)).ToList();
            var descending = entries.OrderByColumnName(nameof(DriverStoreEntry.DriverFiles), ascending: false).ToList();

            CollectionAssert.AreEqual(
                new[] { "oem3.inf", "oem1.inf", "oem2.inf" },
                ascending.Select(e => e.DriverPublishedName).ToArray());
            CollectionAssert.AreEqual(
                new[] { "oem2.inf", "oem1.inf", "oem3.inf" },
                descending.Select(e => e.DriverPublishedName).ToArray());
        }

        [TestMethod]
        public void ThenByColumnWithNonComparableValuesSortsByText()
        {
            var entries = new List<DriverStoreEntry>
            {
                new DriverStoreEntry { DriverPublishedName = "oem2.inf", DriverClass = "Net", DriverFiles = new List<string> { "b.sys" } },
                new DriverStoreEntry { DriverPublishedName = "oem1.inf", DriverClass = "Net", DriverFiles = new List<string> { "a.sys" } },
                new DriverStoreEntry { DriverPublishedName = "oem3.inf", DriverClass = "Audio", DriverFiles = new List<string> { "c.sys" } },
            };

            var sorted = entries
                .OrderByColumnName(nameof(DriverStoreEntry.DriverClass))
                .ThenByColumnName(nameof(DriverStoreEntry.DriverFiles))
                .ToList();

            CollectionAssert.AreEqual(
                new[] { "oem3.inf", "oem1.inf", "oem2.inf" },
                sorted.Select(e => e.DriverPublishedName).ToArray());
        }

        [TestMethod]
        public void OrderByComparableColumnKeepsNativeOrdering()
        {
            var entries = new List<DriverStoreEntry>
            {
                new DriverStoreEntry { DriverPublishedName = "oem1.inf", DriverSize = 100 },
                new DriverStoreEntry { DriverPublishedName = "oem2.inf", DriverSize = 20 },
            };

            var sorted = entries.OrderByColumnName(nameof(DriverStoreEntry.DriverSize)).ToList();

            // Numeric, not textual ("100" < "20"), ordering is preserved for IComparable keys.
            CollectionAssert.AreEqual(
                new[] { "oem2.inf", "oem1.inf" },
                sorted.Select(e => e.DriverPublishedName).ToArray());
        }
    }
}
