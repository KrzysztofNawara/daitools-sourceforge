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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace DA_Tool.Bundle_Explorer
{
    public partial class MeshPreview : Form
    {
        public Device device = null;
        public Material Mat;
        public CustomVertex.PositionColored[] lines;
        public PresentParameters presentParams = new PresentParameters();
        public Object3D MyObject;
        public float CamDistance;
        public float fAngle;
        public bool init = false;
        public bool AutoRotate = true;  

        public MeshPreview()
        {
            InitializeComponent();
        }

        public void CreateDemoMesh()
        {
            string loc = Path.GetDirectoryName(Application.ExecutablePath) + "\\textures\\";
            Object3D o = new Object3D(loc + "demo.psk");
            o.LoadDefaultTextures(device);
            MyObject = o;
        }

        public void Init(PictureBox handle)
        {
            try
            {
                presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.EnableAutoDepthStencil = true;
                presentParams.AutoDepthStencilFormat = DepthFormat.D24S8;
                presentParams.SwapEffect = SwapEffect.Discard;
                device = new Device(0, DeviceType.Hardware, handle, CreateFlags.SoftwareVertexProcessing, presentParams);
                if (device == null)
                    return;
                SetupEnvironment();
                CreateCoordLines();
                CamDistance = 100;
                init = true;
            }
            catch (DirectXException ex)
            {
                string s = "Init error:"
                                + "\n\n" + ex.ToString()
                                + "\n\n" + presentParams.ToString();
                if (device != null)
                    s += "\n\n" + device.DeviceCaps.ToString();
                MessageBox.Show(s);
            }
        }

        public void SetupEnvironment()
        {
            Mat = new Material();
            Mat.Diffuse = Color.White;
            Mat.Ambient = Color.LightGray;
            device.SetTextureStageState(0, TextureStageStates.TextureCoordinateIndex, 0);
            device.SetTextureStageState(0, TextureStageStates.ColorOperation, (int)TextureOperation.Modulate);
            device.SetTextureStageState(0, TextureStageStates.ColorArgument1, (int)TextureArgument.TextureColor);
            device.SetTextureStageState(0, TextureStageStates.ColorArgument2, (int)TextureArgument.Diffuse);
            device.SetTextureStageState(0, TextureStageStates.AlphaOperation, (int)TextureOperation.Disable);
            device.SetTextureStageState(1, TextureStageStates.TextureCoordinateIndex, 0);
            device.SetTextureStageState(1, TextureStageStates.ColorOperation, (int)TextureOperation.Disable);
            device.SetTextureStageState(1, TextureStageStates.AlphaOperation, (int)TextureOperation.Disable);
        }

        public void CreateCoordLines()
        {
            lines = new CustomVertex.PositionColored[6];
            lines[1] = lines[3] = lines[5] = new CustomVertex.PositionColored(new Vector3(0, 0, 0), 0);
            lines[0] = new CustomVertex.PositionColored(new Vector3(1, 0, 0), 0);
            lines[2] = new CustomVertex.PositionColored(new Vector3(0, 1, 0), 0);
            lines[4] = new CustomVertex.PositionColored(new Vector3(0, 0, 1), 0);
        }

        public void Setup(float f)
        {
            device.RenderState.ShadeMode = ShadeMode.Gouraud;
            device.Lights[0].Type = LightType.Point;
            device.Lights[0].Diffuse = Color.White;
            device.Lights[0].Range = CamDistance * 2f;
            device.Lights[0].Position = new Vector3((float)Math.Sin(f * 2) * CamDistance, CamDistance * 0.5f, (float)Math.Cos(f * 2) * CamDistance);
            device.Lights[0].Attenuation0 = 0.01f;
            device.Lights[0].Attenuation1 = 0.0125f;
            device.Lights[0].Enabled = true;
            device.Material = Mat;
            device.RenderState.CullMode = Cull.None;
            device.RenderState.ZBufferEnable = true;
            device.RenderState.Ambient = Color.LightGray;
            device.SamplerState[0].MinFilter = TextureFilter.Anisotropic;
            device.SamplerState[0].MagFilter = TextureFilter.Anisotropic;
            device.RenderState.ZBufferFunction = Compare.LessEqual;
            device.Clear(ClearFlags.Target, System.Drawing.Color.White, 1.0f, 0);
            device.Clear(ClearFlags.ZBuffer, System.Drawing.Color.Black, 1.0f, 0);
            device.Transform.World = Matrix.RotationY(fAngle * 0.2f);
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, CamDistance * 0.5f, CamDistance), new Vector3(0, 0, 0), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 2, 1.0f, 1.0f, 100000.0f);
            device.Material = Mat;
        }

        public void RenderCoordsystem(Device device)
        {
            device.RenderState.FillMode = FillMode.WireFrame;
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.RenderState.Ambient = Color.Black;
            device.DrawUserPrimitives(PrimitiveType.LineList, 3, lines);
            device.RenderState.Ambient = Color.LightGray;
        }

        public void Render()
        {
            if (device == null)
                return;
            try
            {
                if (AutoRotate)
                    fAngle += 0.1f;
                Setup(-fAngle);
                device.BeginScene();
                RenderCoordsystem(device);
                if (MyObject != null)
                    MyObject.Render(device);
                device.EndScene();
                device.Present();
            }
            catch (DirectXException)
            {
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (device != null)
                Render();
        }

        private void MeshPreview_Activated(object sender, EventArgs e)
        {
            if (!init)
            {
                Init(pic1);
                if (device != null)
                {
                    CreateDemoMesh();
                    timer1.Enabled = true;
                }
            }
        }
    }

    public class Object3D
    {
        public struct Face
        {
            public int e0;
            public int e1;
            public int e2;
            public int material;

            public Face(int _e0, int _e1, int _e2, int mat)
            {
                e0 = _e0;
                e1 = _e1;
                e2 = _e2;
                material = mat;
            }
        }
        public struct Vertex
        {
            public Vector3 position;
            public Vector2 UV;
            public List<Vector2> Influences;

            public Vertex(Vector3 p, Vector2 uv, List<Vector2> inf)
            {
                position = p;
                UV = uv;
                Influences = inf;
            }
        }
        public struct Bone
        {
            public string name;
            public Vector3 position;
            public Vector4 rotation;
            public int childs;
            public int parent;
            public float length;
            public Vector3 size;
            public Matrix mat;
        }
        public struct DXSection
        {
            public CustomVertex.PositionColored[] Simple;
            public CustomVertex.PositionNormalTextured[] Textured;
            public CustomVertex.PositionColored[] Normals;
        }
        public struct Section
        {
            public List<Face> Faces;
            public List<Vertex> Vertices;
            public DXSection DXData;
        }
        public struct BoundingBox
        {
            public Vector3 min;
            public Vector3 max;
            public Vector3 center;
            public float r;
            public Matrix MyMatrix;
        }
        public struct Skeleton
        {
            public List<Bone> Bones;
            public CustomVertex.PositionColored[] BoneMesh;
            public int[] BoneMeshIDs;
        }
        public struct SelectedFaceType
        {
            public int Section;
            public int Face;

            public SelectedFaceType(int Sec, int F)
            {
                Section = Sec;
                Face = F;
            }
        }
        public enum DrawStyle
        {
            Wireframe = 0,
            Solid = 1,
            Textured = 2
        }

        public BoundingBox Bounds;
        public List<Section> Data;
        public Skeleton RefSkel;
        public Texture DefaultTexture;
        public Texture SelectTexture;
        public Texture[] SectionTextures;
        public DrawStyle Style;
        public SelectedFaceType SelectedFace = new SelectedFaceType(-1, -1);

        private int _SelectedSection = -1;
        public int SelectedSection
        {
            get { return _SelectedSection; }
            set { _SelectedSection = value; RefreshSectionSelection(); }
        }
        private int _SelectedBone = -1;
        public int SelectedBone
        {
            get { return _SelectedBone; }
            set
            {
                _SelectedBone = value;
                RegenerateMeshes();
                if (value >= 0 && value <= RefSkel.Bones.Count)
                {
                    for (int i = 0; i < RefSkel.BoneMesh.Length; i++)
                        if (RefSkel.BoneMeshIDs[i] == value)
                            RefSkel.BoneMesh[i].Color = Color.Orange.ToArgb();
                }
            }
        }

        public bool DrawNormals = false;
        public bool DrawBones = true;

        public Object3D(string filename)
        {
            string Extension = Path.GetExtension(filename).ToLower();
            switch (Extension)
            {
                case ".psk":
                    LoadPSK(filename);
                    break;
                default:
                    MessageBox.Show("Filetype *." + Extension + " is not supported!");
                    return;
            }
            Style = DrawStyle.Wireframe;
            SectionTextures = new Texture[Data.Count];
        }

        public Object3D(PSKFile PSK)
        {
            LoadPSK(PSK);
            Style = DrawStyle.Wireframe;
            SectionTextures = new Texture[Data.Count];
        }

        public void LoadDefaultTextures(Device device)
        {
            string loc = "";
            try
            {
                loc = Path.GetDirectoryName(Application.ExecutablePath) + "\\textures\\";
                DefaultTexture = TextureLoader.FromFile(device, loc + "Default.bmp");
                SelectTexture = TextureLoader.FromFile(device, loc + "Select.bmp");
            }
            catch (DirectXException ex)
            {
                string s = "Texture load error:" + "\n\n" + ex.ToString();
                if (device != null)
                {
                    s += "\n\n" + device.PresentationParameters.ToString();
                    s += "\n\n" + device.DeviceCaps.ToString();
                }
                MessageBox.Show(s);
            }
        }

        public void LoadSectionTexture(Device device, string filename, int section)
        {
            try
            {
                SectionTextures[section] = TextureLoader.FromFile(device, filename);
            }
            catch (Direct3DXException ex)
            {
                string s = "Texture load error:"
                    + "\n\nPath = " + filename
                    + "\n\n" + ex.ToString();
                if (device != null)
                {
                    s += "\n\n" + device.PresentationParameters.ToString();
                    s += "\n\n" + device.DeviceCaps.ToString();
                }
                MessageBox.Show(s);
            }
        }

        public void LoadPSK(string filename)
        {
            Data = new List<Section>();
            PSKFile PSK = new PSKFile(filename);
            RefSkel = new Skeleton();
            RefSkel.Bones = new List<Bone>();
            foreach (PSKFile.PSKBone b in PSK.bones)
            {
                Bone nb = new Bone();
                nb.childs = b.childs;
                nb.parent = b.parent;
                nb.position = b.location.ToVector3();
                nb.rotation = b.rotation.ToVector4();
                nb.name = b.name;
                nb.length = b.length;
                nb.size = b.size.ToVector3();
                nb.mat = Matrix.Identity;
                RefSkel.Bones.Add(nb);
            }
            for (int i = 0; i < PSK.materials.Count; i++)
            {
                Section sec = new Section();
                sec.DXData = new DXSection();
                sec.Faces = new List<Face>();
                sec.Vertices = new List<Vertex>();
                foreach (PSKFile.PSKPoint p in PSK.points)
                    sec.Vertices.Add(new Vertex(p.ToVector3(), new Vector2(0, 0), new List<Vector2>()));
                foreach (PSKFile.PSKWeight w in PSK.weights)
                    sec.Vertices[w.point].Influences.Add(new Vector2(w.weight, w.bone));
                foreach (PSKFile.PSKEdge e in PSK.edges)
                {
                    Vertex v = sec.Vertices[e.index];
                    v.UV = new Vector2(e.U, e.V);
                    sec.Vertices[e.index] = v;
                }
                foreach (PSKFile.PSKFace f in PSK.faces)
                    if (f.material == i)
                        sec.Faces.Add(new Face(PSK.edges[f.v0].index, PSK.edges[f.v1].index, PSK.edges[f.v2].index, f.material));
                if (sec.Faces.Count != 0)
                    Data.Add(sec);
                RegenerateMeshes();
            }
        }

        public void LoadPSK(PSKFile PSK)
        {
            Data = new List<Section>();
            RefSkel = new Skeleton();
            RefSkel.Bones = new List<Bone>();
            foreach (PSKFile.PSKBone b in PSK.bones)
            {
                Bone nb = new Bone();
                nb.childs = b.childs;
                nb.parent = b.parent;
                nb.position = b.location.ToVector3();
                nb.rotation = b.rotation.ToVector4();
                nb.name = b.name;
                nb.length = b.length;
                nb.size = b.size.ToVector3();
                nb.mat = Matrix.Identity;
                RefSkel.Bones.Add(nb);
            }
            for (int i = 0; i < PSK.materials.Count; i++)
            {
                Section sec = new Section();
                sec.DXData = new DXSection();
                sec.Faces = new List<Face>();
                sec.Vertices = new List<Vertex>();
                foreach (PSKFile.PSKPoint p in PSK.points)
                    sec.Vertices.Add(new Vertex(p.ToVector3(), new Vector2(0, 0), new List<Vector2>()));
                foreach (PSKFile.PSKWeight w in PSK.weights)
                    sec.Vertices[w.point].Influences.Add(new Vector2(w.weight, w.bone));
                foreach (PSKFile.PSKEdge e in PSK.edges)
                {
                    Vertex v = sec.Vertices[e.index];
                    v.UV = new Vector2(e.U, e.V);
                    sec.Vertices[e.index] = v;
                }
                foreach (PSKFile.PSKFace f in PSK.faces)
                    if (f.material == i)
                        sec.Faces.Add(new Face(PSK.edges[f.v0].index, PSK.edges[f.v1].index, PSK.edges[f.v2].index, f.material));
                if (sec.Faces.Count != 0)
                    Data.Add(sec);
                RegenerateMeshes();
            }
        }

        public void Save(string filename)
        {
            if (Data == null || Data.Count == 0)
                return;
            string Extension = Path.GetExtension(filename).ToLower();
            switch (Extension)
            {
                case ".psk":
                    SavePSK(filename);
                    break;
                default:
                    MessageBox.Show("Filetype *." + Extension + " is not supported!");
                    return;
            }
        }

        public void SavePSK(string filename)
        {
            PSKFile PSK = new PSKFile();
            PSK.materials = new List<PSKFile.PSKMaterial>();
            PSK.points = new List<PSKFile.PSKPoint>();
            PSK.edges = new List<PSKFile.PSKEdge>();
            PSK.faces = new List<PSKFile.PSKFace>();
            PSK.bones = new List<PSKFile.PSKBone>();
            PSK.weights = new List<PSKFile.PSKWeight>();
            Section sec = Data[0];
            for (int i = 0; i < sec.Vertices.Count; i++)
            {
                Vertex v = sec.Vertices[i];
                PSK.points.Add(new PSKFile.PSKPoint(v.position));
                PSK.edges.Add(new PSKFile.PSKEdge((ushort)i, new Vector2(0, 0), 0));
                foreach (Vector2 inf in v.Influences)
                    PSK.weights.Add(new PSKFile.PSKWeight(inf.X, i, (int)inf.Y));
            }
            for (int i = 0; i < Data.Count; i++)
            {
                sec = Data[i];
                PSK.materials.Add(new PSKFile.PSKMaterial("Material" + i.ToString("d2"), i));
                for (int j = 0; j < sec.Faces.Count; j++)
                {
                    Face f = sec.Faces[j];
                    PSKFile.PSKEdge e = PSK.edges[f.e0];
                    e.U = sec.Vertices[f.e0].UV.X;
                    e.V = sec.Vertices[f.e0].UV.Y;
                    e.material = (byte)i;
                    PSK.edges[f.e0] = e;
                    e = PSK.edges[f.e1];
                    e.U = sec.Vertices[f.e1].UV.X;
                    e.V = sec.Vertices[f.e1].UV.Y;
                    e.material = (byte)i;
                    PSK.edges[f.e1] = e;
                    e = PSK.edges[f.e2];
                    e.U = sec.Vertices[f.e2].UV.X;
                    e.V = sec.Vertices[f.e2].UV.Y;
                    e.material = (byte)i;
                    PSK.edges[f.e2] = e;
                    PSK.faces.Add(new PSKFile.PSKFace(f.e0, f.e1, f.e2, (byte)i));
                }
            }
            for (int i = 0; i < RefSkel.Bones.Count; i++)
            {
                Bone b = RefSkel.Bones[i];
                PSKFile.PSKBone pb = new PSKFile.PSKBone();
                pb.name = b.name;
                pb.childs = b.childs;
                pb.parent = b.parent;
                pb.location = new PSKFile.PSKPoint(b.position);
                pb.rotation = new PSKFile.PSKQuad(b.rotation);
                PSK.bones.Add(pb);
            }
            PSK.Save(filename);
        }

        public void RegenerateMeshes()
        {
            Bounds = new BoundingBox();
            Bounds.min = Bounds.max = Data[0].Vertices[Data[0].Faces[0].e0].position;
            for (int i = 0; i < Data.Count; i++)
            {
                Section sec = Data[i];
                sec.DXData.Simple = new CustomVertex.PositionColored[sec.Faces.Count * 3];
                sec.DXData.Textured = new CustomVertex.PositionNormalTextured[sec.Faces.Count * 3];
                sec.DXData.Normals = new CustomVertex.PositionColored[sec.Faces.Count * 2];
                float one3rd = 1f / 3f;
                for (int j = 0; j < sec.Faces.Count; j++)
                {
                    Vector3 v0 = sec.Vertices[sec.Faces[j].e0].position;
                    Vector3 v1 = sec.Vertices[sec.Faces[j].e1].position;
                    Vector3 v2 = sec.Vertices[sec.Faces[j].e2].position;
                    Vector2 uv0 = sec.Vertices[sec.Faces[j].e0].UV;
                    Vector2 uv1 = sec.Vertices[sec.Faces[j].e1].UV;
                    Vector2 uv2 = sec.Vertices[sec.Faces[j].e2].UV;
                    Vector3 e1 = v1 - v0;
                    Vector3 e2 = v2 - v0;
                    e1.Normalize(); e2.Normalize();
                    Vector3 n = Vector3.Cross(e1, e2);
                    n.Normalize();
                    sec.DXData.Simple[j * 3] = new CustomVertex.PositionColored(v0, 0);
                    sec.DXData.Simple[j * 3 + 1] = new CustomVertex.PositionColored(v1, 0);
                    sec.DXData.Simple[j * 3 + 2] = new CustomVertex.PositionColored(v2, 0);
                    sec.DXData.Textured[j * 3] = new CustomVertex.PositionNormalTextured(v0, n, uv0.X, uv0.Y);
                    sec.DXData.Textured[j * 3 + 1] = new CustomVertex.PositionNormalTextured(v1, n, uv1.X, uv1.Y);
                    sec.DXData.Textured[j * 3 + 2] = new CustomVertex.PositionNormalTextured(v2, n, uv2.X, uv2.Y);
                    Vector3 o = (v0 + v1 + v2) * one3rd;
                    sec.DXData.Normals[j * 2] = new CustomVertex.PositionColored(o, Color.Red.ToArgb());
                    sec.DXData.Normals[j * 2 + 1] = new CustomVertex.PositionColored(o + n, Color.Red.ToArgb());
                }
                for (int j = 0; j < sec.DXData.Simple.Length; j++)
                {
                    Vector3 v = sec.DXData.Simple[j].Position;
                    Bounds.min.X = Math.Min(v.X, Bounds.min.X);
                    Bounds.min.Y = Math.Min(v.Y, Bounds.min.Y);
                    Bounds.min.Z = Math.Min(v.Z, Bounds.min.Z);
                    Bounds.max.X = Math.Max(v.X, Bounds.max.X);
                    Bounds.max.Y = Math.Max(v.Y, Bounds.max.Y);
                    Bounds.max.Z = Math.Max(v.Z, Bounds.max.Z);
                }
                Data[i] = sec;
            }
            Bounds.center = (Bounds.min + Bounds.max) * 0.5f;
            Bounds.r = (Bounds.min - Bounds.max).Length() * 0.5f;
            Bounds.MyMatrix = Matrix.Translation(-Bounds.center);
            CreateBoneMesh();
        }

        public void CreateBoneMesh()
        {
            List<CustomVertex.PositionColored> _bones = new List<CustomVertex.PositionColored>();
            List<int> BoneIDs = new List<int>();
            for (int i = 0; i < RefSkel.Bones.Count; i++)
            {
                Bone b = RefSkel.Bones[i];
                Vector4 rot = b.rotation;
                Quaternion q = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
                if (i == 0)
                    q = Quaternion.Conjugate(q);
                q.Normalize();
                Matrix mat = Matrix.RotationQuaternion(q);
                mat.M41 = b.position.X;
                mat.M42 = b.position.Y;
                mat.M43 = b.position.Z;
                if (i == 0)
                    b.mat = mat;
                else
                    b.mat = mat * RefSkel.Bones[b.parent].mat;
                RefSkel.Bones[i] = b;
            }
            for (int i = 1; i < RefSkel.Bones.Count; i++)
                for (int j = 0; j < RefSkel.Bones.Count; j++)
                    if (i != j && RefSkel.Bones[j].parent == i)
                    {
                        _bones.Add(new CustomVertex.PositionColored(Vector3.TransformCoordinate(RefSkel.Bones[i].position, RefSkel.Bones[i].mat), Color.Blue.ToArgb()));
                        _bones.Add(new CustomVertex.PositionColored(Vector3.TransformCoordinate(RefSkel.Bones[j].position, RefSkel.Bones[j].mat), Color.Blue.ToArgb()));
                        BoneIDs.Add(i);
                        BoneIDs.Add(j);
                    }
            RefSkel.BoneMesh = _bones.ToArray();
            RefSkel.BoneMeshIDs = BoneIDs.ToArray();
            if (SelectedBone != -1)
            {
                for (int i = 0; i < Data.Count; i++)
                {
                    Section sec = Data[i];
                    for (int j = 0; j < sec.Faces.Count; j++)
                    {
                        foreach (Vector2 inf in sec.Vertices[sec.Faces[j].e0].Influences)
                            if (inf.Y == SelectedBone)
                                sec.DXData.Simple[j * 3].Color = ((int)(inf.X * 255f)) << 16;
                        foreach (Vector2 inf in sec.Vertices[sec.Faces[j].e1].Influences)
                            if (inf.Y == SelectedBone)
                                sec.DXData.Simple[j * 3 + 1].Color = ((int)(inf.X * 255f)) << 16;
                        foreach (Vector2 inf in sec.Vertices[sec.Faces[j].e2].Influences)
                            if (inf.Y == SelectedBone)
                                sec.DXData.Simple[j * 3 + 2].Color = ((int)(inf.X * 255f)) << 16;
                    }
                    Data[i] = sec;
                }
            }
        }

        public void Render(Device device)
        {
            if (DefaultTexture == null || SelectTexture == null)
                LoadDefaultTextures(device);
            Matrix temp = device.Transform.World;
            device.Transform.World = Bounds.MyMatrix * device.Transform.World;
            int count = 0;
            switch (Style)
            {
                case DrawStyle.Solid:
                    device.RenderState.Lighting = false;
                    device.RenderState.FillMode = FillMode.Solid;
                    device.SetTexture(0, DefaultTexture);
                    device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
                    count = 0;
                    foreach (Section sec in Data)
                    {
                        if (count++ != SelectedSection)
                            device.SetTexture(0, DefaultTexture);
                        else
                            device.SetTexture(0, SelectTexture);
                        device.DrawUserPrimitives(PrimitiveType.TriangleList, sec.DXData.Textured.Length / 3, sec.DXData.Textured);
                    }
                    if (SelectedFace.Section != -1)
                    {
                        CustomVertex.PositionNormalTextured[] list = new CustomVertex.PositionNormalTextured[3];
                        list[0] = Data[SelectedFace.Section].DXData.Textured[SelectedFace.Face * 3];
                        list[1] = Data[SelectedFace.Section].DXData.Textured[SelectedFace.Face * 3 + 1];
                        list[2] = Data[SelectedFace.Section].DXData.Textured[SelectedFace.Face * 3 + 2];
                        device.SetTexture(0, SelectTexture);
                        device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, list);
                    }
                    break;
                case DrawStyle.Textured:
                    device.RenderState.Lighting = true;
                    device.RenderState.FillMode = FillMode.Solid;
                    device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
                    count = 0;
                    foreach (Section sec in Data)
                    {
                        if (count != SelectedSection)
                        {
                            if (SectionTextures[count] != null)
                                device.SetTexture(0, SectionTextures[count]);
                            else
                                device.SetTexture(0, DefaultTexture);
                        }
                        else
                            device.SetTexture(0, SelectTexture);
                        device.DrawUserPrimitives(PrimitiveType.TriangleList, sec.DXData.Textured.Length / 3, sec.DXData.Textured);
                        if (SelectedFace.Section != -1)
                        {
                            CustomVertex.PositionNormalTextured[] list = new CustomVertex.PositionNormalTextured[3];
                            list[0] = Data[SelectedFace.Section].DXData.Textured[SelectedFace.Face * 3];
                            list[1] = Data[SelectedFace.Section].DXData.Textured[SelectedFace.Face * 3 + 1];
                            list[2] = Data[SelectedFace.Section].DXData.Textured[SelectedFace.Face * 3 + 2];
                            device.SetTexture(0, SelectTexture);
                            device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, list);
                        }
                        count++;
                    }
                    break;
                default:
                    break;
            }
            device.RenderState.Lighting = false;
            device.SetTexture(0, null);
            device.RenderState.FillMode = FillMode.WireFrame;
            device.VertexFormat = CustomVertex.PositionColored.Format;
            if (Style != DrawStyle.Textured)
                foreach (Section sec in Data)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, sec.DXData.Simple.Length / 3, sec.DXData.Simple);
            if (SelectedFace.Section != -1)
            {
                CustomVertex.PositionColored[] list = new CustomVertex.PositionColored[3];
                list[0] = Data[SelectedFace.Section].DXData.Simple[SelectedFace.Face * 3];
                list[1] = Data[SelectedFace.Section].DXData.Simple[SelectedFace.Face * 3 + 1];
                list[2] = Data[SelectedFace.Section].DXData.Simple[SelectedFace.Face * 3 + 2];
                list[0].Color = list[1].Color = list[2].Color = Color.Orange.ToArgb();
                device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, list);
            }
            if (DrawNormals)
                foreach (Section sec in Data)
                    device.DrawUserPrimitives(PrimitiveType.LineList, sec.DXData.Normals.Length / 2, sec.DXData.Normals);
            if (DrawBones && RefSkel.BoneMesh.Length != 0)
                device.DrawUserPrimitives(PrimitiveType.LineList, RefSkel.BoneMesh.Length / 2, RefSkel.BoneMesh);
            device.Transform.World = temp;
        }

        public void RefreshSectionSelection()
        {
            for (int i = 0; i < Data.Count; i++)
                for (int j = 0; j < Data[i].DXData.Simple.Length; j++)
                    if (i == SelectedSection)
                        Data[i].DXData.Simple[j].Color = Color.Orange.ToArgb();
                    else
                        Data[i].DXData.Simple[j].Color = 0;
        }

        public TreeNode ToTree()
        {
            TreeNode res = new TreeNode("3D Object");
            TreeNode t = new TreeNode("Materials");
            for (int i = 0; i < Data.Count; i++)
                t.Nodes.Add(new TreeNode(i.ToString()));
            res.Nodes.Add(t);
            TreeNode t3 = new TreeNode("Sections");
            for (int i = 0; i < Data.Count; i++)
                t3.Nodes.Add(SectionToTree(Data[i], i));
            res.Nodes.Add(t3);
            res.Nodes.Add(BonesToTree());
            res.Expand();
            return res;
        }

        public TreeNode BonesToTree()
        {
            TreeNode res = new TreeNode("Bones");
            int count = 0;
            foreach (Bone b in RefSkel.Bones)
            {
                res.Nodes.Add(new TreeNode(count
                                          + " : \""
                                          + b.name
                                          + "\" Location : ("
                                          + b.position.X + "; " + b.position.Y + "; " + b.position.Z
                                          + ") Rotation : ("
                                          + b.rotation.X + "; " + b.rotation.Y + "; " + b.rotation.Z
                                          + ") Childs : " + b.childs
                                          + " Parent : " + b.parent
                                          + " Length : " + b.length
                                          + " Size : ("
                                          + b.size.X + "; " + b.size.Y + "; " + b.size.Z + ")"
                                          ));
                count++;
            }
            return res;
        }

        public TreeNode SectionToTree(Section sec, int Index)
        {
            TreeNode res = new TreeNode("Section " + Index);
            TreeNode t = new TreeNode("Vertices");
            int count = 0;
            foreach (Vertex v in sec.Vertices)
                t.Nodes.Add(new TreeNode((count++).ToString("D8") + " : Position ("
                                         + v.position.X + "; "
                                         + v.position.Y + "; "
                                         + v.position.Z + ") UV("
                                         + v.UV.X + "; " + v.UV.Y + ") "
                                         + InfluencesToString(v.Influences)));
            TreeNode t2 = new TreeNode("Faces");
            count = 0;
            foreach (Face f in sec.Faces)
                t2.Nodes.Add(new TreeNode((count++).ToString("D8") + " : V0=" + f.e0 + " : V1=" + f.e1 + " : V2=" + f.e2 + " : Material=" + f.material));
            res.Nodes.Add(t);
            res.Nodes.Add(t2);
            return res;
        }

        public string InfluencesToString(List<Vector2> inf)
        {
            string s = "Influences : (";
            foreach (Vector2 v in inf)
                s += "[" + v.X.ToString(".000") + "; " + ((int)v.Y).ToString() + "] ";
            return s;
        }

        public void Process3DClick(Vector3 org, Vector3 dir, Device device)
        {
            List<float> Distances = new List<float>();
            List<SelectedFaceType> SelFaces = new List<SelectedFaceType>();
            Matrix transform = Bounds.MyMatrix;
            float distance = 0;
            for (int i = 0; i < Data.Count; i++)
                for (int j = 0; j < Data[i].DXData.Simple.Length / 3; j++)
                {
                    DXSection d = Data[i].DXData;
                    Vector3 v0 = Vector3.TransformCoordinate(d.Simple[j * 3].Position, transform);
                    Vector3 v1 = Vector3.TransformCoordinate(d.Simple[j * 3 + 1].Position, transform);
                    Vector3 v2 = Vector3.TransformCoordinate(d.Simple[j * 3 + 2].Position, transform);
                    if (RayIntersectTriangle(org, dir, v0, v1, v2, out distance))
                    {
                        Distances.Add(distance);
                        SelFaces.Add(new SelectedFaceType(i, j));
                    }
                }
            int faceindex = -1;
            distance = float.MaxValue;
            for (int i = 0; i < Distances.Count; i++)
                if (Distances[i] < distance)
                {
                    distance = Distances[i];
                    faceindex = i;
                }
            if (faceindex != -1)
                SelectedFace = new SelectedFaceType(SelFaces[faceindex].Section, SelFaces[faceindex].Face);
            else
                SelectedFace = new SelectedFaceType(-1, -1);
        }

        public bool RayIntersectTriangle(Vector3 rayPosition, Vector3 rayDirection, Vector3 tri0, Vector3 tri1, Vector3 tri2, out float pickDistance)
        {
            pickDistance = -1f;
            Vector3 edge1 = tri1 - tri0;
            Vector3 edge2 = tri2 - tri0;
            Vector3 pvec = Vector3.Cross(rayDirection, edge2);
            float det = Vector3.Dot(edge1, pvec);
            if (det < 0.0001f)
                return false;
            Vector3 tvec = rayPosition - tri0;
            float barycentricU = Vector3.Dot(tvec, pvec);
            if (barycentricU < 0.0f || barycentricU > det)
                return false;
            Vector3 qvec = Vector3.Cross(tvec, edge1);
            float barycentricV = Vector3.Dot(rayDirection, qvec);
            if (barycentricV < 0.0f || barycentricU + barycentricV > det)
                return false;
            pickDistance = Vector3.Dot(edge2, qvec);
            float fInvDet = 1.0f / det;
            pickDistance *= fInvDet;
            return true;
        }
    }

    public class PSKFile
    {
        public struct PSKPoint
        {
            public float x;
            public float y;
            public float z;

            public PSKPoint(float _x, float _y, float _z)
            {
                x = _x;
                y = _y;
                z = _z;
            }

            public PSKPoint(Vector3 v)
            {
                x = v.X;
                y = v.Y;
                z = v.Z;
            }

            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }
        public struct PSKQuad
        {
            public float w;
            public float x;
            public float y;
            public float z;

            public PSKQuad(float _w, float _x, float _y, float _z)
            {
                w = _w;
                x = _x;
                y = _y;
                z = _z;
            }

            public PSKQuad(Vector4 v)
            {
                w = v.W;
                x = v.X;
                y = v.Y;
                z = v.Z;
            }

            public Vector4 ToVector4()
            {
                return new Vector4(x, y, z, w);
            }
        }
        public struct PSKEdge
        {
            public UInt16 index;
            public UInt16 padding1;
            public float U;
            public float V;
            public byte material;
            public byte reserved;
            public UInt16 padding2;

            public PSKEdge(UInt16 _index, float _U, float _V, byte _material)
            {
                index = _index;
                padding1 = 0;
                U = _U;
                V = _V;
                material = _material;
                reserved = 0;
                padding2 = 0;
            }

            public PSKEdge(UInt16 _index, Vector2 _UV, byte _material)
            {
                index = _index;
                padding1 = 0;
                U = _UV.X;
                V = _UV.Y;
                material = _material;
                reserved = 0;
                padding2 = 0;
            }
        }
        public struct PSKFace
        {
            public int v0;
            public int v1;
            public int v2;
            public byte material;
            public byte auxmaterial;
            public int smoothgroup;

            public PSKFace(int _v0, int _v1, int _v2, byte _material)
            {
                v0 = _v0;
                v1 = _v1;
                v2 = _v2;
                material = _material;
                auxmaterial = 0;
                smoothgroup = 0;
            }
        }
        public struct PSKMaterial
        {
            public string name;
            public int texture;
            public int polyflags;
            public int auxmaterial;
            public int auxflags;
            public int LODbias;
            public int LODstyle;

            public PSKMaterial(string _name, int _texture)
            {
                name = _name;
                texture = _texture;
                polyflags = 0;
                auxmaterial = 0;
                auxflags = 0;
                LODbias = 0;
                LODstyle = 0;
            }
        }
        public struct PSKBone
        {
            public string name;
            public int flags;
            public int childs;
            public int parent;
            public PSKQuad rotation;
            public PSKPoint location;
            public float length;
            public PSKPoint size;
            public int index;
        }
        public struct PSKWeight
        {
            public float weight;
            public int point;
            public int bone;

            public PSKWeight(float _weight, int _point, int _bone)
            {
                weight = _weight;
                point = _point;
                bone = _bone;
            }
        }
        public struct PSKExtraUV
        {
            public float U;
            public float V;

            public PSKExtraUV(float _U, float _V)
            {
                U = _U;
                V = _V;
            }
        }
        public struct PSKHeader
        {
            public string name;
            public int flags;
            public int size;
            public int count;
        }

        public List<PSKPoint> points;
        public List<PSKEdge> edges;
        public List<PSKFace> faces;
        public List<PSKMaterial> materials;
        public List<PSKBone> bones;
        public List<PSKWeight> weights;

        public PSKFile()
        {
        }

        public PSKFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            while (fs.Position < fs.Length)
            {
                PSKHeader h = ReadHeader(fs);
                switch (h.name)
                {
                    case "PNTS0000":
                        {
                            points = new List<PSKPoint>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKPoint pskPoint = new PSKPoint();
                                pskPoint.x = ReadFloat(fs);
                                pskPoint.z = ReadFloat(fs);
                                pskPoint.y = ReadFloat(fs);
                                points.Add(pskPoint);
                            }
                        }; break;
                    case "VTXW0000":
                        {
                            edges = new List<PSKEdge>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKEdge pskEdge = new PSKEdge();
                                pskEdge.index = ReadUInt16(fs);
                                ReadUInt16(fs);
                                pskEdge.U = ReadFloat(fs);
                                pskEdge.V = ReadFloat(fs);
                                pskEdge.material = (byte)fs.ReadByte();
                                fs.ReadByte();
                                ReadUInt16(fs);
                                edges.Add(pskEdge);
                            }
                        }; break;
                    case "FACE0000":
                        {
                            faces = new List<PSKFace>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKFace pskFace = new PSKFace(ReadUInt16(fs), ReadUInt16(fs), ReadUInt16(fs), (byte)fs.ReadByte());
                                fs.ReadByte();
                                ReadInt32(fs);
                                faces.Add(pskFace);
                            }
                        }; break;

                    case "MATT0000":
                        {
                            materials = new List<PSKMaterial>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKMaterial pskMaterial = new PSKMaterial();
                                pskMaterial.name = ReadFixedString(fs, 64);
                                pskMaterial.texture = ReadInt32(fs);
                                ReadInt32(fs);
                                ReadInt32(fs);
                                ReadInt32(fs);
                                ReadInt32(fs);
                                ReadInt32(fs);
                                materials.Add(pskMaterial);
                            }
                        }; break;
                    case "REFSKELT":
                        {
                            bones = new List<PSKBone>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKBone b = new PSKBone();
                                b.name = ReadFixedString(fs, 64);
                                ReadInt32(fs);
                                b.childs = ReadInt32(fs);
                                b.parent = ReadInt32(fs);
                                b.rotation.x = ReadFloat(fs);
                                b.rotation.z = ReadFloat(fs);
                                b.rotation.y = ReadFloat(fs);
                                b.rotation.w = ReadFloat(fs);
                                b.location.x = ReadFloat(fs);
                                b.location.z = ReadFloat(fs);
                                b.location.y = ReadFloat(fs);
                                ReadInt32(fs);
                                ReadInt32(fs);
                                ReadInt32(fs);
                                ReadInt32(fs);
                                bones.Add(b);
                            }
                        }; break;
                    case "RAWWEIGHTS":
                        {
                            weights = new List<PSKWeight>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKWeight w = new PSKWeight(ReadFloat(fs), ReadInt32(fs), ReadInt32(fs));
                                weights.Add(w);
                            }
                        }; break;
                    default:
                        fs.Seek(h.size * h.count, SeekOrigin.Current);
                        break;
                }
            }
            fs.Close();
        }

        public MemoryStream SaveToMemory()
        {
            MemoryStream ms = new MemoryStream();
            Save(ms);

            return ms;
        }

        public void Save(String filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            Save(fs);
            fs.Close();
        }

        public void Save(Stream fs)
        {
            //FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            WriteHeader(fs, "ACTRHEAD", 0x1E83B9, 0, 0);
            WriteHeader(fs, "PNTS0000", 0x1E83B9, 0xC, points.Count);
            foreach (PSKPoint p in points)
            {
                WriteFloat(fs, p.x);
                WriteFloat(fs, p.z);
                WriteFloat(fs, p.y);
            }
            WriteHeader(fs, "VTXW0000", 0x1E83B9, 0x10, edges.Count);
            foreach (PSKEdge e in edges)
            {
                WriteUInt16(fs, e.index);
                WriteUInt16(fs, 0);
                WriteFloat(fs, e.U);
                WriteFloat(fs, e.V);
                fs.WriteByte(e.material);
                fs.WriteByte(0);
                WriteUInt16(fs, 0);
            }
            WriteHeader(fs, "FACE0000", 0x1E83B9, 0xC, faces.Count);
            foreach (PSKFace f in faces)
            {
                WriteUInt16(fs, (ushort)f.v0);
                WriteUInt16(fs, (ushort)f.v1);
                WriteUInt16(fs, (ushort)f.v2);
                fs.WriteByte(f.material);
                fs.WriteByte(0);
                WriteInt32(fs, 0);
            }
            WriteHeader(fs, "MATT0000", 0x1E83B9, 0x58, materials.Count);
            foreach (PSKMaterial m in materials)
            {
                WriteFixedString(fs, m.name, 64);
                WriteInt32(fs, m.texture);
                WriteInt32(fs, 0);
                WriteInt32(fs, 0);
                WriteInt32(fs, 0);
                WriteInt32(fs, 0);
                WriteInt32(fs, 0);
            }
            WriteHeader(fs, "REFSKELT", 0x1E83B9, 0x78, bones.Count);
            foreach (PSKBone b in bones)
            {
                WriteFixedString(fs, b.name, 64);
                WriteInt32(fs, 0);
                WriteInt32(fs, b.childs);
                WriteInt32(fs, b.parent);
                WriteFloat(fs, b.rotation.x);
                WriteFloat(fs, b.rotation.z);
                WriteFloat(fs, b.rotation.y);
                WriteFloat(fs, b.rotation.w);
                WriteFloat(fs, b.location.x);
                WriteFloat(fs, b.location.z);
                WriteFloat(fs, b.location.y);
                WriteInt32(fs, 0);
                WriteInt32(fs, 0);
                WriteInt32(fs, 0);
                WriteInt32(fs, 0);
            }
            WriteHeader(fs, "RAWWEIGHTS", 0x1E83B9, 0xC, weights.Count);
            foreach (PSKWeight w in weights)
            {
                WriteFloat(fs, w.weight);
                WriteInt32(fs, w.point);
                WriteInt32(fs, w.bone);
            }
            //fs.Close();
        }

        private PSKHeader ReadHeader(FileStream fs)
        {
            PSKHeader res = new PSKHeader();
            res.name = ReadFixedString(fs, 20);
            res.flags = ReadInt32(fs);
            res.size = ReadInt32(fs);
            res.count = ReadInt32(fs);
            return res;
        }

        private void WriteHeader(Stream fs, string name, int flags, int size, int count)
        {
            WriteFixedString(fs, name, 20);
            WriteInt32(fs, flags);
            WriteInt32(fs, size);
            WriteInt32(fs, count);
        }

        public int ReadInt32(Stream fs)
        {
            byte[] buffer = new byte[4];
            fs.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public float ReadFloat(Stream fs)
        {
            byte[] buffer = new byte[4];
            fs.Read(buffer, 0, 4);
            return BitConverter.ToSingle(buffer, 0);
        }

        public ushort ReadUInt16(Stream fs)
        {
            byte[] buffer = new byte[2];
            fs.Read(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public string ReadFixedString(Stream fs, int len)
        {
            string s = "";
            byte b;
            for (int i = 0; i < len; i++)
                if ((b = (byte)fs.ReadByte()) != 0)
                    s += (char)b;
            return s;
        }

        public void WriteInt32(Stream fs, int i)
        {
            byte[] buffer = BitConverter.GetBytes(i);
            fs.Write(buffer, 0, 4);
        }

        public void WriteFloat(Stream fs, float f)
        {
            byte[] buffer = BitConverter.GetBytes(f);
            fs.Write(buffer, 0, 4);
        }

        public void WriteUInt16(Stream fs, ushort u)
        {
            byte[] buffer = BitConverter.GetBytes(u);
            fs.Write(buffer, 0, 2);
        }

        public void WriteFixedString(Stream fs, string s, int len)
        {
            for (int i = 0; i < len; i++)
                if (i < s.Length)
                    fs.WriteByte((byte)s[i]);
                else
                    fs.WriteByte(0);
        }
    }
}
