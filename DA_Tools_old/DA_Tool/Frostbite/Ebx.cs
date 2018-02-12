using DA_Tool.Frostbite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DA_Tool.Frostbite
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
        DAI_Long,
        DAI_LongLong,
        DAI_Guid
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
        public long GetLongValue() { return BitConverter.ToInt64(Value, 0); }
        public byte[] GetLongLongValue() { return Value; }

        public string GetString(DAIEbx EbxFile, ref int TabCount)
        {
            String RetVal = "";

            if (Descriptor.FieldName == "$")
            {
                TabCount--;
                RetVal += ComplexValue.GetString(EbxFile, ref TabCount);
                TabCount++;
                return RetVal;
            }

            RetVal += DAIEbx.Tabs(TabCount) + "<" + Descriptor.FieldName + ">";

            switch (ValueType)
            {
                case DAIFieldType.DAI_Complex:
                case DAIFieldType.DAI_Array:
                    RetVal += "\n" + ComplexValue.GetString(EbxFile, ref TabCount);
                    RetVal += DAIEbx.Tabs(TabCount) + "</" + Descriptor.FieldName + ">\n";
                    return RetVal;

                case DAIFieldType.DAI_String:
                    RetVal += GetStringValue();
                    break;

                case DAIFieldType.DAI_Int:
                    RetVal += GetIntValue().ToString("X8");
                    break;

                case DAIFieldType.DAI_UInt:
                    RetVal += GetUIntValue().ToString("X8");
                    break;

                case DAIFieldType.DAI_Float:
                    RetVal += GetFloatValue().ToString("F3");
                    break;

                case DAIFieldType.DAI_Short:
                    RetVal += GetShortValue().ToString("X4");
                    break;

                case DAIFieldType.DAI_UShort:
                    RetVal += GetUShortValue().ToString("X4");
                    break;

                case DAIFieldType.DAI_Byte:
                    RetVal += GetByteValue().ToString("X2");
                    break;

                case DAIFieldType.DAI_Long:
                    RetVal += GetLongValue().ToString("X16");
                    break;

                case DAIFieldType.DAI_LongLong:
                    for (int i = 0; i < Value.Length; i++)
                        RetVal += Value[i].ToString("X2");
                    break;

                case DAIFieldType.DAI_Enum:
                    RetVal += GetEnumValue();
                    break;

                case DAIFieldType.DAI_Guid:
                    {
                        uint UIntValue = GetUIntValue();
                        if ((UIntValue >> 31) == 1)
                        {
                            /* External Guid */
                            DAIExternalGuid Guid = EbxFile.ExternalGuids.ElementAt((int)(UIntValue & 0x7fffffff));
                            RetVal += Guid.ToString();
                        }
                        else if (UIntValue == 0)
                        {
                            /* NULL Guid */
                            RetVal += "00";
                        }
                        else
                        {
                            /* Internal Guid */
                            byte[] Guid = EbxFile.InternalGuids[(int)(UIntValue - 1)];
                            for (int i = 0; i < Guid.Length; i++)
                                RetVal += Guid[i].ToString("X2");

                        }
                    }
                    break;
            }

            RetVal += "</" + Descriptor.FieldName + ">\n";
            return RetVal;
        }

        public void WriteToXMLWriter(XmlWriter xmlWriter, DAIEbx EbxFile)
        {
            if (Descriptor.FieldName == "$")
            {
                ComplexValue.WriteToXmlWriter(xmlWriter, EbxFile);
                return;
            }

            xmlWriter.WriteStartElement(Descriptor.FieldName);

            switch (ValueType)
            {
                case DAIFieldType.DAI_Complex:
                case DAIFieldType.DAI_Array:
                    if (ComplexValue != null)
                    {
                        ComplexValue.WriteToXmlWriter(xmlWriter, EbxFile);
                    }
                    break;

                case DAIFieldType.DAI_String:
                    xmlWriter.WriteString(XmlConvert.EncodeNmToken(GetStringValue()));
                    break;

                case DAIFieldType.DAI_Int:
                    xmlWriter.WriteValue(GetIntValue().ToString("X8"));
                    break;

                case DAIFieldType.DAI_UInt:
                    xmlWriter.WriteValue(GetUIntValue().ToString("X8"));
                    break;

                case DAIFieldType.DAI_Float:
                    xmlWriter.WriteValue(GetFloatValue().ToString("F3"));
                    break;

                case DAIFieldType.DAI_Short:
                    xmlWriter.WriteValue(GetShortValue().ToString("X4"));
                    break;

                case DAIFieldType.DAI_UShort:
                    xmlWriter.WriteValue(GetUShortValue().ToString("X4"));
                    break;

                case DAIFieldType.DAI_Byte:
                    xmlWriter.WriteValue(GetByteValue().ToString("X2"));
                    break;

                case DAIFieldType.DAI_Long:
                    xmlWriter.WriteValue(GetLongValue().ToString("X16"));
                    break;

                case DAIFieldType.DAI_LongLong:
                    for (int i = 0; i < Value.Length; i++)
                        xmlWriter.WriteValue(Value[i].ToString("X2"));
                    break;

                case DAIFieldType.DAI_Enum:
                    xmlWriter.WriteString(XmlConvert.EncodeNmToken(GetEnumValue()));
                    break;

                case DAIFieldType.DAI_Guid:
                    {
                        uint UIntValue = GetUIntValue();
                        if ((UIntValue >> 31) == 1)
                        {
                            /* External Guid */
                            DAIExternalGuid Guid = EbxFile.ExternalGuids.ElementAt((int)(UIntValue & 0x7fffffff));
                            xmlWriter.WriteValue(Guid.ToString());
                        }
                        else if (UIntValue == 0)
                        {
                            /* NULL Guid */
                            xmlWriter.WriteValue("00");
                        }
                        else
                        {
                            /* Internal Guid */
                            byte[] Guid = EbxFile.InternalGuids[(int)(UIntValue - 1)];
                            for (int i = 0; i < Guid.Length; i++)
                                xmlWriter.WriteValue(Guid[i].ToString("X2"));
                        }
                    }
                    break;
            }

            xmlWriter.WriteEndElement();
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

        public string GetString(DAIEbx EbxFile, ref int TabCount, bool bPrintDescriptor = true)
        {
            String RetVal = "";

            if (Descriptor.FieldName != "array")
            {
                if (bPrintDescriptor)
                {
                    TabCount++;
                    RetVal += DAIEbx.Tabs(TabCount) + "<" + Descriptor.FieldName + ">\n";
                    TabCount++;
                }
            }
            else
            {
                TabCount++;
            }

            foreach (DAIField CurField in Fields)
            {
                RetVal += CurField.GetString(EbxFile, ref TabCount);
            }

            if (Descriptor.FieldName != "array")
            {
                if (bPrintDescriptor)
                {
                    TabCount--;
                    RetVal += DAIEbx.Tabs(TabCount) + "</" + Descriptor.FieldName + ">\n";
                    TabCount--;
                }
            }
            else
            {
                TabCount--;
            }

            return RetVal;
        }

        public void WriteToXmlWriter(XmlWriter xmlWriter, DAIEbx EbxFile)
        {
            if (Descriptor.FieldName != "array")
            {
                xmlWriter.WriteStartElement(Descriptor.FieldName);
            }

            foreach (DAIField CurField in Fields)
            {
                CurField.WriteToXMLWriter(xmlWriter, EbxFile);
            }

            if (Descriptor.FieldName != "array")
            {
                xmlWriter.WriteEndElement();
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

        public static String Tabs(int TabCount)
        {
            String TabStr = "";
            for (int i = 0; i < TabCount; i++)
            {
                TabStr += "  ";
            }

            return TabStr;
        }

        public string GetString()
        {
            String RetVal = "";
            int TabCount = 0;

            for (int i = 0; i < Instances.Count; i++)
            {
                byte[] Guid = Instances.Keys.ElementAt(i);
                DAIComplex ComplexValue = Instances.Values.ElementAt(i);

                String GuidString = "";
                for (int j = 0; j < Guid.Length; j++)
                    GuidString += Guid[j].ToString("X2");

                RetVal += "<" + ComplexValue.Descriptor.FieldName + " Guid=\"" + GuidString + "\">\n";
                TabCount++;

                RetVal += ComplexValue.GetString(this, ref TabCount, false);

                TabCount--;
                RetVal += "</" + ComplexValue.Descriptor.FieldName + ">\n";
            }

            return RetVal;
        }

        public void WriteToXMLWriter(XmlWriter xmlWriter)
        {
            for (int i = 0; i < Instances.Count; i++)
            {
                byte[] Guid = Instances.Keys.ElementAt(i);
                DAIComplex ComplexValue = Instances.Values.ElementAt(i);

                String GuidString = "";
                for (int j = 0; j < Guid.Length; j++)
                    GuidString += Guid[j].ToString("X2");

                xmlWriter.WriteStartElement(ComplexValue.Descriptor.FieldName);
                xmlWriter.WriteAttributeString("Guid", GuidString);

                ComplexValue.WriteToXmlWriter(xmlWriter, this);

                xmlWriter.WriteEndElement();
            }
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
                Field.Value = BitConverter.GetBytes(ByteValue);
                Field.ValueType = DAIFieldType.DAI_Byte;
            }
            else if (FieldType == 0xc0cd)
            {
                byte ByteValue = (byte)s.ReadByte();
                Field.Value = BitConverter.GetBytes(ByteValue);
                Field.ValueType = DAIFieldType.DAI_Byte;
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
                            InstanceGuid = BitConverter.GetBytes(NonGuidIndex);
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
    }
}
