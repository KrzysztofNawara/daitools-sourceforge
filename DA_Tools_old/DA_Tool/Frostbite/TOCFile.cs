using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Tool.Frostbite
{
    public class TOCFile
    {
        public string MyPath;
        public int magic;
        public byte[] serial;
        public List<Tools.Entry> lines;
        public byte[] xorKey;
        public uint initfs_magic;

        public TOCFile(string path)
        {
            MyPath = path;
            xorKey = new byte[257];
            // Is there any case where the array won't initialize as filled with zeroes?
            for (int xorByteCount = 0; xorByteCount < xorKey.Length; xorByteCount++)
            {
                xorKey[xorByteCount] = 0;
            }
            ReadFile();
        }

        private void ReadFile()
        {
            using (FileStream fs = new FileStream(MyPath, FileMode.Open, FileAccess.Read))
            {
                magic = Tools.ReadInt(fs);
                if (magic != 0x03CED100 && magic != 0x01CED100)
                    return;
                byte b = (byte)fs.ReadByte();
                while (b == 0 || b == 0x78)
                    b = (byte)fs.ReadByte();
                MemoryStream m = new MemoryStream();
                m.WriteByte(b);
                while ((b = (byte)fs.ReadByte()) != 0x78) m.WriteByte(b);
                serial = m.ToArray();

                if (magic == 0x03CED100)
                {
                    // When magic == 0x03CED100, the xor key is entirely zeroes. We can load the xor key and xor everything, and it'll be the same.
                    //  Therefore, just read the data directly from the file.
                    fs.Seek(0x22C, 0);
                    lines = new List<Tools.Entry>();
                    Tools.ReadEntries(fs, lines);
                }
                else if (magic == 0x01CED100)
                {
                    // When magic == 0x01CED100, assume that you must XOR with both the key and 0x7b.
                    fs.Seek(0x128, SeekOrigin.Begin);
                    fs.Read(xorKey, 0, 257);
                    fs.Seek(3, SeekOrigin.Current); // Move to position 0x22c.

                    MemoryStream unxoredStream = new MemoryStream();
                    BuildUnxoredStream(fs, unxoredStream);

                    unxoredStream.Seek(0, SeekOrigin.Begin);
                    // Haven't the foggiest what this is.
                    initfs_magic = Tools.ReadUInt(unxoredStream);
                    lines = new List<Tools.Entry>();
                    Tools.ReadEntries(unxoredStream, lines);
                }
            }
        }

        private void BuildUnxoredStream(FileStream fs, MemoryStream unxoredStream)
        {
            byte[] nextBytes = new byte[257];
            int lengthRead = 0;
            while ((lengthRead = fs.Read(nextBytes, 0, 257)) > 0)
            {
                for (int byteCount = 0; byteCount < lengthRead; byteCount++)
                {
                    byte unxorByte = (byte)(nextBytes[byteCount] ^ xorKey[byteCount] ^ 0x7b);
                    unxoredStream.WriteByte(unxorByte);
                }
            }
        }
    }
}
