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
        NitroFile tmapfile;
        int mapsize;
        private void RedrawMinimap(LevelEditorForm _owner)
        {
            palfile = Program.m_ROM.GetFileFromInternalID(_owner.m_LevelSettings.MinimapPalFileID);
            tsetfile = Program.m_ROM.GetFileFromInternalID(_owner.m_LevelSettings.MinimapTsetFileID);
            try
            {
                tmapfile = Program.m_ROM.GetFileFromInternalID(_owner.m_MinimapFileIDs[m_CurArea]);
            }
            catch//If the file doesn't exist
            {
                MessageBox.Show("Doesn't use separate map");
                return;
            }

            tmapfile.ForceDecompression();
            tsetfile.ForceDecompression();

            mapsize = 0;
            switch (tmapfile.m_Data.Length)
            {
                case 2 * 16 * 16: mapsize = 128; break;
                case 2 * 32 * 32: mapsize = 256; break;
                case 2 * 64 * 64: mapsize = 512; break;       // maps should never get that big
                case 2 * 128 * 128: mapsize = 1024; break;    // but we never know :P
                default: {
                    MessageBox.Show("Doesn't use separate map");
                    return;
                         }
            }

            lblMapSize.Text = "" + mapsize + " x " + mapsize;

            Bitmap bmp = new Bitmap(mapsize, mapsize);

            uint tileoffset = 0;
            for (int my = 0; my < mapsize; my += 8)
            {
                for (int mx = 0; mx < mapsize; mx += 8)
                {
                    ushort tilecrap = tmapfile.Read16(tileoffset);
                    uint tilenum = (uint)(tilecrap & 0x03FF);

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
            for (int i = 0; i < palette.Length; i++)
            {
                //Colour in BGR15 format (16 bits) written to every even address 0,2,4...
                palfile.Write16((uint)i * 2, (ushort)(Helper.ColorToBGR15(palette[i])));
            }

            uint tileoffset = 0;
            for (int my = 0; my < mapsize; my += 8)
            {
                for (int mx = 0; mx < mapsize; mx += 8)
                {
                    ushort tilecrap = tmapfile.Read16(tileoffset);
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
                    ushort tilecrap = tmapfile.Read16(tileoffset);
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

            txtCoordScale.Text = "" + (_owner.m_Overlay.Read16((uint)0x76) / 1000f);

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

    }
}
