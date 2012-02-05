using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SM64DSe
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            cbAutoUpdateODB.Checked = Properties.Settings.Default.AutoUpdateODB;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoUpdateODB = cbAutoUpdateODB.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
