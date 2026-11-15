using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
        private readonly ListView listView;
        private readonly Button buttonRemove;

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

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
            // A fixed-width column lets the description wrap and keeps the buttons inside the window.
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
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

            this.listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true,
                HideSelection = false,
            };
            this.listView.Columns.Add(GetString("GhostDevices_Column_Name"));
            this.listView.Columns.Add(GetString("GhostDevices_Column_Class"));
            this.listView.Columns.Add(GetString("GhostDevices_Column_InstanceId"));

            foreach (var device in devices.OrderBy(d => d.ClassName).ThenBy(d => d.Name))
            {
                this.listView.Items.Add(new ListViewItem(new[] { device.Name ?? device.InstanceId, device.ClassName, device.InstanceId })
                {
                    Tag = device,
                    Checked = device.SelectedByDefault,
                });
            }

            // Column widths need the window handle, so size them once the form is loaded.
            this.Load += (sender, e) => this.listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listView.ItemChecked += (sender, e) => this.buttonRemove.Enabled = this.listView.CheckedItems.Count > 0;

            this.buttonRemove = new Button
            {
                AutoSize = true,
                DialogResult = DialogResult.OK,
                Text = GetString("GhostDevices_Remove"),
                Enabled = this.listView.CheckedItems.Count > 0,
            };
            var buttonCancel = new Button { AutoSize = true, DialogResult = DialogResult.Cancel, Text = Language.Button_Cancel };

            var buttons = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.Right, FlowDirection = FlowDirection.LeftToRight };
            buttons.Controls.Add(this.buttonRemove);
            buttons.Controls.Add(buttonCancel);

            layout.Controls.Add(description, 0, 0);
            layout.Controls.Add(this.listView, 0, 1);
            layout.Controls.Add(buttons, 0, 2);
            this.Controls.Add(layout);

            this.AcceptButton = this.buttonRemove;
            this.CancelButton = buttonCancel;
            DriverStoreTheme.Attach(this);
        }

        public List<GhostDevice> SelectedDevices =>
            this.listView.CheckedItems.Cast<ListViewItem>().Select(item => (GhostDevice)item.Tag).ToList();

        private static string GetString(string name) => Language.ResourceManager.GetString(name, Language.Culture) ?? name;
    }
}
