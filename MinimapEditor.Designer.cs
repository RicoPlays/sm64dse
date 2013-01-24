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
            this.lblMapSizeTitle = new System.Windows.Forms.Label();
            this.gridPalette = new System.Windows.Forms.DataGridView();
            this.lblPaletteTitle = new System.Windows.Forms.Label();
            this.btnSetBackground = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCoordScale = new System.Windows.Forms.TextBox();
            this.cbSizes = new System.Windows.Forms.ComboBox();
            this.btnResize = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblMapsize = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbxMinimapGfx)).BeginInit();
            this.tsMinimapEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPalette)).BeginInit();
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
            this.btnImport});
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
            // lblMapSizeTitle
            // 
            this.lblMapSizeTitle.AutoSize = true;
            this.lblMapSizeTitle.Location = new System.Drawing.Point(544, 25);
            this.lblMapSizeTitle.Name = "lblMapSizeTitle";
            this.lblMapSizeTitle.Size = new System.Drawing.Size(54, 13);
            this.lblMapSizeTitle.TabIndex = 2;
            this.lblMapSizeTitle.Text = "Map Size:";
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
            this.gridPalette.Location = new System.Drawing.Point(547, 88);
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
            this.lblPaletteTitle.Location = new System.Drawing.Point(544, 72);
            this.lblPaletteTitle.Name = "lblPaletteTitle";
            this.lblPaletteTitle.Size = new System.Drawing.Size(43, 13);
            this.lblPaletteTitle.TabIndex = 5;
            this.lblPaletteTitle.Text = "Palette:";
            // 
            // btnSetBackground
            // 
            this.btnSetBackground.Location = new System.Drawing.Point(547, 354);
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
            this.label1.Location = new System.Drawing.Point(544, 392);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Co-ordinate Scale";
            // 
            // txtCoordScale
            // 
            this.txtCoordScale.Location = new System.Drawing.Point(704, 389);
            this.txtCoordScale.Name = "txtCoordScale";
            this.txtCoordScale.Size = new System.Drawing.Size(100, 20);
            this.txtCoordScale.TabIndex = 8;
            this.txtCoordScale.TextChanged += new System.EventHandler(this.txtCoordScale_TextChanged);
            // 
            // cbSizes
            // 
            this.cbSizes.Enabled = false;
            this.cbSizes.FormattingEnabled = true;
            this.cbSizes.Location = new System.Drawing.Point(547, 42);
            this.cbSizes.Name = "cbSizes";
            this.cbSizes.Size = new System.Drawing.Size(121, 21);
            this.cbSizes.TabIndex = 9;
            this.cbSizes.Visible = false;
            // 
            // btnResize
            // 
            this.btnResize.Enabled = false;
            this.btnResize.Location = new System.Drawing.Point(674, 42);
            this.btnResize.Name = "btnResize";
            this.btnResize.Size = new System.Drawing.Size(75, 23);
            this.btnResize.TabIndex = 10;
            this.btnResize.Text = "Resize";
            this.btnResize.UseVisualStyleBackColor = true;
            this.btnResize.Visible = false;
            this.btnResize.Click += new System.EventHandler(this.btnResize_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(547, 446);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(257, 66);
            this.textBox1.TabIndex = 11;
            this.textBox1.Text = "Note: When using 256x256 maps you can only use one image. If you want different i" +
                "mages for different areas you\'ll need to use 128x128 maps.";
            this.textBox1.Visible = false;
            // 
            // lblMapsize
            // 
            this.lblMapsize.AutoSize = true;
            this.lblMapsize.Location = new System.Drawing.Point(604, 25);
            this.lblMapsize.Name = "lblMapsize";
            this.lblMapsize.Size = new System.Drawing.Size(35, 13);
            this.lblMapsize.TabIndex = 12;
            this.lblMapsize.Text = "label2";
            // 
            // MinimapEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 574);
            this.Controls.Add(this.lblMapsize);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnResize);
            this.Controls.Add(this.cbSizes);
            this.Controls.Add(this.txtCoordScale);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSetBackground);
            this.Controls.Add(this.lblPaletteTitle);
            this.Controls.Add(this.gridPalette);
            this.Controls.Add(this.lblMapSizeTitle);
            this.Controls.Add(this.tsMinimapEditor);
            this.Controls.Add(this.pbxMinimapGfx);
            this.Name = "MinimapEditor";
            this.Text = "MinimapEditor";
            this.Load += new System.EventHandler(this.MinimapEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbxMinimapGfx)).EndInit();
            this.tsMinimapEditor.ResumeLayout(false);
            this.tsMinimapEditor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPalette)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbxMinimapGfx;
        private System.Windows.Forms.ToolStrip tsMinimapEditor;
        private System.Windows.Forms.ToolStripLabel tslBeforeAreaBtns;
        private System.Windows.Forms.Label lblMapSizeTitle;
        private System.Windows.Forms.ToolStripButton btnImport;
        private System.Windows.Forms.DataGridView gridPalette;
        private System.Windows.Forms.Label lblPaletteTitle;
        private System.Windows.Forms.Button btnSetBackground;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCoordScale;
        private System.Windows.Forms.ComboBox cbSizes;
        private System.Windows.Forms.Button btnResize;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblMapsize;
    }
}