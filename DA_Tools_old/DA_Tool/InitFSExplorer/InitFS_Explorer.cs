using DA_Tool.Frostbite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DA_Tool.InitFSExplorer
{
    public partial class InitFS_Explorer : Form
    {
        private TOCFile tocFile = null;
        private Tools.Field payloadField = null;

        public InitFS_Explorer()
        {
            InitializeComponent();
            txtPayloadEditor.LostFocus += txtPayloadEditor_LostFocus;
        }

        void txtPayloadEditor_LostFocus(object sender, EventArgs e)
        {
            if (payloadField != null)
            {
                payloadField.data = System.Text.Encoding.ASCII.GetBytes(txtPayloadEditor.Text);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "InitFS_Win32 file|initfs_win32";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tocFile = new TOCFile(d.FileName);
                DisplayCurrentTOCFile();
            }
        }

        private void DisplayCurrentTOCFile()
        {
            txtPayloadEditor.Clear();
            treeView1.Nodes.Clear();
            if (tocFile != null && tocFile.lines != null)
            {
                foreach (Tools.Entry e in tocFile.lines)
                {
                    treeView1.Nodes.Add(Tools.MakeEntry(new TreeNode(e.type.ToString("X")), e));
                }
                TagCheck(treeView1.Nodes);
            }
        }

        private void TagCheck(TreeNodeCollection nodeCollection)
        {
            if (nodeCollection != null)
            {
                foreach (TreeNode node in nodeCollection)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("name '{0}' tag '{1}'", node.Text, node.Tag));
                    TagCheck(node.Nodes);
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null && e.Node.Text == "payload")
            {
                payloadField = (Tools.Field)e.Node.Tag;
                txtPayloadEditor.Text = System.Text.Encoding.ASCII.GetString((byte[])payloadField.data);
            }
        }
    }
}
