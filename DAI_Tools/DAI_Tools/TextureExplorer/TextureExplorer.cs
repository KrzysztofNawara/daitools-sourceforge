using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAI_Tools.Frostbite;
using Be.Windows.Forms;
using DevIL;

namespace DAI_Tools.TextureExplorer
{
    public partial class TextureExplorer : Form
    {

        public bool init = false;
        public List<string> TexPaths;
        public string lastname;

        public TextureExplorer()
        {
            InitializeComponent();
        }

        private void TextureExplorer_Activated(object sender, EventArgs e)
        {
            if (!init)
                Init();
        }
        public void Init()
        {
            if (GlobalStuff.FindSetting("isNew") == "1")
            {
                MessageBox.Show("Please initialize the database in Misc > Database with Scan");
                this.BeginInvoke(new MethodInvoker(Close));
                return;
            }
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = new SQLiteCommand("SELECT DISTINCT name FROM res WHERE rtype = '5C4954A6' ORDER BY name ", con).ExecuteReader();
            TexPaths = new List<string>();
            while (reader.Read())
                TexPaths.Add(reader.GetString(0));
            con.Close();
            MakeTree();
            init = true;
        }

        public void MakeTree()
        {
            treeView1.Nodes.Clear();
            TreeNode t = new TreeNode("Textures");
            foreach (string path in TexPaths)
                t = AddPath(t, path);
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

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode t = treeView1.SelectedNode;
                if (t == null || t.Nodes.Count != 0)
                    return;
                string path = t.Text;
                while (t.Parent.Text != "Textures")
                {
                    t = t.Parent;
                    path = t.Text + "/" + path;
                }
                lastname = path.Replace("/", "\\");
                lastname = Path.GetFileName(lastname);
                SQLiteConnection con = Database.GetConnection();
                con.Open();
                SQLiteDataReader reader = Database.getReader("SELECT * FROM res WHERE name='" + path + "'", con);
                reader.Read();
                string sha1 = reader.GetString(1);
                byte[] header = Database.getDataBySHA1(sha1, con);
                hexBox1.ByteProvider = new DynamicByteProvider(header);
                con.Close();
                string basepath = Application.StartupPath + "\\temp\\temp.dds";
                if (File.Exists(basepath))
                    File.Delete(basepath);
                ExportTexture(header, basepath);
                if (File.Exists(basepath))
                    pb1.Image = DevIL.DevIL.LoadBitmap(basepath);
            }
            catch (Exception)
            {
            }
        }

        public void ExportTexture(byte[] header, string filepath)
        {
            TextureInfo t = new TextureInfo();
            MemoryStream TexBuffer = new MemoryStream(header);
            TexBuffer.Seek(12, SeekOrigin.Begin);
            t.pixelFormatID = Tools.ReadUInt(TexBuffer);
            TexBuffer.Seek(2, SeekOrigin.Current);
            t.textureWidth = Tools.ReadUShort(TexBuffer);
            t.textureHeight = Tools.ReadUShort(TexBuffer);
            TexBuffer.Seek(4, SeekOrigin.Current);
            t.sizes = (uint)TexBuffer.ReadByte();
            TexBuffer.Seek(1, SeekOrigin.Current);
            StringBuilder id = new StringBuilder();
            for (int j = 0; j < 16; j++)
                id.Append(((byte)TexBuffer.ReadByte()).ToString("X2"));
            t.mipSizes = new List<uint>();
            for (int mipCount = 0; mipCount < Math.Min(14, t.sizes); mipCount++)
                t.mipSizes.Add(Tools.ReadUInt(TexBuffer));
            DAITexture.SetPixelFormatData(ref t, t.pixelFormatID);
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            string chunksha1 = "";
            SQLiteDataReader reader = Database.getReader("SELECT sha1 FROM chunk WHERE id='" + id + "'", con);
            if (reader.Read())
                chunksha1 = reader.GetString(0);
            else
            {
                reader = Database.getReader("SELECT sha1 FROM chunkids WHERE id='" + id + "'", con);
                if (reader.Read())
                    chunksha1 = reader.GetString(0);
                else
                    return;
            }
            byte[] data = Database.getDataBySHA1(chunksha1, con);
            con.Clone();
            MemoryStream outputStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(outputStream);
            DAITexture.WriteTextureHeader(t, writer);
            writer.Write(data);
            writer.Close();
            File.WriteAllBytes(filepath, outputStream.ToArray());
        }

        private void TextureExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            string basepath = Application.StartupPath + "\\temp\\temp.dds";
            if (File.Exists(basepath))
                File.Delete(basepath);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string basepath = Application.StartupPath + "\\temp\\temp.dds";
            if (File.Exists(basepath))
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.dds|*.dds";
                d.FileName = lastname + ".dds";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.Copy(basepath, d.FileName, true);
                    MessageBox.Show("Done.");
                }
            }
        }
    }
}
