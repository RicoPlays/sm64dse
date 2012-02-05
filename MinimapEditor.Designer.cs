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
            ((System.ComponentModel.ISupportInitialize)(this.pbxMinimapGfx)).BeginInit();
            this.tsMinimapEditor.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbxMinimapGfx
            // 
            this.pbxMinimapGfx.Location = new System.Drawing.Point(72, 56);
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
            this.tslBeforeAreaBtns});
            this.tsMinimapEditor.Location = new System.Drawing.Point(0, 0);
            this.tsMinimapEditor.Name = "tsMinimapEditor";
            this.tsMinimapEditor.Size = new System.Drawing.Size(721, 25);
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
            // MinimapEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 566);
            this.Controls.Add(this.tsMinimapEditor);
            this.Controls.Add(this.pbxMinimapGfx);
            this.Name = "MinimapEditor";
            this.Text = "MinimapEditor";
            this.Load += new System.EventHandler(this.MinimapEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbxMinimapGfx)).EndInit();
            this.tsMinimapEditor.ResumeLayout(false);
            this.tsMinimapEditor.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbxMinimapGfx;
        private System.Windows.Forms.ToolStrip tsMinimapEditor;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel tslBeforeAreaBtns;
    }
}