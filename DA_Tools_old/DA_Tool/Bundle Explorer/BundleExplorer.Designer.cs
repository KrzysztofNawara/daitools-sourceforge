namespace DA_Tool.Bundle_Explorer
{
    partial class BundleExplorer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BundleExplorer));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.transferToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportResourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllEBXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.rtb1 = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.listBox3 = new System.Windows.Forms.ListBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.listBox4 = new System.Windows.Forms.ListBox();
            this.contextRes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.nopeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previewMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.listBox5 = new System.Windows.Forms.ListBox();
            this.hb1 = new Be.Windows.Forms.HexBox();
            this.rtb2 = new System.Windows.Forms.RichTextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.contextRes.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.transferToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(733, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Visible = false;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.saveToolStripMenuItem.Text = "Save...";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // transferToolStripMenuItem
            // 
            this.transferToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportPreviewToolStripMenuItem,
            this.exportResourceToolStripMenuItem,
            this.loadPreviewToolStripMenuItem,
            this.importPreviewToolStripMenuItem,
            this.exportAllEBXToolStripMenuItem});
            this.transferToolStripMenuItem.Name = "transferToolStripMenuItem";
            this.transferToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.transferToolStripMenuItem.Text = "Transfer";
            // 
            // exportPreviewToolStripMenuItem
            // 
            this.exportPreviewToolStripMenuItem.Name = "exportPreviewToolStripMenuItem";
            this.exportPreviewToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.exportPreviewToolStripMenuItem.Text = "Export Preview";
            this.exportPreviewToolStripMenuItem.Click += new System.EventHandler(this.exportPreviewToolStripMenuItem_Click);
            // 
            // exportResourceToolStripMenuItem
            // 
            this.exportResourceToolStripMenuItem.Name = "exportResourceToolStripMenuItem";
            this.exportResourceToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.exportResourceToolStripMenuItem.Text = "Export Resource";
            this.exportResourceToolStripMenuItem.Click += new System.EventHandler(this.exportResourceToolStripMenuItem_Click);
            // 
            // loadPreviewToolStripMenuItem
            // 
            this.loadPreviewToolStripMenuItem.Name = "loadPreviewToolStripMenuItem";
            this.loadPreviewToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.loadPreviewToolStripMenuItem.Text = "Load Preview";
            this.loadPreviewToolStripMenuItem.Click += new System.EventHandler(this.loadPreviewToolStripMenuItem_Click);
            // 
            // importPreviewToolStripMenuItem
            // 
            this.importPreviewToolStripMenuItem.Name = "importPreviewToolStripMenuItem";
            this.importPreviewToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.importPreviewToolStripMenuItem.Text = "Import Preview";
            this.importPreviewToolStripMenuItem.Click += new System.EventHandler(this.importPreviewToolStripMenuItem_Click);
            // 
            // exportAllEBXToolStripMenuItem
            // 
            this.exportAllEBXToolStripMenuItem.Name = "exportAllEBXToolStripMenuItem";
            this.exportAllEBXToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.exportAllEBXToolStripMenuItem.Text = "Export All EBX";
            this.exportAllEBXToolStripMenuItem.Click += new System.EventHandler(this.exportAllEBXToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(733, 443);
            this.splitContainer1.SplitterDistance = 425;
            this.splitContainer1.TabIndex = 1;
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.IntegralHeight = false;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(425, 443);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
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
            this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.hb1);
            this.splitContainer2.Panel2.Controls.Add(this.rtb2);
            this.splitContainer2.Size = new System.Drawing.Size(304, 443);
            this.splitContainer2.SplitterDistance = 328;
            this.splitContainer2.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(304, 328);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.rtb1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(296, 302);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // rtb1
            // 
            this.rtb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb1.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb1.Location = new System.Drawing.Point(3, 3);
            this.rtb1.Name = "rtb1";
            this.rtb1.ReadOnly = true;
            this.rtb1.Size = new System.Drawing.Size(290, 296);
            this.rtb1.TabIndex = 0;
            this.rtb1.Text = "";
            this.rtb1.WordWrap = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listBox2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(296, 302);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "ebx";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listBox2
            // 
            this.listBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox2.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.IntegralHeight = false;
            this.listBox2.ItemHeight = 16;
            this.listBox2.Location = new System.Drawing.Point(3, 3);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(290, 296);
            this.listBox2.TabIndex = 1;
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged_1);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.listBox3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(296, 302);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "dbx";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // listBox3
            // 
            this.listBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox3.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox3.FormattingEnabled = true;
            this.listBox3.HorizontalScrollbar = true;
            this.listBox3.IntegralHeight = false;
            this.listBox3.ItemHeight = 16;
            this.listBox3.Location = new System.Drawing.Point(3, 3);
            this.listBox3.Name = "listBox3";
            this.listBox3.Size = new System.Drawing.Size(290, 296);
            this.listBox3.TabIndex = 1;
            this.listBox3.SelectedIndexChanged += new System.EventHandler(this.listBox3_SelectedIndexChanged_1);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.listBox4);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(296, 302);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "res";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // listBox4
            // 
            this.listBox4.ContextMenuStrip = this.contextRes;
            this.listBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox4.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox4.FormattingEnabled = true;
            this.listBox4.HorizontalScrollbar = true;
            this.listBox4.IntegralHeight = false;
            this.listBox4.ItemHeight = 16;
            this.listBox4.Location = new System.Drawing.Point(3, 3);
            this.listBox4.Name = "listBox4";
            this.listBox4.Size = new System.Drawing.Size(290, 296);
            this.listBox4.TabIndex = 1;
            this.listBox4.SelectedIndexChanged += new System.EventHandler(this.listBox4_SelectedIndexChanged_1);
            // 
            // contextRes
            // 
            this.contextRes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nopeToolStripMenuItem,
            this.previewMeshToolStripMenuItem});
            this.contextRes.Name = "contextRes";
            this.contextRes.Size = new System.Drawing.Size(141, 48);
            this.contextRes.Opening += new System.ComponentModel.CancelEventHandler(this.contextRes_Opening);
            // 
            // nopeToolStripMenuItem
            // 
            this.nopeToolStripMenuItem.Name = "nopeToolStripMenuItem";
            this.nopeToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.nopeToolStripMenuItem.Text = "Nope";
            // 
            // previewMeshToolStripMenuItem
            // 
            this.previewMeshToolStripMenuItem.Name = "previewMeshToolStripMenuItem";
            this.previewMeshToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.previewMeshToolStripMenuItem.Text = "Preview Mesh";
            this.previewMeshToolStripMenuItem.Click += new System.EventHandler(this.previewMeshToolStripMenuItem_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.listBox5);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(296, 302);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "chunks";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // listBox5
            // 
            this.listBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox5.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox5.FormattingEnabled = true;
            this.listBox5.HorizontalScrollbar = true;
            this.listBox5.IntegralHeight = false;
            this.listBox5.ItemHeight = 16;
            this.listBox5.Location = new System.Drawing.Point(3, 3);
            this.listBox5.Name = "listBox5";
            this.listBox5.Size = new System.Drawing.Size(290, 296);
            this.listBox5.TabIndex = 1;
            this.listBox5.SelectedIndexChanged += new System.EventHandler(this.listBox5_SelectedIndexChanged_1);
            // 
            // hb1
            // 
            this.hb1.BoldFont = null;
            this.hb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hb1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hb1.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hb1.LineInfoVisible = true;
            this.hb1.Location = new System.Drawing.Point(0, 0);
            this.hb1.Name = "hb1";
            this.hb1.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hb1.Size = new System.Drawing.Size(304, 111);
            this.hb1.StringViewVisible = true;
            this.hb1.TabIndex = 1;
            this.hb1.UseFixedBytesPerLine = true;
            this.hb1.VScrollBarVisible = true;
            // 
            // rtb2
            // 
            this.rtb2.DetectUrls = false;
            this.rtb2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb2.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb2.HideSelection = false;
            this.rtb2.Location = new System.Drawing.Point(0, 0);
            this.rtb2.Name = "rtb2";
            this.rtb2.Size = new System.Drawing.Size(304, 111);
            this.rtb2.TabIndex = 3;
            this.rtb2.Text = "";
            this.rtb2.WordWrap = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox1,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(733, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(250, 25);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(57, 22);
            this.toolStripButton1.Text = "Find Next";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // BundleExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(733, 468);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "BundleExplorer";
            this.Text = "Bundle Explorer";
            this.Load += new System.EventHandler(this.BundleExplorer_Load);
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
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.contextRes.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ToolStripMenuItem transferToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportPreviewToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox rtb1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ListBox listBox3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.ListBox listBox4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.ListBox listBox5;
        private Be.Windows.Forms.HexBox hb1;
        private System.Windows.Forms.ToolStripMenuItem exportResourceToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextRes;
        private System.Windows.Forms.ToolStripMenuItem nopeToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem previewMeshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllEBXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importPreviewToolStripMenuItem;
        public System.Windows.Forms.RichTextBox rtb2;
    }
}