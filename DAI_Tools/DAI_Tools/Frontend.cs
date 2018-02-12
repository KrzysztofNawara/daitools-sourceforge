using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAI_Tools
{
    public partial class Frontend : Form
    {
        public bool init = false;
        public AboutBox box;

        public Frontend()
        {
            InitializeComponent();
        }

        private void Frontend_Load(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "DAI Tools by Warranty Voider, Ehamloptiran, Wogoodes and more... Version : " + version;
            SetStatus("Initializing...");
        }

        private void SetStatus(string s)
        {
            status.Text = "Status : " + s;
        }

        private void Init()
        {
            init = true;
            int steps = 0;
            try
            {
                Database.CheckIfDBExists(); steps++;
                Database.LoadSettings(); steps++;
                Database.CheckIfScanIsNeeded(); steps++;
            }
            catch (Exception ex)
            {
                switch(steps)
                { 
                    case 0:
                        SetStatus("Error on finding/creating database");                        
                        break;
                    case 1:
                        SetStatus("Error on loading settings");
                        break;
                    case 2:
                        SetStatus("Error on scanning");
                        break;
                    default:
                        SetStatus("Error on initializing!");
                        break;
                }
                return;
            }
            SetStatus("Ready");
            menuStrip1.Visible = true;
        }



        private void Frontend_Activated(object sender, EventArgs e)
        {
            if (!init)
                Init();
        }

        private void databaseManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new DBManager.DBManager());
        }

        private void OpenMaximized(Form f)
        {
            f.MdiParent = this;
            f.Show();
            f.WindowState = FormWindowState.Maximized;
        }

        private void bundleBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new BundleBrowser.BundleBrowser());
        }

        private void soundExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new SoundExplorer.SoundExplorer());
        }

        private void textureExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new TextureExplorer.TextureExplorer());
        }

        private void eBXExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new EBXExplorer.EBXExplorer());
        }

        private void scriptExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new ScriptExplorer.ScriptExplorer());
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (box != null)
                box.Close();
            box = new AboutBox();
            box.Show();
        }

        private void modScriptToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new ModScriptTool.ModScriptTool());
        }

        private void talktableExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new TalktableExplorer.TalktableExplorer());
        }

        private void shaderExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new ShaderExplorer.ShaderExplorer());
        }
    }
}
