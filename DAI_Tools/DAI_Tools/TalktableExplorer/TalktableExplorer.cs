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

namespace DAI_Tools.TalktableExplorer
{
    public partial class TalktableExplorer : Form
    {
        public bool init = false;
        public List<string> languages = new List<string>();
        public SBFile language;
        public List<string> currtables;
        public CATFile cat;
        public Talktable talk;
        public string basepath = Application.StartupPath + "\\";

        public TalktableExplorer()
        {
            InitializeComponent();
        }

        private void TalktableExplorer_Activated(object sender, EventArgs e)
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
            string path = GlobalStuff.FindSetting("gamepath");
            path += "Data\\cas.cat";
            cat = new CATFile(path);
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = new SQLiteCommand("SELECT path FROM langsbfiles WHERE path LIKE '%\\loctext\\%' ORDER BY path ", con).ExecuteReader();
            toolStripComboBox1.Items.Clear();
            languages = new List<string>();
            while (reader.Read())
                languages.Add(reader.GetString(0));
            con.Close();
            foreach (string l in languages)
                toolStripComboBox1.Items.Add(Path.GetFileNameWithoutExtension(l));
            toolStripComboBox1.SelectedIndex = 0;
            language = new SBFile(languages[0]);
            MakeTree();
            init = true;
        }

        public void MakeTree()
        {
            treeView1.Nodes.Clear();
            TreeNode t = new TreeNode("Talktable Bundles");
            foreach (Bundle b in language.bundles)
                foreach (Bundle.restype res in b.res)
                    if (BitConverter.ToUInt32(res.rtype, 0) == 0x5e862e05) 
                        t = AddPath(t, res.name);
            t.Expand();
            treeView1.Nodes.Add(t);
        }

        public TreeNode AddPath(TreeNode t, string path)
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
                t.Nodes.Add(f);
            }
            if (parts.Length > 1)
            {
                string subpath = path.Substring(parts[0].Length + 1, path.Length - 1 - parts[0].Length);
                f = AddPath(f, subpath);
            }
            return t;
        }

        private TreeNode FindNext(TreeNode start)
        {
            TreeNode sub = FindNextSub(start);
            if (sub != null && sub != start)
                return sub;
            TreeNode next = start.NextNode;
            if (next != null)
                return FindNextSub(next);
            TreeNode p = start.Parent;
            while (p != null)
            {
                if (p.Parent != null && p.NextNode != null)
                    return p.NextNode;
                p = p.Parent;
            }
            return null;
        }

        private TreeNode FindNextSub(TreeNode start)
        {
            if(start.Nodes.Count != 0)
                return start.Nodes[0];
            else
                return start;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            int n = toolStripComboBox1.SelectedIndex;
            if (n == -1)
                return;
            language = new SBFile(languages[n]);
            MakeTree();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null || t.Parent == null)
                return;
            string path = t.Text;
            while (t.Parent.Text != "Talktable Bundles")
            {
                t = t.Parent;
                path = t.Text + "/" + path;
            }
            foreach (Bundle b in language.bundles)
                foreach (Bundle.restype res in b.res)
                    if (BitConverter.ToUInt32(res.rtype, 0) == 0x5e862e05)
                        if (res.name == path)
                        {
                            byte[] data = Tools.GetDataBySHA1(res.SHA1, cat);
                            talk = new Talktable();
                            talk.Read(new MemoryStream(data));
                            RefreshMe();
                        }
        }

        private void RefreshMe()
        {
            listBox2.Items.Clear();
            int count = 0;
            listBox2.BeginUpdate();
            foreach (STR line in talk.Strings)
                listBox2.Items.Add((count++).ToString("d4") + " " + line.ID.ToString("X8") + " : " + line.Value.Replace("\n","[/n]").Replace("\r","[/r]"));
            listBox2.EndUpdate();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void Search()
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null)
                t = treeView1.Nodes[0];
            string search = toolStripTextBox1.Text;
            while ((t = FindNext(t)) != null)
            {
                Application.DoEvents();
                string path = GetPath(t);
                status.Text = "Searching : " + GetPath(t) + "...";
                foreach (Bundle b in language.bundles)
                    foreach (Bundle.restype res in b.res)
                        if (BitConverter.ToUInt32(res.rtype, 0) == 0x5e862e05)
                            if (res.name == path)
                            {
                                byte[] data = Tools.GetDataBySHA1(res.SHA1, cat);
                                if (talk == null)
                                    talk = new Talktable();
                                talk.Read((new MemoryStream(data)));
                                for (int j = 0; j < talk.Strings.Count; j++)
                                    if (talk.Strings[j].Value.Contains(search))
                                    {
                                        status.Text = "";
                                        treeView1.SelectedNode = t;
                                        listBox2.SelectedIndex = j;
                                        return;
                                    }
                            }
            }
            status.Text = "";
        }

        private string GetPath(TreeNode t)
        {
            if (t.Parent != null && t.Parent.Text != "Talktable Bundles")
                return GetPath(t.Parent) + "/" + t.Text;
            else
                return t.Text;
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                Search();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    talk = new Talktable();
                    talk.Read(new MemoryStream(File.ReadAllBytes(d.FileName)));
                    RefreshMe();
                    MessageBox.Show("Done","Loading");
                }
                catch (Exception)
                {
                    MessageBox.Show("Error on loading!");
                }
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream fs = new FileStream(d.FileName, FileMode.Create, FileAccess.Write);
                talk.Save(fs);
                fs.Close();
                MessageBox.Show("Done", "Saving");
            }
        }

        private void openInEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(talk == null)
                return;
            TalktableEditor.TalktableEditor editor = new TalktableEditor.TalktableEditor();
            editor.MdiParent = this.MdiParent;
            editor.Show();
            editor.WindowState = FormWindowState.Maximized;
            editor.talk = talk;
            MemoryStream m = new MemoryStream();
            talk.Save(m);
            editor.raw = m.ToArray();
            editor.RefreshMe();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (talk == null || t == null )
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.daimod|*.daimod";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MakeMod(d.FileName);
                MessageBox.Show("Done.");
            }
        }

        private void MakeMod(string path)
        {
            Mod mod = new Mod();
            mod.jobs = new List<Mod.Modjob>();
            Mod.Modjob job = new Mod.Modjob();
            job.name = "Talktable replacement";
            MemoryStream m = new MemoryStream();
            talk.Save(m);
            job.data = new List<byte[]>();
            job.data.Add(m.ToArray());
            job.script = File.ReadAllText(basepath + "templates\\empty_script.cs");
            string uuid = System.Guid.NewGuid().ToString().ToUpper();
            Mod.ModDetail detail = new Mod.ModDetail("Talktable replacement", 1, Mod.GetOrSetAuthor(), "replaces dialog text");
            List<Mod.ModBundle> bundles = new List<Mod.ModBundle>();
            TreeNode t = treeView1.SelectedNode;
            string rpath = t.Text;
            while (t.Parent.Text != "Talktable Bundles")
            {
                t = t.Parent;
                rpath = t.Text + "/" + rpath;
            }
            Bundle.restype resource = new Bundle.restype();
            string bpath = "";
            foreach (Bundle b in language.bundles)
                if(resource.name != rpath)
                    foreach (Bundle.restype res in b.res)
                        if (BitConverter.ToUInt32(res.rtype, 0) == 0x5e862e05)
                            if (res.name == path)
                            {
                                resource = res;
                                bpath = b.path;
                                break;
                            }
            List<Mod.ModBundleEntry> entries = new List<Mod.ModBundleEntry>();
            Mod.ModBundleEntry entry = new Mod.ModBundleEntry(rpath, "modefy", Tools.ByteArrayToString(resource.SHA1), 0);
            entries.Add(entry);
            bundles.Add(new Mod.ModBundle(bpath, "modefy", entries));
            job.meta = new Mod.ModMetaData(1, uuid, detail, new List<Mod.ModReq>(), bundles);
            job.xml = Mod.MakeXMLfromJobMeta(job.meta);
            mod.jobs.Add(job);
            mod.Save(path);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {            
            if (talk == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.xml|*.xml";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<xml>\r\n");
                foreach (STR line in talk.Strings)
                    sb.AppendFormat(" <entry><id>{0}</id><text>{1}</text></entry>\r\n", line.ID.ToString("X8"), XMLHelper.toXML(line.Value));
                sb.Append("</xml>");
                File.WriteAllText(d.FileName, sb.ToString());
                MessageBox.Show("Done.", "Saving");
            }
        }        

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (talk == null)
                return;
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.xml|*.xml";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string xml = File.ReadAllText(d.FileName);
                string xsd = File.ReadAllText(basepath + "templates\\talkXML.xsd");
                try
                {
                    if (!XMLHelper.Validate(xml, xsd))
                        throw new Exception();
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    XmlNode root = dom.ChildNodes[0];
                    List<STR> list = new List<STR>();
                    foreach (XmlNode n0 in root.ChildNodes)
                    {
                        STR entry = new STR();
                        foreach (XmlNode n1 in n0.ChildNodes)
                        {
                            switch (n1.Name)
                            {
                                case "id":
                                    entry.ID = Convert.ToUInt32(n1.InnerText, 16);
                                    break;
                                case "text":
                                    entry.Value = XMLHelper.fromXML(n1.InnerText);
                                    break;
                            }
                        }
                        list.Add(entry);
                    }
                    talk.Strings = list;
                    RefreshMe();
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid XML!");
                    return;
                }
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (talk == null)
                return;
            int n = listBox2.SelectedIndex;
            STR line = talk.Strings[n];
            rtb1.Text = line.Value;
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {            
            int n = listBox2.SelectedIndex;
            if (talk == null || n == -1)
                return;
            STR line = talk.Strings[n];
            line.Value = rtb1.Text;
            talk.Strings[n] = line;
            RefreshMe();
        }
    }
}
