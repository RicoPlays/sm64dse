/*
    Copyright 2012 Kuribo64

    This file is part of SM64DSe.

    SM64DSe is free software: you can redistribute it and/or modify it under
    the terms of the GNU General Public License as published by the Free
    Software Foundation, either version 3 of the License, or (at your option)
    any later version.

    SM64DSe is distributed in the hope that it will be useful, but WITHOUT ANY 
    WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
    FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along 
    with SM64DSe. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Security.Cryptography;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SM64DSe
{
    public partial class ModelImporter : Form
    {
        class VectorComparer : IEqualityComparer<Vector3>
        {
            public bool Equals(Vector3 x, Vector3 y)
            {
                return Helper.VectorsEqual(x, y);
            }

            public int GetHashCode(Vector3 v)
            {
                return v.GetHashCode();
            }
        }

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

        private ConvertedTexture ConvertTexture(string filename)
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

        class CollisionPolygon
        {
            public Vector3[] m_Points;
            public Vector3 m_Normal;
            public int m_CollType;

            public CollisionPolygon(Vector3 a, Vector3 b, Vector3 c, int colltype)
            {
                m_Points = new Vector3[3];
                m_Points[0] = a;
                m_Points[1] = b;
                m_Points[2] = c;
                m_CollType = colltype;

                Vector3 ab = Vector3.Subtract(b, a);
                Vector3 ac = Vector3.Subtract(c, a);
                m_Normal = Vector3.Normalize(Vector3.Cross(ac, ab));
                if (float.IsNaN(m_Normal.Length))
                    m_Normal = Vector3.Zero;
            }

            public bool IsIdenticalTo(CollisionPolygon b)
            {
                for (int i = 0; i < m_Points.Length; i++)
                {
                    int j = i;
                    bool lol = true;

                    for (int k = 0; k < b.m_Points.Length; k++)
                    {
                        if (!Helper.VectorsEqual(m_Points[j], b.m_Points[k]))
                        {
                            lol = false;
                            break;
                        }

                        j++;
                        if (j >= m_Points.Length) j = 0;
                    }

                    if (!lol) return false;
                }

                return true;
            }

            /*public static bool AreNeighbor(CollisionPolygon a, CollisionPolygon b)
            {
                if (a.m_CollType != b.m_CollType)
                    return false;
                if (!Helper.VectorsEqual(a.m_Normal, b.m_Normal))
                    return false;

                for (int i = 0; i < a.m_Points.Length; i++)
                {
                    int prev = (i == 0) ? a.m_Points.Length - 1 : i - 1;

                    // assume b is a triangle, for simplicity
                    if (Helper.VectorsEqual(a.m_Points[i], b.m_Points[0]) && Helper.VectorsEqual(a.m_Points[prev], b.m_Points[1]))
                        return true;
                    if (Helper.VectorsEqual(a.m_Points[i], b.m_Points[1]) && Helper.VectorsEqual(a.m_Points[prev], b.m_Points[2]))
                        return true;
                    if (Helper.VectorsEqual(a.m_Points[i], b.m_Points[2]) && Helper.VectorsEqual(a.m_Points[prev], b.m_Points[0]))
                        return true;
                }

                return false;
            }*/

            public bool MergeWith(CollisionPolygon b)
            {
                if (m_CollType != b.m_CollType)
                    return false;
                if (!Helper.VectorsEqual(m_Normal, b.m_Normal))
                    return false;

                // wouldn't happen if the code below wasn't shitty
                //if (b.m_Points.Length < 3)
                //     return true;

                bool merged_anything = false;
                int eaten = 0;
                List<uint> connections = new List<uint>();

                for (int i = 0; i < m_Points.Length; )
                {
                    for (int j = 0; j < b.m_Points.Length; )
                    {
                        if (Helper.VectorsEqual(m_Points[i], b.m_Points[j]))
                        {
                            int ncommon = 0;
                            int i2 = i, j2 = j;
                            do
                            {
                                i2++; if (i2 >= m_Points.Length) i2 = 0;
                                j2--; if (j2 < 0) j2 = b.m_Points.Length - 1;
                                ncommon++;
                                if (j2 == j) { eaten = 1; break; }
                                if (i2 == i) { eaten = 2; break; }
                            }
                            while (Helper.VectorsEqual(m_Points[i2], b.m_Points[j2]));

                            i2--; if (i2 < 0) i2 = m_Points.Length - 1;
                            j2++; if (j2 >= b.m_Points.Length) j2 = 0;

                            if (ncommon > 1)
                            {
                                if (eaten != 0) connections.Clear();
                                connections.Add((uint)(i | (j << 16)));
                                connections.Add((uint)(i2 | (j2 << 16)));
                                merged_anything = true;
                            }

                            if ((eaten != 0) && (connections.Count == 0))
                            {
                                MessageBox.Show("derrrrrrp");
                            }

                            i += ncommon - 1;
                            break;
                        }
                        else
                            j++;
                    }

                    if (eaten != 0) break;

                    i++;
                }

                if ((eaten != 0) && (connections.Count == 0))
                    MessageBox.Show(string.Format("LULZ DURR DERPA DERP {0} {1}", m_Points.Length, b.m_Points.Length));

                if (!merged_anything && eaten == 0) return false;
                //if (eaten !=0) return false;
                List<Vector3> pointlist = new List<Vector3>();

                if (eaten == 1)
                {
                    int start = (int)(connections[0] & 0xFFFF);
                    int end = (int)(connections[1] & 0xFFFF);
                    if (start > end)
                    {
                        for (int i = end + 1; i < start; i++)
                            pointlist.Add(m_Points[i]);
                    }
                    else
                    {
                        for (int i = 0; i < start; i++)
                            pointlist.Add(m_Points[i]);
                        for (int i = end + 1; i < m_Points.Length; i++)
                            pointlist.Add(m_Points[i]);
                    }
                }
                else if (eaten == 2)
                {
                    int start = (int)(connections[1] >> 16);
                    int end = (int)(connections[0] >> 16);
                    if (start > end)
                    {
                        for (int i = start; i < b.m_Points.Length; i++)
                            pointlist.Add(b.m_Points[i]);
                        for (int i = 0; i <= end; i++)
                            pointlist.Add(b.m_Points[i]);
                    }
                    else
                    {
                        for (int i = 0; i < start; i++)
                            pointlist.Add(b.m_Points[i]);
                        for (int i = end + 1; i < b.m_Points.Length; i++)
                            pointlist.Add(b.m_Points[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < m_Points.Length; )
                    {
                        if ((connections.Count > 0) && ((int)(connections[0] & 0xFFFF) == i))
                        {
                            uint conn_start = connections[0];
                            uint conn_end = connections[1];
                            connections.RemoveRange(0, 2);
                            int num = 1;

                            for (int j = (int)(conn_start & 0xFFFF); j != (int)(conn_end & 0xFFFF); )
                            {
                                num++;
                                j++;
                                if (j >= m_Points.Length) j = 0;
                            }

                            pointlist.Add(m_Points[i]);

                            int start = (int)(conn_start >> 16) + 1;
                            if (start >= b.m_Points.Length) start = 0;
                            for (int j = start; j != (int)(conn_end >> 16); )
                            {
                                pointlist.Add(b.m_Points[j]);
                                j++;
                                if (j >= b.m_Points.Length) j = 0;
                            }

                            if ((int)(conn_end & 0xFFFF) > i)
                                pointlist.Add(m_Points[conn_end & 0xFFFF]);
                            else
                                pointlist.RemoveRange(0, (int)(conn_end & 0xFFFF));

                            i += num;
                        }
                        else
                        {
                            pointlist.Add(m_Points[i]);
                            i++;
                        }
                    }
                }

                if (pointlist.Count < 3)
                    throw new Exception("Bad polygon. This shouldn't have happened.\nTell Mega-Mario that he needs to fix his shitty code.");

                m_Points = pointlist.ToArray();
                return true;
                /*List<Vector3> temp = new List<Vector3>();

                for (int i = 0; i < m_Points.Length;)
                {
                    Vector3 pt = m_Points[i];
                    int prev = (i == 0) ? (m_Points.Length - 1) : i - 1;
                    int next = (i == (m_Points.Length - 1)) ? 0 : i + 1;

                    if (Helper.VectorsEqual(pt, b.m_Points[0]) && Helper.VectorsEqual(m_Points[next], b.m_Points[2]))
                    {
                        int _next = i + 2; if (_next >= m_Points.Length) _next -= m_Points.Length;
                        Vector3 v1 = Vector3.Subtract(m_Points[prev], pt);
                        Vector3 v2 = Vector3.Subtract(b.m_Points[1], pt);
                        //if (Vector3.Cross(v1, v2).Length > 0f)
                            temp.Add(pt);
                        temp.Add(b.m_Points[1]);
                        if (next > i)
                        {
                            v1 = Vector3.Subtract(b.m_Points[1], b.m_Points[2]);
                            v2 = Vector3.Subtract(m_Points[_next], b.m_Points[2]);
                            //if (Vector3.Cross(v1, v2).Length > 0f)
                                temp.Add(b.m_Points[2]);
                        }
                        i += 2;
                    }
                    else if (Helper.VectorsEqual(pt, b.m_Points[1]) && Helper.VectorsEqual(m_Points[next], b.m_Points[0]))
                    {
                        int _next = i + 2; if (_next >= m_Points.Length) _next -= m_Points.Length;
                        Vector3 v1 = Vector3.Subtract(m_Points[prev], pt);
                        Vector3 v2 = Vector3.Subtract(b.m_Points[2], pt);
                        //if (Vector3.Cross(v1, v2).Length > 0f)
                            temp.Add(pt);
                        temp.Add(b.m_Points[2]);
                        if (next > i)
                        {
                            v1 = Vector3.Subtract(b.m_Points[2], b.m_Points[0]);
                            v2 = Vector3.Subtract(m_Points[_next], b.m_Points[0]);
                            //if (Vector3.Cross(v1, v2).Length > 0f)
                                temp.Add(b.m_Points[0]);
                        }
                        i += 2;
                    }
                    else if (Helper.VectorsEqual(pt, b.m_Points[2]) && Helper.VectorsEqual(m_Points[next], b.m_Points[1]))
                    {
                        int _next = i + 2; if (_next >= m_Points.Length) _next -= m_Points.Length;
                        Vector3 v1 = Vector3.Subtract(m_Points[prev], pt);
                        Vector3 v2 = Vector3.Subtract(b.m_Points[0], pt);
                        //if (Vector3.Cross(v1, v2).Length > 0f)
                            temp.Add(pt);
                        temp.Add(b.m_Points[0]);
                        if (next > i)
                        {
                            v1 = Vector3.Subtract(b.m_Points[0], b.m_Points[1]);
                            v2 = Vector3.Subtract(m_Points[_next], b.m_Points[1]);
                            //if (Vector3.Cross(v1, v2).Length > 0f)
                                temp.Add(b.m_Points[1]);
                        }
                        i += 2;
                    }
                    else
                    {
                        temp.Add(pt);
                        i++;
                    }
                }
                //if (temp.Count < 3) throw new Exception("lol");
                m_Points = temp.ToArray();*/
            }
        }

        class CollisionPlane
        {
            public float m_Length;
            public Vector3 m_Point, m_PointB, m_PointC;
            public Vector3 m_Normal;
            public Vector3 m_Dir1, m_Dir2, m_Dir3;
            public int m_PointIndex;
            public int m_NormalIndex;
            public int m_Dir1Index, m_Dir2Index, m_Dir3Index;
            public ushort m_Type;
            public bool m_Fake;

            public Vector3 roflhax = Vector3.Zero;
            public Vector3 lolcrap = Vector3.Zero;
            public static int _planeid = 0;
            public int planeid = 0;
            public CollisionPlane(Vector3 a, Vector3 b, Vector3 c, ushort type, bool fake)
            {
                planeid = _planeid;
                _planeid++;

                Vector3 side1 = Vector3.Subtract(b, a);
                Vector3 side2 = Vector3.Subtract(c, a);
                Vector3 side3 = Vector3.Subtract(c, b);

                m_Point = a; m_PointB = b; m_PointC = c;
                m_Normal = Vector3.Normalize(Vector3.Cross(side2, side1));
                m_Dir1 = Vector3.Normalize(Vector3.Cross(m_Normal, side1));
                m_Dir2 = Vector3.Normalize(Vector3.Cross(side2, m_Normal));
                m_Dir3 = Vector3.Normalize(Vector3.Cross(m_Normal, side3));
                m_Length = Vector3.Dot(side1, m_Dir3);
                m_Type = type;

                m_PointIndex = m_NormalIndex = m_Dir1Index = m_Dir2Index = m_Dir3Index = -1;
                m_Fake = fake;
            }

            public bool TryMergeWith(CollisionPlane p)
            {
                // find out which points are colliding
                Vector3[] a_pts = { m_Point, m_PointB, m_PointC };
                Vector3[] b_pts = { p.m_Point, p.m_PointB, p.m_PointC };
                int a_pt1, a_pt2, a_pt3;
                int b_pt3;
                if (Helper.VectorsEqual(a_pts[0], b_pts[0]) && Helper.VectorsEqual(a_pts[2], b_pts[1]))
                { a_pt1 = 0; a_pt2 = 2; a_pt3 = 1; b_pt3 = 2; }
                else if (Helper.VectorsEqual(a_pts[0], b_pts[1]) && Helper.VectorsEqual(a_pts[2], b_pts[2]))
                { a_pt1 = 0; a_pt2 = 2; a_pt3 = 1; b_pt3 = 0; }
                else if (Helper.VectorsEqual(a_pts[0], b_pts[2]) && Helper.VectorsEqual(a_pts[2], b_pts[0]))
                { a_pt1 = 0; a_pt2 = 2; a_pt3 = 1; b_pt3 = 1; }
                else if (Helper.VectorsEqual(a_pts[1], b_pts[0]) && Helper.VectorsEqual(a_pts[0], b_pts[1]))
                { a_pt1 = 1; a_pt2 = 0; a_pt3 = 2; b_pt3 = 2; }
                else if (Helper.VectorsEqual(a_pts[1], b_pts[1]) && Helper.VectorsEqual(a_pts[0], b_pts[2]))
                { a_pt1 = 1; a_pt2 = 0; a_pt3 = 2; b_pt3 = 0; }
                else if (Helper.VectorsEqual(a_pts[1], b_pts[2]) && Helper.VectorsEqual(a_pts[0], b_pts[0]))
                { a_pt1 = 1; a_pt2 = 0; a_pt3 = 2; b_pt3 = 1; }
                else if (Helper.VectorsEqual(a_pts[2], b_pts[0]) && Helper.VectorsEqual(a_pts[1], b_pts[1]))
                { a_pt1 = 2; a_pt2 = 1; a_pt3 = 0; b_pt3 = 2; }
                else if (Helper.VectorsEqual(a_pts[2], b_pts[1]) && Helper.VectorsEqual(a_pts[1], b_pts[2]))
                { a_pt1 = 2; a_pt2 = 1; a_pt3 = 0; b_pt3 = 0; }
                else if (Helper.VectorsEqual(a_pts[2], b_pts[2]) && Helper.VectorsEqual(a_pts[1], b_pts[0]))
                { a_pt1 = 2; a_pt2 = 1; a_pt3 = 0; b_pt3 = 1; }
                else return false;

                // find out the right side and proceed to merge the second plane into the first one
                if (Helper.PointOnLine(a_pts[a_pt1], a_pts[a_pt3], b_pts[b_pt3]))
                {
                    m_Point = a_pts[a_pt3];
                    m_PointC = b_pts[b_pt3];
                    m_PointB = a_pts[a_pt2];
                }
                else if (Helper.PointOnLine(a_pts[a_pt2], a_pts[a_pt3], b_pts[b_pt3]))
                {
                    m_Point = a_pts[a_pt3];
                    m_PointB = b_pts[b_pt3];
                    m_PointC = a_pts[a_pt1];
                }
                else
                    return false;

                // readjust the plane's parameters accordingly
                Vector3 side1 = Vector3.Subtract(m_PointB, m_Point);
                Vector3 side2 = Vector3.Subtract(m_PointC, m_Point);
                Vector3 side3 = Vector3.Subtract(m_PointC, m_PointB);

                m_Normal = Vector3.Normalize(Vector3.Cross(side2, side1));
                m_Dir1 = Vector3.Normalize(Vector3.Cross(m_Normal, side1));
                m_Dir2 = Vector3.Normalize(Vector3.Cross(side2, m_Normal));
                m_Dir3 = Vector3.Normalize(Vector3.Cross(m_Normal, side3));
                m_Length = Vector3.Dot(side1, m_Dir3);

                return true;
            }

            public bool InCube(Vector3 start, Vector3 cubesize)
            {
                if (m_Fake) return false;

                //float epsilon = cubesize.X / 16f + 0.1f;
                float epsilon = Math.Max(cubesize.X / 16f, 0.1f);
                Vector3 cubecenter = Vector3.Add(start, Vector3.Divide(cubesize, 2f));

                Vector3 a2center = Vector3.Subtract(cubecenter, m_Point);
                Vector3 ab = Vector3.Subtract(m_PointB, m_Point);
                Vector3 ac = Vector3.Subtract(m_PointC, m_Point);
                Vector3 bc = Vector3.Subtract(m_PointC, m_PointB);
                Vector3 ca = Vector3.Subtract(m_Point, m_PointC);
                Vector3 normal = Vector3.Cross(ac, ab);

                // calculate the distance from the cube center to the plane
                float dist2plane = Vector3.Dot(a2center, normal) / normal.Length;
                Vector3 center2plane = Vector3.Multiply(normal, -dist2plane / normal.Length);
                Vector3 centeronplane = Vector3.Add(cubecenter, center2plane);

                // calculate the maximal distance to stay within the cube
                // if the plane is further, we can return early
                float dmax = Math.Max(Math.Abs(center2plane.X), Math.Max(Math.Abs(center2plane.Y), Math.Abs(center2plane.Z)));
                float maxdist = Vector3.Multiply(center2plane, cubesize.X / (2f * dmax)).Length; // consider the cube cubic, for speed
                if (center2plane.Length > maxdist + epsilon) return false;

                // if the projection of the cube center onto the plane is within the triangle,
                // consider the triangle to be in
                float dot1 = Vector3.Dot(Vector3.Cross(ab, Vector3.Subtract(centeronplane, m_Point)), normal);
                float dot2 = Vector3.Dot(Vector3.Cross(ca, Vector3.Subtract(centeronplane, m_PointC)), normal);
                float dot3 = Vector3.Dot(Vector3.Cross(bc, Vector3.Subtract(centeronplane, m_PointB)), normal);
                if (dot1 < 0f && dot2 < 0f && dot3 < 0f)
                    return true;

                // otherwise, well...
                // calculate the distances from the cube center to each of the triangle's sides
                // if the smallest distance is within the cube, consider it good
                float r1 = Vector3.Dot(Vector3.Subtract(cubecenter, m_Point), ab) / ab.LengthSquared;
                float r2 = Vector3.Dot(Vector3.Subtract(cubecenter, m_PointC), ca) / ca.LengthSquared;
                float r3 = Vector3.Dot(Vector3.Subtract(cubecenter, m_PointB), bc) / bc.LengthSquared;
                r1 = Math.Max(0f, Math.Min(1f, r1));
                r2 = Math.Max(0f, Math.Min(1f, r2));
                r3 = Math.Max(0f, Math.Min(1f, r3));
                Vector3 p1 = Vector3.Add(m_Point, Vector3.Multiply(ab, r1));
                Vector3 p2 = Vector3.Add(m_PointC, Vector3.Multiply(ca, r2));
                Vector3 p3 = Vector3.Add(m_PointB, Vector3.Multiply(bc, r3));
                Vector3 d1 = Vector3.Subtract(p1, cubecenter);
                Vector3 d2 = Vector3.Subtract(p2, cubecenter);
                Vector3 d3 = Vector3.Subtract(p3, cubecenter);

                Vector3 d;
                if (d1.Length < Math.Min(d2.Length, d3.Length))
                    d = d1;
                else if (d2.Length < d3.Length)
                    d = d2;
                else
                    d = d3;

                dmax = Math.Max(Math.Abs(d.X), Math.Max(Math.Abs(d.Y), Math.Abs(d.Z)));
                maxdist = Vector3.Multiply(d, cubesize.X / (2f * dmax)).Length;
                return d.Length <= maxdist + epsilon;
            }
        }

        class OctreeNode
        {
            public List<int> m_PlaneData;
            public OctreeNode[] m_Children;
            public int m_Depth;
            public uint m_BaseOffset, m_NodeOffset;

            public void Divide(List<CollisionPlane> planes, Vector3 start, Vector3 size, bool dropextra)
            {
                m_Children = new OctreeNode[8];
                size = Vector3.Divide(size, 2f);
                m_Children[0] = new OctreeNode(this, planes, start, size, m_Depth + 1, dropextra);
                m_Children[1] = new OctreeNode(this, planes, Vector3.Add(start, new Vector3(size.X, 0f, 0f)), size, m_Depth + 1, dropextra);
                m_Children[2] = new OctreeNode(this, planes, Vector3.Add(start, new Vector3(0f, size.Y, 0f)), size, m_Depth + 1, dropextra);
                m_Children[3] = new OctreeNode(this, planes, Vector3.Add(start, new Vector3(size.X, size.Y, 0f)), size, m_Depth + 1, dropextra);
                m_Children[4] = new OctreeNode(this, planes, Vector3.Add(start, new Vector3(0f, 0f, size.Z)), size, m_Depth + 1, dropextra);
                m_Children[5] = new OctreeNode(this, planes, Vector3.Add(start, new Vector3(size.X, 0f, size.Z)), size, m_Depth + 1, dropextra);
                m_Children[6] = new OctreeNode(this, planes, Vector3.Add(start, new Vector3(0f, size.Y, size.Z)), size, m_Depth + 1, dropextra);
                m_Children[7] = new OctreeNode(this, planes, Vector3.Add(start, new Vector3(size.X, size.Y, size.Z)), size, m_Depth + 1, dropextra);
                m_PlaneData = null;
            }
            static int nnodes = 0;
            static int maxdepth = 0;
            public static List<OctreeNode> lollist;
            public Vector3 _start, _size;
            public OctreeNode(OctreeNode parent, List<CollisionPlane> planes, Vector3 start, Vector3 size, int depth, bool dropextra)
            {
                nnodes++;
                m_Depth = depth;
                if (depth > maxdepth) maxdepth = depth;
                _start = start; _size = size;

                if (lollist == null) lollist = new List<OctreeNode>();
                lollist.Add(this);

                if (depth == 0)
                {
                    Divide(planes, start, size, dropextra);
                    /*m_Children = new OctreeNode[64];
                    size = Vector3.Divide(size, 4f);
                    int ic = 0;
                    for (int iz = 0; iz < 4; iz++)
                    {
                        for (int iy = 0; iy < 4; iy++)
                        {
                            for (int ix = 0; ix < 4; ix++)
                            {
                                Vector3 _start = Vector3.Add(start, new Vector3(size.X * ix, size.Y * iy, size.Z * iz));
                                m_Children[ic++] = new OctreeNode(this, planes, _start, size, 1);
                            }
                        }
                    }*/
                    return;
                }

                m_Children = null;
                m_PlaneData = new List<int>();
                if (parent.m_PlaneData != null)
                {
                    foreach (int i in parent.m_PlaneData)
                    {
                        if (planes[i].InCube(start, size))
                            m_PlaneData.Add(i);
                    }
                }
                else
                {
                    for (int i = 0; i < planes.Count; i++)
                    {
                        if (planes[i].InCube(start, size))
                            m_PlaneData.Add(i);
                    }
                }

                // idk about the exact limit
                // goes up to 22 in the original models
                const int limit = 32;
                if (m_PlaneData.Count > limit)
                {
                    if (size.X > 0.25f)
                        Divide(planes, start, size, dropextra);
                    else if (dropextra)
                        m_PlaneData.RemoveRange(limit, m_PlaneData.Count - limit);
                }
            }

            public void WriteNode(NitroFile file, ref uint pos)
            {
                m_NodeOffset = pos;
                pos += 4;
            }

            public void WriteChildren(NitroFile file, uint basepos, ref uint pos)
            {
                m_BaseOffset = basepos;

                if (m_Children != null)
                {
                    if (m_Depth > 0)
                        file.Write32(m_NodeOffset, pos - m_BaseOffset);
                    else
                        m_NodeOffset = basepos;

                    foreach (OctreeNode child in m_Children)
                        child.WriteNode(file, ref pos);

                    foreach (OctreeNode child in m_Children)
                        child.WriteChildren(file, m_Children[0].m_NodeOffset, ref pos);
                }
            }

            public void WritePlaneData(NitroFile file, ref uint pos)
            {
                if (m_PlaneData != null)
                {
                    if (m_Depth > 0)
                        file.Write32(m_NodeOffset, 0x80000000 | (pos - m_BaseOffset - 2));

                    foreach (int plane in m_PlaneData)
                    {
                        file.Write16(pos, (ushort)(plane + 1));
                        pos += 2;
                    }
                    file.Write16(pos, 0);
                    pos += 2;
                }

                if (m_Children != null)
                {
                    foreach (OctreeNode child in m_Children)
                        child.WritePlaneData(file, ref pos);
                    return;
                }
            }
        }


        private void ClampRotation(ref float val, float twopi)
        {
            if (val > twopi)
            {
                while (val > twopi)
                    val -= twopi;
            }
            else if (val < -twopi)
            {
                while (val < -twopi)
                    val += twopi;
            }
        }


        private bool m_GLLoaded;

        class FaceDef
        {
            public int[] m_VtxIndices;
            public int[] m_TxcIndices;
            public int[] m_NrmIndices;
            public string m_MatName;
        }

        class MaterialDef
        {
            public List<FaceDef> m_Faces;

            public Color m_DiffuseColor;
            public int m_Opacity;

            public bool m_HasTextures;

            //public byte[] m_DiffuseMap;
            public string m_DiffuseMapName;
            public int m_DiffuseMapID;
            public Vector2 m_DiffuseMapSize;

            // haxx
            public float m_TexCoordScale;
        }

        private LevelSettings m_LevelSettings;

        // from loaded model
        private string m_ModelFileName;
        private string m_ModelPath;
        private List<Vector4> m_Vertices;
        private List<Vector2> m_TexCoords;
        private List<Vector3> m_Normals;
        private Dictionary<string, MaterialDef> m_Materials;
        private Dictionary<string, MaterialDef> m_Textures;
        private bool m_SketchupHack;

        // model settings
        private Vector3 m_Scale;
        private bool m_ZMirror;
        private bool m_SwapYZ;

        // camera
        private Vector2 m_CamRotation;
        private Vector3 m_CamTarget;
        private float m_CamDistance;
        private Vector3 m_CamPosition;
        private bool m_UpsideDown;
        private Matrix4 m_CamMatrix;
        private float m_PixelFactorX, m_PixelFactorY;

        // mouse
        private MouseButtons m_MouseDown;
        private Point m_LastMouseClick, m_LastMouseMove;
        private Point m_MouseCoords;
        private uint m_UnderCursor;

        // display
        private BMD m_MarioHeadModel;
        private BMD m_MarioBodyModel;
        private int m_PDisplayList;
        private int m_DisplayList;
        private uint[] m_PickingFrameBuffer;

        // hash
        private MD5CryptoServiceProvider m_MD5;

        // misc
        private Vector3 m_MarioPosition;
        private float m_MarioRotation;


        public ModelImporter()
        {
            InitializeComponent();

            Text = "[EXPERIMENTAL] Model importer - " + Program.AppTitle + " " + Program.AppVersion;

            m_GLLoaded = false;
            tbModelName.Text = "None";

            m_Scale = new Vector3(1f, 1f, 1f);
            m_ZMirror = false;
            m_SwapYZ = false;

            m_DisplayList = 0;

            m_MD5 = new MD5CryptoServiceProvider();
        }

        private string HexString(byte[] crap)
        {
            string ret = "";
            foreach (byte b in crap)
                ret += b.ToString("X2");
            return ret;
        }


        private void OBJ_LoadMTL(string filename)
        {
            Stream fs;
            try
            {
                fs = File.OpenRead(filename);
            }
            catch
            {
                MessageBox.Show("Material library not found:\n\n" + filename + "\n\nA default white material will be used instead.");
                MaterialDef mat = new MaterialDef();
                mat.m_Faces = new List<FaceDef>();
                mat.m_DiffuseColor = Color.White;
                mat.m_Opacity = 255;
                mat.m_HasTextures = false;
                mat.m_DiffuseMapName = "";
                mat.m_DiffuseMapID = 0;
                mat.m_DiffuseMapSize = new Vector2(0f, 0f);
                m_Materials.Add("default_white", mat);
                return;
            }
            StreamReader sr = new StreamReader(fs);

            string curmaterial = "";
            CultureInfo usahax = new CultureInfo("en-US");

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
                            mat.m_Faces = new List<FaceDef>();
                            mat.m_DiffuseColor = Color.White;
                            mat.m_Opacity = 255; // oops
                            mat.m_HasTextures = false;
                            mat.m_DiffuseMapName = "";
                            mat.m_DiffuseMapID = 0;
                            mat.m_DiffuseMapSize = new Vector2(0f, 0f);
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
                            Bitmap tex = new Bitmap(m_ModelPath + texname);

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
                        break;
                }
            }

            sr.Close();
        }

        private void LoadModel_OBJ()
        {
            Stream fs = File.OpenRead(m_ModelFileName);
            StreamReader sr = new StreamReader(fs);

            string curmaterial = "";
            CultureInfo usahax = new CultureInfo("en-US");

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

                    case "usemtl": // material name
                        if (parts.Length < 2) continue;
                        curmaterial = parts[1];
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
                                mat = (MaterialDef)m_Materials[curmaterial];
                            }
                            catch
                            {
                                curmaterial = "default_white";
                                if (m_Materials.Count == 0)
                                {
                                    MessageBox.Show("No material library has been specified, yet faces are still set to use \n" +
                                        "a material. A default white material will be used instead.");
                                    mat.m_Faces = new List<FaceDef>();
                                    mat.m_DiffuseColor = Color.White;
                                    mat.m_Opacity = 255;
                                    mat.m_HasTextures = false;
                                    mat.m_DiffuseMapName = "";
                                    mat.m_DiffuseMapID = 0;
                                    mat.m_DiffuseMapSize = new Vector2(0f, 0f);
                                    m_Materials.Add("default_white", mat);
                                }
                                mat = (MaterialDef)m_Materials[curmaterial];
                            }
                            FaceDef face = new FaceDef();
                            face.m_MatName = curmaterial;
                            face.m_VtxIndices = new int[nvtx];
                            face.m_TxcIndices = new int[nvtx];
                            face.m_NrmIndices = new int[nvtx];

                            for (int i = 0; i < nvtx; i++)
                            {
                                string vtx = parts[i + 1];
                                string[] idxs = vtx.Split(new char[] { '/' });

                                face.m_VtxIndices[i] = int.Parse(idxs[0]) - 1;
                                face.m_TxcIndices[i] = (mat.m_HasTextures && idxs.Length >= 2 && idxs[1].Length > 0)
                                    ? (int.Parse(idxs[1]) - 1) : -1;
                                face.m_NrmIndices[i] = (idxs.Length >= 3) ? (int.Parse(idxs[2]) - 1) : -1;
                            }

                            mat.m_Faces.Add(face);
                        }
                        break;
                }
            }

            sr.Close();
        }

        public bool m_EarlyClosure;

        private void LoadModel(bool required)
        {
            if (ofdLoadModel.ShowDialog(this) == DialogResult.OK)
            {
                m_ModelFileName = ofdLoadModel.FileName.Replace('/', '\\');
                m_ModelPath = m_ModelFileName.Substring(0, m_ModelFileName.LastIndexOf('\\') + 1);

                m_Vertices = new List<Vector4>();
                m_TexCoords = new List<Vector2>();
                m_Normals = new List<Vector3>();
                m_Materials = new Dictionary<string, MaterialDef>();
                m_Textures = new Dictionary<string, MaterialDef>();
                m_SketchupHack = false;

                switch (ofdLoadModel.FilterIndex)
                {
                    case 1: LoadModel_OBJ(); break;
                }
                PrerenderModel();

                tbModelName.Text = m_ModelFileName;
            }
            else if (required)
            {
                m_EarlyClosure = true;
                Close();
            }
        }


        private void UpdateCamera()
        {
            Vector3 up;

            if (Math.Cos(m_CamRotation.Y) < 0)
            {
                m_UpsideDown = true;
                up = new Vector3(0.0f, -1.0f, 0.0f);
            }
            else
            {
                m_UpsideDown = false;
                up = new Vector3(0.0f, 1.0f, 0.0f);
            }

            m_CamPosition.X = m_CamDistance * (float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);
            m_CamPosition.Y = m_CamDistance * (float)Math.Sin(m_CamRotation.Y);
            m_CamPosition.Z = m_CamDistance * (float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);

            Vector3 skybox_target;
            skybox_target.X = -(float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);
            skybox_target.Y = -(float)Math.Sin(m_CamRotation.Y);
            skybox_target.Z = -(float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);

            Vector3.Add(ref m_CamPosition, ref m_CamTarget, out m_CamPosition);

            m_CamMatrix = Matrix4.LookAt(m_CamPosition, m_CamTarget, up);
        }

        private void PrerenderModel()
        {
            m_MarioHeadModel = ModelCache.GetModel("data/player/mario_head_cap.bmd");
            m_MarioBodyModel = ModelCache.GetModel("data/player/mario_model.bmd");
            int[] mheaddl = ModelCache.GetDisplayLists(m_MarioHeadModel);
            int[] mbodydl = ModelCache.GetDisplayLists(m_MarioBodyModel);

            Vector3 mariopos = Vector3.Multiply(m_MarioPosition, m_Scale);

            if (m_DisplayList == 0)
                m_DisplayList = GL.GenLists(1);
            GL.NewList(m_DisplayList, ListMode.Compile);

            GL.FrontFace(FrontFaceDirection.Ccw);

            GL.Disable(EnableCap.Lighting);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Lines);
            GL.Color3(1f, 0f, 0f);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(500f, 0f, 0f);
            GL.Color3(0f, 1f, 0f);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 500f, 0f);
            GL.Color3(0f, 0f, 1f);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 0f, 500f);
            GL.End();

            GL.PushMatrix();
            GL.Translate(mariopos);
            GL.Begin(BeginMode.Lines);
            GL.Color3(1f, 1f, 0f);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, -500f, 0f);
            GL.End();
            GL.Rotate(m_MarioRotation, Vector3.UnitY);
            GL.Scale(0.008f, 0.008f, 0.008f);
            GL.CallList(mbodydl[0]);
            GL.CallList(mbodydl[1]);
            GL.Translate(0f, 11.25f, 0f);
            GL.Rotate(-90f, Vector3.UnitY);
            GL.Rotate(180f, Vector3.UnitX);
            GL.CallList(mheaddl[0]);
            GL.CallList(mheaddl[1]);
            GL.PopMatrix();

            GL.Disable(EnableCap.Lighting);
            GL.PushMatrix();
            if (m_ZMirror)
            {
                GL.Scale(m_Scale.X, m_Scale.Y, -m_Scale.Z);
                GL.FrontFace(FrontFaceDirection.Cw);
            }
            else
            {
                GL.Scale(m_Scale);
                GL.FrontFace(FrontFaceDirection.Ccw);
            }

            foreach (MaterialDef mat in m_Materials.Values)
            {
                if (mat.m_Opacity < 255) continue;

                GL.Color4(Color.FromArgb(mat.m_Opacity, mat.m_DiffuseColor));
                GL.BindTexture(TextureTarget.Texture2D, mat.m_DiffuseMapID);

                foreach (FaceDef face in mat.m_Faces)
                {
                    switch (face.m_VtxIndices.Length)
                    {
                        case 3: GL.Begin(BeginMode.Triangles); break;
                        case 4: GL.Begin(BeginMode.Quads); break;
                        default: GL.Begin(BeginMode.TriangleFan); break;
                    }

                    for (int i = 0; i < face.m_VtxIndices.Length; i++)
                    {
                        int vtx = face.m_VtxIndices[i];
                        int txc = face.m_TxcIndices[i];
                        int nrm = face.m_NrmIndices[i];

                        if (txc > -1) GL.TexCoord2(m_TexCoords[txc]);
                        //if (nrm > -1) GL.Normal3(m_Normals[nrm]);
                        if (m_SwapYZ)
                            GL.Vertex4(m_Vertices[vtx].X, m_Vertices[vtx].Z, m_Vertices[vtx].Y, m_Vertices[vtx].W);
                        else
                            GL.Vertex4(m_Vertices[vtx]);
                    }

                    GL.End();
                }
            }

            foreach (MaterialDef mat in m_Materials.Values)
            {
                if (mat.m_Opacity == 255) continue;

                GL.Color4(Color.FromArgb(mat.m_Opacity, mat.m_DiffuseColor));
                GL.BindTexture(TextureTarget.Texture2D, mat.m_DiffuseMapID);

                foreach (FaceDef face in mat.m_Faces)
                {
                    switch (face.m_VtxIndices.Length)
                    {
                        case 3: GL.Begin(BeginMode.Triangles); break;
                        case 4: GL.Begin(BeginMode.Quads); break;
                        default: GL.Begin(BeginMode.TriangleFan); break;
                    }

                    for (int i = 0; i < face.m_VtxIndices.Length; i++)
                    {
                        int vtx = face.m_VtxIndices[i];
                        int txc = face.m_TxcIndices[i];
                        int nrm = face.m_NrmIndices[i];

                        if (txc > -1) GL.TexCoord2(m_TexCoords[txc]);
                        //if (nrm > -1) GL.Normal3(m_Normals[nrm]);
                        if (m_SwapYZ)
                            GL.Vertex4(m_Vertices[vtx].X, m_Vertices[vtx].Z, m_Vertices[vtx].Y, m_Vertices[vtx].W);
                        else
                            GL.Vertex4(m_Vertices[vtx]);
                    }

                    GL.End();
                }
            }

            GL.PopMatrix();

            GL.EndList();

            if (m_PDisplayList == 0)
                m_PDisplayList = GL.GenLists(1);
            GL.NewList(m_PDisplayList, ListMode.Compile);

            GL.Color4(Color.FromArgb(0x66666666));
            GL.PushMatrix();
            GL.Translate(mariopos);
            GL.Rotate(m_MarioRotation, Vector3.UnitY);
            GL.Scale(0.008f, 0.008f, 0.008f);
            GL.CallList(mbodydl[2]);
            GL.Translate(0f, 11.25f, 0f);
            GL.Rotate(-90f, Vector3.UnitY);
            GL.Rotate(180f, Vector3.UnitX);
            GL.CallList(mheaddl[2]);
            GL.PopMatrix();

            GL.EndList();
        }

        private void ImportModel()
        {
            int b = 0;
            GXDisplayListPacker dlpacker = new GXDisplayListPacker();

            NitroFile bmd = Program.m_ROM.GetFileFromInternalID(m_LevelSettings.BMDFileID);

            if (!m_LevelSettings.editLevelBMDKCL)//If set to import an object model instead
            {
                bmd = Program.m_ROM.GetFileFromName(m_LevelSettings.objBMD);
            }
            bmd.Clear();

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

                float largesttc = 0f;
                foreach (FaceDef face in mat.m_Faces)
                {
                    foreach (int txci in face.m_TxcIndices)
                    {
                        if (txci < 0) continue;
                        Vector2 txc = Vector2.Multiply(m_TexCoords[txci], tcscale);
                        if (Math.Abs(txc.X) > largesttc) largesttc = Math.Abs(txc.X);
                        if (Math.Abs(txc.Y) > largesttc) largesttc = Math.Abs(txc.Y);
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
                Vector4 lastvtx = new Vector4(0f, 0f, 0f, 12345678f);
                foreach (FaceDef face in mat.m_Faces)
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

                        if (lastface != -1) dlpacker.AddCommand(0x41);
                        dlpacker.AddCommand(0x40, vtxtype);
                        if (lastface == -1) dlpacker.AddCommand(0x14, 0);

                        lastface = nvtx;
                    }

                    dlpacker.AddColorCommand(mat.m_DiffuseColor);

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
                dlpacker.AddCommand(0x41);
                byte[] dlist = dlpacker.GetDisplayList();

                bmd.Write32(curoffset, 1);
                bmd.Write32(curoffset + 0x4, curoffset + 0x10);
                bmd.Write32(curoffset + 0x8, (uint)dlist.Length);
                bmd.Write32(curoffset + 0xC, curoffset + 0x14);
                curoffset += 0x10;

                bmd.Write32(curoffset, 0);
                curoffset += 0x4;

                bmd.WriteBlock(curoffset, dlist);
                curoffset += (uint)dlist.Length;

                b++;
            }

            bmd.Write32(0x2C, curoffset);
            bmd.Write16(curoffset, 0);
            curoffset += 2;
            curoffset = (uint)((curoffset + 3) & ~3);

            // build bones
            bmd.Write32(0x4, 1);
            bmd.Write32(0x8, curoffset);

            uint bextraoffset = (uint)(curoffset + (0x40 * 1));

            {
                bmd.Write32(curoffset + 0x00, 0); // bone ID
                bmd.Write32(curoffset + 0x08, 0);
                bmd.Write32(curoffset + 0x0C, 0);
                bmd.Write32(curoffset + 0x10, 0x00001000);
                bmd.Write32(curoffset + 0x14, 0x00001000);
                bmd.Write32(curoffset + 0x18, 0x00001000);
                bmd.Write16(curoffset + 0x1C, 0x0000);
                bmd.Write16(curoffset + 0x1E, 0x0000);
                bmd.Write16(curoffset + 0x20, 0x0000);
                bmd.Write16(curoffset + 0x22, 0);
                bmd.Write32(curoffset + 0x24, 0);
                bmd.Write32(curoffset + 0x28, 0);
                bmd.Write32(curoffset + 0x2C, 0);
                bmd.Write32(curoffset + 0x30, (uint)m_Materials.Count);
                bmd.Write32(curoffset + 0x3C, 0);

                bmd.Write32(curoffset + 0x34, bextraoffset);
                for (byte j = 0; j < m_Materials.Count; j++)
                {
                    bmd.Write8(bextraoffset, j);
                    bextraoffset++;
                }

                bmd.Write32(curoffset + 0x38, bextraoffset);
                for (byte j = 0; j < m_Materials.Count; j++)
                {
                    bmd.Write8(bextraoffset, j);
                    bextraoffset++;
                }

                bmd.Write32(curoffset + 0x04, bextraoffset);
                bmd.WriteString(bextraoffset, "r0", 0);
                bextraoffset += 3;

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
                bmd.Write32(curoffset + 0x28, 0x00000000);
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

            uint textraoffset = (uint)(texoffset + (0x14 * ntex));

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

            uint pextraoffset = (uint)(paloffset + (0x10 * npal));

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

            bmd.SaveChanges();
        }
        List<CollisionPlane> lolplanes = null;
        //List<CollisionPolygon> lolpolys = null;
        // code inspired from Chadsoft's KclTool
        private void ImportCollisionMap()
        {
            NitroFile kcl = Program.m_ROM.GetFileFromInternalID(m_LevelSettings.KCLFileID); ;

            if (!m_LevelSettings.editLevelBMDKCL)
            {
                try
                {
                    kcl = Program.m_ROM.GetFileFromName(m_LevelSettings.objKCL);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString() + "\n\nThis object does not contain collision data, however the \n" +
                        "model will still be imported");
                }
            }
            kcl.Clear();

            Vector4[] scaledvtxs = new Vector4[m_Vertices.Count];
            for (int i = 0; i < m_Vertices.Count; i++)
            {
                if (m_SwapYZ)
                    scaledvtxs[i] = new Vector4(m_Vertices[i].X * m_Scale.X, m_Vertices[i].Z * m_Scale.Y, m_Vertices[i].Y * (m_ZMirror ? -m_Scale.Z : m_Scale.Z), m_Vertices[i].W);
                else
                    scaledvtxs[i] = new Vector4(m_Vertices[i].X * m_Scale.X, m_Vertices[i].Y * m_Scale.Y, m_Vertices[i].Z * (m_ZMirror ? -m_Scale.Z : m_Scale.Z), m_Vertices[i].W);
            }

            Vector3 min = Vector3.Zero, max = Vector3.Zero;
            foreach (Vector4 vec in scaledvtxs)
            {
                if (vec.X > max.X) max.X = vec.X;
                if (vec.Y > max.Y) max.Y = vec.Y;
                if (vec.Z > max.Z) max.Z = vec.Z;

                if (vec.X < min.X) min.X = vec.X;
                if (vec.Y < min.Y) min.Y = vec.Y;
                if (vec.Z < min.Z) min.Z = vec.Z;
            }
            //Vector3 lolwut = new Vector3(0.1f, 0.1f, 0.1f); // 997.f in KclTool's code, but it's too large for SM64DS's scale
            //Vector3.Subtract(ref min, ref lolwut, out min);
            //Vector3.Add(ref max, ref lolwut, out max);

            int maxdist = (int)Math.Max(max.X - min.X, Math.Max(max.Y - min.Y, max.Z - min.Z));
            int shift = 0;
            while (maxdist > 0)
            {
                maxdist >>= 1;
                shift++;
            }

            // KCL header
            uint posmask = (uint)~((1 << (shift + 10)) - 1);
            kcl.Write32(0x10, 0x00050000); // idk about this, really
            kcl.Write32(0x14, (uint)(int)(min.X * 64000f));
            kcl.Write32(0x18, (uint)(int)(min.Y * 64000f));
            kcl.Write32(0x1C, (uint)(int)(min.Z * 64000f));
            kcl.Write32(0x20, posmask);
            kcl.Write32(0x24, posmask);
            kcl.Write32(0x28, posmask);
            kcl.Write32(0x2C, (uint)(shift + 9));
            kcl.Write32(0x30, 1);
            kcl.Write32(0x34, 2);

            List<CollisionPlane> planes = new List<CollisionPlane>();
            //List<CollisionPolygon> polygons = new List<CollisionPolygon>();
            foreach (MaterialDef mat in m_Materials.Values)
            {
                foreach (FaceDef face in mat.m_Faces)
                {
                    if (face.m_VtxIndices.Length < 3)
                        continue;

                    List<Vector3> facepoly = new List<Vector3>();
                    foreach (int i in face.m_VtxIndices)
                    {
                        Vector3 v = scaledvtxs[i].Xyz;
                        Helper.RoundVector(ref v, 64000f);
                        if (!m_ZMirror)
                            facepoly.Insert(0, v);
                        else
                            facepoly.Add(v);
                    }

                    triangulate(planes, facepoly, Vector3.Zero);

                    continue;
                    //Yeap, all below this isn't run at all.
                    for (int i = 0; i < (face.m_VtxIndices.Length - 2); i++)
                    {
                        Vector3 a = scaledvtxs[face.m_VtxIndices[0]].Xyz;
                        Vector3 b = scaledvtxs[face.m_VtxIndices[i + 1]].Xyz;
                        Vector3 c = scaledvtxs[face.m_VtxIndices[i + 2]].Xyz;
                        ushort colltype = 0; // todo make this configurable

                        /*
                            [01:32:59]	dirbaio >	before doing anything at all with the vertexs
                            [01:33:09]	dirbaio >	round them all the same the DS would round them
                            [01:33:20]	dirbaio >	and do all the plane-cube calculations using the rounded values :P
                         */

                        Helper.RoundVector(ref a, 64000f);
                        Helper.RoundVector(ref b, 64000f);
                        Helper.RoundVector(ref c, 64000f);


                        CollisionPlane plane;
                        if (!m_ZMirror)
                            plane = new CollisionPlane(a, c, b, colltype, false);
                        else
                            plane = new CollisionPlane(a, b, c, colltype, false);

                        bool wasmerged = false;
                        /*foreach (CollisionPlane plane2 in planes)
                        {
                            if (plane.m_Type != plane2.m_Type) continue;
                            if (!Helper.VectorsEqual(plane.m_Normal, plane2.m_Normal)) continue;

                            if (plane2.TryMergeWith(plane))
                            {
                                wasmerged = true;
                                break;
                            }
                        }*/

                        if (!wasmerged) planes.Add(plane);

                        /*CollisionPolygon poly;
                        if (!m_ZMirror) poly = new CollisionPolygon(a, c, b, colltype);
                        else poly = new CollisionPolygon(a, b, c, colltype);

                        if (poly.m_Normal != Vector3.Zero)
                        {
                            bool dupe = false;
                            for (int j = 0; j < polygons.Count; j++)
                            {
                                CollisionPolygon lol = polygons[j];
                                if (poly.IsIdenticalTo(lol))
                                {
                                    dupe = true;
                                    break;
                                }
                            }

                            if (dupe) continue;

                            for (int j = 0; j < polygons.Count; )
                            {
                                CollisionPolygon lol = polygons[j];
                                if (poly.MergeWith(lol))
                                    polygons.RemoveAt(j);
                                else
                                    j++;
                            }

                            polygons.Add(poly);
                        }*/
                    }
                }
            }

            simplifyCollisionPlanes(planes);

            //lolpolys = polygons;
            lolplanes = planes;

            List<Vector3> points = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            foreach (CollisionPlane plane in planes)
            {
                for (int i = 0; i < points.Count; i++)
                    if (Helper.VectorsEqual(points[i], plane.m_Point))
                        plane.m_PointIndex = i;
                if (plane.m_PointIndex < 0)
                { plane.m_PointIndex = points.Count; points.Add(plane.m_Point); }

                for (int i = 0; i < normals.Count; i++)
                    if (Helper.VectorsEqual(normals[i], plane.m_Normal))
                        plane.m_NormalIndex = i;
                if (plane.m_NormalIndex < 0)
                { plane.m_NormalIndex = normals.Count; normals.Add(plane.m_Normal); }

                for (int i = 0; i < normals.Count; i++)
                    if (Helper.VectorsEqual(normals[i], plane.m_Dir1))
                        plane.m_Dir1Index = i;
                if (plane.m_Dir1Index < 0)
                { plane.m_Dir1Index = normals.Count; normals.Add(plane.m_Dir1); }

                for (int i = 0; i < normals.Count; i++)
                    if (Helper.VectorsEqual(normals[i], plane.m_Dir2))
                        plane.m_Dir2Index = i;
                if (plane.m_Dir2Index < 0)
                { plane.m_Dir2Index = normals.Count; normals.Add(plane.m_Dir2); }

                for (int i = 0; i < normals.Count; i++)
                    if (Helper.VectorsEqual(normals[i], plane.m_Dir3))
                        plane.m_Dir3Index = i;
                if (plane.m_Dir3Index < 0)
                { plane.m_Dir3Index = normals.Count; normals.Add(plane.m_Dir3); }
            }

            Vector3 size = new Vector3(1 << shift, 1 << shift, 1 << shift);
            OctreeNode octree = new OctreeNode(null, planes, min, size, 0, cbDropExtraShit.Checked);

            uint dataoffset = 0x38;

            // Section 1 -- points
            kcl.Write32(0x00, dataoffset);
            foreach (Vector3 point in points)
            {
                kcl.Write32(dataoffset, (uint)(int)(point.X * 64000f));
                kcl.Write32(dataoffset + 4, (uint)(int)(point.Y * 64000f));
                kcl.Write32(dataoffset + 8, (uint)(int)(point.Z * 64000f));
                dataoffset += 12;
            }

            // Section 2 -- normals
            kcl.Write32(0x04, dataoffset);
            foreach (Vector3 normal in normals)
            {
                kcl.Write16(dataoffset, (ushort)(short)(normal.X * 1024f));
                kcl.Write16(dataoffset + 2, (ushort)(short)(normal.Y * 1024f));
                kcl.Write16(dataoffset + 4, (ushort)(short)(normal.Z * 1024f));
                dataoffset += 6;
            }
            dataoffset = (uint)((dataoffset + 3) & ~3);

            // Section 3 -- planes
            kcl.Write32(0x08, dataoffset - 0x10);
            foreach (CollisionPlane plane in planes)
            {
                kcl.Write32(dataoffset, (uint)(int)(plane.m_Length * 65536000f));
                kcl.Write16(dataoffset + 4, (ushort)plane.m_PointIndex);
                kcl.Write16(dataoffset + 6, (ushort)plane.m_NormalIndex);
                kcl.Write16(dataoffset + 8, (ushort)plane.m_Dir1Index);
                kcl.Write16(dataoffset + 10, (ushort)plane.m_Dir2Index);
                kcl.Write16(dataoffset + 12, (ushort)plane.m_Dir3Index);
                kcl.Write16(dataoffset + 14, plane.m_Type);
                dataoffset += 16;
            }

            // Section 4 -- octree
            kcl.Write32(0xC, dataoffset);
            octree.WriteChildren(kcl, dataoffset, ref dataoffset);
            octree.WritePlaneData(kcl, ref dataoffset);

            kcl.SaveChanges();
            lolplanes = planes;
        }

        private void simplifyCollisionPlanes(List<CollisionPlane> planes)
        {
            //First get all vertexs in the model.
            //Remove duplicate ones for efficiency.
            List<Vector3> vertexs = new List<Vector3>();

            foreach (CollisionPlane p in planes)
            {
                addToList(vertexs, p.m_Point);
                addToList(vertexs, p.m_PointB);
                addToList(vertexs, p.m_PointC);
            }

            //Now iterate through all the vertexs.
            //Try to remove them :)
            //Repeat until we can't simplify anymore.

            bool simplified = true;

            while (simplified)
            {
                simplified = false;
                foreach (Vector3 vtx in vertexs)
                {
                    if (trySimplifyVertex(vtx, planes))
                    {
                        simplified = true;
                        break;
                    }
                }
            }
        }

        private bool trySimplifyVertex(Vector3 v, List<CollisionPlane> planes)
        {

            List<CollisionPlane> polygonPlanes = findPlanesWithVertex(v, planes);
            //            Console.Out.WriteLine("LOL " + polygonPlanes.Count + " triangles!");

            //No use in simplifying quads into quads. Let alone simple triangles.
            if (polygonPlanes.Count < 3)
                return false;

            //Check that the normal is equal for all polys 
            //TODO: Add some tolerance for float errors, so that nearly-equal normals are equal.
            Vector3 normal = polygonPlanes[0].m_Normal;

            foreach (CollisionPlane p in polygonPlanes)
                if (!Helper.VectorsEqual(normal, p.m_Normal))
                    return false;
            Console.Out.WriteLine("Detected " + polygonPlanes.Count + " flat triangles!");

            //Now try to reconstruct the polygon edges.
            //If we fail, don't try harder and return false.

            //First store all the edges in a graph.
            //If a triangle has vertexs V, A, B, store the edge A->B
            //These edges will be the edges of the polygon to be triangulated.

            List<Vector3> indexes = new List<Vector3>();
            Dictionary<int, List<int>> edges = new Dictionary<int, List<int>>();

            foreach (CollisionPlane p in polygonPlanes)
            {
                Vector3 a, b;
                if (Helper.VectorsEqual(p.m_Point, v))
                {
                    a = p.m_PointB;
                    b = p.m_PointC;
                }
                else if (Helper.VectorsEqual(p.m_PointB, v))
                {
                    a = p.m_PointC;
                    b = p.m_Point;
                }
                else if (Helper.VectorsEqual(p.m_PointC, v))
                {
                    a = p.m_Point;
                    b = p.m_PointB;
                }
                else throw new Exception("http://www.youtube.com/watch?v=2Z4m4lnjxkY");

                int indexa = addToList(indexes, a);
                int indexb = addToList(indexes, b);

                if (!edges.ContainsKey(indexa))
                    edges[indexa] = new List<int>();

                edges[indexa].Add(indexb);
            }

            //Now check the edges make a well-formed polygon.
            //If it's well-formed we can start in any vertex and go all the way round.
            int ind = 0;
            List<Vector3> polygon = new List<Vector3>();
            while (!vectorInList(polygon, indexes[ind]))
            {
                //Add vtx to the polygon
                Vector3 vtx = indexes[ind];
                polygon.Add(vtx);

                //Every edge should connect with another one. No more, no less.
                if (!edges.ContainsKey(ind) || edges[ind].Count != 1)
                    return false;

                //Go to next vertex
                ind = edges[ind][0];
            }

            //We have now gone round.
            //We should have gone through all the triangles.
            //If not, something weird's going on, we should stop.
            if (polygon.Count != polygonPlanes.Count)
                return false;

            Console.Out.WriteLine("...and they form a nice polygon!!");

            //Remove all the polys
            foreach (CollisionPlane p in polygonPlanes)
                planes.Remove(p);

            /*
            //Do dumb triangulation.
            //Will screw concave polys up.
            Vector3 center = polygon[0];
            for (int i = 2; i < polygon.Count; i++)
                planes.Add(new CollisionPlane(center, polygon[i - 1], polygon[i], 0, false));
            */

            //Dumb triangulation is dumb. Let's do something better.
            triangulate(planes, polygon, normal);
            return true;
        }

        private void triangulate(List<CollisionPlane> planes, List<Vector3> polygon, Vector3 normal)
        {
            //Clone the poly list because we're going to modify it.
            polygon = new List<Vector3>(polygon);

            //Lines? No thanks.
            if (polygon.Count < 3)
            {
                Console.WriteLine("Bad poly.");
                return;
            }

            int ct;

            //Try to simplify the polygon.
            bool simplified = true;
            while (simplified && polygon.Count >= 3)
            {
                simplified = false;
                ct = polygon.Count;

                for (int i = 0; i < ct; i++)
                {
                    Vector3 a = polygon[i];
                    Vector3 b = polygon[(i + 1) % ct];
                    Vector3 c = polygon[(i + 2) % ct];
                    Vector3 side1 = Vector3.Subtract(b, a);
                    Vector3 side2 = Vector3.Subtract(c, a);

                    //Two parallel or coninciding edges.
                    if (Helper.VectorsEqual(Vector3.Cross(side2, side1), Vector3.Zero))
                    {
                        //Just remove it.
                        polygon.RemoveAt((i + 1) % ct);
                        Console.WriteLine("Simplified 1 vertex out of " + ct);
                        simplified = true;
                        break;
                    }
                }
            }

            //Lines? No thanks. Check again.
            if (polygon.Count < 3)
            {
                Console.WriteLine("Bad poly after simplifying");
                return;
            }

            while (polygon.Count > 3)
            {
                //Find an "ear"
                ct = polygon.Count;

                int earidx = -1;

                for (int i = 0; i < ct; i++)
                {
                    Vector3 a = polygon[i];
                    Vector3 b = polygon[(i + 1) % ct];
                    Vector3 c = polygon[(i + 2) % ct];

                    //First check that the triangle is convex.
                    //If it isn't, the normal would be the other way =D

                    Vector3 side1 = Vector3.Subtract(b, a);
                    Vector3 side2 = Vector3.Subtract(c, a);

                    Vector3 normal2 = Vector3.Normalize(Vector3.Cross(side2, side1));

                    //If both normals point the same way
                    if (Helper.VectorsEqual(Vector3.Zero, normal) || Vector3.Dot(normal2, normal) > 0)
                    {
                        earidx = i;
                        break;
                    }

                    //TODO check that there are no points in the ear.
                }

                //We should always find ears. If not, yell it at Dirbaio.
                if (earidx == -1)
                {
                    //                    throw new Exception("No ear found?!");
                    break;
                }

                //Add the ear to the collision planes :D
                Vector3 sa = polygon[earidx];
                Vector3 sb = polygon[(earidx + 1) % ct];
                Vector3 sc = polygon[(earidx + 2) % ct];

                planes.Add(new CollisionPlane(sa, sb, sc, 0, false));

                //Remove it from the poly.
                polygon.RemoveAt((earidx + 1) % ct);
            }

            //Now add the remaining triangle.
            Vector3 da = polygon[0];
            Vector3 db = polygon[1];
            Vector3 dc = polygon[2];

            planes.Add(new CollisionPlane(da, db, dc, 0, false));
        }

        private List<CollisionPlane> findPlanesWithVertex(Vector3 v, List<CollisionPlane> planes)
        {
            List<CollisionPlane> result = new List<CollisionPlane>();
            foreach (CollisionPlane p in planes)
            {
                if (Helper.VectorsEqual(p.m_Point, v) ||
                    Helper.VectorsEqual(p.m_PointB, v) ||
                    Helper.VectorsEqual(p.m_PointC, v))
                    result.Add(p);
            }

            return result;
        }

        private bool vectorInList(List<Vector3> l, Vector3 p)
        {
            foreach (Vector3 v in l)
                if (Helper.VectorsEqual(v, p))
                    return true;
            return false;
        }

        private int addToList(List<Vector3> l, Vector3 p)
        {
            int i = 0;
            foreach (Vector3 v in l)
            {
                if (Helper.VectorsEqual(v, p))
                    return i;
                i++;
            }

            l.Add(p);
            return l.Count - 1;
        }

        private void glModelView_Load(object sender, EventArgs e)
        {
            m_MarioPosition = Vector3.Zero;
            m_MarioRotation = 0f;
            m_PickingFrameBuffer = new uint[9];
            m_GLLoaded = true;

            GL.Viewport(glModelView.ClientRectangle);

            float ratio = (float)glModelView.Width / (float)glModelView.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 projmtx = Matrix4.CreatePerspectiveFieldOfView((float)((70.0f * Math.PI) / 180.0f), ratio, 0.01f, 1000.0f);
            GL.MultMatrix(ref projmtx);

            m_PixelFactorX = ((2f * (float)Math.Tan((35f * Math.PI) / 180f) * ratio) / (float)(glModelView.Width));
            m_PixelFactorY = ((2f * (float)Math.Tan((35f * Math.PI) / 180f)) / (float)(glModelView.Height));

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.Texture2D);

            GL.LineWidth(2.0f);

            m_CamRotation = new Vector2(0.0f, (float)Math.PI / 8.0f);
            m_CamTarget = new Vector3(0.0f, 0.0f, 0.0f);
            m_CamDistance = 1.0f;
            UpdateCamera();

            GL.ClearColor(Color.FromArgb(0, 0, 32));

            LoadModel(true);
        }

        private void glModelView_Paint(object sender, PaintEventArgs e)
        {
            if (!m_GLLoaded) return;
            glModelView.Context.MakeCurrent(glModelView.WindowInfo);

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref m_CamMatrix);

            GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Dither);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PolygonSmooth);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Lighting);

            GL.CallList(m_PDisplayList);

            GL.Flush();
            GL.ReadPixels(m_MouseCoords.X - 1, glModelView.Height - m_MouseCoords.Y + 1, 3, 3, PixelFormat.Bgra, PixelType.UnsignedByte, m_PickingFrameBuffer);

            GL.ClearColor(0.0f, 0.0f, 0.125f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref m_CamMatrix);

            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Dither);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PolygonSmooth);
            GL.DepthMask(true);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.CallList(m_DisplayList);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            /*if (lolpolys != null)
            {
                Color[] rofl = { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Cyan, Color.Magenta, Color.White, Color.Orange };
                GL.LineWidth(5f);
                GL.Color3(Color.Blue);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                int num = 0;
                Random lolrand = new Random();
                foreach (CollisionPolygon poly in lolpolys)
                {
                    //int lol = 0;
                    GL.Begin(BeginMode.LineLoop);
                    Color clown = rofl[lolrand.Next(7)];
                    //GL.Begin(BeginMode.TriangleFan);
                    //GL.Begin(poly.m_Points.Length == 4 ? BeginMode.Quads : BeginMode.Triangles);
                    foreach (Vector3 pt in poly.m_Points)
                    //for (int i = poly.m_Points.Length-1; i >= 0; i--)
                    {
                        //Vector3 pt = poly.m_Points[i];
                        //if (poly.m_Points.Length == 4)
                            //GL.Color3(rofl[num & 7]);
                        GL.Color3(clown);
                        //else
                        //    GL.Color3(num==288 ? Color.SkyBlue : Color.Pink);
                        //lol++;
                        GL.Vertex3(pt);
                    }
                    GL.End();
                   num++;
                    //lol++;
                }
            }*/

            //bool lolz = false;
            if (lolplanes != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                foreach (CollisionPlane plane in lolplanes)
                {
                    GL.LineWidth(5f);
                    //GL.Begin(BeginMode.Lines);
                    //GL.Begin(BeginMode.LineLoop);
                    GL.Begin(BeginMode.Triangles);
                    GL.Color3(1f, 0f, 0f);
                    GL.Vertex3(plane.m_Point);
                    GL.Color3(0f, 1f, 0f);
                    GL.Vertex3(plane.m_PointC);
                    GL.Color3(0f, 0f, 1f);
                    GL.Vertex3(plane.m_PointB);
                    GL.End();
                    continue;
                    GL.Begin(BeginMode.Lines);
                    /* if (!lolz) { GL.Color3(1f, 0f, 1f); lolz = true; }
                     elseGL.Color3(1f, 0f, 0f);
                     Vector3 lol1 = Vector3.Cross(plane.m_Dir1, plane.m_Normal);
                     float lol1len = plane.m_Length / (float)Math.Cos(Math.Acos(Vector3.Dot(lol1, plane.m_Dir3)));
                     GL.Vertex3(plane.m_Point);
                     GL.Vertex3(Vector3.Add(plane.m_Point, Vector3.Multiply(lol1, lol1len)));

                     GL.Color3(0f, 1f, 0f);
                     Vector3 lol2 = Vector3.Cross(plane.m_Normal, plane.m_Dir2);
                     float lol2len = plane.m_Length / (float)Math.Cos(Math.Acos(Vector3.Dot(lol2, plane.m_Dir3)));
                     GL.Vertex3(plane.m_Point);
                     GL.Vertex3(Vector3.Add(plane.m_Point, Vector3.Multiply(lol2, lol2len)));*/


                    GL.Color3(0f, 0f, 1f);
                    GL.Vertex3(plane.m_Point);
                    GL.Vertex3(Vector3.Add(plane.m_Point, plane.m_Normal));
                    //GL.End();

                    GL.Color3(0f, 1f, 1f);
                    GL.Vertex3(plane.roflhax);
                    GL.Color3(0f, 0f, 1f);
                    GL.Vertex3(plane.lolcrap);
                    GL.End();
                }
            }

            /*GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Begin(BeginMode.Triangles);
            GL.Color3(Color.Red);
            GL.Vertex3(lolplanes[288].m_Point);
            GL.Vertex3(lolplanes[288].m_PointB);
            GL.Vertex3(lolplanes[288].m_PointC);
            GL.Color3(Color.Green);
            GL.Vertex3(lolplanes[289].m_Point);
            GL.Vertex3(lolplanes[289].m_PointB);
            GL.Vertex3(lolplanes[289].m_PointC);
            GL.End();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            foreach (OctreeNode node in OctreeNode.lollist)
            {
                if (node.m_Depth < 10) continue;
                if (node.m_PlaneData.Count <= 8) continue;

                GL.Color3(Color.Magenta);
                GL.Begin(BeginMode.LineLoop);
                GL.Vertex3(node._start);
                GL.Vertex3(node._start + new Vector3(node._size.X, 0, 0));
                GL.Vertex3(node._start + new Vector3(node._size.X, node._size.Y, 0));
                GL.Vertex3(node._start + new Vector3(0, node._size.Y, 0));
                GL.Vertex3(node._start + new Vector3(0, node._size.Y, node._size.Z));
                GL.Vertex3(node._start + new Vector3(node._size.X, node._size.Y, node._size.Z));
                GL.Vertex3(node._start + new Vector3(node._size.X, 0, node._size.Z));
                GL.Vertex3(node._start + new Vector3(0, 0, node._size.Z));
                GL.End();
            }
        }*/


            /*GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            //GL.Translate(-2.3357f+0.7f, -0.8f+0.7f, -1.9134f+0.7f);
            GL.Translate(-1.5233, -0.1, -0.1);
            GL.Scale(0.25f, 0.25f, 0.25f);
            GL.Disable(EnableCap.CullFace);
            GL.Color4(0f, 1f, 0f, 1f);
            GL.Begin(BeginMode.Quads);
            // horizontal
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(8f, 0f, 0f);
            GL.Vertex3(8f, 0f, 8f);
            GL.Vertex3(0f, 0f, 8f);
            GL.Vertex3(0f, 4f, 0f);
            GL.Vertex3(8f, 4f, 0f);
            GL.Vertex3(8f, 4f, 8f);
            GL.Vertex3(0f, 4f, 8f);
            GL.Vertex3(0f, 8f, 0f);
            GL.Vertex3(8f, 8f, 0f);
            GL.Vertex3(8f, 8f, 8f);
            GL.Vertex3(0f, 8f, 8f);
            // vertical X
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(8f, 0f, 0f);
            GL.Vertex3(8f, 8f, 0f);
            GL.Vertex3(0f, 8f, 0f);
            GL.Vertex3(0f, 0f, 4f);
            GL.Vertex3(8f, 0f, 4f);
            GL.Vertex3(8f, 8f, 4f);
            GL.Vertex3(0f, 8f, 4f);
            GL.Vertex3(0f, 0f, 8f);
            GL.Vertex3(8f, 0f, 8f);
            GL.Vertex3(8f, 8f, 8f);
            GL.Vertex3(0f, 8f, 8f);
            // vertical Z
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 0f, 8f);
            GL.Vertex3(0f, 8f, 8f);
            GL.Vertex3(0f, 8f, 0f);
            GL.Vertex3(4f, 0f, 0f);
            GL.Vertex3(4f, 0f, 8f);
            GL.Vertex3(4f, 8f, 8f);
            GL.Vertex3(4f, 8f, 0f);
            GL.Vertex3(8f, 0f, 0f);
            GL.Vertex3(8f, 0f, 8f);
            GL.Vertex3(8f, 8f, 8f);
            GL.Vertex3(8f, 8f, 0f);
            GL.End();
            GL.Begin(BeginMode.Lines);
            GL.Color3(1f, 1f, 0f);
            GL.Vertex3(2f, 0f, 2f);
            GL.Vertex3(2f, 8f, 2f);
            GL.Vertex3(6f, 0f, 2f);
            GL.Vertex3(6f, 8f, 2f);
            GL.Vertex3(2f, 0f, 6f);
            GL.Vertex3(2f, 8f, 6f);
            GL.Vertex3(6f, 0f, 6f);
            GL.Vertex3(6f, 8f, 6f);
            GL.End();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);*/


            glModelView.SwapBuffers();
        }

        private void glModelView_Resize(object sender, EventArgs e)
        {
            if (!m_GLLoaded) return;
            glModelView.Context.MakeCurrent(glModelView.WindowInfo);

            GL.Viewport(glModelView.ClientRectangle);

            float ratio = (float)glModelView.Width / (float)glModelView.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 projmtx = Matrix4.CreatePerspectiveFieldOfView((float)((70.0f * Math.PI) / 180.0f), ratio, 0.01f, 1000.0f);
            GL.MultMatrix(ref projmtx);

            m_PixelFactorX = ((2f * (float)Math.Tan((35f * Math.PI) / 180f) * ratio) / (float)(glModelView.Width));
            m_PixelFactorY = ((2f * (float)Math.Tan((35f * Math.PI) / 180f)) / (float)(glModelView.Height));
        }

        private void btnOpenModel_Click(object sender, EventArgs e)
        {
            LoadModel(false);
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Vector3 originalScale = new Vector3(0, 0, 0);
            float faceSizeThreshold = 0.001f;
            if (txtThreshold.Text == "")
                faceSizeThreshold = 0.001f;//Default value
            else
            {
                try
                { faceSizeThreshold = Convert.ToSingle(txtThreshold.Text); }
                catch { MessageBox.Show(txtThreshold.Text + "\nis not a valid float value. Please enter a value in format 0.123"); return; }
            }
            NitroFile kcl;//This'll hold the KCL file that is to be replaced, either a level's or an object's
            if (!m_LevelSettings.editLevelBMDKCL)//If we're importing an object's model
            {
                //If it's an object it'll be scaled down - need to get back to original value
                originalScale = m_Scale;
                m_Scale = m_Scale * (1 / ObjectRenderer.currentObjScale);
                PrerenderModel();
                glModelView.Refresh();
                ImportModel();

                m_Scale = originalScale;//Back to previous scale for collision as it's not affected like model's scale
                PrerenderModel();
                glModelView.Refresh();
                try
                {
                    kcl = Program.m_ROM.GetFileFromName(m_LevelSettings.objKCL);
                    ObjToKcl.ConvertToKcl(m_ModelFileName, ref kcl, m_Scale.X, faceSizeThreshold);
                }
                catch
                {
                    MessageBox.Show("This object has no collision data, however the model will still be imported.");
                }
            }
            else//If it's a level model
            {
                ImportModel();
                kcl = Program.m_ROM.GetFileFromInternalID(m_LevelSettings.KCLFileID);
                if (cbMakeAcmlmboard.Checked)
                    ObjToKcl.ConvertToKcl(m_ModelFileName, ref kcl, m_Scale.X, faceSizeThreshold);
            }

            //if (cbMakeAcmlmboard.Checked) ImportCollisionMap();//Old method
            ((LevelEditorForm)Owner).UpdateLevelModel();
        }

        private void glModelView_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_MouseDown != MouseButtons.None) return;
            m_MouseDown = e.Button;
            m_LastMouseClick = e.Location;
            m_LastMouseMove = e.Location;

            if ((m_PickingFrameBuffer[4] == m_PickingFrameBuffer[1]) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[3]) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[5]) &&
                (m_PickingFrameBuffer[4] == m_PickingFrameBuffer[7]))
                m_UnderCursor = m_PickingFrameBuffer[4];
            else
                m_UnderCursor = 0xFFFFFFFF;
        }

        private void glModelView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != m_MouseDown) return;
            m_MouseDown = MouseButtons.None;
            m_UnderCursor = 0xFFFFFFFF;
        }

        private void glModelView_MouseMove(object sender, MouseEventArgs e)
        {
            float xdelta = (float)(e.X - m_LastMouseMove.X);
            float ydelta = (float)(e.Y - m_LastMouseMove.Y);

            m_MouseCoords = e.Location;
            m_LastMouseMove = e.Location;

            if (m_MouseDown != MouseButtons.None)
            {
                if (m_UnderCursor == 0x66666666)
                {
                    if (m_MouseDown == MouseButtons.Right)
                    {
                        if (m_UpsideDown)
                            xdelta = -xdelta;

                        // TODO take obj/camera rotation into account?
                        m_MarioRotation += xdelta * 0.5f;

                        if (m_MarioRotation >= 180f)
                        {
                            m_MarioRotation = (float)(-360f + m_MarioRotation);
                        }
                        else if (m_MarioRotation < -180f)
                        {
                            m_MarioRotation = (float)(360f + m_MarioRotation);
                        }
                    }
                    else if (m_MouseDown == MouseButtons.Left)
                    {
                        Vector3 between;
                        Vector3.Subtract(ref m_CamPosition, ref m_MarioPosition, out between);

                        float objz = (((between.X * (float)Math.Cos(m_CamRotation.X)) + (between.Z * (float)Math.Sin(m_CamRotation.X))) * (float)Math.Cos(m_CamRotation.Y)) + (between.Y * (float)Math.Sin(m_CamRotation.Y));
                        objz /= m_Scale.X;

                        xdelta *= m_PixelFactorX * objz;
                        ydelta *= -m_PixelFactorY * objz;

                        float _xdelta = (xdelta * (float)Math.Sin(m_CamRotation.X)) - (ydelta * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Cos(m_CamRotation.X));
                        float _ydelta = ydelta * (float)Math.Cos(m_CamRotation.Y);
                        float _zdelta = (xdelta * (float)Math.Cos(m_CamRotation.X)) + (ydelta * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Sin(m_CamRotation.X));

                        Vector3 offset = new Vector3(_xdelta, _ydelta, -_zdelta);
                        Vector3.Add(ref m_MarioPosition, ref offset, out m_MarioPosition);
                    }

                    PrerenderModel();
                }
                else
                {
                    if (m_MouseDown == MouseButtons.Right)
                    {
                        /*if (btnReverseRot.Checked)
                        {
                            xdelta = -xdelta;
                            ydelta = -ydelta;
                        }*/

                        if (m_UpsideDown)
                            xdelta = -xdelta;

                        m_CamRotation.X -= xdelta * 0.002f;
                        m_CamRotation.Y -= ydelta * 0.002f;

                        ClampRotation(ref m_CamRotation.X, (float)Math.PI * 2.0f);
                        ClampRotation(ref m_CamRotation.Y, (float)Math.PI * 2.0f);
                    }
                    else if (m_MouseDown == MouseButtons.Left)
                    {
                        xdelta *= 0.005f;
                        ydelta *= 0.005f;

                        m_CamTarget.X -= xdelta * (float)Math.Sin(m_CamRotation.X);
                        m_CamTarget.X -= ydelta * (float)Math.Cos(m_CamRotation.X) * (float)Math.Sin(m_CamRotation.Y);
                        m_CamTarget.Y += ydelta * (float)Math.Cos(m_CamRotation.Y);
                        m_CamTarget.Z += xdelta * (float)Math.Cos(m_CamRotation.X);
                        m_CamTarget.Z -= ydelta * (float)Math.Sin(m_CamRotation.X) * (float)Math.Sin(m_CamRotation.Y);
                    }

                    UpdateCamera();
                }
            }

            glModelView.Refresh();
        }

        private void glModelView_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((m_MouseDown == MouseButtons.Left) && (m_UnderCursor == 0x66666666))
            {
                float delta = -(e.Delta / 120f);
                delta = ((delta < 0f) ? -1f : 1f) * (float)Math.Pow(delta, 2f) * 0.05f;
                delta /= m_Scale.X;

                Vector3 offset = Vector3.Zero;
                offset.X += delta * (float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);
                offset.Y += delta * (float)Math.Sin(m_CamRotation.Y);
                offset.Z += delta * (float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);

                float xdist = delta * (m_MouseCoords.X - (glModelView.Width / 2f)) * m_PixelFactorX;
                float ydist = delta * (m_MouseCoords.Y - (glModelView.Height / 2f)) * m_PixelFactorY;

                offset.X -= (xdist * (float)Math.Sin(m_CamRotation.X)) + (ydist * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Cos(m_CamRotation.X));
                offset.Y += ydist * (float)Math.Cos(m_CamRotation.Y);
                offset.Z += (xdist * (float)Math.Cos(m_CamRotation.X)) - (ydist * (float)Math.Sin(m_CamRotation.Y) * (float)Math.Sin(m_CamRotation.X));

                Vector3.Add(ref m_MarioPosition, ref offset, out m_MarioPosition);

                PrerenderModel();
            }
            else
            {
                float delta = -((e.Delta / 120.0f) * 0.1f);
                m_CamTarget.X += delta * (float)Math.Cos(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);
                m_CamTarget.Y += delta * (float)Math.Sin(m_CamRotation.Y);
                m_CamTarget.Z += delta * (float)Math.Sin(m_CamRotation.X) * (float)Math.Cos(m_CamRotation.Y);

                UpdateCamera();
            }
            glModelView.Refresh();
        }

        private void cbZMirror_CheckedChanged(object sender, EventArgs e)
        {
            m_ZMirror = cbZMirror.Checked;
            PrerenderModel();
            glModelView.Refresh();
        }

        private void tbScale_TextChanged(object sender, EventArgs e)
        {
            float val;
            if (float.TryParse(tbScale.Text, out val) || float.TryParse(tbScale.Text, NumberStyles.Float, new CultureInfo("en-US"), out val))
            {
                m_Scale = new Vector3(val, val, val);
                PrerenderModel();
                glModelView.Refresh();
            }
        }

        private void ModelImporter_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_EarlyClosure) return;

            GL.DeleteLists(m_PDisplayList, 1);
            GL.DeleteLists(m_DisplayList, 1);

            foreach (MaterialDef mat in m_Materials.Values)
                GL.DeleteTexture(mat.m_DiffuseMapID);

            ModelCache.RemoveModel(m_MarioHeadModel);
            ModelCache.RemoveModel(m_MarioBodyModel);
        }

        private void ModelImporter_Load(object sender, EventArgs e)
        {
            m_LevelSettings = ((LevelEditorForm)Owner).m_LevelSettings;

            // hide unimplemented shit :)
            groupBox2.Visible = false;
        }

        private void cbMakeAcmlmboard_CheckedChanged(object sender, EventArgs e)
        {
            cbDropExtraShit.Enabled = cbMakeAcmlmboard.Checked;
        }

        private void cbSwapYZ_CheckedChanged(object sender, EventArgs e)
        {
            m_SwapYZ = cbSwapYZ.Checked;
            PrerenderModel();
            glModelView.Refresh();
        }
    }
}
