using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rapr.Utils
{
    /// <summary>
    /// Manages driver packages in a user-selected folder only.
    /// No system driver store, DISM, PnPUtil, or SetupAPI calls are made.
    /// </summary>
    public class CustomFolderDriverStore : IDriverStore
    {
        private readonly string folderPath;

        private static readonly Regex InfStringsLineRegex = new Regex(
            @"^([^;\r\n=#\[][^\r\n=]*?)\s*=\s*([^;\r\n;]+)",
            RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex InfModelLineRegex = new Regex(
            @"^\s*(?<desc>[^=\r\n;]+?)\s*=\s*[^,\r\n]+,\s*(?<hwid>(?:PCI|USB|ROOT|ACPI|HDAUDIO|SCSI|SWD|SWC|HID|UMBUS|MONITOR|DISPLAY|MF|1394|DOT4|MTD|COMPOSITE|BIOMETRIC|LPTENUM|PORTS|STORAGE|WSD|VRD|SENSOR|EFI|GPRS|NET|WPD|MR|FL|SD|DETECTED|SWC)\\[^\r\n;]*)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public DriverStoreType Type => DriverStoreType.CustomFolder;

        public string OfflineStoreLocation => this.folderPath;

        public bool SupportAddInstall => false;

        public bool SupportForceDeletion => false;

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

            this.folderPath = Path.GetFullPath(folderPath).TrimEnd('\\');
        }

        public List<DriverStoreEntry> EnumeratePackages()
        {
            var drivers = new List<DriverStoreEntry>();

            try
            {
                foreach (var infFile in this.EnumerateInfFiles(this.folderPath))
                {
                    try
                    {
                        var entry = this.ParseDriverPackage(infFile);
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
            _ = forceDelete;

            try
            {
                if (driverStoreEntry == null)
                {
                    return false;
                }

                string packageFolder = driverStoreEntry.DriverFolderLocation;
                if (string.IsNullOrEmpty(packageFolder))
                {
                    return false;
                }

                packageFolder = Path.GetFullPath(packageFolder);

                if (!this.IsPathUnderFolder(packageFolder, this.folderPath))
                {
                    Trace.TraceWarning($"Refusing to delete path outside custom folder: {packageFolder}");
                    return false;
                }

                if (!this.IsInfInPackageSubfolder(packageFolder))
                {
                    Trace.TraceWarning($"Refusing to delete root-level INF package: {driverStoreEntry.DriverInfName}");
                    return false;
                }

                if (!Directory.Exists(packageFolder))
                {
                    Trace.TraceWarning($"Driver folder not found: {packageFolder}");
                    return false;
                }

                Directory.Delete(packageFolder, recursive: true);
                Trace.TraceInformation($"Deleted driver folder: {packageFolder}");
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to delete driver: {ex.Message}");
                return false;
            }
        }

        public AddDriverResult AddDriver(string infFullPath, bool install)
        {
            _ = install;

            try
            {
                if (!File.Exists(infFullPath))
                {
                    Trace.TraceError($"INF file not found: {infFullPath}");
                    return AddDriverResult.Failed;
                }

                if (!infFullPath.EndsWith(".inf", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceError($"Not an INF file: {infFullPath}");
                    return AddDriverResult.Failed;
                }

                var incoming = this.ParseDriverPackage(infFullPath);
                if (incoming == null)
                {
                    Trace.TraceError($"Failed to parse driver package: {infFullPath}");
                    return AddDriverResult.Failed;
                }

                string sourceDir = Path.GetFullPath(Path.GetDirectoryName(infFullPath));

                if (this.IsPathUnderFolder(sourceDir, this.folderPath))
                {
                    if (this.IsCustomFolderRoot(sourceDir))
                    {
                        Trace.TraceError($"Driver INF must be in a subfolder of {this.folderPath}, not at the root");
                        return AddDriverResult.Failed;
                    }

                    Trace.TraceInformation($"Driver already in custom folder: {infFullPath}");
                    return AddDriverResult.Skipped;
                }

                var existingMatches = this.EnumeratePackages()
                    .Where(entry => HasSameDriverIdentity(entry, incoming))
                    .ToList();

                if (existingMatches.Any(entry => HasSameVersionAndDate(entry, incoming)))
                {
                    Trace.TraceInformation(
                        $"Skipping driver already in custom folder: {incoming.DriverInfName} ({incoming.DriverVersion}, {incoming.DriverDate:d})");
                    return AddDriverResult.Skipped;
                }

                var newestExisting = existingMatches
                    .OrderByDescending(entry => entry.DriverVersion)
                    .ThenByDescending(entry => entry.DriverDate)
                    .FirstOrDefault();

                if (newestExisting != null && CompareDriverVersion(incoming, newestExisting) <= 0)
                {
                    Trace.TraceInformation(
                        $"Skipping driver; newer or equal version already in custom folder: {incoming.DriverInfName}");
                    return AddDriverResult.Skipped;
                }

                string folderName = new DirectoryInfo(sourceDir).Name;
                string targetFolderPath = this.GetUniqueTargetFolderPath(folderName);

                this.CopyDirectory(sourceDir, targetFolderPath, overwrite: true);
                Trace.TraceInformation($"Added driver package from {sourceDir} to {targetFolderPath}");

                this.RemoveOlderDuplicatePackages(incoming.DriverInfName);

                return AddDriverResult.Added;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to add driver: {ex.Message}");
                return AddDriverResult.Failed;
            }
        }

        private static bool HasSameDriverIdentity(DriverStoreEntry a, DriverStoreEntry b)
        {
            return a.DriverClass.Equals(b.DriverClass, StringComparison.OrdinalIgnoreCase)
                && a.DriverExtensionId == b.DriverExtensionId
                && a.DriverPkgProvider.Equals(b.DriverPkgProvider, StringComparison.OrdinalIgnoreCase)
                && a.DriverInfName.Equals(b.DriverInfName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasSameVersionAndDate(DriverStoreEntry a, DriverStoreEntry b)
        {
            return a.DriverVersion == b.DriverVersion && a.DriverDate.Date == b.DriverDate.Date;
        }

        private static int CompareDriverVersion(DriverStoreEntry a, DriverStoreEntry b)
        {
            int versionCompare = a.DriverVersion.CompareTo(b.DriverVersion);
            if (versionCompare != 0)
            {
                return versionCompare;
            }

            return a.DriverDate.CompareTo(b.DriverDate);
        }

        /// <summary>
        /// Removes superseded packages in the custom folder, keeping the newest version/date per driver identity.
        /// Uses the same grouping rules as Select Old Drivers in the main UI.
        /// </summary>
        private void RemoveOlderDuplicatePackages(string infNameFilter = null)
        {
            var entries = this.EnumeratePackages();

            if (!string.IsNullOrEmpty(infNameFilter))
            {
                entries = entries
                    .Where(entry => entry.DriverInfName.Equals(infNameFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var driverGroups = entries
                .Where(entry => !entry.DriverInfName.Equals("ntprint.inf", StringComparison.OrdinalIgnoreCase))
                .GroupBy(entry => new { entry.DriverClass, entry.DriverExtensionId, entry.DriverPkgProvider, entry.DriverInfName })
                .Select(drivers => drivers
                    .GroupBy(entry => new { entry.DriverVersion, entry.DriverDate })
                    .OrderByDescending(g => g.Key.DriverVersion)
                    .ThenByDescending(g => g.Key.DriverDate)
                    .ToList())
                .Where(groups => groups.Count > 1);

            foreach (var versionGroups in driverGroups)
            {
                foreach (var oldGroup in versionGroups.Skip(1))
                {
                    foreach (var entry in oldGroup)
                    {
                        this.DeleteDriver(entry, forceDelete: false);
                    }
                }
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
                if (string.IsNullOrEmpty(sourceFolderPath) ||
                    !this.IsPathUnderFolder(sourceFolderPath, this.folderPath) ||
                    !this.IsInfInPackageSubfolder(sourceFolderPath))
                {
                    return false;
                }

                string exportFolderName = driverStoreEntry.DriverFolderName;
                string targetFolderPath = Path.Combine(destinationPath, exportFolderName);

                if (Directory.Exists(targetFolderPath))
                {
                    Directory.Delete(targetFolderPath, recursive: true);
                }

                this.CopyDirectory(sourceFolderPath, targetFolderPath, overwrite: true);

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

                foreach (var driverFolder in Directory.GetDirectories(this.folderPath))
                {
                    var folderName = Path.GetFileName(driverFolder);
                    var targetFolderPath = Path.Combine(destinationPath, folderName);

                    if (Directory.Exists(targetFolderPath))
                    {
                        Directory.Delete(targetFolderPath, recursive: true);
                    }

                    this.CopyDirectory(driverFolder, targetFolderPath, overwrite: true);
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

        private IEnumerable<string> EnumerateInfFiles(string rootPath)
        {
            foreach (var file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
            {
                if (!string.Equals(Path.GetExtension(file), ".inf", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string infDir = Path.GetFullPath(Path.GetDirectoryName(file)).TrimEnd('\\');
                if (!this.IsInfInPackageSubfolder(infDir))
                {
                    Trace.TraceWarning($"Skipping root-level INF (must be in a subfolder): {file}");
                    continue;
                }

                yield return file;
            }
        }

        private bool IsInfInPackageSubfolder(string packageFolderPath)
        {
            return !this.IsCustomFolderRoot(packageFolderPath);
        }

        private bool IsCustomFolderRoot(string path)
        {
            return string.Equals(
                Path.GetFullPath(path).TrimEnd('\\'),
                this.folderPath,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPathUnderFolder(string path, string rootFolder)
        {
            string fullPath = Path.GetFullPath(path).TrimEnd('\\') + Path.DirectorySeparatorChar;
            string fullRoot = Path.GetFullPath(rootFolder).TrimEnd('\\') + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }

        private string GetUniqueTargetFolderPath(string folderName)
        {
            string targetFolderPath = Path.Combine(this.folderPath, folderName);
            if (!Directory.Exists(targetFolderPath))
            {
                return targetFolderPath;
            }

            for (int i = 2; i < 1000; i++)
            {
                targetFolderPath = Path.Combine(this.folderPath, $"{folderName}_{i}");
                if (!Directory.Exists(targetFolderPath))
                {
                    return targetFolderPath;
                }
            }

            throw new IOException($"Could not find unused folder name for {folderName}");
        }

        private void CopyDirectory(string sourceDir, string destDir, bool overwrite)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                this.CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)), overwrite);
            }
        }

        private DriverStoreEntry ParseDriverPackage(string infFilePath)
        {
            try
            {
                var folderPath = Path.GetDirectoryName(infFilePath);
                var infFileName = Path.GetFileName(infFilePath);

                var infContent = File.ReadAllText(infFilePath);
                var strings = ParseStringsSection(infContent);

                Version driverVersion = new Version("0.0.0.0");
                DateTime driverDate = File.GetLastWriteTime(infFilePath);

                var driverVerMatch = Regex.Match(
                    infContent,
                    @"DriverVer\s*=\s*([^,\r\n]+)\s*,\s*([\d.]+)",
                    RegexOptions.IgnoreCase);

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

                string driverClass = "Unknown";
                var classMatch = Regex.Match(
                    infContent,
                    @"^Class\s*=\s*([^\r\n;]+)",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);

                if (classMatch.Success)
                {
                    driverClass = ResolveInfToken(classMatch.Groups[1].Value.Trim(), strings);
                }

                string providerRaw = "Custom Folder";
                var providerMatch = Regex.Match(
                    infContent,
                    @"^Provider\s*=\s*([^\r\n;]+)",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);

                if (providerMatch.Success)
                {
                    providerRaw = ResolveInfToken(providerMatch.Groups[1].Value.Trim(), strings);
                }

                ParseFirstModelEntry(infContent, strings, out string deviceName, out string hardwareId);

                return new DriverStoreEntry
                {
                    DriverInfName = infFileName,
                    DriverPublishedName = infFileName,
                    DriverFolderLocation = folderPath,
                    DriverVersion = driverVersion,
                    DriverDate = driverDate,
                    DriverSize = this.CalculatePackageSize(folderPath),
                    DriverClass = driverClass,
                    DriverPkgProvider = providerRaw,
                    DriverSignerName = "Custom Folder",
                    BootCritical = false,
                    DevicePresent = false,
                    DeviceName = deviceName,
                    DeviceId = hardwareId,
                    DriverExtensionId = Guid.Empty
                };
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error parsing driver package {infFilePath}: {ex.Message}");
                return null;
            }
        }

        private long CalculatePackageSize(string packageFolder)
        {
            try
            {
                return Directory.GetFiles(packageFolder, "*", SearchOption.AllDirectories)
                    .Sum(f => new FileInfo(f).Length);
            }
            catch
            {
                return 0;
            }
        }

        private static Dictionary<string, string> ParseStringsSection(string infContent)
        {
            var strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int stringsIdx = infContent.IndexOf("[Strings]", StringComparison.OrdinalIgnoreCase);
            if (stringsIdx < 0)
            {
                return strings;
            }

            string stringsArea = infContent.Substring(stringsIdx);
            foreach (var line in stringsArea.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("[", StringComparison.Ordinal) &&
                    !trimmed.StartsWith("[Strings]", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                Match match = InfStringsLineRegex.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                string key = match.Groups[1].Value.Trim();
                if (key.StartsWith("[", StringComparison.Ordinal))
                {
                    break;
                }

                string value = match.Groups[2].Value.Trim().Trim('"');
                if (!strings.ContainsKey(key))
                {
                    strings[key] = value;
                }
            }

            return strings;
        }

        private static string ResolveInfToken(string raw, IReadOnlyDictionary<string, string> strings)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            raw = raw.Trim().Trim('"');
            for (int i = 0; i < 5; i++)
            {
                if (!raw.StartsWith("%", StringComparison.Ordinal) || !raw.EndsWith("%", StringComparison.Ordinal))
                {
                    return raw;
                }

                string token = raw.Substring(1, raw.Length - 2).Trim();
                if (strings != null && strings.TryGetValue(token, out string resolved))
                {
                    raw = resolved.Trim().Trim('"');
                }
                else
                {
                    return token;
                }
            }

            return raw;
        }

        private static void ParseFirstModelEntry(string infContent, IReadOnlyDictionary<string, string> strings, out string deviceName, out string hardwareId)
        {
            deviceName = string.Empty;
            hardwareId = string.Empty;

            foreach (Match match in InfModelLineRegex.Matches(infContent))
            {
                deviceName = ResolveInfToken(match.Groups["desc"].Value.Trim(), strings);
                hardwareId = match.Groups["hwid"].Value.Trim();
                if (!string.IsNullOrEmpty(deviceName))
                {
                    return;
                }
            }
        }
    }
}
