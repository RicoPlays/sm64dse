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
            this.chkNewPalette = new System.Windows.Forms.CheckBox();
            this.lblPalette = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnReplaceSelected = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExport = new System.Windows.Forms.ToolStripButton();
            this.btnExportAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnLoadBTP = new System.Windows.Forms.ToolStripButton();
            this.lbxPalettes = new System.Windows.Forms.ListBox();
            //((System.ComponentModel.ISupportInitialize)(this.pbxTexture)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Texture:";
            // 
            // lbxTextures
            // 
            this.lbxTextures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lbxTextures.FormattingEnabled = true;
            this.lbxTextures.Location = new System.Drawing.Point(12, 48);
            this.lbxTextures.Name = "lbxTextures";
            this.lbxTextures.Size = new System.Drawing.Size(199, 199);
            this.lbxTextures.TabIndex = 1;
            this.lbxTextures.SelectedIndexChanged += new System.EventHandler(this.lbxTextures_SelectedIndexChanged);
            // 
            // pbxTexture
            // 
            this.pbxTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pbxTexture.Location = new System.Drawing.Point(422, 48);
            this.pbxTexture.Name = "pbxTexture";
            this.pbxTexture.Size = new System.Drawing.Size(212, 199);
            this.pbxTexture.TabIndex = 2;
            this.pbxTexture.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(419, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Preview:";
            // 
            // chkNewPalette
            // 
            this.chkNewPalette.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkNewPalette.AutoSize = true;
            this.chkNewPalette.Enabled = false;
            this.chkNewPalette.Location = new System.Drawing.Point(12, 253);
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
            this.lblPalette.Location = new System.Drawing.Point(214, 32);
            this.lblPalette.Name = "lblPalette";
            this.lblPalette.Size = new System.Drawing.Size(43, 13);
            this.lblPalette.TabIndex = 9;
            this.lblPalette.Text = "Palette:";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSave,
            this.btnReplaceSelected,
            this.toolStripSeparator1,
            this.btnExport,
            this.btnExportAll,
            this.toolStripSeparator2,
            this.btnLoadBTP});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(650, 25);
            this.toolStrip1.TabIndex = 10;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(35, 22);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnReplaceSelected
            // 
            this.btnReplaceSelected.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnReplaceSelected.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReplaceSelected.Name = "btnReplaceSelected";
            this.btnReplaceSelected.Size = new System.Drawing.Size(99, 22);
            this.btnReplaceSelected.Text = "Replace Selected";
            this.btnReplaceSelected.Click += new System.EventHandler(this.btnReplaceSelected_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnExport
            // 
            this.btnExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(44, 22);
            this.btnExport.Text = "Export";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnExportAll
            // 
            this.btnExportAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExportAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExportAll.Name = "btnExportAll";
            this.btnExportAll.Size = new System.Drawing.Size(61, 22);
            this.btnExportAll.Text = "Export All";
            this.btnExportAll.Click += new System.EventHandler(this.btnExportAll_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
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
            this.lbxPalettes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lbxPalettes.FormattingEnabled = true;
            this.lbxPalettes.Location = new System.Drawing.Point(217, 48);
            this.lbxPalettes.Name = "lbxPalettes";
            this.lbxPalettes.Size = new System.Drawing.Size(199, 199);
            this.lbxPalettes.TabIndex = 12;
            this.lbxPalettes.SelectedIndexChanged += new System.EventHandler(this.lbxPalettes_SelectedIndexChanged);
            // 
            // TextureEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 276);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbxPalettes);
            this.Controls.Add(this.lbxTextures);
            this.Controls.Add(this.chkNewPalette);
            this.Controls.Add(this.lblPalette);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pbxTexture);
            this.Controls.Add(this.toolStrip1);
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
        private System.Windows.Forms.CheckBox chkNewPalette;
        private System.Windows.Forms.Label lblPalette;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnLoadBTP;
        private System.Windows.Forms.ListBox lbxPalettes;
        private System.Windows.Forms.ToolStripButton btnReplaceSelected;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnExport;
        private System.Windows.Forms.ToolStripButton btnExportAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}