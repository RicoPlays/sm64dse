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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;

namespace SM64DSe
{
    static class Helper
    {
        public static ushort ColorToBGR15(Color color)
        {
            uint r = (uint)((color.R & 0xF8) >> 3);
            uint g = (uint)((color.G & 0xF8) << 2);
            uint b = (uint)((color.B & 0xF8) << 7);
            return (ushort)(r | g | b);
        }

        public static Color BGR15ToColor(ushort bgr15)
        {
            byte red = (byte)((bgr15 << 3) & 0xF8);
            byte green = (byte)((bgr15 >> 2) & 0xF8);
            byte blue = (byte)((bgr15 >> 7) & 0xF8);

            // compensate the lower 3 bits (so that for example 7FFF -> (FF,FF,FF) instead of (F8,F8,F8))
            red |= (byte)(red >> 5);
            green |= (byte)(green >> 5);
            blue |= (byte)(blue >> 5);

            return Color.FromArgb(red, green, blue);
        }

        public static ushort BlendColorsBGR15(ushort c1, int w1, ushort c2, int w2)
        {
            int r1 = c1 & 0x1F;
            int g1 = (c1 >> 5) & 0x1F;
            int b1 = (c1 >> 10) & 0x1F;
            int r2 = c2 & 0x1F;
            int g2 = (c2 >> 5) & 0x1F;
            int b2 = (c2 >> 10) & 0x1F;

            int rf = ((r1 * w1) + (r2 * w2)) / (w1 + w2);
            int gf = ((g1 * w1) + (g2 * w2)) / (w1 + w2);
            int bf = ((b1 * w1) + (b2 * w2)) / (w1 + w2);
            return (ushort)(rf | (gf << 5) | (bf << 10));
        }

        public static bool VectorsEqual(Vector3 a, Vector3 b)
        {
            float epsilon = 0.00001f;
            if (Math.Abs(a.X - b.X) > epsilon) return false;
            if (Math.Abs(a.Y - b.Y) > epsilon) return false;
            if (Math.Abs(a.Z - b.Z) > epsilon) return false;
            return true;
        }

        public static void RoundVector(ref Vector3 v, float fpcoef)
        {
            v.X = (float)Math.Round((double)(v.X * fpcoef)) / fpcoef;
            v.Y = (float)Math.Round((double)(v.Y * fpcoef)) / fpcoef;
            v.Z = (float)Math.Round((double)(v.Z * fpcoef)) / fpcoef;
        }

        public static bool PointOnLine(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 line; Vector3.Subtract(ref b, ref a, out line);
            Vector3 d; Vector3.Subtract(ref p, ref a, out d);
            Vector3 ratio; Vector3.Divide(ref d, ref line, out ratio);

            float epsilon = 0.00001f;
            if (Math.Abs(ratio.X - ratio.Y) > epsilon || Math.Abs(ratio.X - ratio.Z) > epsilon)
                return false;

            if (ratio.X < 0f || ratio.X > 1f)
                return false;

            return true;
        }

        public static Matrix4 SRTToMatrix(Vector3 scale, Vector3 rot, Vector3 trans)
        {
            Matrix4 ret = Matrix4.Identity;

            Matrix4 mscale = Matrix4.Scale(scale);
            Matrix4 mxrot = Matrix4.CreateRotationX(rot.X);
            Matrix4 myrot = Matrix4.CreateRotationY(rot.Y);
            Matrix4 mzrot = Matrix4.CreateRotationZ(rot.Z);
            Matrix4 mtrans = Matrix4.CreateTranslation(trans);

            Matrix4.Mult(ref ret, ref mscale, out ret);
            Matrix4.Mult(ref ret, ref mxrot, out ret);
            Matrix4.Mult(ref ret, ref myrot, out ret);
            Matrix4.Mult(ref ret, ref mzrot, out ret);
            Matrix4.Mult(ref ret, ref mtrans, out ret);

            return ret;
        }
    }
}
