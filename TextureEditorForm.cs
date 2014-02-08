using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SM64DSe.Importers;

namespace SM64DSe
{
    public partial class TextureEditorForm : Form
    {
        BMD m_Model;
        BTP m_BTP;

        ModelImporter _owner;

        public TextureEditorForm(BMD model, ModelImporter _owner)
        {
            InitializeComponent();

            m_Model = model;
            this._owner = _owner;

            loadTextures();
        }

        private void loadTextures()
        {
            // Reload the model
            m_Model = new BMD(m_Model.m_File);

            if (m_BTP != null)
            {
                try
                {
                    NitroFile file = Program.m_ROM.GetFileFromName(m_BTP.m_FileName);
                    m_BTP = new BTP(file, m_Model);
                    m_BTP.ReadBMDTextures();
                }
                catch (Exception ex) { MessageBox.Show("Error loading BTP:\n" + ex.Message + "\n" + ex.Source); }
            }
            
            lbxTextures.Items.Clear();

            for (int i = 0; i < m_Model.m_TextureIDs.Keys.Count; i++)
            {
                lbxTextures.Items.Add(m_Model.m_TextureIDs.Keys.ElementAt(i));
            }

            lbxPalettes.Items.Clear();

            for (int i = 0; i < m_Model.m_PaletteIDs.Keys.Count; i++)
            {
                lbxPalettes.Items.Add(m_Model.m_PaletteIDs.Keys.ElementAt(i));
            }
        }

        private void lbxTextures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxTextures.SelectedIndex == -1)
                return;
            if (m_Model.m_Textures.ContainsKey(lbxTextures.SelectedItem.ToString()))
            {
                lbxPalettes.SelectedIndex = (int)m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_PalID;
            }
            if (lbxPalettes.SelectedIndex != -1)
            {
                BMD.Texture currentTexture = m_Model.ReadTexture(m_Model.m_TextureIDs[lbxTextures.SelectedItem.ToString()],
                    m_Model.m_PaletteIDs[lbxPalettes.SelectedItem.ToString()]);

                LoadBitmap(currentTexture);
            }
        }

        private void lbxPalettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxTextures.SelectedIndex != -1 && lbxPalettes.SelectedIndex != -1)
            {
                BMD.Texture currentTexture = m_Model.ReadTexture(m_Model.m_TextureIDs[lbxTextures.SelectedItem.ToString()],
                    m_Model.m_PaletteIDs[lbxPalettes.SelectedItem.ToString()]);

                LoadBitmap(currentTexture);
            }
        }

        private void LoadBitmap(BMD.Texture currentTexture)
        {
            Bitmap tex = new Bitmap((int)currentTexture.m_Width, (int)currentTexture.m_Height);

            for (int y = 0; y < (int)currentTexture.m_Height; y++)
            {
                for (int x = 0; x < (int)currentTexture.m_Width; x++)
                {
                    tex.SetPixel(x, y, Color.FromArgb(currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4 + 3],
                     currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4 + 2],
                     currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4 + 1],
                     currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4]));
                }
            }

            pbxTexture.Image = new Bitmap(tex);
            pbxTexture.Refresh();

            lblPalette.Text = "Palette " + currentTexture.m_PalID;
        }

        private void btnExportAll_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            String folderName = "";
            if( result == DialogResult.OK )
            {
                folderName = fbd.SelectedPath;
                for (int i = 0; i < m_Model.m_Textures.Values.Count; i++)
                {
                    BMD.Texture currentTexture = m_Model.m_Textures.Values.ElementAt(i);

                    SaveTextureAsPNG(currentTexture, folderName + "/" + currentTexture.m_TexName + ".png");
                }
                MessageBox.Show("Successfully exported " + m_Model.m_Textures.Values.Count + " texture(s) to:\n" + folderName);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (lbxTextures.SelectedIndex != -1)
            {
                BMD.Texture currentTexture = m_Model.m_Textures.Values.ElementAt(lbxTextures.SelectedIndex);

                SaveFileDialog export = new SaveFileDialog();
                export.FileName = currentTexture.m_TexName;//Default name
                export.DefaultExt = ".png";//Default file extension
                export.Filter = "PNG (.png)|*.png";//Filter by .png
                if (export.ShowDialog() == DialogResult.Cancel)
                    return;

                SaveTextureAsPNG(currentTexture, export.FileName);
            }
            else
            {
                MessageBox.Show("Please select a texture first.");
            }
        }

        private static void SaveTextureAsPNG(BMD.Texture currentTexture, String fileName)
        {
            Bitmap tex = new Bitmap((int)currentTexture.m_Width, (int)currentTexture.m_Height);

            for (int y = 0; y < (int)currentTexture.m_Height; y++)
            {
                for (int x = 0; x < (int)currentTexture.m_Width; x++)
                {
                    tex.SetPixel(x, y, Color.FromArgb(currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4 + 3],
                     currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4 + 2],
                     currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4 + 1],
                     currentTexture.m_Data[((y * currentTexture.m_Width) + x) * 4]));
                }
            }

            try
            {
                tex.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while trying to save texture " + currentTexture.m_TexName + ".\n\n " +
                    ex.Message + "\n" + ex.Data + "\n" + ex.StackTrace + "\n" + ex.Source);
            }
        }

        private void btnReplaceSelected_Click(object sender, EventArgs e)
        {
            if (lbxTextures.SelectedIndex != -1)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Select an image";
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.Cancel)
                    return;

                //int index = lbxTextures.SelectedIndex;

                int texIndex = lbxTextures.SelectedIndex = (int)m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_TexID;
                int palIndex = lbxPalettes.SelectedIndex = (int)m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_PalID;

                try
                {
                    BMD_Importer.ConvertedTexture tex = BMD_Importer.ConvertTexture(ofd.FileName);
                    tex.m_TextureID = (uint)texIndex;
                    tex.m_PaletteID = (uint)palIndex;

                    // Update texture entry
                    uint curoffset = m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_EntryOffset;

                    m_Model.m_File.Write32(curoffset + 0x08, (uint)tex.m_TextureDataLength);
                    m_Model.m_File.Write16(curoffset + 0x0C, (ushort)(8 << (int)((tex.m_DSTexParam >> 20) & 0x7)));
                    m_Model.m_File.Write16(curoffset + 0x0E, (ushort)(8 << (int)((tex.m_DSTexParam >> 23) & 0x7)));
                    m_Model.m_File.Write32(curoffset + 0x10, tex.m_DSTexParam);

                    // Update palette entry
                    if (tex.m_PaletteData != null && !chkNewPalette.Checked)
                    {
                        curoffset = m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_PalEntryOffset;

                        m_Model.m_File.Write32(curoffset + 0x08, (uint)tex.m_PaletteData.Length);
                        m_Model.m_File.Write32(curoffset + 0x0C, 0xFFFFFFFF);
                    }

                    // Write new texture and texture palette data

                    // Check if we need to make room for additional data

                    // For compressed (type 5) textures, the size of the texture data doesn't count the palette index data.
                    // The texture data is then directly followed by (size/2) of palette index data.

                    uint oldTexDataSize = (uint)m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_TexDataSize;
                    if (m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_TexType == 5)
                        oldTexDataSize += (oldTexDataSize / 2);
                    uint newTexDataSize = (uint)((tex.m_TextureData.Length + 3) & ~3);
                    uint oldPalDataSize = (uint)m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_PalSize;
                    uint newPalDataSize = (uint)((tex.m_PaletteData.Length + 3) & ~3);

                    uint texDataOffset = m_Model.m_File.Read32(m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_EntryOffset + 0x04);
                    // If necessary, make room for additional texture data
                    if (newTexDataSize > oldTexDataSize)
                        m_Model.AddSpace(texDataOffset + oldTexDataSize, newTexDataSize - oldTexDataSize);

                    m_Model.m_File.WriteBlock(texDataOffset, tex.m_TextureData);

                    if (!chkNewPalette.Checked) // If we're editing an existing palette
                    {
                        uint palDataOffset = m_Model.m_File.Read32(m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_PalEntryOffset + 0x04);
                        // If necessary, make room for additional palette data
                        if (newPalDataSize > oldPalDataSize)
                            m_Model.AddSpace(palDataOffset + oldPalDataSize, newPalDataSize - oldPalDataSize);
                        // Reload palette data offset
                        palDataOffset = m_Model.m_File.Read32(m_Model.m_Textures[lbxTextures.SelectedItem.ToString()].m_PalEntryOffset + 0x04);

                        if (tex.m_PaletteData != null)
                        {
                            m_Model.m_File.WriteBlock(palDataOffset, tex.m_PaletteData);
                        }
                    }
                    else if (chkNewPalette.Checked)
                    {
                        /*
                         * TODO: To get this to work, need to ensure all offsets remain 4 byte aligned
                         */ 

                        String newPalName = lbxTextures.Items[texIndex].ToString() + "_new";

                        uint newHdrOff = m_Model.m_PalChunksOffset + (m_Model.m_NumPalChunks * 16);
                        m_Model.AddSpace(newHdrOff, 16);
                        uint newPalNameOff = m_Model.m_File.Read32(m_Model.m_Textures.Values.ElementAt(m_Model.m_Textures.Values.Count - 1).m_PalEntryOffset);
                        while (m_Model.m_File.Read8(newPalNameOff) != 0)
                            newPalNameOff += 1;
                        m_Model.AddSpace((uint)newPalNameOff, (uint)newPalName.Length + 1);
                        m_Model.m_File.WriteString(newPalNameOff, newPalName, 0);
                        m_Model.m_File.Write32(newHdrOff, newPalNameOff);
                        uint newPalOff = m_Model.m_Textures.Values.ElementAt(m_Model.m_Textures.Values.Count - 1).m_PalOffset +
                            m_Model.m_Textures.Values.ElementAt(m_Model.m_Textures.Values.Count - 1).m_PalSize;
                        m_Model.AddSpace(newPalOff, (uint)tex.m_PaletteData.Length);
                        m_Model.m_File.Write32(newHdrOff + 0x04, newPalOff);
                        m_Model.m_File.Write32(newHdrOff + 0x08, (uint)tex.m_PaletteData.Length);
                        m_Model.m_File.Write32(newHdrOff + 0x0C, 0xFFFFFFFF);

                        m_Model.m_File.WriteBlock(newPalOff, tex.m_PaletteData);

                        // Set the material's palette ID to the new palette
                        for (int i = 0; i < m_Model.m_ModelChunks.Length; i++)
                        {
                            for (int j = 0; j < m_Model.m_ModelChunks[i].m_MatGroups.Length; j++)
                            {
                                if (m_Model.m_ModelChunks[i].m_MatGroups[j].m_Texture.m_TexName == lbxTextures.Items[texIndex].ToString())
                                {
                                    uint palID = m_Model.m_File.Read32(0x1C);
                                    uint matEntryOff = m_Model.m_File.Read32(0x28) + (m_Model.m_ModelChunks[i].m_MatGroups[j].m_ID * 48);
                                    m_Model.m_File.Write32(matEntryOff + 0x08, palID);
                                }
                            }
                        }

                        // Update number of palettes
                        m_Model.m_File.Write32(0x1C, m_Model.m_File.Read32(0x1C) + 1);
                    }

                    m_Model.m_File.SaveChanges();

                    loadTextures();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ex.Source + ex.TargetSite + ex.StackTrace);
                }
            }
            else
            {
                MessageBox.Show("Please select a texture first.");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            m_Model.m_File.SaveChanges();
        }

        private void btnLoadBTP_Click(object sender, EventArgs e)
        {
            using (var form = new ROMFileSelect("Please select a BTP file to load."))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    try
                    {
                        NitroFile file = Program.m_ROM.GetFileFromName(form.m_SelectedFile);
                        m_BTP = new BTP(file, m_Model);

                        loadTextures();
                    }
                    catch (Exception ex) { MessageBox.Show("Error loading BTP:\n" + ex.Message + "\n" + ex.Source); }
                }
            }
        }

    }
}
