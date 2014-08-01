using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using OpenTK;
using System.Drawing;

namespace SM64DSe.ImportExport
{
    public class ModelBase
    {
        public class BoneDefRoot : IEnumerable<BoneDef>
        {
            private List<BoneDef> m_Bones;

            public BoneDefRoot()
            {
                m_Bones = new List<BoneDef>();
            }

            public void AddRootBone(BoneDef root)
            {
                m_Bones.Add(root);
                root.CalculateInverseTransformation();
            }

            public BoneDef GetBoneByID(string id)
            {
                foreach (BoneDef root in m_Bones)
                {
                    return root.GetBoneByID(id);
                }
                return null; // Not found
            }

            public IEnumerator<BoneDef> GetEnumerator()
            {
                List<BoneDef> bones = GetAsList();
                foreach (BoneDef bone in bones)
                {
                    yield return bone;
                }
            }

            public List<BoneDef> GetAsList()
            {
                return GetAsDictionary().Values.ToList();
            }

            public Dictionary<string, BoneDef> GetAsDictionary()
            {
                Dictionary<string, BoneDef> bones = new Dictionary<string, BoneDef>();
                foreach (BoneDef root in m_Bones)
                {
                    Dictionary<string, BoneDef> tmp = root.GetBranch();
                    foreach (var entry in tmp)
                        bones.Add(entry.Key, entry.Value);
                }
                return bones;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int GetBoneIndex(BoneDef bone)
            {
                return GetAsList().IndexOf(bone);
            }

            public int GetBoneIndex(string id)
            {
                return GetAsDictionary().Keys.ToList().IndexOf(id);
            }

            public int GetParentOffset(BoneDef bone)
            {
                if (bone.m_Parent != null)
                    return (GetBoneIndex(bone.m_Parent.m_ID) - GetBoneIndex(bone.m_ID));
                else
                    return 0;
            }

            public int GetNextSiblingOffset(BoneDef bone)
            {
                Dictionary<string, BoneDef> bonesInBranch = bone.GetRoot().GetBranch();

                // sibling offset, unlike parent offset should not be negative, bones should only point forward to their next sibling
                IEnumerable<BoneDef> siblings = bonesInBranch.Values.Where(bone0 => GetParentOffset(bone0) != 0 &&
                    bone0.m_Parent == bone.m_Parent);
                if (siblings.Count() > 0)
                {
                    int indexCurrent = siblings.ToList().IndexOf(bone);
                    if (indexCurrent == (siblings.Count() - 1))
                        return 0;
                    else
                        return (GetBoneIndex(siblings.ElementAt(indexCurrent + 1)) - GetBoneIndex(bone));
                }

                return 0;
            }

            public void Clear()
            {
                m_Bones.Clear();
            }

            public int Count
            {
                get { return GetAsList().Count; }
            }
        }

        public class BoneDef
        {
            private readonly Dictionary<string, BoneDef> m_Children = new Dictionary<string, BoneDef>();

            public string m_ID;
            public BoneDef m_Parent;

            public Vector3 m_Scale;
            public Vector3 m_Rotation;
            public Vector3 m_Translation;
            public Matrix4 m_GlobalTransformation;
            //public Matrix4 m_InverseTransformation;
            public Matrix4 m_GlobalInverseTransformation;

            public uint[] m_20_12Scale;
            public ushort[] m_4_12Rotation;
            public uint[] m_20_12Translation;

            public Dictionary<string, GeometryDef> m_Geometries;
            public List<string> m_MaterialsInBranch;

            public bool m_HasChildren { get { return m_Children.Count > 0; } }

            public bool m_Billboard;// Not used as not working

            public BoneDef(string id)
            {
                this.m_ID = id;
                m_Geometries = new Dictionary<string, GeometryDef>();
                m_MaterialsInBranch = new List<string>();
                SetScale(new Vector3(1f, 1f, 1f));
                SetRotation(new Vector3(0f, 0f, 0f));
                SetTranslation(new Vector3(0f, 0f, 0f));
            }

            public BoneDef GetBoneByID(string id)
            {
                if (m_ID.Equals(id))
                {
                    return this;
                }
                else
                {
                    foreach (BoneDef child in m_Children.Values)
                        return child.GetBoneByID(id);
                }
                return null; // Not found
            }

            public void AddChild(BoneDef item)
            {
                item.m_Parent = this;

                this.m_Children.Add(item.m_ID, item);
                item.CalculateTransformations();
            }

            public BoneDef GetRoot()
            {
                if (m_Parent == null)
                    return this;
                else
                    return this.m_Parent.GetRoot();
            }

            public string GetRootID()
            {
                return this.GetRoot().m_ID;
            }

            public Dictionary<string, BoneDef> GetBranch()
            {
                Dictionary<string, BoneDef> bones = new Dictionary<string, BoneDef>();
                GetBranchNodes(bones);
                return bones;
            }

            private void GetBranchNodes(Dictionary<string, BoneDef> bones)
            {
                bones.Add(this.m_ID, this);

                foreach (BoneDef child in m_Children.Values)
                    child.GetBranchNodes(bones);
            }

            public void SetScale(uint[] scale)
            {
                m_20_12Scale = scale;
                m_Scale = ConvertUInt20_12ToVector3(m_20_12Scale);
                CalculateTransformations();
            }

            public void SetScale(Vector3 scale)
            {
                m_Scale.X = scale.X;
                m_Scale.Y = scale.Y;
                m_Scale.Z = scale.Z;
                m_20_12Scale = ConvertVector3ToUInt20_12(m_Scale);
                CalculateTransformations();
            }

            public void SetRotation(ushort[] rotation)
            {
                m_4_12Rotation = rotation;
                m_Rotation = ConvertUShort4_12ToVector3(m_4_12Rotation);
                CalculateTransformations();
            }

            public void SetRotation(Vector3 rotation)
            {
                m_Rotation.X = rotation.X;
                m_Rotation.Y = rotation.Y;
                m_Rotation.Z = rotation.Z;
                m_4_12Rotation = ConvertVector3ToUShort4_12(m_Rotation);
                CalculateTransformations();
            }

            public void SetTranslation(uint[] translation)
            {
                m_20_12Translation = translation;
                m_Translation = ConvertUInt20_12ToVector3(m_20_12Translation);
                CalculateTransformations();
            }

            public void SetTranslation(Vector3 translation)
            {
                m_Translation.X = translation.X;
                m_Translation.Y = translation.Y;
                m_Translation.Z = translation.Z;
                m_20_12Translation = ConvertVector3ToUInt20_12(m_Translation);
                CalculateTransformations();
            }

            public void CalculateTransformations()
            {
                CalculateTransformation();
                CalculateInverseTransformation();

                foreach (BoneDef child in m_Children.Values)
                    child.CalculateTransformations();
            }

            public void CalculateTransformation()
            {
                Matrix4 ret = Matrix4.Identity;

                if (m_Parent != null)
                {
                    Matrix4.Mult(ref ret, ref m_Parent.m_GlobalTransformation, out ret);
                }

                Matrix4 srt = Helper.SRTToMatrix(m_Scale, m_Rotation, m_Translation);
                Matrix4.Mult(ref ret, ref srt, out ret);

                m_GlobalInverseTransformation = ret;
            }

            public void CalculateInverseTransformation()
            {
                Vector3 invScale = new Vector3((-1) * m_Scale.X, (-1) * m_Scale.Y, (-1) * m_Scale.Z);
                Vector3 invRot = new Vector3((-1) * m_Rotation.X, (-1) * m_Rotation.Y, (-1) * m_Rotation.Z);
                Vector3 invTrans = new Vector3((-1) * m_Translation.X, (-1) * m_Translation.Y, (-1) * m_Translation.Z);

                Matrix4 ret = Matrix4.Identity;

                if (m_Parent != null)
                {
                    Matrix4.Mult(ref ret, ref m_Parent.m_GlobalInverseTransformation, out ret);
                }

                Matrix4 inv = Helper.SRTToMatrix(invScale, invRot, invTrans);
                Matrix4.Mult(ref ret, ref inv, out ret);

                m_GlobalInverseTransformation = ret;
            }

            public static Vector3 ConvertUInt20_12ToVector3(uint[] values)
            {
                return new Vector3((float)values[0] / 4096.0f, (float)values[1] / 4096.0f, (float)values[2] / 4096.0f);
            }

            public static uint[] ConvertVector3ToUInt20_12(Vector3 vector)
            {
                return new uint[] { (uint)(vector.X * 4096.0f), 
                            (uint)(vector.Y * 4096.0f), (uint)(vector.Z * 4096.0f) };
            }

            public static Vector3 ConvertUShort4_12ToVector3(ushort[] values)
            {
                return new Vector3(((float)values[0] * (float)Math.PI) / 2048.0f,
                    ((float)values[1] * (float)Math.PI) / 2048.0f, ((float)values[2] * (float)Math.PI) / 2048.0f);
            }

            public static ushort[] ConvertVector3ToUShort4_12(Vector3 vector)
            {
                return new ushort[] { (ushort)((vector.X * 2048.0f) / Math.PI), 
                            (ushort)((vector.Y * 2048.0f) / Math.PI), (ushort)((vector.Z * 2048.0f) / Math.PI) };
            }

            public int Count
            {
                get { return this.m_Children.Count; }
            }
        }

        public class GeometryDef
        {
            public string m_ID;
            public Dictionary<string, PolyListDef> m_PolyLists;

            public GeometryDef(string id)
            {
                m_ID = id;
                m_PolyLists = new Dictionary<string, PolyListDef>();
            }
        }

        public class PolyListDef
        {
            public string m_ID;
            public string m_MaterialName;
            public List<FaceDef> m_Faces;

            public PolyListDef(string id, string materialName)
            {
                m_ID = id;
                m_MaterialName = materialName;
                m_Faces = new List<FaceDef>();
            }
        }

        public class FaceDef
        {
            public Vector3[] m_Vertices;
            public Vector2?[] m_TextureCoordinates;
            public Vector3?[] m_Normals;
            public Color?[] m_VertexColours;
            public int[] m_VertexBoneIDs;
            public int m_NumVertices;
        }

        public class MaterialDef
        {
            public string m_ID;
            public int m_Index;

            public Color m_DiffuseColour;
            public Color m_AmbientColour;
            public Color m_SpecularColour;
            public int m_Opacity;

            public bool m_HasTextures;

            public int m_ColType;

            public string m_DiffuseMapName;
            public int m_DiffuseMapID;
            public Vector2 m_DiffuseMapSize;

            // haxx
            public float m_TexCoordScale;

            public MaterialDef(string id)
            {
                m_ID = id;
            }
        }

        public BoneDefRoot m_BoneTree;
        public Dictionary<string, MaterialDef> m_Materials;
        public Dictionary<string, MaterialDef> m_Textures;

        public Dictionary<string, Bitmap> m_ConvertedTexturesBitmap;

        public string m_ModelFileName;
        public string m_ModelPath;

        public ModelBase(string modelFileName)
        {
            m_ModelFileName = modelFileName;
            m_ModelPath = Path.GetDirectoryName(m_ModelFileName);

            m_BoneTree = new BoneDefRoot();
            m_Materials = new Dictionary<string, MaterialDef>();
            m_Textures = new Dictionary<string, MaterialDef>();

            m_ConvertedTexturesBitmap = new Dictionary<string, Bitmap>();
        }

        public void ScaleModel(Vector3 scale)
        {
            foreach (ModelBase.BoneDef bone in m_BoneTree)
            {
                foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        foreach (ModelBase.FaceDef face in polyList.m_Faces)
                        {
                            for (int i = 0; i < face.m_Vertices.Length; i++)
                            {
                                Vector3 vtx = (Vector3)face.m_Vertices[i];
                                vtx.X *= scale.X;
                                vtx.Y *= scale.Y;
                                vtx.Z *= scale.Z;
                            }
                        }
                    }
                }
            }
        }

        public void ApplyTransformations()
        {
            // Need to maintain a list of transformed vertices to prevent the reverse transformation being 
            // applied more than once
            List<Vector3> transformedVertices = new List<Vector3>();
            // Now apply the vertex's bone's reverse transformation to each vertex
            foreach (ModelBase.BoneDef bone in m_BoneTree)
            {
                foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        foreach (ModelBase.FaceDef face in polyList.m_Faces)
                        {
                            for (int i = 0; i < face.m_NumVertices; i++)
                            {
                                ModelBase.BoneDef currentVertexBone = m_BoneTree.GetAsList()[face.m_VertexBoneIDs[i]];

                                // If vertex already transformed don't transform it again
                                if (transformedVertices.Contains(face.m_Vertices[i]))
                                    continue;
                                Vector3 vertex = face.m_Vertices[i];
                                Vector3.Transform(ref vertex, ref currentVertexBone.m_GlobalTransformation, out vertex);

                                transformedVertices.Add(vertex);
                            }
                        }
                    }
                }
            }
        }

        /*
         * Because transformations were applied when exporting, we need to reverse them so that when we set the bones' 
         * transforms, the transforms aren't applied again
         */
        public void ApplyInverseTransformations()
        {
            // Need to maintain a list of transformed vertices to prevent the reverse transformation being 
            // applied more than once
            List<Vector3> transformedVertices = new List<Vector3>();
            // Now apply the vertex's bone's reverse transformation to each vertex
            foreach (ModelBase.BoneDef bone in m_BoneTree)
            {
                foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        foreach (ModelBase.FaceDef face in polyList.m_Faces)
                        {
                            for (int i = 0; i < face.m_NumVertices; i++)
                            {
                                ModelBase.BoneDef currentVertexBone = m_BoneTree.GetAsList()[face.m_VertexBoneIDs[i]];

                                // If vertex already transformed don't transform it again
                                if (transformedVertices.Contains(face.m_Vertices[i]))
                                    continue;
                                Vector3 vertex = face.m_Vertices[i];
                                Vector3.Transform(ref vertex, ref currentVertexBone.m_GlobalInverseTransformation, out vertex);

                                transformedVertices.Add(vertex);
                            }
                        }
                    }
                }
            }
        }

    }
}
