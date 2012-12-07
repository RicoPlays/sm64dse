namespace SM64DSe
{
    partial class TextEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextEditorForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lbxMsgList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtEdit = new System.Windows.Forms.TextBox();
            this.tbxMsgPreview = new System.Windows.Forms.TextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lbxMsgList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.btnSave);
            this.splitContainer1.Panel2.Controls.Add(this.txtEdit);
            this.splitContainer1.Panel2.Controls.Add(this.tbxMsgPreview);
            this.splitContainer1.Size = new System.Drawing.Size(678, 515);
            this.splitContainer1.SplitterDistance = 226;
            this.splitContainer1.TabIndex = 0;
            // 
            // lbxMsgList
            // 
            this.lbxMsgList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbxMsgList.FormattingEnabled = true;
            this.lbxMsgList.Location = new System.Drawing.Point(0, 0);
            this.lbxMsgList.Name = "lbxMsgList";
            this.lbxMsgList.Size = new System.Drawing.Size(226, 515);
            this.lbxMsgList.TabIndex = 0;
            this.lbxMsgList.SelectedIndexChanged += new System.EventHandler(this.lbxMsgList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 272);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Enter your edited text here:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(0, 409);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(101, 34);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtEdit
            // 
            this.txtEdit.Location = new System.Drawing.Point(0, 288);
            this.txtEdit.Multiline = true;
            this.txtEdit.Name = "txtEdit";
            this.txtEdit.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtEdit.Size = new System.Drawing.Size(448, 115);
            this.txtEdit.TabIndex = 1;
            // 
            // tbxMsgPreview
            // 
            this.tbxMsgPreview.Location = new System.Drawing.Point(0, 0);
            this.tbxMsgPreview.Multiline = true;
            this.tbxMsgPreview.Name = "tbxMsgPreview";
            this.tbxMsgPreview.ReadOnly = true;
            this.tbxMsgPreview.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxMsgPreview.Size = new System.Drawing.Size(448, 243);
            this.tbxMsgPreview.TabIndex = 0;
            // 
            // TextEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 515);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TextEditorForm";
            this.Text = "Text editor - SM64DS Editor";
            this.Load += new System.EventHandler(this.TextEditorForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lbxMsgList;
        private System.Windows.Forms.TextBox tbxMsgPreview;
        private System.Windows.Forms.TextBox txtEdit;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label1;
    }
}