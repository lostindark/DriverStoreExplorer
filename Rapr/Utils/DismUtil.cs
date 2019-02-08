using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Dism;

namespace Rapr.Utils
{
    public class DismUtil : IDriverStore
    {
        #region IDriverStore Members
        public List<DriverStoreEntry> EnumeratePackages()
        {
            List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();

            DismApi.Initialize(DismLogLevel.LogErrors);

            try
            {
                using (DismSession session = DismApi.OpenOnlineSession())
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

        public bool DeleteDriver(DriverStoreEntry dse, bool forceDelete)
        {
            DismApi.Initialize(DismLogLevel.LogErrors);

            try
            {
                using (DismSession session = DismApi.OpenOnlineSession())
                {
                    DismApi.RemoveDriver(session, dse.DriverFolderLocation);
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
                using (DismSession session = DismApi.OpenOnlineSession())
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
