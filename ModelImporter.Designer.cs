namespace SM64DSe
{
    partial class ModelImporter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelImporter));
            this.tsToolBar = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tbModelName = new System.Windows.Forms.ToolStripTextBox();
            this.btnOpenModel = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnImport = new System.Windows.Forms.ToolStripButton();
            this.ssStatusBar = new System.Windows.Forms.StatusStrip();
            this.slStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.glModelView = new OpenTK.GLControl();
            this.spcMainContainer = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbWipeLevel = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbDropExtraShit = new System.Windows.Forms.CheckBox();
            this.cbMakeAcmlmboard = new System.Windows.Forms.CheckBox();
            this.lblScale = new System.Windows.Forms.Label();
            this.cbSwapYZ = new System.Windows.Forms.CheckBox();
            this.tbScale = new System.Windows.Forms.TextBox();
            this.cbZMirror = new System.Windows.Forms.CheckBox();
            this.ofdLoadModel = new System.Windows.Forms.OpenFileDialog();
            this.lbl01 = new System.Windows.Forms.Label();
            this.txtThreshold = new System.Windows.Forms.TextBox();
            this.lbl02 = new System.Windows.Forms.Label();
            this.tsToolBar.SuspendLayout();
            this.ssStatusBar.SuspendLayout();
            this.spcMainContainer.Panel1.SuspendLayout();
            this.spcMainContainer.Panel2.SuspendLayout();
            this.spcMainContainer.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsToolBar
            // 
            this.tsToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tbModelName,
            this.btnOpenModel,
            this.toolStripSeparator1,
            this.btnImport});
            this.tsToolBar.Location = new System.Drawing.Point(0, 0);
            this.tsToolBar.Name = "tsToolBar";
            this.tsToolBar.Size = new System.Drawing.Size(753, 25);
            this.tsToolBar.TabIndex = 0;
            this.tsToolBar.Text = "loluseless";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(44, 22);
            this.toolStripLabel1.Text = "Model:";
            // 
            // tbModelName
            // 
            this.tbModelName.Name = "tbModelName";
            this.tbModelName.ReadOnly = true;
            this.tbModelName.Size = new System.Drawing.Size(200, 25);
            this.tbModelName.Text = "<model name here>";
            // 
            // btnOpenModel
            // 
            this.btnOpenModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnOpenModel.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenModel.Image")));
            this.btnOpenModel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpenModel.Name = "btnOpenModel";
            this.btnOpenModel.Size = new System.Drawing.Size(49, 22);
            this.btnOpenModel.Text = "Open...";
            this.btnOpenModel.Click += new System.EventHandler(this.btnOpenModel_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnImport
            // 
            this.btnImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnImport.Image = ((System.Drawing.Image)(resources.GetObject("btnImport.Image")));
            this.btnImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(47, 22);
            this.btnImport.Text = "Import";
            this.btnImport.ToolTipText = "Import the thing into the game!";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // ssStatusBar
            // 
            this.ssStatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slStatus});
            this.ssStatusBar.Location = new System.Drawing.Point(0, 490);
            this.ssStatusBar.Name = "ssStatusBar";
            this.ssStatusBar.Size = new System.Drawing.Size(753, 22);
            this.ssStatusBar.TabIndex = 1;
            this.ssStatusBar.Text = "loluseless";
            // 
            // slStatus
            // 
            this.slStatus.Name = "slStatus";
            this.slStatus.Size = new System.Drawing.Size(20, 17);
            this.slStatus.Text = "lol";
            // 
            // glModelView
            // 
            this.glModelView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.glModelView.BackColor = System.Drawing.Color.Black;
            this.glModelView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glModelView.Location = new System.Drawing.Point(0, 0);
            this.glModelView.Margin = new System.Windows.Forms.Padding(0);
            this.glModelView.Name = "glModelView";
            this.glModelView.Size = new System.Drawing.Size(493, 465);
            this.glModelView.TabIndex = 2;
            this.glModelView.VSync = false;
            this.glModelView.Load += new System.EventHandler(this.glModelView_Load);
            this.glModelView.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseWheel);
            this.glModelView.Paint += new System.Windows.Forms.PaintEventHandler(this.glModelView_Paint);
            this.glModelView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseMove);
            this.glModelView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseDown);
            this.glModelView.Resize += new System.EventHandler(this.glModelView_Resize);
            this.glModelView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseUp);
            // 
            // spcMainContainer
            // 
            this.spcMainContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.spcMainContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcMainContainer.IsSplitterFixed = true;
            this.spcMainContainer.Location = new System.Drawing.Point(0, 25);
            this.spcMainContainer.Margin = new System.Windows.Forms.Padding(0);
            this.spcMainContainer.Name = "spcMainContainer";
            // 
            // spcMainContainer.Panel1
            // 
            this.spcMainContainer.Panel1.Controls.Add(this.groupBox2);
            this.spcMainContainer.Panel1.Controls.Add(this.groupBox1);
            // 
            // spcMainContainer.Panel2
            // 
            this.spcMainContainer.Panel2.Controls.Add(this.glModelView);
            this.spcMainContainer.Size = new System.Drawing.Size(753, 465);
            this.spcMainContainer.SplitterDistance = 256;
            this.spcMainContainer.TabIndex = 3;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.cbWipeLevel);
            this.groupBox2.Location = new System.Drawing.Point(3, 200);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(250, 54);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Level settings";
            // 
            // cbWipeLevel
            // 
            this.cbWipeLevel.AutoSize = true;
            this.cbWipeLevel.Location = new System.Drawing.Point(12, 23);
            this.cbWipeLevel.Name = "cbWipeLevel";
            this.cbWipeLevel.Size = new System.Drawing.Size(148, 17);
            this.cbWipeLevel.TabIndex = 0;
            this.cbWipeLevel.Text = "Wipe level upon importing";
            this.cbWipeLevel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lbl02);
            this.groupBox1.Controls.Add(this.txtThreshold);
            this.groupBox1.Controls.Add(this.lbl01);
            this.groupBox1.Controls.Add(this.cbDropExtraShit);
            this.groupBox1.Controls.Add(this.cbMakeAcmlmboard);
            this.groupBox1.Controls.Add(this.lblScale);
            this.groupBox1.Controls.Add(this.cbSwapYZ);
            this.groupBox1.Controls.Add(this.tbScale);
            this.groupBox1.Controls.Add(this.cbZMirror);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 191);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Model settings";
            // 
            // cbDropExtraShit
            // 
            this.cbDropExtraShit.AutoSize = true;
            this.cbDropExtraShit.Enabled = false;
            this.cbDropExtraShit.Location = new System.Drawing.Point(29, 168);
            this.cbDropExtraShit.Name = "cbDropExtraShit";
            this.cbDropExtraShit.Size = new System.Drawing.Size(184, 17);
            this.cbDropExtraShit.TabIndex = 7;
            this.cbDropExtraShit.Text = "Drop excedentary collision planes";
            this.cbDropExtraShit.UseVisualStyleBackColor = true;
            // 
            // cbMakeAcmlmboard
            // 
            this.cbMakeAcmlmboard.AutoSize = true;
            this.cbMakeAcmlmboard.Checked = true;
            this.cbMakeAcmlmboard.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMakeAcmlmboard.Location = new System.Drawing.Point(12, 49);
            this.cbMakeAcmlmboard.Name = "cbMakeAcmlmboard";
            this.cbMakeAcmlmboard.Size = new System.Drawing.Size(133, 17);
            this.cbMakeAcmlmboard.TabIndex = 6;
            this.cbMakeAcmlmboard.Text = "Generate collision map";
            this.cbMakeAcmlmboard.UseVisualStyleBackColor = true;
            this.cbMakeAcmlmboard.CheckedChanged += new System.EventHandler(this.cbMakeAcmlmboard_CheckedChanged);
            // 
            // lblScale
            // 
            this.lblScale.AutoSize = true;
            this.lblScale.Location = new System.Drawing.Point(9, 26);
            this.lblScale.Name = "lblScale";
            this.lblScale.Size = new System.Drawing.Size(37, 13);
            this.lblScale.TabIndex = 0;
            this.lblScale.Text = "Scale:";
            // 
            // cbSwapYZ
            // 
            this.cbSwapYZ.AutoSize = true;
            this.cbSwapYZ.Enabled = false;
            this.cbSwapYZ.Location = new System.Drawing.Point(12, 145);
            this.cbSwapYZ.Name = "cbSwapYZ";
            this.cbSwapYZ.Size = new System.Drawing.Size(94, 17);
            this.cbSwapYZ.TabIndex = 4;
            this.cbSwapYZ.Text = "Swap Y and Z";
            this.cbSwapYZ.UseVisualStyleBackColor = true;
            this.cbSwapYZ.CheckedChanged += new System.EventHandler(this.cbSwapYZ_CheckedChanged);
            // 
            // tbScale
            // 
            this.tbScale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScale.Location = new System.Drawing.Point(52, 23);
            this.tbScale.Name = "tbScale";
            this.tbScale.Size = new System.Drawing.Size(184, 20);
            this.tbScale.TabIndex = 3;
            this.tbScale.Text = "1";
            this.tbScale.TextChanged += new System.EventHandler(this.tbScale_TextChanged);
            // 
            // cbZMirror
            // 
            this.cbZMirror.AutoSize = true;
            this.cbZMirror.Enabled = false;
            this.cbZMirror.Location = new System.Drawing.Point(12, 122);
            this.cbZMirror.Name = "cbZMirror";
            this.cbZMirror.Size = new System.Drawing.Size(97, 17);
            this.cbZMirror.TabIndex = 2;
            this.cbZMirror.Text = "Reverse Z axis";
            this.cbZMirror.UseVisualStyleBackColor = true;
            this.cbZMirror.CheckedChanged += new System.EventHandler(this.cbZMirror_CheckedChanged);
            // 
            // ofdLoadModel
            // 
            this.ofdLoadModel.Filter = "Wavefront (*.obj)|*.obj";
            this.ofdLoadModel.Title = "Load model file...";
            // 
            // lbl01
            // 
            this.lbl01.AutoSize = true;
            this.lbl01.Location = new System.Drawing.Point(26, 73);
            this.lbl01.Name = "lbl01";
            this.lbl01.Size = new System.Drawing.Size(118, 13);
            this.lbl01.TabIndex = 8;
            this.lbl01.Text = "Drop faces smaller than";
            // 
            // txtThreshold
            // 
            this.txtThreshold.Location = new System.Drawing.Point(150, 70);
            this.txtThreshold.Name = "txtThreshold";
            this.txtThreshold.Size = new System.Drawing.Size(86, 20);
            this.txtThreshold.TabIndex = 9;
            this.txtThreshold.Text = "0.001";
            // 
            // lbl02
            // 
            this.lbl02.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lbl02.Location = new System.Drawing.Point(26, 87);
            this.lbl02.Name = "lbl02";
            this.lbl02.Size = new System.Drawing.Size(109, 32);
            this.lbl02.TabIndex = 10;
            this.lbl02.Text = "Enter 0 to keep all faces, default 0.001";
            // 
            // ModelImporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 512);
            this.Controls.Add(this.spcMainContainer);
            this.Controls.Add(this.ssStatusBar);
            this.Controls.Add(this.tsToolBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModelImporter";
            this.Text = "Model importer - Super Mario 64 DS Editor lolol";
            this.Load += new System.EventHandler(this.ModelImporter_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ModelImporter_FormClosed);
            this.tsToolBar.ResumeLayout(false);
            this.tsToolBar.PerformLayout();
            this.ssStatusBar.ResumeLayout(false);
            this.ssStatusBar.PerformLayout();
            this.spcMainContainer.Panel1.ResumeLayout(false);
            this.spcMainContainer.Panel2.ResumeLayout(false);
            this.spcMainContainer.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsToolBar;
        private System.Windows.Forms.StatusStrip ssStatusBar;
        private OpenTK.GLControl glModelView;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton btnOpenModel;
        private System.Windows.Forms.SplitContainer spcMainContainer;
        private System.Windows.Forms.ToolStripTextBox tbModelName;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.OpenFileDialog ofdLoadModel;
        private System.Windows.Forms.CheckBox cbZMirror;
        private System.Windows.Forms.TextBox tbScale;
        private System.Windows.Forms.ToolStripButton btnImport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripStatusLabel slStatus;
        private System.Windows.Forms.CheckBox cbSwapYZ;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox cbMakeAcmlmboard;
        private System.Windows.Forms.CheckBox cbWipeLevel;
        private System.Windows.Forms.CheckBox cbDropExtraShit;
        private System.Windows.Forms.TextBox txtThreshold;
        private System.Windows.Forms.Label lbl01;
        private System.Windows.Forms.Label lbl02;
    }
}