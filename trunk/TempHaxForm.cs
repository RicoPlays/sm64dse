using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace SM64DSe
{
    public partial class TempHaxForm : Form
    {
        private int nfailed = 0;

        public TempHaxForm()
        {
            InitializeComponent();
        }

        private string HexString(byte[] crap)
        {
            string ret = "";
            foreach (byte b in crap)
                ret += b.ToString("x2");
            return ret;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // wee gratuitous obfuscation
            ASCIIEncoding enc = new ASCIIEncoding();
            string rofl = tbxLOL.Text.Trim().ToLowerInvariant() + "Yeah, this ass-long salt is here so that you can't just take the MD5 and feed it into an MD5 decrypter, you cheap cheater";
            MD5 lol = MD5.Create();
            string hash = HexString(lol.ComputeHash(enc.GetBytes(rofl)));
            if (hash != "d4f40b24d6c68595c7ff577e5049e426")
            {
                MessageBox.Show("Try again :)", "Wrong!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                nfailed++;
                if (nfailed >= 2) lblHalp.Visible = true;
                return;
            }

            MessageBox.Show("Congrats! You unlocked the unfinished Model Importer.\n\nChances are that it won't generate proper collision maps, but enjoy it anyway!", "Right!", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            Properties.Settings.Default.lolhax = true;
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
