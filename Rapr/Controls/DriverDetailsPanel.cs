using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

using Rapr.Lang;
using Rapr.Utils;

namespace Rapr.Controls
{
    /// <summary>
    /// Shows the package selected in the list: notes on whether it may still be needed, and its
    /// main fields.
    /// </summary>
    public sealed class DriverDetailsPanel : GroupBox
    {
        private static readonly Color OldVersionColor = Color.FromArgb(0xFF, 0xF1, 0xC4);
        private static readonly Color NoDeviceColor = Color.FromArgb(0xE6, 0xE6, 0xE6);
        private static readonly Color BootCriticalColor = Color.FromArgb(0xFD, 0xDE, 0xDE);
        private static readonly Color ExtensionColor = Color.FromArgb(0xDB, 0xEA, 0xFA);

        private readonly Panel scroller;
        private readonly TableLayoutPanel table;
        private readonly ToolTip toolTip = new ToolTip();

        // Controls whose width follows the pane, so their text wraps instead of being cut.
        private readonly List<Control> wrappingControls = new List<Control>();

        private DriverInsights insights;
        private Font titleFont;

        public DriverDetailsPanel()
        {
            this.Text = GetString("Details_Title");

            this.table = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Dock = DockStyle.Top,
                GrowStyle = TableLayoutPanelGrowStyle.AddRows,
            };
            this.table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            this.scroller = new Panel
            {
                AutoScroll = true,
                Dock = DockStyle.Fill,
            };
            this.scroller.Controls.Add(this.table);
            this.scroller.ClientSizeChanged += (sender, e) => this.UpdateWrapWidth();
            this.Controls.Add(this.scroller);

            this.ShowEntry(null, null);
        }

        /// <summary>
        /// Raised when the folder link is clicked. The package is <see cref="Entry"/>.
        /// </summary>
        public event EventHandler FolderClicked;

        /// <summary>
        /// The package shown, or null when nothing (or more than one package) is selected.
        /// </summary>
        public DriverStoreEntry Entry { get; private set; }

        /// <summary>
        /// The notes that explain whether a package may still be needed.
        /// </summary>
        public static IList<string> GetNotes(DriverStoreEntry entry, DriverInsights insights)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var notes = new List<string>();

            DriverStoreEntry newer = insights?.GetNewerVersion(entry);
            if (newer != null)
            {
                // A newer copy can have the same version and only a later date; show the date then,
                // or the note would read "A newer version, 1.0, ..." on a package that is 1.0 too.
                string newerVersion = newer.DriverVersion == entry.DriverVersion
                    ? string.Format(CultureInfo.CurrentCulture, "{0} ({1:d})", newer.DriverVersion, newer.DriverDate)
                    : newer.DriverVersion?.ToString();

                notes.Add(string.IsNullOrEmpty(newer.DeviceName) || newer.DevicePresent == false
                    ? FormatString("Details_Newer_Installed", newerVersion)
                    : FormatString("Details_Newer_In_Use", newerVersion, newer.DeviceName));
            }

            if (!string.IsNullOrEmpty(entry.DeviceName))
            {
                notes.Add(entry.DevicePresent == false
                    ? FormatString("Details_Device_Not_Present", entry.DeviceName)
                    : FormatString("Details_In_Use", entry.DeviceName));
            }
            else if (insights?.HasNoDevice(entry) == true)
            {
                notes.Add(GetString("Details_No_Device"));
            }

            if (DriverInsights.IsBootCritical(entry))
            {
                notes.Add(GetString("Details_Boot_Critical"));
            }

            return notes;
        }

        /// <param name="entry">The package to show, or null to show a hint instead.</param>
        /// <param name="insights">Facts about the whole store, or null when they aren't known yet.</param>
        public void ShowEntry(DriverStoreEntry entry, DriverInsights insights)
        {
            this.Entry = entry;
            this.insights = insights;

            this.table.SuspendLayout();
            try
            {
                this.ClearRows();

                if (entry == null)
                {
                    this.AddText(GetString("Details_Empty"), SystemColors.GrayText, wrap: true);
                }
                else
                {
                    this.AddEntry(entry, insights);
                }
            }
            finally
            {
                this.table.ResumeLayout(true);
            }

            this.UpdateWrapWidth();
            this.scroller.AutoScrollPosition = Point.Empty;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (this.table == null)
            {
                return;
            }

            this.titleFont?.Dispose();
            this.titleFont = null;
            this.ShowEntry(this.Entry, this.insights);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.toolTip.Dispose();
                this.titleFont?.Dispose();
            }

            base.Dispose(disposing);
        }

        private static string GetString(string name) => Language.ResourceManager.GetString(name, Language.Culture) ?? name;

        private static string FormatString(string name, params object[] args) =>
            string.Format(CultureInfo.CurrentCulture, GetString(name), args);

        private void AddEntry(DriverStoreEntry entry, DriverInsights insights)
        {
            if (this.titleFont == null)
            {
                this.titleFont = new Font(this.Font.FontFamily, this.Font.SizeInPoints * 1.2f, FontStyle.Bold, GraphicsUnit.Point);
            }

            string name = string.IsNullOrEmpty(entry.DriverInfName) ? entry.DriverPublishedName : entry.DriverInfName;
            Label title = this.AddText(name, Color.Empty, wrap: false);
            title.Font = this.titleFont;
            title.Height = title.PreferredHeight;

            string subtitle = string.Join(
                " · ",
                new[]
                {
                    entry.DriverPkgProvider,
                    entry.DriverVersion?.ToString(),
                    entry.DriverDate == default(DateTime) ? null : entry.DriverDate.ToString("d", CultureInfo.CurrentCulture),
                }.Where(part => !string.IsNullOrEmpty(part)));
            if (subtitle.Length > 0)
            {
                this.AddText(subtitle, SystemColors.GrayText, wrap: true);
            }

            this.AddTags(entry, insights);

            foreach (string note in GetNotes(entry, insights))
            {
                this.AddText(note, Color.Empty, wrap: true).Margin = this.ScaledMargin(top: 8);
            }

            this.AddField(Language.Column_Oem_Inf, entry.DriverPublishedName);
            this.AddField(Language.Column_Driver_Class, entry.DriverClass);
            this.AddField(Language.Column_Size, DriverStoreEntry.GetBytesReadable(entry.DriverSize));
            this.AddField(Language.Column_InstallDate, entry.InstallDate?.ToString("d", CultureInfo.CurrentCulture));
            this.AddField(Language.Column_Signed_By, entry.DriverSignerName);
            this.AddField(Language.Column_DeviceId, entry.DeviceId);
            this.AddField(Language.Column_Extension_Id, DriverInsights.IsExtension(entry) ? entry.DriverExtensionId.ToString() : null);

            if (!string.IsNullOrEmpty(entry.DriverFolderLocation))
            {
                this.AddText(GetString("Details_Folder"), SystemColors.GrayText, wrap: false).Margin = this.ScaledMargin(top: 8);

                var link = new LinkLabel
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    AutoSize = false,
                    Margin = this.ScaledMargin(),
                    Text = entry.DriverFolderName,
                    UseMnemonic = false,
                };
                link.LinkClicked += (sender, e) => this.FolderClicked?.Invoke(this, EventArgs.Empty);
                this.toolTip.SetToolTip(link, entry.DriverFolderLocation);
                this.AddRow(link);
                link.Height = link.PreferredHeight;
            }
        }

        private void AddTags(DriverStoreEntry entry, DriverInsights insights)
        {
            var tags = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = this.ScaledMargin(top: 6),
                WrapContents = true,
            };

            void AddTag(bool show, string name, Color color)
            {
                if (!show)
                {
                    return;
                }

                bool highContrast = SystemInformation.HighContrast;
                tags.Controls.Add(new Label
                {
                    AutoSize = true,
                    BackColor = highContrast ? SystemColors.Highlight : color,
                    ForeColor = highContrast ? SystemColors.HighlightText : Color.Black,
                    Margin = new Padding(0, 0, this.LogicalToDeviceUnits(4), this.LogicalToDeviceUnits(4)),
                    Padding = new Padding(this.LogicalToDeviceUnits(4), this.LogicalToDeviceUnits(1), this.LogicalToDeviceUnits(4), this.LogicalToDeviceUnits(2)),
                    Text = GetString(name),
                    UseMnemonic = false,
                });
            }

            AddTag(insights?.IsOldVersion(entry) == true, "Details_Tag_Old_Version", OldVersionColor);
            AddTag(insights?.HasNoDevice(entry) == true, "Details_Tag_No_Device", NoDeviceColor);
            AddTag(DriverInsights.IsBootCritical(entry), "Details_Tag_Boot_Critical", BootCriticalColor);
            AddTag(DriverInsights.IsExtension(entry), "Details_Tag_Extension", ExtensionColor);

            if (tags.Controls.Count == 0)
            {
                tags.Dispose();
                return;
            }

            this.wrappingControls.Add(tags);
            this.AddRow(tags);
        }

        private void AddField(string caption, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            this.AddText(caption, SystemColors.GrayText, wrap: false).Margin = this.ScaledMargin(top: 8);

            // One line; a value that doesn't fit ends with "..." and shows in full in a tooltip.
            this.AddText(value, Color.Empty, wrap: false);
        }

        private Label AddText(string text, Color color, bool wrap)
        {
            var label = new Label
            {
                Margin = this.ScaledMargin(),
                Text = text,
                UseMnemonic = false,
            };

            if (!color.IsEmpty)
            {
                label.ForeColor = color;
            }

            if (wrap)
            {
                label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                label.AutoSize = true;
                this.wrappingControls.Add(label);
            }
            else
            {
                label.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                label.AutoEllipsis = true;
                label.AutoSize = false;
            }

            this.AddRow(label);

            // Only now does the label have the pane's font; measured before, it is too short and
            // cuts off underscores and letters like g and y.
            if (!wrap)
            {
                label.Height = label.PreferredHeight;
            }

            return label;
        }

        private void AddRow(Control control)
        {
            this.table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.table.Controls.Add(control, 0, this.table.RowStyles.Count - 1);
            this.table.RowCount = this.table.RowStyles.Count;
        }

        private void ClearRows()
        {
            this.wrappingControls.Clear();

            while (this.table.Controls.Count > 0)
            {
                Control control = this.table.Controls[0];
                this.toolTip.SetToolTip(control, null);
                control.Dispose();
            }

            this.table.RowStyles.Clear();
            this.table.RowCount = 0;
        }

        private void UpdateWrapWidth()
        {
            int width = this.scroller.ClientSize.Width - this.table.Padding.Horizontal;

            foreach (Control control in this.wrappingControls)
            {
                control.MaximumSize = new Size(Math.Max(1, width - control.Margin.Horizontal), 0);
            }
        }

        private Padding ScaledMargin(int top = 0) =>
            new Padding(this.LogicalToDeviceUnits(2), this.LogicalToDeviceUnits(top), this.LogicalToDeviceUnits(2), 0);
    }
}
