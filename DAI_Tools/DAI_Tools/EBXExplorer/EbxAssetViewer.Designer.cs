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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.partialsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.assetType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.assetName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.assetGuid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.assetList)).BeginInit();
            this.statusStrip1.SuspendLayout();
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
            this.assetList.Location = new System.Drawing.Point(0, 0);
            this.assetList.Name = "assetList";
            this.assetList.RowHeadersVisible = false;
            this.assetList.RowTemplate.ReadOnly = true;
            this.assetList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.assetList.Size = new System.Drawing.Size(300, 150);
            this.assetList.TabIndex = 0;
            this.assetList.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.assetList_RowEnter);
            this.assetList.RowLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.assetList_RowLeave);
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
            this.partialsLabel.Size = new System.Drawing.Size(10, 17);
            this.partialsLabel.Text = ".";
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
            // EbxAssetViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.assetList);
            this.Name = "EbxAssetViewer";
            this.Size = new System.Drawing.Size(300, 150);
            ((System.ComponentModel.ISupportInitialize)(this.assetList)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
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
    }
}
