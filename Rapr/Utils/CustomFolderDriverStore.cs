using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Rapr.Utils
{
    /// <summary>
    /// A driver store implementation for managing driver packages in a custom folder.
    /// This allows users to maintain a driver library for offline Windows installation.
    /// </summary>
    public class CustomFolderDriverStore : IDriverStore
    {
        private readonly string folderPath;

        public DriverStoreType Type => DriverStoreType.CustomFolder;

        public string OfflineStoreLocation => this.folderPath;

        public bool SupportAddInstall => true;

        public bool SupportForceDeletion => true;

        public bool SupportDeviceNameColumn => true;

        public bool SupportExportDriver => true;

        public bool SupportExportAllDrivers => true;

        public CustomFolderDriverStore(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));
            }

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The folder path does not exist: {folderPath}");
            }

            this.folderPath = folderPath;
        }

        public List<DriverStoreEntry> EnumeratePackages()
        {
            var drivers = new List<DriverStoreEntry>();

            try
            {
                // Find all .inf files in the folder and subfolders
                var infFiles = Directory.EnumerateFiles(this.folderPath, "*.inf", SearchOption.AllDirectories);

                foreach (var infFile in infFiles)
                {
                    try
                    {
                        var entry = ParseDriverPackage(infFile);
                        if (entry != null)
                        {
                            drivers.Add(entry);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning($"Failed to parse driver file {infFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error enumerating driver packages: {ex.Message}");
                throw;
            }

            return drivers;
        }

        public bool DeleteDriver(DriverStoreEntry driverStoreEntry, bool forceDelete)
        {
            try
            {
                string folderPath = driverStoreEntry.DriverFolderLocation;

                if (!Directory.Exists(folderPath))
                {
                    Trace.TraceWarning($"Driver folder not found: {folderPath}");
                    return false;
                }

                // Delete the driver folder
                Directory.Delete(folderPath, recursive: true);
                Trace.TraceInformation($"Deleted driver folder: {folderPath}");
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to delete driver: {ex.Message}");
                return false;
            }
        }

        public bool AddDriver(string infFullPath, bool install)
        {
            try
            {
                if (!File.Exists(infFullPath))
                {
                    Trace.TraceError($"INF file not found: {infFullPath}");
                    return false;
                }

                // Read the INF to get the driver name
                var infFileName = Path.GetFileNameWithoutExtension(infFullPath);
                var driverFolderName = $"{infFileName}_custom";
                var targetFolderPath = Path.Combine(this.folderPath, driverFolderName);

                // Create target folder
                if (!Directory.Exists(targetFolderPath))
                {
                    Directory.CreateDirectory(targetFolderPath);
                }

                // Copy the INF file
                File.Copy(infFullPath, Path.Combine(targetFolderPath, Path.GetFileName(infFullPath)), overwrite: true);

                // Copy all related files (cat, sys, dll, etc.) from the same directory
                var sourceDir = Path.GetDirectoryName(infFullPath);
                var infBaseName = Path.GetFileNameWithoutExtension(infFullPath);

                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    var fileName = Path.GetFileName(file);
                    var fileBaseName = Path.GetFileNameWithoutExtension(file);

                    // Copy files related to this driver
                    if (fileBaseName.Equals(infBaseName, StringComparison.OrdinalIgnoreCase) ||
                        fileName.EndsWith(".cat", StringComparison.OrdinalIgnoreCase) ||
                        fileName.EndsWith(".sys", StringComparison.OrdinalIgnoreCase) ||
                        fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        var targetFile = Path.Combine(targetFolderPath, fileName);
                        File.Copy(file, targetFile, overwrite: true);
                    }
                }

                Trace.TraceInformation($"Added driver package: {infFileName} to {targetFolderPath}");
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to add driver: {ex.Message}");
                return false;
            }
        }

        public bool ExportDriver(DriverStoreEntry driverStoreEntry, string destinationPath)
        {
            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                string sourceFolderPath = driverStoreEntry.DriverFolderLocation;
                string exportFolderName = driverStoreEntry.DriverFolderName;
                string targetFolderPath = Path.Combine(destinationPath, exportFolderName);

                // Create the target folder
                if (Directory.Exists(targetFolderPath))
                {
                    Directory.Delete(targetFolderPath, recursive: true);
                }

                Directory.CreateDirectory(targetFolderPath);

                // Copy all files from the driver folder
                foreach (var file in Directory.GetFiles(sourceFolderPath))
                {
                    var fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(targetFolderPath, fileName), overwrite: true);
                }

                Trace.TraceInformation($"Exported driver to: {targetFolderPath}");
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to export driver: {ex.Message}");
                return false;
            }
        }

        public bool ExportAllDrivers(string destinationPath)
        {
            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                // Copy all driver folders from the custom folder
                foreach (var driverFolder in Directory.GetDirectories(this.folderPath))
                {
                    var folderName = Path.GetFileName(driverFolder);
                    var targetFolderPath = Path.Combine(destinationPath, folderName);

                    // Create or overwrite the target folder
                    if (Directory.Exists(targetFolderPath))
                    {
                        Directory.Delete(targetFolderPath, recursive: true);
                    }

                    Directory.CreateDirectory(targetFolderPath);

                    // Copy all files
                    foreach (var file in Directory.GetFiles(driverFolder))
                    {
                        var fileName = Path.GetFileName(file);
                        File.Copy(file, Path.Combine(targetFolderPath, fileName), overwrite: true);
                    }
                }

                Trace.TraceInformation($"Exported all drivers to: {destinationPath}");
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to export all drivers: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Parses a driver INF file and returns a DriverStoreEntry.
        /// </summary>
        private static DriverStoreEntry ParseDriverPackage(string infFilePath)
        {
            try
            {
                var folderPath = Path.GetDirectoryName(infFilePath);
                var infFileName = Path.GetFileName(infFilePath);

                // Read all text from INF file
                var infContent = File.ReadAllText(infFilePath);

                // 1. Parse DriverVer (Handles: DriverVer = MM/DD/YYYY,VERSION)
                Version driverVersion = new Version("0.0.0.0");
                DateTime driverDate = File.GetLastWriteTime(infFilePath);

                var driverVerMatch = System.Text.RegularExpressions.Regex.Match(
                    infContent, 
                    @"DriverVer\s*=\s*([^,\r\n]+)\s*,\s*([\d.]+)", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (driverVerMatch.Success)
                {
                    if (DateTime.TryParse(driverVerMatch.Groups[1].Value.Trim(), out var parsedDate))
                    {
                        driverDate = parsedDate;
                    }
                    if (Version.TryParse(driverVerMatch.Groups[2].Value.Trim(), out var parsedVersion))
                    {
                        driverVersion = parsedVersion;
                    }
                }

                // 2. Parse Class (Handles: Class = Extension)
                string driverClass = "Unknown";
                var classMatch = System.Text.RegularExpressions.Regex.Match(
                    infContent, 
                    @"^Class\s*=\s*([^\r\n;]+)", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
                
                if (classMatch.Success)
                {
                    driverClass = classMatch.Groups[1].Value.Trim().Trim('"');
                }

                // 3. Parse Provider (Handles: Provider = %Provider%)
                string providerRaw = "Custom Folder";
                var providerMatch = System.Text.RegularExpressions.Regex.Match(
                    infContent, 
                    @"^Provider\s*=\s*([^\r\n;]+)", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
                
                if (providerMatch.Success)
                {
                    providerRaw = providerMatch.Groups[1].Value.Trim().Trim('"');
                }

                // 4. Robust Recursive Resolution Layer targeting the [Strings] block specifically
                int safetyCounter = 0;
                while (providerRaw.StartsWith("%") && providerRaw.EndsWith("%") && safetyCounter < 5)
                {
                    safetyCounter++;
                    string tokenName = providerRaw.Replace("%", "").Trim();
                    
                    // Look for the start of the [Strings] block to isolate lookup context
                    int stringsIdx = infContent.IndexOf("[Strings]", StringComparison.OrdinalIgnoreCase);
                    string contextSearchArea = stringsIdx != -1 ? infContent.Substring(stringsIdx) : infContent;

                    // Match tokenName = "Value" strictly within the designated strings area
                    var stringTokenMatch = System.Text.RegularExpressions.Regex.Match(
                        contextSearchArea, 
                        $@"^{System.Text.RegularExpressions.Regex.Escape(tokenName)}\s*=\s*([^\r\n;]+)", 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
                    
                    if (stringTokenMatch.Success)
                    {
                        providerRaw = stringTokenMatch.Groups[1].Value.Trim().Trim('"');
                    }
                    else
                    {
                        // Fallback: If token cannot be resolved from the strings context, 
                        // strip the % marks and use the raw token name (e.g. "Provider") as the provider identifier
                        providerRaw = tokenName;
                        break;
                    }
                }

                var entry = new DriverStoreEntry
                {
                    DriverInfName = infFileName,
                    DriverPublishedName = infFileName,
                    DriverFolderLocation = folderPath,
                    DriverVersion = driverVersion,
                    DriverDate = driverDate,
                    DriverSize = CalculateFolderSize(folderPath),
                    DriverClass = driverClass,
                    DriverPkgProvider = providerRaw,
                    DriverSignerName = "Custom Folder Repo",
                    BootCritical = false,
                    DevicePresent = false,
                    DeviceName = string.Empty,
                    DeviceId = string.Empty,
                    DriverExtensionId = Guid.Empty
                };

                return entry;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error parsing driver package {infFilePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Calculates the total size of files in a folder.
        /// </summary>
        private static long CalculateFolderSize(string folderPath)
        {
            try
            {
                return Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                    .Sum(f => new FileInfo(f).Length);
            }
            catch
            {
                return 0;
            }
        }
    }
}
