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
            this.createGame_rb = new System.Windows.Forms.RadioButton();
            this.showGames_rb = new System.Windows.Forms.RadioButton();
            this.MenúExtres = new System.Windows.Forms.MenuStrip();
            this.Notificacions_btn = new System.Windows.Forms.ToolStripMenuItem();
            this.NewParty_btn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.QueryGrid)).BeginInit();
            this.MenúExtres.SuspendLayout();
            this.SuspendLayout();
            // 
            // QueryGrid
            // 
            this.QueryGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.QueryGrid.Location = new System.Drawing.Point(430, 25);
            this.QueryGrid.Name = "QueryGrid";
            this.QueryGrid.RowTemplate.Height = 24;
            this.QueryGrid.Size = new System.Drawing.Size(358, 413);
            this.QueryGrid.TabIndex = 0;
            this.QueryGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.QueryGrid_CellClick);
            // 
            // Send_btn
            // 
            this.Send_btn.Location = new System.Drawing.Point(278, 395);
            this.Send_btn.Name = "Send_btn";
            this.Send_btn.Size = new System.Drawing.Size(75, 23);
            this.Send_btn.TabIndex = 1;
            this.Send_btn.Text = "Envia";
            this.Send_btn.UseVisualStyleBackColor = true;
            this.Send_btn.Click += new System.EventHandler(this.Send_btn_Click);
            // 
            // queries_tb
            // 
            this.queries_tb.Location = new System.Drawing.Point(149, 79);
            this.queries_tb.Name = "queries_tb";
            this.queries_tb.Size = new System.Drawing.Size(191, 22);
            this.queries_tb.TabIndex = 2;
            // 
            // TimePlayed_rb
            // 
            this.TimePlayed_rb.AutoSize = true;
            this.TimePlayed_rb.Location = new System.Drawing.Point(149, 133);
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
            this.Ranking_rb.Location = new System.Drawing.Point(149, 175);
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
            this.Characters_rb.Location = new System.Drawing.Point(149, 220);
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
            this.ConnectedList_rb.Location = new System.Drawing.Point(149, 262);
            this.ConnectedList_rb.Name = "ConnectedList_rb";
            this.ConnectedList_rb.Size = new System.Drawing.Size(155, 21);
            this.ConnectedList_rb.TabIndex = 6;
            this.ConnectedList_rb.TabStop = true;
            this.ConnectedList_rb.Text = "Llista de connectats";
            this.ConnectedList_rb.UseVisualStyleBackColor = true;
            // 
            // createGame_rb
            // 
            this.createGame_rb.AutoSize = true;
            this.createGame_rb.Location = new System.Drawing.Point(150, 303);
            this.createGame_rb.Name = "createGame_rb";
            this.createGame_rb.Size = new System.Drawing.Size(112, 21);
            this.createGame_rb.TabIndex = 7;
            this.createGame_rb.TabStop = true;
            this.createGame_rb.Text = "Crear partida";
            this.createGame_rb.UseVisualStyleBackColor = true;
            // 
            // showGames_rb
            // 
            this.showGames_rb.AutoSize = true;
            this.showGames_rb.Location = new System.Drawing.Point(150, 330);
            this.showGames_rb.Name = "showGames_rb";
            this.showGames_rb.Size = new System.Drawing.Size(117, 21);
            this.showGames_rb.TabIndex = 8;
            this.showGames_rb.TabStop = true;
            this.showGames_rb.Text = "Llista partides";
            this.showGames_rb.UseVisualStyleBackColor = true;
            // 
            // MenúExtres
            // 
            this.MenúExtres.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MenúExtres.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Notificacions_btn});
            this.MenúExtres.Location = new System.Drawing.Point(0, 0);
            this.MenúExtres.Name = "MenúExtres";
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
            // NewParty_btn
            // 
            this.NewParty_btn.Location = new System.Drawing.Point(91, 357);
            this.NewParty_btn.Name = "NewParty_btn";
            this.NewParty_btn.Size = new System.Drawing.Size(149, 69);
            this.NewParty_btn.TabIndex = 10;
            this.NewParty_btn.Text = "Nova Partida";
            this.NewParty_btn.UseVisualStyleBackColor = true;
            this.NewParty_btn.Click += new System.EventHandler(this.NewParty_btn_Click);
            // 
            // QueriesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.NewParty_btn);
            this.Controls.Add(this.showGames_rb);
            this.Controls.Add(this.createGame_rb);
            this.Controls.Add(this.ConnectedList_rb);
            this.Controls.Add(this.Characters_rb);
            this.Controls.Add(this.Ranking_rb);
            this.Controls.Add(this.TimePlayed_rb);
            this.Controls.Add(this.queries_tb);
            this.Controls.Add(this.Send_btn);
            this.Controls.Add(this.QueryGrid);
            this.Controls.Add(this.MenúExtres);
            this.MainMenuStrip = this.MenúExtres;
            this.Name = "QueriesForm";
            this.Text = "QueriesForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QueriesForm_FormClosing);
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
        private System.Windows.Forms.RadioButton createGame_rb;
        private System.Windows.Forms.RadioButton showGames_rb;
        private System.Windows.Forms.MenuStrip MenúExtres;
        private System.Windows.Forms.ToolStripMenuItem Notificacions_btn;
        private System.Windows.Forms.Button NewParty_btn;
    }
}