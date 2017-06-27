namespace MusicPlayer
{
    partial class OptionsMenu
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
            this.ColorChange = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.Showinexploerer = new System.Windows.Forms.Button();
            this.AAtoggle = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // ColorChange
            // 
            this.ColorChange.Location = new System.Drawing.Point(16, 80);
            this.ColorChange.Name = "ColorChange";
            this.ColorChange.Size = new System.Drawing.Size(205, 23);
            this.ColorChange.TabIndex = 0;
            this.ColorChange.Text = "Change primary Color [C]";
            this.ColorChange.UseVisualStyleBackColor = true;
            this.ColorChange.Click += new System.EventHandler(this.ColorChange_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(12, 29);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(420, 45);
            this.trackBar1.TabIndex = 1;
            this.trackBar1.Value = 50;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(271, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Percentage of the songs samples that can be preloaded";
            // 
            // Showinexploerer
            // 
            this.Showinexploerer.Location = new System.Drawing.Point(227, 80);
            this.Showinexploerer.Name = "Showinexploerer";
            this.Showinexploerer.Size = new System.Drawing.Size(205, 23);
            this.Showinexploerer.TabIndex = 3;
            this.Showinexploerer.Text = "Show Song File in Explorer [E]";
            this.Showinexploerer.UseVisualStyleBackColor = true;
            this.Showinexploerer.Click += new System.EventHandler(this.button1_Click);
            // 
            // AAtoggle
            // 
            this.AAtoggle.Location = new System.Drawing.Point(16, 109);
            this.AAtoggle.Name = "AAtoggle";
            this.AAtoggle.Size = new System.Drawing.Size(205, 23);
            this.AAtoggle.TabIndex = 4;
            this.AAtoggle.Text = "Toggle Anti-Alising [A]";
            this.AAtoggle.UseVisualStyleBackColor = true;
            this.AAtoggle.Click += new System.EventHandler(this.AAtoggle_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(227, 109);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(205, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Reset Music Source Folder [S]";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Reset_Click_1);
            // 
            // OptionsMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 145);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.AAtoggle);
            this.Controls.Add(this.Showinexploerer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.ColorChange);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "OptionsMenu";
            this.Text = "OptionsMenu";
            this.Load += new System.EventHandler(this.OptionsMenu_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ColorChange;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Showinexploerer;
        private System.Windows.Forms.Button AAtoggle;
        private System.Windows.Forms.Button button1;
    }
}