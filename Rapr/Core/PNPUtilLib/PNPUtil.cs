using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Rapr;
namespace Utils
{

    public class PNPUtil : IDriverStore
    {
        enum PnpUtilOptions { Enumerate, Delete, ForceDelete, Add, AddInstall };
        #region IDriverStore Members
        public List<DriverStoreEntry> EnumeratePackages()
        {
            List<DriverStoreEntry> ldse = new List<DriverStoreEntry>();
            string output = ""; 

            bool result = PnpUtilHelper(PnpUtilOptions.Enumerate, "",  ref output);
            if (result == true)
            {                
                //Trace.TraceInformation("O/P of Enumeration : " + Environment.NewLine + output + Environment.NewLine);

                // Parse the output
                using (StringReader sr = new StringReader(output))                
                {
                    string currentLine = "";
                    DriverStoreEntry dse = new DriverStoreEntry();
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        if ((currentLine.Contains(@"Published name :")))
                        {
                            dse.driverPublishedName = currentLine.Split(new char[] { ':' })[1].Trim();
                            continue;
                        }

                        if ((currentLine.Contains(@"Driver package provider :")))
                        {
                            dse.driverPkgProvider = currentLine.Split(new char[] { ':' })[1].Trim();
                            continue;
                        }

                        if ((currentLine.Contains(@"Class :")))
                        {
                            dse.driverClass = currentLine.Split(new char[] { ':' })[1].Trim();
                            continue;
                        }

                        if ((currentLine.Contains(@"Driver date and version :")))
                        {
                            string DateAndVersion = currentLine.Split(new char[] { ':' })[1].Trim();
                            dse.driverDate = DateAndVersion.Split(new char[] { ' ' })[0].Trim();
                            dse.driverVersion = DateAndVersion.Split(new char[] { ' ' })[1].Trim();

                            continue;
                        }

                        if ((currentLine.Contains(@"Signer name :")))
                        {
                            dse.driverSignerName = currentLine.Split(new char[] { ':' })[1].Trim();

                            ldse.Add(dse);
                            dse = new DriverStoreEntry();

                            continue;
                        }
                    }
                }

            }
            return ldse;
        }

        public bool DeletePackage(DriverStoreEntry dse, bool forceDelete)
        {
            string dummy = "";
            return PnpUtilHelper(forceDelete == true ? PnpUtilOptions.ForceDelete : PnpUtilOptions.Delete,
                          dse.driverPublishedName, 
                          ref dummy);
        }

        public bool AddPackage(string infFullPath, bool install)
        {
            string dummy = "";
            return PnpUtilHelper(install == true ? PnpUtilOptions.AddInstall : PnpUtilOptions.Add,
                    infFullPath, ref dummy);
        }
        #endregion

        static bool PnpUtilHelper(PnpUtilOptions option, string infName, ref string output)
        {
            bool retVal = true;
            bool fDebugPrintOutput = false;
            //
            // Setup the process with the ProcessStartInfo class.
            //
            ProcessStartInfo start = new ProcessStartInfo
                                     {
                                         FileName = @"pnputil.exe" /* exe name.*/,
                                         UseShellExecute = false,
                                         RedirectStandardOutput = true,
                                         CreateNoWindow = true,
                                         WindowStyle = ProcessWindowStyle.Hidden
                                     };
            switch (option)
            {
                case PnpUtilOptions.Enumerate:
                    start.Arguments = @"-e";
                    break;
                case PnpUtilOptions.Delete:
                    start.Arguments = @"-d " + infName;
                    break;
                case PnpUtilOptions.ForceDelete:
                    start.Arguments = @"-f -d " + infName;
                    break;
                case PnpUtilOptions.Add:
                    fDebugPrintOutput = true;
                    start.WorkingDirectory = Path.GetDirectoryName(infName);
                    start.Arguments = @"-a " + Path.GetFileName(infName);
                    AppContext.TraceInformation(String.Format("[Add] workDir = {0}, arguments = {1}", start.WorkingDirectory,
                        start.Arguments));
                    break;
                case PnpUtilOptions.AddInstall:
                    fDebugPrintOutput = true;
                    start.WorkingDirectory = Path.GetDirectoryName(infName);
                    start.Arguments = @"-i -a " + Path.GetFileName(infName);
                    AppContext.TraceInformation(String.Format("[AddInstall] workDir = {0}, arguments = {1}", start.WorkingDirectory,
                        start.Arguments));

                    break;
            }

            //
            // Start the process.
            //
            string result = "";
            try
            {
                using (Process process = Process.Start(start))
                {
                    //
                    // Read in all the text from the process with the StreamReader.
                    //
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                        output = result;
                        if (fDebugPrintOutput == true)
                            AppContext.TraceInformation(String.Format("[Result_start] ---- {0}{1}[----- Result_End]{0}", Environment.NewLine, result));

                        if (option == PnpUtilOptions.Delete || option == PnpUtilOptions.ForceDelete)
                        {
                            if (output.Contains(@"Deleting the driver package failed"))
                            {
                                retVal = false;
                            }
                        }

                        if ((option == PnpUtilOptions.Add || option == PnpUtilOptions.AddInstall))
                        {
                            if (!output.Contains(@"Driver package added successfully"))
                            {
                                AppContext.TraceError("[Error] failed to add " + infName);
                                retVal = false;
                            }                            
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // dont catch all exceptions -- but will do for our needs!
                AppContext.TraceError(String.Format(@"{0}\n{1}" + Environment.NewLine, e.Message, e.StackTrace));
                output = "";
                retVal = false;
            }
            return retVal;
        }        
    }
}
