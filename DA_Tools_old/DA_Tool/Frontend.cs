using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DA_Tool
{
    public partial class Frontend : Form
    {
        public Frontend()
        {
            InitializeComponent();
        }

        private void tOCReaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new TOCReader.TOCReader());
        }

        public void OpenMaximized(Form form)
        {
            form.MdiParent = this;
            form.Show();
            form.WindowState = FormWindowState.Maximized;
        }

        private void cATReaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new CATReader.CATReader());
        }

        private void cASExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new CASExplorer.CASExplorer());
        }

        private void dASReaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new DASReader.DASReader());
        }

        private void bitToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new BitTool.BitTool());
        }

        private void sBReaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new SBReader.SBReader());
        }

        private void resExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new ResExplorer.ResExplorer());
        }

        private void bundleExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new Bundle_Explorer.BundleExplorer());
        }

        private void initFSExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new InitFSExplorer.InitFS_Explorer());
        }

        private void Frontend_Load(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "DA Tools by Warranty Voider, Ehamloptiran and wogoodes Version : " + version;
        }

        private void soundExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaximized(new SoundExplorer.SoundExplorer());
        }
    }
}
