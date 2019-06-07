using System;
using System.Windows.Forms;

using BrightIdeasSoftware;

namespace Rapr
{
    public class MyObjectListView : ObjectListView
    {
        protected override void HandleColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (!this.PossibleFinishCellEditing())
            {
                return;
            }

            int newPrimarySortColumn = e.Column;

            if (this.PrimarySortColumn != null)
            {
                if (Control.ModifierKeys == Keys.Shift)
                {
                    newPrimarySortColumn = this.PrimarySortColumn.Index;

                    // Update secondary column when shift key is down.
                    if (e.Column == this.SecondarySortColumn.Index)
                    {
                        // Toggle the secondary column sorting direction.
                        this.SecondarySortOrder = (this.SecondarySortOrder == SortOrder.Descending
                            ? SortOrder.Ascending
                            : SortOrder.Descending);
                    }
                    else
                    {
                        this.SecondarySortColumn = this.GetColumn(e.Column);
                        this.SecondarySortOrder = SortOrder.Ascending;
                    }
                }
                else
                {
                    if (e.Column == this.PrimarySortColumn.Index)
                    {
                        // Toggle the primary column sorting direction.
                        this.PrimarySortOrder = (this.PrimarySortOrder == SortOrder.Descending
                            ? SortOrder.Ascending
                            : SortOrder.Descending);
                    }
                    else
                    {
                        // Make the previous primary sort column as current secondary sort column.
                        this.SecondarySortColumn = this.PrimarySortColumn;
                        this.SecondarySortOrder = this.PrimarySortOrder;

                        this.PrimarySortOrder = SortOrder.Ascending;
                    }
                }
            }
            else
            {
                this.PrimarySortOrder = SortOrder.Ascending;
            }

            this.BeginUpdate();

            try
            {
                this.Sort(newPrimarySortColumn);
            }
            finally
            {
                this.EndUpdate();
            }
        }
    }
}
