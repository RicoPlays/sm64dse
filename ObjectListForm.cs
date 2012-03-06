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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SM64DSe
{
    public partial class ObjectListForm : Form
    {
        public ushort ObjectID;
        private Dictionary<ushort, bool> m_ObjAvailable;

        public ObjectListForm(ushort objid)
        {
            InitializeComponent();
            ObjectID = objid;
        }

        private void AppendToObjectDesc(string text, FontStyle style, Color color)
        {
            int pos = rtbObjectDesc.Text.Length;
            rtbObjectDesc.Text += text;

            rtbObjectDesc.Select(pos, text.Length);
            rtbObjectDesc.SelectionFont = new Font(rtbObjectDesc.Font, style);
            rtbObjectDesc.SelectionColor = color;
            rtbObjectDesc.Select(0, 0);
        }

        private void lbxObjectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ObjectID = (ushort)lbxObjectList.SelectedIndex;
            if (ObjectID == 326)
                ObjectID = 511; // haxx

            // describe the object
            if (ObjectID != 511)
            {
                ObjectDatabase.ObjectInfo objinfo = ObjectDatabase.m_ObjectInfo[ObjectID];
                rtbObjectDesc.Text = "";
                AppendToObjectDesc(objinfo.m_Name, FontStyle.Bold, rtbObjectDesc.ForeColor);
                AppendToObjectDesc("\n\n" + objinfo.m_Description, FontStyle.Regular, rtbObjectDesc.ForeColor);
            }
            else
            {
                rtbObjectDesc.Text = "";
                AppendToObjectDesc("Minimap change", FontStyle.Bold, rtbObjectDesc.ForeColor);
                AppendToObjectDesc("\n\nChanges the minimap shown on the bottom screen when the user passes near it.", 
                    FontStyle.Regular, rtbObjectDesc.ForeColor);
            }
        }

        private void ObjectListForm_Load(object sender, EventArgs e)
        {
            m_ObjAvailable = ((LevelEditorForm)Owner).m_ObjAvailable;
            for (int i = 0; i < 326; i++)
            {
                ObjectDatabase.ObjectInfo objinfo = ObjectDatabase.m_ObjectInfo[i];
                lbxObjectList.Items.Insert(i, string.Format("{0} - {1}",
                    i, objinfo.m_Name));
            }

            lbxObjectList.Items.Insert(326, "511 - Minimap change");
            lbxObjectList.SelectedIndex = (ObjectID == 511) ? 326 : ObjectID;
        }

        private void lbxObjectList_DoubleClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void lbxObjectList_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            
            ushort id = (ushort)e.Index;
            bool sel = (e.State & (DrawItemState.Focus | DrawItemState.Selected)) != 0;

            bool available;
            if (id == 326) available = true;
            else available = m_ObjAvailable[id];

            Color txtcolor;
            if (available)
                txtcolor = sel ? SystemColors.HighlightText : SystemColors.ControlText;
            else
                txtcolor = sel ? Color.LightPink : Color.Red;

            e.Graphics.DrawString((string)lbxObjectList.Items[id], lbxObjectList.Font, new SolidBrush(txtcolor), e.Bounds);
            e.DrawFocusRectangle();
        }
    }
}
