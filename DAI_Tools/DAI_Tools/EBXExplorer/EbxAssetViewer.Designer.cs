namespace DAI_Tools.EBXExplorer
{
    partial class EbxAssetViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.assetList = new System.Windows.Forms.DataGridView();
            this.assetType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.assetName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.assetGuid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.partialsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.graphVizButton = new System.Windows.Forms.ToolStripButton();
            this.blueprintVizButton = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.assetList)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // assetList
            // 
            this.assetList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.assetList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.assetType,
            this.assetName,
            this.assetGuid});
            this.assetList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assetList.Location = new System.Drawing.Point(0, 25);
            this.assetList.Name = "assetList";
            this.assetList.RowHeadersVisible = false;
            this.assetList.RowTemplate.ReadOnly = true;
            this.assetList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.assetList.Size = new System.Drawing.Size(300, 125);
            this.assetList.TabIndex = 0;
            this.assetList.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.assetList_RowEnter);
            this.assetList.RowLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.assetList_RowLeave);
            // 
            // assetType
            // 
            this.assetType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.assetType.HeaderText = "Type";
            this.assetType.Name = "assetType";
            this.assetType.ReadOnly = true;
            // 
            // assetName
            // 
            this.assetName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.assetName.HeaderText = "Name";
            this.assetName.Name = "assetName";
            this.assetName.ReadOnly = true;
            // 
            // assetGuid
            // 
            this.assetGuid.HeaderText = "";
            this.assetGuid.Name = "assetGuid";
            this.assetGuid.ReadOnly = true;
            this.assetGuid.Visible = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.partialsLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 128);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(300, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // partialsLabel
            // 
            this.partialsLabel.Name = "partialsLabel";
            this.partialsLabel.Size = new System.Drawing.Size(285, 17);
            this.partialsLabel.Spring = true;
            this.partialsLabel.Text = ".";
            this.partialsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.graphVizButton,
            this.blueprintVizButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(300, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // graphVizButton
            // 
            this.graphVizButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.graphVizButton.Enabled = false;
            this.graphVizButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.graphVizButton.Name = "graphVizButton";
            this.graphVizButton.Size = new System.Drawing.Size(58, 22);
            this.graphVizButton.Text = "GraphViz";
            this.graphVizButton.Click += new System.EventHandler(this.graphVizButton_Click);
            // 
            // blueprintVizButton
            // 
            this.blueprintVizButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.blueprintVizButton.Enabled = false;
            this.blueprintVizButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.blueprintVizButton.Name = "blueprintVizButton";
            this.blueprintVizButton.Size = new System.Drawing.Size(74, 22);
            this.blueprintVizButton.Text = "BlueprintViz";
            this.blueprintVizButton.Click += new System.EventHandler(this.blueprintVizButton_Click);
            // 
            // EbxAssetViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.assetList);
            this.Controls.Add(this.toolStrip1);
            this.Name = "EbxAssetViewer";
            this.Size = new System.Drawing.Size(300, 150);
            ((System.ComponentModel.ISupportInitialize)(this.assetList)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView assetList;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel partialsLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn assetType;
        private System.Windows.Forms.DataGridViewTextBoxColumn assetName;
        private System.Windows.Forms.DataGridViewTextBoxColumn assetGuid;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton graphVizButton;
        private System.Windows.Forms.ToolStripButton blueprintVizButton;
    }
}
