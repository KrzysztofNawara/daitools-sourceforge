using DA_Tool.Frostbite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Tool.Frostbite
{
    public class Talktable
    {
        public class STR
        {
            public uint ID;
            public String Value;
        }

        public List<STR> Strings;

        public bool Read(MemoryStream s)
        {
            uint Magic = Tools.ReadUInt(s);
            if (Magic != 0xD78B40EB)
                return false;

            Tools.ReadInt(s);
            int DataOffset = Tools.ReadInt(s);
            Tools.ReadInt(s);
            Tools.ReadInt(s);
            Tools.ReadInt(s);

            int Data1Count = Tools.ReadInt(s);
            int Data1Offset = Tools.ReadInt(s);
            int Data2Count = Tools.ReadInt(s);
            int Data2Offset = Tools.ReadInt(s);
            int Data3Count = Tools.ReadInt(s);
            int Data3Offset = Tools.ReadInt(s);
            int Data4Count = Tools.ReadInt(s);
            int Data4Offset = Tools.ReadInt(s);
            if (Data4Count > 0)
            {
                int Data5Count = Tools.ReadInt(s);
                int Data5Offset = Tools.ReadInt(s);
            }

            s.Seek(Data1Offset, SeekOrigin.Begin);
            List<int> Data1 = new List<int>();
            for (int i = 0; i < Data1Count; i++)
                Data1.Add(Tools.ReadInt(s));

            s.Seek(Data2Offset, SeekOrigin.Begin);
            List<uint> StringIDs = new List<uint>();
            List<int> StringData = new List<int>();

            for (int i = 0; i < Data2Count; i++)
            {
                StringIDs.Add(Tools.ReadUInt(s));
                StringData.Add(Tools.ReadInt(s));
            }

            s.Seek(DataOffset, SeekOrigin.Begin);
            List<uint> Data = new List<uint>();
            while (s.Position < s.Length)
                Data.Add(Tools.ReadUInt(s));

            Strings = new List<STR>();
            for (int i = 0; i < StringIDs.Count; i++)
            {
                STR ValueString = new STR();
                ValueString.ID = StringIDs[i];
                ValueString.Value = "";

                int Index = StringData[i] >> 5;
                int Shift = StringData[i] & 0x1F;

                while (true)
                {
                    int e = (Data1.Count / 2) - 1;
                    while (e > 0)
                    {
                        int offset = (int)((Data[Index] >> Shift) & 1);
                        e = Data1[(e * 2) + offset];

                        Shift++;
                        Index += (Shift >> 5);
                        Shift %= 32;
                    }

                    ushort c = (ushort)(0xFFFF - e);
                    if (c == 0)
                        break;

                    ValueString.Value += (char)c;
                }

                Strings.Add(ValueString);
            }

            return true;
        }
    }
}
