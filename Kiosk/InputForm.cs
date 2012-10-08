using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kiosk
{
    public partial class InputForm : Form
    {
        private string Password;

        public InputForm(string password)
        {
            InitializeComponent();
            this.Password = password;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (tbInput.Text == this.Password)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Incorrect password.");
            }
        }

        private void InputForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void tbInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSubmit_Click(this, null);
            }
        }
    }
}
