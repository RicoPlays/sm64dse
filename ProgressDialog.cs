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
    public partial class ProgressDialog : Form
    {
        public ProgressDialog(string actiontxt, int maxsteps, object userdata)
        {
            InitializeComponent();

            m_InternalClosure = false;

            m_ActionText = actiontxt;
            this.Text = actiontxt + " - " + Program.AppTitle;
            UserData = userdata;
            Error = null;

            SetupProgressBar(0, 100);
            m_MaxSteps = maxsteps;
        }

        public void SetupProgressBar(int min, int max)
        {
            pbProgress.Minimum = min;
            pbProgress.Maximum = max;
            pbProgress.Value = min;
        }

        public void UpdateProgress(int progress)
        {
            pbProgress.Value = progress % 100;
            lblAction.Text = string.Format("{0} - step {1} of {2}", m_ActionText, (int)(progress / 100), m_MaxSteps);
        }

        public void OperationDone()
        {
            m_InternalClosure = true;
            this.Close();
        }

        private void ProgressDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason != CloseReason.UserClosing) || m_InternalClosure)
                return;

            if (MessageBox.Show("Are you sure you want to cancel the pending operation?", Program.AppTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

           // m_Thread.Abort();
        }

        public void RW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateProgress(e.ProgressPercentage);
        }

        public void RW_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            OperationDone();
            Error = e.Error;
        }

        public object UserData;
        public Exception Error;
        private bool m_InternalClosure;
        private string m_ActionText;
        private int m_MaxSteps;
    }
}
