﻿/* BMDWriter
 * 
 * Given a ModelBase object created by a Loader class, generates a BMD model.
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace SM64DSe.ImportExport.Writers.InternalWriters
{
    public class BMDWriter : AbstractModelWriter
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

            public void AddVertexCommand(Vector4 _vtx, Vector4 _prev, bool alwaysWriteFullVertexCmd23h = true)
            {
                if (_prev.W == 12345678f)
                {
                    AddVertexCommand(_vtx);
                    return;
                }

                Vector4 vtx = Vector4.Multiply(_vtx, 4096f);
                Vector4 prev = Vector4.Multiply(_prev, 4096f);

                if (alwaysWriteFullVertexCmd23h)
                {
                    AddVertexCommand(_vtx);
                }
                else
                {
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

            public void AddNormalCommand(Vector3 nrm)
            {
                short x = (short)(nrm.X * 512.0f);
                short y = (short)(nrm.Y * 512.0f);
                short z = (short)(nrm.Z * 512.0f);
                uint param = (uint)(((ushort)x) >> 6 | ((((ushort)y) << 4) & 0xFFC00) |
                    ((((ushort)z) << 14) & 0x3FF00000));
                AddCommand(0x21, param);
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

        public static ConvertedTexture ConvertTexture(ModelBase.TextureDefBase texture)
        {
            if (!texture.IsNitro())
            {
                return ConvertTexture(texture.m_ID, texture.GetTexName(), texture.GetPalName(), texture.GetBitmap());
            }
            else
            {
                int textype = (int)texture.m_Format;
                int dswidth = 0, dsheight = 0, widthPowerOfTwo = 8, heightPowerOfTwo = 8;
                GetDSWidthAndHeight((int)texture.GetWidth(), (int)texture.GetHeight(), out dswidth, out dsheight, 
                    out widthPowerOfTwo, out heightPowerOfTwo);
                uint dstp = GetDSTextureParamsPart1(dswidth, dsheight, textype, texture.GetColor0Mode());

                return new ConvertedTexture(dstp, texture.GetNitroTexData(), texture.GetNitroPalette(), texture.GetTexName(),
                    texture.GetPalName());
            }
        }

        public static ConvertedTexture ConvertTexture(string name, string texname, string palname, Bitmap bmp)
        {
            int dswidth = 0, dsheight = 0, widthPowerOfTwo = 8, heightPowerOfTwo = 8;
            GetDSWidthAndHeight(bmp.Width, bmp.Height, out dswidth, out dsheight, out widthPowerOfTwo, out heightPowerOfTwo);

            // cheap resizing for textures whose dimensions aren't power-of-two
            if ((widthPowerOfTwo != bmp.Width) || (heightPowerOfTwo != bmp.Height))
            {
                Bitmap newbmp = new Bitmap(widthPowerOfTwo, heightPowerOfTwo);
                Graphics g = Graphics.FromImage(newbmp);
                g.DrawImage(bmp, new Rectangle(0, 0, widthPowerOfTwo, heightPowerOfTwo));
                bmp = newbmp;
            }

            bool alpha = false;
            for (int y = 0; y < heightPowerOfTwo; y++)
            {
                for (int x = 0; x < widthPowerOfTwo; x++)
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
                tex = new byte[widthPowerOfTwo * heightPowerOfTwo];
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

                for (int y = 0; y < heightPowerOfTwo; y++)
                {
                    for (int x = 0; x < widthPowerOfTwo; x++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        ushort bgr15 = Helper.ColorToBGR15(c);
                        int a = c.A & alphamask;

                        byte val = (byte)(_pal.FindClosestColorID(bgr15) | a);
                        tex[(y * widthPowerOfTwo) + x] = val;
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
                tex = new byte[((widthPowerOfTwo * heightPowerOfTwo) / 16) * 6];
                List<Palette> pallist = new List<Palette>();
                List<ushort> paldata = new List<ushort>();

                int texoffset = 0;
                int palidxoffset = ((widthPowerOfTwo * heightPowerOfTwo) / 16) * 4;

                for (int y = 0; y < heightPowerOfTwo; y += 4)
                {
                    for (int x = 0; x < widthPowerOfTwo; x += 4)
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

            uint dstp = GetDSTextureParamsPart1(dswidth, dsheight, textype, 0);
            return new ConvertedTexture(dstp, tex, pal, texname, palname);
        }

        public static void GetDSWidthAndHeight(int texWidth, int texHeight, out int dswidth, out int dsheight,
            out int widthPowerOfTwo, out int heightPowerOfTwo)
        {
            // (for N=0..7: Size=(8 SHL N); ie. 8..1024 texels)
            widthPowerOfTwo = 8; heightPowerOfTwo = 8;
            dswidth = 0; dsheight = 0;
            while (widthPowerOfTwo < texWidth) { widthPowerOfTwo *= 2; dswidth++; }
            while (heightPowerOfTwo < texHeight) { heightPowerOfTwo *= 2; dsheight++; }
        }

        public static uint GetDSTextureParamsPart1(int dswidth, int dsheight, int textype, byte color0mode)
        {
            uint dstp = (uint)((dswidth << 20) | (dsheight << 23) |
                    (textype << 26) | (color0mode << 29));
            return dstp;
        }

        public enum VertexListPrimitiveTypes
        {
            SeparateTriangles = 0,
            SeparateQuadrilaterals = 1,
            TriangleStrip = 2,
            QuadrilateralStrip = 3
        };

        public NitroFile m_ModelFile;

        protected bool m_AlwaysWriteFullVertexCmd23h = true;

        protected bool m_ZMirror = false;
        protected bool m_SwapYZ = false;

        public BMDWriter(ModelBase model, ref NitroFile modelFile) :
            this(model, ref modelFile, BMDImporter.BMDExtraImportOptions.DEFAULT) { }

        public BMDWriter(ModelBase model, ref NitroFile modelFile, BMDImporter.BMDExtraImportOptions extraOptions) :
            base(model, modelFile.m_Name)
        {
            m_ModelFile = modelFile;
            m_AlwaysWriteFullVertexCmd23h = extraOptions.m_AlwaysWriteFullVertexCmd23h;
        }

        public override void WriteModel(bool save = true)
        {
            int b = 0;
            GXDisplayListPacker dlpacker = new GXDisplayListPacker();

            ModelBase.BoneDefRoot boneTree = m_Model.m_BoneTree;
            Dictionary<string, ModelBase.MaterialDef> materials = m_Model.m_Materials;
            Dictionary<string, ModelBase.TextureDefBase> textures = m_Model.m_Textures;

            NitroFile bmd = m_ModelFile;
            bmd.Clear();

            // Vertices mustn't be written in their transformed state, otherwise they'll have their transformations applied a second
            // time. We apply the inverse transformation so that when the transformation is applied they appear correctly.
            m_Model.ApplyInverseTransformations();

            float largest = 0f;
            foreach (ModelBase.BoneDef bone in boneTree)
            {
                foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        foreach (ModelBase.FaceListDef faceList in polyList.m_FaceLists)
                        {
                            foreach (ModelBase.FaceDef face in faceList.m_Faces)
                            {
                                foreach (ModelBase.VertexDef vert in face.m_Vertices)
                                {
                                    Vector3 vtx = vert.m_Position;

                                    if (vtx.X > largest) largest = vtx.X;
                                    if (vtx.Y > largest) largest = vtx.Y;
                                    if (vtx.Z > largest) largest = vtx.Z;

                                    if (-vtx.X > largest) largest = -vtx.X;
                                    if (-vtx.Y > largest) largest = -vtx.Y;
                                    if (-vtx.Z > largest) largest = -vtx.Z;
                                }
                            }
                        }
                    }
                }
            }

            float scaleModel = 1f; uint scaleval = 0;
            while (largest > (32767f / 4096f))
            {
                scaleval++;
                scaleModel /= 2f;
                largest /= 2f;
            }

            if (scaleval > 31)
            {
                MessageBox.Show("Your modelFile is too large to be imported. Try scaling it down.", Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (ModelBase.BoneDef bone in boneTree)
            {
                foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                {
                    foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                    {
                        foreach (ModelBase.FaceListDef faceList in polyList.m_FaceLists)
                        {
                            foreach (ModelBase.FaceDef face in faceList.m_Faces)
                            {
                                for (int vert = 0; vert < face.m_Vertices.Length; vert++)
                                {
                                    face.m_Vertices[vert].m_Position.X *= scaleModel;
                                    face.m_Vertices[vert].m_Position.Y *= scaleModel;
                                    face.m_Vertices[vert].m_Position.Z *= scaleModel;
                                }
                            }
                        }
                    }
                }
            }

            Dictionary<string, ConvertedTexture> convertedTextures = new Dictionary<string, ConvertedTexture>();
            uint ntex = 0, npal = 0;
            int texsize = 0;
            foreach (KeyValuePair<string, ModelBase.MaterialDef> _mat in materials)
            {
                ModelBase.MaterialDef mat = _mat.Value;
                string matname = _mat.Key;

                if (mat.m_TextureDefID != null)
                {
                    ModelBase.TextureDefBase texture = m_Model.m_Textures[mat.m_TextureDefID];

                    if (!convertedTextures.ContainsKey(texture.m_ID))
                    {
                        ConvertedTexture tex = ConvertTexture(texture);
                        tex.m_TextureID = ntex;
                        tex.m_PaletteID = npal;
                        if (tex.m_TextureData != null) { ntex++; texsize += tex.m_TextureData.Length; }
                        if (tex.m_PaletteData != null) { npal++; texsize += tex.m_PaletteData.Length; }
                        convertedTextures.Add(texture.m_ID, tex);
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
            bmd.Write32(0x0C, (uint)materials.Count);
            bmd.Write32(0x10, curoffset);

            uint dllistoffset = curoffset;
            curoffset += (uint)(materials.Count * 8);

            int lastColourARGB = -1;

            // build display lists
            b = 0;
            foreach (ModelBase.MaterialDef mat in materials.Values)
            {
                bmd.Write32(dllistoffset, 1);
                bmd.Write32(dllistoffset + 0x4, curoffset);
                dllistoffset += 0x8;

                Vector2 tcscale = (mat.m_TextureDefID != null) ?
                    new Vector2(textures[mat.m_TextureDefID].GetWidth(), textures[mat.m_TextureDefID].GetHeight()) :
                    Vector2.Zero;

                string curmaterial = mat.m_ID;

                // For each bone, for each geometry, for each polylist whose material matches the 
                // current material, for each face, add it to the current display list
                float largesttc = 0f;
                foreach (ModelBase.BoneDef bone in boneTree)
                {
                    foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                    {
                        foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                        {
                            if (!polyList.m_MaterialName.Equals(curmaterial))
                                continue;
                            foreach (ModelBase.FaceListDef faceList in polyList.m_FaceLists)
                            {
                                foreach (ModelBase.FaceDef face in faceList.m_Faces)
                                {
                                    foreach (ModelBase.VertexDef vert in face.m_Vertices)
                                    {
                                        Vector2? txc = vert.m_TextureCoordinate;
                                        if (txc == null) continue;
                                        Vector2 scaledTxc = Vector2.Multiply((Vector2)txc, tcscale);
                                        if (Math.Abs(scaledTxc.X) > largesttc) largesttc = Math.Abs(scaledTxc.X);
                                        if (Math.Abs(scaledTxc.Y) > largesttc) largesttc = Math.Abs(scaledTxc.Y);
                                    }
                                }
                            }
                        }
                    }
                }
                float _tcscale = largesttc / (32767f / 16f);
                if (_tcscale > 1f)
                {
                    _tcscale = (float)Math.Ceiling(_tcscale * 4096f) / 4096f;
                    mat.m_TextureScale = new Vector2(_tcscale, _tcscale);
                    Vector2.Divide(ref tcscale, _tcscale, out tcscale);
                }
                else
                    mat.m_TextureScale = Vector2.Zero;

                dlpacker.ClearCommands();
                int lastface = -1;
                int lastmatrix = -1;
                Vector4 lastvtx = new Vector4(0f, 0f, 0f, 12345678f);
                Vector3 lastnrm = Vector3.Zero;
                foreach (ModelBase.BoneDef bone in boneTree)
                {
                    foreach (ModelBase.GeometryDef geometry in bone.m_Geometries.Values)
                    {
                        foreach (ModelBase.PolyListDef polyList in geometry.m_PolyLists.Values)
                        {
                            if (!polyList.m_MaterialName.Equals(curmaterial))
                                continue;
                            foreach (ModelBase.FaceListDef faceList in polyList.m_FaceLists)
                            {
                                if (faceList.m_Type.Equals(ModelBase.PolyListType.TriangleStrip))
                                {
                                    dlpacker.AddCommand(0x40, (uint)VertexListPrimitiveTypes.TriangleStrip);// Begin Vertex List
                                    AddTriangleStripToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix,
                                                    ref lastvtx, ref lastnrm, faceList.m_Faces);
                                    dlpacker.AddCommand(0x41);//End Vertex List
                                }
                                else
                                {
                                    foreach (ModelBase.FaceDef face in faceList.m_Faces)
                                    {
                                        int nvtx = face.m_NumVertices;

                                        if (nvtx != lastface || lastface > 4)
                                        {
                                            uint vtxtype = 0;
                                            switch (nvtx)
                                            {
                                                case 1:
                                                case 2:
                                                case 3: vtxtype = (uint)VertexListPrimitiveTypes.SeparateTriangles; break;
                                                case 4: vtxtype = (uint)VertexListPrimitiveTypes.SeparateQuadrilaterals; break;
                                                default: vtxtype = (uint)VertexListPrimitiveTypes.TriangleStrip; break;
                                            }

                                            if (lastface != -1) dlpacker.AddCommand(0x41);// End Vertex List

                                            dlpacker.AddCommand(0x40, vtxtype);// Begin Vertex List

                                            lastface = nvtx;
                                        }

                                        switch (nvtx)
                                        {
                                            case 1: // point
                                                AddSinglePointToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix,
                                                    ref lastvtx, ref lastnrm, face);
                                                break;

                                            case 2: // line
                                                AddLineToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix,
                                                    ref lastvtx, ref lastnrm, face);
                                                break;

                                            case 3: // triangle
                                                AddTriangleToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix,
                                                    ref lastvtx, ref lastnrm, face);
                                                break;

                                            case 4: // quad
                                                AddQuadrilateralToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix,
                                                    ref lastvtx, ref lastnrm, face);
                                                break;

                                            default: // whatever (import as triangle strip)
                                                // todo
                                                break;
                                        }
                                    }

                                    dlpacker.AddCommand(0x41);
                                    lastface = -1;
                                }
                            }
                        }
                    }
                }
                byte[] dlist = dlpacker.GetDisplayList();

                // Display list header
                uint dllist_data_offset = (uint)(((curoffset + 0x10 + (boneTree.Count)) + 3) & ~3);
                bmd.Write32(curoffset, (uint)m_Model.m_BoneTransformsMap.Count);// Number of transforms
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
                for (int j = 0; j < m_Model.m_BoneTransformsMap.Count; j++)
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
            // Eg. Bone 26 may be mapped to matrix 6. If a Matrix Restore command is issued with Matrix ID of 6, 
            // it'll be used as an index into this list and should return bone ID 26.
            foreach (string key in m_Model.m_BoneTransformsMap.GetFirstToSecond().Keys)
            {
                bmd.Write16(curoffset, (ushort)m_Model.m_BoneTree.GetBoneIndex(key));
                curoffset += 2;
            }
            curoffset = (uint)((curoffset + 3) & ~3);

            // build bones
            bmd.Write32(0x4, (uint)boneTree.Count);// Number of bones
            bmd.Write32(0x8, curoffset);

            uint bextraoffset = (uint)(curoffset + (0x40 * boneTree.Count));

            int boneID = 0;
            foreach (ModelBase.BoneDef bone in boneTree)
            {
                bmd.Write32(curoffset + 0x00, (uint)boneID); // bone ID
                bmd.Write16(curoffset + 0x08, (ushort)boneTree.GetParentOffset(bone));// Offset in bones to parent bone (signed 16-bit. 0=no parent, -1=parent is the previous bone, ...)
                bmd.Write16(curoffset + 0x0A, (bone.m_HasChildren) ? (ushort)1 : (ushort)0);// 1 if the bone has children, 0 otherwise
                bmd.Write32(curoffset + 0x0C, (uint)boneTree.GetNextSiblingOffset(bone));// Offset in bones to the next sibling bone (0=bone is last child of its parent)
                bmd.Write32(curoffset + 0x10, (uint)bone.m_20_12Scale[0]);// X scale (32-bit signed, 20:12 fixed point. Think GX command 0x1B)
                bmd.Write32(curoffset + 0x14, (uint)bone.m_20_12Scale[1]);// Y scale
                bmd.Write32(curoffset + 0x18, (uint)bone.m_20_12Scale[2]);// Z scale
                bmd.Write16(curoffset + 0x1C, (ushort)bone.m_4_12Rotation[0]);// X rotation (16-bit signed, 0x0400 = 90°)
                bmd.Write16(curoffset + 0x1E, (ushort)bone.m_4_12Rotation[1]);// Y rotation
                bmd.Write16(curoffset + 0x20, (ushort)bone.m_4_12Rotation[2]);// Z rotation
                bmd.Write16(curoffset + 0x22, 0);// Zero (padding)
                bmd.Write32(curoffset + 0x24, (uint)bone.m_20_12Translation[0]);// X translation (32-bit signed, 20:12 fixed point. Think GX command 0x1C)
                bmd.Write32(curoffset + 0x28, (uint)bone.m_20_12Translation[1]);// Y translation
                bmd.Write32(curoffset + 0x2C, (uint)bone.m_20_12Translation[2]);// Z translation
                bmd.Write32(curoffset + 0x30, (uint)bone.m_MaterialsInBranch.Count);// Number of displaylist/material pairs
                bmd.Write32(curoffset + 0x3C, (bone.m_Billboard) ? (uint)1 : (uint)0);// Bit0: bone is rendered facing the camera (billboard); Bit2: ???

                bmd.Write32(curoffset + 0x34, bextraoffset);// Material IDs list
                for (byte j = 0; j < bone.m_MaterialsInBranch.Count; j++)
                {
                    bmd.Write8(bextraoffset, (byte)materials[bone.m_MaterialsInBranch[j]].m_Index);
                    bextraoffset++;
                }

                bmd.Write32(curoffset + 0x38, bextraoffset);// Displaylist IDs list
                for (byte j = 0; j < bone.m_MaterialsInBranch.Count; j++)
                {
                    bmd.Write8(bextraoffset, (byte)materials[bone.m_MaterialsInBranch[j]].m_Index);
                    bextraoffset++;
                }

                bmd.Write32(curoffset + 0x04, bextraoffset);// Bone names (null-terminated ASCII string)
                bmd.WriteString(bextraoffset, bone.m_ID, 0);
                bextraoffset += (uint)(bone.m_ID.Length + 1);

                boneID++;

                curoffset += 0x40;
            }
            curoffset = (uint)((bextraoffset + 3) & ~3);

            // build materials
            bmd.Write32(0x24, (uint)materials.Count);
            bmd.Write32(0x28, curoffset);

            uint mextraoffset = (uint)(curoffset + (0x30 * materials.Count));

            foreach (KeyValuePair<string, ModelBase.MaterialDef> _mat in materials)
            {
                ModelBase.MaterialDef mat = _mat.Value;
                string matname = _mat.Key;

                uint texid = 0xFFFFFFFF, palid = 0xFFFFFFFF;
                uint teximage_param = 0x00000000;
                uint texscaleS = 0x00001000;
                uint texscaleT = 0x00001000;
                if (mat.m_TextureDefID != null && convertedTextures.ContainsKey(mat.m_TextureDefID))
                {
                    ConvertedTexture tex = convertedTextures[mat.m_TextureDefID];
                    texid = tex.m_TextureID;
                    palid = (tex.m_PaletteData != null) ? tex.m_PaletteID : 0xFFFFFFFF;

                    // 16    Repeat in S Direction (0=Clamp Texture, 1=Repeat Texture)
                    // 17    Repeat in T Direction (0=Clamp Texture, 1=Repeat Texture)
                    // 18    Flip in S Direction   (0=No, 1=Flip each 2nd Texture) (requires Repeat)
                    // 19    Flip in T Direction   (0=No, 1=Flip each 2nd Texture) (requires Repeat)
                    if (mat.m_TexTiling[0] == ModelBase.MaterialDef.TexTiling.Repeat)
                        teximage_param |= 0x00010000;
                    else if (mat.m_TexTiling[0] == ModelBase.MaterialDef.TexTiling.Flip)
                        teximage_param |= 0x00040000;
                    if (mat.m_TexTiling[1] == ModelBase.MaterialDef.TexTiling.Repeat)
                        teximage_param |= 0x00030000;
                    else if (mat.m_TexTiling[1] == ModelBase.MaterialDef.TexTiling.Flip)
                        teximage_param |= 0x00080000;

                    if (mat.m_TextureScale.X > 0f && mat.m_TextureScale.Y > 0f)
                    {
                        teximage_param |= 0x40000000;

                        texscaleS = (uint)(int)(mat.m_TextureScale.X * 4096);
                        texscaleT = (uint)(int)(mat.m_TextureScale.Y * 4096);
                    }

                    // 30-31 Texture Coordinates Transformation Mode (0..3, see below)
                    // Texture Coordinates Transformation Modes:
                    // 0  Do not Transform texture coordinates
                    // 1  TexCoord source
                    // 2  Normal source
                    // 3  Vertex source
                    teximage_param |= (uint)(((byte)mat.m_TexGenMode) << 30);
                }

                // Cmd 29h POLYGON_ATTR - Set Polygon Attributes (W)
                uint polyattr = 0x00000000;
                // 0-3   Light 0..3 Enable Flags (each bit: 0=Disable, 1=Enable)
                for (int i = 0; i < 4; i++)
                {
                    if (mat.m_Lights[i] == true)
                        polyattr |= (uint)(0x01 << i);
                }
                // 4-5   Polygon Mode  (0=Modulation,1=Decal,2=Toon/Highlight Shading,3=Shadow)
                switch (mat.m_PolygonMode)
                {
                    case ModelBase.MaterialDef.PolygonMode.Decal:
                        polyattr |= 0x10;
                        break;
                    case ModelBase.MaterialDef.PolygonMode.Toon_HighlightShading:
                        polyattr |= 0x20;
                        break;
                    case ModelBase.MaterialDef.PolygonMode.Shadow:
                        polyattr |= 0x30;
                        break;
                }
                // 6     Polygon Back Surface   (0=Hide, 1=Render)  ;Line-segments are always
                // 7     Polygon Front Surface  (0=Hide, 1=Render)  ;rendered (no front/back)
                if (_mat.Value.m_PolygonDrawingFace == ModelBase.MaterialDef.PolygonDrawingFace.FrontAndBack) polyattr |= 0xC0;
                else if (_mat.Value.m_PolygonDrawingFace == ModelBase.MaterialDef.PolygonDrawingFace.Front) polyattr |= 0x80;
                else if (_mat.Value.m_PolygonDrawingFace == ModelBase.MaterialDef.PolygonDrawingFace.Back) polyattr |= 0x40;
                // 12    Far-plane intersecting polygons       (0=Hide, 1=Render/clipped)
                if (mat.m_FarClipping) polyattr |= 0x1000;
                // 13    1-Dot polygons behind DISP_1DOT_DEPTH (0=Hide, 1=Render)
                if (mat.m_RenderOnePixelPolygons) polyattr |= 0x2000;
                // 14    Depth Test, Draw Pixels with Depth    (0=Less, 1=Equal) (usually 0)
                if (mat.m_DepthTestDecal) polyattr |= 0x4000;
                // 15    Fog Enable                            (0=Disable, 1=Enable)
                if (mat.m_FogFlag) polyattr |= 0x8000;
                // 16-20 Alpha      (0=Wire-Frame, 1..30=Translucent, 31=Solid)
                uint alpha = (uint)(mat.m_Alpha >> 3);
                polyattr |= (alpha << 16);

                // Set material colours
                uint diffuse_ambient = 0x00000000;
                diffuse_ambient |= Helper.ColorToBGR15(mat.m_Diffuse);
                diffuse_ambient |= 0x8000;
                diffuse_ambient |= (uint)(Helper.ColorToBGR15(mat.m_Diffuse) << 0x10);
                uint specular_emission = 0x00000000;
                specular_emission |= Helper.ColorToBGR15(mat.m_Specular);
                specular_emission |= (uint)(Helper.ColorToBGR15(mat.m_Emission) << 0x10);

                bmd.Write32(curoffset + 0x04, texid);
                bmd.Write32(curoffset + 0x08, palid);
                bmd.Write32(curoffset + 0x0C, texscaleS);
                bmd.Write32(curoffset + 0x10, texscaleT);
                bmd.Write16(curoffset + 0x14, (ushort)((mat.m_TextureRotation * 2048.0f) / Math.PI));
                bmd.Write16(curoffset + 0x16, 0);
                bmd.Write32(curoffset + 0x18, (uint)(mat.m_TextureTranslation.X * 4096.0f));
                bmd.Write32(curoffset + 0x1C, (uint)(mat.m_TextureTranslation.Y * 4096.0f));
                bmd.Write32(curoffset + 0x20, teximage_param);
                bmd.Write32(curoffset + 0x24, polyattr);
                bmd.Write32(curoffset + 0x28, diffuse_ambient);
                bmd.Write32(curoffset + 0x2C, specular_emission);

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
            foreach (ConvertedTexture tex in convertedTextures.Values)
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
            foreach (ConvertedTexture tex in convertedTextures.Values)
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
            foreach (ConvertedTexture tex in convertedTextures.Values)
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
        }

        private void AddTriangleStripToDisplayList(GXDisplayListPacker dlpacker, ref int lastColourARGB, ref Vector2 tcscale,
            ref int lastmatrix, ref Vector4 lastvtx, ref Vector3 lastnrm, List<ModelBase.FaceDef> faces)
        {
            if (faces.Count < 1)
                return;

            AddTriangleToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx, ref lastnrm, faces.ElementAt(0));

            bool even = false;
            for (int i = 1; i < faces.Count; i++)
            {
                if (even)
                    WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                        ref lastnrm, faces[i].m_Vertices[2]);
                else
                    WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                        ref lastnrm, faces[i].m_Vertices[0]);

                even = !even;
            }
        }

        private void AddQuadrilateralToDisplayList(GXDisplayListPacker dlpacker, ref int lastColourARGB, ref Vector2 tcscale,
            ref int lastmatrix, ref Vector4 lastvtx, ref Vector3 lastnrm, ModelBase.FaceDef face)
        {
            WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                ref lastnrm, face.m_Vertices[0]);

            /*if (m_ZMirror)
            {
                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, new Vector4(face.m_Vertices[0], 0f),
                    face.m_Vertices[3], face.m_TextureCoordinates[3], face.m_VertexColours[3], face.m_VertexBoneIDs[3]);

                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, new Vector4(face.m_Vertices[3], 0f),
                    face.m_Vertices[2], face.m_TextureCoordinates[2], face.m_VertexColours[2], face.m_VertexBoneIDs[2]);

                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, new Vector4(face.m_Vertices[2], 0f),
                    face.m_Vertices[1], face.m_TextureCoordinates[1], face.m_VertexColours[1], face.m_VertexBoneIDs[1]);

                lastvtx = new Vector4(face.m_Vertices[1], 0f);
            }
            else*/
            {
                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                    ref lastnrm, face.m_Vertices[1]);

                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                    ref lastnrm, face.m_Vertices[2]);

                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                    ref lastnrm, face.m_Vertices[3]);
            }
        }

        private void AddTriangleToDisplayList(GXDisplayListPacker dlpacker, ref int lastColourARGB, ref Vector2 tcscale, ref int lastmatrix,
            ref Vector4 lastvtx, ref Vector3 lastnrm, ModelBase.FaceDef face)
        {
            WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                ref lastnrm, face.m_Vertices[0]);

            /*if (m_ZMirror)
            {
                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, new Vector4(face.m_Vertices[0], 0f),
                    face.m_Vertices[2], face.m_TextureCoordinates[2], face.m_VertexColours[2], face.m_VertexBoneIDs[2]);

                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, new Vector4(face.m_Vertices[2], 0f),
                    face.m_Vertices[1], face.m_TextureCoordinates[1], face.m_VertexColours[1], face.m_VertexBoneIDs[1]);

                lastvtx = new Vector4(face.m_Vertices[1], 0f);
            }
            else*/
            {
                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                    ref lastnrm, face.m_Vertices[1]);

                WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                    ref lastnrm, face.m_Vertices[2]);
            }
        }

        private void AddLineToDisplayList(GXDisplayListPacker dlpacker, ref int lastColourARGB, ref Vector2 tcscale, ref int lastmatrix,
            ref Vector4 lastvtx, ref Vector3 lastnrm, ModelBase.FaceDef face)
        {
            WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                ref lastnrm, face.m_Vertices[0]);

            WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                ref lastnrm, face.m_Vertices[1]);

            WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                ref lastnrm, face.m_Vertices[1]);
        }

        private void AddSinglePointToDisplayList(GXDisplayListPacker dlpacker, ref int lastColourARGB, ref Vector2 tcscale,
            ref int lastmatrix, ref Vector4 lastvtx, ref Vector3 lastnrm, ModelBase.FaceDef face)
        {
            WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                ref lastnrm, face.m_Vertices[0]);

            WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                ref lastnrm, face.m_Vertices[0]);

            WriteVertexToDisplayList(dlpacker, ref lastColourARGB, ref tcscale, ref lastmatrix, ref lastvtx,
                ref lastnrm, face.m_Vertices[0]);
        }

        private void WriteVertexToDisplayList(GXDisplayListPacker dlpacker, ref int lastColourARGB, ref Vector2 tcscale,
            ref int lastmatrix, ref Vector4 lastvtx, ref Vector3 lastnrm, ModelBase.VertexDef vertex)
        {
            Vector4 vtx = new Vector4(vertex.m_Position, 0f);
            int matrixID = m_Model.m_BoneTransformsMap.GetByFirst(m_Model.m_BoneTree.GetBoneByIndex(vertex.m_VertexBoneID).m_ID);
            if (lastmatrix != matrixID)
            {
                dlpacker.AddCommand(0x14, (uint)matrixID);// Matrix Restore ID for current vertex
                lastmatrix = matrixID;
            }
            if (vertex.m_TextureCoordinate != null)
            {
                dlpacker.AddTexCoordCommand(Vector2.Multiply((Vector2)vertex.m_TextureCoordinate, tcscale));
            }
            if (vertex.m_Normal != null && (Vector3)vertex.m_Normal != lastnrm)
            {
                dlpacker.AddNormalCommand((Vector3)vertex.m_Normal);
                lastnrm = (Vector3)vertex.m_Normal;
            }
            if (vertex.m_VertexColour != null && ((Color)vertex.m_VertexColour).ToArgb() != lastColourARGB)
            {
                dlpacker.AddColorCommand((Color)vertex.m_VertexColour);
                lastColourARGB = ((Color)vertex.m_VertexColour).ToArgb();
            }
            dlpacker.AddVertexCommand(vtx, lastvtx, m_AlwaysWriteFullVertexCmd23h);
            lastvtx = vtx;
        }

    }
}
