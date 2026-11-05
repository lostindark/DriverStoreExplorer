using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using BrightIdeasSoftware;

using JR.Utils.GUI.Forms;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rapr.Controls;
using Rapr.Lang;
using Rapr.Utils;

namespace Rapr.Tests
{
    // These tests build WinForms controls and pump window messages. Run them on their own
    // instead of alongside the other test classes.
    [STATestClass]
    [DoNotParallelize]
    public class DSEFormTests
    {
        [TestMethod]
        [DataRow(ObjectListViewBase.ColumnSelectBehaviour.InlineMenu)]
        [DataRow(ObjectListViewBase.ColumnSelectBehaviour.Submenu)]
        public void ColumnSelectionMenuExcludesOnlyBlankSpacerColumns(ObjectListViewBase.ColumnSelectBehaviour behaviour)
        {
            using (var list = new DriverStoreListView { SelectColumnsOnRightClickBehaviour = behaviour })
            using (var menu = new ContextMenuStrip())
            {
                var visible = new OLVColumn("INF", nameof(DriverStoreEntry.DriverInfName));
                var hidden = new OLVColumn("Driver Files", nameof(DriverStoreEntry.DriverFiles)) { IsVisible = false };
                var unnamed = new OLVColumn { Text = string.Empty };
                var spacer = new OLVColumn { FillsFreeSpace = true, Text = string.Empty };
                list.AllColumns.AddRange(new[] { visible, hidden, unnamed, spacer });
                list.Columns.AddRange(new ColumnHeader[] { visible, unnamed, spacer });

                list.MakeColumnSelectMenu(menu);

                var items = behaviour == ObjectListViewBase.ColumnSelectBehaviour.Submenu
                    ? ((ToolStripMenuItem)menu.Items[0]).DropDownItems
                    : menu.Items;
                CollectionAssert.AreEquivalent(
                    new[] { visible, hidden, unnamed },
                    items.Cast<ToolStripItem>().Select(item => item.Tag).ToArray());
                Assert.IsTrue(spacer.IsVisible);
                Assert.IsTrue(list.Columns.Contains(spacer));
                Assert.AreEqual(4, list.AllColumns.Count);
            }
        }

        [TestMethod]
        public void ThemeChangesPreserveWarningStatusAndSearchSelection()
        {
            var previousMode = ObjectListViewTheme.ColorMode;
            try
            {
                WithOperationForm((form, list) =>
                {
                    list.UseFiltering = true;
                    var entry = CreateDriver("first.inf", 1);
                    list.SetObjects(new[] { entry, CreateDriver("second.inf", 1) });
                    list.SelectedObject = entry;
                    list.CheckedObject = entry;
                    var search = (EnhancedSearchTextBox)typeof(DSEForm)
                        .GetField("textBoxSearch", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                    search.Text = "first";
                    Invoke(form, "ShowStatus", DSEForm.Status.Warning, "Review", null, false);
                    var palette = new ThemePalette(true, false);

                    Invoke(form, "ApplyTheme", palette);

                    var status = (ToolStripStatusLabel)typeof(DSEForm)
                        .GetField("lblStatus", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                    Assert.AreEqual("Review", status.Text);
                    Assert.AreEqual(Color.FromArgb(100, 75, 15), status.BackColor);
                    Assert.AreEqual(palette.TextColor, status.ForeColor);
                    Assert.AreEqual("first", search.Text);
                    Assert.AreSame(entry, list.SelectedObject);
                    Assert.AreSame(entry, list.CheckedObject);
                    Assert.AreEqual(1, list.GetItemCount());

                    Invoke(form, "ShowStatus", DSEForm.Status.Normal, "Ready", null, false);
                    Assert.AreNotEqual(Color.FromArgb(100, 75, 15), status.BackColor);
                });
            }
            finally
            {
                ObjectListViewTheme.ColorMode = previousMode;
            }
        }

        [TestMethod]
        public void ColumnAndFilterMenusFollowLanguageChanges()
        {
            var originalCulture = Language.Culture;
            var originalLabels = new[]
            {
                FilterMenuBuilder.FILTERING_LABEL, FilterMenuBuilder.CLEAR_ALL_FILTERS_LABEL,
                FilterMenuBuilder.APPLY_LABEL, FilterMenuBuilder.SELECT_ALL_LABEL
            };
            try
            {
                WithOperationForm((form, list) =>
                {
                    list.SetObjects(new[] { CreateDriver("first.inf", 1) });
                    var filesColumn = (OLVColumn)typeof(DSEForm)
                        .GetField("binaryFilesColumn", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                    list.AllColumns.Add(filesColumn);
                    var cultures = new[] { "en-US", "fr-FR", "zh-CN", "en-US" };
                    var labels = new[]
                    {
                        new[] { "Driver Files", "Filtering", "Clear All Filters", "Apply", "Select All" },
                        new[] { "Fichiers du pilote", "Filtrage", "Effacer tous les filtres", "Appliquer", "Tout sélectionner" },
                        new[] { "驱动文件", "筛选", "清除所有筛选", "应用", "全选" },
                        new[] { "Driver Files", "Filtering", "Clear All Filters", "Apply", "Select All" }
                    };
                    for (int i = 0; i < cultures.Length; i++)
                    {
                        Language.Culture = CultureInfo.GetCultureInfo(cultures[i]);
                        Invoke(form, "SetupListViewColumns");
                        using (var columnsMenu = new ContextMenuStrip())
                        using (var filterMenu = new ContextMenuStrip())
                        {
                            list.MakeColumnSelectMenu(columnsMenu);
                            Assert.AreEqual(labels[i][0], columnsMenu.Items[0].Text);

                            list.MakeFilteringMenu(filterMenu, 0);
                            var filtering = (ToolStripMenuItem)filterMenu.Items[0];
                            Assert.AreEqual(labels[i][1], filtering.Text);
                            Assert.AreEqual(labels[i][2], filtering.DropDownItems[0].Text);
                            Assert.AreEqual(labels[i][3], filtering.DropDownItems[3].Text);
                            var choices = (ToolStripCheckedListBox)filtering.DropDownItems[2];
                            Assert.AreEqual(labels[i][4], choices.Items[choices.Items.Count - 1]);
                        }
                    }
                });
            }
            finally
            {
                Language.Culture = originalCulture;
                FilterMenuBuilder.FILTERING_LABEL = originalLabels[0];
                FilterMenuBuilder.CLEAR_ALL_FILTERS_LABEL = originalLabels[1];
                FilterMenuBuilder.APPLY_LABEL = originalLabels[2];
                FilterMenuBuilder.SELECT_ALL_LABEL = originalLabels[3];
            }
        }

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
        [DataRow("CtxMenuSelectUnusedDrivers_Click")]
        [DataRow("ButtonSelectUnusedDrivers_Click")]
        public void DisabledUnusedDriverWarningSelectsWithoutConfirmation(string handler)
        {
            // Settings is internal to Rapr, so reach it through ApplicationSettingsBase.
            var settings = (ApplicationSettingsBase)typeof(DSEForm).Assembly
                .GetType("Rapr.Properties.Settings")
                .GetProperty("Default", BindingFlags.Static | BindingFlags.Public)
                .GetValue(null);
            object previous = settings["WarnBeforeSelectingUnusedDrivers"];
            settings["WarnBeforeSelectingUnusedDrivers"] = false;

            try
            {
                WithSelectionForm((form, list) =>
                {
                    var unused = CreateDriver("unused.inf", 1);
                    list.SetObjects(new[] { unused });

                    Invoke(form, handler, null, EventArgs.Empty);

                    Assert.AreEqual(0, ((SelectionTestForm)form).ConfirmationCount);
                    CollectionAssert.AreEquivalent(new[] { unused }, list.CheckedObjects.Cast<DriverStoreEntry>().ToArray());
                });
            }
            finally
            {
                settings["WarnBeforeSelectingUnusedDrivers"] = previous;
            }
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

        [TestMethod]
        public void TypedDriverSelectionCanBeToggledAndInverted()
        {
            WithSelectionForm((form, list) =>
            {
                var first = CreateDriver("first.inf", 1);
                var second = CreateDriver("second.inf", 1);
                var third = CreateDriver("third.inf", 1);
                list.CreateControl();
                list.SetObjects(new[] { first, second, third });
                list.SelectedObjects = new[] { first, second };
                list.CheckedObjects = new[] { first, second };

                Invoke(form, "CtxMenuSelect_Click", null, EventArgs.Empty);
                Assert.AreEqual(0, list.CheckedObjects.Count);

                Invoke(form, "CtxMenuSelect_Click", null, EventArgs.Empty);
                CollectionAssert.AreEquivalent(new[] { first, second }, list.CheckedObjects.ToArray());

                Invoke(form, "CtxMenuInvertSelection_Click", null, EventArgs.Empty);
                CollectionAssert.AreEquivalent(new[] { third }, list.CheckedObjects.ToArray());
            });
        }

        [TestMethod]
        public void CheckActionsLeaveDriversHiddenBySearchUnchanged()
        {
            WithSelectionForm((form, list) =>
            {
                var first = CreateDriver("first.inf", 1);
                var second = CreateDriver("second.inf", 1);
                var hiddenChecked = CreateDriver("hidden-checked.inf", 1);
                var hiddenUnchecked = CreateDriver("hidden-unchecked.inf", 1);
                list.CreateControl();
                list.UseFiltering = true;
                list.SetObjects(new[] { first, second, hiddenChecked, hiddenUnchecked });
                list.CheckedObjects = new[] { hiddenChecked };
                list.ModelFilter = new ModelFilter(model => !((DriverStoreEntry)model).DriverInfName.StartsWith("hidden", StringComparison.Ordinal));

                void AssertHiddenUnchanged()
                {
                    Assert.IsTrue(list.IsChecked(hiddenChecked));
                    Assert.IsFalse(list.IsChecked(hiddenUnchecked));
                }

                Invoke(form, "CtxMenuSelectAll_Click", null, EventArgs.Empty);
                CollectionAssert.AreEquivalent(new[] { first, second }, list.CheckedObjects.ToArray());
                AssertHiddenUnchanged();

                Invoke(form, "CtxMenuSelectAll_Click", null, EventArgs.Empty);
                Assert.AreEqual(0, list.CheckedObjects.Count);
                AssertHiddenUnchanged();

                list.CheckedObjects = new[] { first, hiddenChecked };
                Invoke(form, "CtxMenuInvertSelection_Click", null, EventArgs.Empty);
                CollectionAssert.AreEquivalent(new[] { second }, list.CheckedObjects.ToArray());
                AssertHiddenUnchanged();

                list.SelectedObjects = new[] { first };
                Invoke(form, "CtxMenuSelect_Click", null, EventArgs.Empty);
                CollectionAssert.AreEquivalent(new[] { first, second }, list.CheckedObjects.ToArray());
                AssertHiddenUnchanged();

                list.ModelFilter = null;
                CollectionAssert.AreEquivalent(new[] { first, second, hiddenChecked }, list.CheckedObjects.ToArray());
            });
        }

        [TestMethod]
        [DataRow("CtxMenuSelectOldDrivers_Click")]
        [DataRow("CtxMenuSelectUnusedDrivers_Click")]
        public void StoreWideSelectionClearsTheSearchFirst(string handler)
        {
            WithOperationForm((form, list) =>
            {
                var active = CreateDriver("driver.inf", 2);
                active.DeviceName = "Active device";
                var unused = CreateDriver("driver.inf", 1);
                list.CreateControl();
                list.UseFiltering = true;
                list.SetObjects(new[] { active, unused });
                var search = (Control)typeof(DSEForm)
                    .GetField("textBoxSearch", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                search.Text = "Active device";
                Invoke(form, "UpdateSearchFilter");
                Assert.AreEqual(1, list.GetItemCount());

                Invoke(form, handler, null, EventArgs.Empty);

                Assert.IsNull(list.ModelFilter);
                Assert.AreEqual(string.Empty, search.Text);
                Assert.AreEqual(2, list.GetItemCount());
                CollectionAssert.AreEquivalent(new[] { unused }, list.CheckedObjects.ToArray());
            });
        }

        [TestMethod]
        [DataRow("CtxMenuSelectOldDrivers_Click")]
        [DataRow("CtxMenuSelectUnusedDrivers_Click")]
        public void StoreWideSelectionClearsASearchThatIsNotAppliedYet(string handler)
        {
            WithOperationForm((form, list) =>
            {
                var active = CreateDriver("driver.inf", 2);
                active.DeviceName = "Active device";
                var unused = CreateDriver("driver.inf", 1);
                list.CreateControl();
                list.UseFiltering = true;
                list.SetObjects(new[] { active, unused });

                using (var debounce = new Timer { Interval = 300 })
                {
                    SetField(form, "searchDebounceTimer", debounce);
                    var search = (Control)typeof(DSEForm)
                        .GetField("textBoxSearch", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);

                    // Text was typed and the debounce timer is running, but the filter is not applied yet.
                    search.Text = "Active device";
                    debounce.Start();
                    Assert.IsNull(list.ModelFilter);

                    Invoke(form, handler, null, EventArgs.Empty);

                    Assert.IsFalse(debounce.Enabled);
                    Assert.AreEqual(string.Empty, search.Text);

                    // A late debounce tick must not hide the drivers that were just checked.
                    Invoke(form, "UpdateSearchFilter");
                    Assert.AreEqual(2, list.GetItemCount());
                    CollectionAssert.AreEquivalent(new[] { unused }, list.CheckedObjects.ToArray());
                    SetField(form, "searchDebounceTimer", null);
                }
            });
        }

        [TestMethod]
        public void CancelledUnusedDriverSelectionKeepsTheSearch()
        {
            // Settings is internal to Rapr, so reach it through ApplicationSettingsBase.
            var settings = (ApplicationSettingsBase)typeof(DSEForm).Assembly
                .GetType("Rapr.Properties.Settings")
                .GetProperty("Default", BindingFlags.Static | BindingFlags.Public)
                .GetValue(null);
            object previous = settings["WarnBeforeSelectingUnusedDrivers"];
            settings["WarnBeforeSelectingUnusedDrivers"] = true;

            WithOperationForm((form, list) =>
            {
                ((SelectionTestForm)form).SelectionConfirmed = false;
                try
                {
                    var active = CreateDriver("driver.inf", 2);
                    active.DeviceName = "Active device";
                    list.CreateControl();
                    list.UseFiltering = true;
                    list.SetObjects(new[] { active, CreateDriver("driver.inf", 1) });
                    var search = (Control)typeof(DSEForm)
                        .GetField("textBoxSearch", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                    search.Text = "Active device";
                    Invoke(form, "UpdateSearchFilter");

                    Invoke(form, "CtxMenuSelectUnusedDrivers_Click", null, EventArgs.Empty);

                    Assert.AreEqual("Active device", search.Text);
                    Assert.IsNotNull(list.ModelFilter);
                    Assert.AreEqual(0, list.CheckedObjects.Count);
                }
                finally
                {
                    settings["WarnBeforeSelectingUnusedDrivers"] = previous;
                }
            });
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void DetailsPanePreservesResizedWidthAcrossLayoutChanges(bool rightToLeft)
        {
            var settings = (ApplicationSettingsBase)typeof(DSEForm).Assembly
                .GetType("Rapr.Properties.Settings")
                .GetProperty("Default", BindingFlags.Static | BindingFlags.Public)
                .GetValue(null);
            object previous = settings["ShowDetailsPane"];
            object previousWidth = settings["DetailsPaneWidth"];
            var form = (DSEForm)FormatterServices.GetUninitializedObject(typeof(DSEForm));
            typeof(Form).GetConstructor(Type.EmptyTypes).Invoke(form, null);
            try
            {
                settings["ShowDetailsPane"] = false;
                settings["DetailsPaneWidth"] = 360;
                Invoke(form, "InitializeComponent");
                Invoke(form, "SetupDetailsPane");
                form.RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No;
                var column = (Panel)typeof(DSEForm)
                    .GetField("operationsColumn", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                var splitter = (Splitter)typeof(DSEForm)
                    .GetField("detailsSplitter", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                int compactWidth = column.Width;
                int compactMinimumWidth = form.MinimumSize.Width;

                settings["ShowDetailsPane"] = true;
                Invoke(form, "ArrangeOperationsColumn");
                Assert.IsGreaterThan(compactWidth, column.Width);
                Assert.AreEqual(rightToLeft ? DockStyle.Left : DockStyle.Right, column.Dock);
                Assert.AreEqual(column.Dock, splitter.Dock);
                Assert.IsGreaterThan(compactMinimumWidth, form.MinimumSize.Width);

                splitter.SplitPosition = column.Width + 60;
                int expandedWidth = column.Width;
                int savedWidth = (int)settings["DetailsPaneWidth"];
                Assert.IsGreaterThan(360, savedWidth);
                var operations = (GroupBox)typeof(DSEForm)
                    .GetField("gbOptions", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                var actions = operations.Controls.OfType<TableLayoutPanel>().Single();
                int buttonHeight = actions.Controls.OfType<Button>().First().Height;
                foreach (Button button in actions.Controls.OfType<Button>())
                {
                    Assert.AreEqual(actions.DisplayRectangle.Width - button.Margin.Horizontal, button.Width, button.Name);
                    Assert.AreEqual(buttonHeight, button.Height, button.Name);
                }

                int allActionsHeight = operations.Height;
                var exportButton = actions.Controls.OfType<Button>().Single(button => button.Name == "buttonExportDrivers");
                exportButton.Visible = false;
                actions.PerformLayout();
                Assert.IsLessThan(allActionsHeight, operations.Height);
                exportButton.Visible = true;
                actions.PerformLayout();
                Assert.AreEqual(allActionsHeight, operations.Height);

                form.Width += 200;
                Assert.AreEqual(expandedWidth, column.Width);

                settings["ShowDetailsPane"] = false;
                Invoke(form, "ArrangeOperationsColumn");
                Assert.AreEqual(compactWidth, column.Width);
                Assert.AreEqual(compactMinimumWidth, form.MinimumSize.Width);

                settings["ShowDetailsPane"] = true;
                Invoke(form, "ArrangeOperationsColumn");
                Assert.AreEqual(expandedWidth, column.Width);

                int largeFormWidth = form.Width;
                form.Width = form.MinimumSize.Width;
                var list = (Control)typeof(DSEForm)
                    .GetField("lstDriverStoreEntries", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                Assert.IsGreaterThanOrEqualTo(splitter.MinExtra - list.Margin.Horizontal, list.Width);
                Assert.AreEqual(savedWidth, settings["DetailsPaneWidth"]);
                form.Width = largeFormWidth;
                Assert.AreEqual(expandedWidth, column.Width);

                form.Controls.Clear();
                Invoke(form, "InitializeComponent");
                Invoke(form, "SetupDetailsPane");
                column = (Panel)typeof(DSEForm)
                    .GetField("operationsColumn", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                Assert.AreEqual(expandedWidth, column.Width);
            }
            finally
            {
                settings["ShowDetailsPane"] = previous;
                settings["DetailsPaneWidth"] = previousWidth;
                form.Dispose();
            }
        }

        [TestMethod]
        [DataRow(FormWindowState.Normal)]
        [DataRow(FormWindowState.Maximized)]
        public void MinimizingPreservesTheWindowRestoreBounds(FormWindowState initialState)
        {
            var settings = (ApplicationSettingsBase)typeof(DSEForm).Assembly
                .GetType("Rapr.Properties.Settings")
                .GetProperty("Default", BindingFlags.Static | BindingFlags.Public)
                .GetValue(null);
            object previous = settings["ShowDetailsPane"];
            object previousWidth = settings["DetailsPaneWidth"];
            var form = (SelectionTestForm)FormatterServices.GetUninitializedObject(typeof(SelectionTestForm));
            typeof(Form).GetConstructor(Type.EmptyTypes).Invoke(form, null);
            try
            {
                settings["ShowDetailsPane"] = true;
                settings["DetailsPaneWidth"] = 360;
                Invoke(form, "InitializeComponent");
                Invoke(form, "SetupDetailsPane");
                form.Load -= (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), form,
                    typeof(DSEForm).GetMethod("DSEForm_Load", BindingFlags.Instance | BindingFlags.NonPublic));
                form.Shown -= (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), form,
                    typeof(DSEForm).GetMethod("DSEForm_Shown", BindingFlags.Instance | BindingFlags.NonPublic));
                form.FormClosing -= (FormClosingEventHandler)Delegate.CreateDelegate(typeof(FormClosingEventHandler), form,
                    typeof(DSEForm).GetMethod("DSEForm_FormClosing", BindingFlags.Instance | BindingFlags.NonPublic));
                form.ShowInTaskbar = false;
                form.Opacity = 0;
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-20000, -20000);
                form.Size = new Size(1100, 800);
                form.Show();
                Application.DoEvents();
                Rectangle normalBounds = form.Bounds;
                form.WindowState = initialState;
                Application.DoEvents();
                Size minimumSize = form.MinimumSize;

                form.WindowState = FormWindowState.Minimized;
                Application.DoEvents();

                Assert.AreEqual(normalBounds, form.RestoreBounds);
                Assert.AreEqual(minimumSize, form.MinimumSize);
                form.WindowState = initialState;
                Application.DoEvents();
                Assert.AreEqual(initialState, form.WindowState);
                Assert.AreEqual(normalBounds, form.RestoreBounds);
                if (initialState == FormWindowState.Normal)
                {
                    Assert.AreEqual(normalBounds, form.Bounds);
                }
            }
            finally
            {
                settings["ShowDetailsPane"] = previous;
                settings["DetailsPaneWidth"] = previousWidth;
                form.Dispose();
            }
        }

        [TestMethod]
        [DataRow(8.25f)]
        [DataRow(12f)]
        [DataRow(16f)]
        public void DriverDetailsRemainSelectableWhileLongValuesReflow(float fontSize)
        {
            using (var font = new Font(SystemFonts.MessageBoxFont.FontFamily, fontSize))
            using (var panel = new DriverDetailsPanel { Font = font, Size = new Size(200, 700) })
            {
                var entry = CreateDriver("driver.inf", 1);
                entry.DriverPublishedName = "oem1.inf";
                entry.BootCritical = true;
                entry.DeviceId = string.Join("\\", Enumerable.Repeat("LONG_DEVICE_IDENTIFIER_0123456789", 5));
                entry.DriverSignerName = string.Join(" ", Enumerable.Repeat("Contoso Hardware Publisher", 5));
                entry.DriverFolderLocation = @"C:\Windows\System32\DriverStore\FileRepository\"
                    + string.Join("_", Enumerable.Repeat("long_driver_folder_name", 5));
                panel.CreateControl();
                var insights = new DriverInsights(new[] { entry }, devicesKnown: true);
                panel.ShowEntry(entry, insights);
                var content = panel.Controls.OfType<RichTextBox>().Single();
                Assert.AreEqual(1, panel.Controls.Count);
                Assert.IsTrue(content.ReadOnly);
                Assert.IsFalse(content.Text.Contains("\n\n"));
                Assert.IsFalse(content.Text.EndsWith("\n", StringComparison.Ordinal));
                StringAssert.Contains(content.Text, entry.DriverSignerName);
                StringAssert.Contains(content.Text, entry.DriverFolderLocation);
                foreach (string caption in new[] { Language.Column_Oem_Inf, Language.Column_Size, Language.Column_DeviceId })
                {
                    content.Select(content.Text.IndexOf(caption, StringComparison.Ordinal), caption.Length);
                    Assert.AreEqual(SystemColors.GrayText.ToArgb(), content.SelectionColor.ToArgb());
                }

                int sizeHeader = content.Text.IndexOf(Language.Column_Size, StringComparison.Ordinal);
                int headerSpacing = GetParagraphSpaceBefore(content, sizeHeader);
                int valueSpacing = GetParagraphSpaceBefore(content, sizeHeader + Language.Column_Size.Length + 1);
                Assert.IsGreaterThan(valueSpacing, headerSpacing);

                content.Select(0, entry.DriverInfName.Length);
                using (var titleFont = content.SelectionFont)
                {
                    Assert.IsTrue(titleFont.Bold);
                    Assert.IsGreaterThan(font.SizeInPoints, titleFont.SizeInPoints);
                }

                int folderIndex = content.Text.LastIndexOf(entry.DriverFolderLocation, StringComparison.Ordinal);
                content.Select(folderIndex, entry.DriverFolderLocation.Length);
                Assert.AreEqual(SystemColors.HotTrack.ToArgb(), content.SelectionColor.ToArgb());
                using (var folderFont = content.SelectionFont)
                {
                    Assert.IsTrue(folderFont.Underline);
                }

                string tag = Language.ResourceManager.GetString("Details_Tag_Boot_Critical", Language.Culture);
                int tagIndex = content.Text.IndexOf(tag, StringComparison.Ordinal);
                Assert.AreEqual(0, GetParagraphSpaceBefore(content, tagIndex));
                content.Select(tagIndex, tag.Length);
                Assert.AreNotEqual(content.BackColor.ToArgb(), content.SelectionBackColor.ToArgb());

                int firstField = content.Text.IndexOf(Language.Column_Oem_Inf, StringComparison.Ordinal);
                int lastField = content.Text.IndexOf(entry.DeviceId, StringComparison.Ordinal) + entry.DeviceId.Length;
                content.Select(firstField, lastField - firstField);
                string selectedText = content.SelectedText;
                StringAssert.Contains(selectedText, entry.DriverPublishedName);
                StringAssert.Contains(selectedText, entry.DeviceId);
                int narrowLines = content.GetLineFromCharIndex(content.TextLength - 1);
                string document = content.Text;

                panel.Width = 500;

                Assert.IsLessThan(narrowLines, content.GetLineFromCharIndex(content.TextLength - 1));
                Assert.AreEqual(document, content.Text);
                Assert.AreEqual(selectedText, content.SelectedText);

                panel.ShowEntry(CreateDriver("other.inf", 2), null);
                Assert.IsFalse(content.Text.Contains(entry.DeviceId));
                Assert.IsFalse(content.Text.EndsWith("\n", StringComparison.Ordinal));
                panel.ShowEntry(entry, insights);
                Assert.AreEqual(document, content.Text);

                using (var largerFont = new Font(font.FontFamily, font.SizeInPoints * 1.25f))
                {
                    panel.Font = largerFont;
                    Assert.AreEqual(document, content.Text);
                    panel.Font = font;
                }

                panel.Scale(new SizeF(1.5f, 1.5f));
                content.SelectAll();
                Assert.AreEqual(document, content.SelectedText);
            }
        }

        [TestMethod]
        [DataRow(MouseButtons.Left, Keys.None, true, false, RightToLeft.No)]
        [DataRow(MouseButtons.Left, Keys.Control, true, true, RightToLeft.No)]
        [DataRow(MouseButtons.Right, Keys.Control, true, false, RightToLeft.No)]
        [DataRow(MouseButtons.Left, Keys.Control, false, false, RightToLeft.No)]
        [DataRow(MouseButtons.Left, Keys.Control, true, true, RightToLeft.Yes)]
        public void FolderPathOpensOnlyOnControlLeftClick(
            MouseButtons button, Keys modifiers, bool onFolder, bool shouldOpen, RightToLeft rightToLeft)
        {
            using (var panel = new DriverDetailsPanel { Size = new Size(360, 900), RightToLeft = rightToLeft })
            {
                var entry = CreateDriver("driver.inf", 1);
                entry.DriverFolderLocation = @"C:\Windows\System32\DriverStore\FileRepository\driver";
                entry.DriverPkgProvider = entry.DriverFolderLocation;
                panel.ShowEntry(entry, null);
                var content = panel.Controls.OfType<RichTextBox>().Single();
                int index = onFolder
                    ? content.Text.LastIndexOf(entry.DriverFolderLocation, StringComparison.Ordinal)
                    : content.Text.IndexOf(entry.DriverFolderLocation, StringComparison.Ordinal);
                Point location = content.GetPositionFromCharIndex(index + 3);
                Point nextCharacter = content.GetPositionFromCharIndex(index + 4);
                location.X = Math.Min(location.X, nextCharacter.X)
                    + Math.Max(1, Math.Abs(nextCharacter.X - location.X) * 3 / 4);
                location.Y += content.Font.Height / 2;
                int opened = 0;
                panel.FolderClicked += (sender, e) => opened++;

                typeof(Control).GetMethod("OnMouseMove", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(content, new object[] { new MouseEventArgs(MouseButtons.None, 0, location.X, location.Y, 0) });
                var toolTip = (ToolTip)typeof(DriverDetailsPanel)
                    .GetField("toolTip", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel);
                Assert.AreEqual(onFolder
                    ? Language.ResourceManager.GetString("Details_Folder_Open_Hint", Language.Culture)
                    : string.Empty, toolTip.GetToolTip(content));

                ClickTextBox(content, location, modifiers: modifiers, button: button);

                Assert.AreEqual(shouldOpen ? 1 : 0, opened);
                panel.ShowEntry(null, null);
                ClickTextBox(content, location, modifiers: Keys.Control);
                Assert.AreEqual(shouldOpen ? 1 : 0, opened);
            }
        }

        [TestMethod]
        public void ControlClickOpensAScrolledFolderWithoutChangingTheTextSelection()
        {
            using (var panel = new DriverDetailsPanel { Size = new Size(200, 160) })
            {
                var entry = CreateDriver("driver.inf", 1);
                entry.DriverFolderLocation = @"C:\Windows\System32\DriverStore\FileRepository\driver";
                panel.ShowEntry(entry, null);
                var content = panel.Controls.OfType<RichTextBox>().Single();
                content.Select(0, 0);
                SendTextBoxMessage(content, 0x0115, new IntPtr(7), IntPtr.Zero);
                int folderIndex = content.Text.LastIndexOf(entry.DriverFolderLocation, StringComparison.Ordinal);
                Point location = content.GetPositionFromCharIndex(folderIndex + 3);
                location.Offset(1, content.Font.Height / 2);
                Assert.IsTrue(content.ClientRectangle.Contains(location));
                int opened = 0;
                panel.FolderClicked += (sender, e) => opened++;

                ClickTextBox(content, location, modifiers: Keys.Control);

                Assert.AreEqual(1, opened);
                Assert.AreEqual(0, content.SelectionStart);
                Assert.AreEqual(0, content.SelectionLength);
                Assert.IsFalse(content.Capture);
            }
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void TripleClickSelectsOneLogicalLineOnlyAtTheSameLocation(bool moveThirdClick)
        {
            using (var panel = new DriverDetailsPanel { Size = new Size(300, 700) })
            {
                var entry = CreateDriver("first word and more text.inf", 1);
                panel.ShowEntry(entry, null);
                var textBox = panel.Controls.OfType<RichTextBox>().Single();
                var location = new Point(3, 3);

                ClickTextBox(textBox, location);
                ClickTextBox(textBox, location, doubleClick: true);
                Assert.IsGreaterThan(0, textBox.SelectionLength);
                Assert.AreNotEqual(textBox.Text, textBox.SelectedText);

                if (moveThirdClick)
                {
                    location.X += SystemInformation.DoubleClickSize.Width + 1;
                }

                ClickTextBox(textBox, location);

                Assert.AreEqual(!moveThirdClick, textBox.SelectedText == entry.DriverInfName);
                ClickTextBox(textBox, location);
                Assert.AreNotEqual(entry.DriverInfName, textBox.SelectedText);
            }
        }

        [TestMethod]
        public void TripleClickSelectionExpiresAfterTheSystemDoubleClickInterval()
        {
            using (var panel = new DriverDetailsPanel { Size = new Size(300, 700) })
            {
                var entry = CreateDriver("first word and more text.inf", 1);
                panel.ShowEntry(entry, null);
                var textBox = panel.Controls.OfType<RichTextBox>().Single();
                var location = new Point(3, 3);
                ClickTextBox(textBox, location);
                ClickTextBox(textBox, location, doubleClick: true);
                System.Threading.Thread.Sleep(SystemInformation.DoubleClickTime + 20);

                ClickTextBox(textBox, location);

                Assert.AreNotEqual(entry.DriverInfName, textBox.SelectedText);
            }
        }

        [TestMethod]
        public void TripleClickSelectsTheWholeWrappedFolderPathWithoutOtherFields()
        {
            using (var panel = new DriverDetailsPanel { Size = new Size(160, 900) })
            {
                var entry = CreateDriver("driver.inf", 1);
                entry.DriverFolderLocation = @"C:\driver store\" + new string('x', 100);
                panel.ShowEntry(entry, null);
                var content = panel.Controls.OfType<RichTextBox>().Single();
                int folderIndex = content.Text.LastIndexOf(entry.DriverFolderLocation, StringComparison.Ordinal);
                int wrappedLine = content.GetLineFromCharIndex(folderIndex) + 1;
                int wrappedIndex = content.GetFirstCharIndexFromLine(wrappedLine);
                Assert.IsLessThan(folderIndex + entry.DriverFolderLocation.Length, wrappedIndex);
                Point location = content.GetPositionFromCharIndex(wrappedIndex + 2);
                location.Offset(1, 1);

                ClickTextBox(content, location);
                ClickTextBox(content, location, doubleClick: true);
                ClickTextBox(content, location);

                Assert.AreEqual(entry.DriverFolderLocation, content.SelectedText);
            }
        }

        [TestMethod]
        public void DetailsHideTheNativeCaretWithoutDisablingSelection()
        {
            using (var panel = new DriverDetailsPanel())
            {
                panel.ShowEntry(CreateDriver("driver.inf", 1), null);
                var content = panel.Controls.OfType<RichTextBox>().Single();
                Assert.IsTrue(CaretNativeMethods.CreateCaret(content.Handle, IntPtr.Zero, 1, content.Font.Height));
                try
                {
                    Assert.IsTrue(CaretNativeMethods.ShowCaret(content.Handle));
                    var info = new CaretNativeMethods.GuiThreadInfo
                    {
                        Size = (uint)Marshal.SizeOf<CaretNativeMethods.GuiThreadInfo>(),
                    };
                    uint thread = CaretNativeMethods.GetCurrentThreadId();
                    Assert.IsTrue(CaretNativeMethods.GetGUIThreadInfo(thread, ref info));
                    Assert.AreEqual(1u, info.Flags & 1u);

                    SendTextBoxMessage(content, 0x0007, IntPtr.Zero, IntPtr.Zero);

                    Assert.IsTrue(CaretNativeMethods.GetGUIThreadInfo(thread, ref info));
                    Assert.AreEqual(0u, info.Flags & 1u);
                    content.Select(0, "driver.inf".Length);
                    Assert.AreEqual("driver.inf", content.SelectedText);
                }
                finally
                {
                    CaretNativeMethods.DestroyCaret();
                }
            }
        }

        private static int GetParagraphSpaceBefore(RichTextBox content, int index)
        {
            content.Select(index, 1);
            Match match = Regex.Match(content.SelectedRtf, @"\\sb(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
        }

        private static void ClickTextBox(
            TextBoxBase textBox, Point location, bool doubleClick = false,
            Keys modifiers = Keys.None, MouseButtons button = MouseButtons.Left)
        {
            int control = (modifiers & Keys.Control) != 0 ? 8 : 0;
            int buttonFlags = button == MouseButtons.Right ? 2 : 1;
            var position = new IntPtr((location.Y << 16) | (location.X & 0xFFFF));
            SendTextBoxMessage(textBox, button == MouseButtons.Right ? 0x0204 : doubleClick ? 0x0203 : 0x0201,
                new IntPtr(buttonFlags | control), position);
            SendTextBoxMessage(textBox, button == MouseButtons.Right ? 0x0205 : 0x0202,
                new IntPtr(control), position);
        }

        private static void SendTextBoxMessage(TextBoxBase textBox, int message, IntPtr wParam, IntPtr lParam)
        {
            var procedure = textBox.GetType().GetMethod("WndProc", BindingFlags.Instance | BindingFlags.NonPublic);
            procedure.Invoke(textBox, new object[]
            {
                Message.Create(textBox.Handle, message, wParam, lParam)
            });
        }

        private static class CaretNativeMethods
        {
            [DllImport("kernel32.dll")]
            internal static extern uint GetCurrentThreadId();

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CreateCaret(IntPtr window, IntPtr bitmap, int width, int height);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool ShowCaret(IntPtr window);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DestroyCaret();

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetGUIThreadInfo(uint thread, ref GuiThreadInfo info);

            [StructLayout(LayoutKind.Sequential)]
            internal struct GuiThreadInfo
            {
                internal uint Size;
                internal uint Flags;
                internal IntPtr Active;
                internal IntPtr Focus;
                internal IntPtr Capture;
                internal IntPtr MenuOwner;
                internal IntPtr MoveSize;
                internal IntPtr Caret;
                internal int Left;
                internal int Top;
                internal int Right;
                internal int Bottom;
            }
        }

        [TestMethod]
        public void OperationButtonsShareTheNativeButtonStyle()
        {
            // Build only the designer controls; the real constructor requires elevation.
            var form = (DSEForm)FormatterServices.GetUninitializedObject(typeof(DSEForm));
            typeof(Form).GetConstructor(Type.EmptyTypes).Invoke(form, null);
            try
            {
                Invoke(form, "InitializeComponent");
                var panel = (FlowLayoutPanel)typeof(DSEForm)
                    .GetField("flowLayoutPanel1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                var buttons = panel.Controls.OfType<Button>().ToList();

                Assert.IsGreaterThanOrEqualTo(7, buttons.Count);
                foreach (var button in buttons)
                {
                    // Mixed styles give some buttons the Windows hover effect and others a different one.
                    Assert.AreEqual(FlatStyle.System, button.FlatStyle, button.Name);
                }
            }
            finally
            {
                form.Dispose();
            }
        }

        [TestMethod]
        public void TypedDriverListRestoresUntypedViewState()
        {
            using (var previous = new ObjectListView())
            using (var current = new DriverStoreListView())
            {
                foreach (var list in new ObjectListViewBase[] { previous, current })
                {
                    var column = new OLVColumn("INF", nameof(DriverStoreEntry.DriverInfName));
                    list.AllColumns.Add(column);
                    list.Columns.Add(column);
                }

                previous.AllColumns[0].Width = 173;
                previous.PrimarySortColumn = previous.AllColumns[0];
                previous.PrimarySortOrder = SortOrder.Descending;
                previous.ShowGroups = false;

                Assert.IsTrue(current.RestoreState(previous.SaveState()));
                Assert.AreEqual(173, current.AllColumns[0].Width);
                Assert.AreSame(current.AllColumns[0], current.PrimarySortColumn);
                Assert.AreEqual(SortOrder.Descending, current.PrimarySortOrder);
                Assert.IsFalse(current.ShowGroups);
            }
        }

        [TestMethod]
        [DataRow(false, CloseReason.UserClosing, false, false, 0)]
        [DataRow(true, CloseReason.UserClosing, false, true, 1)]
        [DataRow(true, CloseReason.UserClosing, true, false, 1)]
        [DataRow(true, CloseReason.WindowsShutDown, false, false, 0)]
        [DataRow(true, CloseReason.TaskManagerClosing, false, false, 0)]
        public void ClosingDuringAnOperationAsksFirst(
            bool operationRunning, CloseReason reason, bool confirmed, bool keepOpen, int confirmations)
        {
            WithSelectionForm((form, list) =>
            {
                var testForm = (SelectionTestForm)form;
                testForm.CloseConfirmed = confirmed;
                SetField(form, "operationInProgress", operationRunning);

                var result = (bool)typeof(DSEForm)
                    .GetMethod("ShouldKeepOpenForOperation", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(form, new object[] { reason });

                Assert.AreEqual(keepOpen, result);
                Assert.AreEqual(confirmations, testForm.CloseConfirmationCount);
            });
        }

        [TestMethod]
        public void DeclinedCloseDuringAnOperationKeepsTheFormOpen()
        {
            WithOperationForm((form, list) =>
            {
                ((SelectionTestForm)form).CloseConfirmed = false;
                Invoke(form, "StartOperation");
                Assert.IsFalse(IsActionEnabled(form, "exitToolStripMenuItem"));

                var args = new FormClosingEventArgs(CloseReason.UserClosing, false);
                Invoke(form, "DSEForm_FormClosing", form, args);
                Assert.IsTrue(args.Cancel);

                Invoke(form, "EndOperation");
                Assert.IsTrue(IsActionEnabled(form, "exitToolStripMenuItem"));
            });
        }

        [TestMethod]
        public void AboutUpdateIsUnavailableDuringAnOperation()
        {
            // Updating from Help > About exits the app, which would stop the operation partway.
            WithOperationForm((form, list) =>
            {
                Invoke(form, "StartOperation");
                Assert.IsFalse(IsActionEnabled(form, "helpToolStripMenuItem"));

                Invoke(form, "EndOperation");
                Assert.IsTrue(IsActionEnabled(form, "helpToolStripMenuItem"));
            });
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void UnusedDriverSelectionFollowsTheBootCriticalOption(bool includeBootCritical)
        {
            // Settings is internal to Rapr, so reach it through ApplicationSettingsBase.
            var settings = (ApplicationSettingsBase)typeof(DSEForm).Assembly
                .GetType("Rapr.Properties.Settings")
                .GetProperty("Default", BindingFlags.Static | BindingFlags.Public)
                .GetValue(null);
            object previous = settings["IncludeBootCriticalInOldDriverSelection"];
            settings["IncludeBootCriticalInOldDriverSelection"] = includeBootCritical;

            try
            {
                WithSelectionForm((form, list) =>
                {
                    var bootCritical = CreateDriver("storage.inf", 1);
                    bootCritical.BootCritical = true;
                    var unknown = CreateDriver("unknown.inf", 1);
                    var regular = CreateDriver("regular.inf", 1);
                    regular.BootCritical = false;
                    list.SetObjects(new[] { bootCritical, unknown, regular });

                    Invoke(form, "CtxMenuSelectUnusedDrivers_Click", null, EventArgs.Empty);

                    var expected = includeBootCritical
                        ? new[] { bootCritical, unknown, regular }
                        : new[] { unknown, regular };
                    CollectionAssert.AreEquivalent(expected, list.CheckedObjects.Cast<DriverStoreEntry>().ToArray());
                });
            }
            finally
            {
                settings["IncludeBootCriticalInOldDriverSelection"] = previous;
            }
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

        private static void WithSelectionForm(Action<DSEForm, DriverStoreListView> test)
        {
            // Avoid startup/elevation; only the in-memory selection controls are exercised.
            var form = (SelectionTestForm)FormatterServices.GetUninitializedObject(typeof(SelectionTestForm));
            GC.SuppressFinalize(form);
            form.SelectionConfirmed = true;

            using (var list = new DriverStoreListView { CheckBoxes = true })
            using (var status = new ToolStripStatusLabel())
            {
                list.Columns.Add(new OLVColumn { AspectName = "DriverInfName" });
                SetField(form, "lstDriverStoreEntries", list);
                SetField(form, "lblStatus", status);
                SetField(form, "driverStore", new DismUtil());
                SetField(form, "driversWithNewerDate", new HashSet<DriverStoreEntry>());
                SetField(form, "unusedDriversToReview", new HashSet<DriverStoreEntry>());
                SetField(form, "themeColors", new ThemePalette(false, SystemInformation.HighContrast));
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

        private static void WithOperationForm(Action<DSEForm, DriverStoreListView> test)
        {
            WithSelectionForm((form, list) =>
            {
                var controls = new List<IDisposable>();

                try
                {
                    foreach (var field in typeof(DSEForm).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                    {
                        if (field.GetValue(form) == null
                            && (typeof(Control).IsAssignableFrom(field.FieldType)
                                || typeof(ToolStripItem).IsAssignableFrom(field.FieldType)
                                || typeof(ColumnHeader).IsAssignableFrom(field.FieldType)))
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
            protected override bool ShowWithoutActivation => true;

            public bool SelectionConfirmed { get; set; }

            public int ConfirmationCount { get; private set; }

            public Action OnConfirmation { get; set; }

            protected override bool ConfirmUnusedDriverSelection()
            {
                this.ConfirmationCount++;
                this.OnConfirmation?.Invoke();
                return this.SelectionConfirmed;
            }

            public bool CloseConfirmed { get; set; }

            public int CloseConfirmationCount { get; private set; }

            protected override bool ConfirmCloseDuringOperation()
            {
                this.CloseConfirmationCount++;
                return this.CloseConfirmed;
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
