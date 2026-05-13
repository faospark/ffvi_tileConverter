namespace FFVI_tileTool
{
    partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.browseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseRecentSeparatorToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.browseRecentNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseAndMassExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.massImportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filtersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterSnowTilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterGrassTilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterMagitekTilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.isolateFilteredFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.fileListContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.revealInFileExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gzipThisFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.isolateSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonSection1PaletteInfo = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.buttonSection2PaletteInfo = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.browseToolStripMenuItem,
            this.createBackupToolStripMenuItem,
            this.browseAndMassExportToolStripMenuItem,
            this.massImportToolStripMenuItem,
            this.filtersToolStripMenuItem,
            this.darkModeToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(723, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // browseToolStripMenuItem
            // 
            this.browseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.browseOpenToolStripMenuItem,
            this.browseRecentSeparatorToolStripMenuItem,
            this.browseRecentNoneToolStripMenuItem});
            this.browseToolStripMenuItem.Name = "browseToolStripMenuItem";
            this.browseToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.browseToolStripMenuItem.Text = "Browse";
            this.browseToolStripMenuItem.DropDownOpening += new System.EventHandler(this.browseToolStripMenuItem_DropDownOpening);
            // 
            // browseOpenToolStripMenuItem
            // 
            this.browseOpenToolStripMenuItem.Name = "browseOpenToolStripMenuItem";
            this.browseOpenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.browseOpenToolStripMenuItem.Text = "Open...";
            this.browseOpenToolStripMenuItem.Click += new System.EventHandler(this.browseToolStripMenuItem_Click);
            // 
            // browseRecentSeparatorToolStripMenuItem
            // 
            this.browseRecentSeparatorToolStripMenuItem.Name = "browseRecentSeparatorToolStripMenuItem";
            this.browseRecentSeparatorToolStripMenuItem.Size = new System.Drawing.Size(177, 6);
            // 
            // browseRecentNoneToolStripMenuItem
            // 
            this.browseRecentNoneToolStripMenuItem.Enabled = false;
            this.browseRecentNoneToolStripMenuItem.Name = "browseRecentNoneToolStripMenuItem";
            this.browseRecentNoneToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.browseRecentNoneToolStripMenuItem.Text = "No recent directories";
            // 
            // createBackupToolStripMenuItem
            // 
            this.createBackupToolStripMenuItem.Name = "createBackupToolStripMenuItem";
            this.createBackupToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            this.createBackupToolStripMenuItem.Text = "Create Backup";
            this.createBackupToolStripMenuItem.Click += new System.EventHandler(this.createBackupToolStripMenuItem_Click);
            // 
            // browseAndMassExportToolStripMenuItem
            // 
            this.browseAndMassExportToolStripMenuItem.Name = "browseAndMassExportToolStripMenuItem";
            this.browseAndMassExportToolStripMenuItem.Size = new System.Drawing.Size(146, 20);
            this.browseAndMassExportToolStripMenuItem.Text = "Mass Export";
            this.browseAndMassExportToolStripMenuItem.Click += new System.EventHandler(this.browseAndMassExportToolStripMenuItem_Click);
            // 
            // massImportToolStripMenuItem
            // 
            this.massImportToolStripMenuItem.Name = "massImportToolStripMenuItem";
            this.massImportToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.massImportToolStripMenuItem.Text = "Mass Import";
            this.massImportToolStripMenuItem.Click += new System.EventHandler(this.massImportToolStripMenuItem_Click);
            // 
            // filtersToolStripMenuItem
            // 
            this.filtersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filterOffToolStripMenuItem,
            this.filterSnowTilesToolStripMenuItem,
            this.filterGrassTilesToolStripMenuItem,
            this.filterMagitekTilesToolStripMenuItem,
            this.isolateFilteredFilesToolStripMenuItem});
            this.filtersToolStripMenuItem.Name = "filtersToolStripMenuItem";
            this.filtersToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.filtersToolStripMenuItem.Text = "Filters";
            // 
            // filterOffToolStripMenuItem
            // 
            this.filterOffToolStripMenuItem.Name = "filterOffToolStripMenuItem";
            this.filterOffToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.filterOffToolStripMenuItem.Text = "Off";
            this.filterOffToolStripMenuItem.Click += new System.EventHandler(this.filterOffToolStripMenuItem_Click);
            // 
            // filterSnowTilesToolStripMenuItem
            // 
            this.filterSnowTilesToolStripMenuItem.Name = "filterSnowTilesToolStripMenuItem";
            this.filterSnowTilesToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.filterSnowTilesToolStripMenuItem.Text = "Snow Tiles";
            this.filterSnowTilesToolStripMenuItem.Click += new System.EventHandler(this.filterSnowTilesToolStripMenuItem_Click);
            // 
            // filterGrassTilesToolStripMenuItem
            // 
            this.filterGrassTilesToolStripMenuItem.Name = "filterGrassTilesToolStripMenuItem";
            this.filterGrassTilesToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.filterGrassTilesToolStripMenuItem.Text = "Grass Tiles";
            this.filterGrassTilesToolStripMenuItem.Click += new System.EventHandler(this.filterGrassTilesToolStripMenuItem_Click);
            // 
            // filterMagitekTilesToolStripMenuItem
            // 
            this.filterMagitekTilesToolStripMenuItem.Name = "filterMagitekTilesToolStripMenuItem";
            this.filterMagitekTilesToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.filterMagitekTilesToolStripMenuItem.Text = "Magitek Tiles";
            this.filterMagitekTilesToolStripMenuItem.Click += new System.EventHandler(this.filterMagitekTilesToolStripMenuItem_Click);
            // 
            // isolateFilteredFilesToolStripMenuItem
            // 
            this.isolateFilteredFilesToolStripMenuItem.Name = "isolateFilteredFilesToolStripMenuItem";
            this.isolateFilteredFilesToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.isolateFilteredFilesToolStripMenuItem.Text = "Isolate Filtered Files";
            this.isolateFilteredFilesToolStripMenuItem.Click += new System.EventHandler(this.isolateFilteredFilesToolStripMenuItem_Click);
            // 
            // darkModeToolStripMenuItem
            // 
            this.darkModeToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.darkModeToolStripMenuItem.CheckOnClick = true;
            this.darkModeToolStripMenuItem.Name = "darkModeToolStripMenuItem";
            this.darkModeToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.darkModeToolStripMenuItem.Text = "Dark mode";
            this.darkModeToolStripMenuItem.Click += new System.EventHandler(this.darkModeToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1260, 646);
            this.splitContainer1.SplitterDistance = 110;
            this.splitContainer1.TabIndex = 1;
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox1.Size = new System.Drawing.Size(110, 646);
            this.listBox1.TabIndex = 0;
            this.listBox1.ContextMenuStrip = this.fileListContextMenuStrip;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDown);
            // 
            // fileListContextMenuStrip
            // 
            this.fileListContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.revealInFileExplorerToolStripMenuItem,
            this.gzipThisFileToolStripMenuItem,
            this.isolateSelectedFilesToolStripMenuItem});
            this.fileListContextMenuStrip.Name = "fileListContextMenuStrip";
            this.fileListContextMenuStrip.Size = new System.Drawing.Size(190, 70);
            // 
            // revealInFileExplorerToolStripMenuItem
            // 
            this.revealInFileExplorerToolStripMenuItem.Name = "revealInFileExplorerToolStripMenuItem";
            this.revealInFileExplorerToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.revealInFileExplorerToolStripMenuItem.Text = "Reveal in File Explorer";
            this.revealInFileExplorerToolStripMenuItem.Click += new System.EventHandler(this.revealInFileExplorerToolStripMenuItem_Click);
            // 
            // gzipThisFileToolStripMenuItem
            // 
            this.gzipThisFileToolStripMenuItem.Name = "gzipThisFileToolStripMenuItem";
            this.gzipThisFileToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.gzipThisFileToolStripMenuItem.Text = "Gzip this file";
            this.gzipThisFileToolStripMenuItem.Click += new System.EventHandler(this.gzipThisFileToolStripMenuItem_Click);
            // 
            // isolateSelectedFilesToolStripMenuItem
            // 
            this.isolateSelectedFilesToolStripMenuItem.Name = "isolateSelectedFilesToolStripMenuItem";
            this.isolateSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.isolateSelectedFilesToolStripMenuItem.Text = "Isolate selected file(s)";
            this.isolateSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.isolateSelectedFilesToolStripMenuItem_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox4);
            this.splitContainer2.Panel1.Controls.Add(this.groupBox3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer2.Size = new System.Drawing.Size(1094, 646);
            this.splitContainer2.SplitterDistance = 520;
            this.splitContainer2.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.panel2);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(547, 548);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Second section Image";
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.Controls.Add(this.pictureBox2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Name = "panel2";
            this.panel2.TabIndex = 0;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(512, 512);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.panel1);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(547, 548);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "First section Image";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(276, 453);
            this.panel1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(512, 512);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.groupBox1);
            this.flowLayoutPanel1.Controls.Add(this.groupBox2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(570, 102);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonSection1PaletteInfo);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(564, 50);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "I/O controls section 1";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(192, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(180, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Import image";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(180, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Export image";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonSection1PaletteInfo
            // 
            this.buttonSection1PaletteInfo.Location = new System.Drawing.Point(378, 19);
            this.buttonSection1PaletteInfo.Name = "buttonSection1PaletteInfo";
            this.buttonSection1PaletteInfo.Size = new System.Drawing.Size(180, 23);
            this.buttonSection1PaletteInfo.TabIndex = 2;
            this.buttonSection1PaletteInfo.Text = "Palette info: n/a";
            this.buttonSection1PaletteInfo.UseVisualStyleBackColor = true;
            this.buttonSection1PaletteInfo.Click += new System.EventHandler(this.buttonSection1PaletteInfo_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonSection2PaletteInfo);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.button4);
            this.groupBox2.Location = new System.Drawing.Point(3, 59);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(564, 50);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "I/O controls section 2";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(192, 19);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(180, 23);
            this.button3.TabIndex = 1;
            this.button3.Text = "Import image";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(6, 19);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(180, 23);
            this.button4.TabIndex = 0;
            this.button4.Text = "Export image";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // buttonSection2PaletteInfo
            // 
            this.buttonSection2PaletteInfo.Location = new System.Drawing.Point(378, 19);
            this.buttonSection2PaletteInfo.Name = "buttonSection2PaletteInfo";
            this.buttonSection2PaletteInfo.Size = new System.Drawing.Size(180, 23);
            this.buttonSection2PaletteInfo.TabIndex = 2;
            this.buttonSection2PaletteInfo.Text = "Palette info: n/a";
            this.buttonSection2PaletteInfo.UseVisualStyleBackColor = true;
            this.buttonSection2PaletteInfo.Click += new System.EventHandler(this.buttonSection2PaletteInfo_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1260, 670);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "FFVI Old Tile Tool";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem browseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator browseRecentSeparatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseRecentNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createBackupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filtersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterOffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterSnowTilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterGrassTilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterMagitekTilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem isolateFilteredFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ContextMenuStrip fileListContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem revealInFileExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gzipThisFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem isolateSelectedFilesToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolStripMenuItem browseAndMassExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem massImportToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button buttonSection1PaletteInfo;
        private System.Windows.Forms.Button buttonSection2PaletteInfo;
    }
}
