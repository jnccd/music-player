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
    public partial class UpdateExports : Form
    {
        string[] SongPaths;
        bool closing = false;
        string Target;

        public UpdateExports(string[] SongPaths, string Target)
        {
            InitializeComponent();
            this.Text = "Exporting";
            this.SongPaths = SongPaths;
            this.Target = Target;
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
                    File.Copy(SongPaths[i], Target + "\\" + Path.GetFileName(SongPaths[i]));
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
