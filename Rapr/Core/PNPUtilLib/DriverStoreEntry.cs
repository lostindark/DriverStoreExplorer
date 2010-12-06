using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
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
    };
}
