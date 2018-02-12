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

namespace DA_Tool.ResExplorer
{
    public partial class ResExplorer : Form
    {
        public struct DDSPixelFormat
        {
            public int dwSize;
            public int dwFlags;
            public int dwFourCC;
            public int dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;
        }

        public struct TextureInfo
        {
            public string fullpath;
            public string name;
            public string path;
            public byte[] sha1;
            public List<uint> catline;
            public ChunkInfo chunk;
            public uint pixelFormatID;
            public uint textureWidth;
            public uint textureHeight;
            public uint sizes;
            public List<uint> mipSizes;
            public DDSPixelFormat pixelFormat;
            public uint caps2;
        }

        public struct ChunkInfo
        {
            public byte[] id;
            public byte[] sha1;
            public List<uint> catline;
        }

        public CATFile cat;
        public SBFile sb;
        public List<TextureInfo> listTex;
        public List<ChunkInfo> listChunks;

        public static Dictionary<uint, int> PixelFormatTypes = new Dictionary<uint, int>()
        {
            { 0x00, 0x31545844 },
            { 0x01, 0x31545844 },
            { 0x03, 0x35545844 },
            { 0x04, 0x31495441 },
            { 0x10, 0x74 },
            { 0x13, 0x32495441 },
            { 0x14, 0x53354342 },
        };


        public ResExplorer()
        {
            InitializeComponent();
        }

        private void opensbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.sb|*.sb";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sb = new SBFile(d.FileName);
                RefreshMe();
            }
        }

        public void RefreshMe()
        {
            if (sb == null)
                return;
            listTex = new List<TextureInfo>();
            listChunks = new List<ChunkInfo>();
            listBox1.Items.Clear();
            rtb1.Text = "Searching for Textures...";
            Application.DoEvents();
            foreach (Tools.Entry e in sb.lines)
                if (e.type == 0x82)
                {
                    foreach (Tools.Field f in e.fields)
                        if (f.fieldname == "bundles" && f.type == 1)
                            FindTextures((List<Tools.Entry>)f.data);
                }
            rtb1.AppendText("done.\nLoading cat file...");
            Application.DoEvents();
            if (cat == null)
            {
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "*.cat|*.cat";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    cat = new CATFile(d.FileName);
                }
                else
                {
                    rtb1.AppendText("aborted.");
                    return;
                }
            }
            rtb1.AppendText("done.\nSearching cat for SHA1s...");
            pb1.Maximum = listTex.Count + listChunks.Count;
            pb1.Value = 0;
            for (int i = 0; i < listTex.Count; i++)
            {
                pb1.Value++;
                if ((i & 10) == 0)
                    Application.DoEvents();
                TextureInfo t = listTex[i];
                List<uint> res = cat.FindBySHA1(t.sha1);
                if (res.Count == 8)
                {
                    t.catline = res;
                    listTex[i] = t;
                }
            }
            for (int i = 0; i < listChunks.Count; i++)
            {
                pb1.Value++;
                if ((i & 10) == 0)
                    Application.DoEvents();
                ChunkInfo c = listChunks[i];
                List<uint> res = cat.FindBySHA1(c.sha1);
                if (res.Count == 8)
                {
                    c.catline = res;
                    listChunks[i] = c;
                }
            }
            rtb1.AppendText("done.\nLinking Textures and Chunks...");
            pb1.Value = 0;
            Application.DoEvents();
            LinkTexAndChunks();
            rtb1.AppendText("done.\nFinished!");
            Application.DoEvents();
            foreach (TextureInfo t in listTex)
                listBox1.Items.Add(t.fullpath);
        }

        private void LinkTexAndChunks()
        {
            if (cat == null)
                return;
            string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";
            for (int i = 0; i < listTex.Count; i++)
            {
                TextureInfo t = listTex[i];
                if (t.catline != null && CheckCASExist(basepath, t.catline[7]))
                {
                    CASFile cas = new CASFile(CASFile.GetCASFileName(basepath, t.catline[7]));
                    CASFile.CASEntry e = cas.ReadEntry(t.catline.ToArray());
                    byte[] id = null;
                    LoadTextureData(ref t, ref id, e.data);
                    LoadChunkData(ref t, i, id);
                }
            }
        }

        private void LoadChunkData(ref TextureInfo t, int i, byte[] id)
        {
            foreach (ChunkInfo c in listChunks)
            {
                bool match = true;
                for (int j = 0; j < 16; j++)
                    if (c.id[j] != id[j])
                    {
                        match = false;
                        break;
                    }
                if (match)
                {
                    t.chunk = c;
                    listTex[i] = t;
                    break;
                }
            }
        }

        private void LoadTextureData(ref TextureInfo t, ref byte[] id, byte[] casData)
        {
            using (MemoryStream m = new MemoryStream(casData))
            {
                m.Seek(0x70, SeekOrigin.Begin);
                t.name = Tools.ReadNullString(m);

                m.Seek(12, SeekOrigin.Begin);
                t.pixelFormatID = Tools.ReadUInt(m);
                m.Seek(2, SeekOrigin.Current);
                t.textureWidth = Tools.ReadUShort(m);
                t.textureHeight = Tools.ReadUShort(m);
                m.Seek(4, SeekOrigin.Current);
                t.sizes = (uint)m.ReadByte();
                m.Seek(1, SeekOrigin.Current);
                id = new byte[16];
                for (int j = 0; j < 16; j++)
                {
                    id[j] = (byte)m.ReadByte();
                }
                t.mipSizes = new List<uint>();
                for (int mipCount = 0; mipCount < Math.Min(14, t.sizes); mipCount++)
                {
                    t.mipSizes.Add(Tools.ReadUInt(m));
                }


                SetPixelFormatData(ref t, t.pixelFormatID);
            }
        }

        private void SetPixelFormatData(ref TextureInfo t, uint pixelFormatID)
        {
            t.caps2 = 0;
            t.pixelFormat.dwSize = 32;
            t.pixelFormat.dwFlags = 4;
            t.pixelFormat.dwFourCC = 0x31545844;
            t.pixelFormat.dwRGBBitCount = 0;
            t.pixelFormat.dwRBitMask = 0;
            t.pixelFormat.dwGBitMask = 0;
            t.pixelFormat.dwBBitMask = 0;
            t.pixelFormat.dwABitMask = 0;

            if (PixelFormatTypes.ContainsKey(pixelFormatID))
            {
                t.pixelFormat.dwFourCC = PixelFormatTypes[pixelFormatID];
                if (pixelFormatID == 0x01)
                {
                    t.pixelFormat.dwFlags |= 0x01;
                }
            }
            else
            {
                switch (pixelFormatID)
                {
                    case 0x0B:
                    case 0x36:
                        t.pixelFormat.dwFourCC = 0x00;
                        t.pixelFormat.dwRGBBitCount = 0x20;
                        t.pixelFormat.dwRBitMask = 0xFF;
                        t.pixelFormat.dwGBitMask = 0xFF00;
                        t.pixelFormat.dwBBitMask = 0xFF0000;
                        t.pixelFormat.dwABitMask = 0xFF000000;
                        t.pixelFormat.dwFlags = 0x41;
                        if (pixelFormatID == 0x36)
                        {
                            t.caps2 = 0xFE00;
                        }
                        break;
                    case 0x0C:
                        t.pixelFormat.dwFourCC = 0x00;
                        t.pixelFormat.dwRGBBitCount = 0x08;
                        t.pixelFormat.dwABitMask = 0xFF;
                        t.pixelFormat.dwFlags = 0x02;
                        break;
                    case 0x0D:
                        t.pixelFormat.dwFourCC = 0x00;
                        t.pixelFormat.dwRGBBitCount = 0x10;
                        t.pixelFormat.dwRBitMask = 0xFFFF;
                        t.pixelFormat.dwFlags = 0x20000;
                        break;
                }
            }
        }

        private bool CheckCASExist(string basepath, uint id)
        {
            return File.Exists(CASFile.GetCASFileName(basepath, id));
        }

        private void FindTextures(List<Tools.Entry> bundle)
        {
            foreach (Tools.Entry e in bundle)
                if (e.type == 0x82)
                {
                    bool foundc = false;
                    bool foundr = false;
                    string path = "";
                    Tools.Field chunks = new Tools.Field();
                    Tools.Field res = new Tools.Field();
                    #region findfields
                    foreach (Tools.Field f in e.fields)
                        if (f.fieldname == "path")
                        {
                            path = (string)f.data;
                        }
                        else if (f.fieldname == "res")
                        {
                            foundr = true;
                            res = f;
                            if (foundc)
                                break;
                        }
                        else if (f.fieldname == "chunks")
                        {
                            foundc = true;
                            chunks = f;
                            if (foundr)
                                break;
                        }
                    #endregion
                    path += "/";
                    if (foundr && foundc) //contains textures at all
                    {
                        #region gettextures
                        List<Tools.Entry> reslist = (List<Tools.Entry>)res.data;
                        foreach (Tools.Entry e2 in reslist)
                            if (e2.type == 0x82)
                            {
                                bool foundt = false;
                                foreach (Tools.Field f2 in e2.fields)
                                    if (f2.fieldname == "resType")
                                    {
                                        if (Tools.GetResType(BitConverter.ToUInt32((byte[])f2.data, 0)) == ".itexture")
                                        {
                                            foundt = true;
                                            break;
                                        }
                                    }
                                if (foundt) //is a texture
                                {
                                    TextureInfo t = new TextureInfo();
                                    foreach (Tools.Field f2 in e2.fields)
                                        switch (f2.fieldname)
                                        {
                                            case "name":
                                                t.path = (string)f2.data;
                                                t.fullpath = path + t.path;
                                                break;
                                            case "sha1":
                                                t.sha1 = (byte[])f2.data;
                                                break;
                                        }
                                    listTex.Add(t);
                                }
                            }
                        #endregion

                        #region getchunks
                        List<Tools.Entry> chunklist = (List<Tools.Entry>)chunks.data;
                        foreach (Tools.Entry e2 in chunklist)
                            if (e2.type == 0x82)
                            {
                                ChunkInfo c = new ChunkInfo();
                                foreach (Tools.Field f2 in e2.fields)
                                    switch (f2.fieldname)
                                    {
                                        case "id":
                                            c.id = (byte[])f2.data;
                                            break;
                                        case "sha1":
                                            c.sha1 = (byte[])f2.data;
                                            break;
                                    }
                                listChunks.Add(c);
                            }
                        #endregion
                    }
                }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            TextureInfo t = listTex[n];
            rtb1.Text = "Type: .itexture\n";
            rtb1.AppendText("Path: " + t.path + "\n");
            rtb1.AppendText("Name: " + t.name + "\n");
            rtb1.AppendText("FullPath: " + t.fullpath + "\n");
            rtb1.AppendText("SHA1: ");
            foreach (byte b in t.sha1)
                rtb1.AppendText(b.ToString("X2"));
            rtb1.AppendText("\n\n");
            if (t.catline != null)
            {
                rtb1.AppendText("CAT entry: ");
                foreach (uint u in t.catline)
                    rtb1.AppendText(u.ToString("X8") + " ");
                rtb1.AppendText("\n\n");
            }
            else
                rtb1.AppendText("CAT entry: not found!\n\n");
            if (t.chunk.catline != null)
            {
                rtb1.AppendText("Chunk SHA1: ");
                foreach (byte b in t.chunk.sha1)
                    rtb1.AppendText(b.ToString("X2"));
                rtb1.AppendText("\nChunk ID: ");
                foreach (byte b in t.chunk.id)
                    rtb1.AppendText(b.ToString("X2"));
                rtb1.AppendText("\nChunk CAT entry: ");
                foreach (uint u in t.chunk.catline)
                    rtb1.AppendText(u.ToString("X8") + " ");
                rtb1.AppendText("\n");
            }
            else
                rtb1.AppendText("Chunk Not Found!\n");
        }

        private void extractAllTexturesWithPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExtractAllTextures(fbd.SelectedPath + "\\", true);
            }
        }

        private void extractAllTexturesWoPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExtractAllTextures(fbd.SelectedPath + "\\", false);
            }
        }

        private void ExtractAllTextures(string target, bool withpath)
        {
            if (cat == null)
                return;
            rtb1.Text = "";
            string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";
            pb1.Value = 0;
            pb1.Maximum = listTex.Count;
            for (int i = 0; i < listTex.Count; i++)
            {
                TextureInfo t = listTex[i];
                if (t.chunk.catline != null && CheckCASExist(basepath, t.chunk.catline[7]))
                {
                    string path = GetDDSTargetFullPath(target, withpath, t);
                    CASFile cas = new CASFile(CASFile.GetCASFileName(basepath, t.chunk.catline[7]));
                    CASFile.CASEntry e = cas.ReadEntry(t.chunk.catline.ToArray());
                    WriteDDSFile(ref t, path, ref e);
                    rtb1.AppendText("Wrote file: " + path + "\n");
                    Application.DoEvents();
                    pb1.Value = i;    
                }
            }
            rtb1.AppendText("Done.\n");
        }

        private static string GetDDSTargetFullPath(string target, bool withpath, TextureInfo t)
        {
            string path = target;
            if (withpath)
                path += t.path.Replace("/", "\\");
            else
                path += Path.GetFileName(t.path);
            if (String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path += ".dds";
            }
            return path;
        }

        private void WriteDDSFile(ref TextureInfo t, string path, ref CASFile.CASEntry e)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream targetStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            using (BinaryWriter targetWriter = new BinaryWriter(targetStream))
            {
                BuildTextureHeader(t, targetWriter);
                targetWriter.Write(e.data);
            }
        }

        private void BuildTextureHeader(TextureInfo textureInfo, BinaryWriter writer)
        {
            // DDS 4-byte header
            writer.Write(0x20534444);
            // DDS size;
            writer.Write(124);
            // DDS Flags
            writer.Write(0x000A1007);
            // DDS width/height
            writer.Write(textureInfo.textureHeight);
            writer.Write(textureInfo.textureWidth);
            // DDS pitch or linear size (size of first mipmap)
            writer.Write(textureInfo.mipSizes[0]);
            // DDS depth
            writer.Write(0);
            // DDS number of mipmaps
            writer.Write(textureInfo.mipSizes.Count);

            // DDS reserved
            for (int i = 0; i < 11; i++)
            {
                writer.Write(0);
            }

            writer.Write(textureInfo.pixelFormat.dwSize);
            writer.Write(textureInfo.pixelFormat.dwFlags);
            writer.Write(textureInfo.pixelFormat.dwFourCC);
            writer.Write(textureInfo.pixelFormat.dwRGBBitCount);
            writer.Write(textureInfo.pixelFormat.dwRBitMask);
            writer.Write(textureInfo.pixelFormat.dwGBitMask);
            writer.Write(textureInfo.pixelFormat.dwBBitMask);
            writer.Write(textureInfo.pixelFormat.dwABitMask);

            // DDS Caps 1-4
            writer.Write(0);
            writer.Write(textureInfo.caps2);
            writer.Write(0);
            writer.Write(0);
            // DDS Reserved2
            writer.Write(0);
        }
    }
}
