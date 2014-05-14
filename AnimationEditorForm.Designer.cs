namespace SM64DSe
{
    partial class AnimationEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationEditorForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtNumFrames = new System.Windows.Forms.TextBox();
            this.lblFrameNum = new System.Windows.Forms.Label();
            this.txtCurrentFrameNum = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnLastFrame = new System.Windows.Forms.Button();
            this.btnNextFrame = new System.Windows.Forms.Button();
            this.btnPreviousFrame = new System.Windows.Forms.Button();
            this.btnFirstFrame = new System.Windows.Forms.Button();
            this.chkLoopAnimation = new System.Windows.Forms.CheckBox();
            this.btnStopAnimation = new System.Windows.Forms.Button();
            this.btnPlayAnimation = new System.Windows.Forms.Button();
            this.btnOpenBCA = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBCAName = new System.Windows.Forms.TextBox();
            this.btnOpenBMD = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBMDName = new System.Windows.Forms.TextBox();
            this.glModelView = new OpenTK.GLControl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnImportDAE = new System.Windows.Forms.ToolStripButton();
            this.btnExportDAE = new System.Windows.Forms.ToolStripButton();
            //((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.txtNumFrames);
            this.splitContainer1.Panel1.Controls.Add(this.lblFrameNum);
            this.splitContainer1.Panel1.Controls.Add(this.txtCurrentFrameNum);
            this.splitContainer1.Panel1.Controls.Add(this.label9);
            this.splitContainer1.Panel1.Controls.Add(this.btnLastFrame);
            this.splitContainer1.Panel1.Controls.Add(this.btnNextFrame);
            this.splitContainer1.Panel1.Controls.Add(this.btnPreviousFrame);
            this.splitContainer1.Panel1.Controls.Add(this.btnFirstFrame);
            this.splitContainer1.Panel1.Controls.Add(this.chkLoopAnimation);
            this.splitContainer1.Panel1.Controls.Add(this.btnStopAnimation);
            this.splitContainer1.Panel1.Controls.Add(this.btnPlayAnimation);
            this.splitContainer1.Panel1.Controls.Add(this.btnOpenBCA);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.txtBCAName);
            this.splitContainer1.Panel1.Controls.Add(this.btnOpenBMD);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.txtBMDName);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.glModelView);
            this.splitContainer1.Size = new System.Drawing.Size(794, 431);
            this.splitContainer1.SplitterDistance = 230;
            this.splitContainer1.TabIndex = 0;
            // 
            // txtNumFrames
            // 
            this.txtNumFrames.Location = new System.Drawing.Point(119, 243);
            this.txtNumFrames.Name = "txtNumFrames";
            this.txtNumFrames.ReadOnly = true;
            this.txtNumFrames.Size = new System.Drawing.Size(54, 20);
            this.txtNumFrames.TabIndex = 18;
            // 
            // lblFrameNum
            // 
            this.lblFrameNum.AutoSize = true;
            this.lblFrameNum.Location = new System.Drawing.Point(101, 246);
            this.lblFrameNum.Name = "lblFrameNum";
            this.lblFrameNum.Size = new System.Drawing.Size(12, 13);
            this.lblFrameNum.TabIndex = 17;
            this.lblFrameNum.Text = "/";
            // 
            // txtCurrentFrameNum
            // 
            this.txtCurrentFrameNum.Location = new System.Drawing.Point(41, 243);
            this.txtCurrentFrameNum.Name = "txtCurrentFrameNum";
            this.txtCurrentFrameNum.Size = new System.Drawing.Size(54, 20);
            this.txtCurrentFrameNum.TabIndex = 16;
            this.txtCurrentFrameNum.TextChanged += new System.EventHandler(this.txtCurrentFrameNum_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(2, 246);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "Frame:";
            // 
            // btnLastFrame
            // 
            this.btnLastFrame.Location = new System.Drawing.Point(126, 153);
            this.btnLastFrame.Name = "btnLastFrame";
            this.btnLastFrame.Size = new System.Drawing.Size(36, 23);
            this.btnLastFrame.TabIndex = 14;
            this.btnLastFrame.Text = "| >";
            this.btnLastFrame.UseVisualStyleBackColor = true;
            this.btnLastFrame.Click += new System.EventHandler(this.btnLastFrame_Click);
            // 
            // btnNextFrame
            // 
            this.btnNextFrame.Location = new System.Drawing.Point(87, 153);
            this.btnNextFrame.Name = "btnNextFrame";
            this.btnNextFrame.Size = new System.Drawing.Size(36, 23);
            this.btnNextFrame.TabIndex = 13;
            this.btnNextFrame.Text = ">";
            this.btnNextFrame.UseVisualStyleBackColor = true;
            this.btnNextFrame.Click += new System.EventHandler(this.btnNextFrame_Click);
            // 
            // btnPreviousFrame
            // 
            this.btnPreviousFrame.Location = new System.Drawing.Point(45, 153);
            this.btnPreviousFrame.Name = "btnPreviousFrame";
            this.btnPreviousFrame.Size = new System.Drawing.Size(36, 23);
            this.btnPreviousFrame.TabIndex = 12;
            this.btnPreviousFrame.Text = "<";
            this.btnPreviousFrame.UseVisualStyleBackColor = true;
            this.btnPreviousFrame.Click += new System.EventHandler(this.btnPreviousFrame_Click);
            // 
            // btnFirstFrame
            // 
            this.btnFirstFrame.Location = new System.Drawing.Point(6, 153);
            this.btnFirstFrame.Name = "btnFirstFrame";
            this.btnFirstFrame.Size = new System.Drawing.Size(36, 23);
            this.btnFirstFrame.TabIndex = 11;
            this.btnFirstFrame.Text = "< |";
            this.btnFirstFrame.UseVisualStyleBackColor = true;
            this.btnFirstFrame.Click += new System.EventHandler(this.btnFirstFrame_Click);
            // 
            // chkLoopAnimation
            // 
            this.chkLoopAnimation.AutoSize = true;
            this.chkLoopAnimation.Checked = true;
            this.chkLoopAnimation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLoopAnimation.Location = new System.Drawing.Point(171, 128);
            this.chkLoopAnimation.Name = "chkLoopAnimation";
            this.chkLoopAnimation.Size = new System.Drawing.Size(50, 17);
            this.chkLoopAnimation.TabIndex = 9;
            this.chkLoopAnimation.Text = "Loop";
            this.chkLoopAnimation.UseVisualStyleBackColor = true;
            this.chkLoopAnimation.CheckedChanged += new System.EventHandler(this.chkLoopAnimation_CheckedChanged);
            // 
            // btnStopAnimation
            // 
            this.btnStopAnimation.Location = new System.Drawing.Point(101, 124);
            this.btnStopAnimation.Name = "btnStopAnimation";
            this.btnStopAnimation.Size = new System.Drawing.Size(61, 23);
            this.btnStopAnimation.TabIndex = 8;
            this.btnStopAnimation.Text = "Stop";
            this.btnStopAnimation.UseVisualStyleBackColor = true;
            this.btnStopAnimation.Click += new System.EventHandler(this.btnStopAnimation_Click);
            // 
            // btnPlayAnimation
            // 
            this.btnPlayAnimation.Location = new System.Drawing.Point(6, 124);
            this.btnPlayAnimation.Name = "btnPlayAnimation";
            this.btnPlayAnimation.Size = new System.Drawing.Size(89, 23);
            this.btnPlayAnimation.TabIndex = 7;
            this.btnPlayAnimation.Text = "Play";
            this.btnPlayAnimation.UseVisualStyleBackColor = true;
            this.btnPlayAnimation.Click += new System.EventHandler(this.btnPlayAnimation_Click);
            // 
            // btnOpenBCA
            // 
            this.btnOpenBCA.Location = new System.Drawing.Point(193, 68);
            this.btnOpenBCA.Name = "btnOpenBCA";
            this.btnOpenBCA.Size = new System.Drawing.Size(31, 23);
            this.btnOpenBCA.TabIndex = 5;
            this.btnOpenBCA.Text = "...";
            this.btnOpenBCA.UseVisualStyleBackColor = true;
            this.btnOpenBCA.Click += new System.EventHandler(this.btnOpenBCA_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Animation (BCA):";
            // 
            // txtBCAName
            // 
            this.txtBCAName.Location = new System.Drawing.Point(6, 70);
            this.txtBCAName.Name = "txtBCAName";
            this.txtBCAName.ReadOnly = true;
            this.txtBCAName.Size = new System.Drawing.Size(183, 20);
            this.txtBCAName.TabIndex = 3;
            // 
            // btnOpenBMD
            // 
            this.btnOpenBMD.Location = new System.Drawing.Point(193, 20);
            this.btnOpenBMD.Name = "btnOpenBMD";
            this.btnOpenBMD.Size = new System.Drawing.Size(31, 23);
            this.btnOpenBMD.TabIndex = 2;
            this.btnOpenBMD.Text = "...";
            this.btnOpenBMD.UseVisualStyleBackColor = true;
            this.btnOpenBMD.Click += new System.EventHandler(this.btnOpenBMD_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Model (BMD):";
            // 
            // txtBMDName
            // 
            this.txtBMDName.Location = new System.Drawing.Point(6, 22);
            this.txtBMDName.Name = "txtBMDName";
            this.txtBMDName.ReadOnly = true;
            this.txtBMDName.Size = new System.Drawing.Size(183, 20);
            this.txtBMDName.TabIndex = 0;
            // 
            // glModelView
            // 
            this.glModelView.BackColor = System.Drawing.Color.Black;
            this.glModelView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glModelView.Location = new System.Drawing.Point(0, 0);
            this.glModelView.Name = "glModelView";
            this.glModelView.Size = new System.Drawing.Size(560, 431);
            this.glModelView.TabIndex = 0;
            this.glModelView.VSync = false;
            this.glModelView.Load += new System.EventHandler(this.glModelView_Load);
            this.glModelView.Paint += new System.Windows.Forms.PaintEventHandler(this.glModelView_Paint);
            this.glModelView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseDown);
            this.glModelView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseMove);
            this.glModelView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseUp);
            this.glModelView.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glModelView_MouseWheel);
            this.glModelView.Resize += new System.EventHandler(this.glModelView_Resize);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnImportDAE,
            this.btnExportDAE});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(794, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnImportDAE
            // 
            this.btnImportDAE.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnImportDAE.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImportDAE.Name = "btnImportDAE";
            this.btnImportDAE.Size = new System.Drawing.Size(72, 22);
            this.btnImportDAE.Text = "Import DAE";
            this.btnImportDAE.Click += new System.EventHandler(this.btnImportDAE_Click);
            // 
            // btnExportDAE
            // 
            this.btnExportDAE.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExportDAE.Image = ((System.Drawing.Image)(resources.GetObject("btnExportDAE.Image")));
            this.btnExportDAE.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExportDAE.Name = "btnExportDAE";
            this.btnExportDAE.Size = new System.Drawing.Size(69, 22);
            this.btnExportDAE.Text = "Export DAE";
            this.btnExportDAE.ToolTipText = "Export model and animation to COLLADA DAE";
            this.btnExportDAE.Click += new System.EventHandler(this.btnExportToDAE_Click);
            // 
            // AnimationEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 465);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AnimationEditorForm";
            this.Text = "GO GO ANIMATION EDITOR GO!";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            //((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
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
        private System.Windows.Forms.ToolStripButton btnExportDAE;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBMDName;
        private System.Windows.Forms.Button btnOpenBMD;
        private System.Windows.Forms.Button btnOpenBCA;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBCAName;
        private System.Windows.Forms.Button btnStopAnimation;
        private System.Windows.Forms.Button btnPlayAnimation;
        private System.Windows.Forms.CheckBox chkLoopAnimation;
        private System.Windows.Forms.Button btnLastFrame;
        private System.Windows.Forms.Button btnNextFrame;
        private System.Windows.Forms.Button btnPreviousFrame;
        private System.Windows.Forms.Button btnFirstFrame;
        private System.Windows.Forms.TextBox txtNumFrames;
        private System.Windows.Forms.Label lblFrameNum;
        private System.Windows.Forms.TextBox txtCurrentFrameNum;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ToolStripButton btnImportDAE;
    }
}