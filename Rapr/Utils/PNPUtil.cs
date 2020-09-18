using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/*** 
 * Change log:
 * ------------------
 * Dec 30, 2012 : Merged in user submitted patch (ID: 10344) to support non-English builds. Thanks jenda__. 
 * 
 */
namespace Rapr.Utils
{
    public class PnpUtil : IDriverStore
    {
        private static readonly Regex AddResultRegex = new Regex(@".+: +([0-9]+)[\r\n].+: +([0-9]+)[\r\n ]+", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly char[] NameValueDelimiter = new char[] { ':' };

        public DriverStoreType Type => DriverStoreType.Online;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "False positive.")]
        public string OfflineStoreLocation => throw new NotSupportedException();

        public bool SupportAddInstall => true;

        public bool SupportForceDeletion => true;

        public bool SupportDeviceNameColumn => true;

        public bool SupportExportDriver => false;

        public bool SupportExportAllDrivers => false;

        public enum PnpUtilOption
        {
            Enumerate,
            Delete,
            ForceDelete,
            Add,
            AddInstall
        };

        public enum ParsingState
        {
            Header,
            PublishedName,
            PkgProvider,
            Class,
            DriverDateVersion,
            Signer,
        };

        #region IDriverStore Members
        public List<DriverStoreEntry> EnumeratePackages()
        {
            List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();
            string output = "";

            if (PnpUtilHelper(PnpUtilOption.Enumerate, null, ref output))
            {
                // Parse the output
                driverStoreEntries = ParsePnpUtilEnumerateResult(output);

                DriverStoreRepository repository = new DriverStoreRepository();

                List<DeviceDriverInfo> driverInfo = ConfigManager.GetDeviceDriverInfo();

                for (int i = 0; i < driverStoreEntries.Count; i++)
                {
                    DriverStoreEntry driverStoreEntry = driverStoreEntries[i];

                    if (!string.IsNullOrEmpty(driverStoreEntry.DriverPublishedName))
                    {
                        repository.FindInfInfo(
                            driverStoreEntry.DriverPublishedName,
                            out string driverInfName,
                            out string driverFolderLocation,
                            out long driverSize);

                        driverStoreEntry.DriverInfName = driverInfName;
                        driverStoreEntry.DriverFolderLocation = driverFolderLocation;
                        driverStoreEntry.DriverSize = driverSize;

                        var deviceInfo = driverInfo.OrderByDescending(d => d.IsPresent).FirstOrDefault(e => string.Equals(
                            Path.GetFileName(e.DriverInf),
                            driverStoreEntry.DriverPublishedName,
                            StringComparison.OrdinalIgnoreCase));

                        driverStoreEntry.DeviceName = deviceInfo?.DeviceName;
                        driverStoreEntry.DevicePresent = deviceInfo?.IsPresent;

                        driverStoreEntries[i] = driverStoreEntry;
                    }
                }
            }

            return driverStoreEntries;
        }

        public static List<DriverStoreEntry> ParsePnpUtilEnumerateResult(string pnpUtilOutput)
        {
            List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();

            using (StringReader sr = new StringReader(pnpUtilOutput))
            {
                DriverStoreEntry driverStoreEntry = new DriverStoreEntry();
                string currentLine;
                bool sawKey = false;
                ParsingState state = ParsingState.Header;

                while ((currentLine = sr.ReadLine()) != null)
                {
                    if (state == ParsingState.Header)
                    {
                        // We're expecting a header. Blank line marks the end of the header.
                        if (currentLine.Length == 0)
                        {
                            state = ParsingState.PublishedName;
                        }

                        continue;
                    }
                    else if (currentLine.Length == 0)
                    {
                        // Blank line means the end of an driver entry.
                        if (!driverStoreEntry.Equals(default(DriverStoreEntry)))
                        {
                            driverStoreEntries.Add(driverStoreEntry);
                            driverStoreEntry = new DriverStoreEntry();
                        }

                        // The next line should be driver OEM INF name.
                        state = ParsingState.PublishedName;
                        sawKey = false;
                        continue;
                    }
                    else
                    {
                        bool finishProcessCurrentLine = false;
                        string value = null;
                        bool hasKeyValueDelimiter = currentLine.IndexOf(':') != -1;

                        if (hasKeyValueDelimiter)
                        {
                            string[] currentLineDivided = currentLine.Split(NameValueDelimiter, 2);

                            if (currentLineDivided.Length > 1)
                            {
                                value = currentLineDivided[1].Trim();
                            }
                        }
                        else
                        {
                            value = currentLine.Trim();
                        }

                        if (state == ParsingState.PublishedName)
                        {
                            driverStoreEntry.DriverPublishedName = GetDriverPropAndUpdateParsingState(value, hasKeyValueDelimiter, ParsingState.PkgProvider, out finishProcessCurrentLine, ref sawKey, ref state);

                            if (finishProcessCurrentLine)
                            {
                                continue;
                            }
                        }

                        if (state == ParsingState.PkgProvider)
                        {
                            driverStoreEntry.DriverPkgProvider = GetDriverPropAndUpdateParsingState(value, hasKeyValueDelimiter, ParsingState.Class, out finishProcessCurrentLine, ref sawKey, ref state);

                            if (finishProcessCurrentLine)
                            {
                                continue;
                            }
                        }

                        if (state == ParsingState.Class)
                        {
                            driverStoreEntry.DriverClass = GetDriverPropAndUpdateParsingState(value, hasKeyValueDelimiter, ParsingState.DriverDateVersion, out finishProcessCurrentLine, ref sawKey, ref state);

                            if (finishProcessCurrentLine)
                            {
                                continue;
                            }
                        }

                        if (state == ParsingState.DriverDateVersion)
                        {
                            driverStoreEntry.SetDriverDateAndVersion(GetDriverPropAndUpdateParsingState(value, hasKeyValueDelimiter, ParsingState.Signer, out finishProcessCurrentLine, ref sawKey, ref state));

                            if (finishProcessCurrentLine)
                            {
                                continue;
                            }
                        }

                        if (state == ParsingState.Signer)
                        {
                            driverStoreEntry.DriverSignerName = GetDriverPropAndUpdateParsingState(value, hasKeyValueDelimiter, ParsingState.PublishedName, out finishProcessCurrentLine, ref sawKey, ref state);

                            if (finishProcessCurrentLine)
                            {
                                continue;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(driverStoreEntry.DriverPublishedName))
                {
                    driverStoreEntries.Add(driverStoreEntry);
                }
            }

            return driverStoreEntries;
        }

        private static string GetDriverPropAndUpdateParsingState(string value, bool hasKeyValueDelimiter, ParsingState nextState, out bool finishProcessCurrentLine, ref bool sawKey, ref ParsingState state)
        {
            string entryValue = null;
            finishProcessCurrentLine = true;

            if (!sawKey)
            {
                if (hasKeyValueDelimiter)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        // We got a valid value for current driver property.
                        entryValue = value;

                        // Move to next state
                        state = nextState;
                        sawKey = false;
                    }
                    else
                    {
                        // We don't have the driver property value yet, the value may be in the next line.
                        sawKey = true;
                    }
                }
                else
                {
                    // Extra lines we don't care.
                }
            }
            else
            {
                if (hasKeyValueDelimiter)
                {
                    // The driver property we've already saw doesn't have an value, and now we found a new property,
                    // move to next state and let code for that property to handle it.
                    finishProcessCurrentLine = false;
                }
                else
                {
                    // In last line we had driver property key, but didn't have the value. This line is the value.
                    entryValue = value;
                }

                state = nextState;
                sawKey = false;
            }

            return entryValue;
        }

        public bool DeleteDriver(DriverStoreEntry driverStoreEntry, bool forceDelete)
        {
            if (driverStoreEntry == null)
            {
                throw new ArgumentNullException(nameof(driverStoreEntry));
            }

            string dummy = "";
            return PnpUtilHelper(
                forceDelete ? PnpUtilOption.ForceDelete : PnpUtilOption.Delete,
                driverStoreEntry.DriverPublishedName,
                ref dummy);
        }

        public bool AddDriver(string infFullPath, bool install)
        {
            string dummy = "";
            return PnpUtilHelper(
                install ? PnpUtilOption.AddInstall : PnpUtilOption.Add,
                infFullPath,
                ref dummy);
        }
        #endregion

        private static bool PnpUtilHelper(PnpUtilOption option, string infName, ref string output)
        {
            bool retVal = true;
            bool fDebugPrintOutput = false;
            //
            // Setup the process with the ProcessStartInfo class.
            //
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "pnputil.exe" /* exe name.*/,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            switch (option)
            {
                //
                // [jenda_] I also had problems with some arguments starting "-". "/" works fine
                //
                case PnpUtilOption.Enumerate:
                    start.Arguments = "/e";
                    break;

                case PnpUtilOption.Delete:
                    start.Arguments = "/d " + infName;
                    break;

                case PnpUtilOption.ForceDelete:
                    start.Arguments = "/f /d " + infName;
                    break;

                case PnpUtilOption.Add:
                    fDebugPrintOutput = true;
                    start.WorkingDirectory = Path.GetDirectoryName(infName);
                    start.Arguments = "/a " + Path.GetFileName(infName);
                    Trace.TraceInformation($"[Add] workDir = {start.WorkingDirectory}, arguments = {start.Arguments}");
                    break;

                case PnpUtilOption.AddInstall:
                    fDebugPrintOutput = true;
                    start.WorkingDirectory = Path.GetDirectoryName(infName);
                    start.Arguments = "/i /a " + Path.GetFileName(infName);
                    Trace.TraceInformation($"[AddInstall] workDir = {start.WorkingDirectory}, arguments = {start.Arguments}");
                    break;
            }

            //
            // Start the process.
            //
            try
            {
                using (Process process = Process.Start(start))
                {
                    //
                    // Read in all the text from the process with the StreamReader.
                    //
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        output = result;
                        if (fDebugPrintOutput)
                        {
                            Trace.TraceInformation($"[Result_start] ---- {Environment.NewLine}{result}[----- Result_End]");
                        }

                        if (option == PnpUtilOption.Delete || option == PnpUtilOption.ForceDelete)
                        {
                            // [jenda_] Really don't know, how to recognize error without language-specific string recognition :(
                            // [jenda_] But those errors should contain ":"
                            if (output.Contains(":"))     //"Deleting the driver package failed"
                            {
                                retVal = false;
                            }
                        }

                        if (option == PnpUtilOption.Add || option == PnpUtilOption.AddInstall)
                        {
                            /* [jenda_]
                             This regex should recognize (~) this pattern:
                             * MS PnP blah blah
                             * 
                             * blah blah blah
                             * blah blah (...)
                             * 
                             * blah blah:    *number*
                             * blah blah blah:    *number*
                             * 
                             */
                            Match matchResult = AddResultRegex.Match(output);

                            if (matchResult.Success)    // [jenda_] regex recognized successfully
                            {
                                // [jenda_] if trying to add "0" packages or if number packages and number added packages differs
                                if (matchResult.Groups[1].Value == "0" || matchResult.Groups[1].Value != matchResult.Groups[2].Value)
                                {
                                    Trace.TraceError($"Failed to add {infName}");
                                    retVal = false;
                                }
                            }
                            else
                            {
                                Trace.TraceError($"Unknown response while trying to add {infName}");
                                retVal = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (
            ex is Win32Exception
            || ex is IOException
            || ex is InvalidOperationException)
            {
                // dont catch all exceptions -- but will do for our needs!
                Trace.TraceError(ex.ToString());
                output = "";
                retVal = false;
            }

            return retVal;
        }

        public bool ExportDriver(string infName, string destinationPath) => throw new NotSupportedException();

        public bool ExportAllDrivers(string destinationPath) => throw new NotSupportedException();
    }
}
