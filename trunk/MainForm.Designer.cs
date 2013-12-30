namespace SM64DSe
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tsToolBar = new System.Windows.Forms.ToolStrip();
            this.btnOpenROM = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnEditLevel = new System.Windows.Forms.ToolStripButton();
            this.btnEditTexts = new System.Windows.Forms.ToolStripButton();
            this.btnSecretShit = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnDumpObjInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.btnObjFinder = new System.Windows.Forms.ToolStripMenuItem();
            this.mnitDumpAllOvls = new System.Windows.Forms.ToolStripMenuItem();
            this.decompressOverlaysWithinGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnKCLEditor = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAddPatches = new System.Windows.Forms.ToolStripButton();
            this.btnOptions = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnUpdateODB = new System.Windows.Forms.ToolStripMenuItem();
            this.btnEditorSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.btnHalp = new System.Windows.Forms.ToolStripButton();
            this.lbxLevels = new System.Windows.Forms.ListBox();
            this.ofdOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.sfdSaveFile = new System.Windows.Forms.SaveFileDialog();
            this.ssStatusBar = new System.Windows.Forms.StatusStrip();
            this.slStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.spbStatusProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.hexDumpToBinaryFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsToolBar.SuspendLayout();
            this.ssStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsToolBar
            // 
            this.tsToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenROM,
            this.toolStripSeparator1,
            this.btnEditLevel,
            this.btnEditTexts,
            this.btnSecretShit,
            this.btnKCLEditor,
            this.toolStripSeparator2,
            this.btnAddPatches,
            this.btnOptions,
            this.btnHalp});
            this.tsToolBar.Location = new System.Drawing.Point(0, 0);
            this.tsToolBar.Name = "tsToolBar";
            this.tsToolBar.Size = new System.Drawing.Size(496, 25);
            this.tsToolBar.TabIndex = 0;
            this.tsToolBar.Text = "toolStrip1";
            // 
            // btnOpenROM
            // 
            this.btnOpenROM.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnOpenROM.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenROM.Image")));
            this.btnOpenROM.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpenROM.Name = "btnOpenROM";
            this.btnOpenROM.Size = new System.Drawing.Size(79, 22);
            this.btnOpenROM.Text = "Open ROM...";
            this.btnOpenROM.ToolTipText = "what it says";
            this.btnOpenROM.Click += new System.EventHandler(this.btnOpenROM_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnEditLevel
            // 
            this.btnEditLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnEditLevel.Enabled = false;
            this.btnEditLevel.Image = ((System.Drawing.Image)(resources.GetObject("btnEditLevel.Image")));
            this.btnEditLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditLevel.Name = "btnEditLevel";
            this.btnEditLevel.Size = new System.Drawing.Size(58, 22);
            this.btnEditLevel.Text = "Edit level";
            this.btnEditLevel.ToolTipText = "let the fun begin!";
            this.btnEditLevel.Click += new System.EventHandler(this.btnEditLevel_Click);
            // 
            // btnEditTexts
            // 
            this.btnEditTexts.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnEditTexts.Enabled = false;
            this.btnEditTexts.Image = ((System.Drawing.Image)(resources.GetObject("btnEditTexts.Image")));
            this.btnEditTexts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditTexts.Name = "btnEditTexts";
            this.btnEditTexts.Size = new System.Drawing.Size(58, 22);
            this.btnEditTexts.Text = "Edit texts";
            this.btnEditTexts.ToolTipText = "change what signs say";
            this.btnEditTexts.Click += new System.EventHandler(this.btnEditTexts_Click);
            // 
            // btnSecretShit
            // 
            this.btnSecretShit.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnSecretShit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSecretShit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnDumpObjInfo,
            this.btnObjFinder,
            this.mnitDumpAllOvls,
            this.decompressOverlaysWithinGameToolStripMenuItem,
            this.hexDumpToBinaryFileToolStripMenuItem});
            this.btnSecretShit.Image = ((System.Drawing.Image)(resources.GetObject("btnSecretShit.Image")));
            this.btnSecretShit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSecretShit.Name = "btnSecretShit";
            this.btnSecretShit.Size = new System.Drawing.Size(28, 22);
            this.btnSecretShit.Text = "D";
            this.btnSecretShit.ToolTipText = "Debug crap. Don\'t touch unless you know what you\'re doing.";
            // 
            // btnDumpObjInfo
            // 
            this.btnDumpObjInfo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnDumpObjInfo.Image = ((System.Drawing.Image)(resources.GetObject("btnDumpObjInfo.Image")));
            this.btnDumpObjInfo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDumpObjInfo.Name = "btnDumpObjInfo";
            this.btnDumpObjInfo.Size = new System.Drawing.Size(259, 22);
            this.btnDumpObjInfo.Text = "Dump object info ";
            // 
            // btnObjFinder
            // 
            this.btnObjFinder.Name = "btnObjFinder";
            this.btnObjFinder.Size = new System.Drawing.Size(259, 22);
            this.btnObjFinder.Text = "Object finder";
            // 
            // mnitDumpAllOvls
            // 
            this.mnitDumpAllOvls.Name = "mnitDumpAllOvls";
            this.mnitDumpAllOvls.Size = new System.Drawing.Size(259, 22);
            this.mnitDumpAllOvls.Text = "Dump All Overlays";
            this.mnitDumpAllOvls.Click += new System.EventHandler(this.mnitDumpAllOvls_Click);
            // 
            // decompressOverlaysWithinGameToolStripMenuItem
            // 
            this.decompressOverlaysWithinGameToolStripMenuItem.Name = "decompressOverlaysWithinGameToolStripMenuItem";
            this.decompressOverlaysWithinGameToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.decompressOverlaysWithinGameToolStripMenuItem.Text = "Decompress Overlays Within Game";
            this.decompressOverlaysWithinGameToolStripMenuItem.Click += new System.EventHandler(this.decompressOverlaysWithinGameToolStripMenuItem_Click);
            // 
            // btnKCLEditor
            // 
            this.btnKCLEditor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnKCLEditor.Enabled = false;
            this.btnKCLEditor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnKCLEditor.Name = "btnKCLEditor";
            this.btnKCLEditor.Size = new System.Drawing.Size(55, 22);
            this.btnKCLEditor.Text = "Edit KCL";
            this.btnKCLEditor.Click += new System.EventHandler(this.btnKCLEditor_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnAddPatches
            // 
            this.btnAddPatches.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddPatches.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddPatches.Name = "btnAddPatches";
            this.btnAddPatches.Size = new System.Drawing.Size(110, 22);
            this.btnAddPatches.Text = "Additional Patches";
            this.btnAddPatches.Click += new System.EventHandler(this.btnAddPatches_Click);
            // 
            // btnOptions
            // 
            this.btnOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnUpdateODB,
            this.btnEditorSettings});
            this.btnOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnOptions.Image")));
            this.btnOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(62, 22);
            this.btnOptions.Text = "Options";
            this.btnOptions.ToolTipText = "configure the editor";
            // 
            // btnUpdateODB
            // 
            this.btnUpdateODB.Name = "btnUpdateODB";
            this.btnUpdateODB.Size = new System.Drawing.Size(198, 22);
            this.btnUpdateODB.Text = "Update object database";
            this.btnUpdateODB.Click += new System.EventHandler(this.btnUpdateODB_Click);
            // 
            // btnEditorSettings
            // 
            this.btnEditorSettings.Name = "btnEditorSettings";
            this.btnEditorSettings.Size = new System.Drawing.Size(198, 22);
            this.btnEditorSettings.Text = "Editor settings";
            this.btnEditorSettings.Click += new System.EventHandler(this.btnEditorSettings_Click);
            // 
            // btnHalp
            // 
            this.btnHalp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnHalp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnHalp.Image = ((System.Drawing.Image)(resources.GetObject("btnHalp.Image")));
            this.btnHalp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHalp.Name = "btnHalp";
            this.btnHalp.Size = new System.Drawing.Size(23, 19);
            this.btnHalp.Text = "?";
            this.btnHalp.ToolTipText = "Help, about, etc...";
            this.btnHalp.Click += new System.EventHandler(this.btnHalp_Click);
            // 
            // lbxLevels
            // 
            this.lbxLevels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxLevels.FormattingEnabled = true;
            this.lbxLevels.IntegralHeight = false;
            this.lbxLevels.Location = new System.Drawing.Point(0, 25);
            this.lbxLevels.Margin = new System.Windows.Forms.Padding(0);
            this.lbxLevels.Name = "lbxLevels";
            this.lbxLevels.Size = new System.Drawing.Size(496, 388);
            this.lbxLevels.TabIndex = 1;
            this.lbxLevels.SelectedIndexChanged += new System.EventHandler(this.lbxLevels_SelectedIndexChanged);
            this.lbxLevels.DoubleClick += new System.EventHandler(this.lbxLevels_DoubleClick);
            // 
            // ofdOpenFile
            // 
            this.ofdOpenFile.DefaultExt = "nds";
            this.ofdOpenFile.Filter = "Nintendo DS ROM|*.nds|Any file|*.*";
            this.ofdOpenFile.Title = "Open ROM...";
            // 
            // sfdSaveFile
            // 
            this.sfdSaveFile.DefaultExt = "nds";
            this.sfdSaveFile.Filter = "Nintendo DS ROM|*.nds|Any file|*.*";
            this.sfdSaveFile.RestoreDirectory = true;
            this.sfdSaveFile.Title = "Backup ROM...";
            // 
            // ssStatusBar
            // 
            this.ssStatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slStatusLabel,
            this.spbStatusProgress});
            this.ssStatusBar.Location = new System.Drawing.Point(0, 413);
            this.ssStatusBar.Name = "ssStatusBar";
            this.ssStatusBar.Size = new System.Drawing.Size(496, 22);
            this.ssStatusBar.TabIndex = 2;
            this.ssStatusBar.Text = "statusStrip1";
            // 
            // slStatusLabel
            // 
            this.slStatusLabel.Name = "slStatusLabel";
            this.slStatusLabel.Size = new System.Drawing.Size(51, 17);
            this.slStatusLabel.Text = "lolstatus";
            // 
            // spbStatusProgress
            // 
            this.spbStatusProgress.Name = "spbStatusProgress";
            this.spbStatusProgress.Size = new System.Drawing.Size(150, 16);
            this.spbStatusProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.spbStatusProgress.Visible = false;
            // 
            // hexDumpToBinaryFileToolStripMenuItem
            // 
            this.hexDumpToBinaryFileToolStripMenuItem.Name = "hexDumpToBinaryFileToolStripMenuItem";
            this.hexDumpToBinaryFileToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.hexDumpToBinaryFileToolStripMenuItem.Text = "Hex Dump to Binary File";
            this.hexDumpToBinaryFileToolStripMenuItem.Click += new System.EventHandler(this.hexDumpToBinaryFileToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 435);
            this.Controls.Add(this.ssStatusBar);
            this.Controls.Add(this.lbxLevels);
            this.Controls.Add(this.tsToolBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "SM64DS Editor vlol";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.tsToolBar.ResumeLayout(false);
            this.tsToolBar.PerformLayout();
            this.ssStatusBar.ResumeLayout(false);
            this.ssStatusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsToolBar;
        private System.Windows.Forms.ListBox lbxLevels;
        private System.Windows.Forms.ToolStripButton btnOpenROM;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnEditLevel;
        private System.Windows.Forms.ToolStripButton btnEditTexts;
        private System.Windows.Forms.OpenFileDialog ofdOpenFile;
        private System.Windows.Forms.SaveFileDialog sfdSaveFile;
        private System.Windows.Forms.ToolStripDropDownButton btnSecretShit;
        private System.Windows.Forms.ToolStripMenuItem btnDumpObjInfo;
        private System.Windows.Forms.ToolStripMenuItem btnObjFinder;
        private System.Windows.Forms.StatusStrip ssStatusBar;
        private System.Windows.Forms.ToolStripStatusLabel slStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar spbStatusProgress;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton btnOptions;
        private System.Windows.Forms.ToolStripMenuItem btnUpdateODB;
        private System.Windows.Forms.ToolStripMenuItem btnEditorSettings;
        private System.Windows.Forms.ToolStripButton btnHalp;
        private System.Windows.Forms.ToolStripMenuItem mnitDumpAllOvls;
        private System.Windows.Forms.ToolStripButton btnKCLEditor;
        private System.Windows.Forms.ToolStripButton btnAddPatches;
        private System.Windows.Forms.ToolStripMenuItem decompressOverlaysWithinGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hexDumpToBinaryFileToolStripMenuItem;
    }
}

