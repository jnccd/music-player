namespace MusicPlayer
{
    partial class Statistics
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
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.SongName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SongScore = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SongTrend = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SongTotalUpvotes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.volume = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SongAge = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Chance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Surreal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bRefresh = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.bSearch = new System.Windows.Forms.Button();
            this.toPlaying = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowDrop = true;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SongName,
            this.SongScore,
            this.SongTrend,
            this.SongTotalUpvotes,
            this.volume,
            this.SongAge,
            this.Chance,
            this.Surreal});
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.Location = new System.Drawing.Point(13, 41);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(923, 556);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseClick);
            this.dataGridView1.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick);
            this.dataGridView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGridView1_MouseDown);
            this.dataGridView1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dataGridView1_MouseMove);
            // 
            // SongName
            // 
            this.SongName.HeaderText = "Name";
            this.SongName.Name = "SongName";
            this.SongName.ReadOnly = true;
            // 
            // SongScore
            // 
            this.SongScore.HeaderText = "Score";
            this.SongScore.Name = "SongScore";
            this.SongScore.ReadOnly = true;
            // 
            // SongTrend
            // 
            this.SongTrend.HeaderText = "Trend";
            this.SongTrend.Name = "SongTrend";
            this.SongTrend.ReadOnly = true;
            // 
            // SongTotalUpvotes
            // 
            this.SongTotalUpvotes.HeaderText = "Up/Downvote Ratio";
            this.SongTotalUpvotes.Name = "SongTotalUpvotes";
            this.SongTotalUpvotes.ReadOnly = true;
            // 
            // volume
            // 
            this.volume.HeaderText = "Volume Multiplier";
            this.volume.Name = "volume";
            this.volume.ReadOnly = true;
            // 
            // SongAge
            // 
            this.SongAge.HeaderText = "Age (in Days)";
            this.SongAge.Name = "SongAge";
            this.SongAge.ReadOnly = true;
            // 
            // Chance
            // 
            this.Chance.HeaderText = "PlayChance (in %)";
            this.Chance.Name = "Chance";
            this.Chance.ReadOnly = true;
            // 
            // Surreal
            // 
            this.Surreal.HeaderText = "Dont look here b-baka!";
            this.Surreal.Name = "Surreal";
            this.Surreal.ReadOnly = true;
            // 
            // bRefresh
            // 
            this.bRefresh.Location = new System.Drawing.Point(13, 12);
            this.bRefresh.Name = "bRefresh";
            this.bRefresh.Size = new System.Drawing.Size(73, 23);
            this.bRefresh.TabIndex = 1;
            this.bRefresh.Text = "Refresh";
            this.bRefresh.UseVisualStyleBackColor = true;
            this.bRefresh.Click += new System.EventHandler(this.bRefresh_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(171, 14);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(686, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // bSearch
            // 
            this.bSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bSearch.Location = new System.Drawing.Point(863, 12);
            this.bSearch.Name = "bSearch";
            this.bSearch.Size = new System.Drawing.Size(73, 23);
            this.bSearch.TabIndex = 3;
            this.bSearch.Text = "Search";
            this.bSearch.UseVisualStyleBackColor = true;
            this.bSearch.Click += new System.EventHandler(this.bSearch_Click);
            // 
            // toPlaying
            // 
            this.toPlaying.Location = new System.Drawing.Point(92, 12);
            this.toPlaying.Name = "toPlaying";
            this.toPlaying.Size = new System.Drawing.Size(73, 23);
            this.toPlaying.TabIndex = 4;
            this.toPlaying.Text = "To Playing";
            this.toPlaying.UseVisualStyleBackColor = true;
            this.toPlaying.Click += new System.EventHandler(this.toPlaying_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Statistics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 609);
            this.Controls.Add(this.toPlaying);
            this.Controls.Add(this.bSearch);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.bRefresh);
            this.Controls.Add(this.dataGridView1);
            this.MinimumSize = new System.Drawing.Size(684, 127);
            this.Name = "Statistics";
            this.Text = "Statistics";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Statistics_FormClosed);
            this.Load += new System.EventHandler(this.Statistics_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button bRefresh;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button bSearch;
        private System.Windows.Forms.Button toPlaying;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.DataGridViewTextBoxColumn SongName;
        private System.Windows.Forms.DataGridViewTextBoxColumn SongScore;
        private System.Windows.Forms.DataGridViewTextBoxColumn SongTrend;
        private System.Windows.Forms.DataGridViewTextBoxColumn SongTotalUpvotes;
        private System.Windows.Forms.DataGridViewTextBoxColumn volume;
        private System.Windows.Forms.DataGridViewTextBoxColumn SongAge;
        private System.Windows.Forms.DataGridViewTextBoxColumn Chance;
        private System.Windows.Forms.DataGridViewTextBoxColumn Surreal;
    }
}