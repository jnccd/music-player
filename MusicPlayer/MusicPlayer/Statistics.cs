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

            bRefresh.Width = this.Width / 895 * 147;
            textBox1.Width = this.Width / 895 * 547;
            bSearch.Width = this.Width / 895 * 147;

            textBox1.Location = new Point(bRefresh.Bounds.X + bRefresh.Bounds.Width + 7, textBox1.Location.Y);
            bSearch.Location = new Point(textBox1.Bounds.X + textBox1.Bounds.Width + 7, bSearch.Location.Y);
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                Assets.PlayPlaylistSong(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
            }
            catch { }
        }

        private void bRefresh_Click(object sender, EventArgs e)
        {
            int RowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
            dataGridView1.Rows.Clear();
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
            dataGridView1.FirstDisplayedScrollingRowIndex = RowIndex;
        }

        private void bSearch_Click(object sender, EventArgs e)
        {
            string Path = textBox1.Text;
            textBox1.Text = "";

            DistancePerSong[] LDistances = new DistancePerSong[dataGridView1.Rows.Count];
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                LDistances[i].SongDifference = Values.OwnDistanceWrapper(Path, ((string)(dataGridView1.Rows[i].Cells[0].Value)));
                LDistances[i].SongIndex = i;
            }

            LDistances = LDistances.OrderBy(x => x.SongDifference).ToArray();

            dataGridView1.ClearSelection();

            dataGridView1.Rows[LDistances.First().SongIndex].Selected = true;
            for (int i = 1; i < LDistances.Length; i++)
            {
                if (LDistances[i].SongDifference > 1)
                    break;
                dataGridView1.Rows[LDistances[i].SongIndex].Selected = true;
            }
            dataGridView1.FirstDisplayedScrollingRowIndex = LDistances.First().SongIndex;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                bSearch_Click(this, EventArgs.Empty);
        }
    }
}
