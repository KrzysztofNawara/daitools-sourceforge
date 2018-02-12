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

namespace DAI_Tools.ShaderExplorer
{
    public partial class ShaderExplorer : Form
    {
        public bool init = false;
        public Dictionary<string, string> shaderDatabases = new Dictionary<string, string>();
        public SBFile language;
        public List<string> currtables;
        public CATFile cat;

        public ShaderExplorer()
        {
            InitializeComponent();
        }

        private void ShaderExplorer_Activated(object sender, EventArgs e)
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
            SQLiteDataReader reader = new SQLiteCommand("SELECT name,sha1 FROM res WHERE rtype='36F3F2C0' ORDER BY name", con).ExecuteReader();
            shaderDatabases = new Dictionary<string, string>();
            
            while (reader.Read())
                if(!shaderDatabases.ContainsKey(reader.GetString(0)))
                    shaderDatabases.Add(reader.GetString(0), reader.GetString(1));

            con.Close();
            MakeTree();
            init = true;
        }

        public void MakeTree()
        {
            treeView1.Nodes.Clear();
            TreeNode t = new TreeNode("Shader Databases");
            foreach (string shaderdb in shaderDatabases.Keys)
                t = AddPath(t, shaderdb.Remove(shaderdb.Length-9));
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
                return FindNextSub(start.Nodes[0]);
            else
                return start;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null || t.Nodes.Count != 0)
                return;
            string path = t.Text;
            while (t.Parent.Text != "Shader Databases")
            {
                t = t.Parent;
                path = t.Text + "/" + path;
            }

            if (shaderDatabases.ContainsKey(path + "/shaderdb"))
            {
                rtb1.Clear();

                byte[] data = Tools.GetDataBySHA1(shaderDatabases[path + "/shaderdb"], cat);
                ExtractShaderDb(new MemoryStream(data));

                try
                {
                    hb1.ByteProvider = new DynamicByteProvider(data);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\n" + ex.Message);
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void Search()
        {
        }

        private string GetPath(TreeNode t)
        {
            if (t.Parent != null && t.Parent.Text != "Shader Databases")
                return GetPath(t.Parent) + "/" + t.Text;
            else
                return t.Text;
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                Search();
        }

        private string ReadString(Stream s, int numChars)
        {
            string retVal = "";
            for(int i = 0; i < numChars; i++)
            {
                char c = (char)s.ReadByte();
                if (c != 0x00)
                    retVal += c;
            }

            return retVal;
        }

        private void ExtractShaderDb(Stream s)
        {
            s.Seek(0x18, SeekOrigin.Begin);
            int numEntries = Tools.ReadInt(s);

            rtb1.AppendText("Num Entries: " + numEntries + "\n");

            long offset = s.Position;
            for (int i = 0; i < numEntries; i++)
            {
                long entrySize = Tools.ReadLong(s);
                long[] offsets = new long[6];
                for (int x = 0; x < 6; x++)
                    offsets[x] = Tools.ReadLong(s);

                s.Seek(0x05, SeekOrigin.Current);
                int textureCount = s.ReadByte();
                int paramCount = s.ReadByte();

                rtb1.AppendText("Entry #" + i.ToString() + "\n");
                rtb1.AppendText(" Texture Count: " + textureCount + "\n");

                // textures
                s.Seek(offset + offsets[1], SeekOrigin.Begin);
                for (int x = 0; x < textureCount; x++)
                {
                    long textureIndex = Tools.ReadLong(s);
                    string textureName = ReadString(s, 136);
                    uint textureHash = Tools.ReadUInt(s);
                    uint textureUnknown = Tools.ReadUInt(s);

                    rtb1.AppendText("  " + textureIndex + ". " + textureName + " [" + textureHash.ToString("X8") + "]\n");
                }

                rtb1.AppendText(" Parameter Count: " + paramCount + "\n");

                // params
                s.Seek(offset + offsets[2], SeekOrigin.Begin);
                for (int x = 0; x < paramCount; x++)
                {
                    string paramName = ReadString(s, 32);
                    uint paramHash = Tools.ReadUInt(s);
                    int paramIndex = Tools.ReadShort(s);
                    int paramUnknown1 = Tools.ReadShort(s);
                    int paramUnknown2 = Tools.ReadInt(s);

                    rtb1.AppendText("  " + paramIndex + ". " + paramName + " [" + paramHash.ToString("X8") + "]: ");

                    for (int y = 0; y < 4; y++)
                    {
                        float flvalue = Tools.ReadFloat(s);
                        rtb1.AppendText(flvalue.ToString("F3") + ((y < 3) ? ", " : ""));
                    }
                    rtb1.AppendText("\n");
                }

                s.Seek(offset + entrySize, SeekOrigin.Begin);
                offset = s.Position;
            }
        }
    }
}
