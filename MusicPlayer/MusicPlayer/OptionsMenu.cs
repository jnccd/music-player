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
        public XNA parent;
        bool DownloadFinished;
        bool DoesPreloadActuallyWork = true;
        public bool IsClosed = false;

        public OptionsMenu(XNA parent)
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
            this.parent = parent;
        }

        private void OptionsMenu_Load(object sender, EventArgs e)
        {
            trackBar1.Value = config.Default.WavePreload;
            label1.Text = "Percentage of the songs samples that can be preloaded " + trackBar1.Value + "%";
            if (Program.game.Preload)
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
            tSmoothness.Value = (int)(config.Default.Smoothness * 100);
            if (config.Default.OldSmooth)
            {
                tSmoothness.Enabled = false;
                cOldSmooth.Checked = true;
            }
            else
            {
                tSmoothness.Enabled = true;
                cOldSmooth.Checked = false;
            }
        }

        private void PreloadToggle_Click(object sender, EventArgs e)
        {
            if (DoesPreloadActuallyWork)
            {
                Program.game.Preload = !Program.game.Preload;

                Program.game.ShowSecondRowMessage("Preload was set to " + Program.game.Preload + " \nThis setting will be applied when the next song starts", 1);

                if (Program.game.Preload)
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

            label1.Text = "Percentage of the songs samples that can be preloaded " + trackBar1.Value + "%";
        }
        private void ColorChange_Click(object sender, EventArgs e)
        {
            Program.game.ShowColorDialog();
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
            Program.game.ResetMusicSourcePath();
        }
        private void ShowStatistics_Click(object sender, EventArgs e)
        {
            parent.ShowStatistics();
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
            Program.game.VisSetting++;
            if ((int)Program.game.VisSetting > Enum.GetNames(typeof(Visualizations)).Length - 1)
                Program.game.VisSetting = 0;

            if (Program.game.VisSetting == Visualizations.dynamicline)
                Program.game.VisSetting = Visualizations.fft;
        }
        private void SwapBackgrounds_Click(object sender, EventArgs e)
        {
            Program.game.BgModes++;
            if ((int)Program.game.BgModes > Enum.GetNames(typeof(BackGroundModes)).Length - 1)
                Program.game.BgModes = 0;
            Program.game.ForceBackgroundRedraw();
        }

        private void Download_Click(object sender, EventArgs e) // WIP
        {
            string download = DownloadBox.Text;
            Download.Text = "Downloading...";
            Download.Enabled = false;
            DownloadBox.Enabled = false;
            Program.game.PauseConsoleInputThread = true;

            Task.Factory.StartNew(() =>
            {
                Task T = null;

                T = Task.Factory.StartNew(() => 
                {
                    try
                    {
                        Program.game.Download(download);
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
                Program.game.PauseConsoleInputThread = false;
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

        private void bConsoleThreadRestart_Click(object sender, EventArgs e)
        {
            Program.game.PauseConsoleInputThread = false;
            if (Program.game.ConsoleManager.IsCanceled || Program.game.ConsoleManager.IsCompleted || Program.game.ConsoleManager.IsFaulted)
            {
                Program.game.StartSongInputLoop();
            }
        }

        private void tSmoothness_Scroll(object sender, EventArgs e)
        {
            config.Default.Smoothness = tSmoothness.Value / 100f;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            config.Default.OldSmooth = cOldSmooth.Checked;
            tSmoothness.Enabled = !cOldSmooth.Checked;
        }

        private void OptionsMenu_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsClosed = true;
        }
        
        private void bExport_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog choose = new FolderBrowserDialog();
            DialogResult result = choose.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    if (parent.BackgroundOperationRunning)
                    {
                        MessageBox.Show("Multiple BackgroundOperations can not run at the same time!\nWait until the other operation is finished");
                        return;
                    }

                    parent.BackgroundOperationRunning = true;

                    List<string> CopyFiles = new List<string>();
                    for (int i = 0; i < Assets.UpvotedSongNames.Count; i++)
                    {
                        if (Assets.UpvotedSongScores[i] > 1)
                        {
                            string path = Assets.GetSongPathFromSongName(Assets.UpvotedSongNames[i]);
                            if (File.Exists(path))
                                CopyFiles.Add(path);
                        }
                    }

                    UpdateExports updat = new UpdateExports(CopyFiles.ToArray(), choose.SelectedPath);
                    updat.ShowDialog();

                    parent.BackgroundOperationRunning = false;
                }
                catch { MessageBox.Show("OOPSIE WOOPSIE!! Uwu We made a fucky wucky!!"); }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void history_Click(object sender, EventArgs e)
        {
            if (parent.history == null)
                parent.history = new History();

            parent.history.Show();
        }
    }
}
