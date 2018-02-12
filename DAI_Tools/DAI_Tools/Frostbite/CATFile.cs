using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAI_Tools.Frostbite
{
    public class CATFile
    {
        public string MyPath;
        public int Level;
        public uint div;
        public List<uint[]> lines;
        public List<List<uint[]>> fastlookup;
        public TOCFile chunks0;
        public List<ChunkType> chunks;
        public struct ChunkType
        {
            public byte[] id;
            public byte[] sha1;
        }

        public CATFile(string path, int level = 256) //use only 2^x as level, like 2,4,8,16,32
        {
            MyPath = path;
            Level = level;
            div = (uint)(0x100000000 / Level);
            ReadFile();
            ReadChunks0File();
        }

        private void ReadChunks0File()
        {
            string c0path = Path.GetDirectoryName(MyPath) + "\\Win32\\chunks0.toc";
            if (File.Exists(c0path))
                chunks0 = new TOCFile(c0path);
            else
                return;
            foreach (Tools.Entry line in chunks0.lines)
                foreach (Tools.Field f in line.fields)
                {
                    switch (f.fieldname)
                    {
                        case "chunks":
                            chunks = new List<ChunkType>();
                            List<Tools.Entry> list = (List<Tools.Entry>)f.data;
                            foreach (Tools.Entry chunk in list)
                            {
                                ChunkType c = new ChunkType();
                                foreach(Tools.Field f2 in chunk.fields)
                                    switch (f2.fieldname)
                                    {
                                        case "id":
                                            c.id = (byte[])f2.data;
                                            break;
                                        case "sha1":
                                            c.sha1 = (byte[])f2.data;
                                            break;
                                    }
                                chunks.Add(c);
                            }
                            break;
                    }
                }
        }

        private void ReadFile()
        {
            FileStream fs = new FileStream(MyPath, FileMode.Open, FileAccess.Read);
            for (int i = 0; i < 4; i++)
                if (Tools.ReadUInt(fs) != 0x6E61794E)
                    return;
            lines = new List<uint[]>();
            fastlookup = new List<List<uint[]>>();
            for (int i = 0; i < Level; i++)
                fastlookup.Add(new List<uint[]>());            
            while (fs.Position < fs.Length)
            {
                uint[] line = new uint[9];
                line[8] = (uint)fs.Position;
                for (int i = 0; i < 8; i++)
                    if (i < 5)
                        line[i] = Tools.ReadLEUInt(fs);
                    else
                        line[i] = Tools.ReadUInt(fs);
                
                int dic = (int)(line[0] / div);
                List<uint[]> t = fastlookup[dic];
                t.Add(line);
                fastlookup[dic] = t;
                lines.Add(line);
            }
            fs.Close();
        }

        public List<uint> FindBySHA1(byte[] sha1)
        {
            MemoryStream m = new MemoryStream(sha1);
            List<uint> res = new List<uint>();
            uint[] sha1ints = new uint[5];
            for (int i = 0; i < 5; i++)
                sha1ints[i] = Tools.ReadLEUInt(m);
            int dic = (int)(sha1ints[0] / div);
            for (int i = 0; i < fastlookup[dic].Count; i++) 
                if (fastlookup[dic][i][0] == sha1ints[0] &&
                   fastlookup[dic][i][1] == sha1ints[1] &&
                   fastlookup[dic][i][2] == sha1ints[2] &&
                   fastlookup[dic][i][3] == sha1ints[3] &&
                   fastlookup[dic][i][4] == sha1ints[4])
                {
                    res.AddRange(fastlookup[dic][i]);
                    break;
                }
            return res;
        }
    }
}
