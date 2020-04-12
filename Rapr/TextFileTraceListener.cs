using System;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Rapr
{
    public class TextFileTraceListener : TextWriterTraceListener
    {
        private const string DefaultLogFile = "Rapr.log";

        public static string LastTraceFile { get; private set; }

        public TextFileTraceListener(string fileName)
            : base(ExpandFilePathInfo(fileName))
        {
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Base class handled dispose.")]
        public TextFileTraceListener()
            : base(CreateLogStream())
        {
        }

        private static string ExpandFilePathInfo(string fileName)
        {
            LastTraceFile = Environment.ExpandEnvironmentVariables(fileName);
            new FileInfo(LastTraceFile).Directory?.Create();
            return LastTraceFile;
        }

        private static Stream CreateLogStream()
        {
            LastTraceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultLogFile);

            try
            {
                return CreateLogFileStream();
            }
            catch (IOException)
            {
                LastTraceFile = Path.Combine(
                    Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath),
                    DefaultLogFile);

                return CreateLogFileStream();
            }
        }

        private static FileStream CreateLogFileStream() => File.Open(LastTraceFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (this.Filter?.ShouldTrace(eventCache, source, eventType, id, format, args, null, null) == false)
            {
                return;
            }

            this.Write($"{DateTime.UtcNow:u} [{eventType}]: ");

            if (args?.Length > 0)
            {
                base.WriteLine(string.Format(format, args));
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

        public override void WriteLine(string message)
        {
            base.WriteLine($"{DateTime.UtcNow:u}: {message}");
        }
    }
}
