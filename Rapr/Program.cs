using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;

namespace Rapr
{
    public static class Program
    {
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
                stream.Read(assemblyData, 0, assemblyData.Length);
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
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            AddEnvironmentPaths(@"C:\Windows\System32\CompatTel");
            AppDomain.CurrentDomain.AssemblyResolve += ResolveEventHandler;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();

                try
                {
                    CleanUpOldConfig();
                }
                catch (Exception e) when (
                    e is SecurityException
                    || e is IOException)
                {
                }
            }

            using (DSEForm mainForm = new DSEForm())
            {
                Application.Run(mainForm);
            }
        }
    }
}
