using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAI_Tools.Frostbite;

namespace DAI_Tools.ScriptExplorer
{
    public partial class ScriptExplorer : Form
    {
        private bool init = false;
        private List<string> scriptPaths;
        private string prettyName = "";
        private string luaSrc = "";

        public ScriptExplorer()
        {
            InitializeComponent();
        }

        private void ScriptExplorer_Activated(object sender, EventArgs e)
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

            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = new SQLiteCommand("SELECT DISTINCT name FROM res WHERE rtype = 'AFECB022' ORDER BY name ", con).ExecuteReader();
            scriptPaths = new List<string>();
            while (reader.Read())
                scriptPaths.Add(reader.GetString(0));
            con.Close();
            MakeTree();

            const int tabPixels = 24;
            richTextBox1.SelectionTabs = new int[] { (tabPixels * 1), (tabPixels * 2), (tabPixels * 3), (tabPixels * 4), (tabPixels * 5), (tabPixels * 6), (tabPixels * 7), (tabPixels * 8) };
            init = true;
        }

        public void MakeTree()
        {
            treeView1.Nodes.Clear();
            TreeNode t = new TreeNode("Scripts");
            foreach (string path in scriptPaths)
                t = AddPath(t, path);
            t.Expand();
            treeView1.Nodes.Add(t);
        }

        public TreeNode AddPath(TreeNode t, string path)
        {
            string[] parts = path.Split('/');
            TreeNode f = null;
            foreach (TreeNode c in t.Nodes)
            {
                if (c.Text == parts[0])
                {
                    f = c;
                    break;
                }
            }
            if (f == null)
            {
                f = new TreeNode(parts[0]);
                t.Nodes.Add(f);
            }
            if (parts.Length > 1)
            {
                string subpath = path.Substring(parts[0].Length + 1, path.Length - 1 - parts[0].Length);
                f = AddPath(f, subpath);
            }
            return t;
        }

        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            prettyName = ""; luaSrc = "";
            try
            {
                TreeNode t = treeView1.SelectedNode;
                if (t == null || t.Nodes.Count != 0)
                    return;

                string path = t.Text;
                while (t.Parent.Text != "Scripts")
                {
                    t = t.Parent;
                    path = t.Text + "/" + path;
                };

                luaSrc = GetSource(path, out prettyName);
                richTextBox1.Text = luaSrc;
                richTextBox1.BringToFront();
            }
            catch (Exception ex)
            {
                Debug.Assert(false, String.Format("{0} Exception caught.", ex));
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if ((prettyName.Length != 0) && (luaSrc.Length != 0))
            {
                try
                {
                    SaveFileDialog d = new SaveFileDialog();
                    d.Filter = "*.lua|*.txt";
                    d.FileName = prettyName + ".lua";
                    if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        File.WriteAllText(d.FileName, luaSrc);
                        //MessageBox.Show("Done.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, String.Format("{0} Exception caught.", ex));
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if(scriptPaths.Count > 0)
            {
                try
                {
                    FolderBrowserDialog fd = new FolderBrowserDialog();
                    if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        richTextBox1.Text = String.Format("Exporting {0} Lua scripts..\n", scriptPaths.Count);
                        HashSet<string> hs = new HashSet<string>();

                        foreach (string path in scriptPaths) // Loop through List with foreach.
                        {
                            string fname;
                            string lSrc = GetSource(path, out fname);
                            Debug.Assert((lSrc.Length > 1) && (fname.Length > 1));
                            Debug.Assert(!hs.Contains(fname));
                            hs.Add(fname);
                            //System.Diagnostics.Debug.WriteLine(fname);
                            File.WriteAllText(fd.SelectedPath + "\\" + fname + ".lua", lSrc);
                        }

                        richTextBox1.Text = "Done.";
                    }
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, String.Format("{0} Exception caught.", ex));
                }
            }
        }

        private string GetSource(string path, out string scriptName)
        {
            string src = ""; scriptName = "";
            try
            {
                SQLiteConnection con = Database.GetConnection();
                con.Open();
                SQLiteDataReader reader = Database.getReader("SELECT * FROM res WHERE name='" + path + "'", con);
                reader.Read();
                string sha1 = reader.GetString(1);
                byte[] data = Database.getDataBySHA1(sha1, con);

                MemoryStream m = new MemoryStream(data);
                //File.WriteAllBytes("G:\\Temp6\\temp.luac", data);
                if (m.Length > 0x18)
                {
                    // DAI luac header
                    uint magic = Tools.ReadUInt(m);
                    if (magic == 0xE1850009)
                    {
                        try
                        {
                            m.Seek(0x18, 0);
                            string funcName = Tools.ReadNullString(m);
                            string funcArgs = Tools.ReadNullString(m);

                            // Lua 5.1 luac header
                            magic = Tools.ReadUInt(m);
                            if (magic == 0x61754C1B)
                            {
                                string[] pa = path.Split('/');
                                scriptName = pa[pa.Length - 2];

                                m.Seek(8 + 4, SeekOrigin.Current);
                                src = Tools.ReadNullString(m);
                                Debug.Assert(src.Length > 0);
                                return "\r\n-- Name: " + scriptName + "\r\n-- Func: " + funcName + "\r\n-- Path: \"" + path + "\"\r\n\r\n" + src;
                            }
                            else
                                Debug.Assert(false, "Bad luac header magic.");
                        }
                        catch (Exception ex)
                        {
                            Debug.Assert(false, String.Format("{0} Exception caught.", ex));
                        }
                    }
                    else
                        Debug.Assert(false, "Bad DIA luac header magic.");
                }
                else
                    Debug.Assert(false, "Bad DAI luac data length.");

                con.Close();
                return "";
            }
            catch (Exception ex)
            {
                Debug.Assert(false, String.Format("{0} Exception caught.", ex));
            }

            return src;
        }
    }
}
