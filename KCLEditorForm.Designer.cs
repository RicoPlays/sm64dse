namespace SM64DSe
{
    partial class KCLEditorForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtV3 = new System.Windows.Forms.TextBox();
            this.txtV2 = new System.Windows.Forms.TextBox();
            this.txtV1 = new System.Windows.Forms.TextBox();
            this.txtColType = new System.Windows.Forms.TextBox();
            this.lblColType = new System.Windows.Forms.Label();
            this.lblV3 = new System.Windows.Forms.Label();
            this.lblV2 = new System.Windows.Forms.Label();
            this.lblV1 = new System.Windows.Forms.Label();
            this.lbxPlanes = new System.Windows.Forms.ListBox();
            this.glModelView = new OpenTK.GLControl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.cmbPolygonMode = new System.Windows.Forms.ComboBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtV3);
            this.splitContainer1.Panel1.Controls.Add(this.txtV2);
            this.splitContainer1.Panel1.Controls.Add(this.txtV1);
            this.splitContainer1.Panel1.Controls.Add(this.txtColType);
            this.splitContainer1.Panel1.Controls.Add(this.lblColType);
            this.splitContainer1.Panel1.Controls.Add(this.lblV3);
            this.splitContainer1.Panel1.Controls.Add(this.lblV2);
            this.splitContainer1.Panel1.Controls.Add(this.lblV1);
            this.splitContainer1.Panel1.Controls.Add(this.lbxPlanes);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.glModelView);
            this.splitContainer1.Size = new System.Drawing.Size(683, 431);
            this.splitContainer1.SplitterDistance = 227;
            this.splitContainer1.TabIndex = 0;
            // 
            // txtV3
            // 
            this.txtV3.Location = new System.Drawing.Point(73, 382);
            this.txtV3.Name = "txtV3";
            this.txtV3.ReadOnly = true;
            this.txtV3.Size = new System.Drawing.Size(151, 20);
            this.txtV3.TabIndex = 8;
            // 
            // txtV2
            // 
            this.txtV2.Location = new System.Drawing.Point(73, 360);
            this.txtV2.Name = "txtV2";
            this.txtV2.ReadOnly = true;
            this.txtV2.Size = new System.Drawing.Size(151, 20);
            this.txtV2.TabIndex = 7;
            // 
            // txtV1
            // 
            this.txtV1.Location = new System.Drawing.Point(73, 338);
            this.txtV1.Name = "txtV1";
            this.txtV1.ReadOnly = true;
            this.txtV1.Size = new System.Drawing.Size(151, 20);
            this.txtV1.TabIndex = 6;
            // 
            // txtColType
            // 
            this.txtColType.Location = new System.Drawing.Point(73, 404);
            this.txtColType.Name = "txtColType";
            this.txtColType.Size = new System.Drawing.Size(151, 20);
            this.txtColType.TabIndex = 5;
            this.txtColType.TextChanged += new System.EventHandler(this.txtColType_TextChanged);
            // 
            // lblColType
            // 
            this.lblColType.AutoSize = true;
            this.lblColType.Location = new System.Drawing.Point(3, 407);
            this.lblColType.Name = "lblColType";
            this.lblColType.Size = new System.Drawing.Size(72, 13);
            this.lblColType.TabIndex = 4;
            this.lblColType.Text = "Collision Type";
            // 
            // lblV3
            // 
            this.lblV3.AutoSize = true;
            this.lblV3.Location = new System.Drawing.Point(3, 385);
            this.lblV3.Name = "lblV3";
            this.lblV3.Size = new System.Drawing.Size(46, 13);
            this.lblV3.TabIndex = 3;
            this.lblV3.Text = "Vertex 3";
            // 
            // lblV2
            // 
            this.lblV2.AutoSize = true;
            this.lblV2.Location = new System.Drawing.Point(3, 363);
            this.lblV2.Name = "lblV2";
            this.lblV2.Size = new System.Drawing.Size(46, 13);
            this.lblV2.TabIndex = 2;
            this.lblV2.Text = "Vertex 2";
            // 
            // lblV1
            // 
            this.lblV1.AutoSize = true;
            this.lblV1.Location = new System.Drawing.Point(3, 341);
            this.lblV1.Name = "lblV1";
            this.lblV1.Size = new System.Drawing.Size(46, 13);
            this.lblV1.TabIndex = 1;
            this.lblV1.Text = "Vertex 1";
            // 
            // lbxPlanes
            // 
            this.lbxPlanes.FormattingEnabled = true;
            this.lbxPlanes.Location = new System.Drawing.Point(4, 4);
            this.lbxPlanes.Name = "lbxPlanes";
            this.lbxPlanes.Size = new System.Drawing.Size(220, 329);
            this.lbxPlanes.TabIndex = 0;
            this.lbxPlanes.SelectedIndexChanged += new System.EventHandler(this.lbxPlanes_SelectedIndexChanged);
            // 
            // glModelView
            // 
            this.glModelView.BackColor = System.Drawing.Color.Black;
            this.glModelView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glModelView.Location = new System.Drawing.Point(0, 0);
            this.glModelView.Name = "glModelView";
            this.glModelView.Size = new System.Drawing.Size(452, 431);
            this.glModelView.TabIndex = 0;
            this.glModelView.VSync = false;
            this.glModelView.Load += new System.EventHandler(this.glModelView_Load);
            this.glModelView.Paint += new System.Windows.Forms.PaintEventHandler(this.glModelView_Paint);
            this.glModelView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseMove);
            this.glModelView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseDown);
            this.glModelView.Resize += new System.EventHandler(this.glModelView_Resize);
            this.glModelView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseUp);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSave});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(683, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(35, 22);
            this.btnSave.Text = "Save";
            this.btnSave.ToolTipText = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cmbPolygonMode
            // 
            this.cmbPolygonMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPolygonMode.FormattingEnabled = true;
            this.cmbPolygonMode.Location = new System.Drawing.Point(562, 1);
            this.cmbPolygonMode.Name = "cmbPolygonMode";
            this.cmbPolygonMode.Size = new System.Drawing.Size(121, 21);
            this.cmbPolygonMode.TabIndex = 2;
            this.cmbPolygonMode.SelectedIndexChanged += new System.EventHandler(this.cmbPolygonMode_SelectedIndexChanged);
            // 
            // KCLEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 465);
            this.Controls.Add(this.cmbPolygonMode);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Name = "KCLEditorForm";
            this.Text = "KCL Editor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private OpenTK.GLControl glModelView;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ListBox lbxPlanes;
        private System.Windows.Forms.TextBox txtV2;
        private System.Windows.Forms.TextBox txtV1;
        private System.Windows.Forms.TextBox txtColType;
        private System.Windows.Forms.Label lblColType;
        private System.Windows.Forms.Label lblV3;
        private System.Windows.Forms.Label lblV2;
        private System.Windows.Forms.Label lblV1;
        private System.Windows.Forms.TextBox txtV3;
        private System.Windows.Forms.ComboBox cmbPolygonMode;
    }
}