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

        int zoom = 2;

        LevelEditorForm _owner;

        public MinimapEditor()
        {
            InitializeComponent();
        }

        NitroFile palfile;
        NitroFile tsetfile;
        NitroFile[] tmapfiles;
        NitroFile tmapfile;

        int sizeX, sizeY;// Width and Height in pixels, divide by 8 to get number of tiles
        int bpp;
        Boolean usingTMap;

        private void RedrawMinimap(Boolean usingTmap, int sizeX, int sizeY, int bpp)
        {
            tsetfile = Program.m_ROM.GetFileFromName(txtSelNCG.Text);
            if (chkNCGDcmp.Checked)
                tsetfile.ForceDecompression();
            palfile = Program.m_ROM.GetFileFromName(txtSelNCL.Text);
            if (!txtSelNSC.Text.Equals(""))
            {
                usingTMap = true;
                tmapfile = Program.m_ROM.GetFileFromName(txtSelNSC.Text);
                if (chkNSCDcmp.Checked)
                    tmapfile.ForceDecompression();
            }
            else
                usingTMap = false;

            Bitmap bmp = LoadImage(usingTMap, sizeX, sizeY, bpp);

            pbxMinimapGfx.Image = new Bitmap(bmp, new Size(sizeX * zoom, sizeY * zoom));
            pbxMinimapGfx.Refresh();

            LoadPalette();
        }

        public Bitmap LoadImage(Boolean usingTMap, int sizeX, int sizeY, int bpp)
        {
            Bitmap bmp = new Bitmap(sizeX, sizeY);

            uint tileoffset = 0, tilenum = 0;
            ushort tilecrap = 0;
            for (int my = 0; my < sizeY; my += 8)
            {
                for (int mx = 0; mx < sizeX; mx += 8)
                {
                    if (usingTMap)
                    {
                        tilecrap = tmapfile.Read16(tileoffset);
                        tilenum = (uint)(tilecrap & 0x03FF);
                    }

                    for (int ty = 0; ty < 8; ty++)
                    {
                        for (int tx = 0; tx < 8; tx++)
                        {
                            if (bpp == 8)
                            {
                                uint totaloffset = (uint)(tilenum * 64 + ty * 8 + tx);//Address of current pixel
                                byte palentry = tsetfile.Read8(totaloffset);//Offset of current pixel's entry in palette file
                                //Palentry is double to get the position of the colour in the palette file
                                ushort pixel = palfile.Read16((uint)(palentry * 2));//Colour of current pixel from palette file
                                bmp.SetPixel(mx + tx, my + ty, Helper.BGR15ToColor(pixel));
                            }
                            else if (bpp == 4)
                            {
                                float totaloffset = (float)((float)(tilenum * 64 + ty * 8 + tx) / 2f);//Address of current pixel
                                byte palentry = 0;
                                if (totaloffset % 1 == 0)
                                {
                                    palentry = tsetfile.Read8((uint)totaloffset);//Offset of current pixel's entry in palette file
                                    palentry = (byte)(palentry & 0x0F);// Get 4 right bits
                                }
                                else
                                {
                                    palentry = tsetfile.Read8((uint)totaloffset);//Offset of current pixel's entry in palette file
                                    palentry = (byte)(palentry >> 4);// Get 4 left bits
                                }
                                //Palentry is double to get the position of the colour in the palette file
                                ushort pixel = palfile.Read16((uint)(palentry * 2));//Colour of current pixel from palette file
                                bmp.SetPixel(mx + tx, my + ty, Helper.BGR15ToColor(pixel));
                            }
                        }
                    }

                    tileoffset += 2;
                    if (!usingTMap)
                        tilenum++;
                }
            }

            return bmp;
        }

        private void LoadPalette()
        {
            gridPalette.RowCount = 16;
            gridPalette.ColumnCount = 16;

            //Read palette colours
            Color[] paletteColours = new Color[256];
            for (int i = 0; i < palfile.m_Data.Length / 2; i++)
            {
                //Colour in BGR15 format (16 bits) written to every even address 0,2,4...
                ushort palColour = palfile.Read16((uint)(i * 2));
                paletteColours[i] = Helper.BGR15ToColor(palColour);
            }
            for (int i = palfile.m_Data.Length / 2; i < 256; i++)
                paletteColours[i] = Helper.BGR15ToColor(0);// Fill blank entries with black
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

        public void importBMP(string filename, Boolean replaceMinimap)
        {
            // The tile maps (NSC / ISC) files for minimaps are always arranged a particular way - 0, 1, 2...15, 32 for 128 x 128
            Bitmap bmp = new Bitmap(filename);

            Color[] palette = bmp.Palette.Entries.ToArray<Color>();
            if (palette.Length > 256)
            {
                MessageBox.Show("Too many colours\n\n" + 
                    "You must import an indexed bitmap with 256 colours or fewer.");
                return;
            }

            //Write new palette
            palfile.Clear();
            for (int i = 0; i < palette.Length; i++)
            {
                //Colour in BGR15 format (16 bits) written to every even address 0,2,4...
                palfile.Write16((uint)i * 2, (ushort)(Helper.ColorToBGR15(palette[i])));
            }
            for (int i = palette.Length; i < 256; i++)
                palfile.Write16((uint)i * 2, 0);

            palfile.SaveChanges();
            
            //Fill current tmapfiles to use full mapsize x mapsize
            if (usingTMap)
            {
                tmapfile = Program.m_ROM.GetFileFromName(txtSelNSC.Text);
                tmapfile.Clear();
                sizeX = bmp.Width;
                sizeY = bmp.Height;
                uint addr = 0;
                int curTile = 0;
                int row = (int)(sizeX / 8);

                for (int my = 0; my < sizeY; my += 8)
                {
                    for (int mx = 0; mx < sizeX; mx += 8)
                    {
                        tmapfile.Write16(addr, (ushort)curTile);
                        curTile++;
                        addr += 2;
                    }
                }// End For
                if (chkNSCDcmp.Checked)
                    tmapfile.ForceCompression();
                tmapfile.SaveChanges();
            }// End If usingTMap

            //Check to see if there's already an identical tile and if so, change the current value to that
            //Works, but not if you want to keep existing data eg. multiple maps
            
            //List<List<byte>> tiles = new List<List<byte>>();
            //List<byte> curTilePal = new List<byte>();
            //uint tileoffset = 0;
            //for (int my = 0; my < sizeY; my += 8)
            //{
            //    for (int mx = 0; mx < sizeX; mx += 8)
            //    {
            //        ushort tilecrap = tmapfile.Read16(tileoffset);
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
            //            tmapfile.Write16(tileoffset, (ushort)(posInList(tiles, curTilePal)));
            //        }

            //        tileoffset += 2;
            //    }
            //}

            //Write the new image to file
            tsetfile = Program.m_ROM.GetFileFromName(txtSelNCG.Text);
            tsetfile.Clear();
            uint tileoffset = 0;
            uint tileNum = 0;
            for (int my = 0; my < sizeY; my += 8)
            {
                for (int mx = 0; mx < sizeX; mx += 8)
                {
                    for (int ty = 0; ty < 8; ty++)
                    {
                        for (int tx = 0; tx < 8; tx++)
                        {
                            if (bpp == 8)
                            {
                                uint totaloffset = (uint)(tileNum * 64 + ty * 8 + tx);//Position of current pixel's entry
                                byte palentry = (byte)(Array.IndexOf(palette, bmp.GetPixel(mx + tx, my + ty)));
                                tsetfile.Write8(totaloffset, (byte)(palentry));
                            }
                            else if (bpp == 4)
                            {
                                float totaloffset = (float)((float)(tileNum * 64 + ty * 8 + tx) / 2f);//Address of current pixel
                                byte palentry = (byte)(Array.IndexOf(palette, bmp.GetPixel(mx + tx, my + ty)));
                                if (totaloffset % 1 == 0)
                                {
                                    // Right 4 bits
                                    tsetfile.Write8((uint)totaloffset, (byte)palentry);
                                    //(byte)((tsetfile.Read8((uint)totaloffset) & 0xF0) | palentry));
                                }
                                else
                                {
                                    // Left 4 bits
                                    tsetfile.Write8((uint)totaloffset, (byte)((palentry << 4) | (tsetfile.Read8((uint)totaloffset) & 0x0F)));
                                }
                            }
                        }
                    }

                    tileoffset += 2;
                    tileNum++;
                }
            }

            if (chkNCGDcmp.Checked)
                tsetfile.ForceCompression();
            tsetfile.SaveChanges();

            // If it's a minimap that's being replaced, fill the tile maps to allow for multiple maps 
            // and ensure the image's displayed at the right size as you can't change the size of 
            // a level's minimap - it seems to be hardcoded somewhere (in level header?)
            if (replaceMinimap)
            {
                try
                {
                    if (chk128.Checked)
                        FillMinimapTiles(128);
                    else if (chk256.Checked)
                        FillMinimapTiles(256);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message + ex.Source + ex.StackTrace); }
            }
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
            uint tileoffset = 0, tilenum = 0;
            ushort tilecrap = 0;
            for (int my = 0; my < sizeY; my += 8)
            {
                for (int mx = 0; mx < sizeX; mx += 8)
                {
                    if (usingTMap)
                    {
                        tilecrap = tmapfile.Read16(tileoffset);
                        tilenum = (uint)(tilecrap & 0x03FF);
                    }

                    for (int ty = 0; ty < 8; ty++)
                    {
                        for (int tx = 0; tx < 8; tx++)
                        {
                            if (bpp == 8)
                            {
                                uint totaloffset = (uint)(tilenum * 64 + ty * 8 + tx);//Position of current pixel's entry
                                byte palentry = tsetfile.Read8(totaloffset);
                                if (palentry == 0)//If the current pixel points to first colour in palette, 
                                    tsetfile.Write8(totaloffset, (byte)(swapped));//point it to the swapped colour
                                if (palentry == (byte)swapped)//If the current pixel points to the swapped colour in palette, 
                                    tsetfile.Write8(totaloffset, (byte)0);//point it to the first colour
                            }
                            else if (bpp == 4)
                            {
                                float totaloffset = (float)((float)(tilenum * 64 + ty * 8 + tx) / 2f);//Address of current pixel
                                byte palentry = 0;
                                if (totaloffset % 1 == 0)
                                {
                                    // Right 4 bits
                                    palentry = tsetfile.Read8((uint)totaloffset);//Offset of current pixel's entry in palette file
                                    palentry = (byte)(palentry & 0x0F);// Get 4 right bits
                                    if (palentry == 0)//If the current pixel points to first colour in palette, 
                                        tsetfile.Write8((uint)totaloffset, (byte)((tsetfile.Read8((uint)totaloffset) & 0xF0) | swapped));//point it to the swapped colour
                                    if (palentry == (byte)swapped)//If the current pixel points to the swapped colour in palette, 
                                        tsetfile.Write8((uint)totaloffset, (byte)((tsetfile.Read8((uint)totaloffset) & 0xF0) | 0));//point it to the first colour
                                }
                                else
                                {
                                    // Left 4 bits
                                    palentry = tsetfile.Read8((uint)totaloffset);//Offset of current pixel's entry in palette file
                                    palentry = (byte)(palentry >> 4);
                                    if (palentry == 0)//If the current pixel points to first colour in palette, 
                                        tsetfile.Write8((uint)totaloffset, (byte)((swapped << 4) | (tsetfile.Read8((uint)totaloffset) & 0x0F)));//point it to the swapped colour
                                    if (palentry == (byte)swapped)//If the current pixel points to the swapped colour in palette, 
                                        tsetfile.Write8((uint)totaloffset, (byte)(0 | (tsetfile.Read8((uint)totaloffset) & 0x0F)));//point it to the first colour
                                }
                            }
                        }
                    }

                    tileoffset += 2;
                    if (!usingTMap)
                        tilenum++;
                }
            }

            if (chkNCGDcmp.Checked)
                tsetfile.ForceCompression();
            tsetfile.SaveChanges();

            RedrawMinimap(usingTMap, sizeX, sizeY, bpp);
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

                try
                {
                    loadMinimapFiles();

                    RedrawMinimap(usingTMap, sizeX, sizeY, bpp);
                }
                catch { myself.Enabled = false; };// The particular tile map doesn't exist
            }
        }

        private void MinimapEditor_Load(object sender, EventArgs e)
        {
            LevelEditorForm _owner = (LevelEditorForm)Owner;
            this._owner = _owner;

            m_NumAreas = _owner.m_NumAreas;
            m_CurArea = 0;

            tmapfiles = new NitroFile[m_NumAreas];

            txtCoordScale.Text = "" + (_owner.m_Overlay.Read16((uint)0x76) / 1000f);

            int i, pos = tsMinimapEditor.Items.IndexOf(tslBeforeAreaBtns) + 1;
            for (i = 0; i < m_NumAreas; i++, pos++)
            {
                ToolStripButton btn = new ToolStripButton(i.ToString(), null, new EventHandler(btnAreaXX_Click));
                btn.Tag = i;
                tsMinimapEditor.Items.Insert(pos, btn);
            }

            ((ToolStripButton)tsMinimapEditor.Items[pos - i]).Checked = true;

            for (int j = 1024; j >= 0; j -= 8)
            {
                dmnWidth.Items.Add(j);
                dmnHeight.Items.Add(j);
            }

            txtZoom.Text = "" + zoom;

            loadMinimapFiles();

            RedrawMinimap(usingTMap, sizeX, sizeY, bpp);
        }

        private void loadMinimapFiles()
        {

            palfile = Program.m_ROM.GetFileFromInternalID(_owner.m_LevelSettings.MinimapPalFileID);
            tsetfile = Program.m_ROM.GetFileFromInternalID(_owner.m_LevelSettings.MinimapTsetFileID);
            for (int j = 0; j < m_NumAreas; j++)
            {
                try
                {
                    if (_owner.m_MinimapFileIDs[j] != 0)
                    {
                        tmapfiles[j] = (Program.m_ROM.GetFileFromInternalID(_owner.m_MinimapFileIDs[j]));
                        tsMinimapEditor.Items[1 + j].Enabled = true;
                    }
                    else
                        tsMinimapEditor.Items[1 + j].Enabled = false;
                }
                catch//If the file doesn't exist
                {
                    tsMinimapEditor.Items[1 + j].Enabled = false;
                }
            }

            tmapfile = tmapfiles[m_CurArea];
            tmapfile.ForceDecompression();// Only to get accurate size below

            usingTMap = true;

            sizeX = sizeY = (int)(Math.Sqrt(tmapfile.m_Data.Length / 2) * 8);// Minimaps are squares
            bpp = 8;// Bits per pixel is always 8 for the minimaps
            dmnHeight.Text = dmnWidth.Text = "" + sizeX;
            cbxBPP.SelectedIndex = 1;
            if (sizeX == 128)
            {
                chk128.Checked = true;
                chk256.Checked = false;
            }
            else if (sizeX == 256)
            {
                chk128.Checked = false;
                chk256.Checked = true;
            }

            txtSelNCG.Text = tsetfile.m_Name;
            txtSelNCL.Text = palfile.m_Name;
            txtSelNSC.Text = tmapfile.m_Name;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Bitmap (.bmp)|*.bmp";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    importBMP(ofd.FileName, chkIsMinimap.Checked);

                    if (chkIsMinimap.Checked)
                    {
                        if (chk128.Checked)
                            sizeX = sizeY = 128;
                        else if (chk256.Checked)
                            sizeX = sizeY = 256;
                    }
                    RedrawMinimap(usingTMap, sizeX, sizeY, bpp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ex.Source + "\n\nAn error occured:\nCheck they're all valid files." +
                        "\nCheck whether they have/haven't already been decompressed.\nCheck it's a valid size." +
                        "\nCheck that you're using the correct bits per pixel.");
                }
            }
        }

        private void FillMinimapTiles(int size)
        {
            List<int> validFiles = new List<int>();
            for (int j = 0; j < m_NumAreas; j++)
            {
                try
                {
                    if (_owner.m_MinimapFileIDs[j] != 0)
                    {
                        tmapfiles[j] = (Program.m_ROM.GetFileFromInternalID(_owner.m_MinimapFileIDs[j]));
                        validFiles.Add(j);
                    }
                }
                catch { }// Doesn't exist
            }
            for (int i = 0; i < validFiles.Count; i++)
            {
                try { tmapfiles[validFiles[i]].Clear(); }
                catch { continue; }

                uint addr = 0;
                int curTile = 0;
                int row = (int)(size / 8);
                // Images are arranged left to right, top to bottom. Eg. for 128x128 maps:
                // If imported image is 256 x 256,
                // | 1 | 2 |
                // | 3 | 4 |
                    
                // If it's 384 x 384,
                // | 1 | 2 | 3 |
                // | 4 | 5 | 6 |
                // | 7 | 8 | 9 |

                int numInRow = sizeX / size;
                curTile += (i < numInRow) ? (row * i) : (row * (i - numInRow)) + ((row * row * numInRow) * (i / numInRow));
                int count = 0;
                for (int my = 0; my < size; my += 8)
                {
                    for (int mx = 0; mx < size; mx += 8)
                    {
                        if (count == row && validFiles.Count > 1)
                        {
                            curTile += row;
                            count = 0;
                        }
                        tmapfiles[validFiles[i]].Write16(addr, (ushort)curTile);
                        curTile++;
                        count++;
                        addr += 2;
                    }
                }// End For

                if (chkNSCDcmp.Checked)
                    tmapfiles[validFiles[i]].ForceCompression();
                tmapfiles[validFiles[i]].SaveChanges();
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

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog export = new SaveFileDialog();
            export.FileName = "Minimap_" + m_CurArea;//Default name
            export.DefaultExt = ".bmp";//Default file extension
            export.Filter = "Bitmap BMP (.bmp)|*.bmp";//Filter by .obj
            if (export.ShowDialog() == DialogResult.Cancel)
                return;
            try
            {
                sizeX = int.Parse(dmnWidth.Text);
                sizeY = int.Parse(dmnHeight.Text);
                bpp = int.Parse(cbxBPP.Items[cbxBPP.SelectedIndex].ToString());
                Bitmap bmp = LoadImage(usingTMap, sizeX, sizeY, bpp);

                bmp.Save(export.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.Source);
            }
        }

        private void btnSelNCG_Click(object sender, EventArgs e)
        {
            using (var form = new ROMFileSelect("Please select a Graphic (NCG) file."))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtSelNCG.Text = form.m_SelectedFile;
                }
            }
        }

        private void btnSelNCL_Click(object sender, EventArgs e)
        {
            using (var form = new ROMFileSelect("Please select a Palette (NCL) file."))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtSelNCL.Text = form.m_SelectedFile;
                }
            }
        }

        private void btnSelNSC_Click(object sender, EventArgs e)
        {
            using (var form = new ROMFileSelect("Please select a Tile (NSC) file."))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtSelNSC.Text = form.m_SelectedFile;
                }
            }
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            if (txtSelNCG.Text.Equals("") || txtSelNCL.Text.Equals(""))
                MessageBox.Show("You must select a Graphic (NCG) and a Palette (NCL) file,");
            else
            {
                try
                {
                    if (!txtSelNSC.Text.Equals(""))
                    {
                        usingTMap = true;
                    }
                    else
                        usingTMap = false;

                    sizeX = int.Parse(dmnWidth.Text);
                    sizeY = int.Parse(dmnHeight.Text);
                    bpp = int.Parse(cbxBPP.Items[cbxBPP.SelectedIndex].ToString());

                    RedrawMinimap(usingTMap, sizeX, sizeY, bpp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ex.Source + "\n\nAn error occured:\nCheck they're all valid files." + 
                        "\nCheck whether they have/haven't already been decompressed.\nCheck it's a valid size." + 
                        "\nCheck that you're using the correct bits per pixel.");
                }
            }
        }

        private void txtZoom_TextChanged(object sender, EventArgs e)
        {
            try
            {
                zoom = int.Parse(txtZoom.Text);

                RedrawMinimap(usingTMap, sizeX, sizeY, bpp);
            }
            catch { }
        }

        private void chk128_CheckedChanged(object sender, EventArgs e)
        {
            if (chk128.Checked)
                chk256.Checked = false;
            else
                chk256.Checked = true;
        }

        private void chk256_CheckedChanged(object sender, EventArgs e)
        {
            if (chk256.Checked)
                chk128.Checked = false;
            else
                chk128.Checked = true;
        }

    }
}
