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
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class UpdateMetadata : Form
    {
        string[] SongPaths;
        bool closing = false;

        public UpdateMetadata(string[] SongPaths)
        {
            InitializeComponent();
            this.Text = "Updating Mp3-Song-Metadata";
            this.SongPaths = SongPaths;
        }

        private void UpdateMetadata_Load(object sender, EventArgs e)
        {

        }

        private void UpdateMetadata_Shown(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < SongPaths.Length; i++)
            {
                backgroundWorker1.ReportProgress(i);
                string name = Path.GetFileNameWithoutExtension(SongPaths[i]);
                try
                {
                    HttpWebRequest req = null;
                    WebResponse W = null;
                    
                    string ResultURL = name.GetYoutubeVideoURL();

                    // Get VideoThumbnailURL
                    req = (HttpWebRequest)HttpWebRequest.Create(ResultURL);
                    req.KeepAlive = false;
                    W = req.GetResponse();
                    string VideoThumbnailURL;
                    using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                    {
                        // Extract info from HTMP string
                        string html = sr.ReadToEnd();
                        int index;
                        string startcuthtml;

                        index = html.IndexOf("<link itemprop=\"thumbnailUrl\" href=\"");
                        startcuthtml = html.Remove(0, index + "<link itemprop=\"thumbnailUrl\" href=\"".Length);
                        index = startcuthtml.IndexOf('"');
                        VideoThumbnailURL = startcuthtml.Remove(index, startcuthtml.Length - index);
                    }
                    string artist = name.Split('-').First().Trim();

                    // edit mp3 metadata using taglib
                    HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(VideoThumbnailURL);
                    HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    Stream stream = httpWebReponse.GetResponseStream();
                    Image im = Image.FromStream(stream);
                    TagLib.File file = TagLib.File.Create(SongPaths[i]);
                    TagLib.Picture pic = new TagLib.Picture();
                    pic.Type = TagLib.PictureType.FrontCover;
                    pic.Description = "Cover";
                    pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    MemoryStream ms = new MemoryStream();
                    im.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    ms.Position = 0;
                    pic.Data = TagLib.ByteVector.FromStream(ms);
                    file.Tag.Pictures = new TagLib.IPicture[] { pic };
                    file.Tag.Performers = new string[] { artist };
                    file.Tag.Comment = "Downloaded using MusicPlayer";
                    file.Tag.Album = "MusicPlayer Songs";
                    file.Tag.AlbumArtists = new string[] { artist };
                    file.Tag.Artists = new string[] { artist };
                    file.Tag.AmazonId = "AmazonIsShit";
                    file.Tag.Composers = new string[] { artist };
                    file.Tag.Copyright = "None";
                    file.Tag.Disc = 0;
                    file.Tag.DiscCount = 0;
                    file.Tag.Genres = new string[] { "good music" };
                    file.Tag.Grouping = "None";
                    file.Tag.Lyrics = "You expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\nYou expected lyrics, but it was me, dio.\n";
                    file.Tag.MusicIpId = "wubbel";
                    file.Tag.Title = name;
                    file.Tag.Track = 0;
                    file.Tag.TrackCount = 0;
                    file.Tag.Year = 1982;

                    file.Save();
                    ms.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error processing: " + name + "\nError Message: " + ex.Message);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = (int)(e.ProgressPercentage / (float)SongPaths.Length * 100);
            progressLabel.Text = progressBar.Value + " % " + Path.GetFileNameWithoutExtension(SongPaths[e.ProgressPercentage]);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            closing = true;
            this.Close();
        }

        private void UpdateMetadata_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closing == false)
                e.Cancel = true;
        }
    }
}
