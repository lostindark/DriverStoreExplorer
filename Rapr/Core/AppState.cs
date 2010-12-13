using System;
using System.Windows.Forms;
using Utils;
using System.Diagnostics;
using System.IO;
namespace Rapr
{
    public static class AppState
    {
        static Stream logFile;
        public static Form MainForm { get; set; }
        public static IDriverStore GetDriverStoreHandler()
        {
            return new PNPUtil();    
        }
        public static bool IsModeReadOnly()
        {
            return false;
        }

        public static bool IsOSSupported()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;

            //Get version information about the os.
            Version version = os.Version;

            return ((os.Platform == PlatformID.Win32NT) && (version.Major >= 6));
        }

        public static void EnableFileLogging()
        {
            // Create a file for output named TestFile.txt.
            logFile = File.Open("debug.log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            /* Create a new text writer using the output stream, and add it to
             * the trace listeners. */

            TextWriterTraceListener myTextListener = new TextWriterTraceListener(logFile);
            Trace.Listeners.Add(myTextListener);

            // Write output to the file.
            Trace.WriteLine(("---------------------------------------------------------------"));
            Trace.WriteLine(String.Format("{0} - started {1} {2}", Application.ProductName, DateTime.Now.ToLongDateString(),
                DateTime.Now.ToLongTimeString()));
        }

        internal static void FlushTrace()
        {
            logFile.Flush();
        }

        internal static void Cleanup()
        {
            logFile.Flush();
            logFile.Close();
            logFile.Dispose();
        }
    }
}
