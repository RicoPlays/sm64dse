using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace SM64DSe
{
    public partial class TextureAnimationForm : Form
    {
        //reloadData() patch is only done on last area but Castle Grounds Multiplayer seems to be the only one affected, 
        //though may need to set it for each area in future
        
        public LevelEditorForm _owner;
        CultureInfo usa = new CultureInfo("en-US");
        public List<List<string>> matNames = new List<List<string>>();//Holds the material name of each texture animation of each area
        public List<List<uint>> scaleVals = new List<List<uint>>();//Holds the scale values table of each area
        public List<List<ushort>> rotVals = new List<List<ushort>>();//Holds the rotation values table of each area
        public List<List<uint>> transVals = new List<List<uint>>();//Holds the translation values table of each area
        public uint numAreas;
        public List<int> scaleTblLength = new List<int>();//Stores the length (number of values - not bytes) of the whole scale table
        public List<int> rotTblLength = new List<int>();
        public List<int> transTblLength = new List<int>();
        //Hold the scale, rotation and translation table start indices for each texture animation
        public List<List<ushort>> scaleStartIndices = new List<List<ushort>>();
        public List<List<ushort>> rotStartIndices = new List<List<ushort>>();
        public List<List<ushort>> transStartIndices = new List<List<ushort>>();
        //Hold the scale, rotation and translation table sizes for each texture animation
        public List<List<ushort>> scaleSizes = new List<List<ushort>>();
        public List<List<ushort>> rotSizes = new List<List<ushort>>();
        public List<List<ushort>> transSizes = new List<List<ushort>>();
        
        public TextureAnimationForm(LevelEditorForm _owner)
        {
            InitializeComponent();
            this._owner = _owner;

            for (int i = 0; i < _owner.m_NumAreas; i++)
            {
                lbxArea.Items.Add("" + i);
            }
            lbxArea.SelectedIndex = 0;//Make sure an area is selected

            reloadData();
        }

        private void lbxTexAnim_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxTexAnim.SelectedIndex != -1)
                refreshLbx();
        }

        private void refreshLbx()
        {
            txtMaterialName.Text = matNames[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex];
            lbxScale.Items.Clear();
            for (int i = 0; i < scaleVals[lbxArea.SelectedIndex].Count; i++)
            {
                lbxScale.Items.Add("Scale value " + i);
            }
            lbxRotation.Items.Clear();
            for (int i = 0; i < rotVals[lbxArea.SelectedIndex].Count; i++)
            {
                lbxRotation.Items.Add("Rotation value " + i);
            }
            lbxTranslation.Items.Clear();
            for (int i = 0; i < transVals[lbxArea.SelectedIndex].Count; i++)
            {
                lbxTranslation.Items.Add("Translation value " + i);
            }
            txtScaleStart.Text = scaleStartIndices[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].ToString();
            txtScaleSize.Text = scaleSizes[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].ToString();
            txtRotStart.Text = rotStartIndices[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].ToString();
            txtRotSize.Text = rotSizes[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].ToString();
            txtTransStart.Text = transStartIndices[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].ToString();
            txtTransSize.Text = transSizes[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].ToString();

            //Take this out when done testing
            textBox1.Text = "" + _owner.m_Overlay.Read32(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_TexAnimHeaderOffset) +
                ",  " + _owner.m_Overlay.Read32(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_Offset) +
                ",  " + _owner.m_Overlay.Read32((uint)(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_Offset + 0x08)) +
                ",  " + _owner.m_Overlay.Read16((uint)(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_Offset + 0x18)) +
                ",  " + _owner.m_Overlay.Read16((uint)(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_Offset + 0x1A));

        }

        private void lbxArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1)
            {
                lbxTexAnim.Items.Clear();
                for (int j = 0; j < _owner.m_TexAnims[lbxArea.SelectedIndex].Count; j++)
                {
                    lbxTexAnim.Items.Add("" + j);
                }
            }
        }

        private void lbxScale_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxScale.SelectedIndex != -1)
                txtScale.Text = (scaleVals[lbxArea.SelectedIndex][lbxScale.SelectedIndex] / 20 * 12 / 1000f).ToString(usa);//Is it /1000?
        }

        private void lbxRotation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxRotation.SelectedIndex != -1)
                txtRotation.Text = (rotVals[lbxArea.SelectedIndex][lbxRotation.SelectedIndex] / (1024f/90f)).ToString(usa);//1024 = 90 degrees
        }

        private void lbxTranslation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxTranslation.SelectedIndex != -1)
                txtTranslation.Text = (transVals[lbxArea.SelectedIndex][lbxTranslation.SelectedIndex] / 20 * 12 / 1000f).ToString(usa);//Is it /1000?
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            uint numAreas = _owner.m_Overlay.Read8(0x74);
            uint objlistptr = _owner.m_Overlay.ReadPointer(0x70);

            for (byte a = 0; a < _owner.m_NumAreas; a++)//For each area in current overlay
            {
                uint addr = (uint)(objlistptr + (a * 12));//Each level data header is 12 bytes - get the address of current one

                //Texture animation addresses have an offset of 4 bytes within each level data header
                addr += 4;
                if (_owner.m_Overlay.Read32(addr) != 0)//If current area's texture animation data pointer is not NULL
                {
                    _owner.m_Overlay.Write32(addr, 0);//Make it NULL
                }
            }
            reloadData();
            lbxScale.Items.Clear(); lbxRotation.Items.Clear(); lbxTranslation.Items.Clear();
            txtMaterialName.Text = ""; txtScale.Text = ""; txtRotation.Text = ""; txtTranslation.Text = "";
        }

        private void reloadData()
        {
            //Refresh data

            matNames = new List<List<string>>();
            scaleVals = new List<List<uint>>();
            rotVals = new List<List<ushort>>();
            transVals = new List<List<uint>>();
            scaleStartIndices = new List<List<ushort>>();
            scaleSizes = new List<List<ushort>>();
            rotStartIndices = new List<List<ushort>>();
            rotSizes = new List<List<ushort>>();
            transStartIndices = new List<List<ushort>>();
            transSizes = new List<List<ushort>>();
            scaleTblLength = new List<int>();
            rotTblLength = new List<int>();
            transTblLength = new List<int>();

            //The name of the last material goes straight into scale values, need to insert a NULL to avoid corruption
            bool nullFix = false;
            while (!nullFix)
            {
                numAreas = _owner.m_Overlay.Read8(0x74);
                uint objlistptr = _owner.m_Overlay.ReadPointer(0x70);

                _owner.m_TexAnims = new List<LevelTexAnim>[numAreas];
                for (int a = 0; a < numAreas; a++)
                {
                    _owner.m_TexAnims[a] = new List<LevelTexAnim>();
                }

                //Find reference to first texture animation
                uint firstTARef = 0;
                for (byte a = 0; a < numAreas; a++)
                {
                    uint addr = (uint)(objlistptr + (a * 12));
                    // read texture animation
                    addr += 4;
                    if (_owner.m_Overlay.Read32(addr) != 0)
                    {
                        firstTARef = addr;
                        break;
                    }
                }
                //Find its position within the list of pointers and delete everything after that
                int rmPos = 0;
                for (int i = 0; i < _owner.m_PointerList.Count; i++)
                {
                    LevelEditorForm.PointerReference ptrref = _owner.m_PointerList[i];
                    if (ptrref.m_ReferenceAddr == firstTARef)
                    {
                        rmPos = i;
                        break;
                    }
                }
                _owner.m_PointerList.RemoveRange(rmPos, _owner.m_PointerList.Count - rmPos);

                for (byte a = 0; a < numAreas; a++)
                {
                    uint addr = (uint)(objlistptr + (a * 12));

                    // read texture animation
                    addr += 4;
                    if (_owner.m_Overlay.Read32(addr) != 0)
                    {
                        _owner.AddPointer(addr);
                        _owner.ReadTextureAnimations(_owner.m_Overlay.ReadPointer(addr), a);
                    }
                }

                //If the first byte after the last name is in the scale value table, make room for a null byte and insert it
                int[] lastNamePos = new int[2];
                //Find the last material name
                for (int a = 0; a < numAreas; a++)
                {
                    for (int b = 0; b < _owner.m_TexAnims[a].Count; b++)
                    {
                        lastNamePos[0] = a;
                        lastNamePos[1] = b;
                    }
                }
                //Find the first scale values table
                int[] firstScalePos = new int[2];
                for (int a = 0; a < numAreas; a++)
                {
                    for (int b = 0; b < _owner.m_TexAnims[a].Count; b++)
                    {
                        firstScalePos[0] = a;
                        firstScalePos[1] = b;
                        break;
                    }
                }
                uint afterLastname;
                try{
                    afterLastname = (uint)(_owner.m_TexAnims[lastNamePos[0]][lastNamePos[1]].m_MatNameOffset +
                        _owner.m_TexAnims[lastNamePos[0]][lastNamePos[1]].m_MatName.Length);}
                catch{
                    break;//No texture animation data
                }
                uint firstScaleStart = _owner.m_TexAnims[firstScalePos[0]][firstScalePos[1]].m_ScaleTblOffset;
                if (afterLastname == firstScaleStart)
                {
                    AddSpace((uint)(_owner.m_TexAnims[lastNamePos[0]][lastNamePos[1]].m_MatNameOffset +
                    _owner.m_TexAnims[lastNamePos[0]][lastNamePos[1]].m_MatName.Length), 3);//Add 3 nulls just in case

                    _owner.m_Overlay.Write8((uint)(_owner.m_TexAnims[lastNamePos[0]][lastNamePos[1]].m_MatNameOffset +
                    _owner.m_TexAnims[lastNamePos[0]][lastNamePos[1]].m_MatName.Length), (byte)0);
                }
                else
                    nullFix = true;//Exit while loop

            }

            for (int i = 0; i < numAreas; i++)//For each area
            {
                List<string> names = new List<string>();
                List<uint> scale = new List<uint>();
                uint scaleStart = 0, scaleEnd = scaleStart;//Address, only 0 so it's not unassigned
                List<ushort> rot = new List<ushort>();
                uint rotStart = 0, rotEnd = rotStart;//Address
                List<uint> trans = new List<uint>();
                uint transStart = 0, transEnd = transStart;//Address
                List<ushort> scaleIdx = new List<ushort>();
                List<ushort> scaleSize = new List<ushort>();
                List<ushort> rotIdx = new List<ushort>();
                List<ushort> rotSize = new List<ushort>();
                List<ushort> transIdx = new List<ushort>();
                List<ushort> transSize = new List<ushort>();
                for (int j = 0; j < _owner.m_TexAnims[i].Count; j++)//For each texture animation in each area
                {
                    names.Add(_owner.m_TexAnims[i][j].m_MatName);

                    uint tmpSE = (uint)(_owner.m_TexAnims[i][j].m_ScaleTblOffset + _owner.m_TexAnims[i][j].m_ScaleTblSize * 4);
                    if (tmpSE > scaleEnd)
                        scaleEnd = tmpSE;

                    uint tmpRE = (uint)(_owner.m_TexAnims[i][j].m_RotTblOffset + _owner.m_TexAnims[i][j].m_RotTblSize * 2);
                    if (tmpRE > rotEnd)
                        rotEnd = tmpRE;

                    uint tmpTE = (uint)(_owner.m_TexAnims[i][j].m_TransTblOffset + _owner.m_TexAnims[i][j].m_TransTblSize * 4);
                    if (tmpTE > transEnd)
                        transEnd = tmpTE;

                    scaleIdx.Add(_owner.m_TexAnims[i][j].m_ScaleTblStart);
                    scaleSize.Add(_owner.m_TexAnims[i][j].m_ScaleTblSize);
                    rotIdx.Add(_owner.m_TexAnims[i][j].m_RotTblStart);
                    rotSize.Add(_owner.m_TexAnims[i][j].m_RotTblSize);
                    transIdx.Add(_owner.m_TexAnims[i][j].m_TransTblStart);
                    transSize.Add(_owner.m_TexAnims[i][j].m_TransTblSize);
                    
                }

                matNames.Add(names);
                scaleVals.Add(scale);
                rotVals.Add(rot);
                transVals.Add(trans);
                scaleStartIndices.Add(scaleIdx);
                scaleSizes.Add(scaleSize);
                rotStartIndices.Add(rotIdx);
                rotSizes.Add(rotSize);
                transStartIndices.Add(transIdx);
                transSizes.Add(transSize);

                if (_owner.m_TexAnims[i].Count == 0)//If there are no texture animations in area, move on
                    continue;

                scaleStart = _owner.m_TexAnims[i][0].m_ScaleTblAddr;//Address
                rotStart = _owner.m_TexAnims[i][0].m_RotTblAddr;//Address
                transStart = _owner.m_TexAnims[i][0].m_TransTblAddr;//Address

                for (uint addr = scaleStart; addr < scaleEnd; addr = addr + 4)
                {
                    scale.Add(_owner.m_Overlay.Read32(addr));
                }
                scaleTblLength.Add((int)((scaleEnd - scaleStart) / 4));//Number of values
                for (uint addr = rotStart; addr < rotEnd; addr = addr + 2)
                {
                    rot.Add(_owner.m_Overlay.Read16(addr));
                }
                rotTblLength.Add((int)((rotEnd - rotStart) / 2));//Number of values
                for (uint addr = transStart; addr < transEnd; addr = addr + 4)
                {
                    trans.Add(_owner.m_Overlay.Read32(addr));
                }
                transTblLength.Add((int)((transEnd - transStart) / 4));//Number of values

            }

            lbxArea.Items.Clear();
            lbxTexAnim.Items.Clear();
            for (int i = 0; i < _owner.m_NumAreas; i++)
            {
                lbxArea.Items.Add("" + i);
            }
            lbxArea.SelectedIndex = 0;//Make sure an area is selected
        }

        private void RemoveSpace(uint offset, int amount)//amount is pos
        {
            // move the data
            byte[] block = _owner.m_Overlay.ReadBlock((uint)(offset + amount), (uint)(_owner.m_Overlay.GetSize() - offset - amount));
            _owner.m_Overlay.WriteBlock(offset, block);
            _owner.m_Overlay.SetSize((uint)(_owner.m_Overlay.GetSize() - amount));

            // update the pointers
            for (int i = 0; i < _owner.m_PointerList.Count; i++)
            {
                LevelEditorForm.PointerReference ptrref = _owner.m_PointerList[i];
                if (ptrref.m_ReferenceAddr >= (offset + amount))
                    ptrref.m_ReferenceAddr = (uint)(ptrref.m_ReferenceAddr - amount);
                if (ptrref.m_PointerAddr >= (offset + amount))
                {
                    ptrref.m_PointerAddr = (uint)(ptrref.m_PointerAddr - amount);
                    _owner.m_Overlay.WritePointer(ptrref.m_ReferenceAddr, ptrref.m_PointerAddr);
                }
                _owner.m_PointerList[i] = ptrref;
            }

            // update the objects 'n' all
            UpdateObjectOffsets((uint)(offset + amount), -amount);
        }

        private void AddSpace(uint offset, int amount)//amount is pos
        {
            if ((_owner.m_Overlay.GetSize() + amount) > NitroROM.LEVEL_OVERLAY_SIZE)
                throw new Exception("This level has reached the level size limit. Cannot add more data.");

            // move the data
            byte[] block = _owner.m_Overlay.ReadBlock(offset, (uint)(_owner.m_Overlay.GetSize() - offset));
            _owner.m_Overlay.WriteBlock((uint)(offset + amount), block);

            // write zeroes in the newly created space
            for (int i = 0; i < amount; i++)
                _owner.m_Overlay.Write8((uint)(offset + i), 0);

            // update the pointers
            for (int i = 0; i < _owner.m_PointerList.Count; i++)
            {
                LevelEditorForm.PointerReference ptrref = _owner.m_PointerList[i];
                if (ptrref.m_ReferenceAddr >= offset)
                    ptrref.m_ReferenceAddr += (uint)amount;
                if (ptrref.m_PointerAddr >= offset)
                {
                    ptrref.m_PointerAddr += (uint)amount;
                    _owner.m_Overlay.WritePointer(ptrref.m_ReferenceAddr, ptrref.m_PointerAddr);
                }
                _owner.m_PointerList[i] = ptrref;
            }

            // update the objects 'n' all
            UpdateObjectOffsets(offset, amount);
        }

        public void UpdateObjectOffsets(uint start, int delta)
        {
            foreach (LevelObject obj in _owner.m_LevelObjects.Values)
                if (obj.m_Offset >= start) obj.m_Offset = (uint)(obj.m_Offset + delta);

            for (int a = 0; a < _owner.m_TexAnims.Length; a++)
            {
                foreach (LevelTexAnim anim in _owner.m_TexAnims[a])
                {
                    if (anim.m_Offset >= start) anim.m_Offset = (uint)(anim.m_Offset + delta);
                    if (anim.m_ScaleTblOffset >= start) anim.m_ScaleTblOffset = (uint)(anim.m_ScaleTblOffset + delta);
                    if (anim.m_RotTblOffset >= start) anim.m_RotTblOffset = (uint)(anim.m_RotTblOffset + delta);
                    if (anim.m_TransTblOffset >= start) anim.m_TransTblOffset = (uint)(anim.m_TransTblOffset + delta);
                    if (anim.m_MatNameOffset >= start) anim.m_MatNameOffset = (uint)(anim.m_MatNameOffset + delta);
                }
            }
        }

        private void txtScale_TextChanged(object sender, EventArgs e)
        {
            if (txtScale.Text != "" && lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxScale.SelectedIndex != -1)
                scaleVals[lbxArea.SelectedIndex][lbxScale.SelectedIndex] = 
                    (uint)(Convert.ToSingle(txtScale.Text) * 1000 / 12f * 20f);
        }

        private void txtRotation_TextChanged(object sender, EventArgs e)
        {
            if (txtRotation.Text != "" && lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxRotation.SelectedIndex != -1)
                rotVals[lbxArea.SelectedIndex][lbxRotation.SelectedIndex] = 
                    (ushort)(Convert.ToSingle(txtRotation.Text) * (1024f / 90f));
        }

        private void txtTranslation_TextChanged(object sender, EventArgs e)
        {
            if (txtTranslation.Text != "" && lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxTranslation.SelectedIndex != -1)
                transVals[lbxArea.SelectedIndex][lbxTranslation.SelectedIndex] = 
                    (uint)(Convert.ToSingle(txtTranslation.Text) * 1000f / 12 * 20);
        }

        private void btnSaveCurrent_Click(object sender, EventArgs e)
        {
            //Check for errors
            if (lbxArea.SelectedIndex == -1 || lbxTexAnim.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an area and texture animation first.");
                return;
            }
            for (int i = 0; i < numAreas; i++)
            {
                for (int j = 0; j < _owner.m_TexAnims[i].Count; j++)
                {
                    if (scaleVals[i].Count == 0 || rotVals[i].Count == 0 || transVals[i].Count == 0)
                    {
                        MessageBox.Show("You must have at least one scale, rotation and translation value in \n"
                            + "texture animation " + i);
                        return;
                    }
                }
            }
            //Write the new values, adding and removing space is necessary

            for (int i = 0; i < numAreas; i++)//Each area
            {
                //Scale values
                if (scaleVals[i].Count > scaleTblLength[i])
                {
                    AddSpace((uint)(_owner.m_TexAnims[i][0].m_ScaleTblAddr + scaleTblLength[i] * 4),
                        (int)(scaleVals[i].Count * 4 - scaleTblLength[i] * 4));
                }
                if (scaleVals[i].Count < scaleTblLength[i])
                {
                    RemoveSpace((uint)(_owner.m_TexAnims[i][0].m_ScaleTblAddr + (scaleTblLength[i] * 4) - ((scaleTblLength[i] - scaleVals[i].Count) * 4)),
                        (int)(scaleTblLength[i] * 4 - scaleVals[i].Count * 4));
                }
                for (int a = 0; a < scaleVals[i].Count; a++)
                {
                    _owner.m_Overlay.Write32(
                        (uint)(_owner.m_TexAnims[i][0].m_ScaleTblAddr + (4 * a)),
                        (uint)(scaleVals[i][a]));
                }

                //Rotation values
                if (rotVals[i].Count > rotTblLength[i])
                {
                    AddSpace((uint)(_owner.m_TexAnims[i][0].m_RotTblAddr + rotTblLength[i] * 2),
                        (int)(rotVals[i].Count * 2 - rotTblLength[i] * 2));
                }
                if (rotVals[i].Count < rotTblLength[i])
                {
                    RemoveSpace((uint)(_owner.m_TexAnims[i][0].m_RotTblAddr + (rotTblLength[i] * 2) - ((rotTblLength[i] - rotVals[i].Count) * 2)),
                        (int)(rotTblLength[i] * 2 - rotVals[i].Count * 2));
                }
                for (int a = 0; a < rotVals[i].Count; a++)
                {
                    _owner.m_Overlay.Write16(
                        (ushort)(_owner.m_TexAnims[i][0].m_RotTblAddr + (2 * a)),
                        (ushort)(rotVals[i][a]));
                }

                //Translation values
                if (transVals[i].Count > transTblLength[i])
                {
                    AddSpace((uint)(_owner.m_TexAnims[i][0].m_TransTblAddr + transTblLength[i] * 4),
                        (int)(transVals[i].Count * 4 - transTblLength[i] * 4));
                }
                if (transVals[i].Count < transTblLength[i])
                {
                    RemoveSpace((uint)(_owner.m_TexAnims[i][0].m_TransTblAddr + (transTblLength[i] * 4) - ((transTblLength[i] - transVals[i].Count) * 4)),
                        (int)(transTblLength[i] * 4 - transVals[i].Count * 4));
                }
                for (int a = 0; a < transVals[i].Count; a++)
                {
                    _owner.m_Overlay.Write32(
                        (uint)(_owner.m_TexAnims[i][0].m_TransTblAddr + (4 * a)),
                        (uint)(transVals[i][a]));
                }

                for (int j = 0; j < _owner.m_TexAnims[i].Count; j++)//Each texture animation within current area
                {
                    //Material names
                    uint m_MatNameOffset = _owner.m_TexAnims[i][j].m_MatNameOffset;
                    string newMatName = matNames[i][j];
                    int newNameLength = newMatName.Length;
                    int oldNameLength = _owner.m_TexAnims[i][j].m_MatName.Length;
                    int delta = newNameLength - oldNameLength;
                    uint deltaPos;
                    if (delta > 0) deltaPos = (uint)delta;
                    else deltaPos = (uint)(delta * (-1));

                    if (newNameLength < oldNameLength)
                        RemoveSpace((uint)(m_MatNameOffset + newNameLength), -delta);
                    else if (newNameLength > oldNameLength)
                        AddSpace((uint)(m_MatNameOffset + oldNameLength), delta);

                    for (int a = 0; a < newNameLength; a++)
                    {
                        _owner.m_Overlay.Write8((uint)(m_MatNameOffset + a), (byte)newMatName.ToCharArray()[a]);
                    }

                    //Scale table start indices and sizes
                    _owner.m_TexAnims[i][j].m_ScaleTblSize = scaleSizes[i][j];
                    _owner.m_Overlay.Write16((uint)(_owner.m_TexAnims[i][j].m_Offset + 0x0C), _owner.m_TexAnims[i][j].m_ScaleTblSize);
                    _owner.m_TexAnims[i][j].m_ScaleTblStart = scaleStartIndices[i][j];
                    _owner.m_Overlay.Write16((uint)(_owner.m_TexAnims[i][j].m_Offset + 0x0E), _owner.m_TexAnims[i][j].m_ScaleTblStart);

                    //Rotation table start indices and sizes
                    _owner.m_TexAnims[i][j].m_RotTblSize = rotSizes[i][j];
                    _owner.m_Overlay.Write16((uint)(_owner.m_TexAnims[i][j].m_Offset + 0x10), _owner.m_TexAnims[i][j].m_RotTblSize);
                    _owner.m_TexAnims[i][j].m_RotTblStart = rotStartIndices[i][j];
                    _owner.m_Overlay.Write16((uint)(_owner.m_TexAnims[i][j].m_Offset + 0x12), _owner.m_TexAnims[i][j].m_RotTblStart);

                    //Translation table start indices and sizes
                    _owner.m_TexAnims[i][j].m_TransTblSize = transSizes[i][j];
                    _owner.m_Overlay.Write16((uint)(_owner.m_TexAnims[i][j].m_Offset + 0x14), _owner.m_TexAnims[i][j].m_TransTblSize);
                    _owner.m_TexAnims[i][j].m_TransTblStart = transStartIndices[i][j];
                    _owner.m_Overlay.Write16((uint)(_owner.m_TexAnims[i][j].m_Offset + 0x16), _owner.m_TexAnims[i][j].m_TransTblStart);
                }
            }
            if (cbSaveOvl.Checked)
                _owner.m_Overlay.SaveChanges();

            reloadData();
        }

        private void txtMaterialName_TextChanged(object sender, EventArgs e)
        {
            if (txtMaterialName.Text != "")
                matNames[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex] = txtMaterialName.Text;
        }

        private void btnRemScale_Click(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxScale.SelectedIndex != -1)
            {
                scaleVals[lbxArea.SelectedIndex].RemoveAt(lbxScale.SelectedIndex);
                refreshLbx();
            }
        }

        private void btnAddScale_Click(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1)
            {
                if (lbxScale.SelectedIndex == -1)
                    scaleVals[lbxArea.SelectedIndex].Add((uint)0);
                else
                    scaleVals[lbxArea.SelectedIndex].Insert(lbxScale.SelectedIndex + 1, (uint)0);
                refreshLbx();
            }
        }

        private void btnRemRot_Click(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxRotation.SelectedIndex != -1)
            {
                rotVals[lbxArea.SelectedIndex].RemoveAt(lbxRotation.SelectedIndex);
                refreshLbx();
            }
        }

        private void btnAddRot_Click(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1)
            {
                if (lbxRotation.SelectedIndex == -1)
                    rotVals[lbxArea.SelectedIndex].Add((ushort)0);
                else
                    rotVals[lbxArea.SelectedIndex].Insert(lbxRotation.SelectedIndex + 1, (ushort)0);
                refreshLbx();
            }
        }

        private void btnRemTrans_Click(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxTranslation.SelectedIndex != -1)
            {
                transVals[lbxArea.SelectedIndex].RemoveAt(lbxTranslation.SelectedIndex);
                refreshLbx();
            }
        }

        private void btnAddTrans_Click(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1)
            {
                if (lbxTranslation.SelectedIndex == -1)
                    transVals[lbxArea.SelectedIndex].Add((uint)0);
                else
                    transVals[lbxArea.SelectedIndex].Insert(lbxTranslation.SelectedIndex + 1, (uint)0);
                refreshLbx();
            }
        }

        private void txtScaleStart_TextChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && txtScaleStart.Text != "")
            {
                scaleStartIndices[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex] = Convert.ToUInt16(txtScaleStart.Text);
            }
        }

        private void txtRotStart_TextChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && txtRotStart.Text != "")
            {
                rotStartIndices[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex] = Convert.ToUInt16(txtRotStart.Text);
            }
        }

        private void txtTransStart_TextChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && txtTransStart.Text != "")
            {
                transStartIndices[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex] = Convert.ToUInt16(txtTransStart.Text);
            }
        }

        private void txtScaleSize_TextChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && txtScaleSize.Text != "")
            {
                scaleSizes[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex] = Convert.ToUInt16(txtScaleSize.Text);
            }
        }

        private void txtRotSize_TextChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && txtRotSize.Text != "")
            {
                rotSizes[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex] = Convert.ToUInt16(txtRotSize.Text);
            }
        }

        private void txtTransSize_TextChanged(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && txtTransSize.Text != "")
            {
                transSizes[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex] = Convert.ToUInt16(txtTransSize.Text);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && _owner.m_TexAnims[lbxArea.SelectedIndex].Count != 0)
            {
                if (lbxTexAnim.Items.Count > 1)
                {
                    //Reduce animation count
                    _owner.m_Overlay.Write32((uint)(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_TexAnimHeaderOffset + 0x10), (uint)(lbxTexAnim.Items.Count - 1));

                    uint remAnimHdr = _owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_Offset;
                    uint remAnimName = _owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_MatNameOffset;
                    int remAnimNameLen = (_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_MatName.Length + 1);//+1 for null at end

                    //Remove from list of texture animations - important
                    _owner.m_TexAnims[lbxArea.SelectedIndex].RemoveAt(lbxTexAnim.SelectedIndex);
                    //Remove animation data and material name offsets from list of pointers - important
                    List<int> rmAt = new List<int>();
                    for (int i = 0; i < _owner.m_PointerList.Count; i++)
                    {
                        LevelEditorForm.PointerReference ptrref = _owner.m_PointerList[i];
                        if (ptrref.m_PointerAddr == remAnimName)
                        {
                            rmAt.Add(i);
                        }
                        if (ptrref.m_PointerAddr == remAnimHdr)
                        {
                            ptrref.m_PointerAddr += 28;
                            _owner.m_PointerList.RemoveAt(i);
                            _owner.m_PointerList.Insert(i, ptrref);
                        }
                    }
                    for (int i = 0; i < rmAt.Count; i++)
                    {
                        _owner.m_PointerList.RemoveAt(rmAt[i]);
                        for (int j = 0; j < rmAt.Count; j++)
                            rmAt[j]--;
                    }
                    //Remove animation header
                    RemoveSpace(remAnimHdr, 28);
                }
                else if (lbxTexAnim.Items.Count == 1)
                {
                    //Remove texture animation header
                    RemoveSpace(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_TexAnimHeaderOffset, 24);
                    //Remove animation data header
                    RemoveSpace(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_Offset, 28);
                    //Remove animation data
                    //Crashes if you try to remove up to end of data
                    if (numAreas != 1)
                        RemoveSpace(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_MatNameOffset,
                            (int)((_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_TransTblAddr +
                            (transTblLength[lbxArea.SelectedIndex] * 4)) - _owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.SelectedIndex].m_MatNameOffset));
                    //Set current area's texture animation address to NULL
                    uint objlistptr = _owner.m_Overlay.ReadPointer(0x70);
                    uint addr = (uint)(objlistptr + (lbxArea.SelectedIndex * 12));//Each level data header is 12 bytes - get the address of current one
                    //Texture animation addresses have an offset of 4 bytes within each level data header
                    addr += 4;
                    _owner.m_Overlay.Write32(addr, 0);//Make it NULL
                }

                reloadData();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (lbxArea.SelectedIndex != -1 && lbxTexAnim.SelectedIndex != -1 && lbxTexAnim.Items.Count > 0)//Area already has at least one texture animation
            {
                //Had to change patch to pad with 3 nulls instead of 1 as scale started straight after end of string (including null) 
                //and this caused problems in the levels that needed the patch (reloadData())
                
                //Make room for new material name
                string matName = "MaterialName";
                uint newMatOffset = (uint)(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.Items.Count - 1].m_MatNameOffset + _owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.Items.Count - 1].m_MatName.Length + 1);
                AddSpace(newMatOffset, (int)(matName.ToCharArray().Length + 1));
                
                //Make room for new header
                AddSpace((uint)(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.Items.Count - 1].m_Offset + 28), 28);
                uint newHdr = (uint)(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.Items.Count - 1].m_Offset + 28);
                //Increase number of animations
                _owner.m_Overlay.Write32((uint)(_owner.m_TexAnims[lbxArea.SelectedIndex][0].m_TexAnimHeaderOffset + 0x10), (uint)(lbxTexAnim.Items.Count + 1));
                //Write material name offset
                newMatOffset = (uint)(_owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.Items.Count - 1].m_MatNameOffset +
                    _owner.m_TexAnims[lbxArea.SelectedIndex][lbxTexAnim.Items.Count - 1].m_MatName.Length + 1);
                _owner.m_Overlay.WritePointer((uint)(newHdr + 0x04), newMatOffset);
                //Write material name and write scale, rotation and tranlsation start index and size
                for (int a = 0; a < matName.ToCharArray().Length; a++)
                {
                    _owner.m_Overlay.Write8((uint)(newMatOffset + a),
                        (byte)matName.ToCharArray()[a]);
                }
                _owner.m_Overlay.Write32((uint)(newHdr + 0x00), (uint)(65535));//Unknown
                _owner.m_Overlay.Write32((uint)(newHdr + 0x08), (uint)(1));//Unknown
                _owner.m_Overlay.Write16((uint)(newHdr + 0x0C), (ushort)(1));
                _owner.m_Overlay.Write16((uint)(newHdr + 0x0E), (ushort)(0));
                _owner.m_Overlay.Write16((uint)(newHdr + 0x10), (ushort)(1));
                _owner.m_Overlay.Write16((uint)(newHdr + 0x12), (ushort)(0));
                _owner.m_Overlay.Write16((uint)(newHdr + 0x14), (ushort)(1));
                _owner.m_Overlay.Write16((uint)(newHdr + 0x16), (ushort)(0));

                reloadData();
            }
            else if (lbxArea.SelectedIndex != -1 && lbxTexAnim.Items.Count == 0)//Area has no texture animations
            {
                //Get position to insert data, before next area's header if it exists else end of overlay data
                uint freeSpace = 0;
                if (numAreas == 1)
                    freeSpace = _owner.m_Overlay.GetSize();
                else
                {
                    for (int i = 0; i < numAreas; i++)
                    {
                        if (_owner.m_TexAnims[i].Count != 0)
                        {
                            freeSpace = _owner.m_TexAnims[i][0].m_TexAnimHeaderOffset;
                            break;
                        }
                    }
                }
                //Space for texture animation header, animation data header and material name, scale, rotation and translation value
                AddSpace(freeSpace, (24 + 28 + 16 + 4 + 2 + 4));
                //16 - MaterialName Length is 12 + 3 for null values at end
                //Set current area's texture animation address to newly created space
                uint objlistptr = _owner.m_Overlay.ReadPointer(0x70);
                uint addr = (uint)(objlistptr + (lbxArea.SelectedIndex * 12));//Each level data header is 12 bytes - get the address of current one
                addr += 4;//Texture animation addresses have an offset of 4 bytes within each level data header
                _owner.m_Overlay.WritePointer(addr, freeSpace);
                //Write texture animation header
                _owner.m_Overlay.Write32(freeSpace, 100);//Number of frames?
                _owner.m_Overlay.WritePointer((uint)(freeSpace + 0x04), (uint)(freeSpace + 24 + 28 + 16));//Address of scale values table
                _owner.m_Overlay.WritePointer((uint)(freeSpace + 0x08), (uint)(freeSpace + 24 + 28 + 16 + 4));//Address of rotation values table
                _owner.m_Overlay.WritePointer((uint)(freeSpace + 0x0C), (uint)(freeSpace + 24 + 28 + 16 + 4 + 2));//Address of translation values table
                _owner.m_Overlay.Write32((uint)(freeSpace + 0x10), (uint)(1));//Number of animations
                _owner.m_Overlay.WritePointer((uint)(freeSpace + 0x14), (uint)(freeSpace + 24));//Animation header address
                //Write animation data header
                _owner.m_Overlay.Write32((uint)(freeSpace + 24 + 0x00), (uint)(65535));//Unknown
                _owner.m_Overlay.WritePointer((uint)(freeSpace + 24 + 0x04), (uint)(freeSpace + 24 + 28));//Material name address
                _owner.m_Overlay.Write32((uint)(freeSpace + 24 + 0x08), (uint)(1));//Unknown
                _owner.m_Overlay.Write16((uint)(freeSpace + 24 + 0x0C), (ushort)(1));//Amount of scale values
                _owner.m_Overlay.Write16((uint)(freeSpace + 24 + 0x0E), (ushort)(0));//Scale values start index
                _owner.m_Overlay.Write16((uint)(freeSpace + 24 + 0x10), (ushort)(1));//Amount of rotation values
                _owner.m_Overlay.Write16((uint)(freeSpace + 24 + 0x12), (ushort)(0));//Rotation values start index
                _owner.m_Overlay.Write16((uint)(freeSpace + 24 + 0x14), (ushort)(1));//Amount of translation values
                _owner.m_Overlay.Write16((uint)(freeSpace + 24 + 0x16), (ushort)(0));//Translation values start index
                //Write animation data
                string newMatName = "MaterialName";
                for (int i = 0; i < newMatName.Length; i++)
                {
                    _owner.m_Overlay.Write8((uint)(freeSpace + 24 + 28 + i), (byte)(newMatName.ToCharArray()[i]));
                }
                _owner.m_Overlay.Write32((uint)(freeSpace + 24 + 28 + 16), 4096);//First scale value
                _owner.m_Overlay.Write16((uint)(freeSpace + 24 + 28 + 16 + 4), (ushort)(1024));//First rotation value
                _owner.m_Overlay.Write32((uint)(freeSpace + 24 + 28 + 16 + 4 + 2), 0);//First translation value

                reloadData();
            }
        }

    }
}
