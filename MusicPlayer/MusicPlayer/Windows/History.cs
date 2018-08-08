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
    public partial class History : Form
    {
        int currentMouseOverRow;
        XNA parent;

        public History(XNA parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void History_Load(object sender, EventArgs e)
        {
            int RowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
            dataGridView1.Rows.Clear();
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
                    if (!Assets.UpvotedSongData.Select(x => x.Name).Contains(Split[0]))
                        dataGridView1.Rows[dataGridView1.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Red;
                }
            }
            if (RowIndex > 0)
                dataGridView1.FirstDisplayedScrollingRowIndex = RowIndex;
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.RowIndex >= 0 && !Assets.PlayPlaylistSong(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() + ".mp3"))
                    MessageBox.Show("This entry isnt linked to a mp3 file!");
            }
        }
        
        // ContextMenu
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e != null && e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Cells[0].Selected = true;
                ContextMenu m = new ContextMenu();
                m.MenuItems.Add(new MenuItem("Play", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        if (!Assets.PlayPlaylistSong(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString() + ".mp3"))
                            MessageBox.Show("This entry isnt linked to a mp3 file!");
                    }
                    catch { }
                })));
                m.MenuItems.Add(new MenuItem("Queue", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        Assets.QueueNewSong(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString(), false);
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.MenuItems.Add(new MenuItem("Copy Title to Clipboard", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        Clipboard.SetText(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.MenuItems.Add(new MenuItem("Copy URL to Clipboard", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        // Get fitting youtube video
                        string url = string.Format("https://www.youtube.com/results?search_query=" + dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                        HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                        req.KeepAlive = false;
                        WebResponse W = req.GetResponse();
                        string ResultURL;
                        using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                        {
                            string html = sr.ReadToEnd();
                            int index = html.IndexOf("href=\"/watch?");
                            string startcuthtml = html.Remove(0, index + 6);
                            index = startcuthtml.IndexOf('"');
                            string cuthtml = startcuthtml.Remove(index, startcuthtml.Length - index);
                            ResultURL = "https://www.youtube.com" + cuthtml;
                        }

                        Clipboard.SetText(ResultURL);
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                m.MenuItems.Add(new MenuItem("Open in Browser", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        Task.Factory.StartNew(() =>
                        {
                            // Get fitting youtube video
                            string url = string.Format("https://www.youtube.com/results?search_query=" + dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                            req.KeepAlive = false;
                            WebResponse W = req.GetResponse();
                            string ResultURL;
                            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                            {
                                string html = sr.ReadToEnd();
                                int index = html.IndexOf("href=\"/watch?");
                                string startcuthtml = html.Remove(0, index + 6);
                                index = startcuthtml.IndexOf('"');
                                string cuthtml = startcuthtml.Remove(index, startcuthtml.Length - index);
                                ResultURL = "https://www.youtube.com" + cuthtml;
                            }

                            Uri U = new Uri(ResultURL);
                            Process.Start(U.ToString());
                        });
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));
                if (dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString().Equals(Path.GetFileNameWithoutExtension(Assets.currentlyPlayingSongName)))
                    m.MenuItems.Add(new MenuItem("Open in Browser with timestamp", ((object s, EventArgs ev) =>
                    {
                        try
                        {
                            Task.Factory.StartNew(() =>
                            {
                                // Get fitting youtube video
                                string url = string.Format("https://www.youtube.com/results?search_query=" + dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                                req.KeepAlive = false;
                                WebResponse W = req.GetResponse();
                                string ResultURL;
                                using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                                {
                                    string html = sr.ReadToEnd();
                                    int index = html.IndexOf("href=\"/watch?");
                                    string startcuthtml = html.Remove(0, index + 6);
                                    index = startcuthtml.IndexOf('"');
                                    string cuthtml = startcuthtml.Remove(index, startcuthtml.Length - index);
                                    ResultURL = "https://www.youtube.com" + cuthtml;
                                }

                                int seconds = (int)(Assets.Channel32.Position / (double)Assets.Channel32.Length * Assets.Channel32.TotalTime.TotalSeconds);
                                Uri U = new Uri(ResultURL + "&t=" + seconds + "s");
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
                        string path = Assets.GetSongPathFromSongName(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString());
                        if (!File.Exists(path))
                            return;
                        else
                            Process.Start("explorer.exe", "/select, \"" + path + "\"");
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
                m.MenuItems.Add(new MenuItem("Show in Statistics", ((object s, EventArgs ev) =>
                {
                    try
                    {
                        parent.ShowStatistics();
                        parent.statistics.InvokeIfRequired(() => {
                            parent.statistics.toSong(dataGridView1.Rows[currentMouseOverRow].Cells[0].Value.ToString()); });
                    }
                    catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
                })));

                currentMouseOverRow = e.RowIndex;

                m.Show(dataGridView1, new Point(e.X + dataGridView1.GetColumnDisplayRectangle(e.ColumnIndex, true).X, e.Y + dataGridView1.GetRowDisplayRectangle(e.RowIndex, true).Y));
            }
        }

        private void bRefresh_Click(object sender, EventArgs e)
        {
            History_Load(this, EventArgs.Empty);
        }
    }
}
