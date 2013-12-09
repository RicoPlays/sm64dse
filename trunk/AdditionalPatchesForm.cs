using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace SM64DSe
{
    public partial class AdditionalPatchesForm : Form
    {
        List<Patch> m_Patches = new List<Patch>();

        int editingIndex = -1;// Used to differentiate between editing and creating a new patch
        
        public AdditionalPatchesForm()
        {
            InitializeComponent();
            //Patch skipIntro = new Patch("Skip Introduction", new uint[] { 0x13df4 }, new uint[] { 0x13cd0 }, new uint[] { 0x13d48 }, new uint[] { 0x13d48 },
            //    new Tuple<int, uint>[] { new Tuple<int, uint>(8, 0x88) }, new Tuple<int, uint>[] { new Tuple<int, uint>(8, 0x88) },
            //    new Tuple<int, uint>[] { new Tuple<int, uint>(8, 0x88) }, new Tuple<int, uint>[] { new Tuple<int, uint>(8, 0x88) }, 
            //    new uint[] { 0x08 }, new uint[] { 0x08 }, new uint[] { 0x08 }, new uint[] { 0x08 });
            //m_Patches.Add(skipIntro);
            //Patch.PatchToXML(m_Patches);

            m_Patches = Patch.XMLToPatch();

            fillTable();
        }

        private void fillTable()
        {
            // Start writing to file
            Program.m_ROM.BeginRW();

            int numPatches = m_Patches.Count;

            gridPatches.ColumnCount = 6;
            gridPatches.Columns[0].HeaderText = "Name / Description";
            gridPatches.Columns[1].HeaderText = "Applied";
            gridPatches.Columns[2].HeaderText = "EUR Compatible";
            gridPatches.Columns[3].HeaderText = "US v1 Compatible";
            gridPatches.Columns[4].HeaderText = "US v2 Compatible";
            gridPatches.Columns[5].HeaderText = "JAP Compatible";
            gridPatches.RowCount = numPatches;

            for (int i = 0; i < m_Patches.Count; i++)
            {
                gridPatches.Rows[i].Cells[0].Value = m_Patches[i].m_PatchName;
                gridPatches.Rows[i].Cells[1].Value = m_Patches[i].CheckIsApplied(Program.m_ROM);
                gridPatches.Rows[i].Cells[2].Value = m_Patches[i].m_VersionSupport[0];
                gridPatches.Rows[i].Cells[3].Value = m_Patches[i].m_VersionSupport[1];
                gridPatches.Rows[i].Cells[4].Value = m_Patches[i].m_VersionSupport[2];
                gridPatches.Rows[i].Cells[5].Value = m_Patches[i].m_VersionSupport[3];
            }

            Program.m_ROM.EndRW();
        }

        private void AdditionalPatchesForm_Load(object sender, EventArgs e)
        {

        }

        private void btnApplyPatch_Click(object sender, EventArgs e)
        {
            if (gridPatches.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a patch to apply.");
                return;
            }

            Program.m_ROM.BeginRW();

            try
            {
                m_Patches[gridPatches.SelectedRows[0].Index].ApplyPatch(Program.m_ROM);
                MessageBox.Show("Success");
                RefreshRow(gridPatches.SelectedRows[0].Index);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong: \n\n" + ex.Message);
            }

            Program.m_ROM.EndRW();
        }

        private void btnUndoPatch_Click(object sender, EventArgs e)
        {
            if (gridPatches.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a patch to undo.");
                return;
            }

            Program.m_ROM.BeginRW();

            try
            {
                m_Patches[gridPatches.SelectedRows[0].Index].RemovePatch(Program.m_ROM);
                MessageBox.Show("Success");
                RefreshRow(gridPatches.SelectedRows[0].Index);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong: \n\n" + ex.Message + "\n" + ex.Data + "\n" + ex.StackTrace + "\n" + ex.Source);
            }

            Program.m_ROM.EndRW();
        }

        private void RefreshRow(int index)
        {
            Program.m_ROM.BeginRW();

            gridPatches.Rows[index].Cells[0].Value = m_Patches[index].m_PatchName;
            gridPatches.Rows[index].Cells[1].Value = m_Patches[index].CheckIsApplied(Program.m_ROM);
            gridPatches.Rows[index].Cells[2].Value = m_Patches[index].m_VersionSupport[0];
            gridPatches.Rows[index].Cells[3].Value = m_Patches[index].m_VersionSupport[1];
            gridPatches.Rows[index].Cells[4].Value = m_Patches[index].m_VersionSupport[2];
            gridPatches.Rows[index].Cells[5].Value = m_Patches[index].m_VersionSupport[3];

            Program.m_ROM.EndRW();
        }

        private void btnEditPatch_Click(object sender, EventArgs e)
        {
            if (gridPatches.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a patch to edit.");
                return;
            }

            editingIndex = gridPatches.SelectedRows[0].Index;
            clearTextBoxes();

            txtPatchName.Text = m_Patches[gridPatches.SelectedRows[0].Index].m_PatchName;
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrEUR.Count(); i++)
            {
                txtEURPatchAddresses.Text += String.Format("{0:X8}", m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrEUR[i]);
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrEUR.Count() - 1)
                    txtEURPatchAddresses.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrUSv1.Count(); i++)
            {
                txtUSv1PatchAddresses.Text += String.Format("{0:X8}", m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrUSv1[i]);
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrUSv1.Count() - 1)
                    txtUSv1PatchAddresses.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrUSv2.Count(); i++)
            {
                txtUSv2PatchAddresses.Text += String.Format("{0:X8}", m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrUSv2[i]);
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrUSv2.Count() - 1)
                    txtUSv2PatchAddresses.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrJAP.Count(); i++)
            {
                txtJAPPatchAddresses.Text += String.Format("{0:X8}", m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrJAP[i]);
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchAddrJAP.Count() - 1)
                    txtJAPPatchAddresses.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataEUR.Count(); i++)
            {
                txtEURPatchData.Text += String.Format("{0:X8}", m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataEUR[i].Item2);
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataEUR.Count() - 1)
                    txtEURPatchData.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv1.Count(); i++)
            {
                txtUSv1PatchData.Text += String.Format("{0:X8}", m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv1[i].Item2);
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv1.Count() - 1)
                    txtUSv1PatchData.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv2.Count(); i++)
            {
                txtUSv2PatchData.Text += String.Format("{0:X8}", m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv2[i].Item2);
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv2.Count() - 1)
                    txtUSv2PatchData.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataJAP.Count(); i++)
            {
                txtJAPPatchData.Text += String.Format("{0:X8}", m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataJAP[i].Item2);
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataJAP.Count() - 1)
                    txtJAPPatchData.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataEUR.Count(); i++)
            {
                txtEURDataSizes.Text += m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataEUR[i].Item1;
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataEUR.Count() - 1)
                    txtEURDataSizes.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv1.Count(); i++)
            {
                txtUSv1DataSizes.Text += m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv1[i].Item1;
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv1.Count() - 1)
                    txtUSv1DataSizes.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv2.Count(); i++)
            {
                txtUSv2DataSizes.Text += m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv2[i].Item1;
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataUSv2.Count() - 1)
                    txtUSv2DataSizes.Text += ",";
            }
            for (int i = 0; i < m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataJAP.Count(); i++)
            {
                txtJAPDataSizes.Text += m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataJAP[i].Item1;
                if (i != m_Patches[gridPatches.SelectedRows[0].Index].m_PatchDataJAP.Count() - 1)
                    txtJAPDataSizes.Text += ",";
            }
            chkApplyToFile.Checked = m_Patches[gridPatches.SelectedRows[0].Index].m_FileToPatch != null;
            if (chkApplyToFile.Checked) txtApplyToFile.Text = m_Patches[gridPatches.SelectedRows[0].Index].m_FileToPatch;
        }

        private void clearTextBoxes()
        {
            txtPatchName.Text = "";
            txtEURPatchAddresses.Text = "";
            txtUSv1PatchAddresses.Text = "";
            txtUSv2PatchAddresses.Text = "";
            txtJAPPatchAddresses.Text = "";
            txtEURPatchData.Text = "";
            txtUSv1PatchData.Text = "";
            txtUSv2PatchData.Text = "";
            txtJAPPatchData.Text = "";
            txtEURDataSizes.Text = "";
            txtUSv1DataSizes.Text = "";
            txtUSv2DataSizes.Text = "";
            txtJAPDataSizes.Text = "";
            txtApplyToFile.Text = "";
            chkApplyToFile.Checked = false;
        }

        private void btnDeletePatch_Click(object sender, EventArgs e)
        {
            if (gridPatches.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a patch to delete.");
                return;
            }

            DialogResult dlgeResult = MessageBox.Show("Are you sure you want to delete this patch?", "Confirm Deletion", MessageBoxButtons.YesNo);
            if (dlgeResult == DialogResult.No)
                return;

            // Remove current patch from list of patches
            m_Patches.RemoveAt(gridPatches.SelectedRows[0].Index);
            // Write updated list of patches to XML file
            Patch.PatchToXML(m_Patches);

            Patch.XMLToPatch();
            fillTable();
        }

        private void btnNewPatch_Click(object sender, EventArgs e)
        {
            editingIndex = -1;
            clearTextBoxes();
        }

        private void btnSavePatch_Click(object sender, EventArgs e)
        {
            String name = txtPatchName.Text;
            List<uint> EURAddresses = new List<uint>(), USv1Addresses = new List<uint>(), USv2Addresses = new List<uint>(), JAPAddresses = new List<uint>();
            List<int> EURDataSizes = new List<int>(), USv1DataSizes = new List<int>(), USv2DataSizes = new List<int>(), JAPDataSizes = new List<int>();
            List<uint> EURDataValues = new List<uint>(), USv1DataValues = new List<uint>(), USv2DataValues = new List<uint>(), JAPDataValues = new List<uint>();
            List<uint> EURRestore = new List<uint>(), USv1Restore = new List<uint>(), USv2Restore = new List<uint>(), JAPRestore = new List<uint>();
            bool isFileToPatch = chkApplyToFile.Checked;
            String fileToPatch = (isFileToPatch) ? txtApplyToFile.Text : null;
            
            for (int i = 0; i < txtEURPatchAddresses.Text.Split(',').Length; i++)
                if (txtEURPatchAddresses.Text != "") EURAddresses.Add((uint)int.Parse(txtEURPatchAddresses.Text.Split(',')[i], System.Globalization.NumberStyles.HexNumber));
            for (int i = 0; i < txtUSv1PatchAddresses.Text.Split(',').Length; i++)
                if (txtUSv1PatchAddresses.Text != "") USv1Addresses.Add((uint)int.Parse(txtUSv1PatchAddresses.Text.Split(',')[i], System.Globalization.NumberStyles.HexNumber));
            for (int i = 0; i < txtUSv2PatchAddresses.Text.Split(',').Length; i++)
                if (txtUSv2PatchAddresses.Text != "") USv2Addresses.Add((uint)int.Parse(txtUSv2PatchAddresses.Text.Split(',')[i], System.Globalization.NumberStyles.HexNumber));
            for (int i = 0; i < txtJAPPatchAddresses.Text.Split(',').Length; i++)
                if (txtJAPPatchAddresses.Text != "") JAPAddresses.Add((uint)int.Parse(txtJAPPatchAddresses.Text.Split(',')[i], System.Globalization.NumberStyles.HexNumber));
            for (int i = 0; i < txtEURPatchData.Text.Split(',').Length; i++)
                if (txtEURPatchData.Text != "") EURDataValues.Add((uint)int.Parse(txtEURPatchData.Text.Split(',')[i], System.Globalization.NumberStyles.HexNumber));
            for (int i = 0; i < txtUSv1PatchData.Text.Split(',').Length; i++)
                if (txtUSv1PatchData.Text != "") USv1DataValues.Add((uint)int.Parse(txtUSv1PatchData.Text.Split(',')[i], System.Globalization.NumberStyles.HexNumber));
            for (int i = 0; i < txtUSv2PatchData.Text.Split(',').Length; i++)
                if (txtUSv2PatchData.Text != "") USv2DataValues.Add((uint)int.Parse(txtUSv2PatchData.Text.Split(',')[i], System.Globalization.NumberStyles.HexNumber));
            for (int i = 0; i < txtJAPPatchData.Text.Split(',').Length; i++)
                if (txtJAPPatchData.Text != "") JAPDataValues.Add((uint)int.Parse(txtJAPPatchData.Text.Split(',')[i], System.Globalization.NumberStyles.HexNumber));
            for (int i = 0; i < txtEURDataSizes.Text.Split(',').Length; i++)
                if (txtEURDataSizes.Text != "") EURDataSizes.Add(int.Parse(txtEURDataSizes.Text.Split(',')[i]));
            for (int i = 0; i < txtUSv1DataSizes.Text.Split(',').Length; i++)
                if (txtUSv1DataSizes.Text != "") USv1DataSizes.Add(int.Parse(txtUSv1DataSizes.Text.Split(',')[i]));
            for (int i = 0; i < txtUSv2DataSizes.Text.Split(',').Length; i++)
                if (txtUSv2DataSizes.Text != "") USv2DataSizes.Add(int.Parse(txtUSv2DataSizes.Text.Split(',')[i]));
            for (int i = 0; i < txtJAPDataSizes.Text.Split(',').Length; i++)
                if (txtJAPDataSizes.Text != "") JAPDataSizes.Add(int.Parse(txtJAPDataSizes.Text.Split(',')[i]));

            // Generate the restore data by reading the current data at the addresses to be patched
            Program.m_ROM.BeginRW();
            NitroFile file = (isFileToPatch) ? Program.m_ROM.GetFileFromName(fileToPatch) : null;
            for (int i = 0; i < EURAddresses.Count; i++)
            {
                switch (EURDataSizes[i])
                {
                    case 8:
                        if (!isFileToPatch) EURRestore.Add(Program.m_ROM.Read8(EURAddresses[i]));
                        else EURRestore.Add(file.Read8(EURAddresses[i]));
                        break;
                    case 16:
                        if (!isFileToPatch) EURRestore.Add(Program.m_ROM.Read16(EURAddresses[i]));
                        else EURRestore.Add(file.Read16(EURAddresses[i]));
                        break;
                    case 32:
                        if (!isFileToPatch) EURRestore.Add(Program.m_ROM.Read32(EURAddresses[i]));
                        else EURRestore.Add(file.Read32(EURAddresses[i]));
                        break;
                }
            }
            for (int i = 0; i < USv1Addresses.Count; i++)
            {
                switch (USv1DataSizes[i])
                {
                    case 8:
                        if (!isFileToPatch) USv1Restore.Add(Program.m_ROM.Read8(USv1Addresses[i]));
                        else USv1Restore.Add(file.Read8(USv1Addresses[i]));
                        break;
                    case 16:
                        if (!isFileToPatch) USv1Restore.Add(Program.m_ROM.Read16(USv1Addresses[i]));
                        else USv1Restore.Add(file.Read16(USv1Addresses[i]));
                        break;
                    case 32:
                        if (!isFileToPatch) USv1Restore.Add(Program.m_ROM.Read32(USv1Addresses[i]));
                        else USv1Restore.Add(file.Read32(USv1Addresses[i]));
                        break;
                }
            }
            for (int i = 0; i < USv2Addresses.Count; i++)
            {
                switch (USv2DataSizes[i])
                {
                    case 8:
                        if (!isFileToPatch) USv2Restore.Add(Program.m_ROM.Read8(USv2Addresses[i]));
                        else USv2Restore.Add(file.Read8(USv2Addresses[i]));
                        break;
                    case 16:
                        if (!isFileToPatch) USv2Restore.Add(Program.m_ROM.Read16(USv2Addresses[i]));
                        else USv2Restore.Add(file.Read16(USv2Addresses[i]));
                        break;
                    case 32:
                        if (!isFileToPatch) USv2Restore.Add(Program.m_ROM.Read32(USv2Addresses[i]));
                        else USv2Restore.Add(file.Read32(USv2Addresses[i]));
                        break;
                }
            }
            for (int i = 0; i < JAPAddresses.Count; i++)
            {
                switch (JAPDataSizes[i])
                {
                    case 8:
                        if (!isFileToPatch) JAPRestore.Add(Program.m_ROM.Read8(JAPAddresses[i]));
                        else JAPRestore.Add(file.Read8(JAPAddresses[i]));
                        break;
                    case 16:
                        if (!isFileToPatch) JAPRestore.Add(Program.m_ROM.Read16(JAPAddresses[i]));
                        else JAPRestore.Add(file.Read16(JAPAddresses[i]));
                        break;
                    case 32:
                        if (!isFileToPatch) JAPRestore.Add(Program.m_ROM.Read32(JAPAddresses[i]));
                        else JAPRestore.Add(file.Read32(JAPAddresses[i]));
                        break;
                }
            }
            Program.m_ROM.EndRW();

            Tuple<int, uint>[] EURSizeValue = new Tuple<int, uint>[EURDataSizes.Count];
            for (int i = 0; i < EURDataSizes.Count; i++)
                EURSizeValue[i] = new Tuple<int, uint>(EURDataSizes.ElementAt(i), EURDataValues.ElementAt(i));
            Tuple<int, uint>[] USv1SizeValue = new Tuple<int, uint>[USv1DataSizes.Count];
            for (int i = 0; i < USv1DataSizes.Count; i++)
                USv1SizeValue[i] = new Tuple<int, uint>(USv1DataSizes.ElementAt(i), USv1DataValues.ElementAt(i));
            Tuple<int, uint>[] USv2SizeValue = new Tuple<int, uint>[USv2DataSizes.Count];
            for (int i = 0; i < USv2DataSizes.Count; i++)
                USv2SizeValue[i] = new Tuple<int, uint>(USv2DataSizes.ElementAt(i), USv2DataValues.ElementAt(i));
            Tuple<int, uint>[] JAPSizeValue = new Tuple<int, uint>[JAPDataSizes.Count];
            for (int i = 0; i < JAPDataSizes.Count; i++)
                JAPSizeValue[i] = new Tuple<int, uint>(JAPDataSizes.ElementAt(i), JAPDataValues.ElementAt(i));

            // Finally, add the parsed details to the list of patches
            if (editingIndex == -1)
                m_Patches.Add(new Patch(name, EURAddresses.ToArray(), USv1Addresses.ToArray(), USv2Addresses.ToArray(),
                JAPAddresses.ToArray(), EURSizeValue, USv1SizeValue, USv2SizeValue, JAPSizeValue, EURRestore.ToArray(),
                USv1Restore.ToArray(), USv2Restore.ToArray(), JAPRestore.ToArray(), fileToPatch));
            else
                m_Patches[editingIndex] = new Patch(name, EURAddresses.ToArray(), USv1Addresses.ToArray(), USv2Addresses.ToArray(),
                JAPAddresses.ToArray(), EURSizeValue, USv1SizeValue, USv2SizeValue, JAPSizeValue, EURRestore.ToArray(),
                USv1Restore.ToArray(), USv2Restore.ToArray(), JAPRestore.ToArray(), fileToPatch);

            // Write the updated patches to XML
            Patch.PatchToXML(m_Patches);

            // Reload the list of patches
            m_Patches = Patch.XMLToPatch();
            fillTable();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (var form = new ROMFileSelect("Please select a file to open."))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtApplyToFile.Text = form.m_SelectedFile;
                }
            }
        }
    }

    public class Patch
    {
        public String m_PatchName;
        public uint[] m_PatchAddrEUR;// Stores list of addresses to patch
        public uint[] m_PatchAddrUSv1;
        public uint[] m_PatchAddrUSv2;
        public uint[] m_PatchAddrJAP;
        public Tuple<int, uint>[] m_PatchDataEUR;// Stores size in bits and value of data to write
        public Tuple<int, uint>[] m_PatchDataUSv1;
        public Tuple<int, uint>[] m_PatchDataUSv2;
        public Tuple<int, uint>[] m_PatchDataJAP;
        public uint[] m_RestoreDataEUR;// Stores original values so patch can be reversed
        public uint[] m_RestoreDataUSv1;
        public uint[] m_RestoreDataUSv2;
        public uint[] m_RestoreDataJAP;
        public String m_FileToPatch;// Optionally you can specify the patch be applied to a particular file (addresses relative to start)
        private bool m_IsFileToPatch;

        bool m_IsApplied;
        public bool[] m_VersionSupport = new bool[] { false, false, false, false };// Whether each region is supported, EUR, USv1, USv2, JAP

        public Patch(String name, uint[] patchAddrEUR, uint[] patchAddrUSv1, uint[] patchAddrUSv2, uint[] patchAddrJAP, Tuple<int, uint>[] patchDataEUR,
            Tuple<int, uint>[] patchDataUSv1, Tuple<int, uint>[] patchDataUSv2, Tuple<int, uint>[] patchDataJAP, uint[] restoreEUR,
            uint[] restoreUSv1, uint[] restoreUSv2, uint[] restoreJAP, String fileToPatch = null)
        {
            m_PatchName = name;
            m_PatchAddrEUR = patchAddrEUR;
            m_PatchAddrUSv1 = patchAddrUSv1;
            m_PatchAddrUSv2 = patchAddrUSv2;
            m_PatchAddrJAP = patchAddrJAP;
            m_PatchDataEUR = patchDataEUR;
            m_PatchDataUSv1 = patchDataUSv1;
            m_PatchDataUSv2 = patchDataUSv2;
            m_PatchDataJAP = patchDataJAP;
            m_RestoreDataEUR = restoreEUR;
            m_RestoreDataUSv1 = restoreUSv1;
            m_RestoreDataUSv2 = restoreUSv2;
            m_RestoreDataJAP = restoreJAP;
            m_IsFileToPatch = (!(fileToPatch == null || fileToPatch.Equals("")));
            m_FileToPatch = (m_IsFileToPatch) ? fileToPatch : null;

            m_VersionSupport = new bool[] { (patchAddrEUR.Length > 0), (patchAddrUSv1.Length > 0), (patchAddrUSv2.Length > 0), (patchAddrJAP.Length > 0) };
        }

        public Patch() { }

        public bool CheckIsApplied(NitroROM rom)
        {
            bool applied = false;

            int testSize = 0;
            Tuple<int, uint> dataTest = new Tuple<int,uint> (0, 0);
            uint testAddr = 0;
            if (m_PatchAddrEUR.Count() != 0)
            {
                testSize = m_PatchDataEUR.ElementAt(0).Item1;
                dataTest = m_PatchDataEUR.ElementAt(0);
                testAddr = m_PatchAddrEUR.ElementAt(0);
            }
            else if (m_PatchAddrUSv1.Count() != 0)
            {
                testSize = m_PatchDataUSv1.ElementAt(0).Item1;
                dataTest = m_PatchDataUSv1.ElementAt(0);
                testAddr = m_PatchAddrUSv1.ElementAt(0);
            }
            else if (m_PatchAddrUSv2.Count() != 0)
            {
                testSize = m_PatchDataUSv2.ElementAt(0).Item1;
                dataTest = m_PatchDataUSv2.ElementAt(0);
                testAddr = m_PatchAddrUSv2.ElementAt(0);
            }
            else if (m_PatchAddrJAP.Count() != 0)
            {
                testSize = m_PatchDataJAP.ElementAt(0).Item1;
                dataTest = m_PatchDataJAP.ElementAt(0);
                testAddr = m_PatchAddrJAP.ElementAt(0);
            }

            NitroFile fileToPatch = (m_IsFileToPatch) ? Program.m_ROM.GetFileFromName(m_FileToPatch) : null;

            if (testSize != 0)
            {
                switch (testSize)
                {
                        // If the data found at the first patch address equals the data to patch, it's already been applied
                    case 8:
                        if (!m_IsFileToPatch && rom.Read8(testAddr) == (byte)dataTest.Item2)
                            applied = true;
                        else if (m_IsFileToPatch && fileToPatch.Read8(testAddr) == (byte)dataTest.Item2)
                            applied = true;
                        break;
                    case 16:
                        if (!m_IsFileToPatch && rom.Read16(testAddr) == (ushort)dataTest.Item2)
                            applied = true;
                        else if (m_IsFileToPatch && fileToPatch.Read16(testAddr) == (ushort)dataTest.Item2)
                            applied = true;
                        break;
                    case 32:
                        if (!m_IsFileToPatch && rom.Read32(testAddr) == (uint)dataTest.Item2)
                            applied = true;
                        else if (m_IsFileToPatch && fileToPatch.Read32(testAddr) == (uint)dataTest.Item2)
                            applied = true;
                        break;
                }
            }

            m_IsApplied = applied;
            return applied;
        }

        public void ApplyPatch(NitroROM rom)
        {
            uint[] addresses = new uint[1];
            Tuple<int, uint>[] sizesValues = new Tuple<int,uint>[1];

            switch (rom.m_Version)
            {
                case NitroROM.Version.EUR:
                    addresses = m_PatchAddrEUR;
                    sizesValues = m_PatchDataEUR;
                    break;
                case NitroROM.Version.USA_v1:
                    addresses = m_PatchAddrUSv1;
                    sizesValues = m_PatchDataUSv2;
                    break;
                case NitroROM.Version.USA_v2:
                    addresses = m_PatchAddrUSv2;
                    sizesValues = m_PatchDataUSv2;
                    break;
                case NitroROM.Version.JAP:
                    addresses = m_PatchAddrJAP;
                    sizesValues = m_PatchDataJAP;
                    break;
            }

            NitroFile fileToPatch = (m_IsFileToPatch) ? Program.m_ROM.GetFileFromName(m_FileToPatch) : null;

            // Write the specified values of the specified sizes at the specified addresses
            for (int i = 0; i < addresses.Length; i++)
            {
                switch (sizesValues.ElementAt(i).Item1)
                {
                    case 8:
                        if (!m_IsFileToPatch) rom.Write8(addresses[i], (byte)sizesValues.ElementAt(i).Item2);
                        else if (m_IsFileToPatch) fileToPatch.Write8(addresses[i], (byte)sizesValues.ElementAt(i).Item2);
                        break;
                    case 16:
                        if (!m_IsFileToPatch) rom.Write16(addresses[i], (ushort)sizesValues.ElementAt(i).Item2);
                        else if (m_IsFileToPatch) fileToPatch.Write16(addresses[i], (ushort)sizesValues.ElementAt(i).Item2);
                        break;
                    case 32:
                        if (!m_IsFileToPatch) rom.Write32(addresses[i], (uint)sizesValues.ElementAt(i).Item2);
                        else if (m_IsFileToPatch) fileToPatch.Write32(addresses[i], (uint)sizesValues.ElementAt(i).Item2);
                        break;
                }
            }
            if (fileToPatch != null)
                fileToPatch.SaveChanges();
        }

        public void RemovePatch(NitroROM rom)
        {
            uint[] addresses = new uint[1];
            Tuple<int, uint>[] sizesValues = new Tuple<int, uint>[1];
            uint[] restoreData = new uint[1];

            switch (rom.m_Version)
            {
                case NitroROM.Version.EUR:
                    addresses = m_PatchAddrEUR;
                    sizesValues = m_PatchDataEUR;
                    restoreData = m_RestoreDataEUR;
                    break;
                case NitroROM.Version.USA_v1:
                    addresses = m_PatchAddrUSv1;
                    sizesValues = m_PatchDataUSv2;
                    restoreData = m_RestoreDataUSv1;
                    break;
                case NitroROM.Version.USA_v2:
                    addresses = m_PatchAddrUSv2;
                    sizesValues = m_PatchDataUSv2;
                    restoreData = m_RestoreDataUSv2;
                    break;
                case NitroROM.Version.JAP:
                    addresses = m_PatchAddrJAP;
                    sizesValues = m_PatchDataJAP;
                    restoreData = m_RestoreDataJAP;
                    break;
            }

            NitroFile fileToPatch = (m_IsFileToPatch) ? Program.m_ROM.GetFileFromName(m_FileToPatch) : null;

            // Write the specified values of the specified sizes at the specified addresses
            for (int i = 0; i < addresses.Length; i++)
            {
                switch (sizesValues.ElementAt(i).Item1)
                {
                    case 8:
                        if (!m_IsFileToPatch) rom.Write8(addresses[i], (byte)restoreData[i]);
                        else if (m_IsFileToPatch) fileToPatch.Write8(addresses[i], (byte)restoreData[i]);
                        break;
                    case 16:
                        if (!m_IsFileToPatch) rom.Write16(addresses[i], (ushort)restoreData[i]);
                        else if (m_IsFileToPatch) fileToPatch.Write16(addresses[i], (ushort)restoreData[i]);
                        break;
                    case 32:
                        if (!m_IsFileToPatch) rom.Write32(addresses[i], (uint)restoreData[i]);
                        else if (m_IsFileToPatch) fileToPatch.Write32(addresses[i], (uint)restoreData[i]);
                        break;
                }
            }
            if (fileToPatch != null)
                fileToPatch.SaveChanges();
        }

        public static void PatchToXML(List<Patch> patches)
        {
            // Write all patches to XML
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create(Path.Combine(Application.StartupPath, "AdditionalPatches.xml"), settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Patches");

                foreach (Patch patch in patches)
                {
                    writer.WriteStartElement("Patch");

                    writer.WriteElementString("PatchName", patch.m_PatchName);

                    if (patch.m_IsFileToPatch)
                        writer.WriteElementString("FileToPatch", patch.m_FileToPatch);

                    if (patch.m_PatchAddrEUR.Count() != 0)
                    {
                        writer.WriteStartElement("EURAddresses");
                        foreach (uint addr in patch.m_PatchAddrEUR)
                            writer.WriteElementString("Address", addr.ToString());
                        writer.WriteEndElement();
                    }
                    if (patch.m_PatchAddrUSv1.Count() != 0)
                    {
                        writer.WriteStartElement("USv1Addresses");
                        foreach (uint addr in patch.m_PatchAddrUSv1)
                            writer.WriteElementString("Address", addr.ToString());
                        writer.WriteEndElement();
                    }
                    if (patch.m_PatchAddrUSv2.Count() != 0)
                    {
                        writer.WriteStartElement("USv2Addresses");
                        foreach (uint addr in patch.m_PatchAddrUSv2)
                            writer.WriteElementString("Address", addr.ToString());
                        writer.WriteEndElement();
                    }
                    if (patch.m_PatchAddrJAP.Count() != 0)
                    {
                        writer.WriteStartElement("JAPAddresses");
                        foreach (uint addr in patch.m_PatchAddrJAP)
                            writer.WriteElementString("Address", addr.ToString());
                        writer.WriteEndElement();
                    }

                    if (patch.m_PatchDataEUR.Count() != 0)
                    {
                        writer.WriteStartElement("EURPatchData");
                        foreach (Tuple<int, uint> data in patch.m_PatchDataEUR)
                        {
                            writer.WriteElementString("Size", data.Item1.ToString());
                            writer.WriteElementString("Value", data.Item2.ToString());
                        }
                        writer.WriteEndElement();
                    }
                    if (patch.m_PatchDataUSv1.Count() != 0)
                    {
                        writer.WriteStartElement("USv1PatchData");
                        foreach (Tuple<int, uint> data in patch.m_PatchDataUSv1)
                        {
                            writer.WriteElementString("Size", data.Item1.ToString());
                            writer.WriteElementString("Value", data.Item2.ToString());
                        }
                        writer.WriteEndElement();
                    }
                    if (patch.m_PatchDataUSv2.Count() != 0)
                    {
                        writer.WriteStartElement("USv2PatchData");
                        foreach (Tuple<int, uint> data in patch.m_PatchDataUSv2)
                        {
                            writer.WriteElementString("Size", data.Item1.ToString());
                            writer.WriteElementString("Value", data.Item2.ToString());
                        }
                        writer.WriteEndElement();
                    }
                    if (patch.m_PatchDataJAP.Count() != 0)
                    {
                        writer.WriteStartElement("JAPPatchData");
                        foreach (Tuple<int, uint> data in patch.m_PatchDataJAP)
                        {
                            writer.WriteElementString("Size", data.Item1.ToString());
                            writer.WriteElementString("Value", data.Item2.ToString());
                        }
                        writer.WriteEndElement();
                    }

                    if (patch.m_RestoreDataEUR.Count() != 0)
                    {
                        writer.WriteStartElement("EURRestoreData");
                        foreach (uint data in patch.m_RestoreDataEUR)
                            writer.WriteElementString("Value", data.ToString());// Size already written
                        writer.WriteEndElement();
                    }
                    if (patch.m_RestoreDataUSv1.Count() != 0)
                    {
                        writer.WriteStartElement("USv1RestoreData");
                        foreach (uint data in patch.m_RestoreDataUSv1)
                            writer.WriteElementString("Value", data.ToString());// Size already written
                        writer.WriteEndElement();
                    }
                    if (patch.m_RestoreDataUSv2.Count() != 0)
                    {
                        writer.WriteStartElement("USv2RestoreData");
                        foreach (uint data in patch.m_RestoreDataUSv2)
                            writer.WriteElementString("Value", data.ToString());// Size already written
                        writer.WriteEndElement();
                    }
                    if (patch.m_RestoreDataJAP.Count() != 0)
                    {
                        writer.WriteStartElement("JAPRestoreData");
                        foreach (uint data in patch.m_RestoreDataJAP)
                            writer.WriteElementString("Value", data.ToString());// Size already written
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static List<Patch> XMLToPatch()
        {
            List<Patch> patches = new List<Patch>();
            String name = "";
            List<uint> EURAddresses = new List<uint>(), USv1Addresses = new List<uint>(), USv2Addresses = new List<uint>(), JAPAddresses = new List<uint>();
            List<int> EURDataSizes = new List<int>(), USv1DataSizes = new List<int>(), USv2DataSizes = new List<int>(), JAPDataSizes = new List<int>();
            List<uint> EURDataValues = new List<uint>(), USv1DataValues = new List<uint>(), USv2DataValues = new List<uint>(), JAPDataValues = new List<uint>();
            List<uint> EURRestore = new List<uint>(), USv1Restore = new List<uint>(), USv2Restore = new List<uint>(), JAPRestore = new List<uint>();
            String fileToPatch = "";

            // Create an XML reader for this file.
            using (XmlReader reader = XmlReader.Create(Path.Combine(Application.StartupPath, "AdditionalPatches.xml")))
            {
                reader.MoveToContent();

                while (reader.Read())
                {
                    if (reader.NodeType.Equals(XmlNodeType.Element))
                    {
                        switch (reader.LocalName)
                        {
                            case "Patch":
                                break;
                            case "PatchName":
                                reader.MoveToContent();
                                name = reader.ReadElementContentAsString();
                                break;
                            case "FileToPatch":
                                reader.MoveToContent();
                                fileToPatch = reader.ReadElementContentAsString();
                                break;
                            case "EURAddresses":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Address"))
                                    {
                                        EURAddresses.Add((uint)reader.ReadElementContentAsLong());
                                    }
                                    else
                                        break;
                                }
                                break;
                            case "USv1Addresses":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Address"))
                                    {
                                        USv1Addresses.Add((uint)reader.ReadElementContentAsLong());
                                    }
                                    else
                                        break;
                                }
                                break;
                            case "USv2Addresses":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Address"))
                                    {
                                        USv2Addresses.Add((uint)reader.ReadElementContentAsLong());
                                    }
                                    else
                                        break;
                                }
                                break;
                            case "JAPAddresses":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Address"))
                                    {
                                        JAPAddresses.Add((uint)reader.ReadElementContentAsLong());
                                    }
                                    else
                                        break;
                                }
                                break;
                            case "EURPatchData":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Size"))
                                    {
                                        EURDataSizes.Add(reader.ReadElementContentAsInt());
                                    }
                                    else if (reader.LocalName.Equals("Value"))
                                        EURDataValues.Add((uint)reader.ReadElementContentAsLong());
                                    else
                                        break;
                                }
                                break;
                            case "USv1PatchData":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Size"))
                                        USv1DataSizes.Add(reader.ReadElementContentAsInt());
                                    else if (reader.LocalName.Equals("Value"))
                                        USv1DataValues.Add((uint)reader.ReadElementContentAsLong());
                                    else
                                        break;
                                }
                                break;
                            case "USv2PatchData":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Size"))
                                        USv2DataSizes.Add(reader.ReadElementContentAsInt());
                                    else if (reader.LocalName.Equals("Value"))
                                        USv2DataValues.Add((uint)reader.ReadElementContentAsLong());
                                    else
                                        break;
                                }
                                break;
                            case "JAPPatchData":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Size"))
                                        JAPDataSizes.Add(reader.ReadElementContentAsInt());
                                    else if (reader.LocalName.Equals("Value"))
                                        JAPDataValues.Add((uint)reader.ReadElementContentAsLong());
                                    else
                                        break;
                                }
                                break;
                            case "EURRestoreData":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Value"))
                                        EURRestore.Add((uint)reader.ReadElementContentAsLong());
                                    else
                                        break;
                                }
                                break;
                            case "USv1RestoreData":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Value"))
                                        USv1Restore.Add((uint)reader.ReadElementContentAsLong());
                                    else
                                        break;
                                }
                                break;
                            case "USv2RestoreData":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Value"))
                                        USv2Restore.Add((uint)reader.ReadElementContentAsLong());
                                    else
                                        break;
                                }
                                break;
                            case "JAPRestoreData":
                                while (reader.Read())
                                {
                                    reader.MoveToContent();
                                    if (reader.LocalName.Equals("Value"))
                                        JAPRestore.Add((uint)reader.ReadElementContentAsLong());
                                    else
                                        break;
                                }
                                break;

                        }
                    }
                    else if (reader.NodeType.Equals(XmlNodeType.EndElement))
                    {
                        if (reader.LocalName.Equals("Patch"))
                        {
                            Tuple<int, uint>[] EURSizeValue = new Tuple<int, uint>[EURDataSizes.Count];
                            for (int i = 0; i < EURDataSizes.Count; i++)
                                EURSizeValue[i] = new Tuple<int,uint>(EURDataSizes.ElementAt(i), EURDataValues.ElementAt(i));
                            Tuple<int, uint>[] USv1SizeValue = new Tuple<int, uint>[USv1DataSizes.Count];
                            for (int i = 0; i < USv1DataSizes.Count; i++)
                                USv1SizeValue[i] = new Tuple<int, uint>(USv1DataSizes.ElementAt(i), USv1DataValues.ElementAt(i));
                            Tuple<int, uint>[] USv2SizeValue = new Tuple<int, uint>[USv2DataSizes.Count];
                            for (int i = 0; i < USv2DataSizes.Count; i++)
                                USv2SizeValue[i] = new Tuple<int, uint>(USv2DataSizes.ElementAt(i), USv2DataValues.ElementAt(i));
                            Tuple<int, uint>[] JAPSizeValue = new Tuple<int, uint>[JAPDataSizes.Count];
                            for (int i = 0; i < JAPDataSizes.Count; i++)
                                JAPSizeValue[i] = new Tuple<int, uint>(JAPDataSizes.ElementAt(i), JAPDataValues.ElementAt(i));
                            patches.Add(new Patch(name, EURAddresses.ToArray(), USv1Addresses.ToArray(), USv2Addresses.ToArray(), 
                                JAPAddresses.ToArray(), EURSizeValue, USv1SizeValue, USv2SizeValue, JAPSizeValue, EURRestore.ToArray(), 
                                USv1Restore.ToArray(), USv2Restore.ToArray(), JAPRestore.ToArray(), fileToPatch));
                            // Reset lists for next patch
                            EURAddresses.Clear(); USv1Addresses.Clear(); USv2Addresses.Clear(); JAPAddresses.Clear();
                            EURDataSizes.Clear(); USv1DataSizes.Clear(); USv2DataSizes.Clear(); JAPDataSizes.Clear();
                            EURDataValues.Clear(); USv1DataValues.Clear(); USv2DataValues.Clear(); JAPDataValues.Clear();
                            EURRestore.Clear(); USv1Restore.Clear(); USv2Restore.Clear(); JAPRestore.Clear();
                            fileToPatch = "";
                        }
                    }
                }
            }

            return patches;
        }
    }
}
