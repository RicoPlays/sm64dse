namespace SM64DSe
{
    partial class TextureAnimationForm
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
            this.lbxArea = new System.Windows.Forms.ListBox();
            this.lbxTexAnim = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbxScale = new System.Windows.Forms.ListBox();
            this.lbxRotation = new System.Windows.Forms.ListBox();
            this.lbxTranslation = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtScale = new System.Windows.Forms.TextBox();
            this.txtRotation = new System.Windows.Forms.TextBox();
            this.txtTranslation = new System.Windows.Forms.TextBox();
            this.txtMaterialName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnRemoveAll = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnSaveCurrent = new System.Windows.Forms.Button();
            this.btnAddScale = new System.Windows.Forms.Button();
            this.btnRemScale = new System.Windows.Forms.Button();
            this.btnAddRot = new System.Windows.Forms.Button();
            this.btnRemRot = new System.Windows.Forms.Button();
            this.btnAddTrans = new System.Windows.Forms.Button();
            this.btnRemTrans = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtScaleStart = new System.Windows.Forms.TextBox();
            this.txtScaleSize = new System.Windows.Forms.TextBox();
            this.txtRotStart = new System.Windows.Forms.TextBox();
            this.txtRotSize = new System.Windows.Forms.TextBox();
            this.txtTransStart = new System.Windows.Forms.TextBox();
            this.txtTransSize = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.cbSaveOvl = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lbxArea
            // 
            this.lbxArea.FormattingEnabled = true;
            this.lbxArea.Location = new System.Drawing.Point(13, 30);
            this.lbxArea.Name = "lbxArea";
            this.lbxArea.Size = new System.Drawing.Size(120, 95);
            this.lbxArea.TabIndex = 0;
            this.lbxArea.SelectedIndexChanged += new System.EventHandler(this.lbxArea_SelectedIndexChanged);
            // 
            // lbxTexAnim
            // 
            this.lbxTexAnim.FormattingEnabled = true;
            this.lbxTexAnim.Location = new System.Drawing.Point(13, 150);
            this.lbxTexAnim.Name = "lbxTexAnim";
            this.lbxTexAnim.Size = new System.Drawing.Size(120, 95);
            this.lbxTexAnim.TabIndex = 1;
            this.lbxTexAnim.SelectedIndexChanged += new System.EventHandler(this.lbxTexAnim_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(159, 246);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Material Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(159, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Scale Values";
            // 
            // lbxScale
            // 
            this.lbxScale.FormattingEnabled = true;
            this.lbxScale.Location = new System.Drawing.Point(162, 30);
            this.lbxScale.Name = "lbxScale";
            this.lbxScale.Size = new System.Drawing.Size(150, 95);
            this.lbxScale.TabIndex = 4;
            this.lbxScale.SelectedIndexChanged += new System.EventHandler(this.lbxScale_SelectedIndexChanged);
            // 
            // lbxRotation
            // 
            this.lbxRotation.FormattingEnabled = true;
            this.lbxRotation.Location = new System.Drawing.Point(318, 30);
            this.lbxRotation.Name = "lbxRotation";
            this.lbxRotation.Size = new System.Drawing.Size(150, 95);
            this.lbxRotation.TabIndex = 5;
            this.lbxRotation.SelectedIndexChanged += new System.EventHandler(this.lbxRotation_SelectedIndexChanged);
            // 
            // lbxTranslation
            // 
            this.lbxTranslation.FormattingEnabled = true;
            this.lbxTranslation.Location = new System.Drawing.Point(474, 30);
            this.lbxTranslation.Name = "lbxTranslation";
            this.lbxTranslation.Size = new System.Drawing.Size(150, 95);
            this.lbxTranslation.TabIndex = 6;
            this.lbxTranslation.SelectedIndexChanged += new System.EventHandler(this.lbxTranslation_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(315, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Rotation Values";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(471, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Translation Values";
            // 
            // txtScale
            // 
            this.txtScale.Location = new System.Drawing.Point(162, 203);
            this.txtScale.Name = "txtScale";
            this.txtScale.Size = new System.Drawing.Size(90, 20);
            this.txtScale.TabIndex = 9;
            this.txtScale.TextChanged += new System.EventHandler(this.txtScale_TextChanged);
            // 
            // txtRotation
            // 
            this.txtRotation.Location = new System.Drawing.Point(318, 203);
            this.txtRotation.Name = "txtRotation";
            this.txtRotation.Size = new System.Drawing.Size(90, 20);
            this.txtRotation.TabIndex = 10;
            this.txtRotation.TextChanged += new System.EventHandler(this.txtRotation_TextChanged);
            // 
            // txtTranslation
            // 
            this.txtTranslation.Location = new System.Drawing.Point(474, 203);
            this.txtTranslation.Name = "txtTranslation";
            this.txtTranslation.Size = new System.Drawing.Size(90, 20);
            this.txtTranslation.TabIndex = 11;
            this.txtTranslation.TextChanged += new System.EventHandler(this.txtTranslation_TextChanged);
            // 
            // txtMaterialName
            // 
            this.txtMaterialName.Location = new System.Drawing.Point(253, 243);
            this.txtMaterialName.Name = "txtMaterialName";
            this.txtMaterialName.Size = new System.Drawing.Size(120, 20);
            this.txtMaterialName.TabIndex = 12;
            this.txtMaterialName.TextChanged += new System.EventHandler(this.txtMaterialName_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Area";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 134);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Texture Animation";
            // 
            // btnRemoveAll
            // 
            this.btnRemoveAll.Location = new System.Drawing.Point(13, 289);
            this.btnRemoveAll.Name = "btnRemoveAll";
            this.btnRemoveAll.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveAll.TabIndex = 15;
            this.btnRemoveAll.Text = "Remove All";
            this.btnRemoveAll.UseVisualStyleBackColor = true;
            this.btnRemoveAll.Click += new System.EventHandler(this.btnRemoveAll_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemove.Location = new System.Drawing.Point(13, 252);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(23, 23);
            this.btnRemove.TabIndex = 18;
            this.btnRemove.Text = "-";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(42, 252);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(23, 23);
            this.btnAdd.TabIndex = 19;
            this.btnAdd.Text = "+";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnSaveCurrent
            // 
            this.btnSaveCurrent.Location = new System.Drawing.Point(162, 289);
            this.btnSaveCurrent.Name = "btnSaveCurrent";
            this.btnSaveCurrent.Size = new System.Drawing.Size(90, 23);
            this.btnSaveCurrent.TabIndex = 20;
            this.btnSaveCurrent.Text = "Save Changes";
            this.btnSaveCurrent.UseVisualStyleBackColor = true;
            this.btnSaveCurrent.Click += new System.EventHandler(this.btnSaveCurrent_Click);
            // 
            // btnAddScale
            // 
            this.btnAddScale.Location = new System.Drawing.Point(289, 201);
            this.btnAddScale.Name = "btnAddScale";
            this.btnAddScale.Size = new System.Drawing.Size(23, 23);
            this.btnAddScale.TabIndex = 22;
            this.btnAddScale.Text = "+";
            this.btnAddScale.UseVisualStyleBackColor = true;
            this.btnAddScale.Click += new System.EventHandler(this.btnAddScale_Click);
            // 
            // btnRemScale
            // 
            this.btnRemScale.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemScale.Location = new System.Drawing.Point(260, 201);
            this.btnRemScale.Name = "btnRemScale";
            this.btnRemScale.Size = new System.Drawing.Size(23, 23);
            this.btnRemScale.TabIndex = 21;
            this.btnRemScale.Text = "-";
            this.btnRemScale.UseVisualStyleBackColor = true;
            this.btnRemScale.Click += new System.EventHandler(this.btnRemScale_Click);
            // 
            // btnAddRot
            // 
            this.btnAddRot.Location = new System.Drawing.Point(445, 200);
            this.btnAddRot.Name = "btnAddRot";
            this.btnAddRot.Size = new System.Drawing.Size(23, 23);
            this.btnAddRot.TabIndex = 24;
            this.btnAddRot.Text = "+";
            this.btnAddRot.UseVisualStyleBackColor = true;
            this.btnAddRot.Click += new System.EventHandler(this.btnAddRot_Click);
            // 
            // btnRemRot
            // 
            this.btnRemRot.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemRot.Location = new System.Drawing.Point(416, 200);
            this.btnRemRot.Name = "btnRemRot";
            this.btnRemRot.Size = new System.Drawing.Size(23, 23);
            this.btnRemRot.TabIndex = 23;
            this.btnRemRot.Text = "-";
            this.btnRemRot.UseVisualStyleBackColor = true;
            this.btnRemRot.Click += new System.EventHandler(this.btnRemRot_Click);
            // 
            // btnAddTrans
            // 
            this.btnAddTrans.Location = new System.Drawing.Point(601, 201);
            this.btnAddTrans.Name = "btnAddTrans";
            this.btnAddTrans.Size = new System.Drawing.Size(23, 23);
            this.btnAddTrans.TabIndex = 26;
            this.btnAddTrans.Text = "+";
            this.btnAddTrans.UseVisualStyleBackColor = true;
            this.btnAddTrans.Click += new System.EventHandler(this.btnAddTrans_Click);
            // 
            // btnRemTrans
            // 
            this.btnRemTrans.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemTrans.Location = new System.Drawing.Point(572, 201);
            this.btnRemTrans.Name = "btnRemTrans";
            this.btnRemTrans.Size = new System.Drawing.Size(23, 23);
            this.btnRemTrans.TabIndex = 25;
            this.btnRemTrans.Text = "-";
            this.btnRemTrans.UseVisualStyleBackColor = true;
            this.btnRemTrans.Click += new System.EventHandler(this.btnRemTrans_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(159, 153);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 27;
            this.label7.Text = "Start Index";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(159, 177);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 28;
            this.label8.Text = "Length";
            // 
            // txtScaleStart
            // 
            this.txtScaleStart.Location = new System.Drawing.Point(253, 150);
            this.txtScaleStart.Name = "txtScaleStart";
            this.txtScaleStart.Size = new System.Drawing.Size(59, 20);
            this.txtScaleStart.TabIndex = 29;
            this.txtScaleStart.TextChanged += new System.EventHandler(this.txtScaleStart_TextChanged);
            // 
            // txtScaleSize
            // 
            this.txtScaleSize.Location = new System.Drawing.Point(253, 170);
            this.txtScaleSize.Name = "txtScaleSize";
            this.txtScaleSize.Size = new System.Drawing.Size(59, 20);
            this.txtScaleSize.TabIndex = 30;
            this.txtScaleSize.TextChanged += new System.EventHandler(this.txtScaleSize_TextChanged);
            // 
            // txtRotStart
            // 
            this.txtRotStart.Location = new System.Drawing.Point(409, 150);
            this.txtRotStart.Name = "txtRotStart";
            this.txtRotStart.Size = new System.Drawing.Size(59, 20);
            this.txtRotStart.TabIndex = 31;
            this.txtRotStart.TextChanged += new System.EventHandler(this.txtRotStart_TextChanged);
            // 
            // txtRotSize
            // 
            this.txtRotSize.Location = new System.Drawing.Point(409, 170);
            this.txtRotSize.Name = "txtRotSize";
            this.txtRotSize.Size = new System.Drawing.Size(59, 20);
            this.txtRotSize.TabIndex = 32;
            this.txtRotSize.TextChanged += new System.EventHandler(this.txtRotSize_TextChanged);
            // 
            // txtTransStart
            // 
            this.txtTransStart.Location = new System.Drawing.Point(565, 150);
            this.txtTransStart.Name = "txtTransStart";
            this.txtTransStart.Size = new System.Drawing.Size(59, 20);
            this.txtTransStart.TabIndex = 33;
            this.txtTransStart.TextChanged += new System.EventHandler(this.txtTransStart_TextChanged);
            // 
            // txtTransSize
            // 
            this.txtTransSize.Location = new System.Drawing.Point(565, 170);
            this.txtTransSize.Name = "txtTransSize";
            this.txtTransSize.Size = new System.Drawing.Size(59, 20);
            this.txtTransSize.TabIndex = 34;
            this.txtTransSize.TextChanged += new System.EventHandler(this.txtTransSize_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(318, 177);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(40, 13);
            this.label9.TabIndex = 36;
            this.label9.Text = "Length";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(318, 153);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(58, 13);
            this.label10.TabIndex = 35;
            this.label10.Text = "Start Index";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(474, 177);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 13);
            this.label11.TabIndex = 38;
            this.label11.Text = "Length";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(474, 153);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(58, 13);
            this.label12.TabIndex = 37;
            this.label12.Text = "Start Index";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Enabled = false;
            this.label13.Location = new System.Drawing.Point(406, 246);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(198, 13);
            this.label13.TabIndex = 39;
            this.label13.Text = "tbloffset + 0, m_offset + 0, + 8, + 24, +26";
            this.label13.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(409, 263);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(195, 20);
            this.textBox1.TabIndex = 40;
            this.textBox1.Visible = false;
            // 
            // cbSaveOvl
            // 
            this.cbSaveOvl.AutoSize = true;
            this.cbSaveOvl.Location = new System.Drawing.Point(253, 293);
            this.cbSaveOvl.Name = "cbSaveOvl";
            this.cbSaveOvl.Size = new System.Drawing.Size(138, 17);
            this.cbSaveOvl.TabIndex = 41;
            this.cbSaveOvl.Text = "Permanent (Only if sure)";
            this.cbSaveOvl.UseVisualStyleBackColor = true;
            // 
            // TextureAnimationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 324);
            this.Controls.Add(this.cbSaveOvl);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtTransSize);
            this.Controls.Add(this.txtTransStart);
            this.Controls.Add(this.txtRotSize);
            this.Controls.Add(this.txtRotStart);
            this.Controls.Add(this.txtScaleSize);
            this.Controls.Add(this.txtScaleStart);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnAddTrans);
            this.Controls.Add(this.btnRemTrans);
            this.Controls.Add(this.btnAddRot);
            this.Controls.Add(this.btnRemRot);
            this.Controls.Add(this.btnAddScale);
            this.Controls.Add(this.btnRemScale);
            this.Controls.Add(this.btnSaveCurrent);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnRemoveAll);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtMaterialName);
            this.Controls.Add(this.txtTranslation);
            this.Controls.Add(this.txtRotation);
            this.Controls.Add(this.txtScale);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbxTranslation);
            this.Controls.Add(this.lbxRotation);
            this.Controls.Add(this.lbxScale);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbxTexAnim);
            this.Controls.Add(this.lbxArea);
            this.Name = "TextureAnimationForm";
            this.Text = "TextureAnimationForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbxArea;
        private System.Windows.Forms.ListBox lbxTexAnim;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbxScale;
        private System.Windows.Forms.ListBox lbxRotation;
        private System.Windows.Forms.ListBox lbxTranslation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtScale;
        private System.Windows.Forms.TextBox txtRotation;
        private System.Windows.Forms.TextBox txtTranslation;
        private System.Windows.Forms.TextBox txtMaterialName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnRemoveAll;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnSaveCurrent;
        private System.Windows.Forms.Button btnAddScale;
        private System.Windows.Forms.Button btnRemScale;
        private System.Windows.Forms.Button btnAddRot;
        private System.Windows.Forms.Button btnRemRot;
        private System.Windows.Forms.Button btnAddTrans;
        private System.Windows.Forms.Button btnRemTrans;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtScaleStart;
        private System.Windows.Forms.TextBox txtScaleSize;
        private System.Windows.Forms.TextBox txtRotStart;
        private System.Windows.Forms.TextBox txtRotSize;
        private System.Windows.Forms.TextBox txtTransStart;
        private System.Windows.Forms.TextBox txtTransSize;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox cbSaveOvl;
    }
}