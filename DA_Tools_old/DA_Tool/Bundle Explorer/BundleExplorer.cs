using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DA_Tool.Frostbite;
using Be.Windows.Forms;
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.Xml;

namespace DA_Tool.Bundle_Explorer
{
    public partial class BundleExplorer : Form
    {
        public CATFile cat;
        public CASFile cas;
        public SBFile sb;

        public BundleExplorer()
        {
            InitializeComponent();
        }

        private void BundleExplorer_Load(object sender, EventArgs e)
        {
            toolStripComboBox1.Items.Clear();
            List<string> resTypes = new List<string>(Tools.ResTypes.Values);
            List<uint> resT = new List<uint>(Tools.ResTypes.Keys);
            int count = 0;
            foreach (string t in resTypes)
                toolStripComboBox1.Items.Add(resT[count++].ToString("X8") + " : " + t);
            toolStripComboBox1.SelectedIndex = 0;
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
            RefreshMe();
        }

        public void RefreshMe()
        {
            listBox1.Items.Clear();
            foreach (Bundle b in sb.bundles)
                listBox1.Items.Add(b.path);
            listBox2.Items.Clear();
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rtb2.SendToBack();
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
            Bundle b = sb.bundles[n];
            StringBuilder s = new StringBuilder();
            s.Append("path: " + b.path + "\n");
            s.Append("magicSalt: " + b.salt + "\n");
            s.Append("alignMembers: " + b.align + "\n");
            s.Append("ridSupport: " + b.ridsupport + "\n");
            s.Append("storeCompressedSizes: " + b.compressed + "\n");
            s.Append("totalSize: " + b.totalsize.ToString("X") + "\n");
            s.Append("dbxTotalSize: " + b.dbxtotalsize.ToString("X") + "\n");
            rtb1.Text = s.ToString();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
            listBox5.Items.Clear();
            if (b.ebx != null)
                foreach (Bundle.ebxtype ebx in b.ebx)
                    listBox2.Items.Add(ebx.name);
            if (b.dbx != null)
                foreach (Bundle.dbxtype dbx in b.dbx)
                    listBox3.Items.Add(dbx.name);
            if (b.res != null)
                foreach (Bundle.restype res in b.res)
                {
                    uint r = BitConverter.ToUInt32(res.rtype, 0);
                    listBox4.Items.Add(r.ToString("X8") + " : " + res.name + Tools.GetResType(r));
                }
            if (b.chunk != null)
                foreach (Bundle.chunktype chunk in b.chunk)
                {
                    s = new StringBuilder();
                    s.Append("id:");
                    foreach (byte bb in chunk.id)
                        s.Append(bb.ToString("X2") + " ");
                    s.Append("SHA1:");
                    foreach (byte bb in chunk.SHA1)
                        s.Append(bb.ToString("X2") + " ");
                    listBox5.Items.Add(s);
                }
        }

        private void exportPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hb1.ByteProvider != null && hb1.ByteProvider.Length != 0)
            {
                MemoryStream m = new MemoryStream();
                for (int i = 0; i < hb1.ByteProvider.Length; i++)
                    m.WriteByte(hb1.ByteProvider.ReadByte(i));
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.*|*.*";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(d.FileName, m.ToArray());
                    MessageBox.Show("Done.");
                }
            }
        }

        private void listBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
                int n = listBox1.SelectedIndex;
                int m = listBox2.SelectedIndex;
                if (n == -1 || n == -1)
                    return;
                Bundle b = sb.bundles[n];
                Bundle.ebxtype entry = b.ebx[m];
                byte[] data = Tools.GetDataBySHA1(entry.SHA1, cat);
                hb1.ByteProvider = new DynamicByteProvider(data);
                if (data.Length == -1)
                {
                    rtb2.BringToFront();
                    rtb2.Text = Encoding.UTF8.GetString(Tools.ExtractEbx(new MemoryStream(data)));
                }
            }
            catch (Exception)
            {
            }
        }

        public byte[] ExtractStaticMesh(MemoryStream ResBuffer, DAIMesh Mesh, float OverrideScale = 1.0f)
        {
            MemoryStream OutputStream = new MemoryStream();
            StreamWriter Writer = new StreamWriter(OutputStream);

            int Index = 0;
            uint Offset = 0;
            foreach (DAILODLevel CurLod in Mesh.LODLevels)
            {
                List<DAIMeshBuffer> buffers = ReadMeshChunk(CurLod, OverrideScale, true);
                for (int bufIdx = 0; bufIdx < buffers.Count; bufIdx++)
                {
                    DAIMeshBuffer buffer = buffers[bufIdx];
                    DAISubObject subObj = CurLod.SubObjects[bufIdx];

                    Writer.WriteLine("g " + subObj.SubObjectName + "_lod" + Index);

                    for (int i = 0; i < buffer.VertexBuffer.Count; i++)
                        Writer.WriteLine("v " + buffer.VertexBuffer[i].Position.X.ToString("F3") +
                            " " + buffer.VertexBuffer[i].Position.Y.ToString("F3") +
                            " " + buffer.VertexBuffer[i].Position.Z.ToString("F3"));

                    for (int i = 0; i < buffer.VertexBuffer.Count; i++)
                        Writer.WriteLine("vn " + buffer.VertexBuffer[i].Normals.X.ToString("F3") +
                            " " + buffer.VertexBuffer[i].Normals.Y.ToString("F3") +
                            " " + buffer.VertexBuffer[i].Normals.Z.ToString("F3"));

                    for (int i = 0; i < buffer.VertexBuffer.Count; i++)
                        Writer.WriteLine("vt " + buffer.VertexBuffer[i].TexCoords.X.ToString("F3") +
                            " " + buffer.VertexBuffer[i].TexCoords.Y.ToString("F3"));

                    for (int i = 0; i < subObj.TriangleCount; i++)
                    {
                        DAIFace f = buffer.IndexBuffer[i];

                        Writer.Write("f ");
                        Writer.Write(f.V1.ToString() + "/" + f.V1.ToString() + "" + "/" + f.V1.ToString() + " ");
                        Writer.Write(f.V2.ToString() + "/" + f.V2.ToString() + "" + "/" + f.V2.ToString() + " ");
                        Writer.Write(f.V3.ToString() + "/" + f.V3.ToString() + "" + "/" + f.V3.ToString() + " ");
                        Writer.Write("\n");
                    }

                    Offset += (uint)subObj.VertexCount;
                }
                Index++;
            }

            Writer.Close();
            return OutputStream.ToArray();
        }

        public byte[] ExtractMesh(MemoryStream ResBuffer, ref string filter, float OverrideScale = 1.0f)
        {
            DAIMesh Mesh = new DAIMesh();
            Mesh.Serialize(ResBuffer);

            if (Mesh.IsSkinned())
            {
                filter = "*.psk|*.psk";
                return ExtractSkinnedMesh(ResBuffer, Mesh, OverrideScale);
            }
            else
            {
                filter = "*.obj|*.obj";
                return ExtractStaticMesh(ResBuffer, Mesh, OverrideScale);
            }
        }

        public byte[] ExtractTexture(MemoryStream TexBuffer)
        {
            int n = listBox1.SelectedIndex;
            Bundle b = sb.bundles[n];

            TextureInfo t = new TextureInfo();

            //TexBuffer.Seek(0x70, SeekOrigin.Begin);
            //t.name = Tools.ReadNullString(TexBuffer);

            TexBuffer.Seek(12, SeekOrigin.Begin);
            t.pixelFormatID = Tools.ReadUInt(TexBuffer);
            TexBuffer.Seek(2, SeekOrigin.Current);
            t.textureWidth = Tools.ReadUShort(TexBuffer);
            t.textureHeight = Tools.ReadUShort(TexBuffer);
            TexBuffer.Seek(4, SeekOrigin.Current);
            t.sizes = (uint)TexBuffer.ReadByte();
            TexBuffer.Seek(1, SeekOrigin.Current);
            byte[] id = new byte[16];
            for (int j = 0; j < 16; j++)
            {
                id[j] = (byte)TexBuffer.ReadByte();
            }
            t.mipSizes = new List<uint>();
            for (int mipCount = 0; mipCount < Math.Min(14, t.sizes); mipCount++)
            {
                t.mipSizes.Add(Tools.ReadUInt(TexBuffer));
            }
            DAITexture.SetPixelFormatData(ref t, t.pixelFormatID);

            CASFile.CASEntry e = new CASFile.CASEntry();
            if (!GetChunkData(id, b, ref e))
                return null;

            MemoryStream outputStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(outputStream);

            DAITexture.WriteTextureHeader(t, writer);
            writer.Write(e.data);

            writer.Close();
            return outputStream.ToArray();
        }

        public byte[] ExtractSkinnedMesh(MemoryStream ResBuffer, DAIMesh Mesh, float OverrideScale = 1.0f)
        {
            DAISkeleton skeleton = null;

            DialogResult dr = MessageBox.Show("Search local bundle for skeleton?", "Skeleton search", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                int n = listBox1.SelectedIndex;
                Bundle b = sb.bundles[n];

                foreach (Bundle.ebxtype ebx in b.ebx)
                {
                    string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";
                    List<uint> catline = cat.FindBySHA1(ebx.SHA1);
                    CASFile cas = new CASFile(CASFile.GetCASFileName(basepath, catline[7]));
                    CASFile.CASEntry e = cas.ReadEntry(catline.ToArray());

                    DAIEbx ebxFile = new DAIEbx();
                    ebxFile.Serialize(new MemoryStream(e.data));

                    if (ebxFile.RootInstance.GetName() == "SkeletonAsset")
                    {
                        skeleton = new DAISkeleton(ebxFile);
                        break;
                    }
                }
            }

            /* Couldn't find in local bundle */
            if (skeleton == null)
            {
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "*.*|*.*";
                d.Title = "Select Skeleton EBX file";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DAIEbx SkeletonEbx = DAIEbx.ReadFromFile(d.FileName);
                    if (SkeletonEbx == null || SkeletonEbx.RootInstance.GetName() != "SkeletonAsset")
                        return null;

                    skeleton = new DAISkeleton(SkeletonEbx);
                }
            }

            /* No skeleton defined still */
            if (skeleton != null)
            {
                Mesh.SetSkeleton(skeleton);

                /* Only extracting 1st LOD for now */
                List<DAIMeshBuffer> buffers = ReadMeshChunk(Mesh.LODLevels[0], OverrideScale);

                PSKFile Psk = new PSKFile();
                Psk.points = new List<PSKFile.PSKPoint>();
                Psk.edges = new List<PSKFile.PSKEdge>();
                Psk.materials = new List<PSKFile.PSKMaterial>();
                Psk.bones = new List<PSKFile.PSKBone>();
                Psk.faces = new List<PSKFile.PSKFace>();
                Psk.weights = new List<PSKFile.PSKWeight>();

                for (int i = 0; i < Mesh.Skeleton.Bones.Count; i++)
                {
                    PSKFile.PSKBone Bone = new PSKFile.PSKBone();
                    Bone.name = Mesh.Skeleton.Bones[i].Name;
                    Bone.childs = Mesh.Skeleton.Bones[i].Children.Count;
                    Bone.parent = Mesh.Skeleton.Bones[i].ParentIndex;
                    Bone.index = i;
                    Bone.location = new PSKFile.PSKPoint(Mesh.Skeleton.Bones[i].Location * OverrideScale);
                    Bone.rotation = new PSKFile.PSKQuad(0, 0, 0, 0);

                    float[][] RotMatrix = new float[4][];
                    RotMatrix[0] = new float[4];
                    RotMatrix[1] = new float[4];
                    RotMatrix[2] = new float[4];
                    RotMatrix[3] = new float[4];

                    RotMatrix[0][0] = Mesh.Skeleton.Bones[i].Right.X; RotMatrix[0][1] = Mesh.Skeleton.Bones[i].Right.Y; RotMatrix[0][2] = Mesh.Skeleton.Bones[i].Right.Z; RotMatrix[0][3] = 0.0f;
                    RotMatrix[1][0] = Mesh.Skeleton.Bones[i].Up.X; RotMatrix[1][1] = Mesh.Skeleton.Bones[i].Up.Y; RotMatrix[1][2] = Mesh.Skeleton.Bones[i].Up.Z; RotMatrix[1][3] = 0.0f;
                    RotMatrix[2][0] = Mesh.Skeleton.Bones[i].Forward.X; RotMatrix[2][1] = Mesh.Skeleton.Bones[i].Forward.Y; RotMatrix[2][2] = Mesh.Skeleton.Bones[i].Forward.Z; RotMatrix[2][3] = 0.0f;
                    RotMatrix[3][0] = 0.0f; RotMatrix[3][1] = 0.0f; RotMatrix[3][2] = 0.0f; RotMatrix[3][3] = 1.0f;

                    Microsoft.DirectX.Vector4 Quat = new Microsoft.DirectX.Vector4();
                    float tr = RotMatrix[0][0] + RotMatrix[1][1] + RotMatrix[2][2];
                    float s;

                    if (tr > 0.0f)
                    {
                        float InvS = 1.0f / (float)Math.Sqrt(tr + 1.0f);
                        Quat.W = 0.5f * (1.0f / InvS);
                        s = 0.5f * InvS;

                        Quat.X = (RotMatrix[1][2] - RotMatrix[2][1]) * s;
                        Quat.Y = (RotMatrix[2][0] - RotMatrix[0][2]) * s;
                        Quat.Z = (RotMatrix[0][1] - RotMatrix[1][0]) * s;
                    }
                    else
                    {
                        int m = 0;
                        if (RotMatrix[1][1] > RotMatrix[0][0])
                            m = 1;

                        if (RotMatrix[2][2] > RotMatrix[m][m])
                            m = 2;

                        int[] nxt = new int[] { 1, 2, 0 };
                        int j = nxt[m];
                        int k = nxt[j];

                        s = RotMatrix[m][m] - RotMatrix[j][j] - RotMatrix[k][k] + 1.0f;
                        float InvS = 1.0f / (float)Math.Sqrt(s);

                        float[] qt = new float[4];
                        qt[m] = 0.5f * (1.0f / InvS);
                        s = 0.5f * InvS;

                        qt[3] = (RotMatrix[j][k] - RotMatrix[k][j]) * s;
                        qt[j] = (RotMatrix[m][j] + RotMatrix[j][m]) * s;
                        qt[k] = (RotMatrix[m][k] + RotMatrix[k][m]) * s;

                        Quat.X = qt[0];
                        Quat.Y = qt[1];
                        Quat.Z = qt[2];
                        Quat.W = qt[3];
                    }

                    Bone.rotation = new PSKFile.PSKQuad(Quat);
                    Psk.bones.Add(Bone);
                }

                int offset = 0;
                int matIdx = 0;
                for (int bufIdx = 0; bufIdx < buffers.Count; bufIdx++)
                {
                    DAIMeshBuffer MeshBuffer = buffers[bufIdx];
                    for (int i = 0; i < MeshBuffer.VertexBuffer.Count; i++)
                    {
                        Psk.points.Add(new PSKFile.PSKPoint(MeshBuffer.VertexBuffer[i].Position));
                        Psk.edges.Add(new PSKFile.PSKEdge((ushort)(offset + i), MeshBuffer.VertexBuffer[i].TexCoords, (byte)matIdx));

                        for (int x = 0; x < 4; x++)
                        {
                            float Weight = MeshBuffer.VertexBuffer[i].BoneWeights[x];

                            int BoneIndex = MeshBuffer.VertexBuffer[i].BoneIndices[x];
                            int SubObjectBoneIndex = Mesh.LODLevels[0].SubObjects[bufIdx].SubBoneList[BoneIndex];

                            Psk.weights.Add(new PSKFile.PSKWeight(
                                Weight,
                                (int)(offset + i),
                                SubObjectBoneIndex
                                ));
                        }
                    }

                    for (int i = 0; i < MeshBuffer.IndexBuffer.Count; i++)
                    {
                        Psk.faces.Add(new PSKFile.PSKFace(
                            (int)(offset + MeshBuffer.IndexBuffer[i].V1),
                            (int)(offset + MeshBuffer.IndexBuffer[i].V2),
                            (int)(offset + MeshBuffer.IndexBuffer[i].V3),
                            (byte)matIdx)
                        );
                    }

                    Psk.materials.Add(new PSKFile.PSKMaterial("", matIdx));

                    offset += Mesh.LODLevels[0].SubObjects[bufIdx].VertexCount;
                    matIdx++;
                }

                return Psk.SaveToMemory().ToArray();
            }
            else
            {
                MessageBox.Show("Unable to export skinned mesh. Possibly missing skeleton asset");
            }

            return null;
        }

        private void listBox3_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            rtb2.SendToBack();
            try
            {
                hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
                int n = listBox1.SelectedIndex;
                int m = listBox3.SelectedIndex;
                if (n == -1 || n == -1)
                    return;
                Bundle b = sb.bundles[n];
                Bundle.dbxtype entry = b.dbx[m];
                byte[] data = Tools.GetDataBySHA1(entry.SHA1, cat);
                hb1.ByteProvider = new DynamicByteProvider(data);

            }
            catch (Exception)
            {
            }
        }

        private void listBox4_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            rtb2.SendToBack();
            try
            {
                hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
                int n = listBox1.SelectedIndex;
                int m = listBox4.SelectedIndex;
                if (n == -1 || n == -1)
                    return;
                Bundle b = sb.bundles[n];
                Bundle.restype entry = b.res[m];
                byte[] data = Tools.GetDataBySHA1(entry.SHA1, cat);
                hb1.ByteProvider = new DynamicByteProvider(data);
                uint type = BitConverter.ToUInt32(entry.rtype, 0);
                if (type == 0xafecb022)//.luac
                {
                    rtb2.BringToFront();
                    rtb2.Text = Tools.DecompileLUAC(data);
                }
            }
            catch (Exception)
            {
            }
        }

        private void listBox5_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            rtb2.SendToBack();
            try
            {
                hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
                int n = listBox1.SelectedIndex;
                int m = listBox5.SelectedIndex;
                if (n == -1 || n == -1)
                    return;
                Bundle b = sb.bundles[n];
                Bundle.chunktype entry = b.chunk[m];
                byte[] data = Tools.GetDataBySHA1(entry.SHA1, cat);
                hb1.ByteProvider = new DynamicByteProvider(data);
            }
            catch (Exception)
            {
            }
        }

        private bool GetChunkData(byte[] chunkId, Bundle b, ref CASFile.CASEntry e)
        {
            byte[] SHA1 = null;
            for (int i = 0; i < b.chunk.Count; i++)
            {
                bool match = true;
                for (int j = 0; j < 16; j++)
                    if (b.chunk[i].id[j] != chunkId[j])
                    {
                        match = false;
                        break;
                    }
                if (match)
                {
                    SHA1 = b.chunk[i].SHA1;
                    break;
                }
            }

            // chunk not found in bundle. Look in chunks0.toc
            if (SHA1 == null)
            {
                List<Tools.Entry> chunks = (List<Tools.Entry>)cat.chunks0.lines[0].fields[1].data;
                for (int i = 0; i < chunks.Count; i++)
                {
                    bool match = true;
                    for (int j = 0; j < 16; j++)
                    {
                        byte[] id = (byte[])chunks[i].fields[0].data;
                        if (id[j] != chunkId[j])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        SHA1 = (byte[])chunks[i].fields[1].data;
                        break;
                    }
                }

                // chunk not found in chunks0.toc
                if (SHA1 == null)
                {
                    //e = new CASFile.CASEntry();

                    /* If ChunkID == 0x00 then it uses inline data, which we cannot extract yet */
                    MessageBox.Show("Cannot find chunk in current bundle or chunks0.toc. Cannot export");
                    return false;
                }
            }

            string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";

            List<uint> catline = cat.FindBySHA1(SHA1);
            CASFile cas = new CASFile(CASFile.GetCASFileName(basepath, catline[7]));
            e = cas.ReadEntry(catline.ToArray());

            return true;
        }

        private List<DAIMeshBuffer> ReadMeshChunk(DAILODLevel LOD, float ScaleOverride = 1.0f, bool bFlipTexCoords = false)
        {
            List<DAIMeshBuffer> MeshBuffers = new List<DAIMeshBuffer>();

            int n = listBox1.SelectedIndex;
            Bundle b = sb.bundles[n];

            CASFile.CASEntry e = new CASFile.CASEntry();
            if (!GetChunkData(LOD.ChunkID, b, ref e))
                return null;

            MemoryStream chunkStream = new MemoryStream(e.data);
            foreach (DAISubObject SubObject in LOD.SubObjects)
            {
                if (SubObject.SubObjectName == "")
                    continue;

                DAIMeshBuffer MeshBuffer = new DAIMeshBuffer();
                chunkStream.Seek(SubObject.VertexBufferOffset, SeekOrigin.Begin);
                for (int i = 0; i < SubObject.VertexCount; i++)
                {
                    Int64 VertexStartOffset = chunkStream.Position;

                    DAIVertex Vertex = new DAIVertex();

                    foreach (DAIVertexEntry VertexEntry in SubObject.VertexEntries)
                    {
                        chunkStream.Seek(VertexStartOffset + VertexEntry.Offset, SeekOrigin.Begin);

                        if (VertexEntry.VertexType == 0x301)
                        {
                            Vertex.Position.X = Tools.ReadFloat(chunkStream) * ScaleOverride;
                            Vertex.Position.Y = Tools.ReadFloat(chunkStream) * ScaleOverride;
                            Vertex.Position.Z = Tools.ReadFloat(chunkStream) * ScaleOverride;
                        }
                        else if (VertexEntry.VertexType == 0x701)
                        {
                            Vertex.Position.X = HalfUtils.Unpack(Tools.ReadUShort(chunkStream)) * ScaleOverride;
                            Vertex.Position.Y = HalfUtils.Unpack(Tools.ReadUShort(chunkStream)) * ScaleOverride;
                            Vertex.Position.Z = HalfUtils.Unpack(Tools.ReadUShort(chunkStream)) * ScaleOverride;
                        }
                        else if (VertexEntry.VertexType == 0x806)
                        {
                            Vertex.Normals.X = HalfUtils.Unpack(Tools.ReadUShort(chunkStream));
                            Vertex.Normals.Y = HalfUtils.Unpack(Tools.ReadUShort(chunkStream));
                            Vertex.Normals.Z = HalfUtils.Unpack(Tools.ReadUShort(chunkStream));
                        }
                        else if (VertexEntry.VertexType == 0x621)
                        {
                            Vertex.TexCoords.X = HalfUtils.Unpack(Tools.ReadUShort(chunkStream));
                            Vertex.TexCoords.Y = HalfUtils.Unpack(Tools.ReadUShort(chunkStream));

                            if (bFlipTexCoords)
                                Vertex.TexCoords.Y = 1.0f - Vertex.TexCoords.Y;
                        }
                        else if (VertexEntry.VertexType == 0xC02)
                        {
                            Vertex.BoneIndices = new int[4];

                            for (int x = 0; x < 4; x++)
                                Vertex.BoneIndices[x] = (byte)chunkStream.ReadByte();
                        }
                        else if (VertexEntry.VertexType == 0xD04)
                        {
                            Vertex.BoneWeights = new float[4];

                            for (int x = 0; x < 4; x++)
                                Vertex.BoneWeights[x] = chunkStream.ReadByte() / 255.0f;
                        }
                    }

                    MeshBuffer.VertexBuffer.Add(Vertex);

                    chunkStream.Seek(VertexStartOffset + SubObject.VertexStride, SeekOrigin.Begin);
                }

                chunkStream.Seek(LOD.VertexBufferSize + (SubObject.StartIndex * 2), SeekOrigin.Begin);
                for (int i = 0; i < SubObject.TriangleCount; i++)
                {
                    DAIFace Face = new DAIFace();
                    Face.V1 = (uint)(Tools.ReadUShort(chunkStream));
                    Face.V2 = (uint)(Tools.ReadUShort(chunkStream));
                    Face.V3 = (uint)(Tools.ReadUShort(chunkStream));

                    MeshBuffer.IndexBuffer.Add(Face);
                }

                MeshBuffers.Add(MeshBuffer);
            }

            chunkStream.Close();
            return MeshBuffers;
        }

        private void exportResourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (hb1.ByteProvider != null && hb1.ByteProvider.Length != 0)
            {
                int n = listBox1.SelectedIndex;
                int a = listBox2.SelectedIndex;
                int b = listBox4.SelectedIndex;

                if (n == -1 && a == -1 && b == -1)
                    return;

                MemoryStream m = new MemoryStream();
                for (int i = 0; i < hb1.ByteProvider.Length; i++)
                    m.WriteByte(hb1.ByteProvider.ReadByte(i));
                m.Seek(0, SeekOrigin.Begin);

                byte[] buffer = null;
                string filter = "";

                if (a != -1)
                {
                    Bundle c = sb.bundles[n];
                    Bundle.ebxtype entry = c.ebx[a];

                    buffer = Tools.ExtractEbx(m);
                    filter = "*.xml|*.xml";
                }
                else if (b != -1)
                {
                    Bundle c = sb.bundles[n];
                    Bundle.restype entry = c.res[b];
                    string resExt = Tools.GetResType(BitConverter.ToUInt32(entry.rtype, 0));

                    if (resExt == ".mesh")
                    {
                        buffer = ExtractMesh(m, ref filter, 10.0f);
                    }
                    else if(resExt == ".itexture")
                    {
                        buffer = ExtractTexture(m);
                        filter = "*.dds|*.dds";
                    }
                    else if (resExt == ".talktable")
                    {
                        buffer = Tools.ExtractTalktable(m);
                        filter = "*.txt|*.txt";
                    }
                }

                if (buffer != null)
                {
                    /* Write file here */
                    SaveFileDialog d = new SaveFileDialog();
                    d.Filter = filter;
                    if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        WriteByteArrayToFile(buffer, d.FileName);
                        MessageBox.Show("Resource saved to " + d.FileName);
                    }
                    else
                        return;
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.SelectedIndex = -1;
            listBox3.SelectedIndex = -1;
            listBox4.SelectedIndex = -1;
            listBox5.SelectedIndex = -1;
        }

        private void contextRes_Opening(object sender, CancelEventArgs e)
        {
            int n = listBox1.SelectedIndex;
            int m = listBox4.SelectedIndex;
            if (n == -1 || m == -1) 
                return;
            Bundle.restype res = sb.bundles[n].res[m];
            nopeToolStripMenuItem.Visible = true;
            previewMeshToolStripMenuItem.Visible = false;
            uint type = BitConverter.ToUInt32(res.rtype, 0);
            switch (type)
            {
                case 0x49b156d4://.mesh
                    nopeToolStripMenuItem.Visible = false;
                    previewMeshToolStripMenuItem.Visible = true;
                    break;
            }
                
        }    

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (sb == null)
                return;
            int n = listBox1.SelectedIndex;
            int m = listBox4.SelectedIndex;
            List<uint> resTypes = new List<uint>(Tools.ResTypes.Keys);
            uint targetType = resTypes[toolStripComboBox1.SelectedIndex];
            if (n != -1 && m != -1)
            {
                Bundle b = sb.bundles[n];
                if (m < b.res.Count)
                {
                    for (int i = m + 1; i < b.res.Count; i++)
                        if (BitConverter.ToUInt32(b.res[i].rtype, 0) == targetType)
                        {
                            listBox4.SelectedIndex = i;
                            return;
                        }
                }                
            }
            n++;//next bundle or 0
            while (n < sb.bundles.Count)
            {
                Bundle b = sb.bundles[n];
                for (int i = 0; i < b.res.Count; i++)
                    if (BitConverter.ToUInt32(b.res[i].rtype, 0) == targetType)
                    {
                        listBox1.SelectedIndex = n;
                        listBox4.SelectedIndex = i;
                        return;
                    }
                n++;
            }
        }

        private PSKFile GetPreviewMesh(MemoryStream m)
        {
            DAIMesh mesh = new DAIMesh();
            mesh.Serialize(m);

            DAILODLevel lodObj = mesh.LODLevels[0];
            PSKFile previewMesh = new PSKFile();

            int n = listBox1.SelectedIndex;
            Bundle b = sb.bundles[n];

            CASFile.CASEntry e = new CASFile.CASEntry();
            if (!GetChunkData(lodObj.ChunkID, b, ref e))
                return null;

            previewMesh.points = new List<PSKFile.PSKPoint>();
            previewMesh.edges = new List<PSKFile.PSKEdge>();
            previewMesh.weights = new List<PSKFile.PSKWeight>();
            previewMesh.faces = new List<PSKFile.PSKFace>();
            previewMesh.bones = new List<PSKFile.PSKBone>();
            previewMesh.materials = new List<PSKFile.PSKMaterial>();

            BinaryReader ChunkReader = new BinaryReader(new MemoryStream(e.data));
            int offset = 0;
            int matIdx = 0;
            foreach (DAISubObject CurObj in lodObj.SubObjects)
            {
                if (CurObj.SubObjectName == "")
                    continue;

                previewMesh.materials.Add(new PSKFile.PSKMaterial("Material " + matIdx, matIdx));
                ChunkReader.BaseStream.Seek(CurObj.VertexBufferOffset, SeekOrigin.Begin);
                for (int i = 0; i < CurObj.VertexCount; i++)
                {
                    Int64 VertexStartOffset = ChunkReader.BaseStream.Position;
                    for (int vidx = 0; vidx < CurObj.VertexEntries.Count; vidx++)
                    {
                        DAIVertexEntry VertexEntry = CurObj.VertexEntries[vidx];
                        ChunkReader.BaseStream.Seek(VertexStartOffset + VertexEntry.Offset, SeekOrigin.Begin);

                        if (VertexEntry.VertexType == 0x301)
                        {
                            float X = ChunkReader.ReadSingle() * 50.0f;
                            float Y = ChunkReader.ReadSingle() * 50.0f;
                            float Z = ChunkReader.ReadSingle() * 50.0f;

                            previewMesh.points.Add(new PSKFile.PSKPoint(X, Y, Z));
                        }
                        else if (VertexEntry.VertexType == 0x701)
                        {
                            float X = HalfUtils.Unpack(ChunkReader.ReadUInt16()) * 50.0f;
                            float Y = HalfUtils.Unpack(ChunkReader.ReadUInt16()) * 50.0f;
                            float Z = HalfUtils.Unpack(ChunkReader.ReadUInt16()) * 50.0f;

                            previewMesh.points.Add(new PSKFile.PSKPoint(X, Y, Z));
                        }
                        else if (VertexEntry.VertexType == 0x621)
                        {
                            float X = HalfUtils.Unpack(ChunkReader.ReadUInt16());
                            float Y = 1.0f - HalfUtils.Unpack(ChunkReader.ReadUInt16());

                            previewMesh.edges.Add(new PSKFile.PSKEdge((ushort)(offset + i), X, Y, (byte)matIdx));
                        }
                    }

                    ChunkReader.BaseStream.Seek(VertexStartOffset + CurObj.VertexStride, SeekOrigin.Begin);
                }

                ChunkReader.BaseStream.Seek(lodObj.VertexBufferSize + (CurObj.StartIndex * 2), SeekOrigin.Begin);
                for (int i = 0; i < CurObj.TriangleCount; i++)
                {
                    int F1 = offset + ChunkReader.ReadInt16();
                    int F2 = offset + ChunkReader.ReadInt16();
                    int F3 = offset + ChunkReader.ReadInt16();

                    previewMesh.faces.Add(new PSKFile.PSKFace(F1, F2, F3, (byte)matIdx));
                }

                offset += CurObj.VertexCount;
                matIdx++;
            }

            return previewMesh;
        }
        
        private void previewMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MeshPreview mp = new MeshPreview();
            mp.MdiParent = this.MdiParent;
            mp.Show();
            mp.WindowState = FormWindowState.Maximized;

            if (hb1.ByteProvider != null && hb1.ByteProvider.Length != 0)
            {
                MemoryStream m = new MemoryStream();
                for (int i = 0; i < hb1.ByteProvider.Length; i++)
                    m.WriteByte(hb1.ByteProvider.ReadByte(i));
                m.Seek(0, SeekOrigin.Begin);

                PSKFile previewMesh = GetPreviewMesh(m);
                mp.MyObject = new Object3D(previewMesh);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sb == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.sb|*.sb";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sb.Save(d.FileName);
                MessageBox.Show("Done.");
            }
        }

        private void loadPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sb == null)
                return;
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.*|*.*";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                hb1.ByteProvider = new DynamicByteProvider(File.ReadAllBytes(d.FileName));
        }

        private void importPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sb == null || cat == null)
                return;
            string s = Interaction.InputBox("Please enter cas number to save into", "CAS Selection", "22");
            if (s == "")
                return;
            int n = -1;
            if (int.TryParse(s, out n)) 
            {
                switch (tabControl1.SelectedIndex)
                {
                    case 0://general
                        break;
                    case 1://ebx
                        ImportEbx(n);
                        break;
                    case 2://dbx
                        ImportDbx(n);
                        break;
                    case 3://res
                        ImportRes(n);
                        break;
                    case 4://chunk
                        ImportChunk(n);
                        break;
                }
            }
        }

        private void ImportEbx(int cas)
        {
            int a = listBox1.SelectedIndex;
            int n = listBox2.SelectedIndex;
            if (n == -1 || a == -1) 
                return;
            string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < hb1.ByteProvider.Length; i++)
                m.WriteByte(hb1.ByteProvider.ReadByte(i));
            byte[] data = m.ToArray();    
            Bundle.ebxtype ebx = sb.bundles[a].ebx[n];
            List<uint> line = cat.FindBySHA1(ebx.SHA1);
            if (line.Count != 9)
                return;
            FileStream fs = new FileStream(basepath + "cas_" + cas.ToString("d2") + ".cas", FileMode.Append, FileAccess.Write);
            int offset = (int)fs.Position + 0x20;
            int start = 0;
            MemoryStream compdata = new MemoryStream();
            while (start < data.Length)
            {
                int len = data.Length - start;
                if (len > 0x10000)
                    len = 0x10000;
                m = new MemoryStream();
                m.Write(data, start, len);
                byte[] cdata = Tools.CompressZlib(m.ToArray());
                Tools.WriteLEInt(compdata, len);
                int temp = 0x02700000;
                temp |= (ushort)cdata.Length;
                Tools.WriteLEInt(compdata, temp);
                compdata.Write(cdata, 0, cdata.Length);
                start += len;
            }
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] buffsha1 = sha1.ComputeHash(compdata.ToArray());
            fs.WriteByte(0xFA);
            fs.WriteByte(0xCE);
            fs.WriteByte(0x0F);
            fs.WriteByte(0xF0);
            fs.Write(buffsha1, 0, buffsha1.Length);
            Tools.WriteInt(fs, (int)compdata.Length);
            Tools.WriteInt(fs, 0);
            fs.Write(compdata.ToArray(), 0, (int)compdata.Length);
            fs.Close();
            ebx.SHA1 = buffsha1;
            Tools.Entry e = ebx.link;
            for (int i = 0; i < e.fields.Count; i++) 
            {
                Tools.Field f = e.fields[i];
                if (f.fieldname == "sha1")
                {
                    f.data = buffsha1;
                    e.fields[i] = f;
                    break;
                }
            }
            sb.Save();
            sb = new SBFile(sb.MyPath);
            fs = new FileStream(cat.MyPath, FileMode.Open, FileAccess.Write);
            fs.Seek(line[8], 0);
            foreach (byte b in buffsha1)
                fs.WriteByte(b);
            Tools.WriteInt(fs, offset);
            Tools.WriteInt(fs, (int)compdata.Length);
            Tools.WriteInt(fs, cas);
            fs.Close();
            cat = new CATFile(cat.MyPath);
            RefreshMe();
            MessageBox.Show("Done.");
        }

        private void ImportDbx(int cas)
        {
            int a = listBox1.SelectedIndex;
            int n = listBox3.SelectedIndex;
            if (n == -1 || a == -1) 
                return;
            string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < hb1.ByteProvider.Length; i++)
                m.WriteByte(hb1.ByteProvider.ReadByte(i));
            byte[] data = m.ToArray();
            Bundle.dbxtype dbx = sb.bundles[a].dbx[n];
            List<uint> line = cat.FindBySHA1(dbx.SHA1);
            if (line.Count != 9)
                return;
            FileStream fs = new FileStream(basepath + "cas_" + cas.ToString("d2") + ".cas", FileMode.Append, FileAccess.Write);
            int offset = (int)fs.Position + 0x20;
            int start = 0;
            MemoryStream compdata = new MemoryStream();
            while (start < data.Length)
            {
                int len = data.Length - start;
                if (len > 0x10000)
                    len = 0x10000;
                m = new MemoryStream();
                m.Write(data, start, len);
                byte[] cdata = Tools.CompressZlib(m.ToArray());
                Tools.WriteLEInt(compdata, len);
                int temp = 0x02700000;
                temp |= (ushort)cdata.Length;
                Tools.WriteLEInt(compdata, temp);
                compdata.Write(cdata, 0, cdata.Length);
                start += len;
            }
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] buffsha1 = sha1.ComputeHash(compdata.ToArray());
            fs.WriteByte(0xFA);
            fs.WriteByte(0xCE);
            fs.WriteByte(0x0F);
            fs.WriteByte(0xF0);
            fs.Write(buffsha1, 0, buffsha1.Length);
            Tools.WriteInt(fs, (int)compdata.Length);
            Tools.WriteInt(fs, 0);
            fs.Write(compdata.ToArray(), 0, (int)compdata.Length);
            fs.Close();
            dbx.SHA1 = buffsha1;
            Tools.Entry e = dbx.link;
            for (int i = 0; i < e.fields.Count; i++)
            {
                Tools.Field f = e.fields[i];
                if (f.fieldname == "sha1")
                {
                    f.data = buffsha1;
                    e.fields[i] = f;
                    break;
                }
            }
            sb.Save();
            sb = new SBFile(sb.MyPath);
            fs = new FileStream(cat.MyPath, FileMode.Open, FileAccess.Write);
            fs.Seek(line[8], 0);
            foreach (byte b in buffsha1)
                fs.WriteByte(b);
            Tools.WriteInt(fs, offset);
            Tools.WriteInt(fs, (int)compdata.Length);
            Tools.WriteInt(fs, cas);
            fs.Close();
            cat = new CATFile(cat.MyPath);
            RefreshMe();
            MessageBox.Show("Done.");
        }

        private void ImportRes(int cas)
        {
            int a = listBox1.SelectedIndex;
            int n = listBox4.SelectedIndex;
            if (n == -1 || a == -1)
                return;
            string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < hb1.ByteProvider.Length; i++)
                m.WriteByte(hb1.ByteProvider.ReadByte(i));
            byte[] data = m.ToArray();
            Bundle.restype res = sb.bundles[a].res[n];
            List<uint> line = cat.FindBySHA1(res.SHA1);
            if (line.Count != 9)
                return;
            FileStream fs = new FileStream(basepath + "cas_" + cas.ToString("d2") + ".cas", FileMode.Append, FileAccess.Write);
            int offset = (int)fs.Position + 0x20;
            int start = 0;
            MemoryStream compdata = new MemoryStream();
            while (start < data.Length)
            {
                int len = data.Length - start;
                if (len > 0x10000)
                    len = 0x10000;
                m = new MemoryStream();
                m.Write(data, start, len);
                byte[] cdata = Tools.CompressZlib(m.ToArray());
                Tools.WriteLEInt(compdata, len);
                int temp = 0x02700000;
                temp |= (ushort)cdata.Length;
                Tools.WriteLEInt(compdata, temp);
                compdata.Write(cdata, 0, cdata.Length);
                start += len;
            }
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] buffsha1 = sha1.ComputeHash(compdata.ToArray());
            fs.WriteByte(0xFA);
            fs.WriteByte(0xCE);
            fs.WriteByte(0x0F);
            fs.WriteByte(0xF0);
            fs.Write(buffsha1, 0, buffsha1.Length);
            Tools.WriteInt(fs, (int)compdata.Length);
            Tools.WriteInt(fs, 0);
            fs.Write(compdata.ToArray(), 0, (int)compdata.Length);
            fs.Close();
            res.SHA1 = buffsha1;
            Tools.Entry e = res.link;
            for (int i = 0; i < e.fields.Count; i++)
            {
                Tools.Field f = e.fields[i];
                if (f.fieldname == "sha1")
                {
                    f.data = buffsha1;
                    e.fields[i] = f;
                    break;
                }
            }
            sb.Save();
            sb = new SBFile(sb.MyPath);
            fs = new FileStream(cat.MyPath, FileMode.Open, FileAccess.Write);
            fs.Seek(line[8], 0);
            foreach (byte b in buffsha1)
                fs.WriteByte(b);
            Tools.WriteInt(fs, offset);
            Tools.WriteInt(fs, (int)compdata.Length);
            Tools.WriteInt(fs, cas);
            fs.Close();
            cat = new CATFile(cat.MyPath);
            RefreshMe();
            MessageBox.Show("Done.");
        }

        private void ImportChunk(int cas)
        {
            int a = listBox1.SelectedIndex;
            int n = listBox5.SelectedIndex;
            if (n == -1 || a == -1)
                return;
            string basepath = Path.GetDirectoryName(cat.MyPath) + "\\";
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < hb1.ByteProvider.Length; i++)
                m.WriteByte(hb1.ByteProvider.ReadByte(i));
            byte[] data = m.ToArray();
            Bundle.chunktype chunk = sb.bundles[a].chunk[n];
            List<uint> line = cat.FindBySHA1(chunk.SHA1);
            if (line.Count != 9)
                return;
            FileStream fs = new FileStream(basepath + "cas_" + cas.ToString("d2") + ".cas", FileMode.Append, FileAccess.Write);
            int offset = (int)fs.Position + 0x20;
            int start = 0;
            MemoryStream compdata = new MemoryStream();
            while (start < data.Length)
            {
                int len = data.Length - start;
                if (len > 0x10000)
                    len = 0x10000;
                m = new MemoryStream();
                m.Write(data, start, len);
                byte[] cdata = Tools.CompressZlib(m.ToArray());
                Tools.WriteLEInt(compdata, len);
                int temp = 0x02700000;
                temp |= (ushort)cdata.Length;
                Tools.WriteLEInt(compdata, temp);
                compdata.Write(cdata, 0, cdata.Length);
                start += len;
            }
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] buffsha1 = sha1.ComputeHash(compdata.ToArray());
            fs.WriteByte(0xFA);
            fs.WriteByte(0xCE);
            fs.WriteByte(0x0F);
            fs.WriteByte(0xF0);
            fs.Write(buffsha1, 0, buffsha1.Length);
            Tools.WriteInt(fs, (int)compdata.Length);
            Tools.WriteInt(fs, 0);
            fs.Write(compdata.ToArray(), 0, (int)compdata.Length);
            fs.Close();
            chunk.SHA1 = buffsha1;
            Tools.Entry e = chunk.link;
            for (int i = 0; i < e.fields.Count; i++)
            {
                Tools.Field f = e.fields[i];
                if (f.fieldname == "sha1")
                {
                    f.data = buffsha1;
                    e.fields[i] = f;
                    break;
                }
            }
            sb.Save();
            sb = new SBFile(sb.MyPath);
            fs = new FileStream(cat.MyPath, FileMode.Open, FileAccess.Write);
            fs.Seek(line[8], 0);
            foreach (byte b in buffsha1)
                fs.WriteByte(b);
            Tools.WriteInt(fs, offset);
            Tools.WriteInt(fs, (int)compdata.Length);
            Tools.WriteInt(fs, cas);
            fs.Close();
            cat = new CATFile(cat.MyPath);
            RefreshMe();
            MessageBox.Show("Done.");
        }

        private void exportAllEBXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog exportFolder = new FolderBrowserDialog() { };
            var returnVal = exportFolder.ShowDialog();
            if (returnVal == System.Windows.Forms.DialogResult.OK)
            {
                foreach (Bundle bundle in sb.bundles)
                {
                    foreach (var ebx in bundle.ebx)
                    {
                        CASFile.CASEntry? casEntry = GetCasEntry(ebx);
                        if (casEntry.HasValue)
                        {
                            string xmlPath = Path.Combine(exportFolder.SelectedPath, string.Format("{0}.xml", ebx.name));
                            WriteEBXXMLToFile(ebx, casEntry.Value, xmlPath);
                        }
                    }
                }
            }
        }

        #region Helper methods

        private void WriteByteArrayToFile(byte[] byteArray, string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(byteArray);
            }
        }

        private CASFile.CASEntry? GetCasEntry(Bundle.ebxtype ebx)
        {
            CASFile.CASEntry? casEntry = null;

            string basepath = Path.GetDirectoryName(cat.MyPath);
            List<uint> catLines = cat.FindBySHA1(ebx.SHA1);

            if (catLines.Count == 9)
            {
                string casFileName = string.Format("cas_{0:d2}.cas", catLines[7]);
                string casFullPath = Path.Combine(basepath, casFileName);
                if (File.Exists(casFullPath))
                {
                    cas = new CASFile(casFullPath);
                    casEntry = cas.ReadEntry(catLines.ToArray());
                }
            }

            return casEntry;
        }

        private void WriteEBXXMLToFile(Bundle.ebxtype ebx, CASFile.CASEntry casEntry, string xmlPath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(xmlPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(xmlPath));
            }

            DAIEbx EbxFile = new DAIEbx();
            using (MemoryStream casDataStream = new MemoryStream(casEntry.data))
            {
                EbxFile.Serialize(casDataStream);
            }

            XmlWriterSettings xmlSettings = new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment, Indent = true };
            using (XmlWriter xmlWriter = XmlWriter.Create(xmlPath, xmlSettings))
            {
                EbxFile.WriteToXMLWriter(xmlWriter);
            }
        }

        #endregion

    }
}
