using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BrightIdeasSoftware;

using Rapr.Lang;
using Rapr.Utils;

namespace Rapr.Controls
{
    /// <summary>
    /// Shows the package selected in the list: notes on whether it may still be needed, and its
    /// main fields.
    /// </summary>
    public sealed class DriverDetailsPanel : GroupBox, IThemeAware
    {
        private static readonly Color OldVersionColor = Color.FromArgb(0xFF, 0xF1, 0xC4);
        private static readonly Color NoDeviceColor = Color.FromArgb(0xE6, 0xE6, 0xE6);
        private static readonly Color BootCriticalColor = Color.FromArgb(0xFD, 0xDE, 0xDE);
        private static readonly Color ExtensionColor = Color.FromArgb(0xDB, 0xEA, 0xFA);

        private readonly DetailsRichTextBox content;
        private readonly ToolTip toolTip = new ToolTip();

        private DriverInsights insights;
        private Font titleFont;
        private Font folderFont;
        private int folderStart = -1;
        private int folderLength;
        private bool folderHovered;
        private IThemeColors palette = new ThemePalette(false, SystemInformation.HighContrast);

        public DriverDetailsPanel()
        {
            this.Text = GetString("Details_Title");
            this.UpdateContentPadding();

            this.content = new DetailsRichTextBox(this.IsFolderPosition)
            {
                BackColor = this.BackColor,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Default,
                DetectUrls = false,
                Dock = DockStyle.Fill,
                HideSelection = false,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                WordWrap = true,
            };
            EnableTripleClickSelection(this.content);
            this.content.FolderClicked += (sender, e) => this.FolderClicked?.Invoke(this, EventArgs.Empty);
            this.content.MouseMove += (sender, e) => this.SetFolderHover(this.IsFolderPosition(e.Location));
            this.content.MouseLeave += (sender, e) => this.SetFolderHover(false);
            this.content.VScroll += (sender, e) => this.SetFolderHover(false);
            this.Controls.Add(this.content);

            this.ShowEntry(null, null);
        }

        /// <summary>
        /// Raised when the folder path is Ctrl+clicked. The package is <see cref="Entry"/>.
        /// </summary>
        public event EventHandler FolderClicked;

        /// <summary>
        /// The package shown, or null when nothing (or more than one package) is selected.
        /// </summary>
        public DriverStoreEntry Entry { get; private set; }

        public void ApplyTheme(IThemeColors colors)
        {
            this.palette = colors;
            this.content.BackColor = this.BackColor;
            this.content.ForeColor = this.ForeColor;
            this.ShowEntry(this.Entry, this.insights);
        }

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

            this.folderStart = -1;
            this.folderLength = 0;
            this.content.CancelFolderClick();
            this.SetFolderHover(false);
            this.content.Clear();

            if (entry == null)
            {
                this.AppendParagraph(GetString("Details_Empty"), this.palette.GrayTextColor);
            }
            else
            {
                this.AddEntry(entry, insights);
            }

            this.content.Select(0, 0);
            this.content.ScrollToCaret();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (this.content == null)
            {
                return;
            }

            this.titleFont?.Dispose();
            this.titleFont = null;
            this.folderFont?.Dispose();
            this.folderFont = null;
            this.content.Font = this.Font;
            this.ShowEntry(this.Entry, this.insights);
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            this.UpdateContentPadding();
        }

        private void UpdateContentPadding()
        {
            int inset = this.LogicalToDeviceUnits(8);
            this.Padding = new Padding(inset, this.LogicalToDeviceUnits(4), inset, inset);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.toolTip.Dispose();
                this.titleFont?.Dispose();
                this.folderFont?.Dispose();
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
            this.AppendParagraph(name, Color.Empty, this.titleFont);

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
                this.AppendParagraph(subtitle, this.palette.GrayTextColor);
            }

            this.AddTags(entry, insights);

            foreach (string note in GetNotes(entry, insights))
            {
                this.AppendParagraph(note, Color.Empty, spaceBefore: 3);
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
                this.AddCaption(GetString("Details_Folder"));
                if (this.folderFont == null)
                {
                    this.folderFont = new Font(this.Font, this.Font.Style | FontStyle.Underline);
                }

                this.folderStart = this.AppendParagraph(entry.DriverFolderLocation,
                    this.palette.LinkColor, this.folderFont);
                this.folderLength = this.content.TextLength - this.folderStart;
            }
        }

        private bool IsFolderPosition(Point location)
        {
            if (this.folderStart < 0 || !this.content.ClientRectangle.Contains(location))
            {
                return false;
            }

            int index = this.content.GetCharIndexFromPosition(location);
            return this.IsFolderCharacter(index, location) || this.IsFolderCharacter(index - 1, location);
        }

        private bool IsFolderCharacter(int index, Point location)
        {
            if (index < this.folderStart || index >= this.folderStart + this.folderLength)
            {
                return false;
            }

            Point start = this.content.GetPositionFromCharIndex(index);
            Point end = this.content.GetPositionFromCharIndex(index + 1);
            int left = start.Y == end.Y ? Math.Min(start.X, end.X) : start.X;
            int width = start.Y == end.Y
                ? Math.Abs(end.X - start.X)
                : TextRenderer.MeasureText(this.content.Text[index].ToString(), this.Font, Size.Empty, TextFormatFlags.NoPadding).Width;
            return new Rectangle(left, start.Y, Math.Max(1, width), this.content.Font.Height).Contains(location);
        }

        private void SetFolderHover(bool hovered)
        {
            if (this.folderHovered != hovered)
            {
                this.folderHovered = hovered;
                this.toolTip.SetToolTip(this.content, hovered ? GetString("Details_Folder_Open_Hint") : null);
            }
        }

        private void AddTags(DriverStoreEntry entry, DriverInsights insights)
        {
            bool hasTags = false;
            int start = 0;

            void AddTag(bool show, string name, Color color)
            {
                if (!show)
                {
                    return;
                }

                if (hasTags)
                {
                    this.AppendText("  ", Color.Empty);
                }
                else
                {
                    start = this.BeginParagraph();
                }

                bool highContrast = this.palette.HighContrast;
                this.AppendText(
                    " " + GetString(name) + " ",
                    highContrast ? this.palette.HighlightTextColor : this.palette.Dark ? this.palette.TextColor : Color.Black,
                    background: highContrast ? this.palette.HighlightColor : this.palette.Dark ? ControlPaint.Dark(color, 0.65f) : color);
                hasTags = true;
            }

            AddTag(insights?.IsOldVersion(entry) == true, "Details_Tag_Old_Version", OldVersionColor);
            AddTag(insights?.HasNoDevice(entry) == true, "Details_Tag_No_Device", NoDeviceColor);
            AddTag(DriverInsights.IsBootCritical(entry), "Details_Tag_Boot_Critical", BootCriticalColor);
            AddTag(DriverInsights.IsExtension(entry), "Details_Tag_Extension", ExtensionColor);

            if (hasTags)
            {
                this.FormatParagraph(start, 0);
            }
        }

        private void AddField(string caption, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            this.AddCaption(caption);
            this.AppendParagraph(value, Color.Empty);
        }

        private void AddCaption(string text)
        {
            this.AppendParagraph(text, this.palette.GrayTextColor, spaceBefore: 2);
        }

        private int AppendParagraph(string text, Color color, Font font = null, int spaceBefore = 0)
        {
            int start = this.BeginParagraph();
            this.AppendText(text, color, font);
            this.FormatParagraph(start, spaceBefore);
            return start;
        }

        private int BeginParagraph()
        {
            if (this.content.TextLength > 0)
            {
                this.AppendText("\n", Color.Empty);
            }

            return this.content.TextLength;
        }

        private void FormatParagraph(int start, int spaceBefore)
        {
            this.content.Select(start, this.content.TextLength - start);
            this.SetParagraphSpacing(spaceBefore);
            this.content.Select(this.content.TextLength, 0);
        }

        private void SetParagraphSpacing(int spaceBefore)
        {
            const int EmSetParagraphFormat = 0x0447;
            const uint SpaceBefore = 0x40;
            const uint SpaceAfter = 0x80;
            var format = new SafeNativeMethods.ParagraphFormat2
            {
                Size = (uint)Marshal.SizeOf<SafeNativeMethods.ParagraphFormat2>(),
                Mask = SpaceBefore | SpaceAfter,
                SpaceBefore = spaceBefore * 20,
                SpaceAfter = 0,
            };
            if (SafeNativeMethods.SendMessage(
                new HandleRef(this.content, this.content.Handle), EmSetParagraphFormat, IntPtr.Zero, ref format) == IntPtr.Zero)
            {
                throw new InvalidOperationException("Could not set details paragraph spacing.");
            }
        }

        private void AppendText(string text, Color color, Font font = null, Color? background = null)
        {
            this.content.Select(this.content.TextLength, 0);
            this.content.SelectionFont = font ?? this.Font;
            this.content.SelectionColor = color.IsEmpty ? this.ForeColor : color;
            this.content.SelectionBackColor = background ?? this.BackColor;
            this.content.AppendText(text);
        }

        private static void EnableTripleClickSelection(TextBoxBase textBox)
        {
            int clicks = 0;
            int lastClickTime = 0;
            Rectangle clickArea = Rectangle.Empty;
            bool selectLineOnMouseUp = false;
            int selectionStart = 0;
            int selectionLength = 0;

            textBox.MouseDown += (sender, e) =>
            {
                selectLineOnMouseUp = false;
                if (e.Button != MouseButtons.Left)
                {
                    clicks = 0;
                    return;
                }

                int now = Environment.TickCount;
                uint elapsed = unchecked((uint)(now - lastClickTime));
                clicks = clicks > 0 && elapsed <= (uint)SystemInformation.DoubleClickTime && clickArea.Contains(e.Location)
                    ? clicks + 1
                    : 1;
                lastClickTime = now;
                Size size = SystemInformation.DoubleClickSize;
                clickArea = new Rectangle(e.X - (size.Width / 2), e.Y - (size.Height / 2), size.Width, size.Height);

                if (clicks == 3)
                {
                    clicks = 0;
                    selectLineOnMouseUp = true;
                    string text = textBox.Text;
                    int index = textBox.GetCharIndexFromPosition(e.Location);
                    selectionStart = index > 0 ? text.LastIndexOf('\n', index - 1) + 1 : 0;
                    int end = text.IndexOf('\n', index);
                    selectionLength = (end < 0 ? text.Length : end) - selectionStart;
                    textBox.Select(selectionStart, selectionLength);
                }
            };
            // Native mouse-up handling can replace the selection made on mouse-down.
            textBox.MouseUp += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                if (!clickArea.Contains(e.Location))
                {
                    clicks = 0;
                }
                else if (selectLineOnMouseUp)
                {
                    textBox.Select(selectionStart, selectionLength);
                }

                selectLineOnMouseUp = false;
            };
        }

        private sealed class DetailsRichTextBox : RichTextBox
        {
            private const int WmSetFocus = 0x0007;
            private const int WmLeftButtonDown = 0x0201;
            private const int WmLeftButtonUp = 0x0202;
            private const int MkControl = 0x0008;

            private readonly Func<Point, bool> isFolderPosition;
            private bool folderPressed;
            private Rectangle clickArea;

            public DetailsRichTextBox(Func<Point, bool> isFolderPosition)
            {
                this.isFolderPosition = isFolderPosition;
            }

            public event EventHandler FolderClicked;

            public void CancelFolderClick()
            {
                this.folderPressed = false;
                if (this.Capture)
                {
                    this.Capture = false;
                }
            }

            protected override void OnMouseCaptureChanged(EventArgs e)
            {
                if (!this.Capture)
                {
                    this.folderPressed = false;
                }

                base.OnMouseCaptureChanged(e);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmLeftButtonDown || m.Msg == WmLeftButtonUp)
                {
                    var location = new Point((short)m.LParam.ToInt64(), (short)(m.LParam.ToInt64() >> 16));
                    bool control = (m.WParam.ToInt64() & MkControl) != 0;
                    if (m.Msg == WmLeftButtonDown && control && this.isFolderPosition(location))
                    {
                        // Rich Edit handles Ctrl+click itself, so intercept the folder before native selection or scrolling.
                        this.folderPressed = true;
                        Size size = SystemInformation.DragSize;
                        this.clickArea = new Rectangle(
                            location.X - (size.Width / 2), location.Y - (size.Height / 2), size.Width, size.Height);
                        this.Capture = true;
                        m.Result = IntPtr.Zero;
                        return;
                    }

                    if (m.Msg == WmLeftButtonUp && this.folderPressed)
                    {
                        bool open = control && this.clickArea.Contains(location);
                        this.CancelFolderClick();
                        if (open)
                        {
                            this.FolderClicked?.Invoke(this, EventArgs.Empty);
                        }

                        m.Result = IntPtr.Zero;
                        return;
                    }
                }

                base.WndProc(ref m);
                if (m.Msg == WmSetFocus || this.Focused)
                {
                    SafeNativeMethods.HideCaret(new HandleRef(this, m.HWnd));
                }
            }
        }
    }
}
