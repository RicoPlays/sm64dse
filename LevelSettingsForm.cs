﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SM64DSe
{
    public partial class LevelSettingsForm : Form
    {
        private LevelSettings m_LevelSettings;

        public LevelSettingsForm(LevelSettings settings)
        {
            InitializeComponent();
            m_LevelSettings = settings;
        }

        private void LevelSettingsForm_Load(object sender, EventArgs e)
        {
            string[][] bankobjs = new string[8][];
            bankobjs[0] = new string[7];
            bankobjs[1] = new string[7];
            bankobjs[2] = new string[7];
            bankobjs[3] = new string[7];
            bankobjs[4] = new string[7];
            bankobjs[5] = new string[7];
            bankobjs[6] = new string[7];
            bankobjs[7] = new string[53];
            foreach (string[] foo in bankobjs)
                for (int i = 0; i < foo.Length; i++)
                    foo[i] = "";

            foreach (ObjectDatabase.ObjectInfo obj in ObjectDatabase.m_ObjectInfo)
            {
                if (obj.m_BankRequirement != 1)
                    continue;

                bankobjs[obj.m_NumBank][obj.m_BankSetting] += obj.m_ID.ToString() + ",";
            }

            ComboBox[] combos = { cbxBank0, cbxBank1, cbxBank2, cbxBank3, cbxBank4, cbxBank5, cbxBank6, cbxBank7 };
            for (int b = 0; b < 8; b++)
            {
                for (int i = 0; i < bankobjs[b].Length; i++)
                {
                    string txt = string.Format("[{0}] - ", i);
                    string[] objs = bankobjs[b][i].Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                    if (objs.Length > 0)
                    {
                        foreach (string _objid in objs)
                        {
                            int objid = int.Parse(_objid);
                            txt += string.Format("{0} ({1}), ", ObjectDatabase.m_ObjectInfo[objid].m_Name, objid);
                        }
                        txt = txt.Substring(0, txt.Length - 2);
                    }
                    else
                        txt += "(none)";
                    combos[b].Items.Add(txt);
                }

                combos[b].SelectedIndex = (int)m_LevelSettings.ObjectBanks[b];
            }

            cbxBackground.SelectedIndex = m_LevelSettings.Background;
        }

        private void cbxBankX_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;
            e.ItemWidth = cbx.Width;

            string txt = (string)cbx.Items[e.Index];
            int width = e.ItemWidth - (int)e.Graphics.MeasureString(txt.Substring(0, 6), cbx.Font).Width;
            e.ItemHeight = (int)e.Graphics.MeasureString(txt.Substring(6), cbx.Font, width).Height;
        }

        private void cbxBankX_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;

            Color txtcolor;
            if ((e.State & DrawItemState.ComboBoxEdit) != 0)
            {
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.ControlLightLight), e.Bounds);
                txtcolor = SystemColors.ControlText;
            }
            else
            {
                e.DrawBackground();

                if ((e.State & (DrawItemState.Focus | DrawItemState.Selected)) != 0)
                    txtcolor = SystemColors.HighlightText;
                else
                    txtcolor = SystemColors.ControlText;
            }

            Brush txtbrush = new SolidBrush(txtcolor);

            string txt = (string)cbx.Items[e.Index];

            int margin = (int)e.Graphics.MeasureString(txt.Substring(0, 6), cbx.Font).Width;
            RectangleF rect = e.Bounds;
            rect.X += margin;
            rect.Width -= margin;

            e.Graphics.DrawString(txt.Substring(0, 6), cbx.Font, txtbrush, e.Bounds.Location);
            e.Graphics.DrawString(txt.Substring(6), cbx.Font, txtbrush, rect);

            e.DrawFocusRectangle();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            m_LevelSettings.Background = (byte)cbxBackground.SelectedIndex;
            m_LevelSettings.ObjectBanks[0] = (uint)cbxBank0.SelectedIndex;
            m_LevelSettings.ObjectBanks[1] = (uint)cbxBank1.SelectedIndex;
            m_LevelSettings.ObjectBanks[2] = (uint)cbxBank2.SelectedIndex;
            m_LevelSettings.ObjectBanks[3] = (uint)cbxBank3.SelectedIndex;
            m_LevelSettings.ObjectBanks[4] = (uint)cbxBank4.SelectedIndex;
            m_LevelSettings.ObjectBanks[5] = (uint)cbxBank5.SelectedIndex;
            m_LevelSettings.ObjectBanks[6] = (uint)cbxBank6.SelectedIndex;
            m_LevelSettings.ObjectBanks[7] = (uint)cbxBank7.SelectedIndex;
        }

        private void cbxBackground_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((LevelEditorForm)Owner).UpdateSkybox(cbxBackground.SelectedIndex);
        }

        private void LevelSettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ((LevelEditorForm)Owner).UpdateSkybox(-1);
        }
    }
}