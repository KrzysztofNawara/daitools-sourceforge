namespace DAI_Tools
{
    partial class Frontend
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.databaseManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bundleBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textureExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eBXExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modScriptToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.talktableExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shaderExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status});
            this.statusStrip.Location = new System.Drawing.Point(0, 532);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(813, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "StatusStrip";
            // 
            // status
            // 
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(39, 17);
            this.status.Text = "Status";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(813, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Visible = false;
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.databaseManagerToolStripMenuItem,
            this.bundleBrowserToolStripMenuItem,
            this.soundExplorerToolStripMenuItem,
            this.scriptExplorerToolStripMenuItem,
            this.textureExplorerToolStripMenuItem,
            this.eBXExplorerToolStripMenuItem,
            this.modScriptToolToolStripMenuItem,
            this.talktableExplorerToolStripMenuItem,
            this.shaderExplorerToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // databaseManagerToolStripMenuItem
            // 
            this.databaseManagerToolStripMenuItem.Name = "databaseManagerToolStripMenuItem";
            this.databaseManagerToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.databaseManagerToolStripMenuItem.Text = "Database Manager";
            this.databaseManagerToolStripMenuItem.Click += new System.EventHandler(this.databaseManagerToolStripMenuItem_Click);
            // 
            // bundleBrowserToolStripMenuItem
            // 
            this.bundleBrowserToolStripMenuItem.Name = "bundleBrowserToolStripMenuItem";
            this.bundleBrowserToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.bundleBrowserToolStripMenuItem.Text = "Bundle Browser";
            this.bundleBrowserToolStripMenuItem.Click += new System.EventHandler(this.bundleBrowserToolStripMenuItem_Click);
            // 
            // soundExplorerToolStripMenuItem
            // 
            this.soundExplorerToolStripMenuItem.Name = "soundExplorerToolStripMenuItem";
            this.soundExplorerToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.soundExplorerToolStripMenuItem.Text = "Sound Explorer";
            this.soundExplorerToolStripMenuItem.Click += new System.EventHandler(this.soundExplorerToolStripMenuItem_Click);
            // 
            // scriptExplorerToolStripMenuItem
            // 
            this.scriptExplorerToolStripMenuItem.Name = "scriptExplorerToolStripMenuItem";
            this.scriptExplorerToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.scriptExplorerToolStripMenuItem.Text = "Script Explorer";
            this.scriptExplorerToolStripMenuItem.Click += new System.EventHandler(this.scriptExplorerToolStripMenuItem_Click);
            // 
            // textureExplorerToolStripMenuItem
            // 
            this.textureExplorerToolStripMenuItem.Name = "textureExplorerToolStripMenuItem";
            this.textureExplorerToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.textureExplorerToolStripMenuItem.Text = "Texture Explorer";
            this.textureExplorerToolStripMenuItem.Click += new System.EventHandler(this.textureExplorerToolStripMenuItem_Click);
            // 
            // eBXExplorerToolStripMenuItem
            // 
            this.eBXExplorerToolStripMenuItem.Name = "eBXExplorerToolStripMenuItem";
            this.eBXExplorerToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.eBXExplorerToolStripMenuItem.Text = "EBX Explorer";
            this.eBXExplorerToolStripMenuItem.Click += new System.EventHandler(this.eBXExplorerToolStripMenuItem_Click);
            // 
            // modScriptToolToolStripMenuItem
            // 
            this.modScriptToolToolStripMenuItem.Name = "modScriptToolToolStripMenuItem";
            this.modScriptToolToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.modScriptToolToolStripMenuItem.Text = "Mod Script Tool";
            this.modScriptToolToolStripMenuItem.Click += new System.EventHandler(this.modScriptToolToolStripMenuItem_Click);
            // 
            // talktableExplorerToolStripMenuItem
            // 
            this.talktableExplorerToolStripMenuItem.Name = "talktableExplorerToolStripMenuItem";
            this.talktableExplorerToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.talktableExplorerToolStripMenuItem.Text = "Talktable Explorer";
            this.talktableExplorerToolStripMenuItem.Click += new System.EventHandler(this.talktableExplorerToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // shaderExplorerToolStripMenuItem
            // 
            this.shaderExplorerToolStripMenuItem.Name = "shaderExplorerToolStripMenuItem";
            this.shaderExplorerToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.shaderExplorerToolStripMenuItem.Text = "Shader Explorer";
            this.shaderExplorerToolStripMenuItem.Click += new System.EventHandler(this.shaderExplorerToolStripMenuItem_Click);
            // 
            // Frontend
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(813, 554);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Frontend";
            this.Text = "Frontend";
            this.Activated += new System.EventHandler(this.Frontend_Activated);
            this.Load += new System.EventHandler(this.Frontend_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel status;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem databaseManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bundleBrowserToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem soundExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textureExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eBXExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scriptExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modScriptToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem talktableExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shaderExplorerToolStripMenuItem;
    }
}



