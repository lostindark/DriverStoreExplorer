using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Rapr;

/*** 
 * Change log:
 * ------------------
 * Dec 30, 2012 : Merged in user submitted patch (ID: 10344) to support non-English builds. Thanks jenda__. 
 * 
 */
namespace Rapr.Utils
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
                // Trace.TraceInformation("O/P of Enumeration : " + Environment.NewLine + output + Environment.NewLine);

                // Parse the output
                // [jenda_] Didn't work on non-english Windows - changed from string recognition to counting lines
                using (StringReader sr = new StringReader(output))                
                {
                    string currentLine = "";
                    string[] currentLineDivided = { };
                    DriverStoreEntry dse = new DriverStoreEntry();
                    byte lineNum = 0;
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        currentLineDivided = currentLine.Split(new char[] { ':' });
                        if (currentLineDivided.Length == 2)
                        {
                            currentLineDivided[1] = currentLineDivided[1].Trim();
                            switch (lineNum)
                            {
                                case 0:     // [jenda_] Published name :
                                    dse.DriverPublishedName = currentLineDivided[1];
                                    break;
                                case 1:     //Driver package provider :
                                    dse.DriverPkgProvider = currentLine.Split(new char[] { ':' })[1].Trim();
                                    break;
                                case 2:     // [jenda_] Class :
                                    dse.DriverClass = currentLine.Split(new char[] { ':' })[1].Trim();
                                    break;
                                case 3:     // [jenda_] Driver date and version :
                                    string DateAndVersion = currentLine.Split(new char[] { ':' })[1].Trim();
                                    dse.DriverDate = DateTime.Parse(DateAndVersion.Split(new char[] { ' ' })[0].Trim());
                                    dse.DriverVersion = Version.Parse(DateAndVersion.Split(new char[] { ' ' })[1].Trim());
                                    break;
                                case 4:     // [jenda_] Signer name :
                                    dse.DriverSignerName = currentLine.Split(new char[] { ':' })[1].Trim();

                                    ldse.Add(dse);
                                    dse = new DriverStoreEntry();
                                    break;
                                default:
                                    continue;
                            }
                            lineNum++;
                            if (lineNum > 4)
                                lineNum = 0;

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
                          dse.DriverPublishedName, 
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
                //
                // [jenda_] I also had problems with some arguments starting "-". "/" works fine
                //
                case PnpUtilOptions.Enumerate:
                    start.Arguments = @"/e";
                    break;
                case PnpUtilOptions.Delete:
                    start.Arguments = @"/d " + infName;
                    break;
                case PnpUtilOptions.ForceDelete:
                    start.Arguments = @"/f /d " + infName;
                    break;
                case PnpUtilOptions.Add:
                    fDebugPrintOutput = true;
                    start.WorkingDirectory = Path.GetDirectoryName(infName);
                    start.Arguments = @"/a " + Path.GetFileName(infName);
                    AppContext.TraceInformation(String.Format("[Add] workDir = {0}, arguments = {1}", start.WorkingDirectory,
                        start.Arguments));
                    break;
                case PnpUtilOptions.AddInstall:
                    fDebugPrintOutput = true;
                    start.WorkingDirectory = Path.GetDirectoryName(infName);
                    start.Arguments = @"/i /a " + Path.GetFileName(infName);
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
                            // [jenda_] Really don't know, how to recognize error without language-specific string recognition :(
                            // [jenda_] But those errors should contain ":"
                            if (output.Contains(@":"))     //"Deleting the driver package failed"
                            {
                                retVal = false;
                            }
                        }

                        if ((option == PnpUtilOptions.Add || option == PnpUtilOptions.AddInstall))
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
                            System.Text.RegularExpressions.Match MatchResult = System.Text.RegularExpressions.Regex.Match(output, @".+: +([0-9]+)[\r\n].+: +([0-9]+)[\r\n ]+", System.Text.RegularExpressions.RegexOptions.Singleline);

                            if (MatchResult.Success)    // [jenda_] regex recognized successfully
                            {
                                // [jenda_] if trying to add "0" packages or if number packages and number added packages differs
                                if (MatchResult.Groups[1].Value == "0" || MatchResult.Groups[1].Value != MatchResult.Groups[2].Value)
                                {
                                    AppContext.TraceError("[Error] failed to add " + infName);
                                    retVal = false;
                                }
                            }
                            else
                            {
                                AppContext.TraceError("[Error] unknown response while trying to add " + infName);
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
