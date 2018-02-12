using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace DAI_Tools.Frostbite
{
    public static partial class Tools
    {
        public class BinaryReader7Bit : BinaryReader
        {
            public BinaryReader7Bit(Stream stream) : base(stream) { }
            public new int Read7BitEncodedInt()
            {
                return base.Read7BitEncodedInt();
            }
        }

        public class BinaryWriter7Bit : BinaryWriter
        {
            public BinaryWriter7Bit(Stream stream) : base(stream) { }
            public new void Write7BitEncodedInt(int i)
            {
                base.Write7BitEncodedInt(i);
            }
        }

        public class Entry
        {
            public int type;
            public List<Field> fields;
            public string type87name;
        }

        public class Field
        {
            public byte type;
            public string fieldname;
            public List<Field> fields;
            public object data;
        }        

        public static void DeleteFileIfExist(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        public static void RunShell(string file, string command)
        {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = file;
            startInfo.Arguments = command;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = Path.GetDirectoryName(file) + "\\";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        public static byte[] GetDataBySHA1(string SHA1, CATFile cat)
        {
            return GetDataBySHA1(Tools.StringToByteArray(SHA1), cat);
        }

        public static byte[] GetDataBySHA1(byte[] SHA1, CATFile cat)
        {
            string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";
            List<uint> line = cat.FindBySHA1(SHA1);
            if (line.Count == 9)
            {
                uint casnr = line[7];
                string casn = "cas_" + casnr.ToString("d2") + ".cas";
                if (File.Exists(basepath + casn))
                {
                    CASFile cas = new CASFile(basepath + casn);
                    CASFile.CASEntry ent = cas.ReadEntry(line.ToArray());
                    return ent.data;
                }
            }
            return new byte[0];
        }

        public static void ReadEntries(Stream s, List<Entry> list)
        {
            while (s.Position < s.Length)
            {
                byte type1 = (byte)s.ReadByte();
                ulong size;
                Entry e = new Entry();
                e.type = type1;
                switch (type1)
                {
                    case 0x82:
                        size = ReadLEB128(s);
                        long pos = s.Position;
                        e.fields = new List<Field>();
                        while (s.Position - pos < (long)size)
                        {
                            Field currentField = AddField(s);
                            if (currentField.fieldname != null && currentField.fieldname.StartsWith("$"))
                            {
                                currentField = AddSubfields(currentField);
                            }
                            e.fields.Add(currentField);
                        }
                        list.Add(e);
                        break;
                    case 0x87:
                        size = ReadLEB128(s);
                        string res = "";
                        for (int i = 0; i < (int)size; i++)
                        {
                            byte b = (byte)s.ReadByte();
                            if (b != 0)
                                res += (char)b;
                        }
                        e.type87name = res;
                        list.Add(e);
                        break;
                    default: return;
                }
            }
        }

        private static Field AddSubfields(Field currentField)
        {
            currentField.fields = new List<Field>();
            MemoryStream entrydatastream = new MemoryStream();
            byte[] entrydata = (byte[])currentField.data;
            entrydatastream.Write(entrydata, 0, entrydata.Length);
            entrydatastream.Seek(0, SeekOrigin.Begin);
            while (entrydatastream.Position + 1 < entrydatastream.Length)
            {
                currentField.fields.Add(AddField(entrydatastream));
            }
            return currentField;
        }

        public static Field AddField(Stream s)
        {
            Field result = new Field();
            result.fields = null;
            result.type = 0;
            byte type = (byte)s.ReadByte();
            if (type == 0)
                return result;
            string fieldname = ReadNullString(s);
            result.type = type;
            result.fieldname = fieldname;
            ParseBinaryJSONEntry(s, result);
            return result;
        }

        private static void ParseBinaryJSONEntry(Stream s, Field result)
        {
            ulong size;
            ulong count;
            long pos;
            byte[] buff;
            switch (result.type)
            {
                case 0x01:
                    size = ReadLEB128(s);
                    result.data = new List<Entry>();
                    pos = s.Position;
                    buff = new byte[size - 1];
                    s.Read(buff, 0, (int)size - 1);
                    ReadEntries(new MemoryStream(buff), (List<Entry>)result.data);
                    s.ReadByte();
                    break;
                case 0x07:
                    count = ReadLEB128(s);
                    string res = "";
                    for (int i = 0; i < (int)count; i++)
                    {
                        byte b = (byte)s.ReadByte();
                        if (b != 0)
                            res += (char)b;
                    }
                    result.data = res;
                    break;
                case 0x06:
                    result.data = (s.ReadByte() == 1);
                    break;
                case 0x08:
                    buff = new byte[4];
                    s.Read(buff, 0, 4);
                    result.data = buff;
                    break;
                case 0x09:
                    buff = new byte[8];
                    s.Read(buff, 0, 8);
                    result.data = buff;
                    break;
                case 0xf:
                    buff = new byte[0x10];
                    s.Read(buff, 0, 0x10);
                    result.data = buff;
                    break;
                case 0x10:
                    buff = new byte[0x14];
                    s.Read(buff, 0, 0x14);
                    result.data = buff;
                    break;
                case 0x02:
                case 0x13:
                    size = ReadLEB128(s);
                    buff = new byte[size];
                    s.Read(buff, 0, (int)size);
                    result.data = buff;
                    break;
            }
        }

        public static TreeNode MakeEntry(TreeNode t, Tools.Entry e)
        {
            if (e.type == 0x87)
            {
                t.Text = "87 (" + e.type87name + ")";
                return t;
            }
            if (e.type == 0x82)
            {
                foreach (Tools.Field f in e.fields)
                    if (f.type != 0)
                        t.Nodes.Add(MakeField(new TreeNode(f.fieldname), f));
            }
            return t;
        }

        public static TreeNode MakeField(TreeNode t, Tools.Field f)
        {
            byte[] buff;
            if (f.fields != null)
            {
                foreach (Tools.Field subfield in f.fields)
                {
                    t.Nodes.Add(MakeField(new TreeNode(subfield.fieldname), subfield));
                }
            }
            else
            {
                switch (f.type)
                {
                    case 1:
                        List<Tools.Entry> list = (List<Tools.Entry>)f.data;
                        foreach (Tools.Entry e in list)
                            t.Nodes.Add(MakeEntry(new TreeNode(e.type.ToString("X")) { Tag = f }, e));
                        break;
                    case 6:
                        bool b = (bool)f.data;
                        t.Nodes.Add(new TreeNode(b.ToString()) { Tag = f });
                        break;
                    case 7:
                        string s = (string)f.data;
                        t.Nodes.Add(new TreeNode(s) { Tag = f });
                        break;
                    case 8:
                        buff = (byte[])f.data;
                        buff.Reverse();
                        if (t.Text == "resType")
                        {
                            string nodeTitle = string.Format("0x{0} {1}", BitConverter.ToUInt32(buff, 0).ToString("X"), GetResType(BitConverter.ToUInt32(buff, 0)));
                            t.Nodes.Add(new TreeNode(nodeTitle) { Tag = f });
                        }
                        else
                            t.Nodes.Add(new TreeNode("0x" + BitConverter.ToUInt32(buff, 0).ToString("X")) { Tag = f });
                        break;
                    case 9:
                        buff = (byte[])f.data;
                        buff.Reverse();
                        t.Nodes.Add(new TreeNode("0x" + BitConverter.ToUInt64(buff, 0).ToString("X")) { Tag = f });
                        break;
                    case 0xf:
                        buff = (byte[])f.data;
                        buff.Reverse();
                        t.Nodes.Add(new TreeNode("0x" + BitConverter.ToUInt64(buff, 8).ToString("X")) { Tag = f });
                        t.Nodes.Add(new TreeNode("0x" + BitConverter.ToUInt64(buff, 0).ToString("X")) { Tag = f });
                        break;
                    case 2:
                        buff = (byte[])f.data;
                        t.Nodes.Add(new TreeNode(System.Text.Encoding.UTF8.GetString(buff)) { Tag = f });
                        break;
                    case 0x10:
                    case 0x13:
                        // 'payload' fields are only found in the InitFS_Win32 file. Since they can be quite large, do not show them within a treeview.
                        if (f.fieldname != "payload")
                        {
                            buff = (byte[])f.data;
                            StringBuilder res = new StringBuilder();
                            foreach (byte bb in buff)
                                res.Append(bb.ToString("X2"));
                            t.Nodes.Add(new TreeNode(res.ToString()) { Tag = f });
                        }
                        else
                        {
                            t.Tag = f;
                        }
                        break;
                    default:
                        break;
                }
            }
            return t;
        }

        public static void WriteEntry(Stream s, Tools.Entry e)
        {
            Tools.BinaryWriter7Bit w = new Tools.BinaryWriter7Bit(s);
            switch (e.type)
            {
                case 0x82:
                    s.WriteByte(0x82);
                    MemoryStream m = new MemoryStream();
                    foreach (Tools.Field f in e.fields)
                        WriteField(m, f);
                    w.Write7BitEncodedInt((int)m.Length);
                    s.Write(m.ToArray(), 0, (int)m.Length);
                    break;

                case 0x87:
                    w.Write7BitEncodedInt(e.type87name.Length + 1);
                    Tools.WriteNullString(s, e.type87name);
                    break;
            }
        }

        public static void WriteField(Stream s, Tools.Field f)
        {
            if (f.type == 0)
            {
                s.WriteByte(0);
                return;
            }
            s.WriteByte(f.type);
            Tools.WriteNullString(s, f.fieldname);
            Tools.BinaryWriter7Bit w = new Tools.BinaryWriter7Bit(s);
            switch (f.type)
            {
                case 0x01:
                    List<Tools.Entry> list = (List<Tools.Entry>)f.data;
                    MemoryStream m = new MemoryStream();
                    foreach (Tools.Entry e in list)
                        WriteEntry(m, e);
                    m.WriteByte(0);
                    w.Write7BitEncodedInt((int)m.Length);
                    s.Write(m.ToArray(), 0, (int)m.Length);                    
                    break;
                case 0x07:
                    w.Write7BitEncodedInt((int)((string)f.data).Length + 1);
                    Tools.WriteNullString(s, (string)f.data);
                    break;
                case 0x06:
                    s.WriteByte(((bool)f.data) ? (byte)1 : (byte)0);
                    break;
                case 0x08:
                case 0x09:
                case 0xf:
                case 0x10:
                    s.Write((byte[])f.data, 0, (int)((byte[])f.data).Length);
                    break;
                case 0x02:
                case 0x13:
                    w.Write7BitEncodedInt((int)((byte[])f.data).Length);
                    s.Write((byte[])f.data, 0, (int)((byte[])f.data).Length);
                    break;  
            }
            if (f.fields != null)
                foreach (Field subfield in f.fields)
                    WriteField(s, subfield);
        }

        public static byte[] ExtractTalktable(MemoryStream TTBuffer)
        {
            Talktable TTFile = new Talktable();
            TTFile.Read(TTBuffer);

            MemoryStream OutputStream = new MemoryStream();
            StreamWriter Writer = new StreamWriter(OutputStream);

            for (int i = 0; i < TTFile.Strings.Count; i++)
                Writer.WriteLine(TTFile.Strings[i].ID.ToString("X8") + ": " + TTFile.Strings[i].Value);

            Writer.Close();
            return OutputStream.ToArray();
        }

        public static void ExtractEbxGuidAndType(MemoryStream EbxBuffer, out string type, out string guid)
        {
            DAIEbx EbxFile = new DAIEbx();
            EbxFile.Serialize(EbxBuffer);

            guid = "";
            for (int i = 0; i < EbxFile.FileGuid.Length; i++)
                guid += EbxFile.FileGuid[i].ToString("X2");
            type = EbxFile.RootInstance.Descriptor.FieldName;
        }

        public static byte[] ExtractEbx(MemoryStream EbxBuffer)
        {
            MemoryStream OutputStream = new MemoryStream();

            DAIEbx EbxFile = new DAIEbx();
            EbxFile.Serialize(EbxBuffer);

            StreamWriter Writer = new StreamWriter(OutputStream);
            Writer.Write(EbxFile.ToXml());
            Writer.Close();

            return OutputStream.ToArray();
        }

        public static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
                return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }

        public static void WriteInt(Stream s, int i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 4);
        }

        public static void WriteLEInt(Stream s, int i)
        {
            List<byte> t = new List<byte>(BitConverter.GetBytes(i));
            t.Reverse();
            s.Write(t.ToArray(), 0, 4);
        }

        public static int ReadInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToInt32(buff, 0);
        }

        public static uint ReadUInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToUInt32(buff, 0);
        }

        public static short ReadShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToInt16(buff, 0);
        }

        public static ushort ReadUShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToUInt16(buff, 0);
        }

        public static long ReadLong(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToInt64(buff, 0);
        }

        public static ulong ReadULong(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToUInt64(buff, 0);
        }

        public static float ReadFloat(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToSingle(buff, 0);
        }

        public static int ReadLEInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToInt32(buff, 0);
        }

        public static uint ReadLEUInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToUInt32(buff, 0);
        }

        public static short ReadLEShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToInt16(buff, 0);
        }

        public static ushort ReadLEUShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToUInt16(buff, 0);
        }

        public static byte[] ReadFull(Stream s, uint size)
        {
            byte[] buff = new byte[size];
            int totalread = 0;
            while ((totalread += s.Read(buff, totalread, (int)(size - totalread))) < size) ;
            return buff;
        }

        public static string ReadNullString(Stream s)
        {
            string res = "";
            byte b;
            while ((b = (byte)s.ReadByte()) > 0) res += (char)b;
            return res;
        }

        public static void WriteNullString(Stream s, string t)
        {
            foreach (char c in t)
                s.WriteByte((byte)c);
            s.WriteByte(0);
        }

        public static ulong ReadLEB128(Stream s)
        {
            ulong result = 0;
            byte shift = 0;
            while (true)
            {
                byte b = (byte)s.ReadByte();
                result |= (ulong)((b & 0x7f) << shift);
                if ((b >> 7) == 0)
                    return result;
                shift += 7;
            }
        }

        public static void WriteLEB128(Stream s, int value)
        {
            int temp = value;
            while (temp != 0)
            {
                int val = (temp & 0x7f);
                temp >>= 7;

                if (temp > 0)
                    val |= 0x80;

                s.WriteByte((byte)val);
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string ByteArrayToString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        public static int DecompressLZ77(byte[] input, byte[] output, out int decompressedLength)
        {
            int inputPos = 0, outputPos = 0;
            try
            {
                while (true)
                {

                    bool isLookback = true;
                    bool skipParseCopyLength = false;
                    int lookbackLength = 1;
                    int copyLength = 1;
                    byte copyLengthMask = 0;

                    byte code = input[inputPos++];
                    if (code < 0x10)
                    {
                        isLookback = false;
                        copyLength += 2;
                        copyLengthMask = 0x0f;
                    }
                    else if (code < 0x20)
                    {
                        copyLength += 1;
                        copyLengthMask = 0x07;
                        lookbackLength |= (code & 0x08) << 11;
                        lookbackLength += 0x3fff;
                    }
                    else if (code < 0x40)
                    {
                        copyLength += 1;
                        copyLengthMask = 0x1f;
                    }
                    else
                    {
                        skipParseCopyLength = true;
                        copyLength += code >> 5;
                        lookbackLength += (code >> 2) & 0x07;
                        lookbackLength += input[inputPos++] * 8;
                    }

                    if (!isLookback || !skipParseCopyLength)
                    {
                        if ((code & copyLengthMask) == 0)
                        {
                            byte nextCode;
                            for (nextCode = input[inputPos++]; nextCode == 0; nextCode = input[inputPos++])
                            {
                                copyLength += 0xff;
                            }
                            copyLength += nextCode + copyLengthMask;
                        }
                        else
                        {
                            copyLength += code & copyLengthMask;
                        }

                        if (isLookback)
                        {
                            int lookbackCode = input[inputPos++];
                            lookbackCode |= input[inputPos++] << 8;
                            if (code < 0x20 && (lookbackCode >> 2) == 0) break;
                            lookbackLength += lookbackCode >> 2;
                            code = (byte)lookbackCode;
                        }
                    }

                    if (isLookback)
                    {
                        int lookbackPos = outputPos - lookbackLength;
                        for (int i = 0; i < copyLength; ++i)
                        {
                            output[outputPos++] = output[lookbackPos++];
                        }
                        copyLength = code & 0x03;
                    }

                    for (int i = 0; i < copyLength; ++i)
                    {
                        output[outputPos++] = input[inputPos++];
                    }
                }
            }
            catch
            { }

            decompressedLength = outputPos;
            if (inputPos == input.Length) return 0;
            else return inputPos < input.Length ? -8 : -4;
        }

        public static byte[] DecompressZlib(byte[] input, int size)
        {
            byte[] result = new byte[size];
            InflaterInputStream zipStream = new InflaterInputStream(new MemoryStream(input));
            zipStream.Read(result, 0, size);
            zipStream.Flush();
            return result;
        }

        public static byte[] CompressZlib(byte[] input)
        {
            MemoryStream m = new MemoryStream();
            DeflaterOutputStream zipStream = new DeflaterOutputStream(m, new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(8));
            zipStream.Write(input, 0, input.Length);
            zipStream.Finish();
            return m.ToArray();
        }

        public static string DecompileLUAC(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            if (m.Length > 0x18)
            {
                uint magic = Tools.ReadUInt(m);
                if (magic == 0xe1850009)
                {
                    m.Seek(0x18, 0);
                    try
                    {
                        string name = Tools.ReadNullString(m);
                        string clas = Tools.ReadNullString(m);
                        int len = (int)(m.Length - m.Position);
                        if (len > 0)
                        {
                            byte[] script = new byte[len];
                            m.Read(script, 0, len);
                            string basepath = Application.StartupPath + "\\luacdec\\";
                            File.WriteAllBytes(basepath + "temp.luac", script);
                            if (File.Exists(basepath + "temp.lua"))
                                File.Delete(basepath + "temp.lua");
                            Process p = new Process();
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.FileName = basepath + "dec.bat";
                            p.StartInfo.WorkingDirectory = basepath;
                            p.Start();
                            string output = p.StandardOutput.ReadToEnd();
                            p.WaitForExit();
                            if (File.Exists(basepath + "temp.lua"))
                                return "Name: " + name + "\nClass: " + clas + "\n\nDecompilation:\n\n" + File.ReadAllText(basepath + "temp.lua");
                            else
                                return "";
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return "";
        }

        public static string GetResType(uint type)
        {
            if (ResTypes.ContainsKey(type))
                return ResTypes[type];
            else
                return "";
        }

        public static Dictionary<uint, string> ResTypes = new Dictionary<uint, string>()
#region data
        {
            {0x5c4954a6, ".itexture"},
            {0x2d47a5ff, ".gfx"},
            {0x22fe8ac8, ""},
            {0x6bb6d7d2, ".streamingstub"},
            {0x1ca38e06, ""},
            {0x15e1f32e, ""},
            {0x4864737b, ".hkdestruction"},
            {0x91043f65, ".hknondestruction"},
            {0x51a3c853, ".ant"},
            {0xd070eed1, ".animtrackdata"},
            {0x319d8cd0, ".ragdoll"},
            {0x49b156d4, ".mesh"},
            {0x30b4a553, ".occludermesh"},
            {0x5bdfdefe, ".lightingsystem"},
            {0x70c5cb3e, ".enlighten"},
            {0xe156af73, ".probeset"},
            {0x7aefc446, ".staticenlighten"},
            {0x59ceeb57, ".shaderdatabase"},
            {0x36f3f2c0, ".shaderdb"},
            {0x10f0e5a1, ".shaderprogramdb"},
            {0xc6dbee07, ".mohwspecific"},
            {0xafecb022, ".luac"},
            {0x59c79990, ".facefx"},
            {0x1091c8c5, ".morphtargets"},
            {0xe36f0d59, ".clothasset"},
            {0x24a019cc, ".material"},
            {0x5e862e05, ".talktable"},
            {0x957c32b1, ".alttexture"},
            {0x76742dc8, ".delayloadbundles"},
            {0xa23e75db, ".layercombinations"},
            {0xc6cd3286, ".static"},
            {0xeb228507, ".headmoprh"},
            {0xefc70728, ".zs"}
        };
#endregion
    }
}
