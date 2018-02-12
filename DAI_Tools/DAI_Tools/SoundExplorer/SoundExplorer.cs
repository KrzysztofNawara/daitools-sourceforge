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

namespace DAI_Tools.SoundExplorer
{
    public partial class SoundExplorer : Form
    {
        public bool init = false;
        public List<SoundEntry> Sounds;
        public List<string> Languages;
        public TOCFile langTOC;       
        public SoundWaveAssetEntry CurrentSound;

        public struct DialogChunk
        {
            public byte[] id;
            public uint offset;
            public uint size;
        }

        public List<DialogChunk> dchunks;

        public struct SoundChunk
        {
            public byte[] id;
            public string sha1;
        }

        public struct SoundSegment
        {
            public int offset;
        }

        public class SoundWaveAssetEntry
        {
            public List<SoundChunk> chunks;
            public List<SoundSegment> segments;
            public string name;
            public string SoundPath;
        }

        public struct SoundEntry
        {
            public string path;
        }
        public SoundExplorer()
        {
            InitializeComponent();
        }

        public void Init()
        {
            if (GlobalStuff.FindSetting("isNew") == "1")
            {
                MessageBox.Show("Please initialize the database in Database Manager with Scan");
                this.BeginInvoke(new MethodInvoker(Close));
                return;
            }
            Sounds = new List<SoundEntry>();
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = new SQLiteCommand("SELECT * FROM langtocfiles WHERE path LIKE '%\\loc\\%' ORDER BY path ", con).ExecuteReader();
            toolStripComboBox1.Items.Clear();
            Languages = new List<string>();
            while (reader.Read())
                Languages.Add(reader.GetString(0));
            foreach (string l in Languages)
                toolStripComboBox1.Items.Add(Path.GetFileName(l));
            langTOC = new TOCFile(Languages[0]);
            LoadTOC();
            toolStripComboBox1.SelectedIndex = 0;
            reader = new SQLiteCommand("SELECT DISTINCT name FROM ebx WHERE type = 'SoundWaveAsset' ORDER BY name ", con).ExecuteReader();
            while (reader.Read())
            {
                SoundEntry e = new SoundEntry();
                e.path = reader.GetString(0);
                Sounds.Add(e);
            }
            con.Close();
            MakeTree();
            init = true;
        }

        public void MakeTree()
        {
            treeView1.Nodes.Clear();
            TreeNode t = new TreeNode("Sounds");
            foreach (SoundEntry e in Sounds)
                t = AddPath(t, e.path);
            t.Expand();
            treeView1.Nodes.Add(t);
        }

        public TreeNode AddPath(TreeNode t, string path)
        {
            string[] parts = path.Split('/');
            TreeNode f = null;
            foreach(TreeNode c in t.Nodes)
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

        public void LoadTOC()
        {
            dchunks = new List<DialogChunk>();
            foreach (Tools.Entry e in langTOC.lines)
                if (e.type == 0x82)
                    foreach (Tools.Field f in e.fields)
                        switch (f.fieldname)
                        {
                            case "chunks":
                                List<Tools.Entry> list = (List<Tools.Entry>)f.data;
                                foreach (Tools.Entry e2 in list)
                                    if (e.type == 0x82)
                                    {
                                        DialogChunk d = new DialogChunk();
                                        foreach (Tools.Field f2 in e2.fields)
                                            switch (f2.fieldname)
                                            {
                                                case "id":
                                                    d.id = (byte[])f2.data;
                                                    break;
                                                case "offset":
                                                    d.offset = (uint)BitConverter.ToUInt64((byte[])f2.data, 0);
                                                    break;
                                                case "size":
                                                    d.size = BitConverter.ToUInt32((byte[])f2.data, 0);
                                                    break;
                                            }
                                        dchunks.Add(d);
                                    }
                                break;
                        }
        }

        private void SoundExplorer_Activated(object sender, EventArgs e)
        {
            if (!init)
                Init();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            int n = toolStripComboBox1.SelectedIndex;
            if (n == -1)
                return;
            langTOC = new TOCFile(Languages[n]);
            LoadTOC();
            MessageBox.Show("Done.");
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null || t.Nodes.Count != 0)
                return;
            string path = t.Text;
            while (t.Parent.Text != "Sounds")
            {
                t = t.Parent;
                path = t.Text + "/" + path;
            }
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            SQLiteDataReader reader = Database.getReader("SELECT sha1 FROM ebx WHERE name='" + path + "'", con);
            reader.Read();
            string sha1 = reader.GetString(0);
            byte[] data = Database.getDataBySHA1(sha1, con);
            try
            {
                CurrentSound = GetSoundData(data, con);
                CurrentSound.SoundPath = path;
                listBox1.Items.Clear();
                foreach (SoundSegment seg in CurrentSound.segments)
                    listBox1.Items.Add("Segment at 0x" + seg.offset.ToString("X8"));
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message);
                con.Close();
                return;
            }
            con.Close();
        }

        private SoundWaveAssetEntry GetSoundData(byte[] data, SQLiteConnection con)
        {
            string x = Encoding.UTF8.GetString(Tools.ExtractEbx(new MemoryStream(data)));
            XmlDocument xml = new XmlDocument();
            xml.LoadXml("<xml>" + x + "</xml>");
            XmlNodeList list = xml.GetElementsByTagName("SoundWaveAsset");
            XmlNode node = list[0];
            SoundWaveAssetEntry sound = new SoundWaveAssetEntry();
            sound.chunks = new List<SoundChunk>();
            sound.segments = new List<SoundSegment>();
            if (node.Name == "SoundWaveAsset")
                foreach (XmlNode node2 in node.ChildNodes)
                    switch (node2.Name)
                    {
                        case "SoundDataAsset":
                            foreach (XmlNode node3 in node2.ChildNodes)
                                switch (node3.Name)
                                {
                                    case "Asset":
                                        foreach (XmlNode node4 in node3.ChildNodes)
                                            switch (node4.Name)
                                            {
                                                case "Name":
                                                    sound.name = node4.InnerText;
                                                    break;
                                            }
                                        break;
                                    case "Chunks":
                                        XmlNode members = node3.ChildNodes[0];
                                        if (members == null)
                                            continue;
                                        foreach (XmlNode node4 in members)
                                            switch (node4.Name)
                                            {
                                                case "SoundDataChunk":
                                                    SoundChunk chunk = new SoundChunk();
                                                    XmlNode nId = node4.ChildNodes[0];
                                                    chunk.id = Tools.StringToByteArray(nId.InnerText);
                                                    SQLiteDataReader reader = Database.getReader("SELECT sha1 FROM chunkids WHERE id='" + nId.InnerText + "'", con);
                                                    if (reader.Read())
                                                        chunk.sha1 = reader.GetString(0);
                                                    sound.chunks.Add(chunk);
                                                    break;
                                            }
                                        break;
                                }
                            break;
                        case "Segments":
                            foreach (XmlNode node3 in node2.ChildNodes)
                                switch (node3.Name)
                                {
                                    case "member":
                                        XmlNode swvseg = node3.ChildNodes[0];
                                        if (swvseg != null && swvseg.Name == "SoundWaveVariationSegment")
                                        {
                                            SoundSegment seg = new SoundSegment();
                                            XmlNode offset = swvseg.ChildNodes[0];
                                            seg.offset = Convert.ToInt32(offset.InnerText, 16);
                                            sound.segments.Add(seg);
                                        }
                                        break;
                                }
                            break;
                    }
            return sound;
        }

        private byte[] ExtractSound(int segment, bool wav)
        {
            CleanUP();
            if (segment == -1)
                return new byte[0];
            SoundWaveAssetEntry sound = CurrentSound;
            SoundSegment seg = sound.segments[segment];
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            byte[] data = new byte[0];
            if (sound.chunks[0].sha1 != null)
                data = Database.getDataBySHA1(sound.chunks[0].sha1, con);
            else
            {
                foreach (DialogChunk dc in dchunks)
                    if (Tools.ByteArrayCompare(sound.chunks[0].id, dc.id))
                    {
                        string path = Path.GetDirectoryName(langTOC.MyPath) + "\\" + Path.GetFileNameWithoutExtension(langTOC.MyPath) + ".sb";
                        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                        fs.Seek(dc.offset, 0);
                        data = new byte[dc.size];
                        fs.Read(data, 0, (int)dc.size);
                    }
            }
            con.Close();
            if (data.Length == 0)
                return new byte[0];
            int offset = seg.offset;
            if (offset < 0)
                return new byte[0];
            int toffset = -1;
            for (int i = offset; i >= 0; i--)
                if (data[i] == 0x48 &&
                    data[i + 1] == 0x00 &&
                    data[i + 2] == 0x00 &&
                    data[i + 3] == 0x0C)
                {
                    toffset = i;
                    break;
                }
            if (toffset == -1 || offset - toffset > 0x10000)
            {
                int toffset3 = -1;
                for (int i = offset; i < data.Length; i++)
                    if (data[i] == 0x48 &&
                        data[i + 1] == 0x00 &&
                        data[i + 2] == 0x00 &&
                        data[i + 3] == 0x0C)
                    {
                        toffset3 = i;
                        break;
                    }
                if (toffset3 != -1 && toffset3 - offset < 0x10000)
                    toffset = toffset3;

            }
            offset = toffset;
            int size = data.Length - offset;
            if (segment + 1 < sound.segments.Count)
            {
                int offset2 = sound.segments[segment + 1].offset;
                toffset = -1;
                for (int i = offset2; i > 0; i--)
                    if (data[i] == 0x48 &&
                        data[i + 1] == 0x00 &&
                        data[i + 2] == 0x00 &&
                        data[i + 3] == 0x0C)
                    {
                        toffset = i;
                        break;
                    }
                if (toffset == -1 || toffset == offset)
                {
                    int toffset3 = -1;
                    for (int i = offset2; i < data.Length; i++)
                        if (data[i] == 0x48 &&
                            data[i + 1] == 0x00 &&
                            data[i + 2] == 0x00 &&
                            data[i + 3] == 0x0C)
                        {
                            toffset3 = i;
                            break;
                        }
                    if (toffset3 != -1 && toffset3 - offset2 < 0x10000)
                        toffset = toffset3;
                }
                size = toffset - offset;
            }
            if (offset == -1)
                return data;
            string basepath = Application.StartupPath + "\\ealayer3\\";
            byte[] result = new byte[size];
            for (int i = 0; i < size; i++)
                result[i] = data[offset + i];
            File.WriteAllBytes(basepath + "temp.bin", result);
            if (wav)
                Tools.RunShell(basepath + "ealayer3.exe", "-w temp.bin");
            else
                Tools.RunShell(basepath + "ealayer3.exe", "temp.bin");
            return result;
        }

        private void CleanUP()
        {
            string basepath = Application.StartupPath + "\\ealayer3\\";
            Tools.DeleteFileIfExist(basepath + "temp.bin");
            Tools.DeleteFileIfExist(basepath + "temp.wav");
            Tools.DeleteFileIfExist(basepath + "temp.mp3");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                hb1.ByteProvider = new DynamicByteProvider(ExtractSound(listBox1.SelectedIndex, true));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.Message);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1) return;
            string basepath = Application.StartupPath + "\\ealayer3\\";
            if (ExtractSound(n, true).Length != 0 && File.Exists(basepath + "temp.wav"))
            {
                SoundPlayer sp = new SoundPlayer(basepath + "temp.wav");
                sp.PlaySync();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1) return;
            string basepath = Application.StartupPath + "\\ealayer3\\";
            if (ExtractSound(n, false).Length != 0 && File.Exists(basepath + "temp.mp3"))
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.mp3|*.mp3";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.Copy(basepath + "temp.mp3", d.FileName, true);
                    MessageBox.Show("Done.");
                }
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1) return;
            string basepath = Application.StartupPath + "\\ealayer3\\";
            if (ExtractSound(n, true).Length != 0 && File.Exists(basepath + "temp.wav"))
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.wav|*.wav";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.Copy(basepath + "temp.wav", d.FileName, true);
                    MessageBox.Show("Done.");
                }
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (CurrentSound == null)
                return;
            SoundWaveAssetEntry sound = CurrentSound;
            SQLiteConnection con = Database.GetConnection();
            con.Open();
            byte[] data = new byte[0];
            if (sound.chunks[0].sha1 != null)
                data = Database.getDataBySHA1(sound.chunks[0].sha1, con);
            else
            {
                foreach (DialogChunk dc in dchunks)
                    if (Tools.ByteArrayCompare(sound.chunks[0].id, dc.id))
                    {
                        string path = Path.GetDirectoryName(langTOC.MyPath) + "\\" + Path.GetFileNameWithoutExtension(langTOC.MyPath) + ".sb";
                        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                        fs.Seek(dc.offset, 0);
                        data = new byte[dc.size];
                        fs.Read(data, 0, (int)dc.size);
                    }
            }
            con.Close();
            if (data.Length == 0)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                hb1.ByteProvider = new DynamicByteProvider(data);
                File.WriteAllBytes(d.FileName, data);
                MessageBox.Show("Done.");
            }
        }
    }
}
