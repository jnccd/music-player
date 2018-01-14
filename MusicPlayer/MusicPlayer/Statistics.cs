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
    public partial class Statistics : Form
    {
        public Statistics()
        {
            InitializeComponent();
        }

        private void Statistics_Load(object sender, EventArgs e)
        {
            object[] o = new object[6];
            object[,] SongInfo = Assets.GetSongInformationList();

            for (int i = 0; i < Assets.UpvotedSongNames.Count; i++)
            {
                o[0] = SongInfo[i, 0];
                o[1] = SongInfo[i, 1];
                o[2] = SongInfo[i, 2];
                o[3] = SongInfo[i, 3];
                o[4] = SongInfo[i, 4];
                o[5] = SongInfo[i, 5];

                if (File.Exists(Assets.GetSongPathFromSongName(Assets.UpvotedSongNames[i])))
                    dataGridView1.Rows.Add(o);
            }

            dataGridView1.Columns[0].Width = dataGridView1.Width - 460;
            dataGridView1.Columns[1].Width = 80;
            dataGridView1.Columns[2].Width = 80;
            dataGridView1.Columns[3].Width = 80;
            dataGridView1.Columns[4].Width = 80;
            dataGridView1.Columns[5].Width = 80;
        }

        private void dataGridView1_Resize(object sender, EventArgs e)
        {
            dataGridView1.Columns[0].Width = dataGridView1.Width - 460;
            dataGridView1.Columns[1].Width = 80;
            dataGridView1.Columns[2].Width = 80;
            dataGridView1.Columns[3].Width = 80;
            dataGridView1.Columns[4].Width = 80;
            dataGridView1.Columns[5].Width = 80;
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                Assets.PlayPlaylistSong(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
            }
            catch { }
        }
    }
}
