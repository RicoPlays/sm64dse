/* This class allows the importing of models to BMD format.
 * Currently supported:
 *  Wavefront OBJ
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

namespace SM64DSe.Importers
{
    public static class BMD_Importer
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
            public string m_MatName;
            public int m_BoneID;
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

            string texname = filename.Substring(filename.LastIndexOf('\\') + 1).Replace('.', '_');
            string palname = (pal == null) ? null : texname + "_pl";

            uint dstp = (uint)((dswidth << 20) | (dsheight << 23) | (textype << 26));
            return new ConvertedTexture(dstp, tex, pal, texname, palname);
        }

        private static void addWhiteMat()
        {
            MaterialDef mat = new MaterialDef();
            mat.m_ID = m_Materials.Count;
            mat.m_Faces = new List<FaceDef>();
            mat.m_DiffuseColor = Color.White;
            mat.m_Opacity = 255;
            mat.m_HasTextures = false;
            mat.m_DiffuseMapName = "";
            mat.m_DiffuseMapID = 0;
            mat.m_DiffuseMapSize = new Vector2(0f, 0f);
            m_Materials.Add("default_white", mat);
        }

        // from loaded model
        public static List<Vector4> m_Vertices;
        public static List<Vector2> m_TexCoords;
        public static List<Vector3> m_Normals;
        public static Dictionary<string, MaterialDef> m_Materials;
        public static Dictionary<string, MaterialDef> m_Textures;
        public static Dictionary<string, BoneForImport> m_Bones;
        public static bool m_SketchupHack;

        private static void OBJ_LoadMTL(String filename)
        {
            Stream fs;
            try
            {
                fs = File.OpenRead(filename);
            }
            catch
            {
                MessageBox.Show("Material library not found:\n\n" + filename + "\n\nA default white material will be used instead.");
                addWhiteMat();
                return;
            }
            StreamReader sr = new StreamReader(fs);

            string curmaterial = "";
            CultureInfo usahax = new CultureInfo("en-US");

            string imagesNotFound = "";

            string curline;
            while ((curline = sr.ReadLine()) != null)
            {
                curline = curline.Trim();

                // skip empty lines and comments
                if (curline.Length < 1) continue;
                if (curline[0] == '#')
                {
                    if (curline == "#Materials exported from Google Sketchup")
                        m_SketchupHack = true;

                    continue;
                }

                string[] parts = curline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) continue;

                switch (parts[0])
                {
                    case "newmtl": // new material definition
                        {
                            if (parts.Length < 2) continue;
                            curmaterial = parts[1];

                            MaterialDef mat = new MaterialDef();
                            mat.m_ID = m_Materials.Count;
                            mat.m_Name = curmaterial;
                            mat.m_Faces = new List<FaceDef>();
                            mat.m_DiffuseColor = Color.White;
                            mat.m_Opacity = 255; // oops
                            mat.m_HasTextures = false;
                            mat.m_DiffuseMapName = "";
                            mat.m_DiffuseMapID = 0;
                            mat.m_DiffuseMapSize = new Vector2(0f, 0f);
                            mat.m_ColType = 0;
                            try
                            {
                                m_Materials.Add(curmaterial, mat);
                            }
                            catch
                            {
                                //Duplicate material
                            }
                        }
                        break;

                    case "d":
                    case "Tr": // opacity
                        {
                            if (parts.Length < 2) continue;
                            float o = float.Parse(parts[1], usahax);
                            if (m_SketchupHack)
                                o *= 255;

                            MaterialDef mat = (MaterialDef)m_Materials[curmaterial];
                            mat.m_Opacity = Math.Max(0, Math.Min(255, (int)(o * 255)));
                        }
                        break;

                    case "Kd": // diffuse color
                        {
                            if (parts.Length < 4) continue;
                            float r = float.Parse(parts[1], usahax);
                            float g = float.Parse(parts[2], usahax);
                            float b = float.Parse(parts[3], usahax);
                            Color col = Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));

                            MaterialDef mat = (MaterialDef)m_Materials[curmaterial];
                            mat.m_DiffuseColor = col;
                        }
                        break;

                    case "map_Kd":
                    case "mapKd": // diffuse map (texture)
                        {
                            string texname = curline.Substring(parts[0].Length + 1).Trim();
                            Bitmap tex;
                            try
                            {
                                tex = new Bitmap(m_ModelPath + texname);

                                int width = 8, height = 8;
                                while (width < tex.Width) width *= 2;
                                while (height < tex.Height) height *= 2;

                                // cheap resizing for textures whose dimensions aren't power-of-two
                                if ((width != tex.Width) || (height != tex.Height))
                                {
                                    Bitmap newbmp = new Bitmap(width, height);
                                    Graphics g = Graphics.FromImage(newbmp);
                                    g.DrawImage(tex, new Rectangle(0, 0, width, height));
                                    tex = newbmp;
                                }

                                MaterialDef mat = (MaterialDef)m_Materials[curmaterial];
                                mat.m_HasTextures = true;

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
                                //System.Drawing.Imaging.BitmapData lol = tex.LockBits(new Rectangle(0, 0, tex.Width, tex.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                //System.Runtime.InteropServices.Marshal.Copy(lol.Scan0, map, 0, tex.Width * tex.Height * 4);

                                string imghash = HexString(m_MD5.ComputeHash(map));
                                if (m_Textures.ContainsKey(imghash))
                                {
                                    MaterialDef mat2 = m_Textures[imghash];
                                    mat.m_DiffuseMapName = mat2.m_DiffuseMapName;
                                    mat.m_DiffuseMapID = mat2.m_DiffuseMapID;
                                    mat.m_DiffuseMapSize = mat2.m_DiffuseMapSize;
                                    break;
                                }

                                mat.m_DiffuseMapName = texname;
                                m_Textures.Add(imghash, mat);

                                mat.m_DiffuseMapSize.X = tex.Width;
                                mat.m_DiffuseMapSize.Y = tex.Height;

                                mat.m_DiffuseMapID = GL.GenTexture();
                                GL.BindTexture(TextureTarget.Texture2D, mat.m_DiffuseMapID);
                                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, tex.Width, tex.Height,
                                    0, PixelFormat.Bgra, PixelType.UnsignedByte, map);

                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                            }
                            catch
                            {
                                imagesNotFound += m_ModelPath + texname + "\n";
                            }
                            break;
                        }
                }
            }

            if (!imagesNotFound.Equals(""))
                MessageBox.Show("The following images were not found:\n\n" + imagesNotFound);

            sr.Close();
        }

        private static void OBJ_LoadBones(String filename)
        {
            Stream fs;
            try
            {
                fs = File.OpenRead(filename);
            }
            catch
            {
                MessageBox.Show("Specified Bone definitions not found:\n\n" + filename + "\n\nUsing default values.");
                return;
            }
            StreamReader sr = new StreamReader(fs);

            string currentBone = "";
            string currentRootBone = "";// Parent (type 0) bone, as opposed to a child bone that has children bones

            m_Bones.Clear();

            string curline;
            while ((curline = sr.ReadLine()) != null)
            {
                curline = curline.Trim();

                // skip empty lines and comments
                if (curline.Length < 1) continue;
                if (curline[0] == '#')
                {
                    continue;
                }

                string[] parts = curline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) continue;

                switch (parts[0])
                {
                    case "newbone": // new bone definition
                        {
                            if (parts.Length < 2) continue;
                            currentBone = parts[1];

                            BoneForImport bone = new BoneForImport();
                            bone.m_Name = currentBone;

                            try
                            {
                                m_Bones.Add(currentBone, bone);
                            }
                            catch
                            {
                                //Duplicate bone name
                                bone.m_Name += "_";
                                m_Bones.Add(currentBone, bone);
                            }
                        }
                        break;

                    case "parent_offset": // Offset in bones to parent bone (signed 16-bit. 0=no parent, -1=parent is the previous bone, ...)
                        {
                            if (parts.Length < 2) continue;
                            short parent_offset = short.Parse(parts[1]);

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_ParentOffset = parent_offset;

                            if (parent_offset == 0)
                            {
                                currentRootBone = currentBone;
                            }

                            bone.m_RootBone = currentRootBone;
                            bone.m_ParentBone = m_Bones.Values.ElementAt(getBoneIndex(bone.m_Name) + bone.m_ParentOffset).m_Name;
                        }
                        break;

                    case "has_children": // 1 if the bone has children, 0 otherwise
                        {
                            if (parts.Length < 2) continue;
                            bool has_children = (short.Parse(parts[1]) == 1);

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_HasChildren = has_children;
                        }
                        break;

                    case "sibling_offset": // Offset in bones to the next sibling bone (0=bone is last child of its parent)
                        {
                            if (parts.Length < 2) continue;
                            short sibling_offset = short.Parse(parts[1]);

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_SiblingOffset = sibling_offset;
                        }
                        break;

                    case "scale": // Scale transformation
                        {
                            if (parts.Length < 4) continue;
                            uint[] scale = new uint[] { uint.Parse(parts[1], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[2], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[3], System.Globalization.NumberStyles.HexNumber) };

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_Scale = scale;
                        }
                        break;

                    case "rotation": // Rotation transformation
                        {
                            if (parts.Length < 4) continue;
                            ushort[] rotation = new ushort[] { ushort.Parse(parts[1], System.Globalization.NumberStyles.HexNumber), 
                                ushort.Parse(parts[2], System.Globalization.NumberStyles.HexNumber), 
                                ushort.Parse(parts[3], System.Globalization.NumberStyles.HexNumber) };

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_Rotation = rotation;
                        }
                        break;

                    case "translation": // Scale transformation
                        {
                            if (parts.Length < 4) continue;
                            uint[] translation = new uint[] { uint.Parse(parts[1], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[2], System.Globalization.NumberStyles.HexNumber), 
                                uint.Parse(parts[3], System.Globalization.NumberStyles.HexNumber) };

                            BoneForImport bone = (BoneForImport)m_Bones[currentBone];
                            bone.m_Translation = translation;
                        }
                        break;
                }
            }
            sr.Close();
        }

        /* Creates a list of bones based on object names found in OBJ file and assigns them 
         * default values. These will be replaced if a bone definition file is found.
         * By default there is one root parent bone and every other bone is a child bone with one child (until the end)
         */
        private static bool OBJ_LoadDefaultBones(String modelFileName)
        {
            Stream fs = File.OpenRead(m_ModelFileName);
            StreamReader sr = new StreamReader(fs);

            bool foundObjects = false;

            string currentBone = "";
            string rootBone = "";

            string curline;
            while ((curline = sr.ReadLine()) != null)
            {
                curline = curline.Trim();

                // skip empty lines and comments
                if (curline.Length < 1) continue;
                if (curline[0] == '#') continue;

                string[] parts = curline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) continue;

                switch (parts[0])
                {
                    case "o": // object (bone)
                        {
                            currentBone = parts[1];
                            m_Bones.Add(currentBone, new BoneForImport(currentBone, 0, currentBone, 0, false));
                            short parent_offset = -1;
                            if ("".Equals(rootBone))
                            {
                                rootBone = currentBone;
                                parent_offset = 0;
                            }
                            m_Bones[currentBone].m_RootBone = rootBone;
                            m_Bones[currentBone].m_ParentOffset = parent_offset;
                            m_Bones[currentBone].m_ParentBone = m_Bones.Values.ElementAt(getBoneIndex(currentBone) + parent_offset).m_Name;
                            m_Bones[m_Bones[currentBone].m_ParentBone].m_HasChildren = true;// All but last bone set to has children
                            foundObjects = true;
                        }
                        break;
                }
            }

            sr.Close();

            return foundObjects;
        }

        // hash
        private static MD5CryptoServiceProvider m_MD5;

        private static bool m_ZMirror = false;
        private static bool m_SwapYZ = false;

        private static String m_ModelFileName;
        private static String m_ModelPath;

        private static BMD m_ImportedModel;

        /*
         * Main method called when importing a model. Loads an OBJ model and returns an imported BMD 
         * model which is used as a preview for the Model Importer.
         */
        public static BMD LoadModel_OBJ(BMD model, String modelFileName, String modelPath, Vector3 m_Scale)
        {
            m_ImportedModel = model;

            m_Vertices = new List<Vector4>();
            m_TexCoords = new List<Vector2>();
            m_Normals = new List<Vector3>();
            m_Materials = new Dictionary<string, MaterialDef>();
            m_Textures = new Dictionary<string, MaterialDef>();
            m_Bones = new Dictionary<string, BoneForImport>();
            m_SketchupHack = false;

            m_ModelFileName = modelFileName;
            m_ModelPath = modelPath;

            m_MD5 = new MD5CryptoServiceProvider();

            Stream fs = File.OpenRead(m_ModelFileName);
            StreamReader sr = new StreamReader(fs);

            string curmaterial = "";
            CultureInfo usahax = new CultureInfo("en-US");

            bool foundObjects = OBJ_LoadDefaultBones(modelFileName);
            string currentBone = "";
            if (!foundObjects)
            {
                currentBone = "default_bone_name";
                m_Bones.Add(currentBone, new BoneForImport(currentBone, 0, currentBone, 0, false));
            }

            string curline;
            while ((curline = sr.ReadLine()) != null)
            {
                curline = curline.Trim();

                // skip empty lines and comments
                if (curline.Length < 1) continue;
                if (curline[0] == '#') continue;

                string[] parts = curline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1) continue;

                switch (parts[0])
                {
                    case "mtllib": // material lib file
                        {
                            string filename = curline.Substring(parts[0].Length + 1).Trim();
                            OBJ_LoadMTL(m_ModelPath + filename);
                        }
                        break;

                    case "bonelib": // bone definitions file
                        {
                            string filename = curline.Substring(parts[0].Length + 1).Trim();
                            OBJ_LoadBones(m_ModelPath + filename);
                        }
                        break;

                    case "o": // object (bone)
                        if (parts.Length < 2) continue;
                        currentBone = parts[1];
                        m_Bones[currentBone].m_Name = currentBone;
                        break;

                    case "usemtl": // material name
                        if (parts.Length < 2) continue;
                        curmaterial = parts[1];
                        // The parent bone should have a list of all materials used by itself and its children
                        if (!m_Bones[m_Bones[currentBone].m_RootBone].m_Materials.ContainsKey(curmaterial))
                            m_Bones[m_Bones[currentBone].m_RootBone].m_Materials.Add(curmaterial, m_Materials[curmaterial].copyAllButFaces());
                        if (!m_Bones[currentBone].m_Materials.ContainsKey(curmaterial))
                            m_Bones[currentBone].m_Materials.Add(curmaterial, m_Materials[curmaterial].copyAllButFaces());
                        break;

                    case "v": // vertex
                        {
                            if (parts.Length < 4) continue;
                            float x = float.Parse(parts[1], usahax);
                            float y = float.Parse(parts[2], usahax);
                            float z = float.Parse(parts[3], usahax);
                            float w = 1f; //(parts.Length < 5) ? 1f : float.Parse(parts[4], usahax);

                            m_Vertices.Add(new Vector4(x, y, z, w));
                        }
                        break;

                    case "vt": // texcoord
                        {
                            if (parts.Length < 2) continue;
                            float s = float.Parse(parts[1], usahax);
                            float t = (parts.Length < 3) ? 0f : float.Parse(parts[2], usahax);

                            m_TexCoords.Add(new Vector2(s, t));
                        }
                        break;

                    case "vn": // normal
                        {
                            if (parts.Length < 4) continue;
                            float x = float.Parse(parts[1], usahax);
                            float y = float.Parse(parts[2], usahax);
                            float z = float.Parse(parts[3], usahax);

                            Vector3 vec = new Vector3(x, y, z);
                            vec.Normalize();
                            m_Normals.Add(vec);
                        }
                        break;

                    case "f": // face
                        {
                            if (parts.Length < 4) continue;
                            int nvtx = parts.Length - 1;

                            MaterialDef mat = new MaterialDef();
                            try
                            {
                                mat = (MaterialDef)m_Bones[currentBone].m_Materials[curmaterial];
                            }
                            catch
                            {
                                // Referencing a material that doesn't exist
                                curmaterial = "default_white";
                                MessageBox.Show("No material library has been specified, yet faces are still set to use \n" +
                                        "a material. A default white material will be used instead.");
                                addWhiteMat();
                                mat = (MaterialDef)m_Bones[currentBone].m_Materials[curmaterial];
                            }
                            FaceDef face = new FaceDef();
                            face.m_MatName = curmaterial;
                            face.m_VtxIndices = new int[nvtx];
                            face.m_TxcIndices = new int[nvtx];
                            face.m_NrmIndices = new int[nvtx];
                            face.m_BoneID = getBoneIndex(currentBone);

                            for (int i = 0; i < nvtx; i++)
                            {
                                string vtx = parts[i + 1];
                                string[] idxs = vtx.Split(new char[] { '/' });

                                face.m_VtxIndices[i] = int.Parse(idxs[0]) - 1;
                                face.m_TxcIndices[i] = (mat.m_HasTextures && idxs.Length >= 2 && idxs[1].Length > 0)
                                    ? (int.Parse(idxs[1]) - 1) : -1;
                                face.m_NrmIndices[i] = (idxs.Length >= 3) ? (int.Parse(idxs[2]) - 1) : -1;
                            }

                            m_Bones[currentBone].m_Materials[curmaterial] = mat;
                            m_Bones[currentBone].m_Materials[curmaterial].m_Faces.Add(face);
                        }
                        break;
                }
            }

            sr.Close();

            ImportModel(m_Scale, false);

            return m_ImportedModel;
        }

        public static void SaveModelChanges()
        {
            m_ImportedModel.m_File.SaveChanges();
        }

        public static void ImportModel(Vector3 m_Scale, bool save)
        {
            int b = 0;
            GXDisplayListPacker dlpacker = new GXDisplayListPacker();

            NitroFile bmd = m_ImportedModel.m_File;
            bmd.Clear();

            // Because transformations were applied when exporting, we need to reverse them so that when we set the bones' 
            // transforms, the transforms aren't applied again
            applyInverseTransformations();

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
                        ConvertedTexture tex = ConvertTexture(m_ModelPath + mat.m_DiffuseMapName);
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
                                if (lastface == -1)
                                    dlpacker.AddCommand(0x14, (uint)face.m_BoneID);// Matrix Restore ID for current bone

                                lastface = nvtx;
                            }

                            if (lastmatrix != face.m_BoneID)
                                dlpacker.AddCommand(0x14, (uint)face.m_BoneID);// Matrix Restore ID for current bone

                            lastmatrix = face.m_BoneID;

                            // For OBJ the specified diffuse colour is now set via the material headers, 
                            // the ColourCommand is to be used for importing models with vertex colours in future
                            //dlpacker.AddColorCommand(mat.m_DiffuseColour);

                            switch (nvtx)
                            {
                                case 1: // point
                                    {
                                        Vector4 vtx = scaledvtxs[face.m_VtxIndices[0]];
                                        int txc = face.m_TxcIndices[0];

                                        if (txc > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc], tcscale));
                                        dlpacker.AddVertexCommand(vtx, lastvtx);
                                        if (txc > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc], tcscale));
                                        dlpacker.AddVertexCommand(vtx, vtx);
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

                                        if (txc1 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc1], tcscale));
                                        dlpacker.AddVertexCommand(vtx1, lastvtx);
                                        if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                        dlpacker.AddVertexCommand(vtx2, vtx1);
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

                                        if (txc1 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc1], tcscale));
                                        dlpacker.AddVertexCommand(vtx1, lastvtx);
                                        if (m_ZMirror)
                                        {
                                            if (txc3 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc3], tcscale));
                                            dlpacker.AddVertexCommand(vtx3, vtx1);
                                            if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                            dlpacker.AddVertexCommand(vtx2, vtx3);
                                            lastvtx = vtx2;
                                        }
                                        else
                                        {
                                            if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                            dlpacker.AddVertexCommand(vtx2, vtx1);
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

                                        if (txc1 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc1], tcscale));
                                        dlpacker.AddVertexCommand(vtx1, lastvtx);
                                        if (m_ZMirror)
                                        {
                                            if (txc4 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc4], tcscale));
                                            dlpacker.AddVertexCommand(vtx4, vtx1);
                                            if (txc3 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc3], tcscale));
                                            dlpacker.AddVertexCommand(vtx3, vtx4);
                                            if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                            dlpacker.AddVertexCommand(vtx2, vtx3);
                                            lastvtx = vtx2;
                                        }
                                        else
                                        {
                                            if (txc2 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc2], tcscale));
                                            dlpacker.AddVertexCommand(vtx2, vtx1);
                                            if (txc3 > -1) dlpacker.AddTexCoordCommand(Vector2.Multiply(m_TexCoords[txc3], tcscale));
                                            dlpacker.AddVertexCommand(vtx3, vtx2);
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
                bmd.Write32(curoffset + 0x3C, 0);// Bit0: bone is rendered facing the camera (billboard); Bit2: ???

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
                Console.WriteLine(diffuse_ambient);

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
        private static void applyInverseTransformations()
        {
            // Need to maintain a list of transformed vertices to prevent the revers transformation being 
            // applied more than once
            List<int> transformedVertices = new List<int>();
            foreach (BoneForImport bone in m_Bones.Values)
            {
                Vector3 scale = new Vector3((float)(1f / (((int)bone.m_Scale[0]) / 4096.0f)), (float)(1f / (((int)bone.m_Scale[1]) / 4096.0f)), (float)(1f / (((int)bone.m_Scale[2]) / 4096.0f)));
                Vector3 rot = new Vector3(((float)(short)bone.m_Rotation[0] * (float)Math.PI) / -2048.0f, ((float)(short)bone.m_Rotation[1] * (float)Math.PI) / -2048.0f, ((float)(short)bone.m_Rotation[2] * (float)Math.PI) / -2048.0f);
                Vector3 trans = new Vector3((float)(((int)bone.m_Translation[0]) / -4096.0f) * (1f), (float)(((int)bone.m_Translation[1]) / -4096.0f) * (1f), (float)(((int)bone.m_Translation[2]) / -4096.0f) * (1f));

                Matrix4 ret = Matrix4.Identity;

                // If a child bone, apply its parent's transformations as well
                if (bone.m_ParentOffset < 0)
                {
                    BoneForImport parent_bone = m_Bones[bone.m_ParentBone];

                    Matrix4.Mult(ref ret, ref parent_bone.m_ReverseTransform, out ret);
                }

                Matrix4 mscale = Matrix4.Scale(scale);
                Matrix4 mxrot = Matrix4.CreateRotationX(rot.X);
                Matrix4 myrot = Matrix4.CreateRotationY(rot.Y);
                Matrix4 mzrot = Matrix4.CreateRotationZ(rot.Z);
                Matrix4 mtrans = Matrix4.CreateTranslation(trans);

                Matrix4.Mult(ref ret, ref mtrans, out ret);
                Matrix4.Mult(ref ret, ref mzrot, out ret);
                Matrix4.Mult(ref ret, ref myrot, out ret);
                Matrix4.Mult(ref ret, ref mxrot, out ret);
                Matrix4.Mult(ref ret, ref mscale, out ret);

                bone.m_ReverseTransform = ret;

                foreach (MaterialDef mat in bone.m_Materials.Values)
                {
                    foreach (FaceDef face in mat.m_Faces)
                    {
                        foreach (int vertexInd in face.m_VtxIndices)
                        {
                            // If vertex already transformed don't transform it again
                            if (transformedVertices.Contains(vertexInd))
                                continue;
                            Vector4 vert = m_Vertices.ElementAt(vertexInd);
                            Vector3 temp = new Vector3(vert.X, vert.Y, vert.Z);
                            Vector3.Transform(ref temp, ref ret, out temp);

                            m_Vertices[vertexInd] = new Vector4(temp, vert.W);
                            transformedVertices.Add(vertexInd);
                        }
                    }
                }
            }
        }

        private static string HexString(byte[] crap)
        {
            string ret = "";
            foreach (byte b in crap)
                ret += b.ToString("X2");
            return ret;
        }

        private static int getBoneIndex(String key)
        {
            int index = -1;

            for (int i = 0; i < m_Bones.Keys.Count; i++)
            {
                if (m_Bones.Keys.ElementAt(i).Equals(key))
                {
                    index = i;
                    break;
                }
            }

            return index;
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
            }

            public BoneForImport(String name, short parentOffset, String rootBone, short siblingOffset, bool hasChildren)
            {
                m_Materials = new Dictionary<String, MaterialDef>();
                m_Name = name;
                m_ParentOffset = parentOffset;
                m_RootBone = rootBone;
                m_SiblingOffset = siblingOffset;
                m_HasChildren = hasChildren;
                m_Scale = new uint[] { 0x00001000, 0x00001000, 0x00001000 };
                m_Rotation = new ushort[] { 0x0000, 0x0000, 0x0000 };
                m_Translation = new uint[] { 0, 0, 0 };
            }

            public BoneForImport(String name, short parentOffset, String rootBone, short siblingOffset, bool hasChildren,
                uint[] scale, ushort[] rotation, uint[] translation)
            {
                m_Materials = new Dictionary<String, MaterialDef>();
                m_Name = name;
                m_ParentOffset = parentOffset;
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
