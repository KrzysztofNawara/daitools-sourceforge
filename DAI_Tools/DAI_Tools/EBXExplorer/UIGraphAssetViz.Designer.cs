namespace DAI_Tools.EBXExplorer
{
    partial class UIGraphAssetViz
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.graphVizPanel = new System.Windows.Forms.Panel();
            this.hideSplittersCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(284, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // graphVizPanel
            // 
            this.graphVizPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphVizPanel.Location = new System.Drawing.Point(0, 25);
            this.graphVizPanel.Name = "graphVizPanel";
            this.graphVizPanel.Size = new System.Drawing.Size(284, 237);
            this.graphVizPanel.TabIndex = 1;
            // 
            // hideSplittersCheckbox
            // 
            this.hideSplittersCheckbox.AutoSize = true;
            this.hideSplittersCheckbox.Location = new System.Drawing.Point(13, 7);
            this.hideSplittersCheckbox.Name = "hideSplittersCheckbox";
            this.hideSplittersCheckbox.Size = new System.Drawing.Size(114, 17);
            this.hideSplittersCheckbox.TabIndex = 2;
            this.hideSplittersCheckbox.Text = "Hide SplitterNodes";
            this.hideSplittersCheckbox.UseVisualStyleBackColor = true;
            this.hideSplittersCheckbox.CheckedChanged += new System.EventHandler(this.hideSplittersCheckbox_CheckedChanged);
            // 
            // UIGraphAssetViz
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.hideSplittersCheckbox);
            this.Controls.Add(this.graphVizPanel);
            this.Controls.Add(this.toolStrip1);
            this.Name = "UIGraphAssetViz";
            this.Text = "UIGraphAssetViz";
            this.Load += new System.EventHandler(this.UIGraphAssetViz_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.Panel graphVizPanel;
        private System.Windows.Forms.CheckBox hideSplittersCheckbox;
    }
}