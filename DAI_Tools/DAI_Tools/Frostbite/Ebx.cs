using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DAI_Tools.Frostbite
{
    public class DAIExternalGuid
    {
        public byte[] FileGuid;
        public byte[] InstanceGuid;

        public DAIExternalGuid()
        {
            FileGuid = new byte[16];
            InstanceGuid = new byte[16];
        }

        public override string ToString()
        {
            String RetVal = "";
            for (int i = 0; i < 16; i++)
                RetVal += FileGuid[i].ToString("X2");
            RetVal += "/";
            for (int i = 0; i < 16; i++)
                RetVal += InstanceGuid[i].ToString("X2");

            return RetVal;
        }

        public string FileGuidString()
        {
            string RetVal = "";
            for (int i = 0; i < 16; i++)
                RetVal += FileGuid[i].ToString("X2");

            return RetVal;
        }
    }

    public class DAIHeader
    {
        /* Start header */
        public int StringOffset;
        public int StringLengthToEOF;
        public int ExternalGuidCount;
        public int InstanceRepeaterCount;
        public int GuidRepeaterCount;
        public int Unknown01;
        public int ComplexEntryCount;
        public int FieldCount;
        public int NameLength;
        public int StringLength;
        public int ArrayRepeaterCount;
        public int PayloadLength;

        public int ArraySectionStart;
        /* End header */

        public void Serialize(Stream s)
        {
            StringOffset = Tools.ReadInt(s);
            StringLengthToEOF = Tools.ReadInt(s);
            ExternalGuidCount = Tools.ReadInt(s);

            InstanceRepeaterCount = Tools.ReadShort(s);
            GuidRepeaterCount = Tools.ReadShort(s);
            Unknown01 = Tools.ReadShort(s);
            ComplexEntryCount = Tools.ReadShort(s);
            FieldCount = Tools.ReadShort(s);
            NameLength = Tools.ReadShort(s);

            StringLength = Tools.ReadInt(s);
            ArrayRepeaterCount = Tools.ReadInt(s);
            PayloadLength = Tools.ReadInt(s);

            ArraySectionStart = StringOffset + StringLength + PayloadLength;
        }
    }

    public class DAIFieldDescriptor
    {
        public String FieldName;
        public int FieldType;
        public int ComplexReference;
        public int PayloadOffset;
        public int SecondaryOffset;

        public void Serialize(Stream s, DAIEbx EbxFile)
        {
            FieldName = EbxFile.KeywordDict[Tools.ReadInt(s)];
            FieldType = Tools.ReadShort(s);
            ComplexReference = Tools.ReadShort(s);
            PayloadOffset = Tools.ReadInt(s);
            SecondaryOffset = Tools.ReadInt(s);

            if (FieldName == "$")
                PayloadOffset -= 8;
        }
    }

    public class DAIComplexDescriptor
    {
        public String FieldName;
        public int FieldStartIndex;
        public int FieldCount;
        public int Alignment;
        public int FieldType;
        public int FieldSize;
        public int SecondarySize;

        public void Serialize(Stream s, DAIEbx EbxFile)
        {
            FieldName = EbxFile.KeywordDict[Tools.ReadInt(s)];
            FieldStartIndex = Tools.ReadInt(s);
            FieldCount = s.ReadByte();
            Alignment = s.ReadByte();
            FieldType = Tools.ReadShort(s);
            FieldSize = Tools.ReadShort(s);
            SecondarySize = Tools.ReadShort(s);
        }
    }

    public enum DAIFieldType
    {
        DAI_Complex,
        DAI_Array,
        DAI_String,
        DAI_Enum,
        DAI_Int,
        DAI_UInt,
        DAI_Float,
        DAI_Short,
        DAI_UShort,
        DAI_Byte,
        DAI_UByte,
        DAI_Long,
        DAI_LongLong,
        DAI_Guid,
        DAI_Double,
        DAI_Bool
    }

    public class DAIGuid
    {
        public bool external = false;
        public String fileGuid = "";
        public String instanceGuid = "";
    }

    public class DAIField
    {
        public DAIFieldDescriptor Descriptor;
        public Int64 Offset;

        public DAIFieldType ValueType;
        public byte[] Value;
        public DAIComplex ComplexValue;
        public List<DAIComplex> ComplexArrayValue;

        public DAIComplex GetComplexValue() { return ComplexValue; }
        public DAIComplex GetArrayValue() { return ComplexValue; }
        public string GetStringValue() { return System.Text.Encoding.ASCII.GetString(Value); }
        public string GetEnumValue() { return GetStringValue(); }
        public int GetIntValue() { return BitConverter.ToInt32(Value, 0); }
        public uint GetUIntValue() { return BitConverter.ToUInt32(Value, 0); }
        public float GetFloatValue() { return BitConverter.ToSingle(Value, 0); }
        public short GetShortValue() { return BitConverter.ToInt16(Value, 0); }
        public ushort GetUShortValue() { return BitConverter.ToUInt16(Value, 0); }
        public byte GetByteValue() { return Value[0]; }
        public bool GetBoolValue() { return Value[0] == 0x01; }
        public long GetLongValue() { return BitConverter.ToInt64(Value, 0); }
        public byte[] GetLongLongValue() { return Value; }

        public void ToXml(DAIEbx EbxFile, ref StringBuilder sb)
        {
            if (Descriptor.FieldName == "$")
            {
                DAIEbx.TabCount--;
                ComplexValue.ToXml(EbxFile, ref sb);
                DAIEbx.TabCount++;
                return;
            }

            sb.Append(DAIEbx.Tabs() + "<" + Descriptor.FieldName + ">");

            switch (ValueType)
            {
                case DAIFieldType.DAI_Complex:
                case DAIFieldType.DAI_Array:
                    sb.Append("\n");
                    ComplexValue.ToXml(EbxFile, ref sb);
                    sb.Append(DAIEbx.Tabs() + "</" + Descriptor.FieldName + ">\n");
                    return;

                case DAIFieldType.DAI_String:
                    sb.Append(GetStringValue());
                    break;

                case DAIFieldType.DAI_Int:
                    sb.Append(GetIntValue().ToString("X8"));
                    break;

                case DAIFieldType.DAI_UInt:
                    sb.Append(GetUIntValue().ToString("X8"));
                    break;

                case DAIFieldType.DAI_Float:
                    sb.Append(GetFloatValue().ToString("F3"));
                    break;

                case DAIFieldType.DAI_Short:
                    sb.Append(GetShortValue().ToString("X4"));
                    break;

                case DAIFieldType.DAI_UShort:
                    sb.Append(GetUShortValue().ToString("X4"));
                    break;

                case DAIFieldType.DAI_Byte:
                    sb.Append(GetByteValue().ToString("X2"));
                    break;

                case DAIFieldType.DAI_UByte:
                    sb.Append(GetByteValue().ToString("X2"));
                    break;

                case DAIFieldType.DAI_Long:
                    sb.Append(GetLongValue().ToString("X16"));
                    break;

                case DAIFieldType.DAI_LongLong:
                    for (int i = 0; i < Value.Length; i++)
                        sb.Append(Value[i].ToString("X2"));
                    break;

                case DAIFieldType.DAI_Bool:
                    sb.Append(GetBoolValue().ToString());
                    break;

                case DAIFieldType.DAI_Enum:
                    sb.Append(GetEnumValue());
                    break;

                case DAIFieldType.DAI_Guid:
                    {
                        uint UIntValue = GetUIntValue();
                        if ((UIntValue >> 31) == 1)
                        {
                            /* External Guid */
                            DAIExternalGuid Guid = EbxFile.ExternalGuids.ElementAt((int)(UIntValue & 0x7fffffff));
                            System.Data.SQLite.SQLiteConnection con = Database.GetConnection();
                            con.Open();
                            System.Data.SQLite.SQLiteDataReader reader = new System.Data.SQLite.SQLiteCommand("SELECT name,type FROM ebx WHERE guid = '" + Guid.FileGuidString() + "'", con).ExecuteReader();
                            reader.Read();
                            sb.Append("[" + reader.GetString(1) + "] " + reader.GetString(0));
                        }
                        else if (UIntValue == 0)
                        {
                            /* NULL Guid */
                            sb.Append("[null]");
                        }
                        else
                        {
                            /* Internal Guid */
                            byte[] Guid = EbxFile.InternalGuids[(int)(UIntValue - 1)];
                            sb.Append("[" + EbxFile.Instances[Guid].Descriptor.FieldName + "] ");
                            for (int i = 0; i < Guid.Length; i++)
                                sb.Append(Guid[i].ToString("X2"));

                        }
                    }
                    break;
            }

            sb.Append("</" + Descriptor.FieldName + ">\n");
            return;
        }
    }

    public class DAIComplex
    {
        public DAIComplexDescriptor Descriptor;
        public Int64 Offset;
        public List<DAIField> Fields;

        public string GetName() { return Descriptor.FieldName; }

        public DAIField GetFieldByName(String FieldName)
        {
            foreach (DAIField CurField in Fields)
            {
                if (CurField.Descriptor.FieldName == FieldName)
                {
                    return CurField;
                }
            }

            return null;
        }

        public void ToXml(DAIEbx EbxFile, ref StringBuilder sb, bool bPrintDescriptor = true)
        {
            if (Descriptor.FieldName != "array")
            {
                if (bPrintDescriptor)
                {
                    DAIEbx.TabCount++;
                    sb.Append(DAIEbx.Tabs() + "<" + Descriptor.FieldName + ">\n");
                    DAIEbx.TabCount++;
                }
            }
            else
            {
                DAIEbx.TabCount++;
            }

            foreach (DAIField CurField in Fields)
            {
                CurField.ToXml(EbxFile, ref sb);
            }

            if (Descriptor.FieldName != "array")
            {
                if (bPrintDescriptor)
                {
                    DAIEbx.TabCount--;
                    sb.Append(DAIEbx.Tabs() + "</" + Descriptor.FieldName + ">\n");
                    DAIEbx.TabCount--;
                }
            }
            else
            {
                DAIEbx.TabCount--;
            }
        }
    }

    public class DAIInstanceRepeater
    {
        public int ComplexDescriptorIndex;
        public int Count;

        public void Serialize(Stream s, DAIEbx EbxFile)
        {
            ComplexDescriptorIndex = Tools.ReadShort(s);
            Count = Tools.ReadShort(s);
        }
    }

    public class DAIArrayRepeater
    {
        public int Offset;
        public int Count;
        public int ComplexDescriptorIndex;

        public void Serialize(Stream s, DAIEbx EbxFile)
        {
            Offset = Tools.ReadInt(s);
            Count = Tools.ReadInt(s);
            ComplexDescriptorIndex = Tools.ReadInt(s);
        }
    }

    public class DAIEbx
    {
        public DAIHeader Header;

        public byte[] FileGuid;
        public List<DAIExternalGuid> ExternalGuids;
        public List<byte[]> InternalGuids;

        public Dictionary<int, String> KeywordDict;
        public List<DAIFieldDescriptor> FieldDescriptors;
        public List<DAIComplexDescriptor> ComplexDescriptors;
        public List<DAIInstanceRepeater> InstanceRepeaters;
        public List<DAIArrayRepeater> ArrayRepeaters;

        public Dictionary<byte[], DAIComplex> Instances;
        public DAIComplex RootInstance;

        public static int TabCount = 0;

        public static String Tabs()
        {
            StringBuilder TabB = new StringBuilder();
            for (int i = 0; i < DAIEbx.TabCount; i++)
            {
                TabB.Append("  ");
            }

            return TabB.ToString();
        }

        public string ToXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<EbxFile Guid=\"");
            for (int i = 0; i < 16; i++)
                sb.Append(FileGuid[i].ToString("X2"));
            sb.Append("\">\n");
            DAIEbx.TabCount++;

            for (int i = 0; i < Instances.Count; i++)
            {
                byte[] Guid = Instances.Keys.ElementAt(i);
                DAIComplex ComplexValue = Instances.Values.ElementAt(i);

                String GuidString = "";
                for (int j = 0; j < Guid.Length; j++)
                    GuidString += Guid[j].ToString("X2");

                sb.Append(DAIEbx.Tabs() + "<" + ComplexValue.Descriptor.FieldName + " Guid=\"");
                for (int j = 0; j < 16; j++)
                    sb.Append(Guid[j].ToString("X2"));
                sb.Append("\">\n");
                DAIEbx.TabCount++;

                ComplexValue.ToXml(this, ref sb, false);

                DAIEbx.TabCount--;
                sb.Append(DAIEbx.Tabs() + "</" + ComplexValue.Descriptor.FieldName + ">\n");
            }

            DAIEbx.TabCount--;
            sb.Append("</EbxFile>\n");

            return sb.ToString();
        }

        public int Hasher(String StrToHash)
        {
            int Hash = 5381;
            for (int i = 0; i < StrToHash.Length; i++)
            {
                byte B = (byte)StrToHash[i];
                Hash = (Hash * 33) ^ B;
            }

            return Hash;
        }

        public DAIComplex ReadComplex(Stream s, int ComplexIndex, bool IsInstance = false)
        {
            DAIComplex Complex = new DAIComplex();
            Complex.Descriptor = ComplexDescriptors[ComplexIndex];
            Complex.Offset = s.Position;
            Complex.Fields = new List<DAIField>();

            int ObfuscationShift = (IsInstance && Complex.Descriptor.Alignment == 4) ? 8 : 0;

            for (int i = Complex.Descriptor.FieldStartIndex; i < Complex.Descriptor.FieldStartIndex + Complex.Descriptor.FieldCount; i++)
            {
                s.Seek(Complex.Offset + FieldDescriptors[i].PayloadOffset - ObfuscationShift, SeekOrigin.Begin);
                Complex.Fields.Add(ReadField(s, i));
            }

            s.Seek(Complex.Offset + Complex.Descriptor.FieldSize - ObfuscationShift, SeekOrigin.Begin);
            return Complex;
        }

        public DAIField ReadField(Stream s, int FieldIndex)
        {
            DAIField Field = new DAIField();
            Field.Descriptor = FieldDescriptors[FieldIndex];
            Field.Offset = s.Position;

            int FieldType = (Field.Descriptor.FieldType & 0xFFFF);
            if (FieldType == 0x29 || FieldType == 0xd029 || FieldType == 0x00 || FieldType == 0x8029)
            {
                Field.ComplexValue = ReadComplex(s, Field.Descriptor.ComplexReference);
                Field.ValueType = DAIFieldType.DAI_Complex;
            }
            else if (FieldType == 0x407d || FieldType == 0x409d)
            {
                String StrValue = "";
                Int64 PrevPos = s.Position;
                int StringOffset = Tools.ReadInt(s);

                if (StringOffset == -1)
                {
                    StrValue = "";
                }
                else
                {
                    s.Seek(Header.StringOffset + StringOffset, SeekOrigin.Begin);
                    StrValue = Tools.ReadNullString(s);

                    s.Seek(PrevPos + 4, SeekOrigin.Begin);
                }

                Field.Value = new byte[StrValue.Length];
                for (int i = 0; i < StrValue.Length; i++) { Field.Value[i] = (byte)StrValue[i]; }
                Field.ValueType = DAIFieldType.DAI_String;
            }
            else if (FieldType == 0x35)
            {
                uint UIntValue = Tools.ReadUInt(s);
                Field.ValueType = DAIFieldType.DAI_Guid;
                Field.Value = BitConverter.GetBytes(UIntValue);
            }
            else if (FieldType == 0xc10d)
            {
                uint UIntValue = Tools.ReadUInt(s);
                Field.Value = BitConverter.GetBytes(UIntValue);
                Field.ValueType = DAIFieldType.DAI_UInt;
            }
            else if (FieldType == 0xc0fd)
            {
                int IntValue = Tools.ReadInt(s);
                Field.Value = BitConverter.GetBytes(IntValue);
                Field.ValueType = DAIFieldType.DAI_Int;
            }
            else if (FieldType == 0x417d)
            {
                Int64 LongValue = Tools.ReadLong(s);
                Field.Value = BitConverter.GetBytes(LongValue);
                Field.ValueType = DAIFieldType.DAI_Long;
            }
            else if (FieldType == 0xc13d)
            {
                float FloatValue = Tools.ReadFloat(s);
                Field.Value = BitConverter.GetBytes(FloatValue);
                Field.ValueType = DAIFieldType.DAI_Float;
            }
            else if (FieldType == 0xc0ad)
            {
                byte ByteValue = (byte)s.ReadByte();
                Field.Value = new byte[] { ByteValue };
                Field.ValueType = DAIFieldType.DAI_Bool;
            }
            else if (FieldType == 0xc0bd)
            {
                byte ByteValue = (byte)s.ReadByte();
                Field.Value = new byte[] { ByteValue };
                Field.ValueType = DAIFieldType.DAI_Byte;
            }
            else if (FieldType == 0xc0cd)
            {
                byte ByteValue = (byte)s.ReadByte();
                Field.Value = new byte[] { ByteValue };
                Field.ValueType = DAIFieldType.DAI_UByte;
            }
            else if (FieldType == 0xc0dd)
            {
                ushort UShortValue = Tools.ReadUShort(s);
                Field.Value = BitConverter.GetBytes(UShortValue);
                Field.ValueType = DAIFieldType.DAI_UShort;
            }
            else if (FieldType == 0xc0ed)
            {
                short ShortValue = Tools.ReadShort(s);
                Field.Value = BitConverter.GetBytes(ShortValue);
                Field.ValueType = DAIFieldType.DAI_Short;
            }
            else if (FieldType == 0xc15d)
            {
                byte[] Value = new byte[16];
                for (int i = 0; i < 16; i++)
                    Value[i] = (byte)s.ReadByte();

                Field.Value = Value;
                Field.ValueType = DAIFieldType.DAI_LongLong;
            }
            else if (FieldType == 0x89 || FieldType == 0xc089)
            {
                String EnumValue = "";

                int CompareValue = Tools.ReadInt(s);
                DAIComplexDescriptor EnumComplex = ComplexDescriptors[Field.Descriptor.ComplexReference];

                if (EnumComplex.FieldCount != 0)
                {
                    for (int i = EnumComplex.FieldStartIndex; i < EnumComplex.FieldStartIndex + EnumComplex.FieldCount; i++)
                    {
                        if (FieldDescriptors[i].PayloadOffset == CompareValue)
                        {
                            EnumValue = FieldDescriptors[i].FieldName;
                            break;
                        }
                    }
                }

                Field.Value = new byte[EnumValue.Length];
                for (int i = 0; i < EnumValue.Length; i++) { Field.Value[i] = (byte)EnumValue[i]; }
                Field.ValueType = DAIFieldType.DAI_Enum;
            }
            else if (FieldType == 0x41)
            {
                int Index = Tools.ReadInt(s);
                DAIArrayRepeater ArrayRepeater = ArrayRepeaters[Index];
                DAIComplexDescriptor ArrayComplexDesc = ComplexDescriptors[Field.Descriptor.ComplexReference];

                s.Seek(Header.ArraySectionStart + ArrayRepeater.Offset, SeekOrigin.Begin);
                DAIComplex ArrayComplex = new DAIComplex();
                ArrayComplex.Descriptor = ArrayComplexDesc;
                ArrayComplex.Offset = s.Position;

                ArrayComplex.Fields = new List<DAIField>();
                for (int i = 0; i < ArrayRepeater.Count; i++)
                {
                    ArrayComplex.Fields.Add(ReadField(s, ArrayComplexDesc.FieldStartIndex));
                }

                Field.ComplexValue = ArrayComplex;
                Field.ValueType = DAIFieldType.DAI_Array;
            }
            else
            {
            }

            return Field;
        }

        public void Serialize(Stream s)
        {
            int Magic = Tools.ReadInt(s);
            if (Magic == 0x0fb2d1ce)
            {
                Header = new DAIHeader();
                Header.Serialize(s);

                /* File GUID */
                FileGuid = new byte[16];
                s.Read(FileGuid, 0, 16);

                /* Padding */
                while (s.Position % 16 != 0)
                    s.Seek(1, SeekOrigin.Current);

                /* External GUIDs */
                ExternalGuids = new List<DAIExternalGuid>();
                for (int i = 0; i < Header.ExternalGuidCount; i++)
                {
                    DAIExternalGuid ExternalGuid = new DAIExternalGuid();
                    s.Read(ExternalGuid.FileGuid, 0, 16);
                    s.Read(ExternalGuid.InstanceGuid, 0, 16);

                    ExternalGuids.Add(ExternalGuid);
                }

                /* Keywords */
                KeywordDict = new Dictionary<int, string>();
                Int64 StartPos = s.Position;

                while ((s.Position - StartPos) < Header.NameLength)
                {
                    String Keyword = Tools.ReadNullString(s);
                    int Hash = Hasher(Keyword);

                    if (!KeywordDict.ContainsKey(Hash))
                    {
                        KeywordDict.Add(Hash, Keyword);
                    }
                }

                /* Field descriptors */
                FieldDescriptors = new List<DAIFieldDescriptor>();
                for (int i = 0; i < Header.FieldCount; i++)
                {
                    DAIFieldDescriptor CurDescriptor = new DAIFieldDescriptor();
                    CurDescriptor.Serialize(s, this);

                    FieldDescriptors.Add(CurDescriptor);
                }

                /* Complex */
                ComplexDescriptors = new List<DAIComplexDescriptor>();
                for (int i = 0; i < Header.ComplexEntryCount; i++)
                {
                    DAIComplexDescriptor CurDescriptor = new DAIComplexDescriptor();
                    CurDescriptor.Serialize(s, this);

                    ComplexDescriptors.Add(CurDescriptor);
                }

                /* Instance repeaters */
                InstanceRepeaters = new List<DAIInstanceRepeater>();
                for (int i = 0; i < Header.InstanceRepeaterCount; i++)
                {
                    DAIInstanceRepeater CurRepeater = new DAIInstanceRepeater();
                    CurRepeater.Serialize(s, this);

                    InstanceRepeaters.Add(CurRepeater);
                }

                /* Padding */
                while (s.Position % 16 != 0)
                    s.Seek(1, SeekOrigin.Current);

                /* Array repeaters */
                ArrayRepeaters = new List<DAIArrayRepeater>();
                for (int i = 0; i < Header.ArrayRepeaterCount; i++)
                {
                    DAIArrayRepeater CurRepeater = new DAIArrayRepeater();
                    CurRepeater.Serialize(s, this);

                    ArrayRepeaters.Add(CurRepeater);
                }

                /* Payload */
                s.Seek(Header.StringOffset + Header.StringLength, SeekOrigin.Begin);
                InternalGuids = new List<byte[]>();
                Instances = new Dictionary<byte[], DAIComplex>();

                int Idx = 0;
                int NonGuidIndex = 0;
                foreach (DAIInstanceRepeater CurRepeater in InstanceRepeaters)
                {
                    for (int i = 0; i < CurRepeater.Count; i++)
                    {
                        /* Alignment */
                        while ((s.Position % ComplexDescriptors[CurRepeater.ComplexDescriptorIndex].Alignment) != 0)
                            s.Seek(1, SeekOrigin.Current);

                        byte[] InstanceGuid = null;
                        if (Idx < Header.GuidRepeaterCount)
                        {
                            InstanceGuid = new byte[16];
                            s.Read(InstanceGuid, 0, 16);
                        }
                        else
                        {
                            InstanceGuid = new byte[16];
                            InstanceGuid[12] = (byte)((NonGuidIndex >> 24) & 0xFF);
                            InstanceGuid[13] = (byte)((NonGuidIndex >> 16) & 0xFF);
                            InstanceGuid[14] = (byte)((NonGuidIndex >> 8) & 0xFF);
                            InstanceGuid[15] = (byte)((NonGuidIndex) & 0xFF);
                            NonGuidIndex++;
                        }

                        InternalGuids.Add(InstanceGuid);
                        Instances.Add(InstanceGuid, ReadComplex(s, CurRepeater.ComplexDescriptorIndex, true));
                    }

                    Idx++;
                }

                RootInstance = Instances.Values.ElementAt(0);
            }
        }

        public static DAIEbx ReadFromFile(String Filename)
        {
            FileStream file = new FileStream(Filename, FileMode.Open);
            DAIEbx ebx = new DAIEbx();
            ebx.Serialize(file);
            file.Close();

            return ebx;
        }

        public static String GuidToString(byte[] guid)
        {
            if (guid.Length != 16)
                throw new Exception("Guid length should be 16, is " + guid.Length);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 16; i++)
                sb.Append(guid[i].ToString("X2"));
            return sb.ToString();
        }

        public DAIGuid GetDaiGuidFieldValue(DAIField field)
        {
            Debug.Assert(field.ValueType == DAIFieldType.DAI_Guid, "this method can only be applied to GUID fields");

            var guid = new DAIGuid();
            uint UIntValue = field.GetUIntValue();
            if ((UIntValue >> 31) == 1)
            {
                /* External Guid */
                DAIExternalGuid Guid = this.ExternalGuids.ElementAt((int)(UIntValue & 0x7fffffff));
                guid.external = true;
                guid.fileGuid = GuidToString(Guid.FileGuid);
                guid.instanceGuid = GuidToString(Guid.InstanceGuid);
            }
            else if (UIntValue == 0)
            {
                /* NULL Guid */
                guid.instanceGuid = "null";
            }
            else
            {
                /* Internal Guid */
                byte[] Guid = this.InternalGuids[(int)(UIntValue - 1)];
                guid.instanceGuid = GuidToString(Guid);
            }

            return guid;
        }
    }
}
