using System.Collections.Generic;
using Rapr.Utils;

namespace Rapr
{
    public interface IExport
    {
        string Export(List<DriverStoreEntry> driverStoreEntries);
    }
}
