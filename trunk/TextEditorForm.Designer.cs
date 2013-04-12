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
            this.btnSaveAll = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbLanguages = new System.Windows.Forms.ComboBox();
            this.lblVer = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCoins = new System.Windows.Forms.Button();
            this.btnR = new System.Windows.Forms.Button();
            this.btnL = new System.Windows.Forms.Button();
            this.btnY = new System.Windows.Forms.Button();
            this.btnX = new System.Windows.Forms.Button();
            this.btnB = new System.Windows.Forms.Button();
            this.btnA = new System.Windows.Forms.Button();
            this.btnDPad = new System.Windows.Forms.Button();
            this.btnStarEmpty = new System.Windows.Forms.Button();
            this.btnStarFull = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnUpdateString = new System.Windows.Forms.Button();
            this.txtEdit = new System.Windows.Forms.TextBox();
            this.tbxMsgPreview = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
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
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.btnSaveAll);
            this.splitContainer1.Panel2.Controls.Add(this.btnHelp);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.cmbLanguages);
            this.splitContainer1.Panel2.Controls.Add(this.lblVer);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.btnCoins);
            this.splitContainer1.Panel2.Controls.Add(this.btnR);
            this.splitContainer1.Panel2.Controls.Add(this.btnL);
            this.splitContainer1.Panel2.Controls.Add(this.btnY);
            this.splitContainer1.Panel2.Controls.Add(this.btnX);
            this.splitContainer1.Panel2.Controls.Add(this.btnB);
            this.splitContainer1.Panel2.Controls.Add(this.btnA);
            this.splitContainer1.Panel2.Controls.Add(this.btnDPad);
            this.splitContainer1.Panel2.Controls.Add(this.btnStarEmpty);
            this.splitContainer1.Panel2.Controls.Add(this.btnStarFull);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.btnUpdateString);
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
            // btnSaveAll
            // 
            this.btnSaveAll.Location = new System.Drawing.Point(3, 480);
            this.btnSaveAll.Name = "btnSaveAll";
            this.btnSaveAll.Size = new System.Drawing.Size(103, 23);
            this.btnSaveAll.TabIndex = 20;
            this.btnSaveAll.Text = "Save Changes";
            this.btnSaveAll.UseVisualStyleBackColor = true;
            this.btnSaveAll.Click += new System.EventHandler(this.btnSaveAll_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(380, -1);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(56, 23);
            this.btnHelp.TabIndex = 19;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(150, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Language:";
            // 
            // cmbLanguages
            // 
            this.cmbLanguages.FormattingEnabled = true;
            this.cmbLanguages.Location = new System.Drawing.Point(214, 1);
            this.cmbLanguages.Name = "cmbLanguages";
            this.cmbLanguages.Size = new System.Drawing.Size(150, 21);
            this.cmbLanguages.TabIndex = 17;
            this.cmbLanguages.Text = "Select a Language";
            this.cmbLanguages.SelectedIndexChanged += new System.EventHandler(this.cmbLanguages_SelectedIndexChanged);
            // 
            // lblVer
            // 
            this.lblVer.AutoSize = true;
            this.lblVer.Location = new System.Drawing.Point(83, 3);
            this.lblVer.Name = "lblVer";
            this.lblVer.Size = new System.Drawing.Size(23, 13);
            this.lblVer.TabIndex = 16;
            this.lblVer.Text = "Ver";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Rom Version:";
            // 
            // btnCoins
            // 
            this.btnCoins.Image = global::SM64DSe.Properties.Resources.Coins1;
            this.btnCoins.Location = new System.Drawing.Point(0, 409);
            this.btnCoins.Name = "btnCoins";
            this.btnCoins.Size = new System.Drawing.Size(35, 30);
            this.btnCoins.TabIndex = 14;
            this.btnCoins.UseVisualStyleBackColor = true;
            this.btnCoins.Click += new System.EventHandler(this.btnCoins_Click);
            // 
            // btnR
            // 
            this.btnR.Enabled = false;
            this.btnR.Image = global::SM64DSe.Properties.Resources.btnR1;
            this.btnR.Location = new System.Drawing.Point(349, 409);
            this.btnR.Name = "btnR";
            this.btnR.Size = new System.Drawing.Size(35, 30);
            this.btnR.TabIndex = 13;
            this.btnR.UseVisualStyleBackColor = true;
            this.btnR.Visible = false;
            this.btnR.Click += new System.EventHandler(this.btnR_Click);
            // 
            // btnL
            // 
            this.btnL.Enabled = false;
            this.btnL.Image = global::SM64DSe.Properties.Resources.btnL1;
            this.btnL.Location = new System.Drawing.Point(310, 409);
            this.btnL.Name = "btnL";
            this.btnL.Size = new System.Drawing.Size(35, 30);
            this.btnL.TabIndex = 12;
            this.btnL.UseVisualStyleBackColor = true;
            this.btnL.Visible = false;
            this.btnL.Click += new System.EventHandler(this.btnL_Click);
            // 
            // btnY
            // 
            this.btnY.Image = global::SM64DSe.Properties.Resources.btnY1;
            this.btnY.Location = new System.Drawing.Point(271, 409);
            this.btnY.Name = "btnY";
            this.btnY.Size = new System.Drawing.Size(35, 30);
            this.btnY.TabIndex = 11;
            this.btnY.UseVisualStyleBackColor = true;
            this.btnY.Click += new System.EventHandler(this.btnY_Click);
            // 
            // btnX
            // 
            this.btnX.Image = global::SM64DSe.Properties.Resources.btnX1;
            this.btnX.Location = new System.Drawing.Point(232, 409);
            this.btnX.Name = "btnX";
            this.btnX.Size = new System.Drawing.Size(35, 30);
            this.btnX.TabIndex = 10;
            this.btnX.UseVisualStyleBackColor = true;
            this.btnX.Click += new System.EventHandler(this.btnX_Click);
            // 
            // btnB
            // 
            this.btnB.Image = global::SM64DSe.Properties.Resources.btnB1;
            this.btnB.Location = new System.Drawing.Point(193, 409);
            this.btnB.Name = "btnB";
            this.btnB.Size = new System.Drawing.Size(35, 30);
            this.btnB.TabIndex = 9;
            this.btnB.UseVisualStyleBackColor = true;
            this.btnB.Click += new System.EventHandler(this.btnB_Click);
            // 
            // btnA
            // 
            this.btnA.Image = global::SM64DSe.Properties.Resources.btnA1;
            this.btnA.Location = new System.Drawing.Point(154, 409);
            this.btnA.Name = "btnA";
            this.btnA.Size = new System.Drawing.Size(35, 30);
            this.btnA.TabIndex = 8;
            this.btnA.UseVisualStyleBackColor = true;
            this.btnA.Click += new System.EventHandler(this.btnA_Click);
            // 
            // btnDPad
            // 
            this.btnDPad.Image = global::SM64DSe.Properties.Resources.DPad1;
            this.btnDPad.Location = new System.Drawing.Point(115, 409);
            this.btnDPad.Name = "btnDPad";
            this.btnDPad.Size = new System.Drawing.Size(35, 30);
            this.btnDPad.TabIndex = 7;
            this.btnDPad.UseVisualStyleBackColor = true;
            this.btnDPad.Click += new System.EventHandler(this.btnDPad_Click);
            // 
            // btnStarEmpty
            // 
            this.btnStarEmpty.Image = global::SM64DSe.Properties.Resources.StarEmpty1;
            this.btnStarEmpty.Location = new System.Drawing.Point(76, 409);
            this.btnStarEmpty.Name = "btnStarEmpty";
            this.btnStarEmpty.Size = new System.Drawing.Size(35, 30);
            this.btnStarEmpty.TabIndex = 6;
            this.btnStarEmpty.UseVisualStyleBackColor = true;
            this.btnStarEmpty.Click += new System.EventHandler(this.btnStarEmpty_Click);
            // 
            // btnStarFull
            // 
            this.btnStarFull.Image = global::SM64DSe.Properties.Resources.StarFull1;
            this.btnStarFull.Location = new System.Drawing.Point(39, 409);
            this.btnStarFull.Name = "btnStarFull";
            this.btnStarFull.Size = new System.Drawing.Size(35, 30);
            this.btnStarFull.TabIndex = 5;
            this.btnStarFull.UseVisualStyleBackColor = true;
            this.btnStarFull.Click += new System.EventHandler(this.btnStarFull_Click);
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
            // btnUpdateString
            // 
            this.btnUpdateString.Location = new System.Drawing.Point(3, 449);
            this.btnUpdateString.Name = "btnUpdateString";
            this.btnUpdateString.Size = new System.Drawing.Size(103, 23);
            this.btnUpdateString.TabIndex = 2;
            this.btnUpdateString.Text = "Update String";
            this.btnUpdateString.UseVisualStyleBackColor = true;
            this.btnUpdateString.Click += new System.EventHandler(this.btnUpdateString_Click);
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
            this.tbxMsgPreview.Location = new System.Drawing.Point(0, 26);
            this.tbxMsgPreview.Multiline = true;
            this.tbxMsgPreview.Name = "tbxMsgPreview";
            this.tbxMsgPreview.ReadOnly = true;
            this.tbxMsgPreview.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxMsgPreview.Size = new System.Drawing.Size(448, 243);
            this.tbxMsgPreview.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(112, 454);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(268, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Click this when you\'ve finished editing the current entry.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(112, 485);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(207, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "Click this to permanently save all changes.";
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lbxMsgList;
        private System.Windows.Forms.TextBox tbxMsgPreview;
        private System.Windows.Forms.TextBox txtEdit;
        private System.Windows.Forms.Button btnUpdateString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStarEmpty;
        private System.Windows.Forms.Button btnStarFull;
        private System.Windows.Forms.Button btnY;
        private System.Windows.Forms.Button btnX;
        private System.Windows.Forms.Button btnB;
        private System.Windows.Forms.Button btnA;
        private System.Windows.Forms.Button btnDPad;
        private System.Windows.Forms.Button btnR;
        private System.Windows.Forms.Button btnL;
        private System.Windows.Forms.Button btnCoins;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblVer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbLanguages;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnSaveAll;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
    }
}