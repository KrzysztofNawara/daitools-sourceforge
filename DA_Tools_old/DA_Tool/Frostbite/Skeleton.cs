using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Tool.Frostbite
{
    public class DAIBone
    {
        public String Name;
        public Vector3 Right;
        public Vector3 Up;
        public Vector3 Forward;
        public Vector3 Location;
        public int ParentIndex;
        public List<DAIBone> Children;

        public DAIBone(String InName)
        {
            Name = InName;
        }
    }

    public class DAISkeleton
    {
        public List<DAIBone> Bones;
        public DAIBone RootBone;

        public DAISkeleton(DAIEbx Ebx)
        {
            Bones = new List<DAIBone>();

            DAIComplex BoneNamesArray = Ebx.RootInstance.GetFieldByName("BoneNames").GetComplexValue();
            foreach (DAIField BoneNameArrayMember in BoneNamesArray.Fields)
            {
                string BoneName = BoneNameArrayMember.GetStringValue();
                Bones.Add(new DAIBone(BoneName));
            }

            int BoneIdx = 0;
            DAIComplex HierarchyArray = Ebx.RootInstance.GetFieldByName("Hierarchy").GetComplexValue();
            foreach (DAIField HierarchyMember in HierarchyArray.Fields)
            {
                DAIBone Bone = Bones[BoneIdx];
                Bone.ParentIndex = HierarchyMember.GetIntValue();
                BoneIdx++;
            }

            BoneIdx = 0;
            DAIComplex LocalPoseArray = Ebx.RootInstance.GetFieldByName("LocalPose").GetComplexValue();
            foreach (DAIField LocalPoseMember in LocalPoseArray.Fields)
            {
                DAIBone Bone = Bones[BoneIdx];
                DAIComplex LinearTransform = LocalPoseMember.GetComplexValue();
                DAIComplex Right = LinearTransform.GetFieldByName("right").GetComplexValue();
                DAIComplex Up = LinearTransform.GetFieldByName("up").GetComplexValue();
                DAIComplex Forward = LinearTransform.GetFieldByName("forward").GetComplexValue();
                DAIComplex Trans = LinearTransform.GetFieldByName("trans").GetComplexValue();

                Bone.Right = new Vector3();
                Bone.Right.X = Right.GetFieldByName("x").GetFloatValue();
                Bone.Right.Y = Right.GetFieldByName("y").GetFloatValue();
                Bone.Right.Z = Right.GetFieldByName("z").GetFloatValue();

                Bone.Up = new Vector3();
                Bone.Up.X = Up.GetFieldByName("x").GetFloatValue();
                Bone.Up.Y = Up.GetFieldByName("y").GetFloatValue();
                Bone.Up.Z = Up.GetFieldByName("z").GetFloatValue();

                Bone.Forward = new Vector3();
                Bone.Forward.X = Forward.GetFieldByName("x").GetFloatValue();
                Bone.Forward.Y = Forward.GetFieldByName("y").GetFloatValue();
                Bone.Forward.Z = Forward.GetFieldByName("z").GetFloatValue();

                Bone.Location = new Vector3();
                Bone.Location.X = Trans.GetFieldByName("x").GetFloatValue();
                Bone.Location.Y = Trans.GetFieldByName("y").GetFloatValue();
                Bone.Location.Z = Trans.GetFieldByName("z").GetFloatValue();

                BoneIdx++;
            }

            DAIComplex ModelPoseArray = Ebx.RootInstance.GetFieldByName("ModelPose").GetComplexValue();

            for (int i = 0; i < Bones.Count; i++)
            {
                Bones[i].Children = new List<DAIBone>();
                for (int j = 0; j < Bones.Count; j++)
                {
                    if (Bones[j].ParentIndex == i)
                        Bones[i].Children.Add(Bones[j]);
                }

                if (Bones[i].ParentIndex == -1 && RootBone == null)
                    RootBone = Bones[i];
            }
        }
    }
}
