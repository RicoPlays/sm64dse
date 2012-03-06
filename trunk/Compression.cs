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

namespace SM64DSe
{
    // regular LZ77, preceded by a LZ77 tag
    static class LZ77
    {
        public static void Decompress(ref byte[] data, bool hastag)
        {
            uint header = (uint)(data[hastag?4:0] | (data[hastag?5:1] << 8) | (data[hastag?6:2] << 16) | (data[hastag?7:3] << 24));

            uint xLen = (header >> 8);
            byte[] dest = new byte[xLen];

            uint xIn = (uint)(hastag ? 8 : 4);
            uint xOut = 0;

            while (xLen > 0)
            {
                byte d = data[xIn++];

                for (uint i = 0; i < 8; i++)
                {
                    if ((d & 0x80) == 0x80)
                    {
                        ushort stuff = (ushort)((data[xIn] << 8) | data[xIn + 1]);
                        xIn += 2;

                        uint len = (uint)((stuff >> 12) + 3);
                        uint offset = (uint)(stuff & 0xFFF);
                        uint windowOffset = (xOut - offset - 1);

                        for (uint j = 0; j < len; j++)
                        {
                            dest[xOut++] = dest[windowOffset++];

                            xLen--;
                            if (xLen == 0)
                            {
                                Array.Resize(ref data, dest.Length);
                                dest.CopyTo(data, 0);
                                return;
                            }
                        }
                    }
                    else
                    {
                        dest[xOut++] = data[xIn++];

                        xLen--;
                        if (xLen == 0)
                        {
                            Array.Resize(ref data, dest.Length);
                            dest.CopyTo(data, 0);
                            return;
                        }
                    }

                    d <<= 1;
                }
            }

            Array.Resize(ref data, dest.Length);
            dest.CopyTo(data, 0);
        }
    }


    // Jap77 (backwards LZ77 variant)
    // used by overlays for it doesn't require decompression to be done in a separate buffer
    static class Jap77
    {
        public static void Decompress(ref byte[] data)
        {
            int size = data.Length;

            uint param1, decomp_len;
            param1 = (uint)(data[size - 8] | (data[size - 7] << 8) | (data[size - 6] << 16) | (data[size - 5] << 24));
            decomp_len = (uint)(size + (data[size - 4] | (data[size - 3] << 8) | (data[size - 2] << 16) | (data[size - 1] << 24)));

            int inpos = (int)(size - (param1 >> 24) - 1);
            int limit = (int)(size - (param1 & 0x00FFFFFF));
            int outpos = (int)(decomp_len - 1);

            Array.Resize(ref data, (int)decomp_len);

            for (; ; )
            {
                if (inpos <= limit) break;
                byte blockctl = data[inpos--];
                if (inpos <= limit) break;

                bool done = false;

                for (int i = 0; i < 8; i++)
                {
                    if ((blockctl & 0x80) == 0x80)
                    {
                        if (inpos <= limit) { done = true; break; }

                        ushort stuff = (ushort)(data[inpos - 1] | (data[inpos] << 8));
                        inpos -= 2;
                        int wdisp = (stuff & 0x0FFF) + 2;
                        int wsize = (stuff >> 12) + 2;

                        for (int j = wsize; j >= 0; j--)
                        {
                            data[outpos] = data[outpos + wdisp + 1];
                            outpos--;
                        }
                    }
                    else
                    {
                        if (inpos <= limit) { done = true; break; }
                        data[outpos--] = data[inpos--];
                    }

                    blockctl <<= 1;
                }

                if (done) break;
            }
        }
    }
}
