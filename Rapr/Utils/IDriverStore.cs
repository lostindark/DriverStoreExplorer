using System.Collections.Generic;

namespace Rapr.Utils
{
    public enum AddDriverResult
    {
        Added,
        Skipped,
        Failed
    }

    public interface IDriverStore
    {
        DriverStoreType Type { get; }

        string OfflineStoreLocation { get; }

        bool SupportAddInstall { get; }

        bool SupportForceDeletion { get; }

        bool SupportDeviceNameColumn { get; }

        bool SupportExportDriver { get; }

        bool SupportExportAllDrivers { get; }

        List<DriverStoreEntry> EnumeratePackages();

        bool DeleteDriver(DriverStoreEntry driverStoreEntry, bool forceDelete);

        AddDriverResult AddDriver(string infFullPath, bool install);

        bool ExportDriver(DriverStoreEntry driverStoreEntry, string destinationPath);

        bool ExportAllDrivers(string destinationPath);
    }
}
