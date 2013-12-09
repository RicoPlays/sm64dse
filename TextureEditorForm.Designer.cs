namespace SM64DSe
{
    partial class TextureEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureEditorForm));
            this.label1 = new System.Windows.Forms.Label();
            this.lbxTextures = new System.Windows.Forms.ListBox();
            this.pbxTexture = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnReplaceSelected = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnExportAll = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.chkNewPalette = new System.Windows.Forms.CheckBox();
            this.lblPalette = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnLoadBTP = new System.Windows.Forms.ToolStripButton();
            this.lbxPalettes = new System.Windows.Forms.ListBox();
            //((System.ComponentModel.ISupportInitialize)(this.pbxTexture)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Texture:";
            // 
            // lbxTextures
            // 
            this.lbxTextures.FormattingEnabled = true;
            this.lbxTextures.Location = new System.Drawing.Point(16, 43);
            this.lbxTextures.Name = "lbxTextures";
            this.lbxTextures.Size = new System.Drawing.Size(200, 173);
            this.lbxTextures.TabIndex = 1;
            this.lbxTextures.SelectedIndexChanged += new System.EventHandler(this.lbxTextures_SelectedIndexChanged);
            // 
            // pbxTexture
            // 
            this.pbxTexture.Location = new System.Drawing.Point(428, 42);
            this.pbxTexture.Name = "pbxTexture";
            this.pbxTexture.Size = new System.Drawing.Size(200, 173);
            this.pbxTexture.TabIndex = 2;
            this.pbxTexture.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(425, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Preview:";
            // 
            // btnReplaceSelected
            // 
            this.btnReplaceSelected.Location = new System.Drawing.Point(16, 240);
            this.btnReplaceSelected.Name = "btnReplaceSelected";
            this.btnReplaceSelected.Size = new System.Drawing.Size(113, 23);
            this.btnReplaceSelected.TabIndex = 4;
            this.btnReplaceSelected.Text = "Replace Selected";
            this.btnReplaceSelected.UseVisualStyleBackColor = true;
            this.btnReplaceSelected.Click += new System.EventHandler(this.btnReplaceSelected_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(287, 240);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnExportAll
            // 
            this.btnExportAll.Location = new System.Drawing.Point(368, 240);
            this.btnExportAll.Name = "btnExportAll";
            this.btnExportAll.Size = new System.Drawing.Size(75, 23);
            this.btnExportAll.TabIndex = 6;
            this.btnExportAll.Text = "Export All";
            this.btnExportAll.UseVisualStyleBackColor = true;
            this.btnExportAll.Click += new System.EventHandler(this.btnExportAll_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(141, 240);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // chkNewPalette
            // 
            this.chkNewPalette.AutoSize = true;
            this.chkNewPalette.Enabled = false;
            this.chkNewPalette.Location = new System.Drawing.Point(16, 217);
            this.chkNewPalette.Name = "chkNewPalette";
            this.chkNewPalette.Size = new System.Drawing.Size(84, 17);
            this.chkNewPalette.TabIndex = 8;
            this.chkNewPalette.Text = "New Palette";
            this.chkNewPalette.UseVisualStyleBackColor = true;
            this.chkNewPalette.Visible = false;
            // 
            // lblPalette
            // 
            this.lblPalette.AutoSize = true;
            this.lblPalette.Location = new System.Drawing.Point(222, 26);
            this.lblPalette.Name = "lblPalette";
            this.lblPalette.Size = new System.Drawing.Size(43, 13);
            this.lblPalette.TabIndex = 9;
            this.lblPalette.Text = "Palette:";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnLoadBTP});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(650, 25);
            this.toolStrip1.TabIndex = 10;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnLoadBTP
            // 
            this.btnLoadBTP.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnLoadBTP.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLoadBTP.Name = "btnLoadBTP";
            this.btnLoadBTP.Size = new System.Drawing.Size(61, 22);
            this.btnLoadBTP.Text = "Load BTP";
            this.btnLoadBTP.Click += new System.EventHandler(this.btnLoadBTP_Click);
            // 
            // lbxPalettes
            // 
            this.lbxPalettes.FormattingEnabled = true;
            this.lbxPalettes.Location = new System.Drawing.Point(222, 42);
            this.lbxPalettes.Name = "lbxPalettes";
            this.lbxPalettes.Size = new System.Drawing.Size(200, 173);
            this.lbxPalettes.TabIndex = 12;
            this.lbxPalettes.SelectedIndexChanged += new System.EventHandler(this.lbxPalettes_SelectedIndexChanged);
            // 
            // TextureEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 276);
            this.Controls.Add(this.lbxPalettes);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.lblPalette);
            this.Controls.Add(this.chkNewPalette);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnExportAll);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnReplaceSelected);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pbxTexture);
            this.Controls.Add(this.lbxTextures);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TextureEditorForm";
            this.Text = "Texture Editor";
            //((System.ComponentModel.ISupportInitialize)(this.pbxTexture)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lbxTextures;
        private System.Windows.Forms.PictureBox pbxTexture;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnReplaceSelected;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnExportAll;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox chkNewPalette;
        private System.Windows.Forms.Label lblPalette;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnLoadBTP;
        private System.Windows.Forms.ListBox lbxPalettes;
    }
}