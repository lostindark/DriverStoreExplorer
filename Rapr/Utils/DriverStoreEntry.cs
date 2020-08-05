using System;
using System.Collections.Generic;
using System.Globalization;

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

        public bool? BootCritical { get; set; }

        public string DeviceName { get; set; }

        public bool? DevicePresent { get; set; }

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
                "Device Name",
                "Device Present",
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
                this.DeviceName ?? string.Empty,
                this.DevicePresent?.ToString() ?? string.Empty,
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
