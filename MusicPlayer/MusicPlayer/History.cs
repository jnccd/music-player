using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class History : Form
    {
        public History()
        {
            InitializeComponent();
        }

        private void History_Load(object sender, EventArgs e)
        {
            if (File.Exists(Values.CurrentExecutablePath + "\\History.txt"))
            {
                string[] Songs = File.ReadLines(Values.CurrentExecutablePath + "\\History.txt").ToArray();

                for (int i = 0; i < Songs.Length; i++)
                {
                    dataGridView1.Rows.Add(new object[] { Songs[Songs.Length - i - 1] });
                }
            }
        }
    }
}
