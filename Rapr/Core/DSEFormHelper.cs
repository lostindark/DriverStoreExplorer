using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Principal;
using System.Windows.Forms;
using Rapr.Utils;

namespace Rapr
{
    public partial class DSEForm
    {
        public enum OperationCode
        {
            EnumerateStore,
            AddDriver,
            AddInstallDriver,
            DeleteDriver,
            ForceDeleteDriver,
            Dummy,
        };

        public enum Status
        {
            Success,
            Error,
            Warning,
            Normal,
        }

        public struct OperationContext
        {
            public OperationCode Code;
            public string InfPath;  // Addition => Full path of the INF file, Others => INF filename in driverstore
            public object ResultData;
            public bool ResultStatus;

            public List<DriverStoreEntry> DriverStoreEntries;
        };

        private void CleanupContext(OperationContext context)
        {
            context.Code = OperationCode.Dummy;
            context.InfPath = "";
            context.ResultStatus = false;
            context.ResultData = null;

            context.DriverStoreEntries = null;
        }

        private void PopulateUIWithDriverStoreEntries()
        {
            if (!(backgroundWorker1.IsBusy))
            {
                CleanupContext(context);
                this.lstDriverStoreEntries.ClearObjects();
                context.Code = OperationCode.EnumerateStore;
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

        private void AddDriverPackage(string infName)
        {
            if (!(backgroundWorker1.IsBusy))
            {
                CleanupContext(context);
                context.Code = cbAddInstall.Checked ? OperationCode.AddInstallDriver : OperationCode.AddDriver;
                context.InfPath = infName;

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

        private void DeleteDriverPackages(List<DriverStoreEntry> ldse)
        {
            if (!(backgroundWorker1.IsBusy))
            {
                CleanupContext(context);
                context.Code = cbForceDeletion.Checked ? OperationCode.ForceDeleteDriver : OperationCode.DeleteDriver;
                context.DriverStoreEntries = ldse;

                backgroundWorker1.RunWorkerAsync(context);
                ShowOperationInProgress(true);
                ShowStatus("Deleting driver package(s)...");
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
            toolStripProgressBar1.Visible = state;
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
