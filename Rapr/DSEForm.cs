using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.WindowsAPICodePack.Dialogs;

using Rapr.Lang;
using Rapr.Properties;
using Rapr.Utils;

using Timer = System.Threading.Timer;

namespace Rapr
{
    public partial class DSEForm : Form
    {
        private IDriverStore driverStore;
        private Color savedBackColor;
        private Color savedForeColor;

        private static readonly IUpdateManager UpdateManager = new UpdateManager();

        private static readonly List<CultureInfo> SupportedLanguage = DSEFormHelper.GetSupportedLanguage();

        private readonly Timer UpdateCheckedItemSizeTimer;
        private const long RefreshTime = Timeout.Infinite;
        private const long Delay = 100;

        public DSEForm()
        {
            if (!DSEFormHelper.IsOSSupported)
            {
                this.ShowMessageBox(
                    Language.Message_Requires_Later_OS,
                    Language.Product_Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Application.Exit();
            }

            if (!DSEFormHelper.IsRunAsAdmin)
            {
                DSEFormHelper.RunAsAdministrator();
            }

            var lang = Settings.Default.Language;
            if (lang != null && !CultureInfo.InvariantCulture.Equals(lang))
            {
                Thread.CurrentThread.CurrentCulture = lang;
                Thread.CurrentThread.CurrentUICulture = lang;
            }

            this.Font = SystemFonts.MessageBoxFont;

            this.InitializeComponent();

            this.Icon = DSEFormHelper.ExtractAssociatedIcon(Application.ExecutablePath);
            this.BuildLanguageMenu();

            this.lstDriverStoreEntries.PrimarySortColumn = this.driverClassColumn;
            this.lstDriverStoreEntries.PrimarySortOrder = SortOrder.Ascending;
            this.lstDriverStoreEntries.SecondarySortColumn = this.driverDateColumn;
            this.lstDriverStoreEntries.SecondarySortOrder = SortOrder.Descending;
            this.lstDriverStoreEntries.CheckBoxes = DSEFormHelper.IsRunAsAdmin;

            this.SetupListViewColumns();

            Trace.TraceInformation("---------------------------------------------------------------");
            Trace.TraceInformation($"{Application.ProductName} started");

            this.UpdateDriverStore(DriverStoreFactory.CreateOnlineDriverStore());

            this.UpdateCheckedItemSizeTimer = new Timer(x => this.BeginInvoke((Action)(() => this.UpdateCheckedItemSize())));
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if ((components != null))
                {
                    components.Dispose();
                }

                if (this.UpdateCheckedItemSizeTimer != null)
                {
                    this.UpdateCheckedItemSizeTimer.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void UpdateDriverStore(IDriverStore driverStore)
        {
            this.driverStore = driverStore;
            this.exportAllDriversToolStripMenuItem.Enabled = driverStore.SupportExportAllDrivers;
            this.exportAllDriversToolStripMenuItem.Visible = driverStore.SupportExportAllDrivers;
            this.cbAddInstall.Enabled = driverStore.SupportAddInstall;
            this.cbForceDeletion.Enabled = driverStore.SupportForceDeletion;
            this.buttonExportDrivers.Visible = driverStore.SupportExportDriver;
            this.deviceNameColumn.IsVisible = driverStore.SupportForceDeletion;
            this.ctxMenuExportDriver.Visible = driverStore.SupportExportDriver;

            switch (driverStore.Type)
            {
                case DriverStoreType.Online:
                    {
                        this.Text = Language.Product_Name + " - " + Language.DriverStore_LocalMachine;
                        break;
                    }

                case DriverStoreType.Offline:
                    {
                        this.Text = Language.Product_Name + " - " + driverStore.OfflineStoreLocation;
                        break;
                    }
            }
        }

        private void SetupListViewColumns()
        {
            this.driverSizeColumn.AspectToStringConverter = size => DriverStoreEntry.GetBytesReadable((long)size);

            this.driverOemInfColumn.GroupKeyGetter = rowObject => ((DriverStoreEntry)rowObject).OemId / 10;
            this.driverOemInfColumn.GroupKeyToTitleConverter = groupKey =>
            {
                int? valueBase = (groupKey as int?) * 10;

                return valueBase == null
                    ? null
                    : $"oem {valueBase} - {valueBase + 9}";
            };

            this.driverVersionColumn.GroupKeyGetter = rowObject =>
            {
                DriverStoreEntry driver = (DriverStoreEntry)rowObject;

                return driver.DriverVersion == null
                    ? null
                    : new Version(driver.DriverVersion.Major, driver.DriverVersion.Minor);
            };

            this.driverDateColumn.GroupKeyGetter = rowObject =>
            {
                DriverStoreEntry driver = (DriverStoreEntry)rowObject;
                return new DateTime(driver.DriverDate.Year, driver.DriverDate.Month, 1);
            };

            this.driverDateColumn.GroupKeyToTitleConverter = groupKey => ((DateTime)groupKey).ToString("yyyy-MM");

            this.driverSizeColumn.GroupKeyGetter =
                rowObject => DriverStoreEntry.GetSizeRange(((DriverStoreEntry)rowObject).DriverSize);

            this.driverSizeColumn.GroupKeyToTitleConverter =
                groupKey => DriverStoreEntry.GetSizeRangeName((long)groupKey);

            this.bootCriticalColumn.AspectToStringConverter = condition => (bool)(condition ?? false) ? Language.Column_Text_True : Language.Column_Text_False;
        }

        private void BuildLanguageMenu()
        {
            if (SupportedLanguage.Count == 1)
            {
                this.languageToolStripMenuItem.Visible = false;
            }
            else
            {
                ToolStripMenuItem defaultLanguageMenuItem = null;
                bool currentUILanauageSupported = false;

                foreach (var item in SupportedLanguage)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.NativeName),
                        Tag = item
                    };

                    menuItem.Click += this.SwitchCulture;

                    if (CultureInfo.CurrentUICulture.Equals(item))
                    {
                        menuItem.Checked = true;
                        currentUILanauageSupported = true;
                    }

                    if (defaultLanguageMenuItem == null)
                    {
                        defaultLanguageMenuItem = menuItem;
                    }

                    this.languageToolStripMenuItem.DropDownItems.Add(menuItem);
                }

                if (!currentUILanauageSupported && defaultLanguageMenuItem != null)
                {
                    defaultLanguageMenuItem.Checked = true;
                }
            }
        }

        private async void DSEForm_Shown(object sender, EventArgs e)
        {
            this.savedBackColor = this.lblStatus.BackColor;
            this.savedForeColor = this.lblStatus.ForeColor;

            await this.PopulateUIWithDriverStoreEntries().ConfigureAwait(true);
        }

        private void DSEForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Trace.TraceInformation($"Shutting down - reason {e.CloseReason}");
        }

        private async void ButtonEnumerate_Click(object sender, EventArgs e)
        {
            await this.PopulateUIWithDriverStoreEntries().ConfigureAwait(true);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async Task PopulateUIWithDriverStoreEntries()
        {
            try
            {
                this.StartOperation();
                this.ShowStatus(Status.Normal, Language.Message_Scanning_Driver_Store);
                this.lstDriverStoreEntries.EmptyListMsg = Language.Message_Scanning_Driver_Store;
                this.lstDriverStoreEntries.ClearObjects();

                var driverStoreEntries = await Task.Run(() => this.driverStore.EnumeratePackages()).ConfigureAwait(true);
                this.lstDriverStoreEntries.SetObjects(driverStoreEntries);
                this.UpdateColumnSize();
                this.ShowStatus(Status.Normal, Language.Status_Label);
            }
            catch (Exception ex)
            {
                this.ShowStatus(Status.Error, ex.Message, ex.ToString(), true);
            }
            finally
            {
                this.lstDriverStoreEntries.EmptyListMsg = Language.Message_No_Entries;
                this.EndOperation();
            }
        }

        private async void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.CheckedObjects.Count == 0)
            {
                return;
            }

            await this.DeleteDriverStoreEntries(this.lstDriverStoreEntries.CheckedObjects.OfType<DriverStoreEntry>().ToList()).ConfigureAwait(true);
        }

        private async Task DeleteDriverStoreEntries(List<DriverStoreEntry> driverStoreEntries)
        {
            string msgWarning;

            if (driverStoreEntries?.Count > 0)
            {
                if (driverStoreEntries.Count == 1)
                {
                    msgWarning = string.Format(
                        this.cbForceDeletion.Checked ? Language.Message_ForceDelete_Single_Package : Language.Message_Delete_Single_Package,
                        driverStoreEntries[0].DriverInfName,
                        driverStoreEntries[0].DriverPublishedName);
                }
                else
                {
                    msgWarning = string.Format(
                        this.cbForceDeletion.Checked ? Language.Message_ForceDelete_Multiple_Packages : Language.Message_Delete_Multiple_Packages,
                        driverStoreEntries.Count);
                }

                msgWarning += Environment.NewLine + Environment.NewLine + Language.Message_Sure;

                if (DialogResult.OK == this.ShowMessageBox(
                    msgWarning,
                    Language.Message_Title_Warning,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning))
                {
                    await this.DeleteDriverStorePackages(driverStoreEntries).ConfigureAwait(true);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async Task DeleteDriverStorePackages(List<DriverStoreEntry> driverStoreEntries)
        {
            StringBuilder details = new StringBuilder();

            foreach (DriverStoreEntry item in driverStoreEntries)
            {
                details.AppendLine($"{item.DriverPublishedName} - {item.DriverInfName}");
            }

            bool force = this.cbForceDeletion.Checked;

            try
            {
                this.StartOperation();

                this.ShowStatus(
                    Status.Normal,
                    Language.Status_Deleting_Packages,
                    $"{Language.Status_Deleting_Packages}{Environment.NewLine}{details.ToString().Trim()}");

                (bool allSucceeded, string detailResult, List<DriverStoreEntry> allDriverStoreEntries) = await Task.Run(() =>
                {
                    bool totalResult = true;
                    StringBuilder sb = new StringBuilder();

                    if (driverStoreEntries.Count == 1)
                    {
                        totalResult = this.driverStore.DeleteDriver(driverStoreEntries[0], force);
                    }
                    else
                    {
                        foreach (DriverStoreEntry entry in driverStoreEntries)
                        {
                            bool succeeded = this.driverStore.DeleteDriver(entry, force);
                            string resultTxt = string.Format(
                                succeeded ? Language.Message_Delete_Success : Language.Message_Delete_Fail,
                                entry.DriverInfName,
                                entry.DriverPublishedName);

                            Trace.TraceInformation(resultTxt);

                            sb.AppendLine(resultTxt);
                            totalResult &= succeeded;
                        }
                    }

                    var updatedDriverStoreEntries = totalResult
                        ? new List<DriverStoreEntry>()
                        : this.driverStore.EnumeratePackages();

                    return (totalResult, sb.ToString(), updatedDriverStoreEntries);
                }).ConfigureAwait(true);

                if (allSucceeded)
                {
                    this.lstDriverStoreEntries.RemoveObjects(driverStoreEntries);
                }
                else
                {
                    this.lstDriverStoreEntries.SetObjects(allDriverStoreEntries);
                    this.UpdateColumnSize();
                }

                string resultText;
                if (allSucceeded)
                {
                    if (driverStoreEntries.Count == 1)
                    {
                        resultText = string.Format(
                            Language.Message_Delete_Package,
                            driverStoreEntries[0].DriverInfName,
                            driverStoreEntries[0].DriverPublishedName);
                    }
                    else
                    {
                        resultText = string.Format(
                            Language.Message_Delete_Packages,
                            driverStoreEntries.Count.ToString());
                    }

                    this.ShowStatus(Status.Success, resultText);
                }
                else
                {
                    string fullResult = null;

                    if (driverStoreEntries.Count == 1)
                    {
                        resultText = string.Format(
                            Language.Message_Delete_Package_Error,
                            driverStoreEntries[0].DriverInfName,
                            driverStoreEntries[0].DriverPublishedName);

                        if (!force)
                        {
                            resultText += Environment.NewLine + Language.Tip_Driver_In_Use;
                        }
                    }
                    else
                    {
                        resultText = Language.Message_Delete_Packages_Error;

                        if (!force)
                        {
                            resultText += Environment.NewLine + Language.Tip_Driver_In_Use;
                        }

                        fullResult = $"{resultText}{Environment.NewLine}{detailResult}";
                    }

                    this.ShowStatus(Status.Error, resultText, fullResult, true);
                }

                this.cbForceDeletion.Checked = false;
            }
            catch (Exception ex)
            {
                this.ShowStatus(Status.Error, ex.Message, ex.ToString(), true);
            }
            finally
            {
                this.EndOperation();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async void ButtonAddDriver_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string infPath = this.openFileDialog.FileName;
                bool installDriver = this.cbAddInstall.Checked;

                try
                {
                    this.StartOperation();
                    this.ShowStatus(Status.Normal, Language.Status_Adding_Package);

                    bool result = await Task.Run(() => this.driverStore.AddDriver(infPath, installDriver)).ConfigureAwait(true);

                    var allDriverStoreEntries = await Task.Run(() => this.driverStore.EnumeratePackages()).ConfigureAwait(true);
                    this.lstDriverStoreEntries.SetObjects(allDriverStoreEntries);

                    if (result)
                    {
                        var message = string.Format(
                            installDriver ? Language.Message_Driver_Added_Installed : Language.Message_Driver_Added,
                            infPath);

                        this.ShowStatus(Status.Success, message);
                    }
                    else
                    {
                        var message = string.Format(
                            installDriver ? Language.Message_Driver_Added_Installed_Error : Language.Message_Driver_Added_Error,
                            infPath);

                        this.ShowStatus(Status.Error, message, usePopup: true);
                    }

                    this.cbAddInstall.Checked = false;
                }
                catch (Exception ex)
                {
                    this.ShowStatus(Status.Error, ex.Message, ex.ToString(), true);
                }
                finally
                {
                    this.EndOperation();
                }
            }
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // Check if there are any entries
            if (this.lstDriverStoreEntries.Objects != null)
            {
                this.ctxMenuSelectAll.Enabled = true;
                this.ctxMenuSelectOldDrivers.Enabled = true;

                if (this.lstDriverStoreEntries.CheckedObjects?.Count > 0)
                {
                    this.ctxMenuSelectAll.Text = Language.Context_Deselect_All;
                }
                else
                {
                    this.ctxMenuSelectAll.Text = Language.Context_Select_All;
                }

                if (this.lstDriverStoreEntries.SelectedObjects?.Count > 0)
                {
                    this.ctxMenuDelete.Enabled = true;

                    if (this.lstDriverStoreEntries.CheckedObjects?.Count > 0
                        && this.lstDriverStoreEntries
                            .SelectedObjects
                            .Cast<object>()
                            .All(i => this.lstDriverStoreEntries.CheckedObjects.Contains(i)))
                    {
                        this.ctxMenuSelect.Text = Language.Context_Deselect;
                    }
                    else
                    {
                        this.ctxMenuSelect.Text = Language.Context_Select;
                    }

                    this.ctxMenuSelect.Enabled = true;

                    this.ctxMenuOpenFolder.Enabled = this.lstDriverStoreEntries.SelectedObjects.Count == 1;
                }
                else
                {
                    this.ctxMenuOpenFolder.Enabled = false;
                    this.ctxMenuDelete.Enabled = false;
                    this.ctxMenuSelect.Enabled = false;
                }
            }
            else
            {
                this.ctxMenuSelect.Enabled = false;
                this.ctxMenuSelectAll.Enabled = false;
                this.ctxMenuSelectOldDrivers.Enabled = false;
                this.ctxMenuOpenFolder.Enabled = false;
                this.ctxMenuDelete.Enabled = false;
            }
        }

        // Function to switch between "selected" and "unselected" states
        private void CtxMenuSelectAll_Click(object sender, EventArgs e)
        {
            // Check if there are any entries
            if (this.lstDriverStoreEntries.Objects != null)
            {
                if (this.lstDriverStoreEntries.CheckedObjects != null
                    && this.lstDriverStoreEntries.CheckedObjects.Count != 0)
                {
                    this.lstDriverStoreEntries.UncheckAll();
                }
                else
                {
                    this.lstDriverStoreEntries.CheckAll();
                }
            }
        }

        private void CtxMenuSelect_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.Objects != null)
            {
                ArrayList list = new ArrayList();
                if (this.lstDriverStoreEntries.CheckedObjects?.Count > 0)
                {
                    list.AddRange(this.lstDriverStoreEntries.CheckedObjects);
                }

                if (this.lstDriverStoreEntries.SelectedObjects?.Count > 0)
                {
                    if (this.lstDriverStoreEntries
                        .SelectedObjects
                        .Cast<object>()
                        .All(i => this.lstDriverStoreEntries.CheckedObjects.Contains(i)))
                    {
                        foreach (var item in this.lstDriverStoreEntries.SelectedObjects)
                        {
                            list.Remove(item);
                        }
                    }
                    else
                    {
                        list.AddRange(this.lstDriverStoreEntries.SelectedObjects);
                    }
                }

                this.lstDriverStoreEntries.CheckedObjects = list;
            }
        }

        private async void CtxMenuDelete_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.SelectedObjects != null)
            {
                List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();

                foreach (DriverStoreEntry item in this.lstDriverStoreEntries.SelectedObjects)
                {
                    driverStoreEntries.Add(item);
                }

                await this.DeleteDriverStoreEntries(driverStoreEntries).ConfigureAwait(true);
            }
        }

        private void CtxMenuSelectOldDrivers_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.Objects != null)
            {
                this.lstDriverStoreEntries.CheckedObjects = this.lstDriverStoreEntries
                    .Objects
                    .OfType<DriverStoreEntry>()
                    .Where(entry => entry.BootCritical != true)
                    .GroupBy(entry => new { entry.DriverClass, entry.DriverPkgProvider, entry.DriverInfName })
                    .SelectMany(g => g.OrderByDescending(row => row.DriverVersion).ThenByDescending(row => row.DriverDate).Skip(1))
                    .Where(entry => string.IsNullOrEmpty(entry.DeviceName))
                    .ToArray();
            }
        }

        private void ButtonSelectOldDrivers_Click(object sender, EventArgs e)
        {
            this.CtxMenuSelectOldDrivers_Click(sender, e);
        }

        private void ExportList()
        {
            // Check if there are any entries.
            if (this.lstDriverStoreEntries.Objects != null)
            {
                List<DriverStoreEntry> driverStoreEntries = this.lstDriverStoreEntries
                    .Objects
                    .OfType<DriverStoreEntry>()
                    .ToList();

                if (driverStoreEntries.Count == 0)
                {
                    this.ShowMessageBox(
                        Language.Message_No_Entries,
                        Language.Export_Error,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                try
                {
                    string fileName = new CsvExporter().Export(driverStoreEntries);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        string message = string.Format(Language.Export_Complete, fileName);
                        this.ShowStatus(Status.Normal, message, usePopup: true);
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is SecurityException)
                {
                    this.ShowStatus(Status.Error, string.Format(Language.Export_Failed, ex), usePopup: true);
                }
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SwitchCulture(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem)?.Tag is CultureInfo ci)
            {
                Size windowSize = this.Size;
                byte[] driverStoreViewState = this.lstDriverStoreEntries.SaveState();

                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

                // Clear and redraw the window
                this.SuspendLayout();
                this.Controls.Clear();
                this.InitializeComponent();
                this.BuildLanguageMenu();
                this.Size = windowSize;
                this.RightToLeft = ci.TextInfo.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
                this.RightToLeftLayout = ci.TextInfo.IsRightToLeft;
                this.SetupListViewColumns();
                this.lstDriverStoreEntries.RestoreState(driverStoreViewState);
                this.lstDriverStoreEntries.RightToLeft = this.RightToLeft;
                this.lstDriverStoreEntries.RightToLeftLayout = this.RightToLeftLayout;
                this.UpdateDriverStore(this.driverStore);

                this.DSEForm_Shown(sender, e);
                this.ResumeLayout();
                Application.DoEvents();

                Settings.Default.Language = ci;
            }
        }

        private void ViewLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextFileTraceListener.LastTraceFile))
            {
                Process.Start(TextFileTraceListener.LastTraceFile);
            }
            else
            {
                this.ShowMessageBox(Language.Message_Log_File_NotFound, Language.Message_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ExportList();
        }

        private void RunAsAdminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DSEFormHelper.RunAsAdministrator();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox ab = new AboutBox(UpdateManager))
            {
                ab.RightToLeft = this.RightToLeft;
                ab.RightToLeftLayout = this.RightToLeftLayout;
                ab.ShowDialog();
            }
        }

        private void DSEForm_Load(object sender, EventArgs e)
        {
            this.lstDriverStoreEntries.RestoreState(Convert.FromBase64String(Settings.Default.DriverStoreViewState));
            this.RightToLeft = Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeftLayout = Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft;
            this.lstDriverStoreEntries.RightToLeft = this.RightToLeft;
            this.lstDriverStoreEntries.RightToLeftLayout = this.RightToLeftLayout;

            if (Settings.Default.WindowState != default(FormWindowState))
            {
                this.WindowState = Settings.Default.WindowState;
            }

            if (Settings.Default.WindowLocation != default(Point))
            {
                this.Location = Settings.Default.WindowLocation;
            }

            if (Settings.Default.WindowSize != default(Size))
            {
                this.Size = Settings.Default.WindowSize;
            }
        }

        private void DSEForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.DriverStoreViewState = Convert.ToBase64String(this.lstDriverStoreEntries.SaveState());

            Settings.Default.WindowState = this.WindowState;
            Settings.Default.WindowLocation = this.Location;

            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowSize = this.Size;
            }
            else
            {
                Settings.Default.WindowSize = this.RestoreBounds.Size;
            }

            Settings.Default.Save();
        }

        private void LstDriverStoreEntries_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.UpdateCheckedItemSizeTimer.Change(Delay, RefreshTime);
        }

        private void UpdateCheckedItemSize()
        {
            IList checkedObjects = this.lstDriverStoreEntries.CheckedObjects;

            if (checkedObjects?.Count > 0)
            {
                long totalSize = 0;

                foreach (DriverStoreEntry item in checkedObjects)
                {
                    totalSize += item.DriverSize;
                }

                this.ShowStatus(
                    Status.Normal,
                    string.Format(Language.Status_Selected_Drivers, checkedObjects.Count, DriverStoreEntry.GetBytesReadable(totalSize)));
            }
            else
            {
                this.ShowStatus(Status.Normal, Language.Status_No_Drivers_Selected);
            }

            this.buttonDeleteDriver.Enabled = this.lstDriverStoreEntries.CheckedObjects.Count > 0;
            this.cbForceDeletion.Enabled = this.buttonDeleteDriver.Enabled;
        }

        private void UpdateColumnSize()
        {
            // Make the column size fit both header and content.
            for (var i = 0; i < this.lstDriverStoreEntries.Columns.Count - 1; i++)
            {
                this.lstDriverStoreEntries.Columns[i].Width = -2;
            }
        }

        private void CtxMenuOpenFolder_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.SelectedObject != null)
            {
                DriverStoreEntry item = (DriverStoreEntry)this.lstDriverStoreEntries.SelectedObject;
                Process.Start("explorer.exe", "/select, " + Path.Combine(item.DriverFolderLocation, item.DriverInfName));
            }
        }

        private async void ChooseDriverStoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ChooseDriverStore chooseDriverStore = new ChooseDriverStore())
            {
                chooseDriverStore.StoreType = this.driverStore.Type;
                if (this.driverStore.Type == DriverStoreType.Offline)
                {
                    chooseDriverStore.OfflineStoreLocation = this.driverStore.OfflineStoreLocation;
                }

                chooseDriverStore.RightToLeft = this.RightToLeft;
                chooseDriverStore.RightToLeftLayout = this.RightToLeftLayout;

                DialogResult result = chooseDriverStore.ShowDialog();

                if (result == DialogResult.OK)
                {
                    switch (chooseDriverStore.StoreType)
                    {
                        case DriverStoreType.Online:
                            this.UpdateDriverStore(DriverStoreFactory.CreateOnlineDriverStore());
                            break;

                        case DriverStoreType.Offline:
                            this.UpdateDriverStore(DriverStoreFactory.CreateOfflineDriverStore(chooseDriverStore.OfflineStoreLocation));
                            break;
                    }

                    await this.PopulateUIWithDriverStoreEntries().ConfigureAwait(true);
                }
            }
        }

        private void LstDriverStoreEntries_FormatCell(object sender, BrightIdeasSoftware.FormatCellEventArgs e)
        {
            if (e.ColumnIndex == this.deviceNameColumn.Index)
            {
                DriverStoreEntry entry = (DriverStoreEntry)e.Model;
                if (entry.DevicePresent == false)
                {
                    e.SubItem.ForeColor = Color.Gray;
                }
            }
        }

        private void LstDriverStoreEntries_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            if (e.Item.Checked)
            {
                e.Item.BackColor = Color.FromArgb(unchecked((int)0xFFCBE8F6));
            }
        }

        private void StartOperation()
        {
            this.toolStripProgressBar1.Visible = true;
            this.lstDriverStoreEntries.Enabled = false;
            this.buttonEnumerate.Enabled = false;
            this.buttonAddDriver.Enabled = false;
            this.cbAddInstall.Enabled = false;
            this.buttonDeleteDriver.Enabled = false;
            this.cbForceDeletion.Enabled = false;
            this.buttonSelectOldDrivers.Enabled = false;
            this.buttonExportDrivers.Enabled = false;
            this.chooseDriverStoreToolStripMenuItem.Enabled = false;
            this.exportToolStripMenuItem.Enabled = false;
            this.exportAllDriversToolStripMenuItem.Enabled = false;
            this.languageToolStripMenuItem.Enabled = false;
        }

        private void EndOperation()
        {
            this.toolStripProgressBar1.Visible = false;
            this.lstDriverStoreEntries.Enabled = true;
            this.buttonEnumerate.Enabled = true;
            this.buttonAddDriver.Enabled = true;
            this.cbAddInstall.Enabled = this.driverStore.SupportAddInstall;
            this.buttonDeleteDriver.Enabled = this.lstDriverStoreEntries.CheckedObjects.Count > 0;
            this.cbForceDeletion.Enabled = this.buttonDeleteDriver.Enabled && this.driverStore.SupportForceDeletion;
            this.buttonSelectOldDrivers.Enabled = true;
            this.buttonExportDrivers.Enabled = true;
            this.chooseDriverStoreToolStripMenuItem.Enabled = true;
            this.exportToolStripMenuItem.Enabled = true;
            this.exportAllDriversToolStripMenuItem.Enabled = true;
            this.languageToolStripMenuItem.Enabled = true;
        }

        public enum Status
        {
            Success,
            Error,
            Warning,
            Normal,
        }

        private void ShowStatus(Status status, string text, string detail = null, bool usePopup = false)
        {
            this.lblStatus.Text = text.Replace("\r\n", "\n").Replace("\n", " ");
            string detailToLog = string.IsNullOrEmpty(detail) ? text : detail;

            switch (status)
            {
                case Status.Error:
                    this.lblStatus.BackColor = Color.FromArgb(0xFF, 0x00, 0x33);
                    this.lblStatus.ForeColor = Color.White;
                    Trace.TraceError(detailToLog);

                    if (usePopup)
                    {
                        this.ShowMessageBox(text, Language.Message_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    break;

                case Status.Success:
                    this.lblStatus.BackColor = Color.LightGreen;
                    this.lblStatus.ForeColor = Color.Black;
                    Trace.TraceInformation(detailToLog);

                    if (usePopup)
                    {
                        this.ShowMessageBox(text, Language.Product_Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    break;

                case Status.Warning:
                    this.lblStatus.BackColor = Color.Yellow;
                    this.lblStatus.ForeColor = Color.Black;
                    Trace.TraceWarning(detailToLog);

                    if (usePopup)
                    {
                        this.ShowMessageBox(text, Language.Message_Title_Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    break;

                case Status.Normal:
                    this.lblStatus.BackColor = this.savedBackColor;
                    this.lblStatus.ForeColor = this.savedForeColor;
                    Trace.TraceInformation(detailToLog);

                    if (usePopup)
                    {
                        this.ShowMessageBox(text, Language.Product_Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    break;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async void ExportAllDriversToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                CommonFileDialogResult dialogResult = dialog.ShowDialog();

                if (dialogResult == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        this.StartOperation();
                        this.ShowStatus(Status.Normal, Language.Status_Exporting_All_Drivers);

                        bool result = await Task.Run(() => this.driverStore.ExportAllDrivers(dialog.FileName)).ConfigureAwait(true);

                        if (result)
                        {
                            this.ShowStatus(Status.Success, Language.Message_Export_All_Drivers_Success);
                        }
                        else
                        {
                            this.ShowStatus(Status.Error, Language.Message_Export_All_Drivers_Fail, usePopup: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ShowStatus(Status.Error, ex.Message, ex.ToString(), true);
                    }
                    finally
                    {
                        this.EndOperation();
                    }
                }
            }
        }

        private DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(
                this,
                text,
                caption,
                buttons,
                icon,
                MessageBoxDefaultButton.Button1,
                this.RightToLeftLayout ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
        }
    }
}
