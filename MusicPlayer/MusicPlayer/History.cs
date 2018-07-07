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
                    string ScoreChange = "";
                    if (Split.Length == 1)
                    {
                        Title = Path.GetFileNameWithoutExtension(Songs[Songs.Length - i - 1]);
                    }
                    else if (Split.Length == 2)
                    {
                        Title = Path.GetFileNameWithoutExtension(Split[0]);
                        Time = DateTime.FromBinary(Convert.ToInt64(Split[1])).ToString();
                    }
                    else if (Split.Length == 3)
                    {
                        Title = Path.GetFileNameWithoutExtension(Split[0]);
                        Time = DateTime.FromBinary(Convert.ToInt64(Split[1])).ToString();
                        ScoreChange = Split[2];
                    }

                    dataGridView1.Rows.Add(new object[] { Title, Time, ScoreChange });
                }
            }
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.RowIndex > 0 && !Assets.PlayPlaylistSong(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() + ".mp3"))
                    MessageBox.Show("This entry isnt linked to a mp3 file!");
            }
        }
    }
}
