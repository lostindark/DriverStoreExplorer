using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Rapr.Utils
{
    /// <summary>
    /// Data fields retrieved from Driver store for each driver
    /// </summary>
    public class DriverStoreEntry
    {
        /// <summary>
        /// Name of the OEM INF in driver store
        /// </summary>
        public string DriverPublishedName { get; set; }

        /// <summary>
        /// Driver Original INF Name
        /// </summary>
        public string DriverInfName { get; set; }

        /// <summary>
        /// Driver package provider
        /// </summary>
        public string DriverPkgProvider { get; set; }

        /// <summary>
        /// Driver class (ex., "System Devices")
        /// </summary>
        public string DriverClass { get; set; }

        /// <summary>
        /// Driver ExtensionId.
        /// </summary>
        public Guid DriverExtensionId { get; set; }

        /// <summary>
        /// Sys file date
        /// </summary>
        public DateTime DriverDate { get; set; }

        /// <summary>
        /// Sys file version
        /// </summary>
        public Version DriverVersion { get; set; }

        /// <summary>
        /// Signer name. Empty if not WHQLd. 
        /// </summary>
        public string DriverSignerName { get; set; }

        /// <summary>
        /// Estimated driver size on disk.
        /// </summary>
        public long DriverSize { get; set; }

        /// <summary>
        /// The folder that contains the driver.
        /// </summary>
        public string DriverFolderLocation { get; set; }

        /// <summary>
        /// The folder name (not include full path) that contains the driver.
        /// </summary>
        public string DriverFolderName => Path.GetFileName(DriverFolderLocation);

        /// <summary>
        /// Generate a folder name following the DriversBackup naming convention:
        /// DeviceCategory\DeviceName Version
        /// For drivers without devices: "Drivers without existing device\DeviceCategory\DeviceName Version"
        /// </summary>
        public string GetDriversBackupFolderName()
        {
            // Check if this is a driver without an existing device
            // This happens when DeviceName is null/empty and we had to fall back to INF name
            bool isDriverWithoutDevice = string.IsNullOrEmpty(this.DeviceName);

            string folderName = !isDriverWithoutDevice
                ? this.DeviceName
                : !string.IsNullOrEmpty(this.DriverInfName)
                    ? Path.GetFileNameWithoutExtension(this.DriverInfName)
                    : "Unknown Device";

            // Get device category (driver class)
            string deviceCategory = !string.IsNullOrEmpty(this.DriverClass) ? this.DriverClass : "Unknown";

            // Sanitize folder names
            folderName = SanitizeFolderName(folderName, 70);
            deviceCategory = SanitizeFolderName(deviceCategory, 50);

            // Append driver version if available
            if (this.DriverVersion != null)
            {
                folderName += $"_{this.DriverVersion}";
            }

            if (isDriverWithoutDevice)
            {
                return Path.Combine("Drivers without existing device", deviceCategory, folderName);
            }
            else
            {
                return Path.Combine(deviceCategory, folderName);
            }
        }

        // Compiled regex patterns for better performance
        private static readonly Regex InvalidCharsRegex = new Regex(@"[<>:""|?*/\\]|[\x00-\x1F]", RegexOptions.Compiled);
        private static readonly Regex MultipleSpacesRegex = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Sanitize a folder name by removing invalid characters and limiting length,
        /// following Windows file system restrictions.
        /// </summary>
        private static string SanitizeFolderName(string name, int maxLength)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "Unknown";
            }

            // Remove Windows forbidden characters: < > : " | ? * / \
            // Also remove control characters (ASCII 0-31)
            string sanitized = InvalidCharsRegex.Replace(name, " ");

            // Remove registered trademark symbol and other problematic Unicode characters
            sanitized = sanitized.Replace("®", "")
                .Replace("™", "")
                .Replace("©", "");

            // Collapse multiple spaces and trim
            sanitized = MultipleSpacesRegex.Replace(sanitized.Trim(), " ");

            // Check for reserved names
            string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            
            if (Array.Exists(reservedNames, n => n.Equals(sanitized, StringComparison.OrdinalIgnoreCase)))
            {
                sanitized = sanitized + "_";
            }

            // Limit length (leave room for potential suffix)
            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength);
            }

            // Remove trailing periods and spaces (Windows removes these automatically)
            return sanitized.TrimEnd('.', ' ');
        }

        /// <summary>
        /// Whether the driver is marked as boot critical or not.
        /// </summary>
        public bool? BootCritical { get; set; }

        /// <summary>
        /// Associated device Id (device instance path).
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Associated device name.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Whether the associated device present in the system or not.
        /// </summary>
        public bool? DevicePresent { get; set; }

        /// <summary>
        /// Driver install date - when the driver package was imported/added to the driver store
        /// </summary>
        public DateTime? InstallDate { get; set; }

        public override string ToString()
        {
            return $"PublishedName: {this.DriverPublishedName}, InfName: {this.DriverInfName}, Class: {this.DriverClass}, Version: {this.DriverVersion}, DeviceName: {this.DeviceName}";
        }

        public void SetDriverDateAndVersion(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                string[] dateAndVersion = value.Trim().Split(new char[] { ' ' }, 2);
                if (dateAndVersion.Length == 2)
                {
                    this.DriverDate = default(DateTime);
                    this.DriverVersion = null;

                    if (DateTime.TryParse(dateAndVersion[0].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime driverDate))
                    {
                        this.DriverDate = driverDate;
                    }

                    if (Version.TryParse(dateAndVersion[1].Trim(), out Version driverVersion))
                    {
                        this.DriverVersion = driverVersion;
                    }
                }
            }
        }

        public int? OemId
        {
            get
            {
                string oemInfName = this.DriverPublishedName;

                if (!string.IsNullOrEmpty(oemInfName))
                {
                    if (oemInfName.StartsWith("oem", StringComparison.OrdinalIgnoreCase))
                    {
                        oemInfName = oemInfName.Substring(3);
                    }

                    if (oemInfName.EndsWith(".inf", StringComparison.OrdinalIgnoreCase))
                    {
                        oemInfName = oemInfName.Substring(0, oemInfName.Length - 4);
                    }

                    if (int.TryParse(oemInfName, out int id))
                    {
                        return id;
                    }
                }

                return null;
            }
        }

        public static string[] GetFieldNames()
        {
            return new[] {
                "OEM INF",
                "INF",
                "Package Provider",
                "Driver Class",
                "Driver Date",
                "Driver Version",
                "Driver Signer",
                "Driver Size",
                "Driver Folder",
                "Device Id",
                "Device Name",
                "Device Present",
                "Install Date",
            };
        }

        public string[] GetFieldValues()
        {
            return new[]
            {
                this.DriverPublishedName ?? string.Empty,
                this.DriverInfName ?? string.Empty,
                this.DriverPkgProvider ?? string.Empty,
                this.DriverClass ?? string.Empty,
                this.DriverDate.ToString("d"),
                this.DriverVersion?.ToString() ?? string.Empty,
                this.DriverSignerName ?? string.Empty,
                this.DriverSize.ToString(),
                this.DriverFolderLocation ?? string.Empty,
                this.DeviceId ?? string.Empty,
                this.DeviceName ?? string.Empty,
                this.DevicePresent?.ToString() ?? string.Empty,
                this.InstallDate?.ToString("d") ?? string.Empty,
            };
        }

        // Returns the human-readable file size for an arbitrary, 64-bit file size 
        // The default format is "0.### XB", e.g. "4 KB" or "1.4 GB"
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);

            // Determine the format of the readable value
            string format;
            double readable;

            if (absolute_i >= 0x40000000) // Gigabyte
            {
                format = "0.0 \\GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                format = "0 \\MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                format = "0 \\KB";
                readable = i;
            }
            else
            {
                return "1 KB";
            }

            // Divide by 1024 to get fractional value
            readable /= 1024;

            // Return formatted number with suffix
            return readable.ToString(format);
        }

        private static readonly Dictionary<long, string> SizeRangeToName = new Dictionary<long, string>
        {
            { 10 * 1024, "0 - 10 KB" },
            { 100 * 1024, "10 - 100 KB" },
            { 1024 * 1024, "100 KB - 1 MB" },
            { 16 * 1024 * 1024, "1 - 16 MB" },
            { 128 * 1024 * 1024, "16 - 128 MB" },
            { long.MaxValue, "> 128 MB" },
        };

        public static long GetSizeRange(long size)
        {
            foreach (var item in SizeRangeToName)
            {
                if (size < item.Key)
                {
                    return item.Key;
                }
            }

            return -1;
        }

        public static string GetSizeRangeName(long size)
        {
            if (SizeRangeToName.TryGetValue(size, out string name))
            {
                return name;
            }
            else
            {
                return string.Empty;
            }
        }
    };
}
