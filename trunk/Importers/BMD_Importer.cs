/* SM64DSe allows the importing of models to BMD format.
 * Currently supported:
 * 
 * COLLADA DAE:
 *  
 * Imports the model from a COLLADA DAE model complete with full joints and skinning (rigging).
 * 
 * Notes:
 * BMD does not support vertex weights, instead each vertex is only assigned to one bone - where a 
 * DAE model uses multiple < 1.0 vertex weights, the largest value is used to assign the vertex to 
 * a joint.
 * 
 * This is the recommended format for importing as it matches the features of BMD almost exactly.
 * 
 * 
 * Wavefront OBJ:
 * 
 * Imports an OBJ model.
 * 
 * Notes: 
 * Supports the Blender-specific "Extended OBJ" plugin's vertex colours. 
 * OBJ does not support joints so each object, "o" command is treated as a bone with a custom *.bones file 
 * used to read the hierarchy and properties.
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using OpenTK.Graphics.OpenGL;
using System.Security.Cryptography;
using System.Xml;
using System.Text.RegularExpressions;

namespace SM64DSe.Importers
{
    public class BMD_Importer_Base
    {
        public class GXDisplayListPacker
        {
            public GXDisplayListPacker()
            {
                m_CommandList = new List<GXCommand>();
            }

            public void AddCommand(byte _cmd, uint[] _params)
            {
                m_CommandList.Add(new GXCommand(_cmd, _params));
            }
            public void AddCommand(byte _cmd)
            {
                m_CommandList.Add(new GXCommand(_cmd, new uint[] { }));
            }
            public void AddCommand(byte _cmd, uint param1)
            {
                m_CommandList.Add(new GXCommand(_cmd, new uint[] { param1 }));
            }
            public void AddCommand(byte _cmd, uint param1, uint param2)
            {
                m_CommandList.Add(new GXCommand(_cmd, new uint[] { param1, param2 }));
            }

            public void AddVertexCommand(Vector4 _vtx, Vector4 _prev)
            {
                if (_prev.W == 12345678f)
                {
                    AddVertexCommand(_vtx);
                    return;
                }

                Vector4 vtx = Vector4.Multiply(_vtx, 4096f);
                Vector4 prev = Vector4.Multiply(_prev, 4096f);

                if (Math.Abs(vtx.X - prev.X) < 1f)
                {
                    uint param = (uint)(((ushort)(short)vtx.Y) | (((ushort)(short)vtx.Z) << 16));
                    AddCommand(0x27, param);
                }
                else if (Math.Abs(vtx.Y - prev.Y) < 1f)
                {
                    uint param = (uint)(((ushort)(short)vtx.X) | (((ushort)(short)vtx.Z) << 16));
                    AddCommand(0x26, param);
                }
                else if (Math.Abs(vtx.Z - prev.Z) < 1f)
                {
                    uint param = (uint)(((ushort)(short)vtx.X) | (((ushort)(short)vtx.Y) << 16));
                    AddCommand(0x25, param);
                }
                else
                    AddVertexCommand(_vtx);
            }
            public void AddVertexCommand(Vector4 _vtx)
            {
                Vector4 vtx = Vector4.Multiply(_vtx, 4096f);

                uint x = (uint)vtx.X;
                uint y = (uint)vtx.Y;
                uint z = (uint)vtx.Z;
                if (((x & 0x3F) == 0) && ((y & 0x3F) == 0) && ((z & 0x3F) == 0))
                {
                    uint param = (uint)((((ushort)(short)x) >> 6) | (((ushort)(short)y) << 4) | (((ushort)(short)z) << 14));
                    AddCommand(0x24, param);
                }
                else
                {
                    uint param1 = (uint)(((ushort)(short)x) | (((ushort)(short)y) << 16));
                    uint param2 = (uint)((ushort)(short)z);
                    AddCommand(0x23, param1, param2);
                }
            }

            public void AddTexCoordCommand(Vector2 txc)
            {
                short s = (short)(txc.X * 16);
                short t = (short)(txc.Y * 16);
                uint param = (uint)(((ushort)s) | ((ushort)t) << 16);
                AddCommand(0x22, param);
            }

            public void AddColorCommand(Color color)
            {
                AddCommand(0x20, Helper.ColorToBGR15(color));
            }

            public void ClearCommands()
            {
                m_CommandList.Clear();
            }

            public byte[] GetDisplayList()
            {
                List<byte> ret = new List<byte>();
                int numcmd = (m_CommandList.Count + 3) & ~3;

                for (int i = m_CommandList.Count; i < numcmd; i++)
                    AddCommand(0x00);

                for (int i = 0; i < numcmd; i += 4)
                {
                    for (int j = 0; j < 4; j++)
                        ret.Add(m_CommandList[i + j].m_Command);

                    for (int j = 0; j < 4; j++)
                    {
                        foreach (uint param in m_CommandList[i + j].m_Parameters)
                        {
                            ret.Add((byte)(param & 0xFF));
                            ret.Add((byte)((param >> 8) & 0xFF));
                            ret.Add((byte)((param >> 16) & 0xFF));
                            ret.Add((byte)(param >> 24));
                        }
                    }
                }

                return ret.ToArray();
            }


            public struct GXCommand
            {
                public byte m_Command;
                public uint[] m_Parameters;

                public GXCommand(byte _cmd, uint[] _params)
                {
                    m_Command = _cmd;
                    m_Parameters = _params;
                }
            }

            public List<GXCommand> m_CommandList;
        }

        public class ConvertedTexture
        {
            public ConvertedTexture(uint dstp, byte[] tex, byte[] pal, string tn, string pn)
            {
                m_DSTexParam = dstp;
                m_TextureData = tex;
                m_PaletteData = pal;
                m_TextureName = tn;
                m_PaletteName = pn;
                m_TexType = (dstp >> 26) & 0x7;

                m_TextureDataLength = m_TextureData.Length;
                if ((dstp & 0x1C000000) == 0x14000000)
                    m_TextureDataLength -= (m_TextureDataLength / 3);
            }

            public uint m_DSTexParam;
            public byte[] m_TextureData;
            public byte[] m_PaletteData;
            public int m_TextureDataLength;
            public string m_TextureName;
            public string m_PaletteName;
            public uint m_TextureID, m_PaletteID;
            public uint m_TexType;
        }

        public class Palette
        {
            private static int ColorComparer(ushort c1, ushort c2)
            {
                int r1 = c1 & 0x1F;
                int g1 = (c1 >> 5) & 0x1F;
                int b1 = (c1 >> 10) & 0x1F;
                int r2 = c2 & 0x1F;
                int g2 = (c2 >> 5) & 0x1F;
                int b2 = (c2 >> 10) & 0x1F;

                int tdiff = (r2 - r1) + (g2 - g1) + (b2 - b1);
                if (tdiff == 0)
                    return 0;
                else if (tdiff < 0)
                    return 1;
                else
                    return -1;
            }

            public Palette(Bitmap bmp, Rectangle region, int depth)
            {
                List<ushort> pal = new List<ushort>(depth);

                // 1. get the colors used within the requested region
                for (int y = region.Top; y < region.Bottom; y++)
                {
                    for (int x = region.Left; x < region.Right; x++)
                    {
                        ushort col15 = Helper.ColorToBGR15(bmp.GetPixel(x, y));
                        if (!pal.Contains(col15))
                            pal.Add(col15);
                    }
                }

                // 2. shrink down the palette by removing colors that
                // are close to others, until it fits within the
                // requested size
                pal.Sort(Palette.ColorComparer);
                int maxdiff = 1;
                while (pal.Count > depth)
                {
                    for (int i = 1; i < pal.Count; )
                    {
                        ushort c1 = pal[i - 1];
                        ushort c2 = pal[i];

                        int r1 = c1 & 0x1F;
                        int g1 = (c1 >> 5) & 0x1F;
                        int b1 = (c1 >> 10) & 0x1F;
                        int r2 = c2 & 0x1F;
                        int g2 = (c2 >> 5) & 0x1F;
                        int b2 = (c2 >> 10) & 0x1F;

                        if (Math.Abs(r1 - r2) <= maxdiff && Math.Abs(g1 - g2) <= maxdiff && Math.Abs(b1 - b2) <= maxdiff)
                        {
                            ushort cmerged = Helper.BlendColorsBGR15(c1, 1, c2, 1);
                            pal[i - 1] = cmerged;
                            pal.RemoveAt(i);
                        }
                        else
                            i++;
                    }

                    maxdiff++;
                }

                m_Palette = pal;
                m_Referenced = new bool[m_Palette.Count];
                for (int i = 0; i < m_Palette.Count; i++)
                    m_Referenced[i] = false;
            }

            public int FindClosestColorID(ushort c)
            {
                int r = c & 0x1F;
                int g = (c >> 5) & 0x1F;
                int b = (c >> 10) & 0x1F;

                int maxdiff = 1;

                for (; ; )
                {
                    for (int i = 0; i < m_Palette.Count; i++)
                    {
                        ushort c1 = m_Palette[i];
                        int r1 = c1 & 0x1F;
                        int g1 = (c1 >> 5) & 0x1F;
                        int b1 = (c1 >> 10) & 0x1F;

                        if (Math.Abs(r1 - r) <= maxdiff && Math.Abs(g1 - g) <= maxdiff && Math.Abs(b1 - b) <= maxdiff)
                        {
                            m_Referenced[i] = true;
                            return i;
                        }
                    }

                    maxdiff++;
                }
            }

            public static bool AreSimilar(Palette p1, Palette p2)
            {
                if (p1.m_Palette.Count > p2.m_Palette.Count)
                    return false;

                for (int i = 0; i < p1.m_Palette.Count; i++)
                {
                    ushort c1 = p1.m_Palette[i];
                    ushort c2 = p2.m_Palette[i];

                    int r1 = c1 & 0x1F;
                    int g1 = (c1 >> 5) & 0x1F;
                    int b1 = (c1 >> 10) & 0x1F;
                    int r2 = c2 & 0x1F;
                    int g2 = (c2 >> 5) & 0x1F;
                    int b2 = (c2 >> 10) & 0x1F;

                    if (Math.Abs(r1 - r2) > 1 || Math.Abs(g1 - g2) > 1 || Math.Abs(b1 - b2) > 1)
                        return false;
                }

                return true;
            }

            public List<ushort> m_Palette;
            public bool[] m_Referenced;
        }


        public class FaceDef
        {
            public int[] m_VtxIndices;
            public int[] m_TxcIndices;
            public int[] m_NrmIndices;
            public int[] m_ColIndices;
            public int[] m_BoneIDs;
            public string m_MatName;
        }

        public class MaterialDef
        {
            public List<FaceDef> m_Faces;

            public int m_ID;
            public string m_Name;

            public Color m_DiffuseColor;
            public int m_Opacity;

            public bool m_HasTextures;

            public int m_ColType;

            //public byte[] m_DiffuseMap;
            public string m_DiffuseMapName;
            public int m_DiffuseMapID;
            public Vector2 m_DiffuseMapSize;

            // haxx
            public float m_TexCoordScale;

            public MaterialDef copyAllButFaces()
            {
                MaterialDef temp = new MaterialDef();
                temp.m_ColType = m_ColType;
                temp.m_DiffuseColor = Color.FromArgb(m_DiffuseColor.A, m_DiffuseColor.R, m_DiffuseColor.G, m_DiffuseColor.B);
                temp.m_DiffuseMapID = m_DiffuseMapID;
                temp.m_DiffuseMapName = m_DiffuseMapName;
                temp.m_DiffuseMapSize = new Vector2(m_DiffuseMapSize.X, m_DiffuseMapSize.Y);
                temp.m_Faces = new List<FaceDef>();
                temp.m_HasTextures = m_HasTextures;
                temp.m_ID = m_ID;
                temp.m_Name = m_Name;
                temp.m_Opacity = m_Opacity;
                temp.m_TexCoordScale = m_TexCoordScale;

                return temp;
            }
        }

        public static ConvertedTexture ConvertTexture(string filename)
        {
            Bitmap bmp = new Bitmap(filename);

            int width = 8, height = 8;
            int dswidth = 0, dsheight = 0;
            while (width < bmp.Width) { width *= 2; dswidth++; }
            while (height < bmp.Height) { height *= 2; dsheight++; }

            // cheap resizing for textures whose dimensions aren't power-of-two
            if ((width != bmp.Width) || (height != bmp.Height))
            {
                Bitmap newbmp = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(newbmp);
                g.DrawImage(bmp, new Rectangle(0, 0, width, height));
                bmp = newbmp;
            }

            bool alpha = false;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int a = bmp.GetPixel(x, y).A;
                    if (a >= 8 && a <= 248)
                    {
                        alpha = true;
                        break;
                    }
                }
            }

            int textype = 0;
            byte[] tex = null;
            byte[] pal = null;

            if (alpha)
            {
                // a5i3/a3i5
                tex = new byte[width * height];
                Palette _pal = new Palette(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 32);
                int alphamask = 0;

                if (_pal.m_Palette.Count <= 8)
                {
                    textype = 6;
                    alphamask = 0xF8;
                }
                else
                {
                    textype = 1;
                    alphamask = 0xE0;
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        ushort bgr15 = Helper.ColorToBGR15(c);
                        int a = c.A & alphamask;

                        byte val = (byte)(_pal.FindClosestColorID(bgr15) | a);
                        tex[(y * width) + x] = val;
                    }
                }

                pal = new byte[_pal.m_Palette.Count * 2];
                for (int i = 0; i < _pal.m_Palette.Count; i++)
                {
                    pal[i * 2] = (byte)(_pal.m_Palette[i] & 0xFF);
                    pal[(i * 2) + 1] = (byte)(_pal.m_Palette[i] >> 8);
                }
            }
            else
            {
                // type5 - compressed
                textype = 5;
                tex = new byte[((width * height) / 16) * 6];
                List<Palette> pallist = new List<Palette>();
                List<ushort> paldata = new List<ushort>();

                int texoffset = 0;
                int palidxoffset = ((width * height) / 16) * 4;

                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        bool transp = false;

                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 4; x2++)
                            {
                                Color c = bmp.GetPixel(x + x2, y + y2);

                                if (c.A < 8)
                                    transp = true;
                            }
                        }

                        Palette txpal = new Palette(bmp, new Rectangle(x, y, 4, 4), transp ? 3 : 4);
                        uint texel = 0;
                        ushort palidx = (ushort)(transp ? 0x0000 : 0x8000);

                        for (int y2 = 0; y2 < 4; y2++)
                        {
                            for (int x2 = 0; x2 < 4; x2++)
                            {
                                int px = 0;
                                Color c = bmp.GetPixel(x + x2, y + y2);
                                ushort bgr15 = Helper.ColorToBGR15(c);

                                if (transp && c.A < 8)
                                    px = 3;
                                else
                                    px = txpal.FindClosestColorID(bgr15);

                                texel |= (uint)(px << ((2 * x2) + (8 * y2)));
                            }
                        }

                        uint paloffset = 0; bool palfound = false;
                        for (int i = 0; i < pallist.Count; i++)
                        {
                            if (Palette.AreSimilar(txpal, pallist[i]))
                            {
                                palfound = true;
                                break;
                            }

                            paloffset += (uint)pallist[i].m_Palette.Count;
                            if ((paloffset & 1) != 0) paloffset++;
                        }

                        paloffset /= 2;
                        palidx |= (ushort)(paloffset & 0x3FFF);

                        if (!palfound)
                        {
                            pallist.Add(txpal);

                            foreach (ushort col in txpal.m_Palette)
                                paldata.Add(col);
                            if ((paldata.Count & 1) != 0)
                                paldata.Add(0x7C1F);
                        }

                        tex[texoffset] = (byte)(texel & 0xFF);
                        tex[texoffset + 1] = (byte)((texel >> 8) & 0xFF);
                        tex[texoffset + 2] = (byte)((texel >> 16) & 0xFF);
                        tex[texoffset + 3] = (byte)(texel >> 24);
                        texoffset += 4;
                        tex[palidxoffset] = (byte)(palidx & 0xFF);
                        tex[palidxoffset + 1] = (byte)(palidx >> 8);
                        palidxoffset += 2;
                    }
                }

                pal = new byte[paldata.Count * 2];
                for (int i = 0; i < paldata.Count; i++)
                {
                    pal[i * 2] = (byte)(paldata[i] & 0xFF);
                    pal[(i * 2) + 1] = (byte)(paldata[i] >> 8);
                }
            }

            string path_separator = (filename.Contains("/")) ? "/" : "\\";
            string texname = filename.Substring(filename.LastIndexOf(path_separator) + 1).Replace('.', '_');
            string palname = (pal == null) ? null : texname + "_pl";

            uint dstp = (uint)((dswidth << 20) | (dsheight << 23) | (textype << 26));
            return new ConvertedTexture(dstp, tex, pal, texname, palname);
        }

        // from loaded model
        public List<Vector4> m_Vertices;
        public List<Vector2> m_TexCoords;
        public List<Vector3> m_Normals;
        public List<Color> m_Colours;
        // List of Bone ID's for each vertex of each, by default set to index of current <geometry>, 
        // replaced later with vertex weight values from skinning information if present
        public List<int> m_VertexBoneIDs;
        public Dictionary<string, MaterialDef> m_Materials;
        public Dictionary<string, MaterialDef> m_Textures;
        public Dictionary<string, BoneForImport> m_Bones;
        public bool m_SketchupHack;

        protected CultureInfo USA = Helper.USA;

        // hash
        protected MD5CryptoServiceProvider m_MD5;

        protected bool m_ZMirror = false;
        protected bool m_SwapYZ = false;

        protected String m_ModelFileName;
        protected String m_ModelPath;

        protected BMD m_ImportedModel;
        protected BCA m_ImportedAnimation;

        // Temporary objects used during reading
        protected List<float> currentVertices;
        protected List<float> currentTexCoords;
        protected List<float> currentNormals;
        protected List<float> currentColours;
        protected int[] geometryVertexOffsets;// Used to get correct index into m_Vertices in skinning section
        protected string currentBone;
        protected int currentBoneID;
        protected string curmaterial;
        protected int vertexIndex = -1;
        protected int normalIndex = -1;
        protected int texCoordIndex = -1;
        protected int colourIndex = -1;
        protected int[] vtxColourStrideAndRGBOffsets;

        protected void AddWhiteMat()
        {
            if (m_Materials.ContainsKey("default_white"))
                return;
            MaterialDef mat = new MaterialDef();
            mat.m_ID = m_Materials.Count;
            mat.m_Faces = new List<FaceDef>();
            mat.m_DiffuseColor = Color.White;
            mat.m_Opacity = 255;
            mat.m_HasTextures = false;
            mat.m_DiffuseMapName = "";
            mat.m_DiffuseMapID = 0;
            mat.m_DiffuseMapSize = new Vector2(0f, 0f);
            mat.m_Name = "default_white";
            m_Materials.Add("default_white", mat);
        }

        protected void AddWhiteMat(String bone)
        {
            AddWhiteMat();
            if (!m_Bones[m_Bones[bone].m_RootBone].m_Materials.ContainsKey("default_white"))
                m_Bones[m_Bones[bone].m_RootBone].m_Materials.Add("default_white", m_Materials["default_white"].copyAllButFaces());
            if (!m_Bones[bone].m_Materials.ContainsKey("default_white"))
                m_Bones[bone].m_Materials.Add("default_white", m_Materials["default_white"].copyAllButFaces());
        }

        protected String GetClosestMaterialMatch(String mat)
        {
            String matName = mat;

            if (m_Materials.ContainsKey(matName))
                return mat;
            else
            {
                int matNameLength = mat.Length;
                int i = 0;
                while (i < matNameLength)
                {
                    if (!m_Materials.ContainsKey(matName))
                        matName = matName.Substring(0, matName.Length - 1);
                    else
                        return matName;

                    i++;
                }
            }
            // Shouldn't reach
            return mat;
        }

        public static BMD_Importer_Base GetModelImporter(String modelFileName)
        {
            BMD_Importer_Base importer = null;

            string modelFormat = modelFileName.Substring(modelFileName.Length - 3, 3).ToLower();

            switch (modelFormat)
            {
                case "obj":
                    importer = new BMD_Importer_OBJ();
                    break;
                case "dae":
                    importer = new BMD_BCA_Importer_DAE();
                    break;
                default:
                    importer = new BMD_Importer_OBJ();
                    break;
            }

            return importer;
        }

        public BMD ConvertToBMD(BMD model, String modelFileName, Vector3 scale, bool save = true)
        {
            BMD importedModel = null;

            try
            {
                importedModel = Import(model, modelFileName, scale);
            }
            catch (Exception e) 
            { 
                MessageBox.Show("An error occured importing your model: \n\n" + e.Message + "\n" + e.StackTrace);
                return new BMD(Program.m_ROM.GetFileFromName(model.m_FileName));// reload orginal model
            }

            if (save)
                importedModel.m_File.SaveChanges();

            return importedModel;
        }

        public int ConvertToBMDAndBCA(ref BMD model, ref BCA animation, string modelFileName)
        {
            string modelPath = Path.GetDirectoryName(modelFileName);

            //try
            {
                Import(model, modelFileName, new Vector3(1f, 1f, 1f));
                ImportAnimation(animation, modelFileName);
            }
            /*catch (Exception e) 
            { 
                MessageBox.Show("An error occured importing your model and/or animation: \n\n" + 
                e.Message + "\n" + e.StackTrace);
                return -1;
            }*/

            //model.m_File.SaveChanges();
            //animation.m_File.SaveChanges();

            return 0;
        }

        protected virtual BMD Import(BMD model, String modelFileName, Vector3 scale) { return null; }
        protected virtual BCA ImportAnimation(BCA animation, string modelFileName) { return null; }

        protected void GenerateBMD(Vector3 m_Scale, bool save)
        {
            int b = 0;
            GXDisplayListPacker dlpacker = new GXDisplayListPacker();

            NitroFile bmd = m_ImportedModel.m_File;
            bmd.Clear();

            // Because transformations were applied when exporting, we need to reverse them so that when we set the bones' 
            // transforms, the transforms aren't applied again
            ApplyInverseTransformations();

            Vector4[] scaledvtxs = new Vector4[m_Vertices.Count];
            for (int i = 0; i < m_Vertices.Count; i++)
            {
                if (m_SwapYZ)
                    scaledvtxs[i] = new Vector4(m_Vertices[i].X * m_Scale.X, m_Vertices[i].Z * m_Scale.Y, m_Vertices[i].Y * (m_ZMirror ? -m_Scale.Z : m_Scale.Z), m_Vertices[i].W);
                else
                    scaledvtxs[i] = new Vector4(m_Vertices[i].X * m_Scale.X, m_Vertices[i].Y * m_Scale.Y, m_Vertices[i].Z * (m_ZMirror ? -m_Scale.Z : m_Scale.Z), m_Vertices[i].W);
            }

            float largest = 0f;
            foreach (Vector4 vec in scaledvtxs)
            {
                if (vec.X > largest) largest = vec.X;
                if (vec.Y > largest) largest = vec.Y;
                if (vec.Z > largest) largest = vec.Z;

                if (-vec.X > largest) largest = -vec.X;
                if (-vec.Y > largest) largest = -vec.Y;
                if (-vec.Z > largest) largest = -vec.Z;
            }

            float scale = 1f; uint scaleval = 0;
            while (largest > (32767f / 4096f))
            {
                scaleval++;
                scale /= 2f;
                largest /= 2f;
            }

            if (scaleval > 31)
            {
                MessageBox.Show("Your model is too large to be imported. Try scaling it down.", Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < scaledvtxs.Length; i++)
            {
                scaledvtxs[i].X *= scale;
                scaledvtxs[i].Y *= scale;
                scaledvtxs[i].Z *= scale;
            }

            Dictionary<string, ConvertedTexture> textures = new Dictionary<string, ConvertedTexture>();
            uint ntex = 0, npal = 0;
            int texsize = 0;
            foreach (KeyValuePair<string, MaterialDef> _mat in m_Materials)
            {
                MaterialDef mat = _mat.Value;
                string matname = _mat.Key;

                if (mat.m_DiffuseMapName != "")
                {
                    if (!textures.ContainsKey(mat.m_DiffuseMapName))
                    {
                        ConvertedTexture tex = ConvertTexture(m_ModelPath + Path.DirectorySeparatorChar + mat.m_DiffuseMapName);
                        tex.m_TextureID = ntex;
                        tex.m_PaletteID = npal;
                        if (tex.m_TextureData != null) { ntex++; texsize += tex.m_TextureData.Length; }
                        if (tex.m_PaletteData != null) { npal++; texsize += tex.m_PaletteData.Length; }
                        textures.Add(mat.m_DiffuseMapName, tex);
                    }
                }
            }

            if (texsize >= 49152)
            {
                if (MessageBox.Show("Your textures would occupy more than 48k of VRAM.\nThis could cause glitches or freezes.\n\nImport anyway?",
                    Program.AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    return;
            }

            bmd.Write32(0x00, scaleval);

            uint curoffset = 0x3C;
            bmd.Write32(0x0C, (uint)m_Materials.Count);
            bmd.Write32(0x10, curoffset);

            uint dllistoffset = curoffset;
            curoffset += (uint)(m_Materials.Count * 8);

            int lastColourARGB = -1;

            // build display lists
            b = 0;
            foreach (MaterialDef mat in m_Materials.Values)
            {
                bmd.Write32(dllistoffset, 1);
                bmd.Write32(dllistoffset + 0x4, curoffset);
                dllistoffset += 0x8;

                Vector2 tcscale = mat.m_DiffuseMapSize;

                string curmaterial = mat.m_Name;

                // For each bone including the current parent bone and all its child bones, for each material 
                // definition that matches the name of the current material, for each face using that material ...
                float largesttc = 0f;
                foreach (BoneForImport bone in m_Bones.Values)
                {
                    for (int j = 0; j < bone.m_Materials.Count; j++)
                    {
                        if (!bone.m_Materials.Values.ElementAt(j).m_Name.Equals(curmaterial))
                            continue;
                        foreach (FaceDef face in bone.m_Materials.Values.ElementAt(j).m_Faces)
                        {
                            foreach (int txci in face.m_TxcIndices)
                            {
                                if (txci < 0) continue;
                                Vector2 txc = Vector2.Multiply(m_TexCoords[txci], tcscale);
                                if (Math.Abs(txc.X) > largesttc) largesttc = Math.Abs(txc.X);
                                if (Math.Abs(txc.Y) > largesttc) largesttc = Math.Abs(txc.Y);
                            }
                        }
                    }
                }
                float _tcscale = largesttc / (32767f / 16f);
                if (_tcscale > 1f)
                {
                    _tcscale = (float)Math.Ceiling(_tcscale * 4096f) / 4096f;
                    mat.m_TexCoordScale = _tcscale;
                    Vector2.Divide(ref tcscale, _tcscale, out tcscale);
                }
                else
                    mat.m_TexCoordScale = 0f;

                dlpacker.ClearCommands();
                int lastface = -1;
                int lastmatrix = -1;
                Vector4 lastvtx = new Vector4(0f, 0f, 0f, 12345678f);
                for (int j = 0; j < m_Bones.Values.Count; j++)
                {
                    BoneForImport bone = m_Bones.Values.ElementAt(j);
                    for (int k = 0; k < bone.m_Materials.Count; k++)
                    {
                        if (!bone.m_Materials.Values.ElementAt(k).m_Name.Equals(curmaterial))
                            continue;
                        foreach (FaceDef face in bone.m_Materials.Values.ElementAt(k).m_Faces)
                        {
                            int nvtx = face.m_VtxIndices.Length;

                            if (nvtx != lastface || lastface > 4)
                            {
                                uint vtxtype = 0;
                                switch (nvtx)
                                {
                                    case 1:
                                    case 2:
                                    case 3: vtxtype = 0; break;
                                    case 4: vtxtype = 1; break;
                                    default: vtxtype = 2; break;
                                }

                                if (lastface != -1) dlpacker.AddCommand(0x41);// End Vertex List

                                dlpacker.AddCommand(0x40, vtxtype);// Begin Vertex List

                                lastface = nvtx;
                            }

                            switch (nvtx)
                            {
                                case 1: // point
                                    {
                                        Vector4 vtx = scaledvtxs[face.m_VtxIndices[0]];
                                        int txc = face.m_TxcIndices[0];

                                        if (lastmatrix != face.m_BoneIDs[0])
                                        {
                                            dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[0]);// Matrix Restore ID for current vertex
                                            lastmatrix = face.m_BoneIDs[0];
                                        }
                                        if (face.m_ColIndices[0] > -1 && m_Colours[face.m_ColIndices[0]].ToArgb() != lastColourARGB)
                                        {
                                            dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[0]]);
                                            lastColourARGB = m_Colours[face.m_ColIndices[0]].ToArgb();
                                        }
                                        if (txc > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc], tcscale));
                                        if (face.m_ColIndices[0] > -1) dlpacker.AddVertexCommand(vtx, lastvtx);
                                        if (txc > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc], tcscale));
                                        if (face.m_ColIndices[0] > -1) dlpacker.AddVertexCommand(vtx, vtx);
                                        if (txc > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc], tcscale));
                                        dlpacker.AddVertexCommand(vtx, vtx);
                                        lastvtx = vtx;
                                    }
                                    break;

                                case 2: // line
                                    {
                                        Vector4 vtx1 = scaledvtxs[face.m_VtxIndices[0]];
                                        int txc1 = face.m_TxcIndices[0];
                                        Vector4 vtx2 = scaledvtxs[face.m_VtxIndices[1]];
                                        int txc2 = face.m_TxcIndices[1];

                                        if (lastmatrix != face.m_BoneIDs[0])
                                        {
                                            dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[0]);// Matrix Restore ID for current vertex
                                            lastmatrix = face.m_BoneIDs[0];
                                        }
                                        if (face.m_ColIndices[0] > -1 && m_Colours[face.m_ColIndices[0]].ToArgb() != lastColourARGB)
                                        {
                                            dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[0]]);
                                            lastColourARGB = m_Colours[face.m_ColIndices[0]].ToArgb();
                                        }
                                        if (txc1 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc1], tcscale));
                                        dlpacker.AddVertexCommand(vtx1, lastvtx);
                                        if (lastmatrix != face.m_BoneIDs[1])
                                        {
                                            dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[1]);// Matrix Restore ID for current vertex
                                            lastmatrix = face.m_BoneIDs[1];
                                        }
                                        if (face.m_ColIndices[1] > -1 && m_Colours[face.m_ColIndices[1]].ToArgb() != lastColourARGB)
                                        {
                                            dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[1]]);
                                            lastColourARGB = m_Colours[face.m_ColIndices[1]].ToArgb();
                                        }
                                        if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                        dlpacker.AddVertexCommand(vtx2, vtx1);
                                        if (face.m_ColIndices[2] > -1) dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[2]]);
                                        if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                        dlpacker.AddVertexCommand(vtx2, vtx2);
                                        lastvtx = vtx2;
                                    }
                                    break;

                                case 3: // triangle
                                    {
                                        Vector4 vtx1 = scaledvtxs[face.m_VtxIndices[0]];
                                        int txc1 = face.m_TxcIndices[0];
                                        Vector4 vtx2 = scaledvtxs[face.m_VtxIndices[1]];
                                        int txc2 = face.m_TxcIndices[1];
                                        Vector4 vtx3 = scaledvtxs[face.m_VtxIndices[2]];
                                        int txc3 = face.m_TxcIndices[2];

                                        if (lastmatrix != face.m_BoneIDs[0])
                                        {
                                            dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[0]);// Matrix Restore ID for current vertex
                                            lastmatrix = face.m_BoneIDs[0];
                                        }
                                        if (face.m_ColIndices[0] > -1 && m_Colours[face.m_ColIndices[0]].ToArgb() != lastColourARGB)
                                        {
                                            dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[0]]);
                                            lastColourARGB = m_Colours[face.m_ColIndices[0]].ToArgb();
                                        }
                                        if (txc1 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc1], tcscale));
                                        dlpacker.AddVertexCommand(vtx1, lastvtx);
                                        if (m_ZMirror)
                                        {
                                            if (lastmatrix != face.m_BoneIDs[2])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[2]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[2];
                                            }
                                            if (face.m_ColIndices[2] > -1 && m_Colours[face.m_ColIndices[2]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[2]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[2]].ToArgb();
                                            }
                                            if (txc3 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc3], tcscale));
                                            dlpacker.AddVertexCommand(vtx3, vtx1);
                                            if (lastmatrix != face.m_BoneIDs[1])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[1]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[1];
                                            }
                                            if (face.m_ColIndices[1] > -1 && m_Colours[face.m_ColIndices[1]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[1]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[1]].ToArgb();
                                            }
                                            if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                            dlpacker.AddVertexCommand(vtx2, vtx3);
                                            lastvtx = vtx2;
                                        }
                                        else
                                        {
                                            if (lastmatrix != face.m_BoneIDs[1])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[1]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[1];
                                            }
                                            if (face.m_ColIndices[1] > -1 && m_Colours[face.m_ColIndices[1]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[1]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[1]].ToArgb();
                                            }
                                            if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                            dlpacker.AddVertexCommand(vtx2, vtx1);
                                            if (lastmatrix != face.m_BoneIDs[2])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[2]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[2];
                                            }
                                            if (face.m_ColIndices[2] > -1 && m_Colours[face.m_ColIndices[2]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[2]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[2]].ToArgb();
                                            }
                                            if (txc3 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc3], tcscale));
                                            dlpacker.AddVertexCommand(vtx3, vtx2);
                                            lastvtx = vtx3;
                                        }
                                    }
                                    break;

                                case 4: // quad
                                    {
                                        Vector4 vtx1 = scaledvtxs[face.m_VtxIndices[0]];
                                        int txc1 = face.m_TxcIndices[0];
                                        Vector4 vtx2 = scaledvtxs[face.m_VtxIndices[1]];
                                        int txc2 = face.m_TxcIndices[1];
                                        Vector4 vtx3 = scaledvtxs[face.m_VtxIndices[2]];
                                        int txc3 = face.m_TxcIndices[2];
                                        Vector4 vtx4 = scaledvtxs[face.m_VtxIndices[3]];
                                        int txc4 = face.m_TxcIndices[3];

                                        if (lastmatrix != face.m_BoneIDs[0])
                                        {
                                            dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[0]);// Matrix Restore ID for current vertex
                                            lastmatrix = face.m_BoneIDs[0];
                                        }
                                        if (face.m_ColIndices[0] > -1 && m_Colours[face.m_ColIndices[0]].ToArgb() != lastColourARGB)
                                        {
                                            dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[0]]);
                                            lastColourARGB = m_Colours[face.m_ColIndices[0]].ToArgb();
                                        }
                                        if (txc1 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc1], tcscale));
                                        dlpacker.AddVertexCommand(vtx1, lastvtx);
                                        if (m_ZMirror)
                                        {
                                            if (lastmatrix != face.m_BoneIDs[3])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[3]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[3];
                                            }
                                            if (face.m_ColIndices[3] > -1 && m_Colours[face.m_ColIndices[3]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[3]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[3]].ToArgb();
                                            }
                                            if (txc4 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc4], tcscale));
                                            dlpacker.AddVertexCommand(vtx4, vtx1);
                                            if (lastmatrix != face.m_BoneIDs[2])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[2]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[2];
                                            }
                                            if (face.m_ColIndices[2] > -1 && m_Colours[face.m_ColIndices[2]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[2]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[2]].ToArgb();
                                            }
                                            if (txc3 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc3], tcscale));
                                            dlpacker.AddVertexCommand(vtx3, vtx4);
                                            if (lastmatrix != face.m_BoneIDs[1])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[1]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[1];
                                            }
                                            if (face.m_ColIndices[1] > -1 && m_Colours[face.m_ColIndices[1]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[1]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[1]].ToArgb();
                                            }
                                            if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                            dlpacker.AddVertexCommand(vtx2, vtx3);
                                            lastvtx = vtx2;
                                        }
                                        else
                                        {
                                            if (lastmatrix != face.m_BoneIDs[1])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[1]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[1];
                                            }
                                            if (face.m_ColIndices[1] > -1 && m_Colours[face.m_ColIndices[1]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[1]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[1]].ToArgb();
                                            }
                                            if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                            dlpacker.AddVertexCommand(vtx2, vtx1);
                                            if (lastmatrix != face.m_BoneIDs[2])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[2]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[2];
                                            }
                                            if (face.m_ColIndices[2] > -1 && m_Colours[face.m_ColIndices[2]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[2]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[2]].ToArgb();
                                            }
                                            if (txc3 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc3], tcscale));
                                            dlpacker.AddVertexCommand(vtx3, vtx2);
                                            if (lastmatrix != face.m_BoneIDs[3])
                                            {
                                                dlpacker.AddCommand(0x14, (uint)face.m_BoneIDs[3]);// Matrix Restore ID for current vertex
                                                lastmatrix = face.m_BoneIDs[3];
                                            }
                                            if (face.m_ColIndices[3] > -1 && m_Colours[face.m_ColIndices[3]].ToArgb() != lastColourARGB)
                                            {
                                                dlpacker.AddColorCommand(m_Colours[face.m_ColIndices[3]]);
                                                lastColourARGB = m_Colours[face.m_ColIndices[3]].ToArgb();
                                            }
                                            if (txc4 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc4], tcscale));
                                            dlpacker.AddVertexCommand(vtx4, vtx3);
                                            lastvtx = vtx4;
                                        }
                                    }
                                    break;

                                default: // whatever (import as triangle strip)
                                    {
                                        // todo
                                    }
                                    break;
                            }
                        }
                    }
                }
                dlpacker.AddCommand(0x41);
                byte[] dlist = dlpacker.GetDisplayList();

                // Display list header
                uint dllist_data_offset = (uint)(((curoffset + 0x10 + (m_Bones.Count)) + 3) & ~3);
                bmd.Write32(curoffset, (uint)m_Bones.Count);// Number of transforms
                bmd.Write32(curoffset + 0x4, curoffset + 0x10);// Offset to transforms list
                bmd.Write32(curoffset + 0x8, (uint)dlist.Length);// Size of the display list data in bytes
                bmd.Write32(curoffset + 0xC, dllist_data_offset);// Offset to the display list data, make room for transforms list
                curoffset += 0x10;

                /* The transforms list is a series of bytes.
                 * Every time a Matrix restore (0x14) command is issued by the display list, the command's parameter (the matrix ID)
                 * is used as an index into the transforms list. The ID obtained from the transforms list is then used as an index into
                 * the transform/bone map (series of shorts whose offset is defined in the file header, at 0x2C). The ID finally obtained
                 * is the ID of the bone whose transform matrix will be used to transform oncoming geometry.
                 */
                for (int j = 0; j < m_Bones.Count; j++)
                {
                    bmd.Write8(curoffset, (byte)j);
                    curoffset += 0x1;
                }

                curoffset = dllist_data_offset;

                bmd.WriteBlock(curoffset, dlist);
                curoffset += (uint)dlist.Length;

                b++;
            }

            bmd.Write32(0x2C, curoffset);
            // transform / bone map
            for (int j = 0; j < m_Bones.Count; j++)
            {
                bmd.Write16(curoffset, (ushort)j);
                curoffset += 2;
            }
            curoffset = (uint)((curoffset + 3) & ~3);

            // build bones
            bmd.Write32(0x4, (uint)m_Bones.Count);// Number of bones
            bmd.Write32(0x8, curoffset);

            uint bextraoffset = (uint)(curoffset + (0x40 * m_Bones.Count));

            for (int boneID = 0; boneID < m_Bones.Count; boneID++)
            {
                bmd.Write32(curoffset + 0x00, (uint)boneID); // bone ID
                bmd.Write16(curoffset + 0x08, (ushort)m_Bones.Values.ElementAt(boneID).m_ParentOffset);// Offset in bones to parent bone (signed 16-bit. 0=no parent, -1=parent is the previous bone, ...)
                bmd.Write16(curoffset + 0x0A, (m_Bones.Values.ElementAt(boneID).m_HasChildren) ? (ushort)1 : (ushort)0);// 1 if the bone has children, 0 otherwise
                bmd.Write32(curoffset + 0x0C, (uint)m_Bones.Values.ElementAt(boneID).m_SiblingOffset);// Offset in bones to the next sibling bone (0=bone is last child of its parent)
                bmd.Write32(curoffset + 0x10, (uint)m_Bones.Values.ElementAt(boneID).m_Scale[0]);// X scale (32-bit signed, 20:12 fixed point. Think GX command 0x1B)
                bmd.Write32(curoffset + 0x14, (uint)m_Bones.Values.ElementAt(boneID).m_Scale[1]);// Y scale
                bmd.Write32(curoffset + 0x18, (uint)m_Bones.Values.ElementAt(boneID).m_Scale[2]);// Z scale
                bmd.Write16(curoffset + 0x1C, (ushort)m_Bones.Values.ElementAt(boneID).m_Rotation[0]);// X rotation (16-bit signed, 0x0400 = 90°)
                bmd.Write16(curoffset + 0x1E, (ushort)m_Bones.Values.ElementAt(boneID).m_Rotation[1]);// Y rotation
                bmd.Write16(curoffset + 0x20, (ushort)m_Bones.Values.ElementAt(boneID).m_Rotation[2]);// Z rotation
                bmd.Write16(curoffset + 0x22, 0);// Zero (padding)
                bmd.Write32(curoffset + 0x24, (uint)m_Bones.Values.ElementAt(boneID).m_Translation[0]);// X translation (32-bit signed, 20:12 fixed point. Think GX command 0x1C)
                bmd.Write32(curoffset + 0x28, (uint)m_Bones.Values.ElementAt(boneID).m_Translation[1]);// Y translation
                bmd.Write32(curoffset + 0x2C, (uint)m_Bones.Values.ElementAt(boneID).m_Translation[2]);// Z translation
                bmd.Write32(curoffset + 0x30, (!m_Bones.Values.ElementAt(boneID).m_HasChildren &&
                    m_Bones.Values.ElementAt(boneID).m_ParentOffset != 0) ? (uint)0 :
                    (uint)m_Bones.Values.ElementAt(boneID).m_Materials.Count);// Number of displaylist/material pairs
                bmd.Write32(curoffset + 0x3C, (m_Bones.Values.ElementAt(boneID).m_Billboard) ? (uint)1 : (uint)0);// Bit0: bone is rendered facing the camera (billboard); Bit2: ???

                bmd.Write32(curoffset + 0x34, bextraoffset);// Material IDs list
                for (byte j = 0; j < m_Bones.Values.ElementAt(boneID).m_Materials.Count; j++)
                {
                    bmd.Write8(bextraoffset, (byte)m_Bones.Values.ElementAt(boneID).m_Materials.Values.ElementAt(j).m_ID);
                    bextraoffset++;
                }

                bmd.Write32(curoffset + 0x38, bextraoffset);// Displaylist IDs list
                for (byte j = 0; j < m_Bones.Values.ElementAt(boneID).m_Materials.Count; j++)
                {
                    bmd.Write8(bextraoffset, (byte)m_Bones.Values.ElementAt(boneID).m_Materials.Values.ElementAt(j).m_ID);
                    bextraoffset++;
                }

                bmd.Write32(curoffset + 0x04, bextraoffset);// Bone names (null-terminated ASCII string)
                bmd.WriteString(bextraoffset, m_Bones.Values.ElementAt(boneID).m_Name, 0);
                bextraoffset += (uint)(m_Bones.Values.ElementAt(boneID).m_Name.Length + 1);

                curoffset += 0x40;
            }
            curoffset = (uint)((bextraoffset + 3) & ~3);

            // build materials
            bmd.Write32(0x24, (uint)m_Materials.Count);
            bmd.Write32(0x28, curoffset);

            uint mextraoffset = (uint)(curoffset + (0x30 * m_Materials.Count));

            foreach (KeyValuePair<string, MaterialDef> _mat in m_Materials)
            {
                MaterialDef mat = _mat.Value;
                string matname = _mat.Key;

                uint texid = 0xFFFFFFFF, palid = 0xFFFFFFFF;
                uint texrepeat = 0;
                uint texscale = 0x00001000;
                if (textures.ContainsKey(mat.m_DiffuseMapName))
                {
                    ConvertedTexture tex = textures[mat.m_DiffuseMapName];
                    texid = tex.m_TextureID;
                    palid = (tex.m_PaletteData != null) ? tex.m_PaletteID : 0xFFFFFFFF;

                    texrepeat = 0x00030000;

                    if (mat.m_TexCoordScale > 0f)
                    {
                        texrepeat |= 0x40000000;
                        texscale = (uint)(int)(mat.m_TexCoordScale * 4096);
                    }
                }

                uint alpha = (uint)(mat.m_Opacity >> 3);
                uint polyattr = 0x00000080 | (alpha << 16);
                // if (alpha < 0x1F) polyattr |= 0x40;

                // Set material colours
                ushort diffuse_ambient = 0;
                diffuse_ambient = (ushort)(Helper.ColorToBGR15(mat.m_DiffuseColor));

                bmd.Write32(curoffset + 0x04, texid);
                bmd.Write32(curoffset + 0x08, palid);
                bmd.Write32(curoffset + 0x0C, texscale);
                bmd.Write32(curoffset + 0x10, texscale);
                bmd.Write16(curoffset + 0x14, 0x0000);
                bmd.Write16(curoffset + 0x16, 0);
                bmd.Write32(curoffset + 0x18, 0x00000000);
                bmd.Write32(curoffset + 0x1C, 0x00000000);
                bmd.Write32(curoffset + 0x20, texrepeat);
                bmd.Write32(curoffset + 0x24, polyattr);
                bmd.Write32(curoffset + 0x28, (ushort)(diffuse_ambient | 0x8000));
                bmd.Write32(curoffset + 0x2C, 0x00000000);

                bmd.Write32(curoffset + 0x00, mextraoffset);
                bmd.WriteString(mextraoffset, matname, 0);
                mextraoffset += (uint)(matname.Length + 1);

                curoffset += 0x30;
            }
            curoffset = (uint)((mextraoffset + 3) & ~3);

            uint texoffset = curoffset;
            bmd.Write32(0x14, ntex);
            bmd.Write32(0x18, texoffset);

            // Offset to texture names
            uint textraoffset = (uint)(texoffset + (0x14 * ntex));

            // Write texture entries
            foreach (ConvertedTexture tex in textures.Values)
            {
                curoffset = (uint)(texoffset + (0x14 * tex.m_TextureID));

                bmd.Write32(curoffset + 0x08, (uint)tex.m_TextureDataLength);
                bmd.Write16(curoffset + 0x0C, (ushort)(8 << (int)((tex.m_DSTexParam >> 20) & 0x7)));
                bmd.Write16(curoffset + 0x0E, (ushort)(8 << (int)((tex.m_DSTexParam >> 23) & 0x7)));
                bmd.Write32(curoffset + 0x10, tex.m_DSTexParam);

                bmd.Write32(curoffset + 0x00, textraoffset);
                bmd.WriteString(textraoffset, tex.m_TextureName, 0);
                textraoffset += (uint)(tex.m_TextureName.Length + 1);
            }
            curoffset = (uint)((textraoffset + 3) & ~3);

            uint paloffset = curoffset;
            bmd.Write32(0x1C, npal);
            bmd.Write32(0x20, paloffset);

            // Offset to palette names
            uint pextraoffset = (uint)(paloffset + (0x10 * npal));

            // Write texture palette entries
            foreach (ConvertedTexture tex in textures.Values)
            {
                if (tex.m_PaletteData == null)
                    continue;
                curoffset = (uint)(paloffset + (0x10 * tex.m_PaletteID));

                bmd.Write32(curoffset + 0x08, (uint)tex.m_PaletteData.Length);
                bmd.Write32(curoffset + 0x0C, 0xFFFFFFFF);

                bmd.Write32(curoffset + 0x00, pextraoffset);
                bmd.WriteString(pextraoffset, tex.m_PaletteName, 0);
                pextraoffset += (uint)(tex.m_PaletteName.Length + 1);
            }
            curoffset = (uint)((pextraoffset + 3) & ~3);

            // this must point to the texture data block
            bmd.Write32(0x38, curoffset);

            // Write texture and texture palette data
            foreach (ConvertedTexture tex in textures.Values)
            {
                bmd.WriteBlock(curoffset, tex.m_TextureData);
                bmd.Write32((uint)(texoffset + (0x14 * tex.m_TextureID) + 0x4), curoffset);
                curoffset += (uint)tex.m_TextureData.Length;
                curoffset = (uint)((curoffset + 3) & ~3);

                if (tex.m_PaletteData != null)
                {
                    bmd.WriteBlock(curoffset, tex.m_PaletteData);
                    bmd.Write32((uint)(paloffset + (0x10 * tex.m_PaletteID) + 0x4), curoffset);
                    curoffset += (uint)tex.m_PaletteData.Length;
                    curoffset = (uint)((curoffset + 3) & ~3);
                }
            }

            if (save)
                bmd.SaveChanges();

            m_ImportedModel = new BMD(bmd);
        }

        /*
         * Because transformations were applied when exporting, we need to reverse them so that when we set the bones' 
         * transforms, the transforms aren't applied again
         */
        protected void ApplyInverseTransformations()
        {
            // First, calculate the reverse transformation matrix for each bone
            foreach (BoneForImport bone in m_Bones.Values)
            {
                Vector3 invScale = new Vector3((float)(1f / (((int)bone.m_Scale[0]) / 4096.0f)),
                                (float)(1f / (((int)bone.m_Scale[1]) / 4096.0f)), (float)(1f / (((int)bone.m_Scale[2]) / 4096.0f)));
                Vector3 invRot = new Vector3(((float)(short)bone.m_Rotation[0] * (float)Math.PI) / -2048.0f,
                    ((float)(short)bone.m_Rotation[1] * (float)Math.PI) / -2048.0f, ((float)(short)bone.m_Rotation[2] * (float)Math.PI) / -2048.0f);
                Vector3 invTrans = new Vector3((float)(((int)bone.m_Translation[0]) / -4096.0f) * (1f),
                    (float)(((int)bone.m_Translation[1]) / -4096.0f) * (1f), (float)(((int)bone.m_Translation[2]) / -4096.0f) * (1f));

                Matrix4 ret = Matrix4.Identity;

                // If a child bone, apply its parent's transformations as well
                if (bone.m_ParentOffset < 0)
                {
                    BoneForImport parent_bone = m_Bones[bone.m_ParentBone];

                    Matrix4.Mult(ref ret, ref parent_bone.m_ReverseTransform, out ret);
                }

                Matrix4 mscale = Matrix4.CreateScale(invScale);
                Matrix4 mxrot = Matrix4.CreateRotationX(invRot.X);
                Matrix4 myrot = Matrix4.CreateRotationY(invRot.Y);
                Matrix4 mzrot = Matrix4.CreateRotationZ(invRot.Z);
                Matrix4 mtrans = Matrix4.CreateTranslation(invTrans);

                Matrix4.Mult(ref ret, ref mtrans, out ret);
                Matrix4.Mult(ref ret, ref mzrot, out ret);
                Matrix4.Mult(ref ret, ref myrot, out ret);
                Matrix4.Mult(ref ret, ref mxrot, out ret);
                Matrix4.Mult(ref ret, ref mscale, out ret);

                bone.m_ReverseTransform = ret;
            }
            // Need to maintain a list of transformed vertices to prevent the reverse transformation being 
            // applied more than once
            List<int> transformedVertices = new List<int>();
            // Now apply the vertex's bone's reverse transformation to each vertex
            foreach (BoneForImport bone in m_Bones.Values)
            {
                foreach (MaterialDef mat in bone.m_Materials.Values)
                {
                    foreach (FaceDef face in mat.m_Faces)
                    {
                        foreach (int vertexInd in face.m_VtxIndices)
                        {
                            int currentVertexBoneID = m_VertexBoneIDs[vertexInd];
                            BoneForImport currentVertexBone = m_Bones.Values.ElementAt(currentVertexBoneID);

                            // If vertex already transformed don't transform it again
                            if (transformedVertices.Contains(vertexInd))
                                continue;
                            Vector4 vert = m_Vertices.ElementAt(vertexInd);
                            Vector3 temp = new Vector3(vert.X, vert.Y, vert.Z);
                            Vector3.Transform(ref temp, ref currentVertexBone.m_ReverseTransform, out temp);

                            m_Vertices[vertexInd] = new Vector4(temp, vert.W);
                            transformedVertices.Add(vertexInd);
                        }
                    }
                }
            }
        }

        protected static string HexString(byte[] crap)
        {
            string ret = "";
            foreach (byte b in crap)
                ret += b.ToString("X2");
            return ret;
        }

        protected static int GetDictionaryStringKeyIndex(List<Object> keys, Object key)
        {
            int index = -1;
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys.ElementAt(i).Equals(key))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        protected int GetBoneIndex(String key)
        {
            return GetDictionaryStringKeyIndex(m_Bones.Keys.ToList<Object>(), key);
        }

        public class BoneForImport
        {
            public Dictionary<String, MaterialDef> m_Materials;
            public String m_Name;
            public short m_ParentOffset;
            public string m_RootBone;// Root parent bone, as opposed to a child bone that has children
            public string m_ParentBone;
            public short m_SiblingOffset;
            public bool m_HasChildren;
            public uint[] m_Scale;
            public ushort[] m_Rotation;
            public uint[] m_Translation;
            public Matrix4 m_ReverseTransform;
            public bool m_Billboard;

            public BoneForImport()
            {
                m_Materials = new Dictionary<String, MaterialDef>();
            }

            public BoneForImport(string boneName)
            {
                m_Materials = new Dictionary<String, MaterialDef>();
                m_Name = boneName;
                m_Scale = new uint[] { 0x00001000, 0x00001000, 0x00001000 };
                m_Rotation = new ushort[] { 0x0000, 0x0000, 0x0000 };
                m_Translation = new uint[] { 0, 0, 0 };
                m_Billboard = false;
            }

            public BoneForImport(String name, short parentOffset, String parentBone, String rootBone, short siblingOffset, bool hasChildren)
            {
                m_Materials = new Dictionary<String, MaterialDef>();
                m_Name = name;
                m_ParentOffset = parentOffset;
                m_ParentBone = parentBone;
                m_RootBone = rootBone;
                m_SiblingOffset = siblingOffset;
                m_HasChildren = hasChildren;
                m_Scale = new uint[] { 0x00001000, 0x00001000, 0x00001000 };
                m_Rotation = new ushort[] { 0x0000, 0x0000, 0x0000 };
                m_Translation = new uint[] { 0, 0, 0 };
                m_Billboard = false;
            }

            public BoneForImport(String name, short parentOffset, String rootBone, String parentBone, short siblingOffset, bool hasChildren,
                uint[] scale, ushort[] rotation, uint[] translation)
            {
                m_Materials = new Dictionary<String, MaterialDef>();
                m_Name = name;
                m_ParentOffset = parentOffset;
                m_ParentBone = parentBone;
                m_RootBone = rootBone;
                m_SiblingOffset = siblingOffset;
                m_HasChildren = hasChildren;
                m_Scale = scale;
                m_Rotation = rotation;
                m_Translation = translation;
            }
        }
    }
}