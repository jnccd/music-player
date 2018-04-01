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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.SongName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SongScore = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SongTrend = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SongTotalUpvotes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SongAge = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Chance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bRefresh = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.bSearch = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
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
            this.SongAge,
            this.Chance});
            this.dataGridView1.Location = new System.Drawing.Point(13, 41);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(750, 401);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseClick);
            this.dataGridView1.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick);
            this.dataGridView1.Resize += new System.EventHandler(this.dataGridView1_Resize);
            // 
            // SongName
            // 
            this.SongName.HeaderText = "SongName";
            this.SongName.Name = "SongName";
            // 
            // SongScore
            // 
            this.SongScore.HeaderText = "SongScore";
            this.SongScore.Name = "SongScore";
            // 
            // SongTrend
            // 
            this.SongTrend.HeaderText = "SongTrend";
            this.SongTrend.Name = "SongTrend";
            // 
            // SongTotalUpvotes
            // 
            this.SongTotalUpvotes.HeaderText = "SongTotalUpvotes";
            this.SongTotalUpvotes.Name = "SongTotalUpvotes";
            // 
            // SongAge
            // 
            this.SongAge.HeaderText = "SongAge (in Days)";
            this.SongAge.Name = "SongAge";
            // 
            // Chance
            // 
            this.Chance.HeaderText = "PlayChance (in %)";
            this.Chance.Name = "Chance";
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
            this.textBox1.Location = new System.Drawing.Point(92, 14);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(592, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // bSearch
            // 
            this.bSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bSearch.Location = new System.Drawing.Point(690, 12);
            this.bSearch.Name = "bSearch";
            this.bSearch.Size = new System.Drawing.Size(73, 23);
            this.bSearch.TabIndex = 3;
            this.bSearch.Text = "Search";
            this.bSearch.UseVisualStyleBackColor = true;
            this.bSearch.Click += new System.EventHandler(this.bSearch_Click);
            // 
            // Statistics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(775, 454);
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
        private System.Windows.Forms.DataGridViewTextBoxColumn SongName;
        private System.Windows.Forms.DataGridViewTextBoxColumn SongScore;
        private System.Windows.Forms.DataGridViewTextBoxColumn SongTrend;
        private System.Windows.Forms.DataGridViewTextBoxColumn SongTotalUpvotes;
        private System.Windows.Forms.DataGridViewTextBoxColumn SongAge;
        private System.Windows.Forms.DataGridViewTextBoxColumn Chance;
        private System.Windows.Forms.Button bRefresh;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button bSearch;
    }
}