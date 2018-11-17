using Rapr.Core.lang;

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
            this.lstDriverStoreEntries = new BrightIdeasSoftware.ObjectListView();
            this.driverInfColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverOemInfColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverClassColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverProviderColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverVersionColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverDateColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverSizeColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverSignerColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxMenuSelect = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuSelectOldDrivers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxMenuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonDeleteDriver = new System.Windows.Forms.Button();
            this.cbForceDeletion = new System.Windows.Forms.CheckBox();
            this.buttonAddDriver = new System.Windows.Forms.Button();
            this.cbAddInstall = new System.Windows.Forms.CheckBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.InnerContainer = new System.Windows.Forms.SplitContainer();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.buttonSelectOldDrivers = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLogsTtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runAsAdminToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysRunAsAdminToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lstDriverStoreEntries)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InnerContainer)).BeginInit();
            this.InnerContainer.Panel1.SuspendLayout();
            this.InnerContainer.Panel2.SuspendLayout();
            this.InnerContainer.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonEnumerate
            // 
            this.buttonEnumerate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonEnumerate.Location = new System.Drawing.Point(9, 19);
            this.buttonEnumerate.Name = "buttonEnumerate";
            this.buttonEnumerate.Size = new System.Drawing.Size(109, 23);
            this.buttonEnumerate.TabIndex = 0;
            this.buttonEnumerate.Text = global::Rapr.Core.lang.Language.Button_Refresh;
            this.buttonEnumerate.UseVisualStyleBackColor = true;
            this.buttonEnumerate.Click += new System.EventHandler(this.ButtonEnumerate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstDriverStoreEntries);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(800, 583);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = global::Rapr.Core.lang.Language.Group_DriverStore;
            // 
            // lstDriverStoreEntries
            // 
            this.lstDriverStoreEntries.AllColumns.Add(this.driverInfColumn);
            this.lstDriverStoreEntries.AllColumns.Add(this.driverOemInfColumn);
            this.lstDriverStoreEntries.AllColumns.Add(this.driverClassColumn);
            this.lstDriverStoreEntries.AllColumns.Add(this.driverProviderColumn);
            this.lstDriverStoreEntries.AllColumns.Add(this.driverVersionColumn);
            this.lstDriverStoreEntries.AllColumns.Add(this.driverDateColumn);
            this.lstDriverStoreEntries.AllColumns.Add(this.driverSizeColumn);
            this.lstDriverStoreEntries.AllColumns.Add(this.driverSignerColumn);
            this.lstDriverStoreEntries.AllowColumnReorder = true;
            this.lstDriverStoreEntries.AlternateRowBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lstDriverStoreEntries.CellEditUseWholeCell = false;
            this.lstDriverStoreEntries.CheckBoxes = true;
            this.lstDriverStoreEntries.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.driverInfColumn,
            this.driverClassColumn,
            this.driverProviderColumn,
            this.driverVersionColumn,
            this.driverDateColumn,
            this.driverSizeColumn});
            this.lstDriverStoreEntries.ContextMenuStrip = this.contextMenuStrip;
            this.lstDriverStoreEntries.Cursor = System.Windows.Forms.Cursors.Default;
            this.lstDriverStoreEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDriverStoreEntries.EmptyListMsg = global::Rapr.Core.lang.Language.Message_No_Entries;
            this.lstDriverStoreEntries.FullRowSelect = true;
            this.lstDriverStoreEntries.GridLines = true;
            this.lstDriverStoreEntries.HideSelection = false;
            this.lstDriverStoreEntries.Location = new System.Drawing.Point(3, 16);
            this.lstDriverStoreEntries.Name = "lstDriverStoreEntries";
            this.lstDriverStoreEntries.ShowItemToolTips = true;
            this.lstDriverStoreEntries.Size = new System.Drawing.Size(794, 564);
            this.lstDriverStoreEntries.SortGroupItemsByPrimaryColumn = false;
            this.lstDriverStoreEntries.TabIndex = 1;
            this.lstDriverStoreEntries.UseCompatibleStateImageBehavior = false;
            this.lstDriverStoreEntries.View = System.Windows.Forms.View.Details;
            this.lstDriverStoreEntries.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.LstDriverStoreEntries_ItemChecked);
            // 
            // driverInfColumn
            // 
            this.driverInfColumn.AspectName = "DriverInfName";
            this.driverInfColumn.Text = global::Rapr.Core.lang.Language.Column_Inf;
            this.driverInfColumn.UseInitialLetterForGroup = true;
            this.driverInfColumn.Width = 120;
            // 
            // driverOemInfColumn
            // 
            this.driverOemInfColumn.AspectName = "DriverPublishedName";
            this.driverOemInfColumn.IsVisible = false;
            this.driverOemInfColumn.Text = global::Rapr.Core.lang.Language.Column_Oem_Inf;
            this.driverOemInfColumn.Width = 90;
            // 
            // driverClassColumn
            // 
            this.driverClassColumn.AspectName = "DriverClass";
            this.driverClassColumn.Text = global::Rapr.Core.lang.Language.Column_Driver_Class;
            this.driverClassColumn.Width = 170;
            // 
            // driverProviderColumn
            // 
            this.driverProviderColumn.AspectName = "DriverPkgProvider";
            this.driverProviderColumn.Text = global::Rapr.Core.lang.Language.Column_Provider;
            this.driverProviderColumn.Width = 160;
            // 
            // driverVersionColumn
            // 
            this.driverVersionColumn.AspectName = "DriverVersion";
            this.driverVersionColumn.Text = global::Rapr.Core.lang.Language.Column_Version;
            this.driverVersionColumn.Width = 110;
            // 
            // driverDateColumn
            // 
            this.driverDateColumn.AspectName = "DriverDate";
            this.driverDateColumn.AspectToStringFormat = global::Rapr.Core.lang.Language.Date_Format;
            this.driverDateColumn.Text = global::Rapr.Core.lang.Language.Column_Date;
            this.driverDateColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.driverDateColumn.Width = 80;
            // 
            // driverSizeColumn
            // 
            this.driverSizeColumn.AspectName = "DriverSize";
            this.driverSizeColumn.Text = global::Rapr.Core.lang.Language.Column_Size;
            this.driverSizeColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.driverSizeColumn.Width = 120;
            // 
            // driverSignerColumn
            // 
            this.driverSignerColumn.AspectName = "DriverSignerName";
            this.driverSignerColumn.IsVisible = false;
            this.driverSignerColumn.Text = global::Rapr.Core.lang.Language.Column_Signed_By;
            this.driverSignerColumn.Width = 250;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxMenuSelect,
            this.ctxMenuSelectAll,
            this.ctxMenuSelectOldDrivers,
            this.toolStripSeparator1,
            this.ctxMenuDelete});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(167, 98);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip_Opening);
            // 
            // ctxMenuSelect
            // 
            this.ctxMenuSelect.Name = "ctxMenuSelect";
            this.ctxMenuSelect.Size = new System.Drawing.Size(166, 22);
            this.ctxMenuSelect.Text = global::Rapr.Core.lang.Language.Context_Select;
            this.ctxMenuSelect.Click += new System.EventHandler(this.CtxMenuSelect_Click);
            // 
            // ctxMenuSelectAll
            // 
            this.ctxMenuSelectAll.Name = "ctxMenuSelectAll";
            this.ctxMenuSelectAll.Size = new System.Drawing.Size(166, 22);
            this.ctxMenuSelectAll.Text = global::Rapr.Core.lang.Language.Context_Select_All;
            this.ctxMenuSelectAll.Click += new System.EventHandler(this.CtxMenuSelectAll_Click);
            // 
            // ctxMenuSelectOldDrivers
            // 
            this.ctxMenuSelectOldDrivers.Name = "ctxMenuSelectOldDrivers";
            this.ctxMenuSelectOldDrivers.Size = new System.Drawing.Size(166, 22);
            this.ctxMenuSelectOldDrivers.Text = global::Rapr.Core.lang.Language.Context_Select_Old;
            this.ctxMenuSelectOldDrivers.Click += new System.EventHandler(this.CtxMenuSelectOldDrivers_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(163, 6);
            // 
            // ctxMenuDelete
            // 
            this.ctxMenuDelete.Name = "ctxMenuDelete";
            this.ctxMenuDelete.Size = new System.Drawing.Size(166, 22);
            this.ctxMenuDelete.Text = global::Rapr.Core.lang.Language.Context_Delete;
            this.ctxMenuDelete.Click += new System.EventHandler(this.CtxMenuDelete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // buttonDeleteDriver
            // 
            this.buttonDeleteDriver.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonDeleteDriver.Location = new System.Drawing.Point(9, 102);
            this.buttonDeleteDriver.Name = "buttonDeleteDriver";
            this.buttonDeleteDriver.Size = new System.Drawing.Size(109, 23);
            this.buttonDeleteDriver.TabIndex = 3;
            this.buttonDeleteDriver.Text = global::Rapr.Core.lang.Language.Button_Delete_Package;
            this.buttonDeleteDriver.UseVisualStyleBackColor = true;
            this.buttonDeleteDriver.Click += new System.EventHandler(this.ButtonDelete_Click);
            // 
            // cbForceDeletion
            // 
            this.cbForceDeletion.AutoSize = true;
            this.cbForceDeletion.Location = new System.Drawing.Point(9, 131);
            this.cbForceDeletion.Name = "cbForceDeletion";
            this.cbForceDeletion.Size = new System.Drawing.Size(95, 17);
            this.cbForceDeletion.TabIndex = 4;
            this.cbForceDeletion.Text = global::Rapr.Core.lang.Language.Check_Force_Delete;
            this.cbForceDeletion.UseVisualStyleBackColor = true;
            // 
            // buttonAddDriver
            // 
            this.buttonAddDriver.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAddDriver.Location = new System.Drawing.Point(9, 48);
            this.buttonAddDriver.Name = "buttonAddDriver";
            this.buttonAddDriver.Size = new System.Drawing.Size(109, 23);
            this.buttonAddDriver.TabIndex = 5;
            this.buttonAddDriver.Text = global::Rapr.Core.lang.Language.Button_Add_Package;
            this.buttonAddDriver.UseVisualStyleBackColor = true;
            this.buttonAddDriver.Click += new System.EventHandler(this.ButtonAddDriver_Click);
            // 
            // cbAddInstall
            // 
            this.cbAddInstall.AutoSize = true;
            this.cbAddInstall.Location = new System.Drawing.Point(9, 79);
            this.cbAddInstall.Name = "cbAddInstall";
            this.cbAddInstall.Size = new System.Drawing.Size(84, 17);
            this.cbAddInstall.TabIndex = 6;
            this.cbAddInstall.Text = global::Rapr.Core.lang.Language.Check_Install_Driver;
            this.cbAddInstall.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(2, 608);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(933, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.MarqueeAnimationSpeed = 50;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.toolStripProgressBar1.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(918, 17);
            this.lblStatus.Spring = true;
            this.lblStatus.Text = global::Rapr.Core.lang.Language.Status_Label;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = global::Rapr.Core.lang.Language.Dialog_Open_Filters;
            this.openFileDialog.SupportMultiDottedExtensions = true;
            this.openFileDialog.Title = global::Rapr.Core.lang.Language.Dialog_Open_Filter_Text;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1_RunWorkerCompleted);
            // 
            // InnerContainer
            // 
            this.InnerContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.InnerContainer.IsSplitterFixed = true;
            this.InnerContainer.Location = new System.Drawing.Point(5, 24);
            this.InnerContainer.Name = "InnerContainer";
            // 
            // InnerContainer.Panel1
            // 
            this.InnerContainer.Panel1.Controls.Add(this.groupBox1);
            // 
            // InnerContainer.Panel2
            // 
            this.InnerContainer.Panel2.Controls.Add(this.gbOptions);
            this.InnerContainer.Size = new System.Drawing.Size(925, 583);
            this.InnerContainer.SplitterDistance = 800;
            this.InnerContainer.SplitterWidth = 1;
            this.InnerContainer.TabIndex = 12;
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.buttonSelectOldDrivers);
            this.gbOptions.Controls.Add(this.cbForceDeletion);
            this.gbOptions.Controls.Add(this.buttonDeleteDriver);
            this.gbOptions.Controls.Add(this.cbAddInstall);
            this.gbOptions.Controls.Add(this.buttonAddDriver);
            this.gbOptions.Controls.Add(this.buttonEnumerate);
            this.gbOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbOptions.Location = new System.Drawing.Point(0, 0);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(124, 196);
            this.gbOptions.TabIndex = 13;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = global::Rapr.Core.lang.Language.Operations_Text;
            // 
            // buttonSelectOldDrivers
            // 
            this.buttonSelectOldDrivers.Location = new System.Drawing.Point(9, 154);
            this.buttonSelectOldDrivers.Name = "buttonSelectOldDrivers";
            this.buttonSelectOldDrivers.Size = new System.Drawing.Size(109, 23);
            this.buttonSelectOldDrivers.TabIndex = 7;
            this.buttonSelectOldDrivers.Text = global::Rapr.Core.lang.Language.Button_Select_Old;
            this.buttonSelectOldDrivers.UseVisualStyleBackColor = true;
            this.buttonSelectOldDrivers.Click += new System.EventHandler(this.ButtonSelectOldDrivers_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionToolStripMenuItem,
            this.languageToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(2, 2);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(933, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.viewLogsTtoolStripMenuItem,
            this.runAsAdminToolStripMenuItem,
            this.toolStripSeparator4,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = global::Rapr.Core.lang.Language.Menu_File;
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.exportToolStripMenuItem.Text = global::Rapr.Core.lang.Language.Menu_File_Export;
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // viewLogsTtoolStripMenuItem
            // 
            this.viewLogsTtoolStripMenuItem.Name = "viewLogsTtoolStripMenuItem";
            this.viewLogsTtoolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.viewLogsTtoolStripMenuItem.Text = global::Rapr.Core.lang.Language.Menu_File_View_Logs;
            this.viewLogsTtoolStripMenuItem.Click += new System.EventHandler(this.ViewLogsToolStripMenuItem_Click);
            // 
            // runAsAdminToolStripMenuItem
            // 
            this.runAsAdminToolStripMenuItem.Enabled = false;
            this.runAsAdminToolStripMenuItem.Name = "runAsAdminToolStripMenuItem";
            this.runAsAdminToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.runAsAdminToolStripMenuItem.Text = global::Rapr.Core.lang.Language.Menu_File_RunAs_Admin;
            this.runAsAdminToolStripMenuItem.Click += new System.EventHandler(this.RunAsAdminToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(147, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.exitToolStripMenuItem.Text = global::Rapr.Core.lang.Language.Menu_File_Exit;
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // optionToolStripMenuItem
            // 
            this.optionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alwaysRunAsAdminToolStripMenuItem});
            this.optionToolStripMenuItem.Name = "optionToolStripMenuItem";
            this.optionToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionToolStripMenuItem.Text = global::Rapr.Core.lang.Language.Menu_Options;
            // 
            // alwaysRunAsAdminToolStripMenuItem
            // 
            this.alwaysRunAsAdminToolStripMenuItem.CheckOnClick = true;
            this.alwaysRunAsAdminToolStripMenuItem.Name = "alwaysRunAsAdminToolStripMenuItem";
            this.alwaysRunAsAdminToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.alwaysRunAsAdminToolStripMenuItem.Text = global::Rapr.Core.lang.Language.Menu_Option_Always_RunAs_Admin;
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            this.languageToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.languageToolStripMenuItem.Text = "Language";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem1});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = global::Rapr.Core.lang.Language.Menu_Help;
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem1.Text = global::Rapr.Core.lang.Language.Menu_Help_About;
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // DSEForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(937, 632);
            this.Controls.Add(this.InnerContainer);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(480, 360);
            this.Name = "DSEForm";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Driver Store Explorer [RAPR]";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DSEForm_FormClosed);
            this.Shown += new System.EventHandler(this.DSEForm_Shown);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lstDriverStoreEntries)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.InnerContainer.Panel1.ResumeLayout(false);
            this.InnerContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.InnerContainer)).EndInit();
            this.InnerContainer.ResumeLayout(false);
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonEnumerate;
        private BrightIdeasSoftware.ObjectListView lstDriverStoreEntries;
        private BrightIdeasSoftware.OLVColumn driverOemInfColumn;
        private BrightIdeasSoftware.OLVColumn driverDateColumn;
        private BrightIdeasSoftware.OLVColumn driverClassColumn;
        private BrightIdeasSoftware.OLVColumn driverSignerColumn;
        private BrightIdeasSoftware.OLVColumn driverVersionColumn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonDeleteDriver;
        private System.Windows.Forms.CheckBox cbForceDeletion;
        private BrightIdeasSoftware.OLVColumn driverProviderColumn;
        private System.Windows.Forms.Button buttonAddDriver;
        private System.Windows.Forms.CheckBox cbAddInstall;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ctxMenuSelectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.SplitContainer InnerContainer;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.ToolStripMenuItem ctxMenuDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ctxMenuSelect;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private BrightIdeasSoftware.OLVColumn driverInfColumn;
        private BrightIdeasSoftware.OLVColumn driverSizeColumn;
        private System.Windows.Forms.Button buttonSelectOldDrivers;
        private System.Windows.Forms.ToolStripMenuItem ctxMenuSelectOldDrivers;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem viewLogsTtoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem runAsAdminToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysRunAsAdminToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}

