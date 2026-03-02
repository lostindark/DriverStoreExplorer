namespace Rapr.Controls
{
    partial class EnhancedSearchTextBox
    {
        /// <summary>
        /// Internal controls
        /// </summary>
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.PictureBox searchIcon;
        private System.Windows.Forms.PictureBox clearButton;
        private System.Windows.Forms.Label placeholderLabel;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.clearButton?.Image?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnhancedSearchTextBox));
            this.textBox = new System.Windows.Forms.TextBox();
            this.searchIcon = new System.Windows.Forms.PictureBox();
            this.clearButton = new System.Windows.Forms.PictureBox();
            this.placeholderLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.searchIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.clearButton)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox.Location = new System.Drawing.Point(28, 0);
            this.textBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(248, 19);
            this.textBox.TabIndex = 0;
            // 
            // searchIcon
            // 
            this.searchIcon.Image = ((System.Drawing.Image)(resources.GetObject("searchIcon.Image")));
            this.searchIcon.Location = new System.Drawing.Point(4, 2);
            this.searchIcon.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.searchIcon.Name = "searchIcon";
            this.searchIcon.Size = new System.Drawing.Size(20, 20);
            this.searchIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.searchIcon.TabIndex = 1;
            this.searchIcon.TabStop = false;
            // 
            // clearButton
            // 
            this.clearButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.clearButton.Image = ((System.Drawing.Image)(resources.GetObject("clearButton.Image")));
            this.clearButton.Location = new System.Drawing.Point(266, 2);
            this.clearButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(20, 20);
            this.clearButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.clearButton.TabIndex = 2;
            this.clearButton.TabStop = false;
            this.clearButton.Visible = false;
            // 
            // placeholderLabel
            // 
            this.placeholderLabel.BackColor = System.Drawing.Color.Transparent;
            this.placeholderLabel.ForeColor = System.Drawing.Color.Gray;
            this.placeholderLabel.Location = new System.Drawing.Point(28, 0);
            this.placeholderLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.placeholderLabel.Name = "placeholderLabel";
            this.placeholderLabel.Size = new System.Drawing.Size(248, 24);
            this.placeholderLabel.TabIndex = 3;
            this.placeholderLabel.Text = "";
            this.placeholderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // EnhancedSearchTextBox
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.searchIcon);
            this.Controls.Add(this.placeholderLabel);
            this.Controls.Add(this.textBox);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "EnhancedSearchTextBox";
            this.Size = new System.Drawing.Size(304, 24);
            ((System.ComponentModel.ISupportInitialize)(this.searchIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.clearButton)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}