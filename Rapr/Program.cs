using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;

using Bluegrams.Application;

using Rapr.Lang;
using Rapr.Utils;

namespace Rapr
{
    public static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveEventHandler;
        }

        private static Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string resourceName = $"{assembly.EntryPoint.DeclaringType.Namespace}.{new AssemblyName(args.Name).Name}.dll";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }

                byte[] assemblyData = new byte[stream.Length];
                _ = stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }

        private static void AddEnvironmentPaths(string path)
        {
            var paths = new[] { Environment.GetEnvironmentVariable("PATH") ?? string.Empty, path };
            string newPath = string.Join(Path.PathSeparator.ToString(), paths);
            Environment.SetEnvironmentVariable("PATH", newPath);
        }

        private static void CleanUpOldConfig()
        {
            FileInfo fileInfo = new FileInfo(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);
            DirectoryInfo strongAssemblyConfigDirectory = fileInfo.Directory.Parent;

            // Remove old version of config.
            foreach (var dir in strongAssemblyConfigDirectory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                if (Version.TryParse(dir.Name, out Version version) && version < Assembly.GetExecutingAssembly().GetName().Version)
                {
                    dir.Delete(recursive: true);
                }
            }

            DirectoryInfo configDirectory = strongAssemblyConfigDirectory.Parent;

            // Remove configurations created by not strong signed app.
            foreach (var dir in configDirectory.EnumerateDirectories(Assembly.GetEntryAssembly().ManifestModule.Name + "_Url_*", SearchOption.TopDirectoryOnly))
            {
                dir.Delete(recursive: true);
            }
        }

        /// <summary>
        /// Migrates the legacy UseNativeDriverStore boolean setting to the new DriverStoreOption enum.
        /// </summary>
        private static void MigrateDriverStoreSettings()
        {
            // Check if we need to migrate from the old UseNativeDriverStore setting
            // We only migrate if DriverStoreOption is still at its default value (0 = Native)
            // and the old setting exists with a non-default value
            bool useNativeDriverStore = Properties.Settings.Default.UseNativeDriverStore;
            DriverStoreOption currentDriverStoreOption = (DriverStoreOption)Properties.Settings.Default.DriverStoreOption;

            // If DriverStoreOption is still at default, attempt migration
            if (currentDriverStoreOption == DriverStoreOption.Native)
            {
                DriverStoreOption newOption = (useNativeDriverStore && DSEFormHelper.IsNativeDriverStoreSupported)
                    ? DriverStoreOption.Native
                    : (DSEFormHelper.IsWin8OrNewer && DismUtil.IsDismAvailable)
                        ? DriverStoreOption.DISM
                        : DriverStoreOption.PnpUtil;

                Properties.Settings.Default.DriverStoreOption = (int)newOption;

                Trace.TraceInformation($"Migrated UseNativeDriverStore setting: {useNativeDriverStore} -> DriverStoreOption: {newOption}");
            }
            else
            {
                Trace.TraceInformation($"DriverStoreOption already configured: {currentDriverStoreOption}, skipping migration");
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Trace.AutoFlush = true;
            Trace.IndentSize = 4;
            Trace.Listeners.Add(new TextFileTraceListener());

            AddEnvironmentPaths(@"C:\Windows\System32\CompatTel");

            try
            {
                string settingFile = $"{Application.ProductName}.user.config";
                string applicationFolderPath = DSEFormHelper.GetApplicationFolder();

                try
                {
                    // Test if we can open filePath as Read/Write
                    using (FileStream fs = new FileStream(Path.Combine(applicationFolderPath, settingFile), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                    }

                    PortableSettingsProvider.SettingsFileName = settingFile;
                    PortableSettingsProvider.SettingsDirectory = applicationFolderPath;
                    PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);
                }
                catch (SecurityException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (IOException)
                {
                }

                if (Properties.Settings.Default.UpgradeRequired)
                {
                    try
                    {
                        // Upgrade settings from previous version
                        Properties.Settings.Default.Upgrade();

                        // Migrate the old UseNativeDriverStore boolean setting to the new DriverStoreOption enum
                        MigrateDriverStoreSettings();

                        // Mark upgrade as completed
                        Properties.Settings.Default.UpgradeRequired = false;
                        Properties.Settings.Default.Save();

                        Trace.TraceInformation("Settings migration completed successfully");
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't fail the application startup
                        Trace.TraceError($"Settings migration failed: {ex}");

                        // Still mark upgrade as completed to avoid repeated attempts
                        Properties.Settings.Default.UpgradeRequired = false;
                        Properties.Settings.Default.Save();
                    }

                    try
                    {
                        CleanUpOldConfig();
                    }
                    catch (Exception e) when (
                        e is SecurityException
                        || e is IOException)
                    {
                        Trace.TraceError(e.ToString());
                    }
                }

                using (DSEForm mainForm = new DSEForm())
                {
                    Application.Run(mainForm);
                }
            }
            catch (ConfigurationException e)
            {
                Trace.TraceError(e.ToString());
                MessageBox.Show(
                    e.Message,
                    Language.Product_Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
