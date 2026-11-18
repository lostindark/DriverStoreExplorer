using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

using BrightIdeasSoftware;

using Rapr.Lang;
using Rapr.Utils;

namespace Rapr
{
    /// <summary>
    /// Lists ghost devices with checkboxes so the user picks which ones to remove.
    /// Built in code because its texts have no generated Language properties yet.
    /// </summary>
    public class GhostDevicesForm : Form
    {
        // ObjectListView, not ListView: the shared theme only paints its header, scrollbars and checkboxes dark.
        private readonly ObjectListView listView;
        private readonly Button buttonRemove;
        private readonly CheckBox checkBoxHideRecent;
        private readonly TextBox textRecentDays;
        private int recentDays = 30;
        // Check state lives here so a device keeps it while the filter hides it.
        private readonly HashSet<GhostDevice> checkedDevices;

        public GhostDevicesForm(IEnumerable<GhostDevice> devices)
        {
            this.Text = GetString("GhostDevices_Title");
            this.Font = SystemFonts.MessageBoxFont;
            this.ClientSize = new Size(760, 440);
            this.MinimumSize = new Size(480, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(8) };
            // A fixed-width column lets the description wrap and keeps the buttons inside the window.
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var description = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(3, 3, 3, 8),
                Text = GetString("GhostDevices_Description"),
            };

            this.checkedDevices = new HashSet<GhostDevice>(devices.Where(device => device.SelectedByDefault));
            this.listView = new ObjectListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                ShowGroups = false,
                UseFiltering = true,
                // The dialog only opens when there are ghost devices, so an empty list means the filter hid them all.
                EmptyListMsg = GetString("GhostDevices_AllHidden"),
                CheckStateGetter = model => this.checkedDevices.Contains((GhostDevice)model) ? CheckState.Checked : CheckState.Unchecked,
                CheckStatePutter = (model, state) =>
                {
                    this.SetChecked((GhostDevice)model, state == CheckState.Checked);
                    return state;
                },
            };
            this.listView.ItemChecked += (sender, e) => this.UpdateRemoveButton();
            this.listView.AllColumns.AddRange(new[]
            {
                new OLVColumn(GetString("GhostDevices_Column_Name"), null) { AspectGetter = model => ((GhostDevice)model).Name ?? ((GhostDevice)model).InstanceId },
                new OLVColumn(GetString("GhostDevices_Column_Class"), nameof(GhostDevice.ClassName)),
                new OLVColumn(GetString("GhostDevices_Column_LastUsed"), null)
                {
                    AspectGetter = model => ((GhostDevice)model).LastUsed?.ToLocalTime(),
                    AspectToStringFormat = Language.Date_Format,
                },
                new OLVColumn(GetString("GhostDevices_Column_InstanceId"), nameof(GhostDevice.InstanceId)),
            });
            this.listView.RebuildColumns();
            this.listView.SetObjects(devices.OrderBy(d => d.ClassName).ThenBy(d => d.Name).ToList());

            this.checkBoxHideRecent = new CheckBox
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Checked = true,
                Text = GetString("GhostDevices_HideRecent"),
            };
            // A plain text box: the shared theme cannot paint NumericUpDown or ComboBox buttons dark.
            this.textRecentDays = new TextBox
            {
                Anchor = AnchorStyles.Left,
                MaxLength = 4,
                Width = 50,
                Text = this.recentDays.ToString(CultureInfo.CurrentCulture),
            };
            this.textRecentDays.KeyPress += (sender, e) => e.Handled = !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar);
            this.textRecentDays.TextChanged += (sender, e) =>
            {
                // An empty or zero value (mid-edit, or pasted text) keeps the last valid number.
                if (int.TryParse(this.textRecentDays.Text, NumberStyles.None, CultureInfo.CurrentCulture, out int days) && days > 0)
                {
                    this.recentDays = days;
                    this.ApplyFilter();
                }
            };
            this.checkBoxHideRecent.CheckedChanged += (sender, e) => this.ApplyFilter();

            var filter = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.Left, WrapContents = false, Margin = new Padding(0, 0, 0, 4) };
            filter.Controls.Add(this.checkBoxHideRecent);
            filter.Controls.Add(this.textRecentDays);

            // Column widths need the window handle, so size them once the form is loaded,
            // over every device so a column is not cut off when the filter is turned off.
            this.Load += (sender, e) =>
            {
                this.listView.ModelFilter = null;
                this.listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                for (int i = 0; i < this.listView.Columns.Count - 1; i++)
                {
                    ColumnHeader column = this.listView.Columns[i];
                    int contentWidth = column.Width;
                    // Wide enough for the header and its sort arrow, but a name that falls back to
                    // a long instance ID must not push the other columns out of view.
                    this.listView.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.HeaderSize);
                    column.Width = Math.Min(Math.Max(contentWidth, column.Width + SystemInformation.SmallIconSize.Width), this.listView.ClientSize.Width / 3);
                }

                this.ApplyFilter();
            };

            this.buttonRemove = new Button
            {
                AutoSize = true,
                DialogResult = DialogResult.OK,
                Text = GetString("GhostDevices_Remove"),
            };
            var buttonCancel = new Button { AutoSize = true, DialogResult = DialogResult.Cancel, Text = Language.Button_Cancel };

            var buttons = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.Right, FlowDirection = FlowDirection.LeftToRight };
            buttons.Controls.Add(this.buttonRemove);
            buttons.Controls.Add(buttonCancel);

            layout.Controls.Add(description, 0, 0);
            layout.Controls.Add(filter, 0, 1);
            layout.Controls.Add(this.listView, 0, 2);
            layout.Controls.Add(buttons, 0, 3);
            this.Controls.Add(layout);

            this.AcceptButton = this.buttonRemove;
            this.CancelButton = buttonCancel;
            this.ApplyFilter();
            DriverStoreTheme.Attach(this);
        }

        private void ApplyFilter()
        {
            this.ReadVisibleChecks();
            bool hideRecent = this.checkBoxHideRecent.Checked;
            int days = this.recentDays;
            DateTime nowUtc = DateTime.UtcNow;
            this.textRecentDays.Enabled = hideRecent;

            this.listView.ModelFilter = hideRecent
                ? new ModelFilter(model => !GhostDevice.IsUsedWithin(((GhostDevice)model).LastUsed, nowUtc, days))
                : null;
            this.UpdateRemoveButton();
        }

        private void UpdateRemoveButton()
        {
            // ItemChecked also fires while the list is filled, before the button exists.
            if (this.buttonRemove != null)
            {
                this.buttonRemove.Enabled = this.SelectedDevices.Count > 0;
            }
        }

        private void SetChecked(GhostDevice device, bool isChecked)
        {
            if (isChecked)
            {
                this.checkedDevices.Add(device);
            }
            else
            {
                this.checkedDevices.Remove(device);
            }
        }

        // Accessibility tools (UI Automation) toggle a row without going through CheckStatePutter,
        // so the rows on screen are the source of truth for what the user selected.
        private void ReadVisibleChecks()
        {
            foreach (OLVListItem item in this.listView.Items)
            {
                this.SetChecked((GhostDevice)item.RowObject, item.Checked);
            }
        }

        // Exactly the rows shown checked: a device hidden by the filter is never removed.
        public List<GhostDevice> SelectedDevices =>
            this.listView.Items.Cast<OLVListItem>().Where(item => item.Checked).Select(item => (GhostDevice)item.RowObject).ToList();

        private static string GetString(string name) => Language.ResourceManager.GetString(name, Language.Culture) ?? name;
    }
}
