﻿/*
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
    public class NARC : NitroFile
    {
        public NARC(NitroROM rom, ushort id) : base(rom, id)
        {
            FATOffset = 0x1C;
            FATSize = Read32(0x14) - 0xC;
            FNTOffset = 0x1C + FATSize + 0x8;
            FNTSize = Read32(FNTOffset - 0x4) - 0x8;
            IMGOffset = FNTOffset + FNTSize + 0x8;
            IMGSize = Read32(IMGOffset - 0x4) - 0x8;

            ushort numdirs = Read16(FNTOffset + 0x6);
            ushort numfiles = (ushort)(FATSize / 8);

            m_DirEntries = new DirEntry[numdirs];
            m_FileEntries = new FileEntry[numfiles];

            for (ushort f = 0; f < numfiles; f++)
            {
                uint start = Read32((uint)(FATOffset + (f*8)));
                uint end = Read32((uint)(FATOffset + (f*8) + 4));

                FileEntry fe;
                fe.ID = f;
                fe.ParentID = 0;
                fe.Offset = start;
                fe.Size = end - start;
                fe.Name = fe.FullName = "";
                m_FileEntries[f] = fe;
            }

            DirEntry root;
            root.ID = 0xF000;
            root.ParentID = 0;
            root.Name = root.FullName = "";
            m_DirEntries[0] = root;

            uint tableoffset = FNTOffset;
            for (ushort d = 0; d < numdirs; d++)
            {
                uint subtableoffset = FNTOffset + Read32(tableoffset);
                ushort first_fileid = Read16(tableoffset + 0x4);
                ushort cur_fileid = first_fileid;

                for (; ; )
                {
                    byte type_len = Read8(subtableoffset);
                    subtableoffset++;

                    if (type_len == 0x00) break;
                    else if (type_len > 0x80)
                    {
                        DirEntry dir;

                        dir.Name = ReadString(subtableoffset, type_len & 0x7F);
                        subtableoffset += (uint)(type_len & 0x7F);
                        dir.ID = Read16(subtableoffset);
                        subtableoffset += 0x2;
                        dir.ParentID = (ushort)(d + 0xF000);
                        dir.FullName = "";

                        m_DirEntries[dir.ID - 0xF000] = dir;
                    }
                    else if (type_len < 0x80)
                    {
                        m_FileEntries[cur_fileid].ParentID = (ushort)(d + 0xF000);
                        m_FileEntries[cur_fileid].Name = ReadString(subtableoffset, type_len & 0x7F);
                        subtableoffset += (uint)(type_len & 0x7F);
                        cur_fileid++;
                    }
                }

                tableoffset += 8;
            }

            for (int i = 0; i < m_DirEntries.Length; i++)
            {
                if (m_DirEntries[i].ParentID > 0xF000)
                    m_DirEntries[i].FullName = m_DirEntries[m_DirEntries[i].ParentID - 0xF000].FullName + "/" + m_DirEntries[i].Name;
                else
                    m_DirEntries[i].FullName = m_DirEntries[i].Name;
            }

            for (int i = 0; i < m_FileEntries.Length; i++)
            {
                if (m_FileEntries[i].ParentID > 0xF000)
                    m_FileEntries[i].FullName = m_DirEntries[m_FileEntries[i].ParentID - 0xF000].FullName + "/" + m_FileEntries[i].Name;
                else
                    m_FileEntries[i].FullName = m_FileEntries[i].Name;
            }
        }

        public ushort GetFileIDFromName(string name)
        {
            foreach (FileEntry fe in m_FileEntries)
            {
                if (fe.FullName == name)
                    return fe.ID;
            }

            return 0xFFFF;
        }

        public string GetFileNameFromID(ushort id)
        {
            return m_FileEntries[id].FullName;
        }

        public void MakeRoom(uint addr, uint amount)
        {
            int filelen = m_Data.Length;

            byte[] tomove = ReadBlock(addr, (uint)(filelen - addr));
            WriteBlock(addr + amount, tomove);
        }

        public byte[] ExtractFile(ushort fileid)
        {
            FileEntry fe = m_FileEntries[fileid];
            return ReadBlock(IMGOffset + fe.Offset, fe.Size);
        }

        public void ReinsertFile(ushort fileid, byte[] data)
        {
            int datalength = (data.Length + 3) & ~3;

            FileEntry fe = m_FileEntries[fileid];

            UInt32 fileend = IMGOffset + fe.Offset + fe.Size;
            int delta = (int)(datalength - fe.Size);

            // move data that comes after the file
            MakeRoom(fileend, (uint)delta);

            // write the new data for the file
            WriteBlock(IMGOffset + fe.Offset, data);
            fe.Size = (uint)datalength;

            // fix the FAT
            for (uint f = 0; f < (FATSize / 8); f++)
            {
                if (f != fileid)
                {
                    uint start = Read32(FATOffset + (f * 8));
                    if (start >= fileend)
                    {
                        start += (uint)delta;
                        Write32(FATOffset + (f * 8), start);
                    }
                }

                uint end = Read32(FATOffset + (f * 8) + 4);
                if (end >= fileend)
                {
                    end += (uint)delta;
                    Write32(FATOffset + (f * 8) + 4, end);
                }
            }

            // fix misc stuff
            IMGSize += (uint)delta;
            Write32(IMGOffset - 0x4, IMGSize + 0x8);

            Write32(0x8, (uint)((m_Data.Length + 3) & ~3));

            SaveChanges();
        }


        private uint FNTOffset, FNTSize;
        private uint FATOffset, FATSize;
        private uint IMGOffset, IMGSize;

        private struct DirEntry
        {
            public ushort ID;
            public ushort ParentID;
            public string Name;
            public string FullName;
        }

        private struct FileEntry
        {
            public ushort ID;
            public ushort ParentID;
            public string Name;
            public string FullName;
            public uint Offset;
            public uint Size;
        }

        private DirEntry[] m_DirEntries;
        private FileEntry[] m_FileEntries;
    }
}
