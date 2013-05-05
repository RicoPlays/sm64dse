namespace SM64DSe
{
    partial class MinimapEditor
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbxMinimapGfx = new System.Windows.Forms.PictureBox();
            this.tsMinimapEditor = new System.Windows.Forms.ToolStrip();
            this.tslBeforeAreaBtns = new System.Windows.Forms.ToolStripLabel();
            this.btnImport = new System.Windows.Forms.ToolStripButton();
            this.btnExport = new System.Windows.Forms.ToolStripButton();
            this.gridPalette = new System.Windows.Forms.DataGridView();
            this.lblPaletteTitle = new System.Windows.Forms.Label();
            this.btnSetBackground = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCoordScale = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnSelNSC = new System.Windows.Forms.Button();
            this.btnSelNCL = new System.Windows.Forms.Button();
            this.btnSelNCG = new System.Windows.Forms.Button();
            this.cbxBPP = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.dmnHeight = new System.Windows.Forms.DomainUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.dmnWidth = new System.Windows.Forms.DomainUpDown();
            this.txtSelNSC = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSelNCL = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSelNCG = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkNCGDcmpd = new System.Windows.Forms.CheckBox();
            this.chkNSCDcmpd = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtZoom = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxMinimapGfx)).BeginInit();
            this.tsMinimapEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPalette)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbxMinimapGfx
            // 
            this.pbxMinimapGfx.Location = new System.Drawing.Point(12, 56);
            this.pbxMinimapGfx.Name = "pbxMinimapGfx";
            this.pbxMinimapGfx.Size = new System.Drawing.Size(512, 512);
            this.pbxMinimapGfx.TabIndex = 0;
            this.pbxMinimapGfx.TabStop = false;
            // 
            // tsMinimapEditor
            // 
            this.tsMinimapEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslBeforeAreaBtns,
            this.btnImport,
            this.btnExport});
            this.tsMinimapEditor.Location = new System.Drawing.Point(0, 0);
            this.tsMinimapEditor.Name = "tsMinimapEditor";
            this.tsMinimapEditor.Size = new System.Drawing.Size(816, 25);
            this.tsMinimapEditor.TabIndex = 1;
            this.tsMinimapEditor.Text = "toolStrip1";
            // 
            // tslBeforeAreaBtns
            // 
            this.tslBeforeAreaBtns.Name = "tslBeforeAreaBtns";
            this.tslBeforeAreaBtns.Size = new System.Drawing.Size(124, 22);
            this.tslBeforeAreaBtns.Text = "Edit minimap for area:";
            // 
            // btnImport
            // 
            this.btnImport.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(47, 22);
            this.btnImport.Text = "Import";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnExport
            // 
            this.btnExport.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(44, 22);
            this.btnExport.Text = "Export";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // gridPalette
            // 
            this.gridPalette.AllowUserToAddRows = false;
            this.gridPalette.AllowUserToDeleteRows = false;
            this.gridPalette.AllowUserToResizeColumns = false;
            this.gridPalette.AllowUserToResizeRows = false;
            this.gridPalette.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.gridPalette.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gridPalette.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridPalette.ColumnHeadersVisible = false;
            this.gridPalette.EnableHeadersVisualStyles = false;
            this.gridPalette.Location = new System.Drawing.Point(547, 56);
            this.gridPalette.MultiSelect = false;
            this.gridPalette.Name = "gridPalette";
            this.gridPalette.ReadOnly = true;
            this.gridPalette.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.gridPalette.RowHeadersVisible = false;
            this.gridPalette.RowHeadersWidth = 23;
            this.gridPalette.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.gridPalette.RowTemplate.Height = 16;
            this.gridPalette.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.gridPalette.Size = new System.Drawing.Size(260, 260);
            this.gridPalette.TabIndex = 4;
            this.gridPalette.CurrentCellChanged += new System.EventHandler(this.gridPalette_CurrentCellChanged);
            // 
            // lblPaletteTitle
            // 
            this.lblPaletteTitle.AutoSize = true;
            this.lblPaletteTitle.Location = new System.Drawing.Point(544, 40);
            this.lblPaletteTitle.Name = "lblPaletteTitle";
            this.lblPaletteTitle.Size = new System.Drawing.Size(43, 13);
            this.lblPaletteTitle.TabIndex = 5;
            this.lblPaletteTitle.Text = "Palette:";
            // 
            // btnSetBackground
            // 
            this.btnSetBackground.Location = new System.Drawing.Point(547, 322);
            this.btnSetBackground.Name = "btnSetBackground";
            this.btnSetBackground.Size = new System.Drawing.Size(151, 23);
            this.btnSetBackground.TabIndex = 6;
            this.btnSetBackground.Text = "Set Selected as Background";
            this.btnSetBackground.UseVisualStyleBackColor = true;
            this.btnSetBackground.Click += new System.EventHandler(this.btnSetBackground_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(544, 352);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Co-ordinate Scale";
            // 
            // txtCoordScale
            // 
            this.txtCoordScale.Location = new System.Drawing.Point(641, 349);
            this.txtCoordScale.Name = "txtCoordScale";
            this.txtCoordScale.Size = new System.Drawing.Size(163, 20);
            this.txtCoordScale.TabIndex = 8;
            this.txtCoordScale.TextChanged += new System.EventHandler(this.txtCoordScale_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkNSCDcmpd);
            this.groupBox1.Controls.Add(this.chkNCGDcmpd);
            this.groupBox1.Controls.Add(this.btnLoadImage);
            this.groupBox1.Controls.Add(this.btnSelNSC);
            this.groupBox1.Controls.Add(this.btnSelNCL);
            this.groupBox1.Controls.Add(this.btnSelNCG);
            this.groupBox1.Controls.Add(this.cbxBPP);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.dmnHeight);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.dmnWidth);
            this.groupBox1.Controls.Add(this.txtSelNSC);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtSelNCL);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtSelNCG);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(547, 375);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 237);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Image";
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.Location = new System.Drawing.Point(6, 208);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(75, 23);
            this.btnLoadImage.TabIndex = 15;
            this.btnLoadImage.Text = "Load Image";
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.btnLoadImage_Click);
            // 
            // btnSelNSC
            // 
            this.btnSelNSC.Location = new System.Drawing.Point(224, 120);
            this.btnSelNSC.Name = "btnSelNSC";
            this.btnSelNSC.Size = new System.Drawing.Size(30, 24);
            this.btnSelNSC.TabIndex = 14;
            this.btnSelNSC.Text = "...";
            this.btnSelNSC.UseVisualStyleBackColor = true;
            this.btnSelNSC.Click += new System.EventHandler(this.btnSelNSC_Click);
            // 
            // btnSelNCL
            // 
            this.btnSelNCL.Location = new System.Drawing.Point(224, 75);
            this.btnSelNCL.Name = "btnSelNCL";
            this.btnSelNCL.Size = new System.Drawing.Size(30, 24);
            this.btnSelNCL.TabIndex = 13;
            this.btnSelNCL.Text = "...";
            this.btnSelNCL.UseVisualStyleBackColor = true;
            this.btnSelNCL.Click += new System.EventHandler(this.btnSelNCL_Click);
            // 
            // btnSelNCG
            // 
            this.btnSelNCG.Location = new System.Drawing.Point(224, 36);
            this.btnSelNCG.Name = "btnSelNCG";
            this.btnSelNCG.Size = new System.Drawing.Size(30, 24);
            this.btnSelNCG.TabIndex = 12;
            this.btnSelNCG.Text = "...";
            this.btnSelNCG.UseVisualStyleBackColor = true;
            this.btnSelNCG.Click += new System.EventHandler(this.btnSelNCG_Click);
            // 
            // cbxBPP
            // 
            this.cbxBPP.FormattingEnabled = true;
            this.cbxBPP.Items.AddRange(new object[] {
            "4",
            "8"});
            this.cbxBPP.Location = new System.Drawing.Point(145, 175);
            this.cbxBPP.Name = "cbxBPP";
            this.cbxBPP.Size = new System.Drawing.Size(109, 21);
            this.cbxBPP.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 178);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "Bits Per Pixel";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(142, 150);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Height";
            // 
            // dmnHeight
            // 
            this.dmnHeight.Location = new System.Drawing.Point(186, 148);
            this.dmnHeight.Name = "dmnHeight";
            this.dmnHeight.Size = new System.Drawing.Size(68, 20);
            this.dmnHeight.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 150);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Width";
            // 
            // dmnWidth
            // 
            this.dmnWidth.Location = new System.Drawing.Point(47, 148);
            this.dmnWidth.Name = "dmnWidth";
            this.dmnWidth.Size = new System.Drawing.Size(68, 20);
            this.dmnWidth.TabIndex = 6;
            // 
            // txtSelNSC
            // 
            this.txtSelNSC.Location = new System.Drawing.Point(6, 122);
            this.txtSelNSC.Name = "txtSelNSC";
            this.txtSelNSC.Size = new System.Drawing.Size(212, 20);
            this.txtSelNSC.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Screen NSC";
            // 
            // txtSelNCL
            // 
            this.txtSelNCL.Location = new System.Drawing.Point(6, 77);
            this.txtSelNCL.Name = "txtSelNCL";
            this.txtSelNCL.Size = new System.Drawing.Size(212, 20);
            this.txtSelNCL.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Palette NCL";
            // 
            // txtSelNCG
            // 
            this.txtSelNCG.Location = new System.Drawing.Point(6, 38);
            this.txtSelNCG.Name = "txtSelNCG";
            this.txtSelNCG.Size = new System.Drawing.Size(212, 20);
            this.txtSelNCG.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Graphic NCG";
            // 
            // chkNCGDcmpd
            // 
            this.chkNCGDcmpd.AutoSize = true;
            this.chkNCGDcmpd.Location = new System.Drawing.Point(119, 15);
            this.chkNCGDcmpd.Name = "chkNCGDcmpd";
            this.chkNCGDcmpd.Size = new System.Drawing.Size(135, 17);
            this.chkNCGDcmpd.TabIndex = 16;
            this.chkNCGDcmpd.Text = "Already Decompressed";
            this.chkNCGDcmpd.UseVisualStyleBackColor = true;
            // 
            // chkNSCDcmpd
            // 
            this.chkNSCDcmpd.AutoSize = true;
            this.chkNSCDcmpd.Checked = true;
            this.chkNSCDcmpd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNSCDcmpd.Location = new System.Drawing.Point(119, 99);
            this.chkNSCDcmpd.Name = "chkNSCDcmpd";
            this.chkNSCDcmpd.Size = new System.Drawing.Size(135, 17);
            this.chkNSCDcmpd.TabIndex = 17;
            this.chkNSCDcmpd.Text = "Already Decompressed";
            this.chkNSCDcmpd.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 583);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(34, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "Zoom";
            // 
            // txtZoom
            // 
            this.txtZoom.Location = new System.Drawing.Point(58, 580);
            this.txtZoom.Name = "txtZoom";
            this.txtZoom.Size = new System.Drawing.Size(100, 20);
            this.txtZoom.TabIndex = 13;
            this.txtZoom.TextChanged += new System.EventHandler(this.txtZoom_TextChanged);
            // 
            // MinimapEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 624);
            this.Controls.Add(this.txtZoom);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtCoordScale);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSetBackground);
            this.Controls.Add(this.lblPaletteTitle);
            this.Controls.Add(this.gridPalette);
            this.Controls.Add(this.tsMinimapEditor);
            this.Controls.Add(this.pbxMinimapGfx);
            this.Name = "MinimapEditor";
            this.Text = "MinimapEditor";
            this.Load += new System.EventHandler(this.MinimapEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbxMinimapGfx)).EndInit();
            this.tsMinimapEditor.ResumeLayout(false);
            this.tsMinimapEditor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPalette)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbxMinimapGfx;
        private System.Windows.Forms.ToolStrip tsMinimapEditor;
        private System.Windows.Forms.ToolStripLabel tslBeforeAreaBtns;
        private System.Windows.Forms.ToolStripButton btnImport;
        private System.Windows.Forms.DataGridView gridPalette;
        private System.Windows.Forms.Label lblPaletteTitle;
        private System.Windows.Forms.Button btnSetBackground;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCoordScale;
        private System.Windows.Forms.ToolStripButton btnExport;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbxBPP;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DomainUpDown dmnHeight;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DomainUpDown dmnWidth;
        private System.Windows.Forms.TextBox txtSelNSC;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSelNCL;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSelNCG;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSelNSC;
        private System.Windows.Forms.Button btnSelNCL;
        private System.Windows.Forms.Button btnSelNCG;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.CheckBox chkNSCDcmpd;
        private System.Windows.Forms.CheckBox chkNCGDcmpd;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtZoom;
    }
}