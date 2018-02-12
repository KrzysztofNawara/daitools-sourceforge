using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DA_Tool.Frostbite;
using Be.Windows.Forms;

namespace DA_Tool.SBReader
{
    public partial class SBReader : Form
    {
        public SBFile sb;
        public CATFile cat;
        public CASFile cas;
        public SBReader()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.sb|*.sb";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sb = new SBFile(d.FileName);
                RefreshTree();
            }
        }

        public void RefreshTree()
        {
            if (sb == null)
                return;
            treeView1.Nodes.Clear();
            foreach (Tools.Entry e in sb.lines)
                treeView1.Nodes.Add(Tools.MakeEntry(new TreeNode(e.type.ToString("X")), e));
        }

        private void contextmenu_Opening(object sender, CancelEventArgs e)
        {
            nopeToolStripMenuItem.Visible = true;
            extractResourceToolStripMenuItem.Visible = false;
            if (treeView1.SelectedNode != null)
            {
                TreeNode t = treeView1.SelectedNode;
                if (t.Parent != null)
                {
                    TreeNode sha1t = null;
                    foreach(TreeNode child in t.Nodes)
                        if (child.Text == "sha1")
                        {
                            sha1t = t;
                            break;
                        }
                    if (sha1t != null)
                    {
                        nopeToolStripMenuItem.Visible = false;
                        extractResourceToolStripMenuItem.Visible = true;
                    }
                }
            }
        }

        private void nopeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("No operation");
        }

        private void getResourceTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            TreeNode t2 = new TreeNode();
            if (t.Text == "resType")
                t2 = t.Nodes[0];
            if (t.Parent != null && t.Parent.Text == "resType")
                t2 = t;
            string hex = t2.Text.Trim();
            hex = hex.Substring(2, hex.Length - 2);
            uint type = Convert.ToUInt32(hex, 16);
            string restype = Tools.GetResType(type);
            if (restype != "")
                MessageBox.Show("Resource type:\n" + restype);
            else
                MessageBox.Show("Resource type:\nunknown");
        }

        private void extractResourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null)
                return;
            TreeNode sha1t = null;
            foreach (TreeNode child in t.Nodes)
                if (child.Text == "sha1")
                {
                    sha1t = child;
                    break;
                }
            if (sha1t == null)
                return;
            string sha1 = sha1t.Nodes[0].Text;
            byte[] sha1b = Tools.StringToByteArray(sha1);
            if (cat == null)
            {
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "*.cat|*.cat";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    cat = new CATFile(d.FileName);
                else
                    return;
            }
            List<uint> catline = cat.FindBySHA1(sha1b);
            CASFile.CASEntry ce = new CASFile.CASEntry();
            if (catline.Count == 8)
            {
                if (cas != null && cas.casnumber == catline[7])
                    ce = cas.ReadEntry(catline.ToArray());
                else
                {
                    OpenFileDialog d = new OpenFileDialog();
                    string casname = CASFile.GetCASFileName(catline[7]);
                    d.Filter = casname + "|" + casname;
                    d.FileName = casname;
                    if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        cas = new CASFile(d.FileName);
                        ce = cas.ReadEntry(catline.ToArray());
                    }
                    else
                        return;
                }
            }            
            else
            {
                MessageBox.Show("SHA1 Not found!");
                return;
            }
            SaveFileDialog d2 = new SaveFileDialog();
            d2.Filter = "*.bin|*.bin";
            d2.FileName = sha1 + ".bin";
            if (d2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(d2.FileName, ce.data);
                MessageBox.Show("Done.");
                return;
            }
            return;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null) return;
            if (t.Text == "82")
            {
                TreeNode t2 = null;
                foreach(TreeNode child in t.Nodes)
                    if (child.Text == "sha1")
                    {
                        t2 = child;
                        break;
                    }
                if (t2 != null)
                {
                    string sha1 = t2.Nodes[0].Text;
                    byte[] sha1buff = Tools.StringToByteArray(sha1);
                    if (cat == null)
                    {
                        OpenFileDialog d = new OpenFileDialog();
                        d.Filter = "*.cat|*.cat";
                        if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            cat = new CATFile(d.FileName);
                        else
                            return;
                    }
                    List<uint> casline = cat.FindBySHA1(sha1buff);
                    if (casline.Count == 8)
                    {
                        if (cas == null)
                        {
                            string[] files = Directory.GetFiles(Path.GetDirectoryName(cat.MyPath));
                            foreach(string file in files)
                                if (Path.GetFileName(file) == CASFile.GetCASFileName(casline[7]))
                                {
                                    cas = new CASFile(file);
                                    break;
                                }
                        }
                        if (cas != null && cas.casnumber == casline[7])
                        {
                            CASFile.CASEntry ce = cas.ReadEntry(casline.ToArray());
                            hb1.ByteProvider = new DynamicByteProvider(ce.data);
                            hb1.BringToFront();
                            rtb1.Text = Tools.DecompileLUAC(ce.data);
                            if (rtb1.Text != "")
                                rtb1.BringToFront();
                        }
                    }
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string text = toolStripTextBox1.Text;
            SelectNext(text);
        }

        private void SelectNext(string text)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null && treeView1.Nodes.Count != 0)
                t = treeView1.Nodes[0];
            while (true)
            {
                TreeNode t2 = FindNext(t, text);
                if (t2 != null)
                {
                    treeView1.SelectedNode = t2;
                    return;
                }
                else if (t.Parent != null && t.Parent.NextNode != null)
                    t = t.Parent.NextNode;
                else if (t.Parent != null && t.Parent.NextNode == null)
                    while (t.Parent != null)
                    {
                        t = t.Parent;
                        if (t.Parent != null && t.Parent.NextNode != null)
                        {
                            t = t.Parent.NextNode;
                            break;
                        }
                    }
                else
                    return;
            }
        }

        private TreeNode FindNext(TreeNode t, string text)
        {
            foreach (TreeNode t2 in t.Nodes)
            {
                if (t2.Text == text)
                    return t2;
                if (t2.Nodes.Count != 0)
                {
                    TreeNode t3 = FindNext(t2, text);
                    if (t3 != null)
                        return t3;
                }
            }
            return null;
        }

        private void SBReader_Load(object sender, EventArgs e)
        {
            toolStripComboBox1.Items.Clear();
            foreach (KeyValuePair<uint, string> entry in Tools.ResTypes)
                toolStripComboBox1.Items.Add("0x" + entry.Key.ToString("X8") + " " + entry.Value);
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int n = toolStripComboBox1.SelectedIndex;
            if (n == -1)
                return;
            SelectNext(toolStripComboBox1.Items[n].ToString());
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sb == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.sb|*.sb";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sb.Save(d.FileName);
                MessageBox.Show("Done.");
            }
        }
    }
}
