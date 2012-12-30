using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rapr.Utils
{
    public interface IDriverStore
    {
        List<DriverStoreEntry> EnumeratePackages();
        bool DeletePackage(DriverStoreEntry dse, bool forceDelete);
        bool AddPackage(string infFullPath, bool install);
    }
}
