using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DA_Tool.Frostbite;

namespace DA_Tool.TOCReader
{
    public partial class TOCReader : Form
    {
        public TOCFile toc;

        public TOCReader()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.toc|*.toc";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                toc = new TOCFile(d.FileName);
                RefreshTree();
            }
        }

        public void RefreshTree()
        {
            treeView1.Nodes.Clear();
            if (toc != null && toc.lines != null)
            {
                foreach (Tools.Entry e in toc.lines)
                    treeView1.Nodes.Add(Tools.MakeEntry(new TreeNode(e.type.ToString("X")), e));
            }
        }        
    }
}
