using System.Collections.Generic;

namespace Rapr.Utils
{
    public interface IDriverStore
    {
        List<DriverStoreEntry> EnumeratePackages();

        bool DeleteDriver(DriverStoreEntry dse, bool forceDelete);

        bool AddDriver(string infFullPath, bool install);
    }
}
