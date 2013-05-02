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
            this.label1 = new System.Windows.Forms.Label();
            this.lbxTextures = new System.Windows.Forms.ListBox();
            this.pbxTexture = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnReplaceSelected = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnExportAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbxTexture)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Texture:";
            // 
            // lbxTextures
            // 
            this.lbxTextures.FormattingEnabled = true;
            this.lbxTextures.Location = new System.Drawing.Point(16, 30);
            this.lbxTextures.Name = "lbxTextures";
            this.lbxTextures.Size = new System.Drawing.Size(200, 173);
            this.lbxTextures.TabIndex = 1;
            this.lbxTextures.SelectedIndexChanged += new System.EventHandler(this.lbxTextures_SelectedIndexChanged);
            // 
            // pbxTexture
            // 
            this.pbxTexture.Location = new System.Drawing.Point(289, 30);
            this.pbxTexture.Name = "pbxTexture";
            this.pbxTexture.Size = new System.Drawing.Size(194, 173);
            this.pbxTexture.TabIndex = 2;
            this.pbxTexture.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(289, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Preview:";
            // 
            // btnReplaceSelected
            // 
            this.btnReplaceSelected.Location = new System.Drawing.Point(16, 223);
            this.btnReplaceSelected.Name = "btnReplaceSelected";
            this.btnReplaceSelected.Size = new System.Drawing.Size(113, 23);
            this.btnReplaceSelected.TabIndex = 4;
            this.btnReplaceSelected.Text = "Replace Selected";
            this.btnReplaceSelected.UseVisualStyleBackColor = true;
            this.btnReplaceSelected.Click += new System.EventHandler(this.btnReplaceSelected_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(289, 223);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnExportAll
            // 
            this.btnExportAll.Location = new System.Drawing.Point(370, 223);
            this.btnExportAll.Name = "btnExportAll";
            this.btnExportAll.Size = new System.Drawing.Size(75, 23);
            this.btnExportAll.TabIndex = 6;
            this.btnExportAll.Text = "Export All";
            this.btnExportAll.UseVisualStyleBackColor = true;
            this.btnExportAll.Click += new System.EventHandler(this.btnExportAll_Click);
            // 
            // TextureEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 262);
            this.Controls.Add(this.btnExportAll);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnReplaceSelected);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pbxTexture);
            this.Controls.Add(this.lbxTextures);
            this.Controls.Add(this.label1);
            this.Name = "TextureEditorForm";
            this.Text = "TextureEditorForm";
            ((System.ComponentModel.ISupportInitialize)(this.pbxTexture)).EndInit();
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
    }
}