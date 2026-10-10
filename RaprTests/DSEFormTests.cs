using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

using BrightIdeasSoftware;

using JR.Utils.GUI.Forms;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Lang;
using Rapr.Utils;

namespace Rapr.Tests
{
    [STATestClass]
    public class DSEFormTests
    {
        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void SuccessfulDriverExportsClearTheReminderOnlyForBackedUpDrivers(bool exportAllSelected)
        {
            WithOperationForm((form, list) =>
            {
                SetField(form, "driverStore", new ExportTestDriverStore());
                var first = CreateDriver("first.inf", 1);
                var second = CreateDriver("second.inf", 1);
                list.SetObjects(new[] { first, second });
                Invoke(form, "CtxMenuSelectUnusedDrivers_Click", null, EventArgs.Empty);

                var exported = exportAllSelected ? new[] { first, second } : new[] { first };
                var task = (Task)typeof(DSEForm)
                    .GetMethod("ExportDrivers", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(form, new object[] { exported, @"C:\TestBackup" });
                var timeout = Stopwatch.StartNew();
                while (!task.IsCompleted)
                {
                    Application.DoEvents();
                    Assert.IsTrue(timeout.Elapsed < TimeSpan.FromSeconds(10), "Driver export did not complete.");
                }

                task.GetAwaiter().GetResult();
                var status = (ToolStripStatusLabel)typeof(DSEForm)
                    .GetField("lblStatus", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                string reminder = Language.ResourceManager.GetString("Status_Review_Unused_Drivers", Language.Culture);
                Assert.AreEqual(!exportAllSelected, status.Text.Contains(reminder));
                Assert.AreEqual(exportAllSelected ? Color.LightGreen : Color.Yellow, status.BackColor);

                Invoke(form, "UpdateCheckedItemSize");

                Assert.AreEqual(!exportAllSelected, status.Text.Contains(reminder));
                list.CheckedObjects = new[] { first };
                Invoke(form, "UpdateCheckedItemSize");

                Assert.IsFalse(status.Text.Contains(reminder));
            });
        }

        [TestMethod]
        [DataRow(DSEForm.Status.Normal, false)]
        [DataRow(DSEForm.Status.Success, true)]
        [DataRow(DSEForm.Status.Error, true)]
        public void OperationResultsKeepTheUnusedDriverReminderWithoutHidingErrors(DSEForm.Status result, bool operation)
        {
            WithOperationForm((form, list) =>
            {
                list.SetObjects(new[] { CreateDriver("unused.inf", 1) });
                Invoke(form, "CtxMenuSelectUnusedDrivers_Click", null, EventArgs.Empty);
                if (operation)
                {
                    Invoke(form, "StartOperation");
                }

                string message = result == DSEForm.Status.Error
                    ? Language.Message_Export_Drivers_Error
                    : Language.Message_Export_Drivers_Success;
                Invoke(form, "ShowStatus", result, message, null, false);
                if (operation)
                {
                    Invoke(form, "EndOperation");
                }

                var status = (ToolStripStatusLabel)typeof(DSEForm)
                    .GetField("lblStatus", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                string reminder = Language.ResourceManager.GetString("Status_Review_Unused_Drivers", Language.Culture);
                StringAssert.Contains(status.Text, message);
                StringAssert.Contains(status.Text, reminder);
                Assert.AreEqual(
                    result == DSEForm.Status.Error ? Color.FromArgb(0xFF, 0x00, 0x33) : Color.Yellow,
                    status.BackColor);
            });
        }

        [TestMethod]
        public void UnusedDriverReviewWarningSurvivesSelectionUpdates()
        {
            WithOperationForm((form, list) =>
            {
                var unused = CreateDriver("unused.inf", 1);
                var active = CreateDriver("active.inf", 1);
                active.DeviceName = "Active device";
                list.SetObjects(new[] { unused, active });
                var status = (ToolStripStatusLabel)typeof(DSEForm)
                    .GetField("lblStatus", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                string reminder = Language.ResourceManager.GetString("Status_Review_Unused_Drivers", Language.Culture);

                Invoke(form, "CtxMenuSelectUnusedDrivers_Click", null, EventArgs.Empty);

                Assert.AreEqual(Color.Yellow, status.BackColor);
                StringAssert.Contains(status.Text, reminder);

                list.CheckedObjects = new[] { unused, active };
                Invoke(form, "UpdateCheckedItemSize");

                Assert.AreEqual(Color.Yellow, status.BackColor);
                StringAssert.Contains(status.Text, reminder);
                StringAssert.Contains(
                    status.Text,
                    string.Format(Language.Status_Selected_Drivers, 2, DriverStoreEntry.GetBytesReadable(0)));

                list.CheckedObjects = new[] { active };
                Invoke(form, "UpdateCheckedItemSize");

                Assert.AreNotEqual(Color.Yellow, status.BackColor);
                Assert.IsFalse(status.Text.Contains(reminder));

                list.CheckedObjects = Array.Empty<DriverStoreEntry>();
                Invoke(form, "UpdateCheckedItemSize");

                Assert.AreEqual(Language.Status_No_Drivers_Selected, status.Text);
                Assert.AreNotEqual(Color.Yellow, status.BackColor);
            });
        }

        [TestMethod]
        [DataRow(MessageBoxButtons.OKCancel, MessageBoxDefaultButton.Button1, DialogResult.OK)]
        [DataRow(MessageBoxButtons.OKCancel, MessageBoxDefaultButton.Button2, DialogResult.Cancel)]
        [DataRow(MessageBoxButtons.OKCancel, MessageBoxDefaultButton.Button3, DialogResult.Cancel)]
        [DataRow(MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, DialogResult.OK)]
        [DataRow(MessageBoxButtons.YesNoCancel, MessageBoxDefaultButton.Button2, DialogResult.No)]
        public void MessageBoxDefaultFocusMatchesTheVisibleButton(
            MessageBoxButtons buttons,
            MessageBoxDefaultButton defaultButton,
            DialogResult expected)
        {
            var dialogType = typeof(FlexibleMessageBox).GetNestedType("FlexibleMessageBoxForm", BindingFlags.NonPublic);
            using (var dialog = (Form)Activator.CreateInstance(dialogType, true))
            using (var timer = new Timer { Interval = 1 })
            {
                var bindingSource = (BindingSource)dialogType
                    .GetField("FlexibleMessageBoxFormBindingSource", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(dialog);
                bindingSource.DataSource = dialog;
                dialogType.GetMethod("SetDialogButtons", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new object[] { dialog, buttons, defaultButton });
                DialogResult focusedResult = DialogResult.None;
                timer.Tick += (sender, args) =>
                {
                    timer.Stop();
                    if (dialog.ActiveControl is Button button && button.Focused)
                    {
                        focusedResult = button.DialogResult;
                    }

                    dialog.Close();
                };

                timer.Start();
                dialog.ShowDialog();

                Assert.AreEqual(expected, focusedResult);
            }
        }

        [TestMethod]
        [DataRow("CtxMenuSelectUnusedDrivers_Click", false)]
        [DataRow("ButtonSelectUnusedDrivers_Click", false)]
        [DataRow("CtxMenuSelectUnusedDrivers_Click", true)]
        [DataRow("ButtonSelectUnusedDrivers_Click", true)]
        public void UnusedDriverSelectionRequiresConfirmationBeforeChangingChecks(string handler, bool confirmed)
        {
            WithSelectionForm((form, list) =>
            {
                var active = CreateDriver("active.inf", 1);
                active.DeviceName = "Active device";
                var unused = CreateDriver("unused.inf", 1);
                list.SetObjects(new[] { active, unused });
                list.CheckedObjects = new[] { active };
                var testForm = (SelectionTestForm)form;
                testForm.SelectionConfirmed = confirmed;
                testForm.OnConfirmation = () =>
                    CollectionAssert.AreEquivalent(new[] { active }, list.CheckedObjects.Cast<DriverStoreEntry>().ToArray());

                Invoke(form, handler, null, EventArgs.Empty);

                Assert.AreEqual(1, testForm.ConfirmationCount);
                CollectionAssert.AreEquivalent(
                    new[] { confirmed ? unused : active },
                    list.CheckedObjects.Cast<DriverStoreEntry>().ToArray());
            });
        }

        [TestMethod]
        [DataRow(true, false)]
        [DataRow(false, true)]
        public void UnavailableUnusedDriverSelectionDoesNotShowConfirmation(bool operationInProgress, bool offline)
        {
            WithSelectionForm((form, list) =>
            {
                list.SetObjects(new[] { CreateDriver("unused.inf", 1) });
                SetField(form, "operationInProgress", operationInProgress);
                if (offline)
                {
                    SetField(form, "driverStore", new DismUtil(@"C:\ReviewImage"));
                }

                Invoke(form, "CtxMenuSelectUnusedDrivers_Click", null, EventArgs.Empty);

                Assert.AreEqual(0, ((SelectionTestForm)form).ConfirmationCount);
            });
        }

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
            var form = (SelectionTestForm)FormatterServices.GetUninitializedObject(typeof(SelectionTestForm));
            GC.SuppressFinalize(form);
            form.SelectionConfirmed = true;

            using (var list = new MyObjectListView { CheckBoxes = true })
            using (var status = new ToolStripStatusLabel())
            {
                list.Columns.Add(new OLVColumn { AspectName = "DriverInfName" });
                SetField(form, "lstDriverStoreEntries", list);
                SetField(form, "lblStatus", status);
                SetField(form, "driverStore", new DismUtil());
                SetField(form, "driversWithNewerDate", new HashSet<DriverStoreEntry>());
                SetField(form, "unusedDriversToReview", new HashSet<DriverStoreEntry>());
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

        private sealed class SelectionTestForm : DSEForm
        {
            public bool SelectionConfirmed { get; set; }

            public int ConfirmationCount { get; private set; }

            public Action OnConfirmation { get; set; }

            protected override bool ConfirmUnusedDriverSelection()
            {
                this.ConfirmationCount++;
                this.OnConfirmation?.Invoke();
                return this.SelectionConfirmed;
            }
        }

        private sealed class ExportTestDriverStore : IDriverStore
        {
            public DriverStoreType Type => DriverStoreType.Online;

            public string OfflineStoreLocation => null;

            public bool SupportAddInstall => false;

            public bool SupportForceDeletion => false;

            public bool SupportDeviceNameColumn => true;

            public bool SupportExportDriver => true;

            public bool SupportExportAllDrivers => true;

            public List<DriverStoreEntry> EnumeratePackages() => throw new NotSupportedException();

            public bool DeleteDriver(DriverStoreEntry entry, bool forceDelete) => throw new NotSupportedException();

            public bool AddDriver(string path, bool install) => throw new NotSupportedException();

            public bool ExportDriver(DriverStoreEntry entry, string destinationPath) => true;

            public bool ExportAllDrivers(string destinationPath) => throw new NotSupportedException();
        }
    }
}
