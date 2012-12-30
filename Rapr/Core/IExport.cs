using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rapr.Utils;
namespace Rapr
{
    interface IExport
    {
        void Export(List<DriverStoreEntry> ldse);
    }
}
