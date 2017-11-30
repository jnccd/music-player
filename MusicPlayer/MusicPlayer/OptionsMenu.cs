using MediaToolkit;
using MediaToolkit.Model;
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
    public partial class OptionsMenu : Form
    {
        Statistics S;
        bool DownloadFinished;

        public OptionsMenu()
        {
            InitializeComponent();
        }

        private void OptionsMenu_Load(object sender, EventArgs e)
        {
            trackBar1.Value = config.Default.WavePreload;
            if (XNA.Preload)
            {
                PreloadToggle.Text = "Disable Preload";
                trackBar1.Enabled = true;
                label1.Enabled = true;
            }
            else
            {
                PreloadToggle.Text = "Enable Preload";
                trackBar1.Enabled = false;
                label1.Enabled = false;
            }
        }

        private void PreloadToggle_Click(object sender, EventArgs e)
        {
            XNA.Preload = !XNA.Preload;

            XNA.ShowSecondRowMessage("Preload was set to " + XNA.Preload + " \nThis setting will be applied when the next song starts", 1);

            if (XNA.Preload)
            {
                PreloadToggle.Text = "Disable Preload";
                trackBar1.Enabled = true;
                label1.Enabled = true;
            }
            else
            {
                PreloadToggle.Text = "Enable Preload";
                trackBar1.Enabled = false;
                label1.Enabled = false;
            }
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            config.Default.WavePreload = trackBar1.Value;
        }
        private void ColorChange_Click(object sender, EventArgs e)
        {
            XNA.ShowColorDialog();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Assets.currentlyPlayingSongPath))
                return;
            else
                Process.Start("explorer.exe", "/select, \"" + Assets.currentlyPlayingSongPath + "\"");
        }
        private void AAtoggle_Click(object sender, EventArgs e)
        {
            config.Default.AntiAliasing = !config.Default.AntiAliasing;
        }
        private void Reset_Click_1(object sender, EventArgs e)
        {
            XNA.ResetMusicSourcePath();
        }
        private void ShowStatistics_Click(object sender, EventArgs e)
        {
            S = new Statistics();
            S.Show();
        }
        private void ShowConsole_Click(object sender, EventArgs e)
        {
            Values.ShowWindow(Values.GetConsoleWindow(), 0x09);
            Values.SetForegroundWindow(Values.GetConsoleWindow());
        }
        private void ShowBrowser_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    // Use the I'm Feeling Lucky URL
                    string url = string.Format("https://www.google.com/search?num=100&site=&source=hp&q={0}&btnI=1", Assets.currentlyPlayingSongName.Split('.').First());
                    url = url.Replace(' ', '+');
                    WebRequest req = HttpWebRequest.Create(url);
                    Uri U = req.GetResponse().ResponseUri;

                    Process.Start(U.ToString());
                }
                catch
                {
                    MessageBox.Show("Couldn't open Song in Webbrowser!");
                }
            });
        }
        private void SwapVisualisations_Click(object sender, EventArgs e)
        {
            XNA.VisSetting++;
            if ((int)XNA.VisSetting > Enum.GetNames(typeof(Visualizations)).Length - 1)
                XNA.VisSetting = 0;

            if (XNA.VisSetting == Visualizations.dynamicline)
                XNA.VisSetting = Visualizations.fft;
        }
        private void SwapBackgrounds_Click(object sender, EventArgs e)
        {
            XNA.BgModes++;
            if ((int)XNA.BgModes > Enum.GetNames(typeof(BackGroundModes)).Length - 1)
                XNA.BgModes = 0;
        }

        private void Download_Click(object sender, EventArgs e) // WIP
        {
            string download = DownloadBox.Text;
            Download.Text = "Downloading...";
            Download.Enabled = false;
            DownloadBox.Enabled = false;

            Task.Factory.StartNew(() =>
            {
                Task T = null;

                T = Task.Factory.StartNew(() => 
                {
                    try
                    {
                        // Get fitting youtube video
                        string url = string.Format("https://www.youtube.com/results?search_query=" + download);
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
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

                        // Get video title
                        req = (HttpWebRequest)WebRequest.Create(ResultURL);
                        req.KeepAlive = false;
                        W = req.GetResponse();
                        string VideoTitle;
                        using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                        {
                            string html = sr.ReadToEnd();
                            int index = html.IndexOf("<span id=\"eow-title\" class=\"watch-title\" dir=\"ltr\" title=\"");
                            string startcuthtml = html.Remove(0, index + "<span id=\"eow-title\" class=\"watch-title\" dir=\"ltr\" title=\"".Length);
                            index = startcuthtml.IndexOf('"');
                            VideoTitle = startcuthtml.Remove(index, startcuthtml.Length - index);

                            // Decode the encoded string.
                            StringWriter myWriter = new StringWriter();
                            System.Web.HttpUtility.HtmlDecode(VideoTitle, myWriter);
                            VideoTitle = myWriter.ToString();
                        }

                        if (MessageBox.Show("Closest song found: " + VideoTitle, "Song found", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {

                            // Delete File if there still is one for some reason? The thread crashes otherwise so better do it.
                            string videofile = "" + Environment.CurrentDirectory + "\\Downloads\\File.mp4";
                            if (File.Exists(videofile))
                                File.Delete(videofile);

                            // Download Video File
                            Process P = new Process();
                            P.StartInfo = new ProcessStartInfo("youtube-dl.exe", "-o \"/Downloads/File.mp4\" " + ResultURL)
                            {
                                UseShellExecute = false, RedirectStandardOutput = true
                            };

                            P.Start();
                            string result = "";

                            while (!P.HasExited)
                            {
                                result += P.StandardOutput.ReadLine();
                                DownloadBox.Text = result;
                                P.Refresh();
                            }

                            P.WaitForExit();

                            // Convert Video File to mp3 and put it into the default folder
                            MediaFile input = new MediaFile { Filename = videofile };
                            MediaFile output = new MediaFile { Filename = config.Default.MusicPath + "\\" + VideoTitle + ".mp3" };
                            using (var engine = new Engine())
                            {
                                engine.GetMetadata(input);
                                engine.Convert(input, output);
                            }

                            File.Delete(videofile);
                            Assets.PlayNewSong(output.Filename);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                });

                if (T != null)
                    T.Wait();

                DownloadFinished = true;
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DownloadFinished)
            {
                DownloadBox.Text = "";
                Download.Text = "Start";
                Download.Enabled = true;
                DownloadBox.Enabled = true;
                DownloadFinished = false;
            }
        }
    }
}
