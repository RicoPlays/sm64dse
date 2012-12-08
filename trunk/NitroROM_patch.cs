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
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;
using System.ComponentModel;

namespace SM64DSe
{
    public partial class NitroROM
    {
        public const uint ROM_PATCH_VERSION = 3;
        public const uint LEVEL_OVERLAY_SIZE = 32768;

        private uint ARM_BL(uint src, uint dst) { return (0xEB000000 | (((dst - src - 8) >> 2) & 0x00FFFFFF)); }

        private ushort CalcCRC16(uint offset, uint len)
        {
            uint crc = 0xFFFF;
            uint[] val = {0xC0C1, 0xC181, 0xC301, 0xC601, 0xCC01, 0xD801, 0xF001, 0xA001};

            m_FileStream.Position = offset;
            for (uint i = 0; i < len; i++)
            {
                crc ^= m_BinReader.ReadByte();

                for (int j = 0; j < 8; j++)
                {
                    bool carry = ((crc & 0x1) == 0x1);

                    crc >>= 1;
                    if (carry)
                        crc ^= (val[j] << (7 - j));
                }
            }

            return (ushort)crc;
        }

        public bool NeedsPatch()
        {
            m_FileStream.Position = 0x1FC;
	        UInt32 patchversion = m_BinReader.ReadUInt32();

	        if (patchversion > ROM_PATCH_VERSION)
		        throw new Exception("This ROM has been patched by a more recent version of "+Program.AppTitle+" and cannot be opened in this version.");

	        return (patchversion < ROM_PATCH_VERSION);
        }

        private void Patch_v1(BackgroundWorker lazyman)
        {
            lazyman.ReportProgress(100);

            // read the ARM9 binary, decompress it and write it back
            m_FileStream.Position = 0x2C;
            int oldsize = (int)(m_BinReader.ReadUInt32() - 0x4000);

            m_FileStream.Position = 0x8000;
            byte[] binary = m_BinReader.ReadBytes(oldsize);
            Jap77.Decompress(ref binary);

            lazyman.ReportProgress(150);

            int newsize = (binary.Length + 3) & ~3;
            int delta = newsize - oldsize;

            MakeRoom((uint)(0x8000 + oldsize), (uint)delta);

            m_FileStream.Position = 0x8000;
            m_BinWriter.Write(binary);

            lazyman.ReportProgress(199);

            // patch the loader code so that it doesn't try to decompress
            // the already decompressed binary
            UInt32 nop = 0xE1A00000;
            m_FileStream.Position = 0x484C;
            m_BinWriter.Write(nop);
            m_BinWriter.Write(nop);
            m_BinWriter.Write(nop);

            int fixstart = 0x8000 + oldsize;
            AutoFix(0xFFFF, (uint)fixstart, delta);

            // fix the ARM9 binary size - important
            uint newarm9size = (uint)(0x4000 + newsize);
            m_FileStream.Position = 0x2C;
            m_BinWriter.Write(newarm9size);
        }

        private void Patch_v2(BackgroundWorker lazyman)
        {
            lazyman.ReportProgress(200);

            uint levelptr_table = 0, objbank_table = 0;
            uint mempatch1 = 0, mempatch2 = 0;
            byte[] loadercode = null;
            uint lvl_ovlid_table = 0;
            uint unload_patch = 0, unload_branchop = 0;
	        uint[] ovltable_addr_patch = new uint[4];
	        uint[] ovltable_size_patch = new uint[4];
            uint lvlload_addr_patch = 0, lvlload_code_patch = 0;
            uint objbank_addr_patch = 0, objbank_code_patch = 0;

	        switch (m_Version)
	        {
	        case Version.EUR:
		        levelptr_table = 0x92208;
		        objbank_table = 0x75998;
		        mempatch1 = 0x18B60;
		        mempatch2 = 0x58DE0;
                loadercode = Properties.Resources.level_ovl_init_EUR;
		        lvl_ovlid_table = 0x758C8;
		        unload_patch = 0x2DE80;
		        unload_branchop = ARM_BL(0x0202DE80, 0x0214EAD8);
		        ovltable_addr_patch[0] = 0x17E90; ovltable_addr_patch[1] = 0x17F30;
		        ovltable_addr_patch[2] = 0x17FCC; ovltable_addr_patch[3] = 0x180FC;
		        ovltable_size_patch[0] = 0x17E80; ovltable_size_patch[1] = 0x17ED0;
		        ovltable_size_patch[2] = 0x17F70; ovltable_size_patch[3] = 0x180C4;
		        lvlload_addr_patch = 0x2D62C;
		        lvlload_code_patch = 0x2D288;
		        objbank_addr_patch = 0x2E074;
		        objbank_code_patch = 0x2DFAC;
		        break;

            case Version.JAP:
                levelptr_table = 0x902B8;
                objbank_table = 0x73C08;
                mempatch1 = 0x57368;
                mempatch2 = 0xFFFFFFFF;
                loadercode = Properties.Resources.level_ovl_init_JAP;
                lvl_ovlid_table = 0x73B38;
                unload_patch = 0x2CDD8;
                unload_branchop = ARM_BL(0x0202CDD8, 0x021434B8);
                ovltable_addr_patch[0] = 0x17DE4; ovltable_addr_patch[1] = 0x17E84;
                ovltable_addr_patch[2] = 0x17F20; ovltable_addr_patch[3] = 0x18050;
                ovltable_size_patch[0] = 0x17DD4; ovltable_size_patch[1] = 0x17E24;
                ovltable_size_patch[2] = 0x17EC4; ovltable_size_patch[3] = 0x18018;
                lvlload_addr_patch = 0x2C7D4;
                lvlload_code_patch = 0x2C430;
                objbank_addr_patch = 0x2CFCC;
                objbank_code_patch = 0x2CF04;
                break;

	        case Version.USA_v1:
		        levelptr_table = 0x8FDB0;
		        objbank_table = 0x73664;
		        mempatch1 = 0x56EB8;
		        mempatch2 = 0xFFFFFFFF;
                loadercode = Properties.Resources.level_ovl_init_USAv1;
		        lvl_ovlid_table = 0x73594;
		        unload_patch = 0x2CB00;
		        unload_branchop = ARM_BL(0x0202CB00, 0x02143BD8);
		        ovltable_addr_patch[0] = 0x17D70; ovltable_addr_patch[1] = 0x17E10;
		        ovltable_addr_patch[2] = 0x17EAC; ovltable_addr_patch[3] = 0x17FDC;
		        ovltable_size_patch[0] = 0x17D60; ovltable_size_patch[1] = 0x17DB0;
		        ovltable_size_patch[2] = 0x17E50; ovltable_size_patch[3] = 0x17FA4;
		        lvlload_addr_patch = 0x2C4FC;
		        lvlload_code_patch = 0x2C15C;
		        objbank_addr_patch = 0x2CCF4;
		        objbank_code_patch = 0x2CC2C;
		        break;

	        case Version.USA_v2:
		        levelptr_table = 0x90ACC;
		        objbank_table = 0x74384;
		        mempatch1 = 0x18A44;
		        mempatch2 = 0x57B30;
                loadercode = Properties.Resources.level_ovl_init_USAv2;
		        lvl_ovlid_table = 0x742B4;
		        unload_patch = 0x2CE14;
		        unload_branchop = ARM_BL(0x0202CE14, 0x02145758);
		        ovltable_addr_patch[0] = 0x17DE4; ovltable_addr_patch[1] = 0x17E84;
		        ovltable_addr_patch[2] = 0x17F20; ovltable_addr_patch[3] = 0x18050;
		        ovltable_size_patch[0] = 0x17DD4; ovltable_size_patch[1] = 0x17E24;
		        ovltable_size_patch[2] = 0x17EC4; ovltable_size_patch[3] = 0x18018;
		        lvlload_addr_patch = 0x2C810;
		        lvlload_code_patch = 0x2C46C;
		        objbank_addr_patch = 0x2D008;
		        objbank_code_patch = 0x2CF40;
		        break;
	        }

	        // tweak the root heap start address to gain more overlay space :)
	        m_FileStream.Position = mempatch1;
	        uint lvl_start = m_BinReader.ReadUInt32();

	        uint lvl_end = lvl_start + LEVEL_OVERLAY_SIZE;
	        m_FileStream.Position = mempatch1;
	        m_BinWriter.Write(lvl_end);
	        if (mempatch2 != 0xFFFFFFFF)
	        {
		        m_FileStream.Position = mempatch2;
		        m_BinWriter.Write(lvl_end);
	        }

	        // patch level overlay unloading
	        m_FileStream.Position = unload_patch;
	        m_BinWriter.Write(unload_branchop);

	        // patch more stuff
	        foreach (uint addr in ovltable_addr_patch)
	        {
		        m_FileStream.Position = addr;
		        m_BinWriter.Write((uint)(0x02000000 + levelptr_table));
	        }
	        foreach (uint addr in ovltable_size_patch)
	        {
		        m_FileStream.Position = addr;
		        uint op = m_BinReader.ReadUInt32();
		        op = (op & 0xFFFFFF00) | 0x0D;
		        m_FileStream.Position = addr;
		        m_BinWriter.Write(op);
	        }

            lazyman.ReportProgress(201);

	        // for each level, create a new overlay with the loader code and level data
	        uint dataoffset = (uint)((loadercode.Length + 3) & ~3);

	        for (int i = 0; i < 52; i++)
	        {
		        uint overlayid = AddOverlay(lvl_start);

		        m_FileStream.Position = lvl_ovlid_table + (i*4);
		        uint old_overlayid = m_BinReader.ReadUInt32();

		        m_FileStream.Position = lvl_ovlid_table + (i*4);
		        m_BinWriter.Write(overlayid);

		        NitroOverlay ovl = new NitroOverlay(this, overlayid);
		        ovl.SetInitializer(lvl_start, 4);
		        ovl.WriteBlock(0, loadercode);

		        // write the object bank settings
		        m_FileStream.Position = objbank_table + (i * 7);
		        byte[] objbanks = m_BinReader.ReadBytes(7);
		        ovl.WriteBlock(dataoffset, objbanks);
        		
		        ovl.Write8(dataoffset + 0x7, 0x00);
		        ovl.Write32(dataoffset + 0x8, (uint)(i + 1));

		        // copy level data
		        {
			        NitroOverlay oldovl = new NitroOverlay(this, old_overlayid);
			        uint oldbase = oldovl.GetRAMAddr();

			        m_FileStream.Position = levelptr_table + (i*4);
			        uint header_offset = m_BinReader.ReadUInt32() - oldbase;

			        uint curoffset = dataoffset + 0x40;

			        // CLPS (collision behaviors) chunk
			        {
				        uint clps_addr = oldovl.ReadPointer(header_offset);
				        ushort clps_num = oldovl.Read16(clps_addr + 0x06);
				        uint clps_size = (uint)(8 + (clps_num * 8));
				        byte[] clps = oldovl.ReadBlock(clps_addr, clps_size);
				        ovl.WriteBlock(curoffset, clps);

				        ovl.WritePointer(dataoffset + 0x0C, curoffset);
				        curoffset += clps_size;
			        }

			        // misc objects table list
			        {
				        uint tables_addr = oldovl.ReadPointer(header_offset + 0x4);
				        ushort tables_num = oldovl.Read16(tables_addr);
				        tables_addr = oldovl.ReadPointer(tables_addr + 0x4);

				        ovl.WritePointer(dataoffset + 0x10, curoffset);
				        ovl.Write16(curoffset, tables_num);
				        ovl.WritePointer(curoffset + 0x04, curoffset + 0x8);
				        curoffset += 0x8;

				        uint objdata_offset = (uint)(curoffset + (tables_num * 8));
				        for (ushort t = 0; t < tables_num; t++)
				        {
					        uint tbl_part1 = oldovl.Read32(tables_addr);
					        uint tbl_addr = oldovl.ReadPointer(tables_addr + 0x4);

					        ovl.Write32(curoffset, tbl_part1);
					        ovl.WritePointer(curoffset + 0x4, objdata_offset);

					        tables_addr += 8;
					        curoffset += 8;

					        byte tbltype = (byte)(tbl_part1 & 0x1F);
					        byte numentries = (byte)((tbl_part1 >> 8) & 0xFF);

					        if ((tbltype == 13) || (tbltype > 14))
						        throw new Exception(String.Format("Wrong object table type {0} (0x{0:X2}) in level data", tbltype));

					        int[] datasizes = {16, 16, 6, 6, 14, 8, 8, 8, 8, 12, 14, 2, 2, 0, 4};
					        int datasize = numentries * datasizes[tbltype];

					        byte[] data = oldovl.ReadBlock(tbl_addr, (uint)datasize);
					        ovl.WriteBlock(objdata_offset, data);

					        objdata_offset += (uint)((datasize + 3) & ~3);
				        }

				        curoffset = objdata_offset;
			        }

			        // main object table lists
                    // and that stuff about texture scrolling
			        {
				        uint tlists_addr = oldovl.ReadPointer(header_offset + 0x10);
				        byte tlists_num = oldovl.Read8(header_offset + 0x14);

				        uint tlists_new_addr = curoffset;
				        curoffset += (uint)(tlists_num * 12);

				        ovl.WritePointer(dataoffset + 0x1C, tlists_new_addr);
				        ovl.Write8(dataoffset + 0x20, tlists_num);

				        for (byte tl = 0; tl < tlists_num; tl++)
				        {
					        uint tables_addr = oldovl.ReadPointer(tlists_addr);
                            uint texanm_addr = oldovl.ReadPointer(tlists_addr + 0x4);

                            if (tables_addr != 0xFFFFFFFF)
                            {
                                ovl.WritePointer(tlists_new_addr, curoffset);

                                ushort tables_num = oldovl.Read16(tables_addr);
                                tables_addr = oldovl.ReadPointer(tables_addr + 0x4);

                                ovl.Write16(curoffset, tables_num);
                                ovl.WritePointer(curoffset + 0x04, curoffset + 0x8);
                                curoffset += 0x8;

                                uint objdata_offset = (uint)(curoffset + (tables_num * 8));
                                for (ushort t = 0; t < tables_num; t++)
                                {
                                    uint tbl_part1 = oldovl.Read32(tables_addr);
                                    uint tbl_addr = oldovl.ReadPointer(tables_addr + 0x4);

                                    ovl.Write32(curoffset, tbl_part1);
                                    ovl.WritePointer(curoffset + 0x4, objdata_offset);

                                    tables_addr += 8;
                                    curoffset += 8;

                                    byte tbltype = (byte)(tbl_part1 & 0x1F);
                                    byte numentries = (byte)((tbl_part1 >> 8) & 0xFF);

                                    if ((tbltype == 13) || (tbltype > 14))
                                        throw new Exception(String.Format("Wrong object table type {0} (0x{0:X2}) in level data", tbltype));

                                    int[] datasizes = { 16, 16, 6, 6, 14, 8, 8, 8, 8, 12, 14, 2, 2, 0, 4 };
                                    int datasize = numentries * datasizes[tbltype];

                                    byte[] data = oldovl.ReadBlock(tbl_addr, (uint)datasize);
                                    ovl.WriteBlock(objdata_offset, data);

                                    objdata_offset += (uint)((datasize + 3) & ~3);
                                }

                                curoffset = objdata_offset;
                            }
                            else
                                ovl.Write32(tlists_new_addr, 0);

                            if (texanm_addr != 0xFFFFFFFF)//If not null
                            {
                                ovl.WritePointer(tlists_new_addr + 0x4, curoffset);

                                uint texanm_new_addr = curoffset;
                                curoffset += 0x18;

                                uint textures_addr = oldovl.ReadPointer(texanm_addr + 0x14);
                                uint numscale = 0, numrot = 0, numtrans = 0;
                                uint numtextures = oldovl.Read32(texanm_addr + 0x10);

                                ovl.Write32(texanm_new_addr, oldovl.Read32(texanm_addr));
                                ovl.Write32(texanm_new_addr + 0x10, numtextures);

                                uint textures_new_addr = curoffset;
                                curoffset += (numtextures * 0x1C);

                                ovl.WritePointer(texanm_new_addr + 0x14, textures_new_addr);
                                for (uint t = 0; t < numtextures; t++)
                                {
                                    uint tex_old_addr = textures_addr + (t * 0x1C);
                                    uint tex_new_addr = textures_new_addr + (t * 0x1C);

                                    ushort tex_scalenum = oldovl.Read16(tex_old_addr + 0x0C);
                                    ushort tex_scalestart = oldovl.Read16(tex_old_addr + 0x0E);
                                    ushort tex_rotnum = oldovl.Read16(tex_old_addr + 0x10);
                                    ushort tex_rotstart = oldovl.Read16(tex_old_addr + 0x12);
                                    ushort tex_transnum = oldovl.Read16(tex_old_addr + 0x14);
                                    ushort tex_transstart = oldovl.Read16(tex_old_addr + 0x16);

                                    if ((tex_scalestart + tex_scalenum) > numscale)
                                        numscale = (uint)(tex_scalestart + tex_scalenum);
                                    if ((tex_rotstart + tex_rotnum) > numrot)
                                        numrot = (uint)(tex_rotstart + tex_rotnum);
                                    if ((tex_transstart + tex_transnum) > numtrans)
                                        numtrans = (uint)(tex_transstart + tex_transnum);

                                    ovl.Write32(tex_new_addr, oldovl.Read32(tex_old_addr));
                                    ovl.WritePointer(tex_new_addr + 0x4, curoffset);
                                    ovl.Write32(tex_new_addr + 0x8, oldovl.Read32(tex_old_addr + 0x8));
                                    ovl.Write16(tex_new_addr + 0xC, tex_scalenum);
                                    ovl.Write16(tex_new_addr + 0xE, tex_scalestart);
                                    ovl.Write16(tex_new_addr + 0x10, tex_rotnum);
                                    ovl.Write16(tex_new_addr + 0x12, tex_rotstart);
                                    ovl.Write16(tex_new_addr + 0x14, tex_transnum);
                                    ovl.Write16(tex_new_addr + 0x16, tex_transstart);
                                    ovl.Write32(tex_new_addr + 0x18, oldovl.Read32(tex_old_addr + 0x18));

                                    string tex_matname = oldovl.ReadString(oldovl.ReadPointer(tex_old_addr + 0x4), 0);
                                    ovl.WriteString(curoffset, tex_matname, 0);
                                    curoffset += (uint)((tex_matname.Length + 3) & ~3);
                                }

                                uint scale_addr = oldovl.ReadPointer(texanm_addr + 0x4);
                                ovl.WritePointer(texanm_new_addr + 0x4, curoffset);
                                for (uint v = 0; v < numscale; v++)
                                {
                                    ovl.Write32(curoffset, oldovl.Read32(scale_addr + (v * 4)));
                                    curoffset += 4;
                                }

                                uint rot_addr = oldovl.ReadPointer(texanm_addr + 0x8);
                                ovl.WritePointer(texanm_new_addr + 0x8, curoffset);
                                for (uint v = 0; v < numrot; v++)
                                {
                                    ovl.Write16(curoffset, oldovl.Read16(rot_addr + (v * 2)));
                                    curoffset += 2;
                                }
                                curoffset = (uint)((curoffset + 3) & ~3);

                                uint trans_addr = oldovl.ReadPointer(texanm_addr + 0xC);
                                ovl.WritePointer(texanm_new_addr + 0xC, curoffset);
                                for (uint v = 0; v < numtrans; v++)
                                {
                                    ovl.Write32(curoffset, oldovl.Read32(trans_addr + (v * 4)));
                                    curoffset += 4;
                                }
                            }
                            else
                                ovl.Write32(tlists_new_addr + 0x4, 0);
					        
                            ovl.Write32(tlists_new_addr + 0x8, oldovl.Read32(tlists_addr + 0x8));
                            tlists_new_addr += 12;
                            tlists_addr += 12;
				        }
			        }

			        // misc header pieces
			        ovl.Write32(dataoffset + 0x14, oldovl.Read32(header_offset + 0x8));
			        ovl.Write32(dataoffset + 0x18, oldovl.Read32(header_offset + 0xC));
			        ovl.Write16(dataoffset + 0x22, oldovl.Read16(header_offset + 0x16));
			        ovl.Write8(dataoffset + 0x24, oldovl.Read8(header_offset + 0x18));
		        }

		        ovl.SaveChanges();
                lazyman.ReportProgress(201 + (int)((98f / 54f) * i));
	        }

	        // fix the level loader as to load from the new overlays
	        m_FileStream.Position = lvlload_addr_patch;
	        m_BinWriter.Write((uint)(lvl_start + dataoffset + 0xC));

	        m_FileStream.Position = lvlload_code_patch;
	        m_BinWriter.Write((uint)0xE1A00000);

	        // refill the old level header address table
	        // to adapt it to its new usages
	        m_FileStream.Position = levelptr_table;
	        for (uint i = 0; i < 13; i++)
	        {
		        m_BinWriter.Write((uint)0xFFFFFFFF);
		        m_BinWriter.Write((uint)0x00000000);
		        m_BinWriter.Write((uint)0x00000000);
	        }
	        m_BinWriter.Write((uint)0x00000000);
	        m_BinWriter.Write((uint)0x00000000);

	        // fix the object banks thingy
	        m_FileStream.Position = objbank_addr_patch;
	        m_BinWriter.Write((uint)(levelptr_table + 0x0200009C));

	        m_FileStream.Position = objbank_code_patch;
	        m_BinWriter.Write((uint)0xE1A07001);

	        // phew! what a goddamn long thing
            lazyman.ReportProgress(299);
        }

        private void Patch_v3(BackgroundWorker lazyman)
        {
            // this should keep the r4 firmware from trying to decompress 
            // the ARM9 binary we already decompressed
            m_FileStream.Position = 0x4AF4;
            m_BinWriter.Write((uint)0);
            m_BinWriter.Write((uint)0);

            // we never know
            m_FileStream.Position = 0x2C;
            uint binend = m_BinReader.ReadUInt32() + 0x4000;
            m_FileStream.Position = binend;
            m_BinWriter.Write((uint)0);

            lazyman.ReportProgress(399);
        }

        public void Patch()
        {
            // read previous patch version
            m_FileStream.Position = 0x1FC;
            uint oldversion = m_BinReader.ReadUInt32();

            // write patch version to the end of the ROM header
            // this place should normally be zero and unused :)
            m_FileStream.Position = 0x1FC;
            m_BinWriter.Write(ROM_PATCH_VERSION);

            BackgroundWorker lazyman = new BackgroundWorker();
            ProgressDialog progdlg = new ProgressDialog("Patching ROM", (int)ROM_PATCH_VERSION, oldversion);

            lazyman.WorkerReportsProgress = true;
            lazyman.WorkerSupportsCancellation = false;
            lazyman.DoWork += new DoWorkEventHandler(this.OnPatchRun);
            lazyman.ProgressChanged += new ProgressChangedEventHandler(progdlg.RW_ProgressChanged);
            lazyman.RunWorkerCompleted += new RunWorkerCompletedEventHandler(progdlg.RW_Completed);
            lazyman.RunWorkerAsync(progdlg);
            progdlg.ShowDialog();
            
            if (progdlg.Error != null)
                throw progdlg.Error;

            // [s]fix the secure area CRC16[/s]
            // what would care about this?
            // also, it may be the CRC16 of the encrypted secure area...
            /*m_FileStream.Position = 0x20;
            uint sastart = m_BinReader.ReadUInt32();
            ushort sacrc = CalcCRC16(sastart, 0x8000 - sastart);
            m_FileStream.Position = 0x6C;
            m_BinWriter.Write(sacrc);*/

            // fix the header CRC16... we never know :P
            // as an example NO$GBA won't load the ROM if this CRC16 is wrong
            ushort hcrc = CalcCRC16(0, 0x15E);
            m_FileStream.Position = 0x15E;
            m_BinWriter.Write(hcrc);
        }

        public void OnPatchRun(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker lazyman = (BackgroundWorker)sender;
            ProgressDialog progdlg = (ProgressDialog)e.Argument;
            uint oldversion = (uint)progdlg.UserData;

            if (oldversion < 1) Patch_v1(lazyman); // patch v1: decompress the ARM9 binary to make stuff accessible easily
            if (oldversion < 2) Patch_v2(lazyman); // patch v2: relocate level data to make it expandable
            if (oldversion < 3) Patch_v3(lazyman); // patch v3: fix for R4/acekard flashcarts
        }
    }
}
