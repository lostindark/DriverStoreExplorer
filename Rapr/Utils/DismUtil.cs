using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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

        public static bool IsDismAvailable { get; } = new Func<bool>(() =>
        {
            try
            {
                Marshal.Prelink(((Func<DismLogLevel, string, string, int>)NativeMethods.DismInitialize).Method);
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            catch (EntryPointNotFoundException)
            {
                return false;
            }

            return true;
        })();

        public bool SupportAddInstall => this.Type == DriverStoreType.Online;

        public bool SupportForceDeletion => this.Type == DriverStoreType.Online;

        public bool SupportDeviceNameColumn => this.Type == DriverStoreType.Online;

        public bool SupportExportDriver => false;

        public bool SupportExportAllDrivers { get; } = new Func<bool>(() =>
        {
            try
            {
                Marshal.Prelink(((Func<DismSession, string, int>)NativeMethods._DismExportDriver).Method);
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            catch (EntryPointNotFoundException)
            {
                return false;
            }

            return true;
        })();

        #region IDriverStore Members
        public List<DriverStoreEntry> EnumeratePackages()
        {
            List<DriverStoreEntry> driverStoreEntries = new List<DriverStoreEntry>();

            DismApi.Initialize(DismLogLevel.LogErrors);

            try
            {
                using (DismSession session = this.GetSession())
                {
                    List<DeviceDriverInfo> driverInfo = this.Type == DriverStoreType.Online
                        ? ConfigManager.GetDeviceDriverInfo()
                        : null;

                    foreach (var driverPackage in DismApi.GetDrivers(session, false))
                    {
                        DriverStoreEntry driverStoreEntry = new DriverStoreEntry
                        {
                            DriverClass = driverPackage.ClassDescription,
                            DriverInfName = Path.GetFileName(driverPackage.OriginalFileName),
                            DriverPublishedName = driverPackage.PublishedName,
                            DriverPkgProvider = driverPackage.ProviderName,
                            DriverSignerName = driverPackage.DriverSignature == DismDriverSignature.Signed ? SetupAPI.GetDriverSignerInfo(driverPackage.OriginalFileName) : string.Empty,
                            DriverDate = driverPackage.Date,
                            DriverVersion = driverPackage.Version,
                            DriverFolderLocation = Path.GetDirectoryName(driverPackage.OriginalFileName),
                            DriverSize = DriverStoreRepository.GetFolderSize(new DirectoryInfo(Path.GetDirectoryName(driverPackage.OriginalFileName))),
                            BootCritical = driverPackage.BootCritical,
                        };

                        var deviceInfo = driverInfo?.OrderByDescending(d => d.IsPresent)?.FirstOrDefault(e =>
                            string.Equals(Path.GetFileName(e.DriverInf), driverStoreEntry.DriverPublishedName, StringComparison.OrdinalIgnoreCase)
                            && e.DriverVersion == driverStoreEntry.DriverVersion
                            && e.DriverDate == driverStoreEntry.DriverDate);

                        driverStoreEntry.DeviceName = deviceInfo?.DeviceName;
                        driverStoreEntry.DevicePresent = deviceInfo?.IsPresent;

                        driverStoreEntries.Add(driverStoreEntry);
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
            if (driverStoreEntry == null)
            {
                throw new ArgumentNullException(nameof(driverStoreEntry));
            }

            switch (this.Type)
            {
                case DriverStoreType.Online:
                    try
                    {
                        SetupAPI.DeleteDriver(driverStoreEntry, forceDelete);
                    }
                    catch (Win32Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                        return false;
                    }

                    return true;

                case DriverStoreType.Offline:
                    try
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
                    }
                    catch (DismRebootRequiredException)
                    {
                        return true;
                    }
                    catch (DismException ex)
                    {
                        Trace.TraceError(ex.ToString());
                        return false;
                    }

                    return true;

                default:
                    throw new NotSupportedException();
            }
        }

        public bool AddDriver(string infFullPath, bool install)
        {
            switch (this.Type)
            {
                case DriverStoreType.Online:
                    try
                    {
                        SetupAPI.AddDriver(infFullPath, install);
                    }
                    catch (Win32Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                        return false;
                    }

                    return true;

                case DriverStoreType.Offline:
                    try
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
                    }
                    catch (DismRebootRequiredException)
                    {
                        return true;
                    }
                    catch (DismException ex)
                    {
                        Trace.TraceError(ex.ToString());
                        return false;
                    }

                    return true;

                default:
                    throw new NotSupportedException();
            }
        }

        public bool ExportDriver(string infName, string destinationPath) => throw new NotSupportedException();

        public bool ExportAllDrivers(string destinationPath)
        {
            try
            {
                DismApi.Initialize(DismLogLevel.LogErrors);

                try
                {
                    using (DismSession session = this.GetSession())
                    {
                        int hresult = NativeMethods._DismExportDriver(session, destinationPath);

                        if (hresult != 0 && hresult != 1)
                        {
                            string lastErrorMessage = DismApi.GetLastErrorMessage();
                            if (!string.IsNullOrEmpty(lastErrorMessage))
                            {
                                throw new DismException(lastErrorMessage.Trim());
                            }

                            throw new DismException(hresult);
                        }
                    }
                }
                finally
                {
                    DismApi.Shutdown();
                }
            }
            catch (DismRebootRequiredException)
            {
                return true;
            }
            catch (DismException ex)
            {
                Trace.TraceError(ex.ToString());
                return false;
            }

            return true;
        }

        #endregion

        internal static class NativeMethods
        {
            [DllImport("DismApi", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismInitialize(DismLogLevel logLevel, string logFilePath, string scratchDirectory);

            [DllImport("DismApi", CharSet = CharSet.Unicode)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
            public static extern int _DismExportDriver(DismSession Session, string Destination);
        }
    }
}
