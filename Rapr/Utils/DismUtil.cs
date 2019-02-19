using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Dism;

namespace Rapr.Utils
{
    public class DismUtil : IDriverStore
    {
        public DriverStoreType Type { get; }

        public string OfflineStoreLocation { get; }

        public DismUtil()
        {
            this.Type = DriverStoreType.Online;
        }

        public DismUtil(string imagePath)
        {
            this.Type = DriverStoreType.Offline;
            this.OfflineStoreLocation = imagePath;
        }

        #region IDriverStore Members
        public List<DriverStoreEntry> EnumeratePackages()
        {
            List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();

            DismApi.Initialize(DismLogLevel.LogErrors);

            try
            {
                using (DismSession session = this.GetSession())
                {
                    foreach (var driverPackage in DismApi.GetDrivers(session, false))
                    {
                        driverStoreEntries.Add(new DriverStoreEntry
                        {
                            DriverClass = driverPackage.ClassDescription,
                            DriverInfName = Path.GetFileName(driverPackage.OriginalFileName),
                            DriverPublishedName = driverPackage.PublishedName,
                            DriverPkgProvider = driverPackage.ProviderName,
                            DriverSignerName = driverPackage.DriverSignature.ToString(),
                            DriverDate = driverPackage.Date,
                            DriverVersion = driverPackage.Version,
                            DriverFolderLocation = Path.GetDirectoryName(driverPackage.OriginalFileName),
                            DriverSize = DriverStoreRepository.GetFolderSize(new DirectoryInfo(Path.GetDirectoryName(driverPackage.OriginalFileName))),
                            BootCritical = driverPackage.BootCritical,
                            Inbox = driverPackage.InBox,
                        });
                    }
                }
            }
            finally
            {
                DismApi.Shutdown();
            }

            return driverStoreEntries;
        }

        private DismSession GetSession()
        {
            switch (this.Type)
            {
                case DriverStoreType.Online:
                    return DismApi.OpenOnlineSession();

                case DriverStoreType.Offline:
                    return DismApi.OpenOfflineSession(this.OfflineStoreLocation);

                default:
                    throw new NotSupportedException();
            }
        }

        public bool DeleteDriver(DriverStoreEntry driverStoreEntry, bool forceDelete)
        {
            DismApi.Initialize(DismLogLevel.LogErrors);

            try
            {
                using (DismSession session = this.GetSession())
                {
                    DismApi.RemoveDriver(session, driverStoreEntry.DriverPublishedName);
                }
            }
            finally
            {
                DismApi.Shutdown();
            }

            return true;
        }

        public bool AddDriver(string infFullPath, bool install)
        {
            DismApi.Initialize(DismLogLevel.LogErrors);

            try
            {
                using (DismSession session = this.GetSession())
                {
                    DismApi.AddDriver(session, infFullPath, false);
                }
            }
            finally
            {
                DismApi.Shutdown();
            }

            return true;
        }
        #endregion

    }
}
