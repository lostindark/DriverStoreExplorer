
namespace Rapr
{
    partial class DSEForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonEnumerate = new System.Windows.Forms.Button();
            this.driverOemInfColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverSignerColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxMenuSelect = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenuSelectOldDrivers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxMenuOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ctxMenuExportDriver = new System.Windows.Forms.ToolStripMenuItem();
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
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonSelectOldDrivers = new System.Windows.Forms.Button();
            this.buttonExportDrivers = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chooseDriverStoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllDriversToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLogsTtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lstDriverStoreEntries = new Rapr.MyObjectListView();
            this.driverInfColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverClassColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverProviderColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverVersionColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverDateColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.driverSizeColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.deviceNameColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.bootCriticalColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.dummyColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.contextMenuStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lstDriverStoreEntries)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonEnumerate
            // 
            this.buttonEnumerate.AutoSize = true;
            this.buttonEnumerate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonEnumerate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonEnumerate.Location = new System.Drawing.Point(9, 9);
            this.buttonEnumerate.Name = "buttonEnumerate";
            this.buttonEnumerate.Size = new System.Drawing.Size(124, 23);
            this.buttonEnumerate.TabIndex = 0;
            this.buttonEnumerate.Text = global::Rapr.Lang.Language.Button_Refresh;
            this.buttonEnumerate.UseVisualStyleBackColor = true;
            this.buttonEnumerate.Click += new System.EventHandler(this.ButtonEnumerate_Click);
            // 
            // driverOemInfColumn
            // 
            this.driverOemInfColumn.AspectName = "DriverPublishedName";
            this.driverOemInfColumn.IsVisible = false;
            this.driverOemInfColumn.Text = global::Rapr.Lang.Language.Column_Oem_Inf;
            this.driverOemInfColumn.Width = 90;
            // 
            // driverSignerColumn
            // 
            this.driverSignerColumn.AspectName = "DriverSignerName";
            this.driverSignerColumn.IsVisible = false;
            this.driverSignerColumn.Text = global::Rapr.Lang.Language.Column_Signed_By;
            this.driverSignerColumn.Width = 250;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxMenuSelect,
            this.ctxMenuSelectAll,
            this.ctxMenuSelectOldDrivers,
            this.toolStripSeparator1,
            this.ctxMenuOpenFolder,
            this.toolStripSeparator3,
            this.ctxMenuExportDriver,
            this.ctxMenuDelete});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(189, 170);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip_Opening);
            // 
            // ctxMenuSelect
            // 
            this.ctxMenuSelect.Name = "ctxMenuSelect";
            this.ctxMenuSelect.Size = new System.Drawing.Size(188, 22);
            this.ctxMenuSelect.Text = global::Rapr.Lang.Language.Context_Select;
            this.ctxMenuSelect.Click += new System.EventHandler(this.CtxMenuSelect_Click);
            // 
            // ctxMenuSelectAll
            // 
            this.ctxMenuSelectAll.Name = "ctxMenuSelectAll";
            this.ctxMenuSelectAll.Size = new System.Drawing.Size(188, 22);
            this.ctxMenuSelectAll.Text = global::Rapr.Lang.Language.Context_Select_All;
            this.ctxMenuSelectAll.Click += new System.EventHandler(this.CtxMenuSelectAll_Click);
            // 
            // ctxMenuSelectOldDrivers
            // 
            this.ctxMenuSelectOldDrivers.Name = "ctxMenuSelectOldDrivers";
            this.ctxMenuSelectOldDrivers.Size = new System.Drawing.Size(188, 22);
            this.ctxMenuSelectOldDrivers.Text = global::Rapr.Lang.Language.Context_Select_Old;
            this.ctxMenuSelectOldDrivers.Click += new System.EventHandler(this.CtxMenuSelectOldDrivers_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(185, 6);
            // 
            // ctxMenuOpenFolder
            // 
            this.ctxMenuOpenFolder.Name = "ctxMenuOpenFolder";
            this.ctxMenuOpenFolder.Size = new System.Drawing.Size(188, 22);
            this.ctxMenuOpenFolder.Text = global::Rapr.Lang.Language.Context_Open_Folder;
            this.ctxMenuOpenFolder.Click += new System.EventHandler(this.CtxMenuOpenFolder_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(185, 6);
            // 
            // ctxMenuExportDriver
            // 
            this.ctxMenuExportDriver.Name = "ctxMenuExportDriver";
            this.ctxMenuExportDriver.Size = new System.Drawing.Size(188, 22);
            this.ctxMenuExportDriver.Text = global::Rapr.Lang.Language.Context_Export_Driver;
            // 
            // ctxMenuDelete
            // 
            this.ctxMenuDelete.Name = "ctxMenuDelete";
            this.ctxMenuDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.ctxMenuDelete.Size = new System.Drawing.Size(188, 22);
            this.ctxMenuDelete.Text = global::Rapr.Lang.Language.Context_Delete;
            this.ctxMenuDelete.Click += new System.EventHandler(this.CtxMenuDelete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // buttonDeleteDriver
            // 
            this.buttonDeleteDriver.AutoSize = true;
            this.buttonDeleteDriver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonDeleteDriver.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonDeleteDriver.Location = new System.Drawing.Point(9, 90);
            this.buttonDeleteDriver.Name = "buttonDeleteDriver";
            this.buttonDeleteDriver.Size = new System.Drawing.Size(124, 23);
            this.buttonDeleteDriver.TabIndex = 3;
            this.buttonDeleteDriver.Text = global::Rapr.Lang.Language.Button_Delete_Package;
            this.buttonDeleteDriver.UseVisualStyleBackColor = true;
            this.buttonDeleteDriver.Click += new System.EventHandler(this.ButtonDelete_Click);
            // 
            // cbForceDeletion
            // 
            this.cbForceDeletion.AutoSize = true;
            this.cbForceDeletion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbForceDeletion.Location = new System.Drawing.Point(9, 119);
            this.cbForceDeletion.Name = "cbForceDeletion";
            this.cbForceDeletion.Size = new System.Drawing.Size(124, 17);
            this.cbForceDeletion.TabIndex = 4;
            this.cbForceDeletion.Text = global::Rapr.Lang.Language.Check_Force_Delete;
            this.cbForceDeletion.UseVisualStyleBackColor = true;
            // 
            // buttonAddDriver
            // 
            this.buttonAddDriver.AutoSize = true;
            this.buttonAddDriver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonAddDriver.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAddDriver.Location = new System.Drawing.Point(9, 38);
            this.buttonAddDriver.Name = "buttonAddDriver";
            this.buttonAddDriver.Size = new System.Drawing.Size(124, 23);
            this.buttonAddDriver.TabIndex = 1;
            this.buttonAddDriver.Text = global::Rapr.Lang.Language.Button_Add_Package;
            this.buttonAddDriver.UseVisualStyleBackColor = true;
            this.buttonAddDriver.Click += new System.EventHandler(this.ButtonAddDriver_Click);
            // 
            // cbAddInstall
            // 
            this.cbAddInstall.AutoSize = true;
            this.cbAddInstall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbAddInstall.Location = new System.Drawing.Point(9, 67);
            this.cbAddInstall.Name = "cbAddInstall";
            this.cbAddInstall.Size = new System.Drawing.Size(124, 17);
            this.cbAddInstall.TabIndex = 2;
            this.cbAddInstall.Text = global::Rapr.Lang.Language.Check_Install_Driver;
            this.cbAddInstall.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(2, 705);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1004, 22);
            this.statusStrip1.TabIndex = 3;
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
            this.lblStatus.Size = new System.Drawing.Size(989, 17);
            this.lblStatus.Spring = true;
            this.lblStatus.Text = global::Rapr.Lang.Language.Status_Label;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = global::Rapr.Lang.Language.Dialog_Open_Filters;
            this.openFileDialog.SupportMultiDottedExtensions = true;
            this.openFileDialog.Title = global::Rapr.Lang.Language.Dialog_Open_Filter_Text;
            // 
            // gbOptions
            // 
            this.gbOptions.AutoSize = true;
            this.gbOptions.Controls.Add(this.flowLayoutPanel1);
            this.gbOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbOptions.Location = new System.Drawing.Point(853, 3);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(148, 673);
            this.gbOptions.TabIndex = 1;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = global::Rapr.Lang.Language.Operations_Text;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.buttonEnumerate);
            this.flowLayoutPanel1.Controls.Add(this.buttonAddDriver);
            this.flowLayoutPanel1.Controls.Add(this.cbAddInstall);
            this.flowLayoutPanel1.Controls.Add(this.buttonDeleteDriver);
            this.flowLayoutPanel1.Controls.Add(this.cbForceDeletion);
            this.flowLayoutPanel1.Controls.Add(this.buttonSelectOldDrivers);
            this.flowLayoutPanel1.Controls.Add(this.buttonExportDrivers);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(6);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(142, 654);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // buttonSelectOldDrivers
            // 
            this.buttonSelectOldDrivers.AutoSize = true;
            this.buttonSelectOldDrivers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonSelectOldDrivers.Location = new System.Drawing.Point(9, 142);
            this.buttonSelectOldDrivers.Name = "buttonSelectOldDrivers";
            this.buttonSelectOldDrivers.Size = new System.Drawing.Size(124, 23);
            this.buttonSelectOldDrivers.TabIndex = 5;
            this.buttonSelectOldDrivers.Text = global::Rapr.Lang.Language.Button_Select_Old;
            this.buttonSelectOldDrivers.UseVisualStyleBackColor = true;
            this.buttonSelectOldDrivers.Click += new System.EventHandler(this.ButtonSelectOldDrivers_Click);
            // 
            // buttonExportDrivers
            // 
            this.buttonExportDrivers.AutoSize = true;
            this.buttonExportDrivers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonExportDrivers.Location = new System.Drawing.Point(9, 171);
            this.buttonExportDrivers.Name = "buttonExportDrivers";
            this.buttonExportDrivers.Size = new System.Drawing.Size(124, 23);
            this.buttonExportDrivers.TabIndex = 6;
            this.buttonExportDrivers.Text = global::Rapr.Lang.Language.Button_Export_Drivers;
            this.buttonExportDrivers.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.languageToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(2, 2);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1004, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chooseDriverStoreToolStripMenuItem,
            this.toolStripSeparator5,
            this.exportToolStripMenuItem,
            this.exportAllDriversToolStripMenuItem,
            this.viewLogsTtoolStripMenuItem,
            this.toolStripSeparator4,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = global::Rapr.Lang.Language.Menu_File;
            // 
            // chooseDriverStoreToolStripMenuItem
            // 
            this.chooseDriverStoreToolStripMenuItem.Name = "chooseDriverStoreToolStripMenuItem";
            this.chooseDriverStoreToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.chooseDriverStoreToolStripMenuItem.Text = global::Rapr.Lang.Language.Menu_File_Choose_Driver_Store;
            this.chooseDriverStoreToolStripMenuItem.Click += new System.EventHandler(this.ChooseDriverStoreToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(175, 6);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.exportToolStripMenuItem.Text = global::Rapr.Lang.Language.Menu_File_Export;
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // exportAllDriversToolStripMenuItem
            // 
            this.exportAllDriversToolStripMenuItem.Name = "exportAllDriversToolStripMenuItem";
            this.exportAllDriversToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.exportAllDriversToolStripMenuItem.Text = global::Rapr.Lang.Language.Menu_File_Export_All_Drivers;
            this.exportAllDriversToolStripMenuItem.Click += new System.EventHandler(this.ExportAllDriversToolStripMenuItem_Click);
            // 
            // viewLogsTtoolStripMenuItem
            // 
            this.viewLogsTtoolStripMenuItem.Name = "viewLogsTtoolStripMenuItem";
            this.viewLogsTtoolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.viewLogsTtoolStripMenuItem.Text = global::Rapr.Lang.Language.Menu_File_View_Logs;
            this.viewLogsTtoolStripMenuItem.Click += new System.EventHandler(this.ViewLogsToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(175, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.exitToolStripMenuItem.Text = global::Rapr.Lang.Language.Menu_File_Exit;
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            this.languageToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.languageToolStripMenuItem.Text = global::Rapr.Lang.Language.Menu_Language;
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem1});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = global::Rapr.Lang.Language.Menu_Help;
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem1.Text = global::Rapr.Lang.Language.Menu_Help_About;
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.gbOptions, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lstDriverStoreEntries, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 26);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1004, 679);
            this.tableLayoutPanel1.TabIndex = 0;
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
            this.lstDriverStoreEntries.AllColumns.Add(this.deviceNameColumn);
            this.lstDriverStoreEntries.AllColumns.Add(this.bootCriticalColumn);
            this.lstDriverStoreEntries.AllColumns.Add(this.dummyColumn);
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
            this.driverSizeColumn,
            this.deviceNameColumn,
            this.dummyColumn});
            this.lstDriverStoreEntries.ContextMenuStrip = this.contextMenuStrip;
            this.lstDriverStoreEntries.Cursor = System.Windows.Forms.Cursors.Default;
            this.lstDriverStoreEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDriverStoreEntries.EmptyListMsg = global::Rapr.Lang.Language.Message_No_Entries;
            this.lstDriverStoreEntries.FullRowSelect = true;
            this.lstDriverStoreEntries.GridLines = true;
            this.lstDriverStoreEntries.HideSelection = false;
            this.lstDriverStoreEntries.Location = new System.Drawing.Point(3, 3);
            this.lstDriverStoreEntries.Name = "lstDriverStoreEntries";
            this.lstDriverStoreEntries.ShowItemToolTips = true;
            this.lstDriverStoreEntries.Size = new System.Drawing.Size(844, 673);
            this.lstDriverStoreEntries.SortGroupItemsByPrimaryColumn = false;
            this.lstDriverStoreEntries.TabIndex = 0;
            this.lstDriverStoreEntries.UseCellFormatEvents = true;
            this.lstDriverStoreEntries.UseCompatibleStateImageBehavior = false;
            this.lstDriverStoreEntries.View = System.Windows.Forms.View.Details;
            this.lstDriverStoreEntries.FormatCell += new System.EventHandler<BrightIdeasSoftware.FormatCellEventArgs>(this.LstDriverStoreEntries_FormatCell);
            this.lstDriverStoreEntries.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.LstDriverStoreEntries_FormatRow);
            this.lstDriverStoreEntries.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.LstDriverStoreEntries_ItemChecked);
            // 
            // driverInfColumn
            // 
            this.driverInfColumn.AspectName = "DriverInfName";
            this.driverInfColumn.Text = global::Rapr.Lang.Language.Column_Inf;
            this.driverInfColumn.UseInitialLetterForGroup = true;
            this.driverInfColumn.Width = 120;
            // 
            // driverClassColumn
            // 
            this.driverClassColumn.AspectName = "DriverClass";
            this.driverClassColumn.Text = global::Rapr.Lang.Language.Column_Driver_Class;
            this.driverClassColumn.Width = 170;
            // 
            // driverProviderColumn
            // 
            this.driverProviderColumn.AspectName = "DriverPkgProvider";
            this.driverProviderColumn.Text = global::Rapr.Lang.Language.Column_Provider;
            this.driverProviderColumn.Width = 160;
            // 
            // driverVersionColumn
            // 
            this.driverVersionColumn.AspectName = "DriverVersion";
            this.driverVersionColumn.Text = global::Rapr.Lang.Language.Column_Version;
            this.driverVersionColumn.Width = 110;
            // 
            // driverDateColumn
            // 
            this.driverDateColumn.AspectName = "DriverDate";
            this.driverDateColumn.AspectToStringFormat = global::Rapr.Lang.Language.Date_Format;
            this.driverDateColumn.Text = global::Rapr.Lang.Language.Column_Date;
            this.driverDateColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.driverDateColumn.Width = 80;
            // 
            // driverSizeColumn
            // 
            this.driverSizeColumn.AspectName = "DriverSize";
            this.driverSizeColumn.Text = global::Rapr.Lang.Language.Column_Size;
            this.driverSizeColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.driverSizeColumn.Width = 59;
            // 
            // deviceNameColumn
            // 
            this.deviceNameColumn.AspectName = "DeviceName";
            this.deviceNameColumn.Text = global::Rapr.Lang.Language.Column_DeviceName;
            this.deviceNameColumn.UseInitialLetterForGroup = true;
            this.deviceNameColumn.Width = 170;
            // 
            // bootCriticalColumn
            // 
            this.bootCriticalColumn.AspectName = "BootCritical";
            this.bootCriticalColumn.IsVisible = false;
            this.bootCriticalColumn.Text = global::Rapr.Lang.Language.Column_BootCritical;
            // 
            // dummyColumn
            // 
            this.dummyColumn.FillsFreeSpace = true;
            this.dummyColumn.Text = "";
            // 
            // DSEForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(480, 360);
            this.Name = "DSEForm";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = global::Rapr.Lang.Language.Product_Name;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DSEForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DSEForm_FormClosed);
            this.Load += new System.EventHandler(this.DSEForm_Load);
            this.Shown += new System.EventHandler(this.DSEForm_Shown);
            this.contextMenuStrip.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lstDriverStoreEntries)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonEnumerate;
        private MyObjectListView lstDriverStoreEntries;
        private BrightIdeasSoftware.OLVColumn driverOemInfColumn;
        private BrightIdeasSoftware.OLVColumn driverDateColumn;
        private BrightIdeasSoftware.OLVColumn driverClassColumn;
        private BrightIdeasSoftware.OLVColumn driverSignerColumn;
        private BrightIdeasSoftware.OLVColumn driverVersionColumn;
        private System.Windows.Forms.Button buttonDeleteDriver;
        private System.Windows.Forms.CheckBox cbForceDeletion;
        private BrightIdeasSoftware.OLVColumn driverProviderColumn;
        private System.Windows.Forms.Button buttonAddDriver;
        private System.Windows.Forms.CheckBox cbAddInstall;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ctxMenuSelectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
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
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem viewLogsTtoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private BrightIdeasSoftware.OLVColumn dummyColumn;
        private System.Windows.Forms.ToolStripMenuItem ctxMenuOpenFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem chooseDriverStoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private BrightIdeasSoftware.OLVColumn deviceNameColumn;
        private System.Windows.Forms.ToolStripMenuItem exportAllDriversToolStripMenuItem;
        private System.Windows.Forms.Button buttonExportDrivers;
        private System.Windows.Forms.ToolStripMenuItem ctxMenuExportDriver;
        private BrightIdeasSoftware.OLVColumn bootCriticalColumn;
    }
}

