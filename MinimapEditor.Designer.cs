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
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tslBeforeAreaBtns = new System.Windows.Forms.ToolStripLabel();
            this.btnImport = new System.Windows.Forms.ToolStripButton();
            this.lblMapSizeTitle = new System.Windows.Forms.Label();
            this.lblMapSize = new System.Windows.Forms.Label();
            this.gridPalette = new System.Windows.Forms.DataGridView();
            this.lblPaletteTitle = new System.Windows.Forms.Label();
            this.btnSetBackground = new System.Windows.Forms.Button();
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
            this.toolStripLabel1,
            this.toolStripSeparator1,
            this.tslBeforeAreaBtns,
            this.btnImport});
            this.tsMinimapEditor.Location = new System.Drawing.Point(0, 0);
            this.tsMinimapEditor.Name = "tsMinimapEditor";
            this.tsMinimapEditor.Size = new System.Drawing.Size(816, 25);
            this.tsMinimapEditor.TabIndex = 1;
            this.tsMinimapEditor.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(26, 22);
            this.toolStripLabel1.Text = "test";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
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
            // lblMapSize
            // 
            this.lblMapSize.AutoSize = true;
            this.lblMapSize.Location = new System.Drawing.Point(604, 25);
            this.lblMapSize.Name = "lblMapSize";
            this.lblMapSize.Size = new System.Drawing.Size(45, 13);
            this.lblMapSize.TabIndex = 3;
            this.lblMapSize.Text = "mapsize";
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
            // MinimapEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 574);
            this.Controls.Add(this.btnSetBackground);
            this.Controls.Add(this.lblPaletteTitle);
            this.Controls.Add(this.gridPalette);
            this.Controls.Add(this.lblMapSize);
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
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel tslBeforeAreaBtns;
        private System.Windows.Forms.Label lblMapSizeTitle;
        private System.Windows.Forms.Label lblMapSize;
        private System.Windows.Forms.ToolStripButton btnImport;
        private System.Windows.Forms.DataGridView gridPalette;
        private System.Windows.Forms.Label lblPaletteTitle;
        private System.Windows.Forms.Button btnSetBackground;
    }
}