using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Principal;
using System.Windows.Forms;

using Microsoft.Win32;

namespace Rapr
{
    public static class DSEFormHelper
    {
        private const string AppCompatRegistry = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        private const string RunAsAdminRegistryValue = "RUNASADMIN";

        public static bool IsRunAsAdmin { get; } = IsRunAsAdministrator();

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
