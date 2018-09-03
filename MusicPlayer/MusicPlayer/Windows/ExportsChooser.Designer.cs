namespace MusicPlayer
{
    partial class ExportsChooser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lScore = new System.Windows.Forms.Label();
            this.tScore = new System.Windows.Forms.TrackBar();
            this.tTrend = new System.Windows.Forms.TrackBar();
            this.lTrend = new System.Windows.Forms.Label();
            this.tRatio = new System.Windows.Forms.TrackBar();
            this.lRatio = new System.Windows.Forms.Label();
            this.lResult = new System.Windows.Forms.Label();
            this.bFinished = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tScore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tTrend)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tRatio)).BeginInit();
            this.SuspendLayout();
            // 
            // lScore
            // 
            this.lScore.AutoSize = true;
            this.lScore.Location = new System.Drawing.Point(12, 9);
            this.lScore.Name = "lScore";
            this.lScore.Size = new System.Drawing.Size(85, 13);
            this.lScore.TabIndex = 0;
            this.lScore.Text = "Minimal Score: 0";
            // 
            // tScore
            // 
            this.tScore.Location = new System.Drawing.Point(15, 25);
            this.tScore.Maximum = 1200;
            this.tScore.Name = "tScore";
            this.tScore.Size = new System.Drawing.Size(396, 45);
            this.tScore.TabIndex = 1;
            this.tScore.Scroll += new System.EventHandler(this.tScore_Scroll);
            // 
            // tTrend
            // 
            this.tTrend.Location = new System.Drawing.Point(15, 73);
            this.tTrend.Maximum = 1200;
            this.tTrend.Name = "tTrend";
            this.tTrend.Size = new System.Drawing.Size(396, 45);
            this.tTrend.TabIndex = 3;
            this.tTrend.Scroll += new System.EventHandler(this.tTrend_Scroll);
            // 
            // lTrend
            // 
            this.lTrend.AutoSize = true;
            this.lTrend.Location = new System.Drawing.Point(12, 57);
            this.lTrend.Name = "lTrend";
            this.lTrend.Size = new System.Drawing.Size(85, 13);
            this.lTrend.TabIndex = 2;
            this.lTrend.Text = "Minimal Trend: 0";
            // 
            // tRatio
            // 
            this.tRatio.Location = new System.Drawing.Point(15, 121);
            this.tRatio.Maximum = 1200;
            this.tRatio.Name = "tRatio";
            this.tRatio.Size = new System.Drawing.Size(396, 45);
            this.tRatio.TabIndex = 5;
            this.tRatio.Scroll += new System.EventHandler(this.tRatio_Scroll);
            // 
            // lRatio
            // 
            this.lRatio.AutoSize = true;
            this.lRatio.Location = new System.Drawing.Point(12, 105);
            this.lRatio.Name = "lRatio";
            this.lRatio.Size = new System.Drawing.Size(82, 13);
            this.lRatio.TabIndex = 4;
            this.lRatio.Text = "Minimal Ratio: 0";
            // 
            // lResult
            // 
            this.lResult.AutoSize = true;
            this.lResult.Location = new System.Drawing.Point(12, 169);
            this.lResult.Name = "lResult";
            this.lResult.Size = new System.Drawing.Size(84, 26);
            this.lResult.TabIndex = 6;
            this.lResult.Text = "0 Songs chosen\r\ntotal Size 0 MB";
            // 
            // bFinished
            // 
            this.bFinished.Location = new System.Drawing.Point(336, 172);
            this.bFinished.Name = "bFinished";
            this.bFinished.Size = new System.Drawing.Size(75, 23);
            this.bFinished.TabIndex = 7;
            this.bFinished.Text = "Ok";
            this.bFinished.UseVisualStyleBackColor = true;
            this.bFinished.Click += new System.EventHandler(this.bFinished_Click);
            // 
            // ExportsChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 207);
            this.Controls.Add(this.bFinished);
            this.Controls.Add(this.lResult);
            this.Controls.Add(this.tRatio);
            this.Controls.Add(this.lRatio);
            this.Controls.Add(this.tTrend);
            this.Controls.Add(this.lTrend);
            this.Controls.Add(this.tScore);
            this.Controls.Add(this.lScore);
            this.Name = "ExportsChooser";
            this.Text = "ExportsChooser";
            this.Load += new System.EventHandler(this.ExportsChooser_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tScore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tTrend)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tRatio)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lScore;
        private System.Windows.Forms.TrackBar tScore;
        private System.Windows.Forms.TrackBar tTrend;
        private System.Windows.Forms.Label lTrend;
        private System.Windows.Forms.TrackBar tRatio;
        private System.Windows.Forms.Label lRatio;
        private System.Windows.Forms.Label lResult;
        private System.Windows.Forms.Button bFinished;
    }
}