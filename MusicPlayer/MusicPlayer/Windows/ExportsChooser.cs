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
    public partial class ExportsChooser : Form
    {
        public List<UpvotedSong> SelectedSongs = new List<UpvotedSong>();
        public List<UpvotedSong> SongsToChooseFrom = new List<UpvotedSong>();
        public float[] ChanceAmounts;
        public string[] Output;

        public ExportsChooser()
        {
            InitializeComponent();
        }

        private void bFinished_Click(object sender, EventArgs e)
        {
            Output = SelectedSongs.Select(x => x.Path).ToArray();
            this.Close();
        }

        private void ExportsChooser_Load(object sender, EventArgs e)
        {
            foreach (UpvotedSong s in Assets.UpvotedSongData)
            {
                s.Path = Assets.GetSongPathFromSongName(s.Name);
                if (File.Exists(s.Path))
                    SongsToChooseFrom.Add(s);
            }

            int max = SongsToChooseFrom.Max(x => x.TotalDislikes == 0 ? int.MinValue : x.TotalLikes / x.TotalDislikes);

            tScore.Minimum = (int)SongsToChooseFrom.Min(x => x.Score);
            tScore.Maximum = (int)SongsToChooseFrom.Max(x => x.Score);
            tTrend.Minimum = SongsToChooseFrom.Min(x => x.Streak);
            tTrend.Maximum = SongsToChooseFrom.Max(x => x.Streak);
            tRatio.Minimum = SongsToChooseFrom.Min(x => x.TotalDislikes == 0 ? max + 1 : x.TotalLikes / x.TotalDislikes);
            tRatio.Maximum = SongsToChooseFrom.Max(x => x.TotalDislikes == 0 ? max + 1 : x.TotalLikes / x.TotalDislikes);

            int sum = 0;
            ChanceAmounts = new float[SongsToChooseFrom.Count];
            for (int i = 0; i < SongsToChooseFrom.Count; i++)
            {
                int amount = (int)Assets.GetSongChoosingAmount(i) + 1;
                sum += amount;
                ChanceAmounts[i] = amount;
            }
            for (int i = 0; i < SongsToChooseFrom.Count; i++)
                ChanceAmounts[i] = ChanceAmounts[i] * 1000 / sum;

            tChance.Minimum = 0;
            tChance.Maximum = (int)(ChanceAmounts.Max() * 1000);
            
            UpdateSelectedSongs();
        }

        private void UpdateSelectedSongs()
        {
            SelectedSongs.Clear();
            for (int i = 0; i < SongsToChooseFrom.Count; i++)
            {
                UpvotedSong s = SongsToChooseFrom[i];
                if (s.Score >= tScore.Value && s.Streak >= tTrend.Value && 
                    (s.TotalDislikes == 0 ? int.MaxValue : s.TotalLikes / s.TotalDislikes) >= tRatio.Value &&
                    ChanceAmounts[i] > tChance.Value / 1000f)
                    SelectedSongs.Add(s);
            }

            long SongSizeSum = 0;
            foreach (UpvotedSong s in SelectedSongs)
                SongSizeSum += (int)(new FileInfo(s.Path).Length);
            SongSizeSum /= 1024;
            SongSizeSum /= 1024;

            lResult.Text = SelectedSongs.Count + " Songs chosen\nTotal Size: " + SongSizeSum + " MB";
        }

        private void tScore_Scroll(object sender, EventArgs e)
        {
            UpdateSelectedSongs();
            lScore.Text = "Minimal Score: " + tScore.Value;
        }

        private void tTrend_Scroll(object sender, EventArgs e)
        {
            UpdateSelectedSongs();
            lTrend.Text = "Minimal Trend: " + tTrend.Value;
        }

        private void tRatio_Scroll(object sender, EventArgs e)
        {
            UpdateSelectedSongs();
            lRatio.Text = "Minimal Ratio: " + tRatio.Value;
        }

        private void tChance_Scroll(object sender, EventArgs e)
        {
            UpdateSelectedSongs();
            lChance.Text = "Minimal Play-Chance: " + tChance.Value / 1000f;
        }
    }
}
