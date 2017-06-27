using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class OptionsMenu : Form
    {
        public OptionsMenu()
        {
            InitializeComponent();
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

        private void OptionsMenu_Load(object sender, EventArgs e)
        {
            trackBar1.Value = config.Default.WavePreload;
        }

        private void AAtoggle_Click(object sender, EventArgs e)
        {
            XNA.GauD.AntiAlising = !XNA.GauD.AntiAlising;
        }

        private void Reset_Click_1(object sender, EventArgs e)
        {
            XNA.ResetMusicSourcePath();
        }
    }
}
