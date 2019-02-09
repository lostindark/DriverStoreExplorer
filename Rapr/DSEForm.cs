using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Rapr.Lang;
using Rapr.Properties;
using Rapr.Utils;

namespace Rapr
{
    public partial class DSEForm : Form
    {
        private readonly IDriverStore driverStore;
        private Color savedBackColor;
        private Color savedForeColor;
        private OperationContext context = new OperationContext();

        private static readonly CultureInfo[] SupportedLanguage = {
            new CultureInfo("en"),
            new CultureInfo("fr-FR"),
            new CultureInfo("ja-JP"),
            new CultureInfo("ru-RU"),
            new CultureInfo("zh-CN"),
        };

        public DSEForm()
        {
            if (!IsOSSupported())
            {
                MessageBox.Show(
                    Language.Message_Requires_Later_OS,
                    Language.Product_Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Application.Exit();
            }

            if (!isRunAsAdministrator)
            {
                RunAsAdministrator();
            }

            var lang = Settings.Default.Language;
            if (lang != null && !CultureInfo.InvariantCulture.Equals(lang))
            {
                Thread.CurrentThread.CurrentCulture = lang;
                Thread.CurrentThread.CurrentUICulture = lang;
            }

            this.InitializeComponent();

            this.Icon = ExtractAssociatedIcon(Application.ExecutablePath);
            this.BuildLanguageMenu();

            this.lstDriverStoreEntries.PrimarySortColumn = this.driverClassColumn;
            this.lstDriverStoreEntries.PrimarySortOrder = SortOrder.Ascending;
            this.lstDriverStoreEntries.SecondarySortColumn = this.driverDateColumn;
            this.lstDriverStoreEntries.SecondarySortOrder = SortOrder.Descending;
            this.lstDriverStoreEntries.CheckBoxes = isRunAsAdministrator;

            this.SetupListViewColumns();

            Trace.TraceInformation("---------------------------------------------------------------");
            Trace.TraceInformation($"{Application.ProductName} started");

            this.driverStore = new DismUtil();
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
                return new Version(driver.DriverVersion.Major, driver.DriverVersion.Minor);
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
        }

        /// <summary>
        /// Returns an icon representation of an image contained in the specified file.
        /// This function is identical to System.Drawing.Icon.ExtractAssociatedIcon, except this version works.
        /// </summary>
        /// <param name="filePath">The path to the file that contains an image.</param>
        /// <returns>The System.Drawing.Icon representation of the image contained in the specified file.</returns>
        /// <exception cref="System.ArgumentException">filePath does not indicate a valid file.</exception>
        public static Icon ExtractAssociatedIcon(string filePath)
        {
            int index = 0;

            Uri uri;

            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            try
            {
                uri = new Uri(filePath);
            }
            catch (UriFormatException)
            {
                filePath = Path.GetFullPath(filePath);
                uri = new Uri(filePath);
            }

            if (uri.IsFile)
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException(filePath);
                }

                StringBuilder iconPath = new StringBuilder(filePath, 260);

                IntPtr handle = SafeNativeMethods.ExtractAssociatedIcon(
                    new HandleRef(null, IntPtr.Zero),
                    iconPath,
                    ref index);

                if (handle != IntPtr.Zero)
                {
                    return Icon.FromHandle(handle);
                }
            }

            return null;
        }

        private void BuildLanguageMenu()
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

        private void DSEForm_Shown(object sender, EventArgs e)
        {
            this.savedBackColor = this.lblStatus.BackColor;
            this.savedForeColor = this.lblStatus.ForeColor;

            if (!isRunAsAdministrator)
            {
                this.Text += " " + Language.Product_Name_Additional_ReadOnly;
                this.ShowStatus(Language.Label_RunAsAdmin, Status.Warning);
                this.buttonAddDriver.Enabled = false;
                this.buttonDeleteDriver.Enabled = false;
                this.buttonSelectOldDrivers.Enabled = false;
            }

            this.PopulateUIWithDriverStoreEntries();
        }

        private void DSEForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Trace.TraceInformation($"Shutting down - reason {e.CloseReason}");
        }

        private void ButtonEnumerate_Click(object sender, EventArgs e)
        {
            this.PopulateUIWithDriverStoreEntries();
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.CheckedObjects.Count == 0 && this.lstDriverStoreEntries.SelectedIndex == -1)
            {
                // No entry is selected 
                this.ShowStatus(Language.Message_Select_Driver_Entry, Status.Warning);
                return;
            }

            List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();
            if (this.lstDriverStoreEntries.CheckedObjects.Count == 0)
            {
                foreach (DriverStoreEntry o in this.lstDriverStoreEntries.SelectedObjects)
                {
                    driverStoreEntries.Add(o);
                }
            }
            else if (this.lstDriverStoreEntries.CheckedItems.Count > 0)
            {
                foreach (DriverStoreEntry o in this.lstDriverStoreEntries.CheckedObjects)
                {
                    driverStoreEntries.Add(o);
                }
            }

            this.DeleteDriverStoreEntries(driverStoreEntries);
        }

        private void DeleteDriverStoreEntries(List<DriverStoreEntry> driverStoreEntries)
        {
            string msgWarning;

            if (driverStoreEntries?.Count > 0)
            {
                if (driverStoreEntries.Count == 1)
                {
                    msgWarning = string.Format(
                        Language.Message_Delete_Single_Package,
                        Language.Message_Delete,
                        driverStoreEntries[0].DriverInfName,
                        driverStoreEntries[0].DriverPublishedName);
                }
                else
                {
                    msgWarning = string.Format(
                        Language.Message_Delete_Multiple_Packages,
                        Language.Message_Delete,
                        driverStoreEntries.Count);
                }

                msgWarning += Environment.NewLine + Language.Message_Sure;

                if (DialogResult.OK == MessageBox.Show(
                    msgWarning,
                    Language.Message_Title_Warning,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning))
                {
                    this.DeleteDriverPackages(driverStoreEntries);
                }
            }
        }

        private void ButtonAddDriver_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string pkgFolder = Path.GetDirectoryName(this.openFileDialog.FileName);
                string infName = Path.GetFileName(this.openFileDialog.FileName);

                this.AddDriverPackage(this.openFileDialog.FileName);
            }
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            OperationContext localContext = (OperationContext)e.Argument;

            switch (localContext.Code)
            {
                case OperationCode.EnumerateStore:
                    localContext.ResultData = this.driverStore.EnumeratePackages();
                    break;

                case OperationCode.DeleteDriver:
                    this.DeleteDriver(ref localContext, false);
                    break;

                case OperationCode.ForceDeleteDriver:
                    this.DeleteDriver(ref localContext, true);
                    break;

                case OperationCode.AddDriver:
                    localContext.ResultStatus = this.driverStore.AddDriver(localContext.InfPath, false);
                    break;

                case OperationCode.AddInstallDriver:
                    localContext.ResultStatus = this.driverStore.AddDriver(localContext.InfPath, true);
                    break;

                case OperationCode.Dummy:
                    throw new Exception("Invalid argument rcvd by bgroundWorker");
            }

            e.Result = localContext;
        }

        private void DeleteDriver(ref OperationContext localContext, bool force)
        {
            if (localContext.DriverStoreEntries != null)
            {
                bool totalResult = true;
                StringBuilder sb = new StringBuilder();

                if (localContext.DriverStoreEntries.Count == 1)
                {
                    localContext.ResultStatus = this.driverStore.DeleteDriver(localContext.DriverStoreEntries[0], force);
                }
                else
                {
                    foreach (DriverStoreEntry dse in localContext.DriverStoreEntries)
                    {
                        bool result = this.driverStore.DeleteDriver(dse, force);
                        string resultTxt = string.Format(
                            Language.Message_Delete_Result,
                            dse.DriverInfName,
                            dse.DriverPublishedName,
                            result ? Language.Message_Success : Language.Message_Failed);

                        Trace.TraceInformation(resultTxt);

                        sb.AppendLine(resultTxt);
                        totalResult &= result;
                    }

                    localContext.ResultStatus = totalResult;
                    localContext.ResultData = sb.ToString();
                }
            }
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OperationContext localContext = (OperationContext)e.Result;
            string result;

            switch (localContext.Code)
            {
                case OperationCode.EnumerateStore:
                    this.lstDriverStoreEntries.SetObjects(localContext.ResultData as List<DriverStoreEntry>);
                    this.UpdateColumnSize();
                    break;

                case OperationCode.ForceDeleteDriver:
                case OperationCode.DeleteDriver:
                    if (localContext.ResultStatus)
                    {
                        if (localContext.DriverStoreEntries.Count == 1)
                        {
                            result = string.Format(
                                Language.Message_Delete_Package,
                                localContext.DriverStoreEntries[0].DriverInfName,
                                localContext.DriverStoreEntries[0].DriverPublishedName);
                        }
                        else
                        {
                            result = string.Format(
                                Language.Message_Delete_Packages,
                                localContext.DriverStoreEntries.Count.ToString());
                        }

                        // refresh the UI
                        this.PopulateUIWithDriverStoreEntries();

                        this.ShowStatus(result, Status.Success);
                    }
                    else
                    {
                        string driverDeleteTip = localContext.Code == OperationCode.DeleteDriver
                            ? " " + Language.Tip_Driver_In_Use
                            : string.Empty;

                        if (localContext.DriverStoreEntries.Count == 1)
                        {
                            result = string.Format(
                                Language.Message_Delete_Package_Error,
                                localContext.DriverStoreEntries[0].DriverInfName,
                                localContext.DriverStoreEntries[0].DriverPublishedName,
                                driverDeleteTip);
                        }
                        else
                        {
                            result = string.Format(Language.Message_Delete_Packages_Error, driverDeleteTip);
                            string fullResult = $"{result}{Environment.NewLine}{localContext.ResultData as string}";

                            // refresh the UI
                            this.PopulateUIWithDriverStoreEntries();

                            MessageBox.Show(
                                fullResult,
                                Language.Message_Detailed_Log,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }

                        this.ShowStatus(result, Status.Error);
                    }

                    break;

                case OperationCode.AddDriver:
                case OperationCode.AddInstallDriver:
                    if (localContext.ResultStatus)
                    {
                        result = string.Format(
                            Language.Message_Driver_Added,
                            localContext.Code == OperationCode.AddInstallDriver
                                ? Language.Message_Driver_And_Installed
                                : "",
                            localContext.InfPath);

                        // refresh the UI
                        this.PopulateUIWithDriverStoreEntries();
                        this.ShowStatus(result, Status.Success);
                    }
                    else
                    {
                        result = string.Format(
                            Language.Message_Driver_Added_Error,
                            localContext.Code == OperationCode.AddInstallDriver
                                ? Language.Message_Driver_And_Installed
                                : "",
                            localContext.InfPath);

                        this.ShowStatus(result, Status.Error);
                    }

                    break;
            }

            this.ShowOperationInProgress(false);
        }

        private static void ShowAboutBox()
        {
            using (AboutBox ab = new AboutBox())
            {
                ab.ShowDialog();
            }
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // Check if there are any entries
            if (this.lstDriverStoreEntries.Objects != null)
            {
                this.ctxMenuSelectAll.Enabled = isRunAsAdministrator;
                this.ctxMenuSelectOldDrivers.Enabled = isRunAsAdministrator;

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
                    this.ctxMenuDelete.Enabled = isRunAsAdministrator;

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

                    this.ctxMenuSelect.Enabled = isRunAsAdministrator;

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

        private void CtxMenuDelete_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.SelectedObjects != null)
            {
                List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();

                foreach (DriverStoreEntry item in this.lstDriverStoreEntries.SelectedObjects)
                {
                    driverStoreEntries.Add(item);
                }

                this.DeleteDriverStoreEntries(driverStoreEntries);
            }
        }

        private void CtxMenuSelectOldDrivers_Click(object sender, EventArgs e)
        {
            if (this.lstDriverStoreEntries.Objects != null)
            {
                List<DriverStoreEntry> driverStoreEntryList = this.lstDriverStoreEntries.Objects as List<DriverStoreEntry>;

                this.lstDriverStoreEntries.CheckedObjects = driverStoreEntryList
                    .GroupBy(entry => new { entry.DriverClass, entry.DriverPkgProvider, entry.DriverInfName })
                    .SelectMany(g => g.OrderByDescending(row => row.DriverVersion).Skip(1))
                    .ToArray();
            }
        }

        private void ButtonSelectOldDrivers_Click(object sender, EventArgs e)
        {
            this.CtxMenuSelectOldDrivers_Click(sender, e);
        }

        private void ExportList()
        {
            // Check if there are any entries
            if (this.lstDriverStoreEntries.Objects != null)
            {
                try
                {
                    List<DriverStoreEntry> ldse = this.lstDriverStoreEntries.Objects as List<DriverStoreEntry>;
                    IExport exporter = new CSVExporter();   // TODO: Factory?? Change this when we add support for 
                                                            // direct Excel export
                    string fileName = exporter.Export(ldse);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        string message = string.Format(Language.Export_Complete, fileName);
                        MessageBox.Show(message);
                        this.ShowStatus(message, Status.Normal);
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format(Language.Export_Failed, ex);
                    MessageBox.Show(message);
                    this.ShowStatus(message, Status.Error);
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
                this.SetupListViewColumns();
                this.lstDriverStoreEntries.RestoreState(driverStoreViewState);

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
                MessageBox.Show(Language.Message_Log_File_NotFound, Language.Message_Error);
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ExportList();
        }

        private void RunAsAdminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunAsAdministrator();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAboutBox();
        }

        private void DSEForm_Load(object sender, EventArgs e)
        {
            this.lstDriverStoreEntries.RestoreState(Convert.FromBase64String(Settings.Default.DriverStoreViewState));

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
            IList checkedObjects = this.lstDriverStoreEntries.CheckedObjects;

            if (checkedObjects?.Count > 0)
            {
                long totalSize = 0;

                foreach (DriverStoreEntry item in checkedObjects)
                {
                    totalSize += item.DriverSize;
                }

                this.ShowStatus(string.Format(
                    Language.Status_Selected_Drivers,
                    checkedObjects.Count.ToString(),
                    DriverStoreEntry.GetBytesReadable(totalSize)),
                    Status.Normal);
            }
            else
            {
                this.ShowStatus(Language.Status_No_Drivers_Selected, Status.Normal);
            }
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
    }
}
