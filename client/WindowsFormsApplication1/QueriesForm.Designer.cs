namespace BaboGameClient
{
    partial class QueriesForm
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
            this.QueryGrid = new System.Windows.Forms.DataGridView();
            this.Send_btn = new System.Windows.Forms.Button();
            this.queries_tb = new System.Windows.Forms.TextBox();
            this.TimePlayed_rb = new System.Windows.Forms.RadioButton();
            this.Ranking_rb = new System.Windows.Forms.RadioButton();
            this.Characters_rb = new System.Windows.Forms.RadioButton();
            this.ConnectedList_rb = new System.Windows.Forms.RadioButton();
            this.MenúExtres = new System.Windows.Forms.MenuStrip();
            this.Notificacions_btn = new System.Windows.Forms.ToolStripMenuItem();
            this.MainMenu_btn = new System.Windows.Forms.ToolStripMenuItem();
            this.QueriesMenu_btn = new System.Windows.Forms.ToolStripMenuItem();
            this.Opponents_rb = new System.Windows.Forms.RadioButton();
            this.gameResultsWithPlayers_rb = new System.Windows.Forms.RadioButton();
            this.dateTimeStart_dt = new System.Windows.Forms.DateTimePicker();
            this.dateTimeEnd = new System.Windows.Forms.DateTimePicker();
            this.GameListInterval_rb = new System.Windows.Forms.RadioButton();
            this.TimeInterval1_lbl = new System.Windows.Forms.Label();
            this.TimeInterval2_lbl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.QueryGrid)).BeginInit();
            this.MenúExtres.SuspendLayout();
            this.SuspendLayout();
            // 
            // QueryGrid
            // 
            this.QueryGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.QueryGrid.Location = new System.Drawing.Point(429, 25);
            this.QueryGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.QueryGrid.Name = "QueryGrid";
            this.QueryGrid.RowTemplate.Height = 24;
            this.QueryGrid.Size = new System.Drawing.Size(357, 414);
            this.QueryGrid.TabIndex = 0;
            this.QueryGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.QueryGrid_CellClick);
            // 
            // Send_btn
            // 
            this.Send_btn.Location = new System.Drawing.Point(149, 393);
            this.Send_btn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Send_btn.Name = "Send_btn";
            this.Send_btn.Size = new System.Drawing.Size(160, 46);
            this.Send_btn.TabIndex = 1;
            this.Send_btn.Text = "Envia";
            this.Send_btn.UseVisualStyleBackColor = true;
            this.Send_btn.Click += new System.EventHandler(this.Send_btn_Click);
            // 
            // queries_tb
            // 
            this.queries_tb.Location = new System.Drawing.Point(118, 55);
            this.queries_tb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.queries_tb.Name = "queries_tb";
            this.queries_tb.Size = new System.Drawing.Size(191, 22);
            this.queries_tb.TabIndex = 2;
            // 
            // TimePlayed_rb
            // 
            this.TimePlayed_rb.AutoSize = true;
            this.TimePlayed_rb.Location = new System.Drawing.Point(23, 98);
            this.TimePlayed_rb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TimePlayed_rb.Name = "TimePlayed_rb";
            this.TimePlayed_rb.Size = new System.Drawing.Size(204, 21);
            this.TimePlayed_rb.TabIndex = 3;
            this.TimePlayed_rb.TabStop = true;
            this.TimePlayed_rb.Text = "Temps jugat per un jugador";
            this.TimePlayed_rb.UseVisualStyleBackColor = true;
            // 
            // Ranking_rb
            // 
            this.Ranking_rb.AutoSize = true;
            this.Ranking_rb.Location = new System.Drawing.Point(23, 132);
            this.Ranking_rb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Ranking_rb.Name = "Ranking_rb";
            this.Ranking_rb.Size = new System.Drawing.Size(160, 21);
            this.Ranking_rb.TabIndex = 4;
            this.Ranking_rb.TabStop = true;
            this.Ranking_rb.Text = "Ranking de jugadors";
            this.Ranking_rb.UseVisualStyleBackColor = true;
            // 
            // Characters_rb
            // 
            this.Characters_rb.AutoSize = true;
            this.Characters_rb.Location = new System.Drawing.Point(23, 166);
            this.Characters_rb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Characters_rb.Name = "Characters_rb";
            this.Characters_rb.Size = new System.Drawing.Size(260, 21);
            this.Characters_rb.TabIndex = 5;
            this.Characters_rb.TabStop = true;
            this.Characters_rb.Text = "Personatges utilitzats en una partida";
            this.Characters_rb.UseVisualStyleBackColor = true;
            // 
            // ConnectedList_rb
            // 
            this.ConnectedList_rb.AutoSize = true;
            this.ConnectedList_rb.Location = new System.Drawing.Point(23, 191);
            this.ConnectedList_rb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ConnectedList_rb.Name = "ConnectedList_rb";
            this.ConnectedList_rb.Size = new System.Drawing.Size(155, 21);
            this.ConnectedList_rb.TabIndex = 6;
            this.ConnectedList_rb.TabStop = true;
            this.ConnectedList_rb.Text = "Llista de connectats";
            this.ConnectedList_rb.UseVisualStyleBackColor = true;
            // 
            // MenúExtres
            // 
            this.MenúExtres.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MenúExtres.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Notificacions_btn,
            this.MainMenu_btn,
            this.QueriesMenu_btn});
            this.MenúExtres.Location = new System.Drawing.Point(0, 0);
            this.MenúExtres.Name = "MenúExtres";
            this.MenúExtres.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.MenúExtres.Size = new System.Drawing.Size(800, 28);
            this.MenúExtres.TabIndex = 9;
            this.MenúExtres.Text = "menuStrip1";
            // 
            // Notificacions_btn
            // 
            this.Notificacions_btn.Name = "Notificacions_btn";
            this.Notificacions_btn.Size = new System.Drawing.Size(108, 24);
            this.Notificacions_btn.Text = "Notificacions";
            this.Notificacions_btn.Click += new System.EventHandler(this.Notificacions_btn_Click);
            // 
            // MainMenu_btn
            // 
            this.MainMenu_btn.Name = "MainMenu_btn";
            this.MainMenu_btn.Size = new System.Drawing.Size(78, 24);
            this.MainMenu_btn.Text = "Principal";
            this.MainMenu_btn.Click += new System.EventHandler(this.MainMenu_btn_Click);
            // 
            // QueriesMenu_btn
            // 
            this.QueriesMenu_btn.Name = "QueriesMenu_btn";
            this.QueriesMenu_btn.Size = new System.Drawing.Size(84, 24);
            this.QueriesMenu_btn.Text = "Consultes";
            this.QueriesMenu_btn.Click += new System.EventHandler(this.QueriesMenu_btn_Click);
            // 
            // Opponents_rb
            // 
            this.Opponents_rb.AutoSize = true;
            this.Opponents_rb.Location = new System.Drawing.Point(23, 225);
            this.Opponents_rb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Opponents_rb.Name = "Opponents_rb";
            this.Opponents_rb.Size = new System.Drawing.Size(239, 21);
            this.Opponents_rb.TabIndex = 10;
            this.Opponents_rb.TabStop = true;
            this.Opponents_rb.Text = "Llista de jugadors en qui he jugat";
            this.Opponents_rb.UseVisualStyleBackColor = true;
            // 
            // gameResultsWithPlayers_rb
            // 
            this.gameResultsWithPlayers_rb.AutoSize = true;
            this.gameResultsWithPlayers_rb.Location = new System.Drawing.Point(23, 259);
            this.gameResultsWithPlayers_rb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gameResultsWithPlayers_rb.Name = "gameResultsWithPlayers_rb";
            this.gameResultsWithPlayers_rb.Size = new System.Drawing.Size(237, 21);
            this.gameResultsWithPlayers_rb.TabIndex = 11;
            this.gameResultsWithPlayers_rb.TabStop = true;
            this.gameResultsWithPlayers_rb.Text = "Llista de partides amb x jugadors";
            this.gameResultsWithPlayers_rb.UseVisualStyleBackColor = true;
            this.gameResultsWithPlayers_rb.CheckedChanged += new System.EventHandler(this.gameResultsWithPlayers_rb_CheckedChanged);
            // 
            // dateTimeStart_dt
            // 
            this.dateTimeStart_dt.Location = new System.Drawing.Point(131, 322);
            this.dateTimeStart_dt.Name = "dateTimeStart_dt";
            this.dateTimeStart_dt.Size = new System.Drawing.Size(281, 22);
            this.dateTimeStart_dt.TabIndex = 12;
            // 
            // dateTimeEnd
            // 
            this.dateTimeEnd.Location = new System.Drawing.Point(131, 350);
            this.dateTimeEnd.Name = "dateTimeEnd";
            this.dateTimeEnd.Size = new System.Drawing.Size(281, 22);
            this.dateTimeEnd.TabIndex = 13;
            // 
            // GameListInterval_rb
            // 
            this.GameListInterval_rb.AutoSize = true;
            this.GameListInterval_rb.Location = new System.Drawing.Point(25, 296);
            this.GameListInterval_rb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GameListInterval_rb.Name = "GameListInterval_rb";
            this.GameListInterval_rb.Size = new System.Drawing.Size(279, 21);
            this.GameListInterval_rb.TabIndex = 14;
            this.GameListInterval_rb.TabStop = true;
            this.GameListInterval_rb.Text = "Partides jugades a un interval de temps";
            this.GameListInterval_rb.UseVisualStyleBackColor = true;
            // 
            // TimeInterval1_lbl
            // 
            this.TimeInterval1_lbl.AutoSize = true;
            this.TimeInterval1_lbl.Location = new System.Drawing.Point(16, 327);
            this.TimeInterval1_lbl.Name = "TimeInterval1_lbl";
            this.TimeInterval1_lbl.Size = new System.Drawing.Size(109, 17);
            this.TimeInterval1_lbl.TabIndex = 15;
            this.TimeInterval1_lbl.Text = "Inici del interval:";
            // 
            // TimeInterval2_lbl
            // 
            this.TimeInterval2_lbl.AutoSize = true;
            this.TimeInterval2_lbl.Location = new System.Drawing.Point(12, 355);
            this.TimeInterval2_lbl.Name = "TimeInterval2_lbl";
            this.TimeInterval2_lbl.Size = new System.Drawing.Size(115, 17);
            this.TimeInterval2_lbl.TabIndex = 16;
            this.TimeInterval2_lbl.Text = "Final del interval:";
            // 
            // QueriesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TimeInterval2_lbl);
            this.Controls.Add(this.TimeInterval1_lbl);
            this.Controls.Add(this.GameListInterval_rb);
            this.Controls.Add(this.dateTimeEnd);
            this.Controls.Add(this.dateTimeStart_dt);
            this.Controls.Add(this.gameResultsWithPlayers_rb);
            this.Controls.Add(this.Opponents_rb);
            this.Controls.Add(this.ConnectedList_rb);
            this.Controls.Add(this.Characters_rb);
            this.Controls.Add(this.Ranking_rb);
            this.Controls.Add(this.TimePlayed_rb);
            this.Controls.Add(this.queries_tb);
            this.Controls.Add(this.Send_btn);
            this.Controls.Add(this.QueryGrid);
            this.Controls.Add(this.MenúExtres);
            this.MainMenuStrip = this.MenúExtres;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "QueriesForm";
            this.Text = "QueriesForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QueriesForm_FormClosing);
            this.Load += new System.EventHandler(this.QueriesForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.QueryGrid)).EndInit();
            this.MenúExtres.ResumeLayout(false);
            this.MenúExtres.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView QueryGrid;
        private System.Windows.Forms.Button Send_btn;
        private System.Windows.Forms.TextBox queries_tb;
        private System.Windows.Forms.RadioButton TimePlayed_rb;
        private System.Windows.Forms.RadioButton Ranking_rb;
        private System.Windows.Forms.RadioButton Characters_rb;
        private System.Windows.Forms.RadioButton ConnectedList_rb;
        private System.Windows.Forms.MenuStrip MenúExtres;
        private System.Windows.Forms.ToolStripMenuItem Notificacions_btn;
        private System.Windows.Forms.ToolStripMenuItem MainMenu_btn;
        private System.Windows.Forms.ToolStripMenuItem QueriesMenu_btn;
        private System.Windows.Forms.RadioButton Opponents_rb;
        private System.Windows.Forms.RadioButton gameResultsWithPlayers_rb;
        private System.Windows.Forms.DateTimePicker dateTimeStart_dt;
        private System.Windows.Forms.DateTimePicker dateTimeEnd;
        private System.Windows.Forms.RadioButton GameListInterval_rb;
        private System.Windows.Forms.Label TimeInterval1_lbl;
        private System.Windows.Forms.Label TimeInterval2_lbl;
    }
}