namespace SM64DSe
{
    partial class AdditionalPatchesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gridPatches = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.btnApplyPatch = new System.Windows.Forms.Button();
            this.btnUndoPatch = new System.Windows.Forms.Button();
            this.txtPatchName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnNewPatch = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtEURPatchAddresses = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtEURPatchData = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtEURDataSizes = new System.Windows.Forms.TextBox();
            this.txtUSv1PatchAddresses = new System.Windows.Forms.TextBox();
            this.txtUSv2PatchAddresses = new System.Windows.Forms.TextBox();
            this.txtJAPPatchAddresses = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtUSv1PatchData = new System.Windows.Forms.TextBox();
            this.txtUSv2PatchData = new System.Windows.Forms.TextBox();
            this.txtJAPPatchData = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.txtUSv1DataSizes = new System.Windows.Forms.TextBox();
            this.txtUSv2DataSizes = new System.Windows.Forms.TextBox();
            this.txtJAPDataSizes = new System.Windows.Forms.TextBox();
            this.btnSavePatch = new System.Windows.Forms.Button();
            this.btnEditPatch = new System.Windows.Forms.Button();
            this.btnDeletePatch = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.txtApplyToFile = new System.Windows.Forms.TextBox();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.chkApplyToFile = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // gridPatches
            // 
            this.gridPatches.AllowUserToAddRows = false;
            this.gridPatches.AllowUserToDeleteRows = false;
            this.gridPatches.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridPatches.Location = new System.Drawing.Point(12, 66);
            this.gridPatches.MultiSelect = false;
            this.gridPatches.Name = "gridPatches";
            this.gridPatches.Size = new System.Drawing.Size(561, 150);
            this.gridPatches.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Available Patches:";
            // 
            // btnApplyPatch
            // 
            this.btnApplyPatch.Location = new System.Drawing.Point(12, 223);
            this.btnApplyPatch.Name = "btnApplyPatch";
            this.btnApplyPatch.Size = new System.Drawing.Size(96, 23);
            this.btnApplyPatch.TabIndex = 2;
            this.btnApplyPatch.Text = "Apply Patch";
            this.btnApplyPatch.UseVisualStyleBackColor = true;
            this.btnApplyPatch.Click += new System.EventHandler(this.btnApplyPatch_Click);
            // 
            // btnUndoPatch
            // 
            this.btnUndoPatch.Location = new System.Drawing.Point(114, 223);
            this.btnUndoPatch.Name = "btnUndoPatch";
            this.btnUndoPatch.Size = new System.Drawing.Size(96, 23);
            this.btnUndoPatch.TabIndex = 3;
            this.btnUndoPatch.Text = "Undo Patch";
            this.btnUndoPatch.UseVisualStyleBackColor = true;
            this.btnUndoPatch.Click += new System.EventHandler(this.btnUndoPatch_Click);
            // 
            // txtPatchName
            // 
            this.txtPatchName.Location = new System.Drawing.Point(114, 282);
            this.txtPatchName.Name = "txtPatchName";
            this.txtPatchName.Size = new System.Drawing.Size(286, 20);
            this.txtPatchName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 285);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Name/Description:";
            // 
            // btnNewPatch
            // 
            this.btnNewPatch.Location = new System.Drawing.Point(420, 223);
            this.btnNewPatch.Name = "btnNewPatch";
            this.btnNewPatch.Size = new System.Drawing.Size(96, 23);
            this.btnNewPatch.TabIndex = 6;
            this.btnNewPatch.Text = "New Patch";
            this.btnNewPatch.UseVisualStyleBackColor = true;
            this.btnNewPatch.Click += new System.EventHandler(this.btnNewPatch_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 320);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(325, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Addresses to patch in Hexadecimal Format (separate with commas):";
            // 
            // txtEURPatchAddresses
            // 
            this.txtEURPatchAddresses.Location = new System.Drawing.Point(114, 336);
            this.txtEURPatchAddresses.Name = "txtEURPatchAddresses";
            this.txtEURPatchAddresses.Size = new System.Drawing.Size(286, 20);
            this.txtEURPatchAddresses.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 460);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(299, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Data to patch in Hexadecimal Format (separate with commas):";
            // 
            // txtEURPatchData
            // 
            this.txtEURPatchData.Location = new System.Drawing.Point(114, 494);
            this.txtEURPatchData.Name = "txtEURPatchData";
            this.txtEURPatchData.Size = new System.Drawing.Size(285, 20);
            this.txtEURPatchData.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(432, 460);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(186, 31);
            this.label6.TabIndex = 11;
            this.label6.Text = "Size of data to patch - byte 8, ushort 16, uint 32 (separate with commas):";
            // 
            // txtEURDataSizes
            // 
            this.txtEURDataSizes.Location = new System.Drawing.Point(435, 494);
            this.txtEURDataSizes.Name = "txtEURDataSizes";
            this.txtEURDataSizes.Size = new System.Drawing.Size(183, 20);
            this.txtEURDataSizes.TabIndex = 12;
            // 
            // txtUSv1PatchAddresses
            // 
            this.txtUSv1PatchAddresses.Location = new System.Drawing.Point(114, 362);
            this.txtUSv1PatchAddresses.Name = "txtUSv1PatchAddresses";
            this.txtUSv1PatchAddresses.Size = new System.Drawing.Size(286, 20);
            this.txtUSv1PatchAddresses.TabIndex = 13;
            // 
            // txtUSv2PatchAddresses
            // 
            this.txtUSv2PatchAddresses.Location = new System.Drawing.Point(114, 388);
            this.txtUSv2PatchAddresses.Name = "txtUSv2PatchAddresses";
            this.txtUSv2PatchAddresses.Size = new System.Drawing.Size(286, 20);
            this.txtUSv2PatchAddresses.TabIndex = 14;
            // 
            // txtJAPPatchAddresses
            // 
            this.txtJAPPatchAddresses.Location = new System.Drawing.Point(114, 414);
            this.txtJAPPatchAddresses.Name = "txtJAPPatchAddresses";
            this.txtJAPPatchAddresses.Size = new System.Drawing.Size(286, 20);
            this.txtJAPPatchAddresses.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 339);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "EUR ROMs:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 365);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "US v1 ROMs:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 391);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(73, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "US v2 ROMs:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 417);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(62, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "JAP ROMs:";
            // 
            // txtUSv1PatchData
            // 
            this.txtUSv1PatchData.Location = new System.Drawing.Point(114, 520);
            this.txtUSv1PatchData.Name = "txtUSv1PatchData";
            this.txtUSv1PatchData.Size = new System.Drawing.Size(285, 20);
            this.txtUSv1PatchData.TabIndex = 20;
            // 
            // txtUSv2PatchData
            // 
            this.txtUSv2PatchData.Location = new System.Drawing.Point(114, 546);
            this.txtUSv2PatchData.Name = "txtUSv2PatchData";
            this.txtUSv2PatchData.Size = new System.Drawing.Size(285, 20);
            this.txtUSv2PatchData.TabIndex = 21;
            // 
            // txtJAPPatchData
            // 
            this.txtJAPPatchData.Location = new System.Drawing.Point(114, 572);
            this.txtJAPPatchData.Name = "txtJAPPatchData";
            this.txtJAPPatchData.Size = new System.Drawing.Size(285, 20);
            this.txtJAPPatchData.TabIndex = 22;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 575);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(62, 13);
            this.label10.TabIndex = 26;
            this.label10.Text = "JAP ROMs:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(13, 549);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(73, 13);
            this.label11.TabIndex = 25;
            this.label11.Text = "US v2 ROMs:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(13, 523);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(73, 13);
            this.label12.TabIndex = 24;
            this.label12.Text = "US v1 ROMs:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(13, 497);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(66, 13);
            this.label13.TabIndex = 23;
            this.label13.Text = "EUR ROMs:";
            // 
            // txtUSv1DataSizes
            // 
            this.txtUSv1DataSizes.Location = new System.Drawing.Point(435, 520);
            this.txtUSv1DataSizes.Name = "txtUSv1DataSizes";
            this.txtUSv1DataSizes.Size = new System.Drawing.Size(183, 20);
            this.txtUSv1DataSizes.TabIndex = 27;
            // 
            // txtUSv2DataSizes
            // 
            this.txtUSv2DataSizes.Location = new System.Drawing.Point(435, 546);
            this.txtUSv2DataSizes.Name = "txtUSv2DataSizes";
            this.txtUSv2DataSizes.Size = new System.Drawing.Size(183, 20);
            this.txtUSv2DataSizes.TabIndex = 28;
            // 
            // txtJAPDataSizes
            // 
            this.txtJAPDataSizes.Location = new System.Drawing.Point(435, 572);
            this.txtJAPDataSizes.Name = "txtJAPDataSizes";
            this.txtJAPDataSizes.Size = new System.Drawing.Size(183, 20);
            this.txtJAPDataSizes.TabIndex = 29;
            // 
            // btnSavePatch
            // 
            this.btnSavePatch.Location = new System.Drawing.Point(114, 610);
            this.btnSavePatch.Name = "btnSavePatch";
            this.btnSavePatch.Size = new System.Drawing.Size(103, 34);
            this.btnSavePatch.TabIndex = 30;
            this.btnSavePatch.Text = "Save Patch";
            this.btnSavePatch.UseVisualStyleBackColor = true;
            this.btnSavePatch.Click += new System.EventHandler(this.btnSavePatch_Click);
            // 
            // btnEditPatch
            // 
            this.btnEditPatch.Location = new System.Drawing.Point(216, 223);
            this.btnEditPatch.Name = "btnEditPatch";
            this.btnEditPatch.Size = new System.Drawing.Size(96, 23);
            this.btnEditPatch.TabIndex = 31;
            this.btnEditPatch.Text = "Edit Patch";
            this.btnEditPatch.UseVisualStyleBackColor = true;
            this.btnEditPatch.Click += new System.EventHandler(this.btnEditPatch_Click);
            // 
            // btnDeletePatch
            // 
            this.btnDeletePatch.Location = new System.Drawing.Point(318, 223);
            this.btnDeletePatch.Name = "btnDeletePatch";
            this.btnDeletePatch.Size = new System.Drawing.Size(96, 23);
            this.btnDeletePatch.TabIndex = 32;
            this.btnDeletePatch.Text = "Delete Patch";
            this.btnDeletePatch.UseVisualStyleBackColor = true;
            this.btnDeletePatch.Click += new System.EventHandler(this.btnDeletePatch_Click);
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(232, 610);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(298, 34);
            this.label14.TabIndex = 33;
            this.label14.Text = "Note: When editing a patch, make sure it\'s not applied to ensure restore data is " +
                "correctly generated.";
            // 
            // txtApplyToFile
            // 
            this.txtApplyToFile.Location = new System.Drawing.Point(420, 336);
            this.txtApplyToFile.Name = "txtApplyToFile";
            this.txtApplyToFile.Size = new System.Drawing.Size(168, 20);
            this.txtApplyToFile.TabIndex = 35;
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(594, 334);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(30, 23);
            this.btnSelectFile.TabIndex = 36;
            this.btnSelectFile.Text = "...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // chkApplyToFile
            // 
            this.chkApplyToFile.AutoSize = true;
            this.chkApplyToFile.Location = new System.Drawing.Point(420, 319);
            this.chkApplyToFile.Name = "chkApplyToFile";
            this.chkApplyToFile.Size = new System.Drawing.Size(90, 17);
            this.chkApplyToFile.TabIndex = 37;
            this.chkApplyToFile.Text = "Apply To File:";
            this.chkApplyToFile.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(417, 359);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(201, 34);
            this.label15.TabIndex = 38;
            this.label15.Text = "Note: Addresses should be relative to the start of the file selected.";
            // 
            // AdditionalPatchesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(636, 660);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.chkApplyToFile);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.txtApplyToFile);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.btnDeletePatch);
            this.Controls.Add(this.btnEditPatch);
            this.Controls.Add(this.btnSavePatch);
            this.Controls.Add(this.txtJAPDataSizes);
            this.Controls.Add(this.txtUSv2DataSizes);
            this.Controls.Add(this.txtUSv1DataSizes);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtJAPPatchData);
            this.Controls.Add(this.txtUSv2PatchData);
            this.Controls.Add(this.txtUSv1PatchData);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtJAPPatchAddresses);
            this.Controls.Add(this.txtUSv2PatchAddresses);
            this.Controls.Add(this.txtUSv1PatchAddresses);
            this.Controls.Add(this.txtEURDataSizes);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnNewPatch);
            this.Controls.Add(this.txtEURPatchData);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtPatchName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtEURPatchAddresses);
            this.Controls.Add(this.btnUndoPatch);
            this.Controls.Add(this.btnApplyPatch);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gridPatches);
            this.Name = "AdditionalPatchesForm";
            this.Text = "Additional Patches";
            this.Load += new System.EventHandler(this.AdditionalPatchesForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gridPatches;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnApplyPatch;
        private System.Windows.Forms.Button btnUndoPatch;
        private System.Windows.Forms.TextBox txtPatchName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnNewPatch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtEURPatchAddresses;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtEURPatchData;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtEURDataSizes;
        private System.Windows.Forms.TextBox txtUSv1PatchAddresses;
        private System.Windows.Forms.TextBox txtUSv2PatchAddresses;
        private System.Windows.Forms.TextBox txtJAPPatchAddresses;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtUSv1PatchData;
        private System.Windows.Forms.TextBox txtUSv2PatchData;
        private System.Windows.Forms.TextBox txtJAPPatchData;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtUSv1DataSizes;
        private System.Windows.Forms.TextBox txtUSv2DataSizes;
        private System.Windows.Forms.TextBox txtJAPDataSizes;
        private System.Windows.Forms.Button btnSavePatch;
        private System.Windows.Forms.Button btnEditPatch;
        private System.Windows.Forms.Button btnDeletePatch;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtApplyToFile;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.CheckBox chkApplyToFile;
        private System.Windows.Forms.Label label15;
    }
}