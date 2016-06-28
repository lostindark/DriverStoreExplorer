using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rapr.Utils
{
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
        public string DriverDate;

        /// <summary>
        /// Sys file version
        /// </summary>
        public string DriverVersion;

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
            fieldValues.Add(this.DriverDate);
            fieldValues.Add(this.DriverVersion);
            fieldValues.Add(this.DriverSignerName);

            return fieldValues.ToArray();
        }
    };
}
