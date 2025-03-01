using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace Rapr
{
    public static class DSEFormHelper
    {
        private const string AppCompatRegistry = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        private const string RunAsAdminRegistryValue = "RUNASADMIN";
        private static readonly Version Win8Version = new Version(6, 2);

        public static string GetApplicationFolder()
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;

            if (!string.IsNullOrEmpty(assemblyLocation))
            {
                // Check if the file is a symbolic link
                FileInfo fileInfo = new FileInfo(assemblyLocation);

                if (fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    // Resolve the real path
                    assemblyLocation = GetFinalPathName(assemblyLocation);
                }
                return Path.GetDirectoryName(assemblyLocation);
            }
            else
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        private static string GetFinalPathName(string path)
        {
            const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
            const int OPEN_EXISTING = 3;

            using (SafeFileHandle handle = NativeMethods.CreateFile(path, 0, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero))
            {
                if (handle.IsInvalid)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                StringBuilder finalPath = new StringBuilder(512);
                int result = NativeMethods.GetFinalPathNameByHandle(handle, finalPath, finalPath.Capacity, 0);
                if (result < 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Remove the "\\?\" prefix
                const string PathPrefix = @"\\?\";
                string finalPathString = finalPath.ToString();

                if (finalPath.Length > PathPrefix.Length && finalPathString.StartsWith(PathPrefix))
                {
                    finalPathString = finalPathString.Substring(PathPrefix.Length);
                }

                return finalPathString;
            }
        }

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

        public static bool IsOSSupported =>
            Environment.OSVersion.Platform == PlatformID.Win32NT
            && (Environment.OSVersion.Version.Major >= 6);

        public static bool IsWin8OrNewer =>
            Environment.OSVersion.Platform == PlatformID.Win32NT
            && Environment.OSVersion.Version >= Win8Version;

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

        public static ICollection<CultureInfo> GetSupportedLanguage()
        {
            HashSet<CultureInfo> supportedLanguage = new HashSet<CultureInfo>
            {
                new CultureInfo("en")
            };

            Assembly assembly = Assembly.GetExecutingAssembly();
            var resoures = assembly.GetManifestResourceNames();

            string resourcePrefix = $"{assembly.EntryPoint.DeclaringType.Namespace}.";
            const string ResourceSuffix = ".resources.dll";

            foreach (var resource in resoures)
            {
                if (resource.StartsWith(resourcePrefix) && resource.EndsWith(ResourceSuffix))
                {
                    string cultureName = resource.Substring(resourcePrefix.Length, resource.Length - resourcePrefix.Length - ResourceSuffix.Length);
                    try
                    {
                        supportedLanguage.Add(new CultureInfo(cultureName));
                    }
                    catch (CultureNotFoundException)
                    {
                    }
                }
            }

            string currentFolder = Path.GetDirectoryName(assembly.Location);

            try
            {
                DirectoryInfo dir = new DirectoryInfo(currentFolder);

                foreach (var file in dir.EnumerateFiles($"{resourcePrefix}{ResourceSuffix}", SearchOption.AllDirectories))
                {
                    string folderName = file.Directory.Name;
                    try
                    {
                        supportedLanguage.Add(new CultureInfo(folderName));
                    }
                    catch (CultureNotFoundException)
                    {
                    }
                }
            }
            catch (SecurityException)
            {
            }

            return supportedLanguage;
        }

        internal static class NativeMethods
        {

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern SafeFileHandle CreateFile(
                string lpFileName,
                int dwDesiredAccess,
                int dwShareMode,
                IntPtr lpSecurityAttributes,
                int dwCreationDisposition,
                int dwFlagsAndAttributes,
                IntPtr hTemplateFile);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int GetFinalPathNameByHandle(
                SafeFileHandle hFile,
                StringBuilder lpszFilePath,
                int cchFilePath,
                int dwFlags);
        }
    }
}
