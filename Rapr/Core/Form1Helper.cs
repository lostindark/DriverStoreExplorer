using System;
using System.Drawing;
using System.Windows.Forms;
using Utils;
using System.Security.Principal;
using System.Collections.Generic;
namespace Rapr
{
    public partial class Form1     
    {
        public enum OperationCode
        {
            EnumerateStore, AddDriver, AddInstallDriver, DeleteDriver, ForceDeleteDriver,Dummy
        };
        public enum Status
        {
            Success, Error, Warning,Normal
        }
        public struct OperationContext
        {
            public OperationCode code;
            public string infPath;  // Addition => Full path of the INF file, Others => INF filename in driverstore
            public object resultData;
            public object resultStatus;

            public DriverStoreEntry dse;
            public List<DriverStoreEntry> ldse;
            public bool IsCollectionPassed;
        };

        private void CleanupContext(OperationContext context)
        {
            context.code = OperationCode.Dummy;
            context.infPath = "";
            context.resultStatus = context.resultData = null;

            context.IsCollectionPassed = false;
            context.ldse = null;
        }

        private void PopulateUIWithDriverStoreEntries()
        {
            if (!(backgroundWorker1.IsBusy))
            {
                CleanupContext(context);
                this.lstDriverStoreEntries.ClearObjects();
                context.code = OperationCode.EnumerateStore;
                backgroundWorker1.RunWorkerAsync(context);
                ShowOperationInProgress(true);
                //ShowStatus("Enumerating driver store...");
            }
            else
            {
                MessageBox.Show("Another operation in progress");
                ShowStatus("Ready");
            }
        }

        private void AddDriverPackage(string infName )
        {
            if (!(backgroundWorker1.IsBusy))
            {
                CleanupContext(context);
                context.code = cbAddInstall.Checked == true ? OperationCode.AddInstallDriver: OperationCode.AddDriver;
                context.infPath = infName;         

                backgroundWorker1.RunWorkerAsync(context);

                ShowOperationInProgress(true);
                ShowStatus("Adding driver package...");
            }
            else
            {
                MessageBox.Show("Another operation in progress");
                ShowStatus("Ready");
            }
        }

        private void DeleteDriverPackage(DriverStoreEntry dse)
        {
            if (!(backgroundWorker1.IsBusy))
            {
                CleanupContext(context);
                context.code = cbForceDeletion.Checked == true ? OperationCode.ForceDeleteDriver : OperationCode.DeleteDriver;
                context.dse = dse;

                context.IsCollectionPassed = false;
                context.ldse = null;

                backgroundWorker1.RunWorkerAsync(context);

                ShowOperationInProgress(true);
                ShowStatus("Deleting driver package...");
            }
            else
            {
                MessageBox.Show("Another operation in progress");
                ShowStatus("Ready");
            }
        }

        private void DeleteDriverPackages(List<DriverStoreEntry> ldse)
        {
            if (!(backgroundWorker1.IsBusy))
            {
                CleanupContext(context);
                context.code = cbForceDeletion.Checked == true ? OperationCode.ForceDeleteDriver : OperationCode.DeleteDriver;
                context.IsCollectionPassed = true;
                context.ldse = ldse;;
                
                backgroundWorker1.RunWorkerAsync(context);                
                ShowOperationInProgress(true);
                ShowStatus("Deleting driver packages...");
            }
            else
            {
                MessageBox.Show("Another operation in progress");
                ShowStatus("Ready");
            }
        }
        // true = show wait_form
        // false = hide wait_form
        private void ShowOperationInProgress(bool state)
        {
            if (state == true)
            {
                progressPane.Visible = true;
            }
            else
            {
                progressPane.Visible = false;
            }
        }
        
        private void ShowStatus(string text)
        {
            ShowStatus(text, Status.Normal);
        }

        private void ShowStatus(string text, Status status)
        {
            lblStatus.Text = text;
            switch (status)
            {
                case Status.Error:
                    lblStatus.BackColor = Color.FromArgb(0xFF, 0x00, 0x33);
                    lblStatus.ForeColor = Color.White;
                    AppContext.TraceError(String.Format("[Error] {0}{1}", text, Environment.NewLine));
                    break;
                case Status.Success:
                    lblStatus.BackColor = Color.LightGreen;
                    lblStatus.ForeColor = Color.Black;
                    AppContext.TraceInformation(String.Format("[Success] {0}{1}", text, Environment.NewLine));
                    break;
                case Status.Warning:
                    lblStatus.BackColor = Color.Yellow;
                    lblStatus.ForeColor = Color.Black;
                    AppContext.TraceWarning(String.Format("[Warning] {0}{1}", text, Environment.NewLine));
                    break;
                case Status.Normal:
                    lblStatus.BackColor = SavedBackColor;
                    lblStatus.ForeColor = SavedForeColor;
                    AppContext.TraceInformation(String.Format("[Info] {0}{1}", text, Environment.NewLine));
                    break;               
            }            
        }

        static bool IsAnAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
