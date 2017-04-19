using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Rapr.Core
{
    public class TextFileTraceListener : TextWriterTraceListener
    {
        public static string LastTraceFile { get; private set; }

        public TextFileTraceListener(string fileName)
            : base(ExpandFilePathInfo(fileName))
        {
        }

        private static string ExpandFilePathInfo(string fileName)
        {
            string filePath = Environment.ExpandEnvironmentVariables(fileName);

            new FileInfo(filePath).Directory.Create();

            LastTraceFile = filePath;

            return filePath;
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (this.Filter != null && !this.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                return;
            }

            this.Write($"{DateTime.UtcNow:u} [{eventType}]: ");

            if (args != null)
            {
                base.WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
            }
            else
            {
                base.WriteLine(format);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            this.TraceEvent(eventCache, source, eventType, id, message, null);
        }

        public override void WriteLine(string str)
        {
            base.WriteLine($"{DateTime.UtcNow:u}: {str}");
        }
    }
}
