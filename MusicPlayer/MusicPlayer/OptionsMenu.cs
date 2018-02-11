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
        public Statistics S = null;
        bool DownloadFinished;
        bool DoesPreloadActuallyWork = false;

        public OptionsMenu()
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
            //FormBorderStyle = FormBorderStyle.None;
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
            cAutoVolume.Checked = config.Default.AutoVolume;
        }

        private void PreloadToggle_Click(object sender, EventArgs e)
        {
            if (DoesPreloadActuallyWork)
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
            if (S == null || S.IsDisposed)
            {
                S = new Statistics();
                S.Show();
            }
            else
                S.BringToFront();
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
            XNA.ForceBackgroundRedraw();
        }

        private void Download_Click(object sender, EventArgs e) // WIP
        {
            string download = DownloadBox.Text;
            Download.Text = "Downloading...";
            Download.Enabled = false;
            DownloadBox.Enabled = false;
            XNA.PauseConsoleInputThread = true;

            Task.Factory.StartNew(() =>
            {
                Task T = null;

                T = Task.Factory.StartNew(() => 
                {
                    try
                    {
                        XNA.Download(download);
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
                XNA.PauseConsoleInputThread = false;
            }
        }

        private void ShowProgramFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "/select, \"" + Values.CurrentExecutablePath + "\"");
        }

        private void cAutoVolume_CheckedChanged(object sender, EventArgs e)
        {
            config.Default.AutoVolume = cAutoVolume.Checked;
        }
    }
}
