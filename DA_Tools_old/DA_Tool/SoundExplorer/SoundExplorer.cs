using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DA_Tool.Frostbite;
using System.Xml;
using Be.Windows.Forms;
using System.Media;

namespace DA_Tool.SoundExplorer
{
    public partial class SoundExplorer : Form
    {
        public SBFile sb;
        public CATFile cat;
        public TOCFile langTOC;
        public List<SoundWaveAssetEntry> sounds;

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
            public byte[] sha1;
        }

        public struct SoundSegment
        {
            public int offset;
        }

        public struct SoundWaveAssetEntry
        {
            public List<SoundChunk> chunks;            
            public List<SoundSegment> segments;
            public string name;
            public int Index;
        }

        public SoundExplorer()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.sb|*.sb";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                sb = new SBFile(d.FileName);
            else
                return;
            if (cat == null)
            {
                d.Filter = "*.cat|*.cat";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    cat = new CATFile(d.FileName);
            }
            if (langTOC == null)
            {
                d.Filter = "*.toc|*.toc";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    langTOC = new TOCFile(d.FileName);
                LoadTOC();
            }
            RefreshMe();
        }

        public void LoadTOC()
        {
            dchunks = new List<DialogChunk>();
            foreach(Tools.Entry e in langTOC.lines)
                if(e.type == 0x82)
                    foreach(Tools.Field f in e.fields)
                        switch (f.fieldname)
                        {
                            case "chunks":
                                List<Tools.Entry> list = (List<Tools.Entry>)f.data;
                                foreach(Tools.Entry e2 in list)
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

        public void RefreshMe()
        {
            if (sb == null || cat == null)
                return;
            listBox1.Items.Clear();
            foreach (Bundle b in sb.bundles)
                listBox1.Items.Add(b.path);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Bundle b = sb.bundles[n];
            listBox2.Items.Clear();
            sounds = new List<SoundWaveAssetEntry>();
            for (int i = 0; i < b.ebx.Count; i++)
            {
                Bundle.ebxtype ebx = b.ebx[i];
                try
                {
                    string x = Encoding.UTF8.GetString(Tools.ExtractEbx(new MemoryStream(Tools.GetDataBySHA1(ebx.SHA1, cat))));
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml("<xml>" + x + "</xml>");
                    XmlNodeList list = xml.GetElementsByTagName("SoundWaveAsset");
                    foreach (XmlNode node in list)
                    {
                        SoundWaveAssetEntry sound = new SoundWaveAssetEntry();
                        sound.chunks = new List<SoundChunk>();
                        sound.segments = new List<SoundSegment>();
                        sound.Index = i;
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
                                                                foreach (CATFile.ChunkType c in cat.chunks)
                                                                    if (Tools.ByteArrayCompare(c.id, chunk.id))
                                                                        chunk.sha1 = c.sha1;
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
                        sounds.Add(sound);
                    }
                }
                catch (Exception)
                {
                }
            }
            foreach (SoundWaveAssetEntry sound in sounds)
                listBox2.Items.Add(sound.name);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = listBox2.SelectedIndex;
            if (n == -1 || m == -1)
                return;
            Bundle b = sb.bundles[n];
            SoundWaveAssetEntry sound = sounds[m];
            Bundle.ebxtype ebx = b.ebx[sound.Index];
            string xml = Encoding.UTF8.GetString(Tools.ExtractEbx(new MemoryStream(Tools.GetDataBySHA1(ebx.SHA1, cat))));
            rtb1.Text = xml;
            if (sound.chunks.Count != 0 && sound.chunks[0].sha1!=null)
                hb1.ByteProvider = new DynamicByteProvider(Tools.GetDataBySHA1(sound.chunks[0].sha1,cat));
            if (sound.chunks.Count == 0)
                return;
            foreach(DialogChunk dc in dchunks)
                if (Tools.ByteArrayCompare(sound.chunks[0].id, dc.id))
                {
                    string basepath = Path.GetDirectoryName(langTOC.MyPath) + "\\" + Path.GetFileNameWithoutExtension(langTOC.MyPath) + ".sb";
                    FileStream fs = new FileStream(basepath, FileMode.Open, FileAccess.Read);
                    fs.Seek(dc.offset, 0);
                    byte[] buff = new byte[dc.size];
                    fs.Read(buff, 0, (int)dc.size);
                    hb1.ByteProvider = new DynamicByteProvider(buff);
                }
            listBox3.Items.Clear();
            if (sound.segments.Count != 0)
                for (int i = 0; i < sound.segments.Count; i++)
                    listBox3.Items.Add(i.ToString("d4") + " : Segment at 0x" + sound.segments[i].offset.ToString("X"));
        }

        private void extractHEXOfEBXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = listBox2.SelectedIndex;
            if (n == -1 || m == -1)
                return;
            Bundle b = sb.bundles[n];
            Bundle.ebxtype ebx = b.ebx[sounds[m].Index];
            byte[] data = Tools.GetDataBySHA1(ebx.SHA1, cat);
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if(d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(d.FileName,data);
                MessageBox.Show("Done.");
            }
        }

        private void extractHEXOfChunkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = listBox2.SelectedIndex;
            if (n == -1 || m == -1)
                return;
            Bundle b = sb.bundles[n];
            SoundWaveAssetEntry sound = sounds[m];
            if (sound.chunks.Count == 0 || sound.chunks[0].sha1 == null)
                return;
            byte[] data = Tools.GetDataBySHA1(sound.chunks[0].sha1, cat);
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if(d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(d.FileName,data);
                MessageBox.Show("Done.");
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            int a = listBox1.SelectedIndex;
            int b = listBox2.SelectedIndex;
            int c = listBox3.SelectedIndex;
            string basepath = Application.StartupPath + "\\ealayer3\\";
            ExtractSounds(a, b, c, true);
            if (File.Exists(basepath + "temp.wav"))
            {
                SoundPlayer sp = new SoundPlayer(basepath + "temp.wav");
                sp.Play();
            }
            try
            {
                CleanUP();
            }
            catch (Exception)
            {
            }
        }

        private void ExtractSounds(int a, int b, int c, bool wav)
        {
            if (a == -1 || b == -1 || c == -1)
                return;
            Bundle bun = sb.bundles[a];
            SoundWaveAssetEntry sound = sounds[b];
            SoundSegment seg = sound.segments[c];
            byte[] data = new byte[0];
            if (sound.chunks[0].sha1 != null)
                data = Tools.GetDataBySHA1(sound.chunks[0].sha1, cat);
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
            if (data.Length == 0)
                return;
            int offset = seg.offset;
            if (offset < 0)
                return;
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
            if (toffset == -1 || offset - toffset > 0x1000)
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
                if (toffset3 != -1 && toffset3 - offset < 0x1000)
                    toffset = toffset3;

            }
            offset = toffset;
            int size = data.Length - offset;
            if (c + 1 < sound.segments.Count)
            {
                int offset2 = sound.segments[c + 1].offset;
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
                    if (toffset3 != -1 && toffset3 - offset2 < 0x1000)
                        toffset = toffset3;
                }
                size = toffset - offset;
            }
            string basepath = Application.StartupPath + "\\ealayer3\\";
            byte[] result = new byte[size];
            for (int i = 0; i < size; i++)
                result[i] = data[offset + i];
            CleanUP();
            File.WriteAllBytes(basepath + "temp.bin", result);
            if (wav)
                Tools.RunShell(basepath + "ealayer3.exe", "-w temp.bin");
            else
                Tools.RunShell(basepath + "ealayer3.exe", "temp.bin");
        }

        private void CleanUP()
        {
            string basepath = Application.StartupPath + "\\ealayer3\\";
            Tools.DeleteFileIfExist(basepath + "temp.bin");
            Tools.DeleteFileIfExist(basepath + "temp.wav");
            Tools.DeleteFileIfExist(basepath + "temp.mp3");
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int a = listBox1.SelectedIndex;
            int b = listBox2.SelectedIndex;
            int c = listBox3.SelectedIndex;
            string basepath = Application.StartupPath + "\\ealayer3\\";
            ExtractSounds(a, b, c, false);
            if (File.Exists(basepath + "temp.mp3"))
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.mp3|*.mp3";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.Copy(basepath + "temp.mp3", d.FileName, true);
                    MessageBox.Show("Done.");
                }
            }
            CleanUP();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            int a = listBox1.SelectedIndex;
            int b = listBox2.SelectedIndex;
            int c = listBox3.SelectedIndex;
            string basepath = Application.StartupPath + "\\ealayer3\\";
            ExtractSounds(a, b, c, true);
            if (File.Exists(basepath + "temp.wav"))
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.wav|*.wav";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.Copy(basepath + "temp.wav", d.FileName, true);
                    MessageBox.Show("Done.");
                }
            }
            CleanUP();
        }

        
    }
}
