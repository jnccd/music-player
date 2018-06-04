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
                    string[] Split = Songs[Songs.Length - i - 1].Split(':');
                    string Title = "";
                    string Time = "";
                    if (Split.Length > 1)
                    {
                        Title = Split[0];
                        Time = DateTime.FromBinary(Convert.ToInt64(Split[1])).ToString();
                    }
                    else
                    {
                        Title = Songs[Songs.Length - i - 1];
                    }

                    dataGridView1.Rows.Add(new object[] { Title, Time });
                }
            }
        }
    }
}
