using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;

using BrightIdeasSoftware;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Utils;

namespace Rapr.Tests
{
    [STATestClass]
    public class DSEFormTests
    {
        [TestMethod]
        [DataRow("[Unknown]")]
        [DataRow(null)]
        [DataRow("")]
        public void OldDriverSelectionDoesNotMergeUnresolvedInfIdentities(string unresolvedInfName)
        {
            WithSelectionForm((form, list) =>
            {
                var unresolvedOld = CreateDriver(unresolvedInfName, 1);
                var unresolvedNew = CreateDriver(unresolvedInfName, 2);
                var resolvedOld = CreateDriver("resolved.inf", 1);
                var resolvedNew = CreateDriver("resolved.inf", 2);
                list.SetObjects(new[] { unresolvedOld, unresolvedNew, resolvedOld, resolvedNew });

                Invoke(form, "CtxMenuSelectOldDrivers_Click", null, EventArgs.Empty);

                CollectionAssert.AreEquivalent(new[] { resolvedOld }, list.CheckedObjects.Cast<DriverStoreEntry>().ToArray());
            });
        }

        private static DriverStoreEntry CreateDriver(string infName, int version)
        {
            return new DriverStoreEntry
            {
                DriverInfName = infName,
                DriverClass = "System",
                DriverPkgProvider = "Test provider",
                DriverVersion = new Version(version, 0),
                DriverDate = new DateTime(2026, 1, 1)
            };
        }

        private static void WithSelectionForm(Action<DSEForm, MyObjectListView> test)
        {
            // Avoid startup/elevation; only the in-memory selection controls are exercised.
            var form = (DSEForm)FormatterServices.GetUninitializedObject(typeof(DSEForm));
            GC.SuppressFinalize(form);

            using (var list = new MyObjectListView { CheckBoxes = true })
            using (var status = new ToolStripStatusLabel())
            {
                list.Columns.Add(new OLVColumn { AspectName = "DriverInfName" });
                SetField(form, "lstDriverStoreEntries", list);
                SetField(form, "lblStatus", status);
                SetField(form, "driverStore", new DismUtil());
                SetField(form, "driversWithNewerDate", new HashSet<DriverStoreEntry>());
                test(form, list);
            }
        }

        private static void SetField(DSEForm form, string name, object value)
        {
            typeof(DSEForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(form, value);
        }

        private static void Invoke(DSEForm form, string name, params object[] arguments)
        {
            typeof(DSEForm).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, arguments);
        }
    }
}
