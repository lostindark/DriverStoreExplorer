using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Rapr.Utils
{
    public class DriverDate : IComparable, IComparable<DriverDate>, IEquatable<DriverDate>
    {
        private DateTime date;

        public DriverDate(string str)
        {
            this.date = DateTime.Parse(str);
        }

        public DriverDate(DateTime date)
        {
            this.date = date;
        }

        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }

            DriverDate driverDate = value as DriverDate;
            if (driverDate == null)
            {
                throw new ArgumentException("The argument must be a DriverDate object.", "value");
            }

            return this.CompareTo(driverDate);
        }

        public int CompareTo(DriverDate value)
        {
            if (value == null)
            {
                return 1;
            }

            return this.date.CompareTo(value.date);
        }

        public bool Equals(DriverDate other)
        {
            return other != null && this.date.Equals(other.date);
        }

        public override string ToString()
        {
            return this.date.ToString("d", DateTimeFormatInfo.InvariantInfo);
        }
    }

    /// <summary>
    /// Data fields retrieved from Driver store for each driver
    /// </summary>
    public struct DriverStoreEntry
    {
        /// <summary>
        /// Name of the INF in driver store
        /// </summary>
        public string DriverPublishedName;

        /// <summary>
        /// Driver package provider
        /// </summary>
        public string DriverPkgProvider;

        /// <summary>
        /// Driver class (ex., "System Devices")
        /// </summary>
        public string DriverClass;

        /// <summary>
        /// Sys file date
        /// </summary>
        public DriverDate DriverDate;

        /// <summary>
        /// Sys file version
        /// </summary>
        public Version DriverVersion;

        /// <summary>
        /// Signer name. Empty if not WHQLd. 
        /// </summary>
        public string DriverSignerName;

        /// <summary>
        /// Field count
        /// </summary>
        private const int FIELD_COUNT = 6;

        public int GetFieldCount()
        {
            return FIELD_COUNT;
        }

        public string[] GetFieldNames()
        {
            return new String[] {
                "INF",
                "Package Provider",
                "Driver Class",
                "Driver Date",
                "Driver Version",
                "Driver Signer"};
        }

        public string[] GetFieldValues()
        {
            List<string> fieldValues = new List<string>();

            fieldValues.Add(this.DriverPublishedName);
            fieldValues.Add(this.DriverPkgProvider);
            fieldValues.Add(this.DriverClass);
            fieldValues.Add(this.DriverDate.ToString());
            fieldValues.Add(this.DriverVersion.ToString());
            fieldValues.Add(this.DriverSignerName);

            return fieldValues.ToArray();
        }
    };
}
