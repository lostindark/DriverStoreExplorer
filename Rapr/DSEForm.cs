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

using BrightIdeasSoftware;

using JR.Utils.GUI.Forms;

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

        private static readonly ICollection<CultureInfo> SupportedLanguage = DSEFormHelper.GetSupportedLanguage();

        private readonly Timer UpdateCheckedItemSizeTimer;
        private Timer searchDebounceTimer;
        private const long RefreshTime = Timeout.Infinite;
        private const long Delay = 100;
        private const long SearchDebounceDelay = 300; // 0.3 seconds

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

            // Initialize the BootCritical option menu item
            this.includeBootCriticalDriversStripMenuItem.Checked = Settings.Default.IncludeBootCriticalInOldDriverSelection;

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
            this.searchDebounceTimer = new Timer(x => this.BeginInvoke((Action)(() => this.UpdateSearchFilter())));
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

                if (this.searchDebounceTimer != null)
                {
                    var timer = this.searchDebounceTimer;
                    this.searchDebounceTimer = null;
                    timer.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private async Task UpdateDriverStoreAPI(DriverStoreOption driverStoreOption)
        {
            if (this.driverStore.Type == DriverStoreType.Online && Settings.Default.DriverStoreOption != driverStoreOption.ToString())
            {
                Settings.Default.DriverStoreOption = driverStoreOption.ToString();
                this.UpdateDriverStore(DriverStoreFactory.CreateOnlineDriverStore());
                await this.PopulateUIWithDriverStoreEntries().ConfigureAwait(true);
            }
        }

        private void UpdateDriverStore(IDriverStore driverStore)
        {
            this.driverStore = driverStore;

            // Update menu item checked states
            _ = Enum.TryParse(Settings.Default.DriverStoreOption, out DriverStoreOption driverStoreOption);
            this.useNativeDriveStoreStripMenuItem.Checked = driverStoreOption == DriverStoreOption.Native;
            this.useDismStripMenuItem.Checked = driverStoreOption == DriverStoreOption.DISM;
            this.usePnpUtilStripMenuItem.Checked = driverStoreOption == DriverStoreOption.PnpUtil;

            if (driverStore.Type == DriverStoreType.Online)
            {
                // Update menu item enabled states
                this.useNativeDriveStoreStripMenuItem.Enabled = DSEFormHelper.IsNativeDriverStoreSupported;
                this.useDismStripMenuItem.Enabled = DismUtil.IsDismAvailable;
                this.usePnpUtilStripMenuItem.Enabled = DSEFormHelper.IsPnpUtilSupported;
                this.installDateColumn.IsVisible = driverStoreOption == DriverStoreOption.Native;
            }
            else
            {
                // Update menu item enabled states
                this.useNativeDriveStoreStripMenuItem.Enabled = false;
                this.useDismStripMenuItem.Enabled = false;
                this.usePnpUtilStripMenuItem.Enabled = false;
                this.installDateColumn.IsVisible = false;
            }

            this.cbAddInstall.Enabled = driverStore.SupportAddInstall;
            this.cbForceDeletion.Enabled = driverStore.SupportForceDeletion;
            this.buttonExportDrivers.Visible = driverStore.SupportExportDriver;
            this.buttonExportAllDrivers.Visible = driverStore.SupportExportAllDrivers;
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
            this.driverExtensionIdColumn.AspectToStringConverter = id => (Guid)id == Guid.Empty ? String.Empty : id.ToString();
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
            
            // Setup InstallDate column formatting - shows when the driver package was imported to the store
            this.installDateColumn.GroupKeyGetter = rowObject =>
            {
                DriverStoreEntry driver = (DriverStoreEntry)rowObject;
                return driver.InstallDate?.Date ?? DateTime.MinValue.Date;
            };

            this.installDateColumn.GroupKeyToTitleConverter = groupKey => 
            {
                var date = (DateTime)groupKey;
                return date == DateTime.MinValue.Date ? "" : date.ToString("yyyy-MM");
            };
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

        private void EnsureOnScreen()
        {
            var workArea = Screen.GetWorkingArea(this);
            var currentBounds = this.Bounds;
            var newLocation = currentBounds.Location;

            if (currentBounds.Right <= workArea.Left)
                newLocation.X = workArea.Left;
            else if (currentBounds.Left >= workArea.Right)
                newLocation.X = workArea.Right - currentBounds.Width;

            if (currentBounds.Bottom <= workArea.Top)
                newLocation.Y = workArea.Top;
            else if (currentBounds.Top >= workArea.Bottom)
                newLocation.Y  = workArea.Bottom - currentBounds.Height;

            this.Location = newLocation;
        }

        private async void DSEForm_Shown(object sender, EventArgs e)
        {
            this.savedBackColor = this.lblStatus.BackColor;
            this.savedForeColor = this.lblStatus.ForeColor;

            this.EnsureOnScreen();

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

            await this.DeleteDriverStoreEntries(this.lstDriverStoreEntries.CheckedObjects).ConfigureAwait(true);
        }

        private async Task DeleteDriverStoreEntries(IEnumerable entries)
        {
            StringBuilder msgWarning = new StringBuilder();
            var driverStoreEntries = entries.OfType<DriverStoreEntry>()
                .OrderByColumnName(this.lstDriverStoreEntries.PrimarySortColumn?.AspectName, this.lstDriverStoreEntries.PrimarySortOrder == SortOrder.Ascending)
                .ThenByColumnName(this.lstDriverStoreEntries.SecondarySortColumn?.AspectName, this.lstDriverStoreEntries.SecondarySortOrder == SortOrder.Ascending)
                .ToList();

            if (driverStoreEntries?.Count > 0)
            {
                if (driverStoreEntries.Count == 1)
                {
                    msgWarning.AppendFormat(
                        this.cbForceDeletion.Checked ? Language.Message_ForceDelete_Single_Package : Language.Message_Delete_Single_Package,
                        driverStoreEntries[0].DriverInfName,
                        $"{driverStoreEntries[0].DriverVersion}, {DriverStoreEntry.GetBytesReadable(driverStoreEntries[0].DriverSize)}");
                }
                else
                {
                    msgWarning.AppendFormat(
                        this.cbForceDeletion.Checked ? Language.Message_ForceDelete_Multiple_Packages : Language.Message_Delete_Multiple_Packages,
                        driverStoreEntries.Count);

                    msgWarning.AppendLine();

                    foreach (DriverStoreEntry item in driverStoreEntries)
                    {
                        msgWarning.AppendLine($"{item.DriverInfName} - {item.DriverVersion} - {DriverStoreEntry.GetBytesReadable(item.DriverSize)}");
                    }
                }

                msgWarning.AppendLine().Append(Language.Message_Sure);

                if (DialogResult.OK == this.ShowMessageBox(
                    msgWarning.ToString(),
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
                details.AppendLine($"{item.DriverPublishedName} - {item.DriverFolderName} - {DriverStoreEntry.GetBytesReadable(item.DriverSize)}");
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
                                entry.DriverPublishedName,
                                entry.DriverFolderName);

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
            using (var dialog = new CommonOpenFileDialog { IsFolderPicker = true, EnsurePathExists = true, Title = Language.Dialog_Select_Inf_Folder })
            {
                CommonFileDialogResult dialogResult = dialog.ShowDialog();

                if (dialogResult == CommonFileDialogResult.Ok)
                {
                    string infPath = dialog.FileName;
                    bool installDriver = this.cbAddInstall.Checked;

                    var infFiles = FindInfFile(infPath).ToList();

                    if (infFiles.Count == 0)
                    {
                        this.ShowStatus(Status.Error, string.Format(Language.Message_No_Inf_Found, infPath), usePopup: true);
                        return;
                    }

                    try
                    {
                        this.StartOperation();
                        this.ShowStatus(Status.Normal, Language.Status_Adding_Package);

                        (bool allSucceeded, string detailResult) = await Task.Run(() =>
                        {
                            bool totalResult = true;
                            StringBuilder sb = new StringBuilder();

                            if (infFiles.Count == 1)
                            {
                                totalResult = this.driverStore.AddDriver(infFiles[0], installDriver);
                                sb.AppendFormat(
                                    installDriver ? Language.Message_Driver_Added_Installed : Language.Message_Driver_Added,
                                    infPath);
                            }
                            else
                            {
                                foreach (string infFile in infFiles)
                                {
                                    bool succeeded = this.driverStore.AddDriver(infFile, installDriver);

                                    string resultTxt;
                                    if (succeeded)
                                    {
                                        resultTxt = string.Format(
                                            installDriver ? Language.Message_Driver_Added_Installed : Language.Message_Driver_Added,
                                            infFile.Substring(infPath.Length).TrimStart('\\'));
                                        Trace.TraceInformation(resultTxt);
                                    }
                                    else
                                    {
                                        resultTxt = string.Format(
                                            installDriver ? Language.Message_Driver_Added_Installed_Error : Language.Message_Driver_Added_Error,
                                            infFile.Substring(infPath.Length).TrimStart('\\'));
                                        Trace.TraceError(resultTxt);
                                    }

                                    sb.AppendLine(resultTxt);
                                    totalResult &= succeeded;
                                }
                            }

                            return (totalResult, sb.ToString());
                        }).ConfigureAwait(true);


                        var allDriverStoreEntries = await Task.Run(() => this.driverStore.EnumeratePackages()).ConfigureAwait(true);
                        this.lstDriverStoreEntries.SetObjects(allDriverStoreEntries);


                        if (infFiles.Count == 1)
                        {
                            if (allSucceeded)
                            {
                                this.ShowStatus(
                                    Status.Success,
                                    string.Format(
                                        installDriver ? Language.Message_Driver_Added_Installed : Language.Message_Driver_Added,
                                        infPath));
                            }
                            else
                            {
                                this.ShowStatus(
                                    Status.Error,
                                    string.Format(
                                        installDriver ? Language.Message_Driver_Added_Installed_Error : Language.Message_Driver_Added_Error,
                                       infPath),
                                    usePopup: true);
                            }
                        }
                        else
                        {
                            if (allSucceeded)
                            {
                                this.ShowStatus(
                                    Status.Success,
                                    string.Format(
                                        installDriver ? Language.Message_Drivers_Added_Installed : Language.Message_Drivers_Added,
                                        infFiles.Count));
                            }
                            else
                            {
                                var message = installDriver ? Language.Message_Drivers_Added_Installed_Error : Language.Message_Drivers_Added_Error;
                                this.ShowStatus(Status.Error, $"{message}{Environment.NewLine}{detailResult}", usePopup: true);
                            }
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

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // Check if there are any entries
            if (this.lstDriverStoreEntries.Objects != null)
            {
                this.ctxMenuSelectAll.Enabled = true;
                this.ctxMenuSelectOldDrivers.Enabled = true;
                this.ctxMenuInvertSelection.Enabled = true;

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
                    this.ctxMenuExportDriver.Enabled = true;

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

                    this.ctxMenuOpenDeviceProperties.Enabled = !string.IsNullOrEmpty((this.lstDriverStoreEntries.SelectedObject as DriverStoreEntry)?.DeviceId);
                    this.ctxMenuOpenFolder.Enabled = this.lstDriverStoreEntries.SelectedObjects.Count == 1;
                }
                else
                {
                    this.ctxMenuOpenFolder.Enabled = false;
                    this.ctxMenuDelete.Enabled = false;
                    this.ctxMenuSelect.Enabled = false;
                    this.ctxMenuExportDriver.Enabled = false;
                }
            }
            else
            {
                this.ctxMenuSelect.Enabled = false;
                this.ctxMenuSelectAll.Enabled = false;
                this.ctxMenuInvertSelection.Enabled = false;
                this.ctxMenuSelectOldDrivers.Enabled = false;
                this.ctxMenuOpenDeviceProperties.Enabled = false;
                this.ctxMenuOpenFolder.Enabled = false;
                this.ctxMenuDelete.Enabled = false;
                this.ctxMenuExportDriver.Enabled = false;
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

        private void CtxMenuInvertSelection_Click(object sender, System.EventArgs e)
        {
            if (this.lstDriverStoreEntries.Objects != null)
            {

                this.lstDriverStoreEntries.CheckedObjects = this.lstDriverStoreEntries
                    .Objects
                    .Cast<object>()
                    .Except(this.lstDriverStoreEntries.CheckedObjects.Cast<object>())
                    .ToList();
            }
        }

        private async void CtxMenuDelete_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.SelectedObjects != null)
            {
                await this.DeleteDriverStoreEntries(this.lstDriverStoreEntries.SelectedObjects).ConfigureAwait(true);
            }
        }

        private void CtxMenuSelectOldDrivers_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.Objects != null)
            {
                var queryEntries = this.lstDriverStoreEntries
                    .Objects
                    .OfType<DriverStoreEntry>();

                // Apply BootCritical filter based on the setting
                if (!Settings.Default.IncludeBootCriticalInOldDriverSelection)
                {
                    queryEntries = queryEntries.Where(entry => entry.BootCritical != true);
                }

                var oldDriversToSelect = queryEntries
                    .Where(entry => entry.DriverInfName != "ntprint.inf")
                    .GroupBy(entry => new { entry.DriverClass, entry.DriverExtensionId, entry.DriverPkgProvider, entry.DriverInfName })
                    .SelectMany(drivers => drivers
                        .GroupBy(entry => new { entry.DriverVersion, entry.DriverDate })
                        .OrderByDescending(g => g.Key.DriverVersion)
                        .ThenByDescending(g => g.Key.DriverDate)
                        .Skip(1)
                        .Where(g => g.All(entry => string.IsNullOrEmpty(entry.DeviceName)))
                        .SelectMany(g => g))
                    .ToArray();

                if (oldDriversToSelect.Length == 0)
                {
                    this.ShowStatus(Status.Warning, Language.Message_No_Old_Drivers_Found);
                }
                else
                {
                    this.lstDriverStoreEntries.CheckedObjects = oldDriversToSelect;
                }
            }
        }

        private void ButtonSelectOldDrivers_Click(object sender, EventArgs e)
        {
            this.CtxMenuSelectOldDrivers_Click(sender, e);
        }

        private void ExportDriverList(IEnumerable objects)
        {
            if (objects == null)
            {
                return;
            }

            List<DriverStoreEntry> driverStoreEntries = objects.OfType<DriverStoreEntry>().ToList();

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

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SwitchCulture(object sender, EventArgs e)
        {
            if ((sender as ToolStripMenuItem)?.Tag is CultureInfo ci)
            {
                byte[] driverStoreViewState = this.lstDriverStoreEntries.SaveState();

                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

                Language.Culture = ci;

                // Clear and redraw the window
                this.SuspendLayout();
                this.Controls.Clear();
                this.InitializeComponent();
                this.BuildLanguageMenu();
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

        private void ExportSelectedDriverListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportDriverList(this.lstDriverStoreEntries.CheckedObjects);
        }

        private void ExportAllDriverListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportDriverList(this.lstDriverStoreEntries.Objects);
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
                List<string> driverList = new List<string>();

                foreach (DriverStoreEntry item in checkedObjects)
                {
                    totalSize += item.DriverSize;
                    driverList.Add($"{item.DriverPublishedName} - {item.DriverFolderName} - {DriverStoreEntry.GetBytesReadable(item.DriverSize)}");
                }

                string message = string.Format(Language.Status_Selected_Drivers, checkedObjects.Count, DriverStoreEntry.GetBytesReadable(totalSize));
                this.ShowStatus(
                    Status.Normal,
                    message,
                    message + Environment.NewLine + string.Join(Environment.NewLine, driverList));
            }
            else
            {
                this.ShowStatus(Status.Normal, Language.Status_No_Drivers_Selected);
            }

            this.buttonDeleteDriver.Enabled = this.lstDriverStoreEntries.CheckedObjects.Count > 0;
            this.exportSelectedDriverListToolStripMenuItem.Enabled = this.buttonDeleteDriver.Enabled;
            this.cbForceDeletion.Enabled = this.buttonDeleteDriver.Enabled;
            this.buttonExportDrivers.Enabled = this.buttonDeleteDriver.Enabled;
        }

        private void UpdateColumnSize()
        {
            // Make the column size fit both header and content.
            for (var i = 0; i < this.lstDriverStoreEntries.Columns.Count - 1; i++)
            {
                this.lstDriverStoreEntries.Columns[i].Width = -2;
            }
        }

        private void CtxMenuOpenDeviceProperties_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.SelectedObject != null)
            {
                DriverStoreEntry item = (DriverStoreEntry)this.lstDriverStoreEntries.SelectedObject;
                Process.Start("rundll32.exe", $"devmgr.dll,DeviceProperties_RunDLL /MachineName \"\" /DeviceID \"{item.DeviceId}\"");
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
            this.buttonExportAllDrivers.Enabled = false;
            this.chooseDriverStoreToolStripMenuItem.Enabled = false;
            this.exportSelectedDriverListToolStripMenuItem.Enabled = false;
            this.exportAllDriverListToolStripMenuItem.Enabled = false;
            this.languageToolStripMenuItem.Enabled = false;
            this.optionsStripMenuItem.Enabled = false;
            this.textBoxSearch.Enabled = false;
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
            this.buttonExportDrivers.Enabled = this.buttonDeleteDriver.Enabled;
            this.buttonExportAllDrivers.Enabled = this.lstDriverStoreEntries.Objects != null;
            this.chooseDriverStoreToolStripMenuItem.Enabled = true;
            this.exportSelectedDriverListToolStripMenuItem.Enabled = this.lstDriverStoreEntries.CheckedObjects.Count > 0;
            this.exportAllDriverListToolStripMenuItem.Enabled = this.lstDriverStoreEntries.Objects != null;
            this.languageToolStripMenuItem.Enabled = true;
            this.optionsStripMenuItem.Enabled = true;
            this.textBoxSearch.Enabled = true;
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
        private async void ButtonExportAllDrivers_Click(object sender, EventArgs e)
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
            return FlexibleMessageBox.Show(
                this,
                text,
                caption,
                buttons,
                icon,
                MessageBoxDefaultButton.Button1);
        }

        private async void UseNativeDriveStoreStripMenuItem_Click(object sender, System.EventArgs e)
        {
            await this.UpdateDriverStoreAPI(DriverStoreOption.Native);
        }

        private async void UseDismStripMenuItem_Click(object sender, System.EventArgs e)
        {
            await this.UpdateDriverStoreAPI(DriverStoreOption.DISM);
        }

        private async void UsePnpUtilStripMenuItem_Click(object sender, System.EventArgs e)
        {
            await this.UpdateDriverStoreAPI(DriverStoreOption.PnpUtil);
        }

        private void IncludeBootCriticalDriversStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Settings.Default.IncludeBootCriticalInOldDriverSelection = this.includeBootCriticalDriversStripMenuItem.Checked;
            Settings.Default.Save();
        }

        private async void CtxMenuExportDriver_Click(object sender, System.EventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                CommonFileDialogResult dialogResult = dialog.ShowDialog();

                if (dialogResult == CommonFileDialogResult.Ok)
                {
                    await ExportDrivers(this.lstDriverStoreEntries.SelectedObjects, dialog.FileName);
                }
            }
        }

        private async void ButtonExportDrivers_Click(object sender, System.EventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                CommonFileDialogResult dialogResult = dialog.ShowDialog();

                if (dialogResult == CommonFileDialogResult.Ok)
                {
                    await ExportDrivers(this.lstDriverStoreEntries.CheckedObjects, dialog.FileName);
                }
            }
        }

        private async Task ExportDrivers(IEnumerable entries, string destinationPath)
        {
            try
            {
                var driverStoreEntries = entries
                    ?.OfType<DriverStoreEntry>()
                    .OrderByColumnName(this.lstDriverStoreEntries.PrimarySortColumn?.AspectName, this.lstDriverStoreEntries.PrimarySortOrder == SortOrder.Ascending)
                    .ThenByColumnName(this.lstDriverStoreEntries.SecondarySortColumn?.AspectName, this.lstDriverStoreEntries.SecondarySortOrder == SortOrder.Ascending)
                    .ToList();

                if (driverStoreEntries?.Count > 0)
                {
                    this.StartOperation();
                    this.ShowStatus(Status.Normal, Language.Status_Exporting_Drivers);

                    (bool allSucceeded, string detailResult) = await Task.Run(() =>
                    {
                        bool totalResult = true;
                        StringBuilder sb = new StringBuilder();

                        if (driverStoreEntries.Count == 1)
                        {
                            totalResult = this.driverStore.ExportDriver(driverStoreEntries[0], destinationPath);
                        }
                        else
                        {
                            foreach (DriverStoreEntry entry in driverStoreEntries)
                            {
                                bool succeeded = this.driverStore.ExportDriver(entry, destinationPath);
                                string resultTxt = string.Format(
                                    succeeded ? Language.Message_Export_Success : Language.Message_Export_Fail,
                                    entry.DriverPublishedName,
                                    entry.DriverFolderName);

                                Trace.TraceInformation(resultTxt);

                                sb.AppendLine(resultTxt);
                                totalResult &= succeeded;
                            }
                        }

                        return (totalResult, sb.ToString());
                    }).ConfigureAwait(true);

                    if (allSucceeded)
                    {
                        this.ShowStatus(Status.Success, Language.Message_Export_Drivers_Success);
                    }
                    else
                    {
                        this.ShowStatus(Status.Error, Language.Message_Export_Drivers_Error, usePopup: true);
                    }
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

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            // Reset the debounce timer
            this.searchDebounceTimer?.Change(SearchDebounceDelay, Timeout.Infinite);
        }

        private void UpdateSearchFilter()
        {
            if (textBoxSearch.Text == Language.Message_Type_Here_To_Search || string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                this.lstDriverStoreEntries.ModelFilter = null;
                this.lstDriverStoreEntries.DefaultRenderer = null;
                if (this.lstDriverStoreEntries.EmptyListMsg == Language.Message_No_Entries)
                {
                    this.lstDriverStoreEntries.EmptyListMsg = Language.Message_No_Entries;
                }
            }
            else
            {
                if (this.lstDriverStoreEntries.EmptyListMsg == Language.Message_No_Entries)
                {
                    this.lstDriverStoreEntries.EmptyListMsg = this.lstDriverStoreEntries.Objects != null ? Language.Message_No_Match_Result : Language.Message_No_Entries;
                }

                TextMatchFilter filter = TextMatchFilter.Contains(this.lstDriverStoreEntries, textBoxSearch.Text);
                this.lstDriverStoreEntries.ModelFilter = filter;
                this.lstDriverStoreEntries.DefaultRenderer = new HighlightTextRenderer(filter);
            }
        }

        private void TextBoxSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                textBoxSearch.Text = Language.Message_Type_Here_To_Search;
            }
        }

        private void TextBoxSearch_Enter(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == Language.Message_Type_Here_To_Search)
            {
                textBoxSearch.Text = "";
            }
        }

        private IEnumerable<string> FindInfFile(string infPath)
        {
            // Search for *.inf files in the current directory  
            var infFiles = Directory.GetFiles(infPath, "*.inf", SearchOption.TopDirectoryOnly);
            if (infFiles.Length > 0)
            {
                yield return infFiles[0];
                yield break;
            }

            // If no .inf files are found, search one level down in subdirectories  
            var subDirectories = Directory.GetDirectories(infPath);
            foreach (var subDir in subDirectories)
            {
                infFiles = Directory.GetFiles(subDir, "*.inf", SearchOption.TopDirectoryOnly);
                if (infFiles.Length > 0)
                {
                    yield return infFiles[0];
                }
            }
        }
    }
}
