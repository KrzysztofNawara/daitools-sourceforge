using DA_Tool.Frostbite;
using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Tool.Frostbite
{
    public class DAIVertex
    {
        public Vector3 Position;
        public Vector3 Normals;
        public Vector2 TexCoords;
        public int[] BoneIndices;
        public float[] BoneWeights;

        public DAIVertex()
        {
            Position = new Vector3();
            Normals = new Vector3();
            TexCoords = new Vector2();
        }
    }

    public class DAIFace
    {
        public uint V1;
        public uint V2;
        public uint V3;
    }

    public class DAIMeshBuffer
    {
        public List<DAIVertex> VertexBuffer;
        public List<DAIFace> IndexBuffer;

        public DAIMeshBuffer()
        {
            VertexBuffer = new List<DAIVertex>();
            IndexBuffer = new List<DAIFace>();
        }
    }

    public class DAIVertexEntry
    {
        public int Unknown01;
        public int Offset;
        public int VertexType;
    }

    public class DAISubObject
    {
        public Int64 Offset;

        public int Unknown01;
        public int Unknown02;
        public String SubObjectName;
        public int Unknown03;
        public int TriangleCount;
        public int StartIndex;
        public int VertexBufferOffset;
        public int VertexCount;
        public int VertexStride;
        public int Unknown04;
        public int Unknown05;
        public int Unknown06;
        public int[] Unknowns2;
        public ushort[] SubBoneList;

        public List<DAIVertexEntry> VertexEntries;

        public String SerializeString(Stream s)
        {
            long StringLocation = Tools.ReadLong(s);
            long PrevPosition = s.Position;
            s.Seek(StringLocation, SeekOrigin.Begin);
            String RetVal = Tools.ReadNullString(s);
            s.Seek(PrevPosition, SeekOrigin.Begin);

            return RetVal;
        }

        public void Serialize(Stream s)
        {
            Offset = s.Position;

            Unknown01 = Tools.ReadInt(s);
            Unknown02 = Tools.ReadInt(s);
            SubObjectName = SerializeString(s);
            Unknown03 = Tools.ReadInt(s);
            TriangleCount = Tools.ReadInt(s);
            StartIndex = Tools.ReadInt(s);

            VertexBufferOffset = Tools.ReadInt(s);
            VertexCount = Tools.ReadInt(s);

            Unknown04 = Tools.ReadInt(s);
            Unknown05 = Tools.ReadInt(s);
            Unknown06 = Tools.ReadInt(s);

            VertexEntries = new List<DAIVertexEntry>();
            for (int i = 0; i < 16; i++)
            {
                DAIVertexEntry VertexEntry = new DAIVertexEntry();
                VertexEntry.VertexType = Tools.ReadShort(s);
                VertexEntry.Offset = s.ReadByte();
                VertexEntry.Unknown01 = s.ReadByte();

                if (VertexEntry.Offset != 0xFF)
                {
                    VertexEntries.Add(VertexEntry);
                }
            }
            VertexStride = Tools.ReadInt(s);

            Unknowns2 = new int[19];
            for (int i = 0; i < 19; i++)
            {
                Unknowns2[i] = Tools.ReadInt(s);
            }
        }
    }

    public class DAILODLevel
    {
        public Int64 Size;
        public Int64 Offset;

        public int BoneDataCount;
        public int Unknown02;
        public int NumSubObjects;
        public List<DAISubObject> SubObjects;
        public int Unknown03;
        public Int64[] DataOffsets;
        public int[] DataValues;
        public int Unknown04;
        public int IndexBufferSize;
        public int VertexBufferSize;
        public int Unknown05;
        public byte[] ChunkID;
        public int InlineDataOffset;
        public int Unknown07;
        public int Unknown08;
        public String String01;
        public String String02;
        public String String03;
        public int Unknown09;
        public int Unknown10;
        public int Unknown11;
        public int BoneCount;
        public int Unknown13;

        public List<long> BoneData;
        public List<byte[]> BoneDataValues;

        public String SerializeString(Stream s)
        {
            long StringLocation = Tools.ReadLong(s);
            long PrevPosition = s.Position;
            s.Seek(StringLocation, SeekOrigin.Begin);
            String RetVal = Tools.ReadNullString(s);
            s.Seek(PrevPosition, SeekOrigin.Begin);

            return RetVal;
        }

        public void Serialize(Stream s)
        {
            Offset = s.Position;

            BoneDataCount = Tools.ReadInt(s);
            Unknown02 = Tools.ReadInt(s);
            NumSubObjects = Tools.ReadInt(s);
            long SubObjectLocation = Tools.ReadLong(s);
            Unknown03 = Tools.ReadInt(s);

            DataOffsets = new Int64[4];
            DataValues = new int[4];

            for (int i = 0; i < 4; i++)
            {
                DataOffsets[i] = Tools.ReadLong(s);
                DataValues[i] = Tools.ReadInt(s);
            }

            Unknown04 = Tools.ReadInt(s);
            IndexBufferSize = Tools.ReadInt(s);
            VertexBufferSize = Tools.ReadInt(s);
            Unknown05 = Tools.ReadInt(s);
            
            ChunkID = new byte[16];
            for (int i = 0; i < 16; i++)
                ChunkID[i] = (byte) s.ReadByte();

            InlineDataOffset = Tools.ReadInt(s);
            Unknown07 = Tools.ReadInt(s);
            Unknown08 = Tools.ReadInt(s);
            String01 = SerializeString(s);
            String02 = SerializeString(s);
            String03 = SerializeString(s);
            Unknown09 = Tools.ReadInt(s);
            Unknown10 = Tools.ReadInt(s);
            Unknown11 = Tools.ReadInt(s);
            BoneCount = Tools.ReadInt(s);

            BoneData = new List<Int64>();
            BoneDataValues = new List<byte[]>();
            for (int i = 0; i < BoneDataCount; i++)
            {
                BoneData.Add(Tools.ReadLong(s));
                BoneData.Add(Tools.ReadLong(s));
            }

            Unknown13 = Tools.ReadInt(s);

            SubObjects = new List<DAISubObject>();
            if (NumSubObjects > 0)
            {
                s.Seek(SubObjectLocation, SeekOrigin.Begin);
                for (int i = 0; i < NumSubObjects; i++)
                {
                    DAISubObject SubObject = new DAISubObject();
                    SubObject.Serialize(s);

                    SubObjects.Add(SubObject);
                }
            }

            for (int i = 0; i < NumSubObjects; i++)
            {
                if (SubObjects[i].Unknown05 > 0)
                {
                    int NumSubObjectBones = (SubObjects[i].Unknown04 >> 24);
                    SubObjects[i].SubBoneList = new ushort[NumSubObjectBones];
                    s.Seek(SubObjects[i].Unknown05, SeekOrigin.Begin);
                    for (int j = 0; j < NumSubObjectBones; j++)
                        SubObjects[i].SubBoneList[j] = Tools.ReadUShort(s);
                }
            }
        }
    }

    public class DAIMesh
    {
        public Vector3 MinPosition;
        public float Unknown01;
        public Vector3 MaxPosition;
        public float Unknown02;
        public List<DAILODLevel> LODLevels;
        public String MeshPath;
        public String MeshName;
        public int Unknown03;
        public int Unknown04;
        public int Unknown05;
        public int TotalLODCount;
        public int TotalSubObjectCount;
        public DAISkeleton Skeleton;

        public bool IsSkinned() { return LODLevels[0].BoneCount > 0; }

        public DAILODLevel GetLODByName(String Name)
        {
            for (int i = 0; i < LODLevels.Count; i++)
            {
                if (LODLevels[i].String03 == Name)
                    return LODLevels[i];
            }

            return null;
        }

        public String SerializeString(Stream s)
        {
            long StringLocation = Tools.ReadLong(s);
            long PrevPosition = s.Position;
            s.Seek(StringLocation, SeekOrigin.Begin);
            String RetVal = Tools.ReadNullString(s);
            s.Seek(PrevPosition, SeekOrigin.Begin);

            return RetVal;
        }

        public void Serialize(Stream s)
        {
            MinPosition.X = Tools.ReadFloat(s);
            MinPosition.Y = Tools.ReadFloat(s);
            MinPosition.Z = Tools.ReadFloat(s);
            Unknown01 = Tools.ReadFloat(s);

            MaxPosition.X = Tools.ReadFloat(s);
            MaxPosition.Y = Tools.ReadFloat(s);
            MaxPosition.Z = Tools.ReadFloat(s);
            Unknown02 = Tools.ReadFloat(s);

            Int64[] LODLocations = new Int64[6];
            for (int i = 0; i < 6; i++)
            {
                LODLocations[i] = Tools.ReadLong(s);
            }

            MeshPath = SerializeString(s);
            MeshName = SerializeString(s);

            Unknown03 = Tools.ReadInt(s);
            Unknown04 = Tools.ReadInt(s);
            Unknown05 = Tools.ReadInt(s);

            TotalLODCount = Tools.ReadShort(s);
            TotalSubObjectCount = Tools.ReadShort(s);

            LODLevels = new List<DAILODLevel>();
            for (int i = 0; i < 6; i++)
            {
                if (LODLocations[i] != 0x00)
                {
                    s.Seek(LODLocations[i], SeekOrigin.Begin);
                    DAILODLevel LodLevel = new DAILODLevel();
                    LodLevel.Serialize(s);

                    if ((i + 1) < 6 && LODLocations[i + 1] != 0x00)
                    {
                        LodLevel.Size = LODLocations[i + 1] - LODLocations[i];
                    }

                    LODLevels.Add(LodLevel);
                }
            }
        }

        public void SetSkeleton(DAISkeleton InSkeleton)
        {
            Skeleton = InSkeleton;
        }
    }
}
