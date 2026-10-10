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
        [DataRow("CtxMenuSelectOldDrivers_Click")]
        [DataRow("CtxMenuSelectUnusedDrivers_Click")]
        public void AssociatedDriversWithoutDisplayNamesAreNotCleanupCandidates(string handler)
        {
            WithSelectionForm((form, list) =>
            {
                var associated = CreateDriver("driver.inf", 1);
                associated.DeviceId = "ACTIVE-DEVICE";
                associated.DevicePresent = true;
                list.SetObjects(new[] { associated, CreateDriver("driver.inf", 2) });
                Invoke(form, handler, null, EventArgs.Empty);
                Assert.IsFalse(list.CheckedObjects.Contains(associated));
            });
        }

        [TestMethod]
        public void PendingSelectionUpdateCannotUnlockARunningOperation()
        {
            WithOperationForm((form, list) =>
            {
                SetField(form, "driverStore", new NativeDriverStore());
                var driver = CreateDriver("driver.inf", 1);
                list.SetObjects(new[] { driver });
                list.CheckedObjects = new[] { driver };

                Invoke(form, "StartOperation");
                Invoke(form, "UpdateCheckedItemSize");

                foreach (string name in new[] { "buttonDeleteDriver", "buttonExportDrivers", "cbForceDeletion", "exportSelectedDriverListToolStripMenuItem" })
                {
                    Assert.IsFalse(IsActionEnabled(form, name), name);
                }

                var opening = new System.ComponentModel.CancelEventArgs();
                Invoke(form, "ContextMenuStrip_Opening", null, opening);
                Assert.IsTrue(opening.Cancel);

                Invoke(form, "EndOperation");

                foreach (string name in new[] { "buttonDeleteDriver", "buttonExportDrivers", "cbForceDeletion", "exportSelectedDriverListToolStripMenuItem" })
                {
                    Assert.IsTrue(IsActionEnabled(form, name), name);
                }
            });
        }

        [TestMethod]
        public void SelectionUpdateDoesNotEnableUnsupportedOfflineActions()
        {
            WithOperationForm((form, list) =>
            {
                SetField(form, "driverStore", new DismUtil(@"C:\ReviewImage"));
                var driver = CreateDriver("driver.inf", 1);
                list.SetObjects(new[] { driver });
                list.CheckedObjects = new[] { driver };

                Invoke(form, "UpdateCheckedItemSize");

                Assert.IsTrue(IsActionEnabled(form, "buttonDeleteDriver"));
                Assert.IsFalse(IsActionEnabled(form, "cbForceDeletion"));
                Assert.IsFalse(IsActionEnabled(form, "buttonExportDrivers"));

                Invoke(form, "EndOperation");

                Assert.IsFalse(IsActionEnabled(form, "buttonSelectOldDrivers"));
                Assert.IsFalse(IsActionEnabled(form, "buttonSelectUnusedDrivers"));

                SetField(form, "driverStore", new PnpUtil());
                Invoke(form, "EndOperation");
                Assert.IsFalse(IsActionEnabled(form, "buttonExportAllDrivers"));
            });
        }

        [TestMethod]
        [DataRow("CtxMenuSelectOldDrivers_Click")]
        [DataRow("CtxMenuSelectUnusedDrivers_Click")]
        public void CleanupSelectionClearsStaleChecksWhenNoCandidatesExist(string handler)
        {
            WithSelectionForm((form, list) =>
            {
                var active = CreateDriver("active.inf", 1);
                active.DeviceName = "Active device";
                list.SetObjects(new[] { active });
                list.CheckedObjects = new[] { active };

                Invoke(form, handler, null, EventArgs.Empty);

                Assert.AreEqual(0, list.CheckedObjects.Count);
            });
        }

        [TestMethod]
        [DataRow("CtxMenuSelectOldDrivers_Click")]
        [DataRow("CtxMenuSelectUnusedDrivers_Click")]
        public void CleanupSelectionReplacesStaleChecksWithTheMatchingCandidates(string handler)
        {
            WithSelectionForm((form, list) =>
            {
                var active = CreateDriver("driver.inf", 2);
                active.DeviceName = "Active device";
                var unused = CreateDriver("driver.inf", 1);
                list.SetObjects(new[] { active, unused });
                list.CheckedObjects = new[] { active };

                Invoke(form, handler, null, EventArgs.Empty);

                CollectionAssert.AreEquivalent(new[] { unused }, list.CheckedObjects.Cast<DriverStoreEntry>().ToArray());
            });
        }

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

        private static void WithOperationForm(Action<DSEForm, MyObjectListView> test)
        {
            WithSelectionForm((form, list) =>
            {
                var controls = new List<IDisposable>();

                try
                {
                    foreach (var field in typeof(DSEForm).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                    {
                        if (field.GetValue(form) == null
                            && (typeof(Control).IsAssignableFrom(field.FieldType) || typeof(ToolStripItem).IsAssignableFrom(field.FieldType)))
                        {
                            var control = Activator.CreateInstance(field.FieldType);
                            field.SetValue(form, control);
                            controls.Add((IDisposable)control);
                        }
                    }

                    test(form, list);
                }
                finally
                {
                    foreach (var control in controls)
                    {
                        control.Dispose();
                    }
                }
            });
        }

        private static bool IsActionEnabled(DSEForm form, string name)
        {
            var action = typeof(DSEForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
            return action is Control control ? control.Enabled : ((ToolStripItem)action).Enabled;
        }
    }
}
