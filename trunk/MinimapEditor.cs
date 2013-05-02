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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SM64DSe
{
    public partial class MinimapEditor : Form
    {
        private int m_NumAreas;
        private int m_CurArea;

        LevelEditorForm _owner;

        public MinimapEditor()
        {
            InitializeComponent();
        }

        NitroFile palfile;
        NitroFile tsetfile;
        NitroFile[] tmapfile;
        int mapsize;
        private void RedrawMinimap(LevelEditorForm _owner)
        {
            palfile = Program.m_ROM.GetFileFromInternalID(_owner.m_LevelSettings.MinimapPalFileID);
            tsetfile = Program.m_ROM.GetFileFromInternalID(_owner.m_LevelSettings.MinimapTsetFileID);
            for (int i = 0; i < m_NumAreas; i++)
            {
                try
                {
                    tmapfile[i] = (Program.m_ROM.GetFileFromInternalID(_owner.m_MinimapFileIDs[i]));
                    tmapfile[i].ForceDecompression();
                }
                catch//If the file doesn't exist
                {
                    continue;
                }
            }

            tsetfile.ForceDecompression();

            try
            {
                uint test = tmapfile[m_CurArea].Read16(0);
                btnImport.Enabled = true;
            }
            catch
            {
                btnImport.Enabled = false;
                return;
            }

            mapsize = 0;
            switch (tmapfile[m_CurArea].m_Data.Length)
            {
                case 2 * 16 * 16: { mapsize = 128; cbSizes.SelectedIndex = 0; } break;
                case 2 * 32 * 32: { mapsize = 256; cbSizes.SelectedIndex = 1; } break;
                case 2 * 64 * 64: mapsize = 512; break;       // maps should never get that big
                case 2 * 128 * 128: mapsize = 1024; break;    // but we never know :P
                default: {
                    mapsize = (int)(Math.Sqrt(tmapfile[m_CurArea].m_Data.Length / 2) * 8);
                    break;
                         }
            }

            lblMapsize.Text = mapsize + " x " + mapsize;

            Bitmap bmp = new Bitmap(mapsize, mapsize);

            uint tileoffset = 0;
            for (int my = 0; my < mapsize; my += 8)
            {
                for (int mx = 0; mx < mapsize; mx += 8)
                {
                    ushort tilecrap = tmapfile[m_CurArea].Read16(tileoffset);
                    uint tilenum = (uint)(tilecrap & 0x03FF);
                    //Console.WriteLine("" + tilecrap);

                    for (int ty = 0; ty < 8; ty++)
                    {
                        for (int tx = 0; tx < 8; tx++)
                        {
                            uint totaloffset = (uint)(tilenum * 64 + ty * 8 + tx);//Address of current pixel
                            byte palentry = tsetfile.Read8(totaloffset);//Offset of current pixel's entry in palette file
                            //Palentry is double to get the position of the colour in the palette file
                            ushort pixel = palfile.Read16((uint)(palentry * 2));//Colour of current pixel from palette file
                            bmp.SetPixel(mx + tx, my + ty, Helper.BGR15ToColor(pixel));
                        }
                    }

                    tileoffset += 2;
                }
            }

            pbxMinimapGfx.Image = new Bitmap(bmp, new Size(mapsize * 2, mapsize * 2));
            pbxMinimapGfx.Refresh();

            gridPalette.RowCount = 16;
            gridPalette.ColumnCount = 16;
            
            //Read palette colours
            Color[] paletteColours = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                //Colour in BGR15 format (16 bits) written to every even address 0,2,4...
                ushort palColour = palfile.Read16((uint)(i * 2));
                paletteColours[i] = Helper.BGR15ToColor(palColour);
            }
            //Display palette colours
            int clr = 0;
            for (int row = 0; row < gridPalette.RowCount; row++)//For every row
            {
                gridPalette.Columns[row].Width = 16;//Set each cell's width to 16
                for (int column = 0; column < gridPalette.ColumnCount; column++)//For every column
                {
                    gridPalette.Rows[row].Cells[column].Style.BackColor = paletteColours[clr];//Set the cell colour to the corresponding palette colour
                    clr += 1;
                }
            }
            gridPalette.CurrentCell.Selected = false;//Select none by default
        }

        public void importBMP(string filename)
        {
            Bitmap bmp = new Bitmap(filename);

            Color[] palette = bmp.Palette.Entries.ToArray<Color>();
            if (palette.Length > 256)
            {
                MessageBox.Show("Too many colours\n\n" + 
                    "You must import an indexed bitmap with 256 colours or fewer " + 
                    "\nwith a resolution matching the mapsize");
                return;
            }
            if (bmp.Width > mapsize || bmp.Height > mapsize)
            {
                MessageBox.Show("Wrong size\n\n" +
                    "You must import an indexed bitmap with 256 colours or fewer " +
                    "\nwith a resolution matching the mapsize");
                return;
            }

            //Write new palette
            palfile.Clear();
            for (int i = 0; i < palette.Length; i++)
            {
                //Colour in BGR15 format (16 bits) written to every even address 0,2,4...
                palfile.Write16((uint)i * 2, (ushort)(Helper.ColorToBGR15(palette[i])));
            }

            //Get last used tile number (highest) in previous areas so we know where write new data to
            ushort prevLastTile = 0;
            if (m_NumAreas > 1)
            {
                for (int i = 0; i < m_NumAreas; i++)
                {
                    try
                    {
                        uint tile = 0;
                        for (int my = 0; my < mapsize; my += 8)
                        {
                            for (int mx = 0; mx < mapsize; mx += 8)
                            {
                                ushort tilecrap = tmapfile[i].Read16(tile);
                                uint tilenum = (uint)(tilecrap & 0x03FF);
                                if ((ushort)tilenum > prevLastTile)
                                    prevLastTile = (ushort)tilenum;

                                tile += 2;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            //Fill current tmapfile to use full mapsize x mapsize
            tmapfile[m_CurArea].Clear();
            uint addr = 0;
            int curTile = 0;
            if (m_NumAreas == 1 || prevLastTile == 0)//If this is going to be the first tile, keep it at zero
                curTile = prevLastTile;
            else//else tiles should start one after the last (highest) one used by previous map
                curTile = prevLastTile + 1;
            int row = (int)(mapsize / 8);
            int count = 0;
            for (int my = 0; my < mapsize; my += 8)
            {
                for (int mx = 0; mx < mapsize; mx += 8)
                {
                    if (row == 16)//128x128 0, 1, 2...14, 15, 32, 33...47, 64...
                    {
                        if (count == row)
                        {
                            curTile += row;
                            count = 0;
                        }
                        //Console.WriteLine("" + curTile);
                        tmapfile[m_CurArea].Write16(addr, (ushort)curTile);
                        curTile++;
                        count++;
                        addr += 2;
                    }
                    else//256x256 0, 1, 2, 3...
                    {
                        tmapfile[m_CurArea].Write16(addr, (ushort)curTile);
                        curTile++;
                        addr += 2;
                    }
                }
            }

            //Check to see if there's already an identical tile and if so, change the current value to that
            //Works, but not if you want to keep existing data eg. multiple maps

            //List<List<byte>> tiles = new List<List<byte>>();
            //List<byte> curTilePal = new List<byte>();
            //uint tileoffset = 0;
            //for (int my = 0; my < mapsize; my += 8)
            //{
            //    for (int mx = 0; mx < mapsize; mx += 8)
            //    {
            //        ushort tilecrap = tmapfile[m_CurArea].Read16(tileoffset);
            //        uint tilenum = (uint)(tilecrap & 0x03FF);

            //        curTilePal = new List<byte>();
            //        for (int ty = 0; ty < 8; ty++)
            //        {
            //            for (int tx = 0; tx < 8; tx++)
            //            {
            //                uint totaloffset = (uint)(tilenum * 64 + ty * 8 + tx);//Position of current pixel's entry
            //                curTilePal.Add((byte)(Array.IndexOf(palette, bmp.GetPixel(mx + tx, my + ty))));
            //            }
            //        }

            //        tiles.Add(curTilePal);

            //        if (posInList(tiles, curTilePal) != -1)
            //        {
            //            tmapfile[m_CurArea].Write16(tileoffset, (ushort)(posInList(tiles, curTilePal)));
            //        }

            //        tileoffset += 2;
            //    }
            //}

            //Write the new image to file
            uint tileoffset = 0;
            for (int my = 0; my < mapsize; my += 8)
            {
                for (int mx = 0; mx < mapsize; mx += 8)
                {
                    ushort tilecrap = tmapfile[m_CurArea].Read16(tileoffset);
                    uint tilenum = (uint)(tilecrap & 0x03FF);

                    for (int ty = 0; ty < 8; ty++)
                    {
                        for (int tx = 0; tx < 8; tx++)
                        {
                            uint totaloffset = (uint)(tilenum * 64 + ty * 8 + tx);//Position of current pixel's entry
                            byte palentry = (byte)(Array.IndexOf(palette, bmp.GetPixel(mx + tx, my + ty)));
                            tsetfile.Write8(totaloffset, (byte)(palentry));
                        }
                    }

                    tileoffset += 2;
                }
            }

            tsetfile.ForceCompression();
            for (int i = 0; i < tmapfile.Length; i++)
            {
                try
                {
                    tmapfile[i].ForceCompression();
                    tmapfile[i].SaveChanges();
                }
                catch
                {
                    continue;
                }
            }

            palfile.SaveChanges();
            tsetfile.SaveChanges();

            RedrawMinimap((LevelEditorForm)Owner);
        }

        public void switchBackground(int swapped)
        {
            //The background colour is the first colour stored in the palette
            ushort first = palfile.Read16((uint)0);//Read the first colour in the palette file
            ushort swappedColour = palfile.Read16((uint)(swapped * 2));//Read the colour to be swapped
            //Colour in BGR15 format (16 bits) written to every even address 0,2,4...
            palfile.Write16((uint)0, swappedColour);//Write new background colour to first entry
            palfile.Write16((uint)(swapped * 2), first);//Write the previously first colour to the colour being swapped

            palfile.SaveChanges();

            //Swap all palette file entries for the swapped colours in the graphic file
            uint tileoffset = 0;
            for (int my = 0; my < mapsize; my += 8)
            {
                for (int mx = 0; mx < mapsize; mx += 8)
                {
                    ushort tilecrap = tmapfile[m_CurArea].Read16(tileoffset);
                    uint tilenum = (uint)(tilecrap & 0x03FF);

                    for (int ty = 0; ty < 8; ty++)
                    {
                        for (int tx = 0; tx < 8; tx++)
                        {
                            uint totaloffset = (uint)(tilenum * 64 + ty * 8 + tx);//Position of current pixel's entry
                            byte palentry = tsetfile.Read8(totaloffset);
                            if (palentry == 0)//If the current pixel points to first colour in palette, 
                                tsetfile.Write8(totaloffset, (byte)(swapped));//point it to the swapped colour
                            if (palentry == (byte)swapped)//If the current pixel points to the swapped colour in palette, 
                                tsetfile.Write8(totaloffset, (byte)0);//point it to the first colour
                        }
                    }

                    tileoffset += 2;
                }
            }

            tsetfile.ForceCompression();
            tsetfile.SaveChanges();

            RedrawMinimap((LevelEditorForm)Owner);
        }

        private void btnAreaXX_Click(object sender, EventArgs e)
        {
            ToolStripButton myself = (ToolStripButton)sender;
            if (!myself.Checked)
            {
                int pos = tsMinimapEditor.Items.IndexOf(tslBeforeAreaBtns) + 1;
                for (int i = 0; i < m_NumAreas; i++, pos++)
                    ((ToolStripButton)tsMinimapEditor.Items[pos]).Checked = false;

                myself.Checked = true;
                m_CurArea = (int)myself.Tag;
                RedrawMinimap((LevelEditorForm)Owner);
            }
        }

        private void MinimapEditor_Load(object sender, EventArgs e)
        {
            LevelEditorForm _owner = (LevelEditorForm)Owner;
            this._owner = _owner;

            m_NumAreas = _owner.m_NumAreas;
            m_CurArea = 0;

            tmapfile = new NitroFile[m_NumAreas];

            txtCoordScale.Text = "" + (_owner.m_Overlay.Read16((uint)0x76) / 1000f);

            cbSizes.Items.Add("512: 128x128"); cbSizes.Items.Add("2048: 256x256");

            int i, pos = tsMinimapEditor.Items.IndexOf(tslBeforeAreaBtns) + 1;
            for (i = 0; i < m_NumAreas; i++, pos++)
            {
                ToolStripButton btn = new ToolStripButton(i.ToString(), null, new EventHandler(btnAreaXX_Click));
                btn.Tag = i;
                tsMinimapEditor.Items.Insert(pos, btn);
            }

            ((ToolStripButton)tsMinimapEditor.Items[pos - i]).Checked = true;

            RedrawMinimap(_owner);
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Bitmap (.bmp)|*.bmp";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                importBMP(ofd.FileName);
            }
        }

        private void btnSetBackground_Click(object sender, EventArgs e)
        {
            if (gridPalette.CurrentCell == null)
                MessageBox.Show("Please select a colour first.");
            else
            {
                int palIndex = (gridPalette.RowCount * gridPalette.CurrentCell.RowIndex) + gridPalette.CurrentCell.ColumnIndex;//Get the index of the selected colour in the palette file
                switchBackground(palIndex);
            }
        }

        public void gridPalette_CurrentCellChanged(object sender, System.EventArgs e)
        {
            
        }

        private void txtCoordScale_TextChanged(object sender, EventArgs e)
        {
            if (txtCoordScale.Text != "")
            {
                try
                {
                    _owner.m_Overlay.Write16((uint)0x76, (ushort)(Convert.ToSingle(txtCoordScale.Text) * 1000));
                }
                catch
                {
                    MessageBox.Show("Please enter a valid float value in format 1.23");
                }
            }
        }

        public int posInList(List<List<byte>> bigList, List<byte> indices)
        {
            if (bigList.Count == 0)
                return -1;
            for (int i = 0; i < bigList.Count; i++)
            {
                List<byte> compare = bigList[i];
                if (compare.Count != indices.Count)
                    continue;
                else
                {
                    int wrongFlag = 0;
                    for (int j = 0; j < compare.Count; j++)
                    {
                        if (compare[j] != indices[j])
                            wrongFlag += 1;
                    }
                    if (wrongFlag == 0)//No differences
                        return i;//They're the same, return position
                }
            }
            return -1;//Not found
        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            if (cbSizes.SelectedIndex == 0)
                resizeMaps(128);
            else if (cbSizes.SelectedIndex == 1)
                resizeMaps(256);
        }

        private void resizeMaps(int newMapSize)
        {
            //Fill current tmapfile to use full mapsize x mapsize
            uint addr = 0;
            int curTile = 0;
            int row = (int)(newMapSize / 8);
            int count = 0;
            for (int i = 0; i < m_NumAreas; i++)
            {
                curTile = 0;
                addr = 0;
                count = 0;
                try
                {
                    tmapfile[i].Clear();
                    for (int my = 0; my < newMapSize; my += 8)
                    {
                        for (int mx = 0; mx < newMapSize; mx += 8)
                        {
                            if (row == 16)//128x128 0, 1, 2...14, 15, 32, 33...47, 64...
                            {
                                if (count == row)
                                {
                                    curTile += row;
                                    count = 0;
                                }
                                tmapfile[i].Write16(addr, (ushort)curTile);
                                //tmapfile[i].Write16(addr, (ushort)0);
                                curTile++;
                                count++;
                                addr += 2;
                            }
                            else//256x256 0, 1, 2, 3...
                            {
                                tmapfile[i].Write16(addr, (ushort)curTile);
                                //tmapfile[i].Write16(addr, (ushort)0);
                                curTile++;
                                addr += 2;
                            }
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            for (int i = 0; i < tmapfile.Length; i++)
            {
                try
                {
                    tmapfile[i].ForceCompression();
                    tmapfile[i].SaveChanges();
                }
                catch
                {
                    continue;
                }
            }

            RedrawMinimap((LevelEditorForm)Owner);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog export = new SaveFileDialog();
            export.FileName = "Minimap_" + m_CurArea;//Default name
            export.DefaultExt = ".bmp";//Default file extension
            export.Filter = "Bitmap BMP (.bmp)|*.bmp";//Filter by .obj
            if (export.ShowDialog() == DialogResult.Cancel)
                return;

            Bitmap bmp = new Bitmap(mapsize, mapsize);

            uint tileoffset = 0;
            for (int my = 0; my < mapsize; my += 8)
            {
                for (int mx = 0; mx < mapsize; mx += 8)
                {
                    ushort tilecrap = tmapfile[m_CurArea].Read16(tileoffset);
                    uint tilenum = (uint)(tilecrap & 0x03FF);
                    //Console.WriteLine("" + tilecrap);

                    for (int ty = 0; ty < 8; ty++)
                    {
                        for (int tx = 0; tx < 8; tx++)
                        {
                            uint totaloffset = (uint)(tilenum * 64 + ty * 8 + tx);//Address of current pixel
                            byte palentry = tsetfile.Read8(totaloffset);//Offset of current pixel's entry in palette file
                            //Palentry is double to get the position of the colour in the palette file
                            ushort pixel = palfile.Read16((uint)(palentry * 2));//Colour of current pixel from palette file
                            bmp.SetPixel(mx + tx, my + ty, Helper.BGR15ToColor(pixel));
                        }
                    }

                    tileoffset += 2;
                }
            }

            bmp.Save(export.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
        }

    }
}
