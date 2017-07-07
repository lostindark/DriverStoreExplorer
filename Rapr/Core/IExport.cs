using System.Collections.Generic;
using Rapr.Utils;

namespace Rapr
{
    interface IExport
    {
        string Export(List<DriverStoreEntry> ldse);
    }
}
