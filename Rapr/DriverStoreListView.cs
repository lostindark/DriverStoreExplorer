using System;
using System.Windows.Forms;

using BrightIdeasSoftware;

using Rapr.Utils;

namespace Rapr
{
    public sealed class DriverStoreListView : TypedObjectListView<DriverStoreEntry>
    {
        public override ToolStripDropDown MakeColumnSelectMenu(ToolStripDropDown strip)
        {
            base.MakeColumnSelectMenu(strip);
            RemoveSpacerColumnItems(strip.Items);
            return strip;
        }

        private static void RemoveSpacerColumnItems(ToolStripItemCollection items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                // Blank filler columns are layout spacers, not user-selectable data columns.
                if (item.Tag is OLVColumn column && column.FillsFreeSpace && String.IsNullOrEmpty(column.Text))
                {
                    items.RemoveAt(i);
                    item.Dispose();
                }
                else if (item is ToolStripMenuItem menu)
                {
                    RemoveSpacerColumnItems(menu.DropDownItems);
                }
            }
        }
    }
}
