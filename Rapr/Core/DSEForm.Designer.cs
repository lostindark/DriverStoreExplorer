namespace Rapr
{
    partial class DSEForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonEnumerate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxtMenuSelect = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxtMenuExport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxtMenuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonDeleteDriver = new System.Windows.Forms.Button();
            this.cbForceDeletion = new System.Windows.Forms.CheckBox();
            this.buttonAddDriver = new System.Windows.Forms.Button();
            this.cbAddInstall = new System.Windows.Forms.CheckBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.linkAbout = new System.Windows.Forms.LinkLabel();
            this.InnerContainer = new System.Windows.Forms.SplitContainer();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.lstDriverStoreEntries = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn6 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn5 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn4 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.groupBox1.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.InnerContainer.Panel1.SuspendLayout();
            this.InnerContainer.Panel2.SuspendLayout();
            this.InnerContainer.SuspendLayout();
            this.gbOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lstDriverStoreEntries)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonEnumerate
            // 
            this.buttonEnumerate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonEnumerate.Location = new System.Drawing.Point(11, 31);
            this.buttonEnumerate.Name = "buttonEnumerate";
            this.buttonEnumerate.Size = new System.Drawing.Size(109, 23);
            this.buttonEnumerate.TabIndex = 0;
            this.buttonEnumerate.Text = "Enumerate";
            this.buttonEnumerate.UseVisualStyleBackColor = true;
            this.buttonEnumerate.Click += new System.EventHandler(this.buttonEnumerate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstDriverStoreEntries);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(811, 626);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Driver Store";
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxtMenuSelect,
            this.ctxtMenuExport,
            this.toolStripSeparator1,
            this.ctxtMenuAbout});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(121, 76);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // ctxtMenuSelect
            // 
            this.ctxtMenuSelect.Name = "ctxtMenuSelect";
            this.ctxtMenuSelect.Size = new System.Drawing.Size(120, 22);
            this.ctxtMenuSelect.Text = "Select all";
            this.ctxtMenuSelect.Click += new System.EventHandler(this.ctxtMenuSelect_Click);
            // 
            // ctxtMenuExport
            // 
            this.ctxtMenuExport.Name = "ctxtMenuExport";
            this.ctxtMenuExport.Size = new System.Drawing.Size(120, 22);
            this.ctxtMenuExport.Text = "Export";
            this.ctxtMenuExport.Click += new System.EventHandler(this.ctxtMenuExport_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(117, 6);
            // 
            // ctxtMenuAbout
            // 
            this.ctxtMenuAbout.Name = "ctxtMenuAbout";
            this.ctxtMenuAbout.Size = new System.Drawing.Size(120, 22);
            this.ctxtMenuAbout.Text = "About";
            this.ctxtMenuAbout.Click += new System.EventHandler(this.ctxtMenuAbout_Click);
            // 
            // buttonDeleteDriver
            // 
            this.buttonDeleteDriver.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonDeleteDriver.Location = new System.Drawing.Point(11, 114);
            this.buttonDeleteDriver.Name = "buttonDeleteDriver";
            this.buttonDeleteDriver.Size = new System.Drawing.Size(109, 23);
            this.buttonDeleteDriver.TabIndex = 3;
            this.buttonDeleteDriver.Text = "Delete Package";
            this.buttonDeleteDriver.UseVisualStyleBackColor = true;
            this.buttonDeleteDriver.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // cbForceDeletion
            // 
            this.cbForceDeletion.AutoSize = true;
            this.cbForceDeletion.Location = new System.Drawing.Point(22, 143);
            this.cbForceDeletion.Name = "cbForceDeletion";
            this.cbForceDeletion.Size = new System.Drawing.Size(95, 17);
            this.cbForceDeletion.TabIndex = 4;
            this.cbForceDeletion.Text = "Force Deletion";
            this.cbForceDeletion.UseVisualStyleBackColor = true;
            // 
            // buttonAddDriver
            // 
            this.buttonAddDriver.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAddDriver.Location = new System.Drawing.Point(11, 60);
            this.buttonAddDriver.Name = "buttonAddDriver";
            this.buttonAddDriver.Size = new System.Drawing.Size(109, 23);
            this.buttonAddDriver.TabIndex = 5;
            this.buttonAddDriver.Text = "Add Package";
            this.buttonAddDriver.UseVisualStyleBackColor = true;
            this.buttonAddDriver.Click += new System.EventHandler(this.buttonAddDriver_Click);
            // 
            // cbAddInstall
            // 
            this.cbAddInstall.AutoSize = true;
            this.cbAddInstall.Location = new System.Drawing.Point(22, 89);
            this.cbAddInstall.Name = "cbAddInstall";
            this.cbAddInstall.Size = new System.Drawing.Size(84, 17);
            this.cbAddInstall.TabIndex = 6;
            this.cbAddInstall.Text = "Install Driver";
            this.cbAddInstall.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 626);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(945, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(39, 17);
            this.lblStatus.Text = "Ready";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "INF files | *.inf";
            this.openFileDialog.SupportMultiDottedExtensions = true;
            this.openFileDialog.Title = "Select the INF";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(10, 239);
            this.progressBar.MarqueeAnimationSpeed = 50;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(109, 19);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 10;
            this.progressBar.Visible = false;
            // 
            // linkAbout
            // 
            this.linkAbout.AutoSize = true;
            this.linkAbout.Location = new System.Drawing.Point(1, 192);
            this.linkAbout.Name = "linkAbout";
            this.linkAbout.Size = new System.Drawing.Size(129, 13);
            this.linkAbout.TabIndex = 12;
            this.linkAbout.TabStop = true;
            this.linkAbout.Text = "About DriverStoreExplorer";
            this.linkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAbout_LinkClicked);
            // 
            // InnerContainer
            // 
            this.InnerContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InnerContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.InnerContainer.IsSplitterFixed = true;
            this.InnerContainer.Location = new System.Drawing.Point(0, 0);
            this.InnerContainer.Name = "InnerContainer";
            // 
            // InnerContainer.Panel1
            // 
            this.InnerContainer.Panel1.Controls.Add(this.groupBox1);
            // 
            // InnerContainer.Panel2
            // 
            this.InnerContainer.Panel2.Controls.Add(this.progressBar);
            this.InnerContainer.Panel2.Controls.Add(this.gbOptions);
            this.InnerContainer.Panel2.Controls.Add(this.linkAbout);
            this.InnerContainer.Size = new System.Drawing.Size(945, 626);
            this.InnerContainer.SplitterDistance = 811;
            this.InnerContainer.SplitterWidth = 1;
            this.InnerContainer.TabIndex = 12;
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.cbForceDeletion);
            this.gbOptions.Controls.Add(this.buttonDeleteDriver);
            this.gbOptions.Controls.Add(this.cbAddInstall);
            this.gbOptions.Controls.Add(this.buttonAddDriver);
            this.gbOptions.Controls.Add(this.buttonEnumerate);
            this.gbOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbOptions.Location = new System.Drawing.Point(0, 0);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(133, 183);
            this.gbOptions.TabIndex = 13;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = " Operations ";
            // 
            // lstDriverStoreEntries
            // 
            this.lstDriverStoreEntries.AllowColumnReorder = true;
            this.lstDriverStoreEntries.AlternateRowBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lstDriverStoreEntries.CheckBoxes = true;
            this.lstDriverStoreEntries.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn6,
            this.olvColumn3,
            this.olvColumn2,
            this.olvColumn5,
            this.olvColumn4});
            this.lstDriverStoreEntries.ContextMenuStrip = this.contextMenuStrip;
            this.lstDriverStoreEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDriverStoreEntries.EmptyListMsg = "No entries loaded.";
            this.lstDriverStoreEntries.FullRowSelect = true;
            this.lstDriverStoreEntries.GridLines = true;
            this.lstDriverStoreEntries.HideSelection = false;
            this.lstDriverStoreEntries.Location = new System.Drawing.Point(3, 16);
            this.lstDriverStoreEntries.MultiSelect = false;
            this.lstDriverStoreEntries.Name = "lstDriverStoreEntries";
            this.lstDriverStoreEntries.ShowGroups = false;
            this.lstDriverStoreEntries.ShowItemToolTips = true;
            this.lstDriverStoreEntries.Size = new System.Drawing.Size(805, 607);
            this.lstDriverStoreEntries.TabIndex = 1;
            this.lstDriverStoreEntries.UseCompatibleStateImageBehavior = false;
            this.lstDriverStoreEntries.View = System.Windows.Forms.View.Details;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "driverPublishedName";
            this.olvColumn1.MinimumWidth = 40;
            this.olvColumn1.Text = "INF";
            this.olvColumn1.Width = 90;
            // 
            // olvColumn6
            // 
            this.olvColumn6.AspectName = "driverPkgProvider";
            this.olvColumn6.Text = "Pkg Provider";
            this.olvColumn6.Width = 150;
            // 
            // olvColumn3
            // 
            this.olvColumn3.AspectName = "driverClass";
            this.olvColumn3.Text = "Driver Class";
            this.olvColumn3.Width = 170;
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "driverDate";
            this.olvColumn2.Text = "Driver Date";
            this.olvColumn2.Width = 100;
            // 
            // olvColumn5
            // 
            this.olvColumn5.AspectName = "driverVersion";
            this.olvColumn5.Text = "Driver Version";
            this.olvColumn5.Width = 132;
            // 
            // olvColumn4
            // 
            this.olvColumn4.AspectName = "driverSignerName";
            this.olvColumn4.Text = "Driver Signer";
            this.olvColumn4.Width = 220;
            // 
            // DSEForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(945, 648);
            this.Controls.Add(this.InnerContainer);
            this.Controls.Add(this.statusStrip1);
            this.Name = "DSEForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Driver Store Explorer [RAPR]";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DSEForm_FormClosed);
            this.Shown += new System.EventHandler(this.DSEForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.contextMenuStrip.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.InnerContainer.Panel1.ResumeLayout(false);
            this.InnerContainer.Panel2.ResumeLayout(false);
            this.InnerContainer.Panel2.PerformLayout();
            this.InnerContainer.ResumeLayout(false);
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lstDriverStoreEntries)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonEnumerate;
        private BrightIdeasSoftware.ObjectListView lstDriverStoreEntries;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
        private BrightIdeasSoftware.OLVColumn olvColumn3;
        private BrightIdeasSoftware.OLVColumn olvColumn4;
        private BrightIdeasSoftware.OLVColumn olvColumn5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonDeleteDriver;
        private System.Windows.Forms.CheckBox cbForceDeletion;
        private BrightIdeasSoftware.OLVColumn olvColumn6;
        private System.Windows.Forms.Button buttonAddDriver;
        private System.Windows.Forms.CheckBox cbAddInstall;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.LinkLabel linkAbout;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ctxtMenuSelect;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ctxtMenuAbout;
        private System.Windows.Forms.SplitContainer InnerContainer;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.ToolStripMenuItem ctxtMenuExport;
    }
}

