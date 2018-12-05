using System;
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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveEventHandler;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DSEForm());
        }
    }
}
