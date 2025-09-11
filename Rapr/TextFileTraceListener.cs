using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Text;
using Rapr.Properties;

namespace Rapr
{
    public class TextFileTraceListener : TextWriterTraceListener
    {
        private const string DefaultLogFile = "Rapr.log";

        public static string LastTraceFile { get; private set; }

        public TextFileTraceListener()
            : base(CreateLogStreamWithCleanup())
        {
        }

        private static Stream CreateLogStreamWithCleanup()
        {
            LastTraceFile = Path.Combine(DSEFormHelper.GetApplicationFolder(), DefaultLogFile);

            try
            {
                // Clean up old entries before opening the file for writing
                CleanupOldLogEntries();
                return CreateLogFileStream();
            }
            catch (IOException)
            {
                string fallbackLogDirectory = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);
                Directory.CreateDirectory(fallbackLogDirectory);

                LastTraceFile = Path.Combine(fallbackLogDirectory, DefaultLogFile);

                // Try cleanup for the fallback location too
                CleanupOldLogEntries();
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

        /// <summary>
        /// Cleans up log entries older than the configured retention period from the current log file.
        /// Uses a two-phase approach: check if cleanup is needed (entries older than cutoffDate exist),
        /// then remove entries older than cutoffDate + 1 day to avoid running cleanup multiple times per day.
        /// </summary>
        public static void CleanupOldLogEntries()
        {
            // Early exit conditions - no cleanup needed
            if (string.IsNullOrEmpty(LastTraceFile) || !File.Exists(LastTraceFile))
            {
                return;
            }

            TimeSpan retentionPeriod = Settings.Default.LogRetentionPeriod;
            if (retentionPeriod <= TimeSpan.Zero)
            {
                return; // No cleanup if retention is zero or negative (cleanup disabled)
            }

            try
            {
                // Read all log lines at once for processing
                var allLines = File.ReadAllLines(LastTraceFile);

                if (allLines.Length == 0)
                {
                    return; // Empty file, nothing to clean up
                }

                // Calculate cleanup boundaries
                DateTime cutoffDate = DateTime.UtcNow - retentionPeriod; // Trigger cleanup if entries older than this exist
                DateTime keepFromDate = cutoffDate.AddDays(1); // Actually remove entries older than this (cutoffDate + 1 day)

                bool isFirstLogLine = true; // Flag to track if we're examining the first (oldest) log entry
                int firstLineToKeep = -1; // Index of the first line that should be retained

                // Single-pass loop with integrated fast path optimization
                for (int i = 0; i < allLines.Length; i++)
                {
                    if (TryParseLogEntryDate(allLines[i], out DateTime logEntryDate))
                    {
                        // Fast path optimization: check if the very first (oldest) entry is within retention period
                        if (isFirstLogLine && logEntryDate >= cutoffDate)
                        {
                            // If the oldest entry is newer than cutoffDate, then ALL entries are within retention period
                            // No cleanup needed - exit immediately for O(1) performance in the common case
                            break;
                        }
                        else
                        {
                            // We've processed the first log line, disable fast path check
                            isFirstLogLine = false;
                        }

                        // Find the first line that should be kept (entries >= keepFromDate)
                        // This implements the "remove cutoffDate + 1 day" logic to ensure cleanup runs only once per day
                        if (firstLineToKeep == -1 && logEntryDate >= keepFromDate)
                        {
                            firstLineToKeep = i;
                            break; // Found the boundary, no need to continue searching
                        }
                    }
                }

                // Perform cleanup based on findings
                // Note: if firstLineToKeep == 0, no cleanup is needed (all entries are within retention period)
                // Note: if firstLineToKeep == -1, it means we hit the fast path and exited early (no cleanup needed)
                if (firstLineToKeep > 0)
                {
                    // We found old entries to remove - keep only lines from firstLineToKeep onwards
                    var linesToKeep = new string[allLines.Length - firstLineToKeep];
                    Array.Copy(allLines, firstLineToKeep, linesToKeep, 0, linesToKeep.Length);
                    File.WriteAllLines(LastTraceFile, linesToKeep, Encoding.UTF8);
                }
            }
            catch (IOException)
            {
                // Ignore IO exceptions during cleanup to avoid interfering with application startup
                // This could happen if the file is locked by another process
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore access exceptions during cleanup to avoid interfering with application startup
                // This could happen if the process lacks write permissions to the log file
            }
        }

        /// <summary>
        /// Attempts to parse the date from a log entry line.
        /// </summary>
        /// <param name="logLine">The log line to parse</param>
        /// <param name="date">The parsed date if successful</param>
        /// <returns>True if the date was successfully parsed, false otherwise</returns>
        private static bool TryParseLogEntryDate(string logLine, out DateTime date)
        {
            date = default;

            if (string.IsNullOrEmpty(logLine) || logLine.Length < 20)
            {
                return false;
            }

            // The timestamp is always the first 20 characters in format "YYYY-MM-DD HH:mm:ssZ"
            string potentialDate = logLine.Substring(0, 20);
            if (potentialDate.EndsWith("Z"))
            {
                return DateTime.TryParseExact(potentialDate, "u",
                    CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out date);
            }

            return false;
        }
    }
}
