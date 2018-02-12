using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using DAI_Tools.Frostbite;
using Be.Windows.Forms;

namespace DAI_Tools.EBXExplorer
{
    public partial class EBXExplorer : Form
    {
        public bool init = false;
        public CATFile cat;
        public bool stop = false;
        public bool ignoreonce = false;

        public struct EBXEntry
        {
            public string path;
            public string sha1;
        }

        public List<EBXEntry> EBXList;

        public EBXExplorer()
        {
            InitializeComponent();
        }

        private void EBXExplorer_Activated(object sender, EventArgs e)
        {
            if (!init)
                Init();
        }

        public void Init()
        {
            if (GlobalStuff.FindSetting("isNew") == "1")
            {
                MessageBox.Show("Please initialize the database in Database Manager with Scan");
                this.BeginInvoke(new MethodInvoker(Close));
                return;
            }
            rtb1.Text = "Loading CAT for faster lookup...\n";
            Application.DoEvents();
            string path = GlobalStuff.FindSetting("gamepath");
            path += "Data\\cas.cat";
            cat = new CATFile(path);
            EBXList = new List<EBXEntry>();
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            rtb1.AppendText("Querying...\n");
            this.WindowState = FormWindowState.Maximized;
            Application.DoEvents();
            SQLiteDataReader reader = new SQLiteCommand("SELECT DISTINCT name,sha1 FROM ebx", con).ExecuteReader();
            while (reader.Read())
            {
                EBXEntry e = new EBXEntry();
                e.path = reader.GetString(0);
                e.sha1 = reader.GetString(1);
                EBXList.Add(e);
            }
            con.Close();
            rtb1.AppendText("Making Tree...\n");
            Application.DoEvents();
            MakeTree();            
            rtb1.Text = "Done.";
            init = true;
        }

        public void MakeTree()
        {
            treeView1.Nodes.Clear();
            TreeNode t = new TreeNode("EBX");
            foreach (EBXEntry e in EBXList)
                t = AddPath(t, e.path, e.sha1);
            t.Expand();
            treeView1.Nodes.Add(t);
            treeView1.Sort();
        }

        public TreeNode AddPath(TreeNode t, string path, string sha1)
        {
            string[] parts = path.Split('/');
            TreeNode f = null;
            foreach (TreeNode c in t.Nodes)
                if (c.Text == parts[0])
                {
                    f = c;
                    break;
                }
            if (f == null)
            {
                f = new TreeNode(parts[0]);
                if (parts.Length == 1)
                    f.Name = sha1;
                else
                    f.Name = "";
                t.Nodes.Add(f);
            }
            if (parts.Length > 1)
            {
                string subpath = path.Substring(parts[0].Length + 1, path.Length - 1 - parts[0].Length);
                f = AddPath(f, subpath, sha1);
            }
            return t;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (ignoreonce)
                {
                    ignoreonce = false;
                    return;
                }
                TreeNode t = treeView1.SelectedNode;
                if (t == null || t.Name == "")
                    return;
                string sha1 = t.Name;
                byte[] data = Tools.GetDataBySHA1(sha1, cat);
                rtb1.Text = Encoding.UTF8.GetString(Tools.ExtractEbx(new MemoryStream(data)));
            }
            catch (Exception)
            {
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null)
                t = treeView1.Nodes[0];
            string search = toolStripTextBox1.Text;
            toolStripButton2.Visible = true;
            stop = false;
            while ((t = FindNext(t)) != null)
            {
                Application.DoEvents();
                status.Text = "Searching : " + GetPath(t) + "...";
                if (stop)
                {
                    status.Text = "";
                    toolStripButton2.Visible = false;
                    return;
                }
                try
                {
                    byte[] data = Tools.GetDataBySHA1(t.Name, cat);
                    if (data.Length != 0)
                    {
                        string xml = Encoding.UTF8.GetString(Tools.ExtractEbx(new MemoryStream(data)));
                        if (xml.Contains(search))
                        {
                            ignoreonce = true;
                            treeView1.SelectedNode = t;
                            rtb1.Text = xml;
                            int start = xml.IndexOf(search);
                            rtb1.SelectionStart = start;
                            rtb1.ScrollToCaret();
                            rtb1.SelectionLength = search.Length;
                            status.Text = "";
                            toolStripButton2.Visible = false;
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            toolStripButton2.Visible = false;
            status.Text = "Not found.";
        }

        private string GetPath(TreeNode t)
        {
            if (t.Parent != null && t.Parent.Text != "EBX")
                return GetPath(t.Parent) + "/" + t.Text;
            else
                return t.Text;
        }

        private TreeNode FindNext(TreeNode start)
        {
            TreeNode t = FindNextSub(start);
            if (t != null && t != start)
                return t;
            TreeNode next = start.NextNode;
            if (next != null)
            {
                if (next.Name != "")
                    return next;
                t = FindNext(next);
                if (t != null)
                    return t;
            }
            TreeNode p = start.Parent;
            while (p != null)
            {
                if (p.Parent != null && p.NextNode != null)
                {
                    t = FindNext(p.NextNode);
                    if (t != null)
                        return t;
                }
                p = p.Parent;
            }
            return null;
        }

        private TreeNode FindNextSub(TreeNode start)
        {
            foreach (TreeNode n in start.Nodes)
            {
                TreeNode t = FindNextSub(n);
                if (t != null)
                    return t;
            }
            if (start.Name != "")
                return start;
            else
                return null;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        private void EBXExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            stop = true;
        }
    }
}
