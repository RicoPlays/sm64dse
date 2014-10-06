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
                if (GetBoneIndex(root.m_ID) == -1)
                {
                    m_Bones.Add(root);
                    root.CalculateInverseTransformation();
                }
            }

            public BoneDef GetBoneByID(string id)
            {
                var queue = new Queue<BoneDef>();
                foreach (BoneDef root in m_Bones)
                    queue.Enqueue(root);

                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();

                    if (node.m_ID.Equals(id))
                        return node;

                    foreach (var child in node.GetBranch().Values)
                        queue.Enqueue(child);
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

            public List<BoneDef> GetRootBones()
            {
                return m_Bones;
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
            public Matrix4 m_LocalTransformation;
            public Matrix4 m_GlobalTransformation;
            public Matrix4 m_LocalInverseTransformation;
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

            public void AddChild(BoneDef item)
            {
                item.m_Parent = this;

                this.m_Children.Add(item.m_ID, item);
                item.CalculateBranchTransformations();
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

            public Dictionary<string, BoneDef> GetChildren()
            {
                return m_Children;
            }

            public void SetScale(uint[] scale)
            {
                m_20_12Scale = scale;
                m_Scale = ConvertUInt20_12ToVector3(m_20_12Scale);
            }

            public void SetScale(Vector3 scale)
            {
                m_Scale.X = scale.X;
                m_Scale.Y = scale.Y;
                m_Scale.Z = scale.Z;
                m_20_12Scale = ConvertVector3ToUInt20_12(m_Scale);
            }

            public void SetRotation(ushort[] rotation)
            {
                m_4_12Rotation = rotation;
                m_Rotation = ConvertUShort4_12ToVector3(m_4_12Rotation);
            }

            public void SetRotation(Vector3 rotation)
            {
                m_Rotation.X = rotation.X;
                m_Rotation.Y = rotation.Y;
                m_Rotation.Z = rotation.Z;
                m_4_12Rotation = ConvertVector3ToUShort4_12(m_Rotation);
            }

            public void SetTranslation(uint[] translation)
            {
                m_20_12Translation = translation;
                m_Translation = ConvertUInt20_12ToVector3(m_20_12Translation);
            }

            public void SetTranslation(Vector3 translation)
            {
                m_Translation.X = translation.X;
                m_Translation.Y = translation.Y;
                m_Translation.Z = translation.Z;
                m_20_12Translation = ConvertVector3ToUInt20_12(m_Translation);
            }

            public void CalculateBranchTransformations()
            {
                CalculateTransformation();
                CalculateInverseTransformation();

                foreach (BoneDef child in m_Children.Values)
                    child.CalculateBranchTransformations();
            }

            public void CalculateTransformation()
            {
                m_LocalTransformation = Helper.SRTToMatrix(m_Scale, m_Rotation, m_Translation);
                m_GlobalTransformation = m_LocalTransformation;

                if (m_Parent != null)
                {
                    Matrix4.Mult(ref m_LocalTransformation, ref m_Parent.m_GlobalTransformation, out m_GlobalTransformation);
                }
            }

            public void CalculateInverseTransformation()
            {
                Vector3 invScale = new Vector3((1f) / m_Scale.X, (1f) / m_Scale.Y, (1f) / m_Scale.Z);
                Vector3 invRot = new Vector3((-1) * m_Rotation.X, (-1) * m_Rotation.Y, (-1) * m_Rotation.Z);
                Vector3 invTrans = new Vector3((-1) * m_Translation.X, (-1) * m_Translation.Y, (-1) * m_Translation.Z);

                Matrix4 ret = Matrix4.Identity;

                if (m_Parent != null)
                {
                    Matrix4.Mult(ref ret, ref m_Parent.m_GlobalInverseTransformation, out ret);
                }

                Matrix4 inv = Helper.InverseSRTToMatrix(invScale, invRot, invTrans);
                Matrix4.Mult(ref ret, ref inv, out ret);

                m_LocalInverseTransformation = inv;
                m_GlobalInverseTransformation = ret;
            }

            public static Vector3 ConvertUInt20_12ToVector3(uint[] values)
            {
                return new Vector3((float)(int)values[0] / 4096.0f, (float)(int)values[1] / 4096.0f, (float)(int)values[2] / 4096.0f);
            }

            public static uint[] ConvertVector3ToUInt20_12(Vector3 vector)
            {
                return new uint[] { (uint)(vector.X * 4096.0f), 
                    (uint)(vector.Y * 4096.0f), (uint)(vector.Z * 4096.0f) };
            }

            public static Vector3 ConvertUShort4_12ToVector3(ushort[] values)
            {
                return new Vector3(((float)(short)values[0] * (float)Math.PI) / 2048.0f,
                    ((float)(short)values[1] * (float)Math.PI) / 2048.0f, ((float)(short)values[2] * (float)Math.PI) / 2048.0f);
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
            public List<FaceListDef> m_FaceLists;

            public PolyListDef(string id, string materialName)
            {
                m_ID = id;
                m_MaterialName = materialName;
                m_FaceLists = new List<FaceListDef>();
            }
        }

        public enum PolyListType
        {
            Polygons,
            Triangles,
            TriangleStrip
        };

        public class FaceListDef
        {
            public List<FaceDef> m_Faces;
            public PolyListType m_Type;

            public FaceListDef() :
                this(PolyListType.Polygons) { }

            public FaceListDef(PolyListType type)
            {
                m_Faces = new List<FaceDef>();
                m_Type = type;
            }
        }

        public class FaceDef
        {
            public VertexDef[] m_Vertices;
            public int m_NumVertices;

            public FaceDef() { }

            public FaceDef(int numVertices)
            {
                m_NumVertices = numVertices;
                m_Vertices = new VertexDef[m_NumVertices];
            }
        }

        public class VertexDef
        {
            public Vector3 m_Position;
            public Vector2? m_TextureCoordinate;
            public Vector3? m_Normal;
            public Color m_VertexColour;
            public int m_VertexBoneID;

            public VertexDef() { }

            public VertexDef(Vector3 position, Vector2? textureCoordinate, Vector3? normal, Color vertexColour, int vertexBoneID)
            {
                m_Position = position;
                m_TextureCoordinate = textureCoordinate;
                m_Normal = normal;
                m_VertexColour = vertexColour;
                m_VertexBoneID = vertexBoneID;
            }

            public override bool Equals(object obj)
            {
                var fv = obj as VertexDef;
                if (fv == null)
                    return false;

                if (!(fv.m_Position.X == m_Position.X && fv.m_Position.Y == m_Position.Y && fv.m_Position.Z == this.m_Position.Z))
                    return false;

                if (!(fv.m_TextureCoordinate == null && m_TextureCoordinate == null))
                {
                    if (!(((Vector2)fv.m_TextureCoordinate).X == ((Vector2)m_TextureCoordinate).X && 
                        ((Vector2)fv.m_TextureCoordinate).Y == ((Vector2)m_TextureCoordinate).Y))
                        return false;
                }

                if (!(fv.m_Normal == null && m_Normal == null))
                {
                    if (!(((Vector3)fv.m_Normal).X == ((Vector3)m_Normal).X && ((Vector3)fv.m_Normal).Y == ((Vector3)m_Normal).Y &&
                        ((Vector3)fv.m_Normal).Z == ((Vector3)m_Normal).Z))
                        return false;
                }

                if (!(fv.m_VertexColour.R == m_VertexColour.R && fv.m_VertexColour.G == m_VertexColour.G &&
                    fv.m_VertexColour.B == m_VertexColour.B && fv.m_VertexColour.A == m_VertexColour.A))
                    return false;

                if (!(fv.m_VertexBoneID == m_VertexBoneID))
                    return false;

                return true;
            }
        }

        public class MaterialDef
        {
            public string m_ID;
            public int m_Index;

            public Color m_DiffuseColour;
            public Color m_AmbientColour;
            public Color m_SpecularColour;
            public Color m_EmissionColour;
            public int m_Opacity;

            public bool m_HasTextures;

            public bool m_IsDoubleSided;

            public int m_ColType;

            public string m_DiffuseMapName;
            public Vector2 m_DiffuseMapSize;
            public bool m_DiffuseMapInMemory;

            // haxx
            public float m_TexCoordScale;

            public MaterialDef(string id, int index)
            {
                m_ID = id;
                m_Index = index;
                m_DiffuseColour = Color.White;
                m_AmbientColour = Color.White;
                m_SpecularColour = Color.White;
                m_EmissionColour = Color.White;
                m_Opacity = 255; // oops
                m_HasTextures = false;
                m_IsDoubleSided = false;
                m_DiffuseMapName = "";
                m_DiffuseMapSize = new Vector2(0f, 0f);
                m_DiffuseMapInMemory = false;
                m_ColType = 0;
            }
        }

        public class AnimationDef
        {
            public string m_ID;
            public string m_BoneID;
            public List<AnimationFrameDef> m_AnimationFrames;

            public AnimationDef(string id, string boneID)
            {
                m_ID = id;
                m_BoneID = boneID;
                m_AnimationFrames = new List<AnimationFrameDef>();
            }

            public AnimationDef(string id, string boneID, List<AnimationFrameDef> animationFrames)
            {
                m_ID = id;
                m_BoneID = boneID;
                m_AnimationFrames = animationFrames;
            }

            public int m_NumFrames
            {
                get { return this.m_AnimationFrames.Count; }
            }
        }

        public class AnimationFrameDef
        {
            private Vector3 m_Scale;
            private Vector3 m_Rotation;
            private Vector3 m_Translation;
            private Matrix4 m_Transformation;

            public AnimationFrameDef()
            {
                m_Scale = new Vector3(1f, 1f, 1f);
                m_Rotation = new Vector3(0f, 0f, 0f);
                m_Translation = new Vector3(0f, 0f, 0f);
                m_Transformation = Helper.SRTToMatrix(m_Scale, m_Rotation, m_Translation);
            }

            public AnimationFrameDef(Vector3 scale, Vector3 rotation, Vector3 tranlation)
            {
                m_Scale = scale;
                m_Rotation = rotation;
                m_Translation = tranlation;
                m_Transformation = Helper.SRTToMatrix(m_Scale, m_Rotation, m_Translation);
            }

            public void SetScale(Vector3 scale)
            {
                m_Scale = scale;
                m_Transformation = Helper.SRTToMatrix(m_Scale, m_Rotation, m_Translation);
            }

            public void SetRotation(Vector3 rotation)
            {
                m_Rotation = rotation;
                m_Transformation = Helper.SRTToMatrix(m_Scale, m_Rotation, m_Translation);
            }

            public void SetTranslation(Vector3 translation)
            {
                m_Translation = translation;
                m_Transformation = Helper.SRTToMatrix(m_Scale, m_Rotation, m_Translation);
            }

            public Vector3 GetScale() { return m_Scale; }
            public Vector3 GetRotation() { return m_Rotation; }
            public Vector3 GetRotationInDegrees()
            {
                return new Vector3(m_Rotation.X * Helper.Rad2Deg, m_Rotation.Y * Helper.Rad2Deg, m_Rotation.Z * Helper.Rad2Deg);
            }
            public Vector3 GetTranslation() { return m_Translation; }
            public Matrix4 GetTransformation() { return m_Transformation; }
        }

        public BoneDefRoot m_BoneTree;
        public Dictionary<string, MaterialDef> m_Materials;
        public Dictionary<string, MaterialDef> m_Textures;
        public Dictionary<string, AnimationDef> m_Animations;

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
            m_Animations = new Dictionary<string, AnimationDef>();

            m_ConvertedTexturesBitmap = new Dictionary<string, Bitmap>();
        }

        public void ScaleModel(Vector3 scale)
        {
            foreach (BoneDef bone in m_BoneTree)
            {
                foreach (GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        foreach (FaceListDef faceList in polyList.m_FaceLists)
                        {
                            foreach (FaceDef face in faceList.m_Faces)
                            {
                                foreach (VertexDef vert in face.m_Vertices)
                                {
                                    vert.m_Position.X *= scale.X;
                                    vert.m_Position.Y *= scale.Y;
                                    vert.m_Position.Z *= scale.Z;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Use below method can be used as part of a manual hack to export existing animations at a larger scale, 
        // combine them with a scaled geometry exported from a 3D modeller and replace the existing model. This was 
        // used to get around the fact Blender only exports matrices in DAE.
        public void ScaleSkeletonAndAnimations(Vector3 scale)
        {
            foreach (BoneDef bone in m_BoneTree)
            {
                Vector3 translation = bone.m_Translation;
                bone.SetTranslation(Vector3.Multiply(translation, scale));
            }
            foreach (AnimationDef animDef in m_Animations.Values)
            {
                foreach (AnimationFrameDef frame in animDef.m_AnimationFrames)
                {
                    Vector3 translation = frame.GetTranslation();
                    frame.SetTranslation(Vector3.Multiply(translation, scale));
                }
            }
        }

        public void ApplyTransformations()
        {
            // Now apply the vertex's bone's transformation to each vertex
            foreach (BoneDef bone in m_BoneTree)
            {
                foreach (GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        foreach (FaceListDef faceList in polyList.m_FaceLists)
                        {
                            foreach (FaceDef face in faceList.m_Faces)
                            {
                                foreach (VertexDef vert in face.m_Vertices)
                                {
                                    BoneDef currentVertexBone = m_BoneTree.GetAsList()[vert.m_VertexBoneID];

                                    Vector3 vertex = vert.m_Position;
                                    Vector3.Transform(ref vertex, ref currentVertexBone.m_GlobalTransformation, out vertex);
                                    vert.m_Position = vertex;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ApplyInverseTransformations()
        {
            // DEBUG ONLY
            //foreach (BoneDef bone in m_BoneTree)
            //    Console.WriteLine(bone.m_ID + ".m_GlobalInverseTransformation: " + bone.m_GlobalInverseTransformation.ToString());

            // Now apply the vertex's bone's reverse transformation to each vertex
            foreach (BoneDef bone in m_BoneTree)
            {
                foreach (GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        foreach (FaceListDef faceList in polyList.m_FaceLists)
                        {
                            foreach (FaceDef face in faceList.m_Faces)
                            {
                                foreach (VertexDef vert in face.m_Vertices)
                                {
                                    BoneDef currentVertexBone = m_BoneTree.GetAsList()[vert.m_VertexBoneID];

                                    Vector3 vertex = vert.m_Position;
                                    Vector3.Transform(ref vertex, ref currentVertexBone.m_GlobalInverseTransformation, out vertex);
                                    vert.m_Position = vertex;
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
