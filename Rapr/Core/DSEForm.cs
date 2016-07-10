using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Rapr.Utils;

namespace Rapr
{
    public partial class DSEForm : Form
    {
        IDriverStore driverStore;
        Color SavedBackColor, SavedForeColor;
        OperationContext context = new OperationContext();

        public DSEForm()
        {
            InitializeComponent();
            lstDriverStoreEntries.AlwaysGroupByColumn = this.driverClassColumn;
            lstDriverStoreEntries.AlwaysGroupBySortOrder = SortOrder.Ascending;
            lstDriverStoreEntries.PrimarySortColumn = this.driverInfColumn;
            lstDriverStoreEntries.PrimarySortOrder = SortOrder.Ascending;
            lstDriverStoreEntries.SecondarySortColumn = this.driverVersionColumn;
            lstDriverStoreEntries.SecondarySortOrder = SortOrder.Descending;
            driverSizeColumn.AspectToStringConverter = size => DriverStoreEntry.GetBytesReadable((long)size);

            AppContext.MainForm = this;
            AppContext.EnableLogging();
            driverStore = AppContext.GetDriverStoreHandler();
            if (!AppContext.IsOSSupported())
            {
                MessageBox.Show("This utility cannot be run in pre-Vista OS", "Rapr", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }

        private void DSEForm_Shown(object sender, EventArgs e)
        {
            SavedBackColor = lblStatus.BackColor;
            SavedForeColor = lblStatus.ForeColor;

            if (!IsAnAdministrator())
            {
                Text = Text + " [Read-Only Mode]";
                ShowStatus("Running in Read-Only mode", Status.Warning);
                buttonAddDriver.Enabled = false;
                cbAddInstall.Enabled = false;
                buttonDeleteDriver.Enabled = false;
                cbForceDeletion.Enabled = false;
                labelRunAsAdmin.Visible = true;
                buttonRunAsAdmin.Visible = true;
            }

            PopulateUIWithDriverStoreEntries();
        }

        private void DSEForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            AppContext.TraceInformation("Shutting down - reason " + e.CloseReason);
            AppContext.Cleanup();
        }

        private void buttonEnumerate_Click(object sender, EventArgs e)
        {
            PopulateUIWithDriverStoreEntries();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (lstDriverStoreEntries.CheckedObjects.Count == 0 && lstDriverStoreEntries.SelectedIndex == -1)
            {
                // No entry is selected 
                ShowStatus("Select a driver entry first", Status.Warning);
                return;
            }

            List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();
            if (lstDriverStoreEntries.CheckedObjects.Count == 0)
            {
                foreach (DriverStoreEntry o in lstDriverStoreEntries.SelectedObjects)
                {
                    driverStoreEntries.Add(o);
                }
            }
            else if (lstDriverStoreEntries.CheckedItems.Count > 0)
            {
                foreach (DriverStoreEntry o in lstDriverStoreEntries.CheckedObjects)
                {
                    driverStoreEntries.Add(o);
                }
            }

            DeleteDriverStoreEntries(driverStoreEntries);
        }

        private void DeleteDriverStoreEntries(List<DriverStoreEntry> driverStoreEntries)
        {
            string msgWarning;

            if (driverStoreEntries != null && driverStoreEntries.Count > 0)
            {
                if (driverStoreEntries.Count == 1)
                {
                    msgWarning = String.Format(
                        "About to {0} {1} from driver store.\nAre you sure?",
                        cbForceDeletion.Checked ? "force delete" : "delete",
                        driverStoreEntries[0].DriverPublishedName);
                }
                else
                {
                    msgWarning = String.Format(
                        "About to {0} {1} packages from driver store.\nAre you sure?",
                        cbForceDeletion.Checked ? "force delete" : "delete",
                        driverStoreEntries.Count);
                }

                if (DialogResult.OK == MessageBox.Show(
                    msgWarning,
                    "Warning",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning))
                {
                    DeleteDriverPackages(driverStoreEntries);
                }
            }
        }

        private void buttonAddDriver_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string pkgFolder = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                string infName = System.IO.Path.GetFileName(openFileDialog.FileName);

                AddDriverPackage(openFileDialog.FileName);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            OperationContext localContext = (OperationContext)e.Argument;

            switch (localContext.Code)
            {
                case OperationCode.EnumerateStore:
                    localContext.ResultData = driverStore.EnumeratePackages();
                    break;

                case OperationCode.DeleteDriver:
                    DeleteDriver(ref localContext, false);
                    break;

                case OperationCode.ForceDeleteDriver:
                    DeleteDriver(ref localContext, true);
                    break;

                case OperationCode.AddDriver:
                    localContext.ResultStatus = driverStore.AddPackage(localContext.InfPath, false);
                    break;

                case OperationCode.AddInstallDriver:
                    localContext.ResultStatus = driverStore.AddPackage(localContext.InfPath, true);
                    break;

                case OperationCode.Dummy:
                    throw new Exception("Invalid argument rcvd by bgroundWorker");
            }

            e.Result = localContext;
            AppContext.FlushTrace();
        }

        private void DeleteDriver(ref OperationContext localContext, bool force)
        {
            if (localContext.DriverStoreEntries != null)
            {
                bool totalResult = true;
                StringBuilder sb = new StringBuilder();

                if (localContext.DriverStoreEntries.Count == 1)
                {
                    localContext.ResultStatus = driverStore.DeletePackage(localContext.DriverStoreEntries[0], force);
                }
                else
                {
                    foreach (DriverStoreEntry dse in localContext.DriverStoreEntries)
                    {
                        bool result = driverStore.DeletePackage(dse, force);
                        string resultTxt = String.Format(
                            "Delete {0} {1}",
                            dse.DriverPublishedName,
                            result ? "succeeded." : "failed.");
                        AppContext.TraceInformation(resultTxt + Environment.NewLine);

                        sb.AppendLine(resultTxt);
                        totalResult &= result;
                    }

                    localContext.ResultStatus = totalResult;
                    localContext.ResultData = sb.ToString();
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            OperationContext localContext = (OperationContext)e.Result;
            string result;

            switch (localContext.Code)
            {
                case OperationCode.EnumerateStore:
                    List<DriverStoreEntry> ldse = localContext.ResultData as List<DriverStoreEntry>;
                    lstDriverStoreEntries.SetObjects(ldse);
                    lstDriverStoreEntries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    ShowStatus("Driver store enumerated", Status.Success);
                    break;

                case OperationCode.ForceDeleteDriver:
                case OperationCode.DeleteDriver:
                    if ((bool)localContext.ResultStatus)
                    {
                        if (localContext.DriverStoreEntries.Count == 1)
                        {
                            result = String.Format("Removed the package {0} from driver store", localContext.DriverStoreEntries[0].DriverPublishedName);
                        }
                        else
                        {
                            result = String.Format("Removed {0} packages from driver store", localContext.DriverStoreEntries.Count);
                        }

                        // refresh the UI
                        PopulateUIWithDriverStoreEntries();

                        ShowStatus(result, Status.Success);
                    }
                    else
                    {
                        if (localContext.DriverStoreEntries.Count == 1)
                        {
                            result = String.Format("Error removing the package {0} from driver store{1}", localContext.DriverStoreEntries[0].DriverPublishedName,
                            localContext.Code == OperationCode.DeleteDriver ? " [TIP: Try FORCE deleting the package]" : "");

                            ShowStatus(result, Status.Error);
                        }
                        else
                        {
                            result = String.Format(
                                "Error removing some packages from driver store{0}\r\n{1}",
                                localContext.Code == OperationCode.DeleteDriver ? " [TIP: Try FORCE deleting the package]" : "",
                                localContext.ResultData as string);

                            // refresh the UI
                            PopulateUIWithDriverStoreEntries();

                            MessageBox.Show(
                                result,
                                "Detailed Error Log", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    cbForceDeletion.Checked = false;

                    break;

                case OperationCode.AddDriver:
                case OperationCode.AddInstallDriver:
                    if ((bool)localContext.ResultStatus == true)
                    {
                        result = String.Format("Added{1} the package {0} to driver store", localContext.InfPath,
                                                localContext.Code == OperationCode.AddInstallDriver ? " & installed " : "");

                        // refresh the UI
                        PopulateUIWithDriverStoreEntries();
                        //MessageBox.Show(, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ShowStatus(result, Status.Success);
                    }
                    else
                    {
                        result = String.Format("Error adding{1} the package {0} to driver store",
                                                localContext.InfPath,
                                                localContext.Code == OperationCode.AddInstallDriver ? " & installing " : "");
                        ShowStatus(result, Status.Error);
                    }
                    cbAddInstall.Checked = false;
                    break;
            }
            ShowOperationInProgress(false);
            AppContext.FlushTrace();
        }

        private static void ShowAboutBox()
        {
            using (AboutBox ab = new AboutBox())
            {
                ab.ShowDialog();
            }
        }
        private void linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowAboutBox();
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // Check if there are any entries
            if ((lstDriverStoreEntries.Objects != null))
            {
                ctxMenuSelectAll.Enabled = true;
                ctxMenuExport.Enabled = true;
                if (lstDriverStoreEntries.CheckedObjects != null && lstDriverStoreEntries.CheckedObjects.Count > 0)
                {
                    ctxMenuSelectAll.Text = "Unselect All";
                }
                else
                {
                    ctxMenuSelectAll.Text = "Select All";
                }

                if (lstDriverStoreEntries.SelectedObjects != null && lstDriverStoreEntries.SelectedObjects.Count > 0)
                {
                    ctxMenuDelete.Enabled = IsAnAdministrator();

                    if (lstDriverStoreEntries.CheckedObjects != null
                        && lstDriverStoreEntries.CheckedObjects.Count > 0
                        && new ArrayList(lstDriverStoreEntries.SelectedObjects).ToArray().All(i => lstDriverStoreEntries.CheckedObjects.Contains(i)))
                    {
                        ctxMenuSelect.Text = "Unselect";
                    }
                    else
                    {
                        ctxMenuSelect.Text = "Select";
                    }

                    ctxMenuSelect.Enabled = true;
                }
                else
                {
                    ctxMenuDelete.Enabled = false;
                    ctxMenuSelect.Enabled = false;
                }
            }
            else
            {
                ctxMenuSelect.Enabled = false;
                ctxMenuSelectAll.Enabled = false;
                ctxMenuDelete.Enabled = false;
                ctxMenuExport.Enabled = false;
                //ctxMenuSelectAll.Text = "No entries loaded";
            }
        }

        // Function to switch between "selected" and "unselected" states
        private void ctxMenuSelectAll_Click(object sender, EventArgs e)
        {
            // Check if there are any entries
            if ((lstDriverStoreEntries.Objects != null))
            {
                if (lstDriverStoreEntries.CheckedObjects != null && lstDriverStoreEntries.CheckedObjects.Count != 0)
                {
                    lstDriverStoreEntries.CheckedObjects = null;
                }
                else
                {
                    lstDriverStoreEntries.CheckedObjects = lstDriverStoreEntries.Objects as System.Collections.IList;
                }
            }
        }

        private void ctxMenuSelect_Click(object sender, EventArgs e)
        {
            if ((lstDriverStoreEntries.Objects != null))
            {
                ArrayList list = new ArrayList();
                if (lstDriverStoreEntries.CheckedObjects != null && lstDriverStoreEntries.CheckedObjects.Count > 0)
                {
                    list.AddRange(lstDriverStoreEntries.CheckedObjects);
                }

                if (lstDriverStoreEntries.SelectedObjects != null && lstDriverStoreEntries.SelectedObjects.Count > 0)
                {
                    if (new ArrayList(lstDriverStoreEntries.SelectedObjects).ToArray().All(i => lstDriverStoreEntries.CheckedObjects.Contains(i)))
                    {
                        foreach (var item in lstDriverStoreEntries.SelectedObjects)
                        {
                            list.Remove(item);
                        }
                    }
                    else
                    {
                        list.AddRange(lstDriverStoreEntries.SelectedObjects);
                    }
                }

                lstDriverStoreEntries.CheckedObjects = list;
            }
        }

        private void ctxMenuDelete_Click(object sender, EventArgs e)
        {
            if (lstDriverStoreEntries.SelectedObjects != null)
            {
                List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();

                foreach (DriverStoreEntry item in lstDriverStoreEntries.SelectedObjects)
                {
                    driverStoreEntries.Add(item);
                }

                DeleteDriverStoreEntries(driverStoreEntries);
            }
        }

        private void buttonRunAsAdmin_Click(object sender, EventArgs e)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = Assembly.GetExecutingAssembly().Location;

            Process.Start(processInfo);
            Application.Exit();
        }

        private void ctxMenuExport_Click(object sender, EventArgs e)
        {
            // Check if there are any entries
            if ((lstDriverStoreEntries.Objects != null))
            {
                try
                {
                    List<DriverStoreEntry> ldse = lstDriverStoreEntries.Objects as List<DriverStoreEntry>;
                    IExport exporter = new CSVExporter();   // TODO: Factory?? Change this when we add support for 
                                                            // direct Excel export
                    exporter.Export(ldse);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed: " + ex.Message);
                }
            }
        }
    }
}