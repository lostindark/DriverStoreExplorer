using System;
using System.IO;
using System.Reflection;
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
            Application.Run(new DSEForm());
        }
    }
}
