using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class stringDialog : Form
    {
        public string result;

        public stringDialog(string Question, string preText)
        {
            InitializeComponent();
            Text = Question;
            textBox1.Text = preText;
        }

        private void done_Click(object sender, EventArgs e)
        {
            result = textBox1.Text;
            Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n')
                done_Click(this, EventArgs.Empty);
        }
    }
}
