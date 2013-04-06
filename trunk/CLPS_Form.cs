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
    public partial class CLPS_Form : Form
    {

        LevelEditorForm _owner;

        public CLPS_Form(LevelEditorForm _owner)
        {
            InitializeComponent();

            this._owner = _owner;

            uint clps_addr = _owner.m_Overlay.ReadPointer(0x60);
            ushort clps_num = _owner.m_Overlay.Read16(clps_addr + 0x06);
            uint clps_size = (uint)(8 + (clps_num * 8));
            txtNumEntries.Text = "" + clps_num;

            //uint entry = clps_addr + 0x08;
            //for (int i = 0; i < clps_num; i++)
            //{
            //    lbxEntries.Items.Add(_owner.m_Overlay.Read8(entry));
            //    entry += 8;
            //}

            for (int i = 0; i < 52; i++)
            {
                cbxLevels.Items.Add(i + " - " + Strings.LevelNames[i]);
            }
        }

        private void copyCLPS(int sourceLevel)
        {
            NitroOverlay otherOVL = new NitroOverlay(Program.m_ROM, Program.m_ROM.GetLevelOverlayID(sourceLevel));

            uint other_clps_addr = otherOVL.ReadPointer(0x60);
            ushort other_clps_num = otherOVL.Read16(other_clps_addr + 0x06);
            uint other_clps_size = (uint)(8 + (other_clps_num * 8));

            uint clps_addr = _owner.m_Overlay.ReadPointer(0x60);
            ushort clps_num = _owner.m_Overlay.Read16(clps_addr + 0x06);
            uint clps_size = (uint)(8 + (clps_num * 8));

            byte[] CLPS_data = otherOVL.ReadBlock(other_clps_addr, other_clps_size);

            if (clps_size < other_clps_size)
            {
                AddSpace(clps_addr + clps_size, other_clps_size - clps_size);
            }
            else if (clps_size > other_clps_size)
            {
                RemoveSpace(clps_addr + clps_size - (clps_size - other_clps_size), clps_size - other_clps_size);
            }

            _owner.m_Overlay.WriteBlock(clps_addr, CLPS_data);
        }

        private void AddSpace(uint offset, uint amount)
        {
            if ((_owner.m_Overlay.GetSize() + amount) > NitroROM.LEVEL_OVERLAY_SIZE)
                throw new Exception("This level has reached the level size limit. Cannot add more data.");

            // move the data
            byte[] block = _owner.m_Overlay.ReadBlock(offset, (uint)(_owner.m_Overlay.GetSize() - offset));
            _owner.m_Overlay.WriteBlock(offset + amount, block);

            // write zeroes in the newly created space
            for (int i = 0; i < amount; i++)
                _owner.m_Overlay.Write8((uint)(offset + i), 0);

            // update the pointers
            for (int i = 0; i < _owner.m_PointerList.Count; i++)
            {
                LevelEditorForm.PointerReference ptrref = _owner.m_PointerList[i];
                if (ptrref.m_ReferenceAddr >= offset)
                    ptrref.m_ReferenceAddr += amount;
                if (ptrref.m_PointerAddr >= offset)
                {
                    ptrref.m_PointerAddr += amount;
                    _owner.m_Overlay.WritePointer(ptrref.m_ReferenceAddr, ptrref.m_PointerAddr);
                }
                _owner.m_PointerList[i] = ptrref;
            }

            // update the objects 'n' all
            UpdateObjectOffsets(offset, amount);
        }

        private void RemoveSpace(uint offset, uint amount)
        {
            // move the data
            byte[] block = _owner.m_Overlay.ReadBlock(offset + amount, (uint)(_owner.m_Overlay.GetSize() - offset - amount));
            _owner.m_Overlay.WriteBlock(offset, block);
            _owner.m_Overlay.SetSize(_owner.m_Overlay.GetSize() - amount);

            // update the pointers
            for (int i = 0; i < _owner.m_PointerList.Count; i++)
            {
                LevelEditorForm.PointerReference ptrref = _owner.m_PointerList[i];
                if (ptrref.m_ReferenceAddr >= (offset + amount))
                    ptrref.m_ReferenceAddr -= amount;
                if (ptrref.m_PointerAddr >= (offset + amount))
                {
                    ptrref.m_PointerAddr -= amount;
                    _owner.m_Overlay.WritePointer(ptrref.m_ReferenceAddr, ptrref.m_PointerAddr);
                }
                _owner.m_PointerList[i] = ptrref;
            }

            // update the objects 'n' all
            UpdateObjectOffsets(offset + amount, (uint)-amount);
        }

        public void UpdateObjectOffsets(uint start, uint delta)
        {
            foreach (LevelObject obj in _owner.m_LevelObjects.Values)
                if (obj.m_Offset >= start) obj.m_Offset += delta;

            for (int a = 0; a < _owner.m_TexAnims.Length; a++)
            {
                foreach (LevelTexAnim anim in _owner.m_TexAnims[a])
                {
                    if (anim.m_Offset >= start) anim.m_Offset += delta;
                    if (anim.m_ScaleTblOffset >= start) anim.m_ScaleTblOffset += delta;
                    if (anim.m_RotTblOffset >= start) anim.m_RotTblOffset += delta;
                    if (anim.m_TransTblOffset >= start) anim.m_TransTblOffset += delta;
                    if (anim.m_MatNameOffset >= start) anim.m_MatNameOffset += delta;
                }
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (cbxLevels.SelectedIndex != -1)
                copyCLPS(cbxLevels.SelectedIndex);
        }
    }
}
