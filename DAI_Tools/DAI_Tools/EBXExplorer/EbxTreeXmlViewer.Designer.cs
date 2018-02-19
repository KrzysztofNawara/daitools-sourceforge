namespace DAI_Tools.EBXExplorer
{
    partial class EbxTreeXmlViewer
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
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.flattendChbx = new System.Windows.Forms.CheckBox();
            this.flatRefsChbx = new System.Windows.Forms.CheckBox();
            this.showGuidsChbx = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 25);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(300, 275);
            this.treeView1.TabIndex = 0;
            this.treeView1.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeExpand);
            this.treeView1.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeSelect);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(300, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // flattendChbx
            // 
            this.flattendChbx.AutoSize = true;
            this.flattendChbx.Location = new System.Drawing.Point(4, 4);
            this.flattendChbx.Name = "flattendChbx";
            this.flattendChbx.Size = new System.Drawing.Size(70, 17);
            this.flattendChbx.TabIndex = 2;
            this.flattendChbx.Text = "Flattened";
            this.flattendChbx.UseVisualStyleBackColor = true;
            this.flattendChbx.CheckedChanged += new System.EventHandler(this.flattendChbx_CheckedChanged);
            // 
            // flatRefsChbx
            // 
            this.flatRefsChbx.AutoSize = true;
            this.flatRefsChbx.Location = new System.Drawing.Point(81, 4);
            this.flatRefsChbx.Name = "flatRefsChbx";
            this.flatRefsChbx.Size = new System.Drawing.Size(65, 17);
            this.flatRefsChbx.TabIndex = 3;
            this.flatRefsChbx.Text = "FlatRefs";
            this.flatRefsChbx.UseVisualStyleBackColor = true;
            this.flatRefsChbx.CheckedChanged += new System.EventHandler(this.flatRefsChbx_CheckedChanged);
            // 
            // showGuidsChbx
            // 
            this.showGuidsChbx.AutoSize = true;
            this.showGuidsChbx.Checked = true;
            this.showGuidsChbx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showGuidsChbx.Location = new System.Drawing.Point(153, 4);
            this.showGuidsChbx.Name = "showGuidsChbx";
            this.showGuidsChbx.Size = new System.Drawing.Size(88, 17);
            this.showGuidsChbx.TabIndex = 4;
            this.showGuidsChbx.Text = "Show GUIDs";
            this.showGuidsChbx.UseVisualStyleBackColor = true;
            this.showGuidsChbx.CheckedChanged += new System.EventHandler(this.showGuidsChbx_CheckedChanged);
            // 
            // EbxTreeXmlViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.showGuidsChbx);
            this.Controls.Add(this.flatRefsChbx);
            this.Controls.Add(this.flattendChbx);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "EbxTreeXmlViewer";
            this.Size = new System.Drawing.Size(300, 300);
            this.VisibleChanged += new System.EventHandler(this.EbxTreeXmlViewer_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.CheckBox flattendChbx;
        private System.Windows.Forms.CheckBox flatRefsChbx;
        private System.Windows.Forms.CheckBox showGuidsChbx;
    }
}
