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
using System.Net;
using System.Web;

namespace SM64DSe
{
    public partial class MainForm : Form
    {
        private void LoadROM(string filename)
        {
            if (!File.Exists(filename))
            {
                MessageBox.Show("The specified file doesn't exist.", Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Program.m_ROMPath != "")
            {
                while (Program.m_LevelEditors.Count > 0)
                    Program.m_LevelEditors[0].Close();

                lbxLevels.Items.Clear();

                Program.m_ROM.EndRW();
            }

            Program.m_ROMPath = filename;
            try { Program.m_ROM = new NitroROM(Program.m_ROMPath); }
            catch (Exception ex)
            {
                string msg;

                if (ex is IOException)
                    msg = "The ROM couldn't be opened. Close any program that may be using it and try again.";
                else
                    msg = "The following error occured while loading the ROM:\n" + ex.Message;

                MessageBox.Show(msg, Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (Program.m_ROM != null)
                {
                    Program.m_ROM.EndRW();
                    Program.m_ROMPath = "";
                }
                return;
            }

            Program.m_ROM.BeginRW();
            if (Program.m_ROM.NeedsPatch())
            {
                DialogResult res = MessageBox.Show(
                    "This ROM needs to be patched before the editor can work with it.\n\n" +
                    "Do you want to first make a backup of it incase the patching\n" +
                    "operation goes wrong somehow?",
                    Program.AppTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                {
                    sfdSaveFile.FileName = Program.m_ROMPath.Substring(0, Program.m_ROMPath.Length - 4) + "_bak.nds";
                    if (sfdSaveFile.ShowDialog(this) == DialogResult.OK)
                    {
                        Program.m_ROM.EndRW();
                        File.Copy(Program.m_ROMPath, sfdSaveFile.FileName, true);
                    }
                }
                else if (res == DialogResult.Cancel)
                {
                    Program.m_ROM.EndRW();
                    Program.m_ROMPath = "";
                    return;
                }

                // switch to buffered RW mode (faster for patching)
                Program.m_ROM.EndRW();
                Program.m_ROM.BeginRW(true);

                try { Program.m_ROM.Patch(); }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "An error occured while patching your ROM.\n" + 
                        "No changes have been made to your ROM.\n" + 
                        "Try using a different ROM. If the error persists, report it to Mega-Mario, with the details below:\n\n" + 
                        ex.Message + "\n" + 
                        ex.StackTrace,
                        Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Program.m_ROM.EndRW(false);
                    Program.m_ROMPath = "";
                    return;
                }
            }

            Program.m_ROM.LoadTables();
            Program.m_ROM.EndRW();

            ModelCache.Init();
            // Program.m_ShaderCache = new ShaderCache();

            lbxLevels.Items.AddRange(Strings.LevelNames);

            btnEditTexts.Enabled = true;
        }

        public MainForm(string[] args)
        {
            InitializeComponent();

            Text = Program.AppTitle + " " + Program.AppVersion;
            Program.m_ROMPath = "";
            Program.m_LevelEditors = new List<LevelEditorForm>();

            if (!Program.AppVersion.ToLowerInvariant().Contains("private beta"))
                btnSecretShit.Visible = false;
            else
            {
                btnEditTexts.Visible = true; // remove me
                btnSecretShit.DropDownItems.Add("animation editor test", null, new EventHandler(this.animationeditortest));
            }

            slStatusLabel.Text = "Ready";
            ObjectDatabase.Initialize();

            if (args.Length >= 1) LoadROM(args[0]);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            ObjectDatabase.LoadFallback();
            try { ObjectDatabase.Load(); }
            catch { }

            if (!Properties.Settings.Default.AutoUpdateODB)
                return;

            ObjectDatabase.m_WebClient.DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(this.ODBDownloadProgressChanged);
            //ObjectDatabase.m_WebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.ODBDownloadDone);
            ObjectDatabase.m_WebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(this.ODBDownloadDone);

            ObjectDatabase.Update(false);
        }

        private void ODBDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!spbStatusProgress.Visible)
            {
                slStatusLabel.Text = "Updating object database...";
                spbStatusProgress.Visible = true;
            }

            spbStatusProgress.Value = e.ProgressPercentage;
        }

        private void ODBDownloadDone(object sender, DownloadStringCompletedEventArgs e)
        {
            spbStatusProgress.Visible = false;

            if (e.Cancelled || (e.Error != null))
            {
                slStatusLabel.Text = "Object database update " + (e.Cancelled ? "cancelled" : "failed") + ".";
            }
            else
            {
                if (e.Result == "noupdate")
                {
                    slStatusLabel.Text = "Object database already up to date.";
                }
                else
                {
                    slStatusLabel.Text = "Object database updated.";

                    try
                    {
                        File.WriteAllText("objectdb.xml", e.Result);
                    }
                    catch
                    {
                        slStatusLabel.Text = "Object database update failed.";
                    }
                }
            }

            try { ObjectDatabase.Load(); }
            catch { }
        }

        private void btnOpenROM_Click(object sender, EventArgs e)
        {
            btnEditLevel.Enabled = false;
            btnEditTexts.Enabled = false;
            btnKCLEditor.Enabled = false;

            if (ofdOpenFile.ShowDialog(this) == DialogResult.OK)
                LoadROM(ofdOpenFile.FileName);
        }

        private void OpenLevel(int levelid)
        {
            if ((levelid < 0) || (levelid >= 52))
                return;

            foreach (LevelEditorForm lvledit in Program.m_LevelEditors)
            {
                if (lvledit.LevelID == levelid)
                {
                    lvledit.Focus();
                    return;
                }
            }

           // try
            {
                LevelEditorForm newedit = new LevelEditorForm(Program.m_ROM, levelid);
                newedit.Show();
                Program.m_LevelEditors.Add(newedit);
            }
            /*catch (Exception ex)
            {
                MessageBox.Show("The following error occured while opening the level:\n" + ex.Message,
                    Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
        }


        private void btnEditLevel_Click(object sender, EventArgs e)
        {
            OpenLevel(lbxLevels.SelectedIndex);
        }

        private void lbxLevels_DoubleClick(object sender, EventArgs e)
        {
            OpenLevel(lbxLevels.SelectedIndex);
        }

        private void lbxLevels_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEditLevel.Enabled = (lbxLevels.SelectedIndex != -1);
            btnKCLEditor.Enabled = (lbxLevels.SelectedIndex != -1);
        }

        private void btnDumpObjInfo_Click(object sender, EventArgs e)
        {
            if (Program.m_ROM.m_Version != NitroROM.Version.EUR)
            {
                MessageBox.Show("Only compatible with EUR ROMs.", Program.AppTitle);
                return;
            }

            DumpObjectInfo();
        }

        private void btnUpdateODB_Click(object sender, EventArgs e)
        {
            ObjectDatabase.Update(true);
        }

        private void btnEditorSettings_Click(object sender, EventArgs e)
        {
            new SettingsForm().ShowDialog(this);
        }

        private void btnHalp_Click(object sender, EventArgs e)
        {
            string msg = Program.AppTitle + " " + Program.AppVersion + "\n\n" +
                "A level editor for Super Mario 64 DS.\n" +
                "Coding and design by Mega-Mario, with help from others (see credits).\n" +
                "Provided to you by Kuribo64, the SM64DS hacking department.\n" +
                "\n" +
                "Credits:\n" +
                "- Treeki for the overlay decompression (Jap77), the object list, and other help\n" +
                "- Dirbaio for other help\n" +
                "- blank for helping with generating collision\n" + 
                "- Fiachra Murray for helping with text editing, collision and other help\n" + 
                "\n" +
                Program.AppTitle + " is free software. If you paid for it, notify Mega-Mario about it.\n" +
                "\n" +
                "Visit Kuribo64's site (http://kuribo64.cjb.net/) for more details.";

            // for the lulz
            if (Program.AppVersion.ToLowerInvariant().Contains("private beta"))
                msg += "\n\nThis is a private beta. Leaking it out will get you sued by Kuribo64.";

            MessageBox.Show(msg, "About " + Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEditTexts_Click(object sender, EventArgs e)
        {
            new TextEditorForm().Show();
        }

        private void animationeditortest(object sender, EventArgs e)
        {
            new AnimationEditorForm().Show();
        }

        private void mnitDumpAllOvls_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 155; i++)
            {
                NitroOverlay overlay = new NitroOverlay(Program.m_ROM, (uint)i);
                string filename = "DecompressedOverlays/overlay_" + i.ToString("0000") + ".bin";
                string dir = "DecompressedOverlays";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                System.IO.File.WriteAllBytes(filename, overlay.m_Data);
            }
            MessageBox.Show("All overlays have been successfully dumped.");
        }

        private void btnKCLEditor_Click(object sender, EventArgs e)
        {
            uint ovlID = Program.m_ROM.GetLevelOverlayID(lbxLevels.SelectedIndex);
            NitroOverlay curOvl = new NitroOverlay(Program.m_ROM, ovlID);
            NitroFile curKCL = Program.m_ROM.GetFileFromInternalID(curOvl.Read16((uint)(0x6A)));
            KCLEditorForm kclForm = new KCLEditorForm(curKCL);
            kclForm.Show();
        }

    }
}
