using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Utils;
using System.Security.Principal;

namespace Rapr
{
    public partial class Form1 : Form
    {
        IDriverStore driverStore;       
        Color SavedBackColor, SavedForeColor;
        OperationContext context = new OperationContext();

     
        public Form1()
        {
            InitializeComponent();            
            AppState.MainForm = this;
            AppState.EnableFileLogging();
            driverStore = AppState.GetDriverStoreHandler();
            if (!AppState.IsOSSupported())
            {
                MessageBox.Show("This utility cannot be run in pre-Vista OS", "Rapr", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            SavedBackColor = lblStatus.BackColor;
            SavedForeColor = lblStatus.ForeColor;

            if (!IsAnAdministrator())
            {
                Text = Text + " [Read-Only Mode]";
                ShowStatus("Running in Read-Only mode", Status.Warning);
                groupBoxAddDriver.Enabled = false;
                groupBoxDeleteDriver.Enabled = false;
                MessageBox.Show("Started in non-admin mode. Some of the features are disabled.\nRestart the app in ADMIN mode to enable them"
                    , "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Trace.TraceInformation("Shutting down - reason " + e.CloseReason);
            AppState.Cleanup();
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

            DriverStoreEntry dse = new DriverStoreEntry();  // for appeasing the compiler!
            List<DriverStoreEntry> ldse = new List<DriverStoreEntry>();
            string msgWarning = "";
            bool fMultiPackageDeletion = false;
            if (lstDriverStoreEntries.CheckedObjects.Count == 0)
            {
               dse = (DriverStoreEntry)lstDriverStoreEntries.GetSelectedObject();
               msgWarning = String.Format("About to {0} {1} from driver store.\nAre you sure?", 
                                            cbForceDeletion.Checked == true ? "force delete" : "delete", 
                                            dse.driverPublishedName
                                            );
            }
            else
            {
                if (lstDriverStoreEntries.CheckedItems.Count > 0)
                {
                    //string s = "";                    
                    foreach (DriverStoreEntry o in lstDriverStoreEntries.CheckedObjects)
                    {
                        //s += o.driverPublishedName + "\n";
                        ldse.Add(o);
                    }
                    //MessageBox.Show(s);
                    fMultiPackageDeletion = true;
                    msgWarning = String.Format("About to {0} {1} packages from driver store.\nAre you sure?",
                                            cbForceDeletion.Checked == true ? "force delete" : "delete",
                                            ldse.Count
                                            );
                }
                else
                {
                    throw new Exception("Unknown state - CP100");   //Checkpoint - CP100
                }
            }
            if (DialogResult.OK == MessageBox.Show
                                (msgWarning, 
                                "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                                )
            {                
                if (fMultiPackageDeletion == false)
                    DeleteDriverPackage(dse);
                else
                    DeleteDriverPackages(ldse);
            }
        }

        private void buttonAddDriver_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string pkgFolder = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                string infName = System.IO.Path.GetFileName(openFileDialog.FileName);

                AddDriverPackage(infName);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            OperationContext localContext = (OperationContext) e.Argument;

            switch (localContext.code)
            {
                case OperationCode.EnumerateStore:
                    List<DriverStoreEntry> ldse = driverStore.EnumeratePackages();
                    localContext.resultData = ldse;                                                   
                    break;
                case OperationCode.DeleteDriver:
                    if (!localContext.IsCollectionPassed)
                        localContext.resultStatus = driverStore.DeletePackage(localContext.dse, false);
                    else
                    {
                        bool result = true, temp = true;
                        string resultTxt;
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        foreach (DriverStoreEntry dse in localContext.ldse)
                        {
                            temp = driverStore.DeletePackage(dse, false);
                            resultTxt = String.Format("Delete({0}) {1}", dse.driverPublishedName,
                                temp == true ? "succeeded" : "failed");
                            System.Diagnostics.Trace.TraceInformation(resultTxt + Environment.NewLine);

                            sb.AppendLine(resultTxt);

                            result &= temp;
                        }
                        localContext.resultStatus = result;
                        localContext.resultData = sb.ToString();
                    }
                    break;
                case OperationCode.ForceDeleteDriver:
                    if (!localContext.IsCollectionPassed)
                        localContext.resultStatus = driverStore.DeletePackage(localContext.dse, true);
                    else
                    {
                        bool result = true, temp = true;
                        string resultTxt;
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        foreach (DriverStoreEntry dse in localContext.ldse)
                        {
                            temp = driverStore.DeletePackage(dse, true);
                            resultTxt = String.Format("ForceDelete({0}) {1}", dse.driverPublishedName,
                                    temp == true ? "succeeded" : "failed");
                            System.Diagnostics.Trace.TraceInformation(resultTxt + Environment.NewLine);

                            sb.AppendLine(resultTxt);

                            result &= temp;
                        }
                        localContext.resultStatus = result;
                        localContext.resultData = sb.ToString();
                    }
                    break;
                case OperationCode.AddDriver:
                    localContext.resultStatus = driverStore.AddPackage(localContext.infPath, false);
                    break;
                case OperationCode.AddInstallDriver:
                    localContext.resultStatus = driverStore.AddPackage(localContext.infPath, true);
                    break;
                case OperationCode.Dummy:
                    throw new Exception("Invalid argument rcvd by bgroundWorker");
            }            
            e.Result = localContext;
            AppState.FlushTrace();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            OperationContext localContext = (OperationContext)e.Result;
            string result;

            switch (localContext.code)
            {
                case OperationCode.EnumerateStore:
                    List<DriverStoreEntry> ldse = localContext.resultData as List<DriverStoreEntry>;
                    lstDriverStoreEntries.SetObjects(ldse);
                    //lstDriverStoreEntries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    //ShowStatus("Driver store enumerated", Status.Success);
                    break;
                
                case OperationCode.ForceDeleteDriver:
                case OperationCode.DeleteDriver:
                    if (!(localContext.IsCollectionPassed))
                    {
                        if ((bool)localContext.resultStatus == true)
                        {
                            result = String.Format("Removed the package {0} from driver store", localContext.dse.driverPublishedName);

                            // refresh the UI
                            PopulateUIWithDriverStoreEntries();
                            ShowStatus(result, Status.Success);
                        }
                        else
                        {
                            result = String.Format("Error removing the package {0} from driver store{1}", localContext.dse.driverPublishedName,
                                localContext.code == OperationCode.DeleteDriver ? " [TIP: Try FORCE deleting the package]" : "");

                            ShowStatus(result, Status.Error);
                        }
                    }
                    else
                    {
                        if ((bool)localContext.resultStatus == true)
                        {
                            result = String.Format("Removed {0} packages from driver store", localContext.ldse.Count);

                            // refresh the UI
                            PopulateUIWithDriverStoreEntries();

                            ShowStatus(result, Status.Success);
                        }
                        else
                        {
                            result = String.Format("Error removing the packages from driver store{1}", localContext.dse.driverPublishedName,
                                localContext.code == OperationCode.DeleteDriver ? " [TIP: Try FORCE deleting the package]" : "");

                            // refresh the UI
                            PopulateUIWithDriverStoreEntries();

                            ShowStatus(result, Status.Error);

                            MessageBox.Show(
                                localContext.resultData as string, 
                                "Detailed Error Log", MessageBoxButtons.OK, MessageBoxIcon.Error );
                        }
                    }
                    cbForceDeletion.Checked = false;
                    
                    break;                

                case OperationCode.AddDriver:
                case OperationCode.AddInstallDriver:                    
                    if ((bool) localContext.resultStatus == true)
                    {
                        result = String.Format("Added{1} the package {0} to driver store", localContext.infPath,
                                                localContext.code == OperationCode.AddInstallDriver? " & installed " : "");

                        // refresh the UI
                        PopulateUIWithDriverStoreEntries();
                        //MessageBox.Show(, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ShowStatus(result, Status.Success);
                    }
                    else
                    {
                        result = String.Format("Error adding{1} the package {0} to driver store", 
                                                localContext.infPath, 
                                                localContext.code == OperationCode.AddInstallDriver ? " & installing " : "");                     
                        ShowStatus(result, Status.Error);
                    }                    
                    cbAddInstall.Checked = false;
                    break;                    
            }
            ShowOperationInProgress(false);
            AppState.FlushTrace();
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
                ctxtMenuSelect.Enabled = true;
                if ((lstDriverStoreEntries.CheckedObjects != null) && lstDriverStoreEntries.CheckedObjects.Count != 0)
                {
                    ctxtMenuSelect.Text = "Unselect All";
                }
                else
                {
                    ctxtMenuSelect.Text = "Select All";
                }
            }
            else
            {
                ctxtMenuSelect.Enabled = false;
                ctxtMenuSelect.Text = "No entries loaded";
            }
        }

        // Function to switch between "selected" and "unselected" states
        private void ctxtMenuSelect_Click(object sender, EventArgs e)
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

        private void ctxtMenuAbout_Click(object sender, EventArgs e)
        {
            ShowAboutBox();
        }
      
    }
}