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

        public MinimapEditor()
        {
            InitializeComponent();
        }


        private void RedrawMinimap(LevelEditorForm _owner)
        {
            NitroFile palfile = Program.m_ROM.GetFileFromInternalID(_owner.m_LevelSettings.MinimapPalFileID);
            NitroFile tsetfile = Program.m_ROM.GetFileFromInternalID(_owner.m_LevelSettings.MinimapTsetFileID);
            NitroFile tmapfile = Program.m_ROM.GetFileFromInternalID(_owner.m_MinimapFileIDs[m_CurArea]);

            tmapfile.ForceDecompression();
            tsetfile.ForceDecompression();

            int mapsize = 0;
            switch (tmapfile.m_Data.Length)
            {
                case 2 * 16 * 16: mapsize = 128; break;
                case 2 * 32 * 32: mapsize = 256; break;
                case 2 * 64 * 64: mapsize = 512; break;       // maps should never get that big
                case 2 * 128 * 128: mapsize = 1024; break;    // but we never know :P
                default: throw new Exception("map is fucked up (filesize " + tmapfile.m_Data.Length.ToString() + " bytes)");
            }

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
                            uint totaloffset = (uint)(tilenum * 64 + ty * 8 + tx);
                            byte palentry = tsetfile.Read8(totaloffset);
                            ushort pixel = palfile.Read16((uint)(palentry * 2));
                            bmp.SetPixel(mx + tx, my + ty, Helper.BGR15ToColor(pixel));
                        }
                    }

                    tileoffset += 2;
                }
            }

            pbxMinimapGfx.Image = new Bitmap(bmp, new Size(mapsize * 2, mapsize * 2));
            pbxMinimapGfx.Refresh();
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

            m_NumAreas = _owner.m_NumAreas;
            m_CurArea = 0;

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
    }
}
