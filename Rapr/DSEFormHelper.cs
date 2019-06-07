using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Principal;
using System.Windows.Forms;

using Microsoft.Win32;

using Rapr.Lang;
using Rapr.Utils;

namespace Rapr
{
    public partial class DSEForm
    {
        private const string AppCompatRegistry = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        private const string RunAsAdminRegistryValue = "RUNASADMIN";
        private static readonly bool isRunAsAdministrator = IsRunAsAdministrator();

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

        private class OperationContext
        {

            public OperationCode Code { get; set; }

            public string InfPath // Addition => Full path of the INF file, Others => INF filename in driverstore
            {
                get;
                set;
            }

            public object ResultData { get; set; }

            public bool ResultStatus { get; set; }

            public List<DriverStoreEntry> DriverStoreEntries { get; set; }
        };

        private static void CleanupContext(OperationContext context)
        {
            context.Code = OperationCode.Dummy;
            context.InfPath = "";
            context.ResultStatus = false;
            context.ResultData = null;
            context.DriverStoreEntries = null;
        }

        private void InProgress()
        {
            MessageBox.Show(this, Language.Message_Operation_In_Progress, Language.Message_Title_Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void PopulateUIWithDriverStoreEntries(bool updateStatus = false)
        {
            if (!this.backgroundWorker1.IsBusy)
            {
                CleanupContext(this.context);
                this.lstDriverStoreEntries.ClearObjects();
                this.context.Code = OperationCode.EnumerateStore;
                this.backgroundWorker1.RunWorkerAsync(this.context);

                if (updateStatus)
                {
                    this.ShowStatus(Language.Status_Label);
                }
            }
            else
            {
                this.InProgress();
            }
        }

        private void AddDriverPackage(string infName)
        {
            if (!this.backgroundWorker1.IsBusy)
            {
                CleanupContext(this.context);
                this.context.Code = this.cbAddInstall.Checked ? OperationCode.AddInstallDriver : OperationCode.AddDriver;
                this.context.InfPath = infName;

                this.backgroundWorker1.RunWorkerAsync(this.context);

                this.ShowStatus(Language.Status_Adding_Package);
            }
            else
            {
                this.InProgress();
            }
        }

        private void DeleteDriverPackages(List<DriverStoreEntry> ldse)
        {
            if (!this.backgroundWorker1.IsBusy)
            {
                CleanupContext(this.context);
                this.context.Code = this.cbForceDeletion.Checked ? OperationCode.ForceDeleteDriver : OperationCode.DeleteDriver;
                this.context.DriverStoreEntries = ldse;

                this.backgroundWorker1.RunWorkerAsync(this.context);
                this.ShowStatus(Language.Status_Deleting_Packages);
            }
            else
            {
                this.InProgress();
            }
        }

        private void ShowOperationInProgress(bool state)
        {
            this.toolStripProgressBar1.Visible = state;
        }

        private void ShowStatus(string text)
        {
            this.ShowStatus(Status.Normal, text);
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
                        MessageBox.Show(this, text, Language.Message_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    break;

                case Status.Success:
                    this.lblStatus.BackColor = Color.LightGreen;
                    this.lblStatus.ForeColor = Color.Black;
                    Trace.TraceInformation(detailToLog);

                    if (usePopup)
                    {
                        MessageBox.Show(this, text, Language.Product_Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    break;

                case Status.Warning:
                    this.lblStatus.BackColor = Color.Yellow;
                    this.lblStatus.ForeColor = Color.Black;
                    Trace.TraceWarning(detailToLog);

                    if (usePopup)
                    {
                        MessageBox.Show(this, text, Language.Message_Title_Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    break;

                case Status.Normal:
                    this.lblStatus.BackColor = this.savedBackColor;
                    this.lblStatus.ForeColor = this.savedForeColor;
                    Trace.TraceInformation(detailToLog);

                    if (usePopup)
                    {
                        MessageBox.Show(this, text, Language.Product_Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    break;
            }
        }

        private static bool IsRunAsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        public static bool IsOSSupported()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;

            //Get version information about the os.
            Version version = os.Version;

            return os.Platform == PlatformID.Win32NT && (version.Major >= 6);
        }

        public static void RunAsAdministrator()
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                Verb = "runas",
                FileName = Assembly.GetExecutingAssembly().Location
            };

            try
            {
                Process.Start(processInfo);
            }
            catch (Win32Exception ex)
            {
                // Ignore error 1223: The operation was canceled by the user.
                if (ex.NativeErrorCode == 1223)
                {
                    return;
                }

                throw;
            }

            Application.Exit();
        }

        public static bool RunAsAdmin
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(AppCompatRegistry))
                {
                    string valueStr = (string)key?.GetValue(Assembly.GetExecutingAssembly().Location);
                    return !string.IsNullOrEmpty(valueStr)
                        && valueStr.Split(' ').Any(s => s == RunAsAdminRegistryValue);
                }
            }

            set
            {
                using (var key = Registry.CurrentUser.CreateSubKey(AppCompatRegistry, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key != null)
                    {
                        List<string> values = new List<string> { "~" };
                        string keyStr = Assembly.GetExecutingAssembly().Location;
                        string valueStr = (string)key.GetValue(keyStr);

                        if (!string.IsNullOrEmpty(valueStr))
                        {
                            values = valueStr.Split(' ').ToList();
                        }

                        values.Remove(RunAsAdminRegistryValue);

                        if (value)
                        {
                            values.Add(RunAsAdminRegistryValue);
                        }

                        if (values.Count == 1 && values[0] == "~")
                        {
                            key.DeleteValue(keyStr);
                        }
                        else
                        {
                            key.SetValue(keyStr, string.Join(" ", values), RegistryValueKind.String);
                        }
                    }
                }
            }
        }
    }
}
