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
        public string driverPublishedName;

        /// <summary>
        /// Driver package provider
        /// </summary>
        public string driverPkgProvider;

        /// <summary>
        /// Driver class (ex., "System Devices")
        /// </summary>
        public string driverClass;

        /// <summary>
        /// Sys file date
        /// </summary>
        public string driverDate;

        /// <summary>
        /// Sys file version
        /// </summary>
        public string driverVersion;

        /// <summary>
        /// Signer name. Empty if not WHQLd. 
        /// </summary>
        public string driverSignerName;

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
            return new String[] {   "INF", 
                                    "Package Provider", 
                                    "Driver Class", 
                                    "Driver Date", 
                                    "Driver Version", 
                                    "Driver Signer"
                                };
        }

        public string[] GetFieldValues()
        {
            List<string> fieldValues = new List<string>();

            fieldValues.Add(this.driverPublishedName);
            fieldValues.Add(this.driverPkgProvider);
            fieldValues.Add(this.driverClass);
            fieldValues.Add(this.driverDate);
            fieldValues.Add(this.driverVersion);
            fieldValues.Add(this.driverSignerName);

            return fieldValues.ToArray();
        }
    };
}
