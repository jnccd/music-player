using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class Statistics : Form
    {
        XNA parent;
        int currentMouseOverRow;
        public bool IsClosed = false;

        public Statistics(XNA parent)
        {
            /*
            this.EnableBlur();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.LimeGreen;
            TransparencyKey = Color.LimeGreen;
            */
            InitializeComponent();
            this.parent = parent;
        }

        private void Statistics_Load(object sender, EventArgs e)
        {
            bRefresh_Click(this, EventArgs.Empty);
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
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    Assets.PlayPlaylistSong(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                }
                catch { }
            }
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
            if (RowIndex != -1)
                dataGridView1.FirstDisplayedScrollingRowIndex = RowIndex;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                if (Assets.currentlyPlayingSongName.Equals(dataGridView1.Rows[i].Cells[0].Value))
                {
                    dataGridView1.Rows[i].Selected = true;
                    int heightInRows = dataGridView1.Height / dataGridView1.Rows[0].Height;
                    int index = i - heightInRows / 2 + 2;
                    if (index < 0)
                        index = 0;
                    dataGridView1.FirstDisplayedScrollingRowIndex = index;
                }

            if (dataGridView1.SortOrder != SortOrder.None)
                dataGridView1.Sort(dataGridView1.SortedColumn, dataGridView1.SortOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
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

        // ContextMenu
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                ContextMenu m = new ContextMenu();
                m.MenuItems.Add(new MenuItem("Play", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        Assets.PlayPlaylistSong(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                    }
                    catch { }
                })));
                m.MenuItems.Add(new MenuItem("Queue", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        Assets.QueueNewSong(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString(), false);
                    }
                    catch { }
                })));
                m.MenuItems.Add(new MenuItem("Open in Browser", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        Task.Factory.StartNew(() =>
                        {
                            // Use the I'm Feeling Lucky URL
                            string url = string.Format("https://www.google.com/search?num=100&site=&source=hp&q={0}&btnI=1", Path.GetFileNameWithoutExtension(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString()));
                            url = url.Replace(' ', '+');
                            WebRequest req = HttpWebRequest.Create(url);
                            Uri U = req.GetResponse().ResponseUri;

                            Process.Start(U.ToString());
                        });
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.MenuItems.Add(new MenuItem("Open in Browser with timestamp", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        Task.Factory.StartNew(() =>
                        {
                            // Use the I'm Feeling Lucky URL
                            string url = string.Format("https://www.google.com/search?num=100&site=&source=hp&q={0}&btnI=1", Path.GetFileNameWithoutExtension(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString()));
                            url = url.Replace(' ', '+');
                            WebRequest req = HttpWebRequest.Create(url);
                            Uri U = req.GetResponse().ResponseUri;
                            if (dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString().Equals(Assets.currentlyPlayingSongName))
                            {
                                int seconds = (int)(Assets.Channel32.Position / (double)Assets.Channel32.Length * Assets.Channel32.TotalTime.TotalSeconds);
                                U = new Uri(U.ToString() + "&t=" + seconds + "s");
                            }

                            Process.Start(U.ToString());
                            if (Assets.IsPlaying())
                                Assets.PlayPause();
                        });
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.MenuItems.Add(new MenuItem("Open in Explorer", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        if (!File.Exists(Assets.currentlyPlayingSongPath))
                            return;
                        else
                            Process.Start("explorer.exe", "/select, \"" + Assets.GetSongPathFromSongName(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString()) + "\"");
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.MenuItems.Add(new MenuItem("Update Mp3-Metadata of Selection", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        if (parent.BackgroundOperationRunning)
                        {
                            MessageBox.Show("Multiple BackgroundOperations can not run at the same time!\nWait until the other operation is finished");
                            return;
                        }

                        parent.BackgroundOperationRunning = true;

                        List<string> SongPaths = new List<string>();
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            if (dataGridView1.Rows[i].Selected)
                                SongPaths.Add(Assets.GetSongPathFromSongName((string)dataGridView1.Rows[i].Cells[0].Value));
                        UpdateMetadata updat = new UpdateMetadata(SongPaths.ToArray());

                        if (SongPaths.Count > 0)
                            updat.ShowDialog();
                        else
                            MessageBox.Show("You havent selected anything!\nMake sure to select entire Rows");

                        parent.BackgroundOperationRunning = false;
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.MenuItems.Add(new MenuItem("Show Cover Picture", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        string path = Assets.GetSongPathFromSongName(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                        TagLib.File file = TagLib.File.Create(path);
                        TagLib.IPicture pic = file.Tag.Pictures[0];
                        MemoryStream ms = new MemoryStream(pic.Data.Data);
                        if (ms != null && ms.Length > 4096)
                        {
                            Image currentImage = Image.FromStream(ms);
                            path = Values.CurrentExecutablePath + "\\Downloads\\Thumbnail.png";
                            currentImage.Save(path);
                            Process.Start(path);
                        }
                        ms.Close();
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));

                currentMouseOverRow = e.RowIndex;

                m.Show(dataGridView1, new Point(e.X + dataGridView1.GetColumnDisplayRectangle(e.ColumnIndex, true).X, e.Y + dataGridView1.GetRowDisplayRectangle(e.RowIndex, true).Y));
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Statistics_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsClosed = true;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
