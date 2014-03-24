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
using System.Globalization;

namespace SM64DSe
{
    static class Helper
    {
        public static uint ACT_SELECTOR_ID_TABLE;
        public static CultureInfo USA = new CultureInfo("en-US");

        static Helper()
        {
            switch (Program.m_ROM.m_Version)
            {
                case NitroROM.Version.EUR:
                    ACT_SELECTOR_ID_TABLE = 0x75298;
                    break;
                case NitroROM.Version.USA_v1:
                    ACT_SELECTOR_ID_TABLE = 0x731F0;
                    break;
                case NitroROM.Version.USA_v2:
                    ACT_SELECTOR_ID_TABLE = 0x73F10;
                    break;
                case NitroROM.Version.JAP:
                    ACT_SELECTOR_ID_TABLE = 0x73744;
                    break;
            }
        }

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

        public static uint GetActSelectorIDTableAddress()
        {
            return ACT_SELECTOR_ID_TABLE;
        }

        public static void DecompressOverlaysWithinGame()
        {
            if (CheckAllOverlaysDecompressed())
                return;

            for (int i = 0; i < 155; i++)
            {
                NitroOverlay overlay = new NitroOverlay(Program.m_ROM, (uint)i);
                // Overlay is decompressed when initialised above automatically if needed, just need to save changes
                overlay.SaveChanges();
            }
        }

        public static bool CheckAllOverlaysDecompressed()
        {
            bool allDecompressed = true;
            for (int i = 0; i < 155; i++)
            {
                if (CheckOverlayCompressed((uint)i) == true)
                {
                    allDecompressed = false;
                    break;
                }
            }
            return allDecompressed;
        }

        public static bool CheckOverlayCompressed(uint id)
        {
            uint OVTEntryAddr = Program.m_ROM.GetOverlayEntryOffset(id);
            Byte flags = Program.m_ROM.Read8(OVTEntryAddr + 0x1F);

            if ((flags & 0x01) == 0x01)
                return true;
            else
                return false;
        }

        /*
         * Converts a Hex Dump to binary file
         */ 
        public static byte[] HexDumpToBinary(string hexdump)
        {
            string[] lines = hexdump.Split('\n');
            string[][] hexBytes = new string[lines.Length][];
            // For now just assume <ADDRESS> <hex data 1> ... <hex data 8>
            int startIndex = (lines[0].Split(' ').Length >= 17) ? 1 : 0;
            int endIndex = (lines[0].Split(' ').Length >= 17) ? 16 : 15;
            int numBytes = 0;

            // Get string array of each individual byte in hex format
            for (int i = 0; i < lines.Length; i++)
            {
                string[] split = lines[i].Replace('-', ' ').Split(' ');// Some files have "-" separator in middle
                if (split.Length == 1)
                    break;// End of file
                string[] hexData = new string[(endIndex + 1) - startIndex];
                for (int j = startIndex, k = 0; j <= endIndex; j++, k++)
                {
                    hexData[k] = split[j];
                }
                hexBytes[i] = hexData;
                numBytes += hexData.Length;
            }

            // Convert string hex representation of each byte to actual byte
            byte[] binaryData = new byte[numBytes];
            int count = 0;
            for (int i = 0; i < hexBytes.Length; i++)
            {
                if (hexBytes[i] == null)
                    break;
                for (int j = 0; j < hexBytes[i].Length; j++)
                {
                    binaryData[count] = byte.Parse(hexBytes[i][j], System.Globalization.NumberStyles.HexNumber);
                    count++;
                }
            }

            return binaryData;
        }
    }

    // Taken from http://stackoverflow.com/questions/1440392/use-byte-as-key-in-dictionary
    // By user: JaredPar

    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }
            return left.SequenceEqual(right);
        }
        public int GetHashCode(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            int sum = 0;
            foreach (byte cur in key)
            {
                sum = 33 * sum + cur;
            }
            return sum;
        }
    }
}
