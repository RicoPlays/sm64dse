﻿using System;
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

            public BoneDef GetBoneByIndex(int index)
            {
                return (index < Count) ? GetAsList().ElementAt(index) : null; 
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
            TriangleStrip,
            QuadrilateralStrip
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

            public VertexDef() 
            {
                m_TextureCoordinate = null;
                m_Normal = null;
                m_VertexColour = Color.White;
            }

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
            public string m_TextureDefID;
            public bool[] m_Lights;
            public PolygonDrawingFace m_PolygonDrawingFace;
            public int m_Alpha;
            public bool m_WireMode;
            public PolygonMode m_PolygonMode;
            public bool m_FogFlag;
            public bool m_DepthTestDecal;
            public bool m_RenderOnePixelPolygons;
            public bool m_FarClipping;
            public Color m_Diffuse;
            public Color m_Ambient;
            public Color m_Specular;
            public Color m_Emission;
            public TexTiling[] m_TexTiling;
            public Vector2 m_TextureScale;
            public float m_TextureRotation;
            public Vector2 m_TextureTranslation;

            public MaterialDef(string id, int index)
            {
                m_ID = id;
                m_Index = index;
                m_TextureDefID = null;
                m_Lights = new bool[] { false, false, false, false };
                m_PolygonDrawingFace = PolygonDrawingFace.Front;
                m_Alpha = 255;
                m_WireMode = false;
                m_PolygonMode = PolygonMode.Modulation;
                m_FogFlag = true;
                m_DepthTestDecal = false;
                m_RenderOnePixelPolygons = false;
                m_FarClipping = true;
                m_Diffuse = Color.White;
                m_Ambient = Color.White;
                m_Specular = Color.White;
                m_Emission = Color.Black;
                m_TexTiling = new TexTiling[] { TexTiling.Repeat, TexTiling.Repeat };
                m_TextureScale = new Vector2(1f, 1f);
                m_TextureRotation = 0.0f;
                m_TextureTranslation = new Vector2(0f, 0f);
            }

            public enum PolygonDrawingFace
            {
                Front,
                Back,
                FrontAndBack
            };

            public enum PolygonMode
            {
                Modulation,
                Decal,
                Toon_HighlightShading,
                Shadow
            };

            public enum TexTiling
            {
                Clamp,
                Repeat,
                Flip
            };
        }

        public enum TextureFormat
        {
            Nitro_A3I5 = 1,
            Nitro_Palette4 = 2,
            Nitro_Palette16 = 3,
            Nitro_Palette256 = 4,
            Nitro_Tex4x4 = 5,
            Nitro_A5I3 = 6,
            Nitro_Direct = 7,
            ExternalBitmap = 8,
            InMemoryBitmap = 9
        };

        public class TextureDefBase
        {
            public string m_ID;
            public string m_ImgHash;
            public TextureFormat m_Format;
            protected uint m_Width;
            protected uint m_Height;
            protected string m_TexName;
            protected string m_PalName;

            public virtual uint GetWidth() { return m_Width; }
            public virtual uint GetHeight() { return m_Height; }
            public virtual string GetTexName() { return m_TexName; }
            public virtual string GetPalName() { return m_PalName; }
            public virtual string CalculateHash() { return null; }
            public virtual bool IsNitro() { return false; }
            public virtual Bitmap GetBitmap() { return null; }
            public virtual byte[] GetNitroTexData() { return null; }
            public virtual bool HasNitroPalette() { return false; }
            public virtual byte[] GetNitroPalette() { return null; }
            public virtual byte GetColor0Mode() { return 0; }
        }

        public class TextureDefBitmapBase : TextureDefBase
        {
            protected void TexAndPalNamesFromFilename(string name)
            {
                string path_separator = (name.Contains("/")) ? "/" : "\\";
                m_TexName = name.Substring(name.LastIndexOf(path_separator) + 1).Replace('.', '_');
                m_PalName = m_TexName + "_pl";
            }

            public override string CalculateHash()
            {
                Bitmap tex = GetBitmap();

                int width = 8, height = 8;
                while (width < GetWidth()) width *= 2;
                while (height < GetHeight()) height *= 2;

                // cheap resizing for textures whose dimensions aren't power-of-two
                if ((width != GetWidth()) || (height != GetHeight()))
                {
                    Bitmap newbmp = new Bitmap(width, height);
                    Graphics g = Graphics.FromImage(newbmp);
                    g.DrawImage(tex, new Rectangle(0, 0, width, height));
                    tex = newbmp;
                }

                byte[] map = new byte[tex.Width * tex.Height * 4];
                for (int y = 0; y < tex.Height; y++)
                {
                    for (int x = 0; x < tex.Width; x++)
                    {
                        Color pixel = tex.GetPixel(x, y);
                        int pos = ((y * tex.Width) + x) * 4;

                        map[pos] = pixel.B;
                        map[pos + 1] = pixel.G;
                        map[pos + 2] = pixel.R;
                        map[pos + 3] = pixel.A;
                    }
                }

                string imghash = Helper.HexString(Helper.m_MD5.ComputeHash(map));
                return imghash;
            }
        }

        public class TextureDefExternalBitmap : TextureDefBitmapBase
        {
            public string m_FileName;

            public TextureDefExternalBitmap(string id, string fileName)
            {
                m_ID = id;
                m_FileName = fileName;
                m_Format = TextureFormat.ExternalBitmap;
                using (Bitmap tmp = new Bitmap(fileName))
                {
                    m_Width = (uint)tmp.Width;
                    m_Height = (uint)tmp.Height;
                }
                TexAndPalNamesFromFilename(fileName);
                m_ImgHash = CalculateHash();
            }

            public override Bitmap GetBitmap()
            {
                return new Bitmap(m_FileName);
            }
        }

        public class TextureDefInMemoryBitmap : TextureDefBitmapBase
        {
            protected Bitmap m_Bitmap;

            public TextureDefInMemoryBitmap(string id, Bitmap bmp)
            {
                m_ID = id;
                m_Format = TextureFormat.InMemoryBitmap;
                m_Bitmap = bmp;
                m_TexName = id;
                m_PalName = id + "_pl";
                m_ImgHash = CalculateHash();
            }

            public override Bitmap GetBitmap()
            {
                return m_Bitmap;
            }
        }

        public class TextureDefNitro : TextureDefBase
        {
            protected byte[] m_TexData;
            protected byte[] m_PalData;
            protected byte m_Color0Mode;

            public TextureDefNitro(string texID, byte[] texData, uint width, uint height, byte color0Mode, TextureFormat format)
            {
                m_ID = texID;
                m_TexName = texID;
                m_TexData = texData;
                m_Format = format;
                m_Width = width;
                m_Height = height;
                m_Color0Mode = color0Mode;
                m_ImgHash = CalculateHash();
            }

            public TextureDefNitro(string texID, byte[] texData, string palID, byte[] palData, 
                uint width, uint height, byte color0Mode, TextureFormat format)
            {
                m_ID = texID;
                m_TexName = texID;
                m_TexData = texData;
                m_PalName = palID;
                m_PalData = palData;
                m_Width = width;
                m_Height = height;
                m_Color0Mode = color0Mode;
                m_Format = format;
                m_ImgHash = CalculateHash();
            }

            public override string CalculateHash()
            {
                if (!HasNitroPalette())
                    return Helper.HexString(Helper.m_MD5.ComputeHash(m_TexData));
                else
                {
                    byte[] hashtmp = new byte[m_TexData.Length + m_PalData.Length];
                    Array.Copy(m_TexData, hashtmp, m_TexData.Length);
                    Array.Copy(m_PalData, 0, hashtmp, m_TexData.Length, m_PalData.Length);
                    return Helper.HexString(Helper.m_MD5.ComputeHash(hashtmp));
                }
            }

            public override Bitmap GetBitmap()
            {
                throw new NotImplementedException();
                // May implement for exporting IMD directly to OBJ or DAE in future (but why?)
            }

            public override bool IsNitro()
            {
                return true;
            }

            public override byte[] GetNitroTexData()
            {
                return m_TexData;
            }

            public override bool HasNitroPalette()
            {
                return (m_Format != TextureFormat.Nitro_Direct);
            }

            public override byte[] GetNitroPalette()
            {
                return m_PalData;
            }

            public override byte GetColor0Mode()
            {
                return m_Color0Mode;
            }
        }

        public class AnimationDef
        {
            public string m_ID;
            public string m_BoneID;
            public Dictionary<AnimationComponentType, AnimationComponentDataDef> m_AnimationComponents;
            public int m_NumFrames;

            public AnimationDef(string id, string boneID, int numFrames)
            {
                m_ID = id;
                m_BoneID = boneID;
                m_NumFrames = numFrames;
                m_AnimationComponents = new Dictionary<AnimationComponentType, AnimationComponentDataDef>();
            }

            public AnimationDef(string id, string boneID, int numFrames, 
                Dictionary<AnimationComponentType, AnimationComponentDataDef> animationComponents)
            {
                m_ID = id;
                m_BoneID = boneID;
                m_NumFrames = numFrames;
                m_AnimationComponents = animationComponents;
            }

            public int GetTotalNumberOfFrameValues()
            {
                int sum = 0;
                foreach (AnimationComponentDataDef comp in m_AnimationComponents.Values)
                    sum += comp.GetNumValues();

                return sum;
            }

            public int GetScaleValuesCount()
            {
                return m_AnimationComponents[AnimationComponentType.ScaleX].GetNumValues() +
                    m_AnimationComponents[AnimationComponentType.ScaleY].GetNumValues() +
                    m_AnimationComponents[AnimationComponentType.ScaleZ].GetNumValues();
            }

            public int GetRotateValuesCount()
            {
                return m_AnimationComponents[AnimationComponentType.RotateX].GetNumValues() +
                    m_AnimationComponents[AnimationComponentType.RotateY].GetNumValues() +
                    m_AnimationComponents[AnimationComponentType.RotateZ].GetNumValues();
            }

            public int GetTranslateValuesCount()
            {
                return m_AnimationComponents[AnimationComponentType.TranslateX].GetNumValues() +
                    m_AnimationComponents[AnimationComponentType.TranslateY].GetNumValues() +
                    m_AnimationComponents[AnimationComponentType.TranslateZ].GetNumValues();
            }

            public Vector3 GetFrameScale(int frame)
            {
                return new Vector3(m_AnimationComponents[AnimationComponentType.ScaleX].GetFrameValue(frame),
                    m_AnimationComponents[AnimationComponentType.ScaleY].GetFrameValue(frame),
                    m_AnimationComponents[AnimationComponentType.ScaleZ].GetFrameValue(frame));
            }

            public Vector3 GetFrameRotation(int frame)
            {
                return new Vector3(m_AnimationComponents[AnimationComponentType.RotateX].GetFrameValue(frame),
                    m_AnimationComponents[AnimationComponentType.RotateY].GetFrameValue(frame),
                    m_AnimationComponents[AnimationComponentType.RotateZ].GetFrameValue(frame));
            }

            public Vector3 GetFrameTranslation(int frame)
            {
                return new Vector3(m_AnimationComponents[AnimationComponentType.TranslateX].GetFrameValue(frame),
                    m_AnimationComponents[AnimationComponentType.TranslateY].GetFrameValue(frame),
                    m_AnimationComponents[AnimationComponentType.TranslateZ].GetFrameValue(frame));
            }

            public BCA.SRTContainer GetFrame(int frame)
            {
                Vector3 scale = new Vector3(GetFrameScale(frame));
                Vector3 rotation = new Vector3(GetFrameRotation(frame));
                Vector3 translation = new Vector3(GetFrameTranslation(frame));

                return new BCA.SRTContainer(scale, rotation, translation);
            }

            public BCA.SRTContainer[] GetAllFrames()
            {
                BCA.SRTContainer[] frames = new BCA.SRTContainer[m_NumFrames];

                for (int i = 0; i < m_NumFrames; i++)
                {
                    frames[i] = GetFrame(i);
                }

                return frames;
            }
        }
        
        // Note: Rotation is stored in Radians

        public class AnimationComponentDataDef
        {
            public AnimationComponentType m_AnimationComponentType;
            private float[] m_Values;
            private int m_NumFrames;
            private bool m_IsConstant;
            private int m_FrameStep;

            public AnimationComponentDataDef(float[] values, int numFrames, bool isConstant, int frameStep, 
                AnimationComponentType animationComponentType)
            {
                m_Values = values;
                m_NumFrames = numFrames;
                m_IsConstant = isConstant;
                m_FrameStep = frameStep;
                m_AnimationComponentType = animationComponentType;
            }

            public float GetValue(int index) { return m_Values[index]; }
            public void SetValue(int index, float value) { m_Values[index] = value; }

            public byte[] GetFixedPointValues()
            {
                byte[] result = new byte[0];
                if (m_AnimationComponentType == AnimationComponentType.RotateX ||
                    m_AnimationComponentType == AnimationComponentType.RotateY ||
                    m_AnimationComponentType == AnimationComponentType.RotateZ)
                {
                    result = new byte[m_Values.Length * sizeof(ushort)];
                    Buffer.BlockCopy(Array.ConvertAll<float, ushort>(m_Values, x => (ushort)((x * 2048.0f) / Math.PI)), 
                        0, result, 0, result.Length);
                }
                else
                {
                    result = new byte[m_Values.Length * sizeof(uint)];
                    Buffer.BlockCopy(Array.ConvertAll<float, uint>(m_Values, x => (uint)(x * 4096f)), 
                        0, result, 0, result.Length);
                }
                return result;
            }

            public int GetNumValues() { return m_Values.Length; }

            public int GetFrameStep() { return m_FrameStep; }
            public bool GetIsConstant() { return m_IsConstant; }

            public float GetFrameValue(int frameNum)
            {
                if (m_IsConstant)
                {
                    return m_Values[0];
                }
                else
                {
                    if (m_FrameStep == 1)
                    {
                        return m_Values[frameNum];
                    }
                    else
                    {
                        // Odd frames
                        if ((frameNum & 1) != 0)
                        {

                            if ((frameNum / m_FrameStep) + 1 > m_Values.Length - 1)
                            {
                                // if floor(frameNum / 2) + 1 > number of values, use floor(frameNum / 2)
                                return m_Values[(frameNum / m_FrameStep)];
                            }
                            else if (frameNum == (m_NumFrames - 1))
                            {
                                // else if it's the last frame, don't interpolate
                                return m_Values[(frameNum / m_FrameStep) + 1];
                            }
                            else
                            {
                                float val1 = m_Values[frameNum >> 1];
                                float val2 = m_Values[(frameNum >> 1) + 1];
                                if (m_AnimationComponentType == AnimationComponentType.RotateX ||
                                    m_AnimationComponentType == AnimationComponentType.RotateY ||
                                    m_AnimationComponentType == AnimationComponentType.RotateZ)
                                {
                                    if (val1 < 0f && val2 > 0f)
                                    {
                                        if (Math.Abs(val2 - (val1 + (Math.PI * 2f))) < Math.Abs(val2 - val1))
                                        {
                                            val2 -= (float)(Math.PI * 2f);
                                        }
                                    }
                                    else if (val1 > 0f && val2 < 0f)
                                    {
                                        if (Math.Abs(val1 - (val2 + (Math.PI * 2f))) < Math.Abs(val1 - val2))
                                        {
                                            val2 += (float)(Math.PI * 2f);
                                        }
                                    }
                                }
                                return val1 + (((val1 + val2) / 2f) * (frameNum % m_FrameStep));
                            }
                        }
                        else
                        {
                            // Even frames
                            return m_Values[frameNum / m_FrameStep];
                        }
                    }
                }
            }
        }

        public enum AnimationComponentType
        {
            ScaleX,
            ScaleY,
            ScaleZ,
            RotateX,
            RotateY,
            RotateZ,
            TranslateX,
            TranslateY,
            TranslateZ
        };

        public BoneDefRoot m_BoneTree;
        public Dictionary<string, MaterialDef> m_Materials;
        public Dictionary<string, TextureDefBase> m_Textures;
        public Dictionary<string, AnimationDef> m_Animations;
        public BiDictionaryOneToOne<string, int> m_BoneTransformsMap;

        // Should just be temporary, need to work out how to properly import IMD models where pos_scale > 0, 
        // result of (1 << pos_scale), default pos_scale = 0
        public uint m_PosScaleFactor = 1;

        public string m_ModelFileName;
        public string m_ModelPath;

        public ModelBase(string modelFileName)
        {
            m_ModelFileName = modelFileName;
            m_ModelPath = Path.GetDirectoryName(m_ModelFileName);

            m_BoneTree = new BoneDefRoot();
            m_Materials = new Dictionary<string, MaterialDef>();
            m_Textures = new Dictionary<string, TextureDefBase>();
            m_Animations = new Dictionary<string, AnimationDef>();
            m_BoneTransformsMap = new BiDictionaryOneToOne<string, int>();
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
                foreach (AnimationComponentDataDef comp in animDef.m_AnimationComponents.Values)
                {
                    switch (comp.m_AnimationComponentType)
                    {
                        case AnimationComponentType.TranslateX:
                            {
                                for (int i = 0; i < comp.GetNumValues(); i++)
                                    comp.SetValue(i, comp.GetValue(i) * scale.X);
                            }
                            break;
                        case AnimationComponentType.TranslateY:
                            {
                                for (int i = 0; i < comp.GetNumValues(); i++)
                                    comp.SetValue(i, comp.GetValue(i) * scale.Y);
                            }
                            break;
                        case AnimationComponentType.TranslateZ:
                            {
                                for (int i = 0; i < comp.GetNumValues(); i++)
                                    comp.SetValue(i, comp.GetValue(i) * scale.Z);
                            }
                            break;
                    }
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