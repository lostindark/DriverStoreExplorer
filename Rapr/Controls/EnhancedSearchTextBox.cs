using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Rapr.Properties;

namespace Rapr.Controls
{
    /// <summary>
    /// Enhanced search textbox with search icon, placeholder text overlay, and clear button
    /// </summary>
    [DesignerCategory("UserControl")]
    [ToolboxItem(true)]
    public partial class EnhancedSearchTextBox : UserControl
    {
        private const int DesignHeight = 24;
        private const int DesignIconSize = 16;
        private const int DesignPadding = 4;

        public EnhancedSearchTextBox()
        {
            this.InitializeComponent();
            this.SetupEventHandlers();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.SetupControl();
        }

        private float ScaleFactor => (float)this.Height / DesignHeight;

        private void SetupEventHandlers()
        {
            // Set up event handlers for the components
            this.textBox.TextChanged += this.TextBox_TextChanged;
            this.textBox.Enter += this.TextBox_Enter;
            this.textBox.Leave += this.TextBox_Leave;
            this.textBox.KeyDown += this.TextBox_KeyDown;

            this.clearButton.Click += this.ClearButton_Click;
            this.clearButton.MouseEnter += this.ClearButton_MouseEnter;
            this.clearButton.MouseLeave += this.ClearButton_MouseLeave;

            this.placeholderLabel.Click += this.PlaceholderLabel_Click;
            this.placeholderLabel.MouseDown += this.PlaceholderLabel_MouseDown;

            this.Paint += this.EnhancedSearchTextBox_Paint;
            this.Resize += this.EnhancedSearchTextBox_Resize;
            this.RightToLeftChanged += (s, ev) => this.EnhancedSearchTextBox_Resize(s, ev);
        }

        private void SetupControl()
        {
            if (this.DesignMode)
            {
                return;
            }

            // Update placeholder visibility
            this.UpdatePlaceholderVisibility();

            // Call resize method initially to set correct positions
            this.EnhancedSearchTextBox_Resize(this, EventArgs.Empty);
        }

        private void UpdatePlaceholderVisibility()
        {
            if (this.placeholderLabel != null && this.textBox != null)
            {
                this.placeholderLabel.Visible = string.IsNullOrWhiteSpace(this.textBox.Text);
            }
        }

        private void EnhancedSearchTextBox_Resize(object sender, EventArgs e)
        {
            if (this.textBox == null || this.placeholderLabel == null || this.clearButton == null)
            {
                return;
            }

            float scale = this.ScaleFactor;
            int iconSize = (int)(DesignIconSize * scale);
            int padding = (int)(DesignPadding * scale);
            bool isRtl = this.RightToLeft == RightToLeft.Yes;

            // Scale icon sizes
            this.searchIcon.Size = new Size(iconSize, iconSize);
            this.clearButton.Size = new Size(iconSize, iconSize);

            int iconAreaWidth = iconSize + padding * 2;
            int clearAreaWidth = iconSize + padding * 2;
            int availableWidth = this.Width - iconAreaWidth - clearAreaWidth;

            // Position and size the textBox
            int textBoxY = Math.Max(0, (this.Height - this.textBox.PreferredHeight) / 2);
            int textBoxX = isRtl ? clearAreaWidth : iconAreaWidth;
            this.textBox.Width = availableWidth;
            this.textBox.Location = new Point(textBoxX, textBoxY);

            // Position and size the placeholder
            this.placeholderLabel.Width = availableWidth;
            this.placeholderLabel.Height = this.Height - 2;
            this.placeholderLabel.Location = new Point(textBoxX, 0);
            this.placeholderLabel.TextAlign = isRtl
                ? ContentAlignment.MiddleRight
                : ContentAlignment.MiddleLeft;

            // Vertically center icons
            int iconY = Math.Max(0, (this.Height - iconSize) / 2);

            // In RTL: search icon on right, clear button on left
            int searchIconX = isRtl ? this.Width - iconSize - padding : padding;
            int clearButtonX = isRtl ? padding : this.Width - iconSize - padding;
            this.searchIcon.Location = new Point(searchIconX, iconY);
            this.clearButton.Location = new Point(clearButtonX, iconY);
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            // Hide placeholder when textbox gets focus
            if (this.placeholderLabel != null)
            {
                this.placeholderLabel.Visible = false;
            }
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            // Show placeholder if textbox is empty when it loses focus
            this.UpdatePlaceholderVisibility();
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox == null || this.clearButton == null)
            {
                return;
            }

            bool hasText = !string.IsNullOrWhiteSpace(this.textBox.Text);
            this.clearButton.Visible = hasText;

            // Update placeholder visibility
            this.UpdatePlaceholderVisibility();

            // Raise the TextChanged event
            this.OnTextChanged(EventArgs.Empty);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.ClearText();
            }
        }

        private void PlaceholderLabel_Click(object sender, EventArgs e)
        {
            // When placeholder is clicked, hide it and focus the textbox
            if (this.textBox != null)
            {
                this.placeholderLabel.Visible = false;
                this.textBox.Focus();
            }
        }

        private void PlaceholderLabel_MouseDown(object sender, MouseEventArgs e)
        {
            // When placeholder is clicked, hide it and focus the textbox
            if (this.textBox != null)
            {
                this.placeholderLabel.Visible = false;
                this.textBox.Focus();
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            this.ClearText();
        }

        private void ClearButton_MouseEnter(object sender, EventArgs e)
        {
            if (this.DesignMode || this.clearButton == null)
            {
                return;
            }

            try
            {
                if (Resources.Clear != null)
                {
                    var oldImage = this.clearButton.Image;
                    this.clearButton.Image = Resources.Clear.ToBitmap();
                    oldImage?.Dispose();
                    return;
                }
            }
            catch
            {
            }

            var oldImg = this.clearButton.Image;
            this.clearButton.Image = this.CreateXBitmap(Color.Black);
            oldImg?.Dispose();
        }

        private void ClearButton_MouseLeave(object sender, EventArgs e)
        {
            if (this.DesignMode || this.clearButton == null)
            {
                return;
            }

            var oldImage = this.clearButton.Image;
            this.clearButton.Image = this.GetClearButtonIcon();
            oldImage?.Dispose();
        }

        /// <summary>
        /// Gets the clear button icon
        /// </summary>
        private Image GetClearButtonIcon()
        {
            if (this.DesignMode) return null;

            try
            {
                if (Resources.Clear != null)
                {
                    return Resources.Clear.ToBitmap();
                }
            }
            catch
            {
            }

            return this.CreateXBitmap(Color.Gray);
        }

        private Bitmap CreateXBitmap(Color color)
        {
            int size = Math.Max(16, this.clearButton.Width);
            int margin = size / 4;
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                float penWidth = Math.Max(1f, size / 8f);
                using (var pen = new Pen(color, penWidth))
                {
                    g.DrawLine(pen, margin, margin, size - margin, size - margin);
                    g.DrawLine(pen, size - margin, margin, margin, size - margin);
                }
            }
            return bitmap;
        }

        private void ClearText()
        {
            if (this.textBox == null || this.clearButton == null)
            {
                return;
            }

            this.textBox.Text = "";
            this.clearButton.Visible = false;
            this.placeholderLabel.Visible = false;
            this.textBox.Focus();
        }

        private void EnhancedSearchTextBox_Paint(object sender, PaintEventArgs e)
        {
            float penWidth = Math.Max(1f, this.ScaleFactor);
            using (var pen = new Pen(Color.Gray, penWidth))
            {
                Rectangle borderRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                e.Graphics.DrawRectangle(pen, borderRect);
            }
        }

        // Public properties and events to mimic TextBox behavior
        [Browsable(false)]
        public override string Text
        {
            get
            {
                return this.textBox?.Text ?? "";
            }
            set
            {
                if (this.textBox != null)
                {
                    this.textBox.Text = value ?? "";
                    this.UpdatePlaceholderVisibility();
                    if (this.clearButton != null)
                    {
                        this.clearButton.Visible = !string.IsNullOrWhiteSpace(this.textBox.Text);
                    }
                }
            }
        }

        [Browsable(true)]
        public string PlaceholderText
        {
            get { return this.placeholderLabel?.Text ?? ""; }
            set
            {
                if (this.placeholderLabel != null)
                {
                    this.placeholderLabel.Text = value ?? "";
                }
            }
        }

        public new event EventHandler TextChanged;

        protected override void OnTextChanged(EventArgs e)
        {
            TextChanged?.Invoke(this, e);
            base.OnTextChanged(e);
        }

        public new bool Focus()
        {
            return this.textBox?.Focus() ?? false;
        }

        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                if (this.textBox != null)
                {
                    this.textBox.Enabled = value;
                }

                if (this.searchIcon != null)
                {
                    this.searchIcon.Enabled = value;
                }

                if (this.clearButton != null)
                {
                    this.clearButton.Enabled = value;
                }

                if (this.placeholderLabel != null)
                {
                    this.placeholderLabel.Enabled = value;
                }
            }
        }
    }
}