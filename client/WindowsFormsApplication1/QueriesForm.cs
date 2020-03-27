using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BaboGameClient
{
    public partial class QueriesForm : Form
    {
        ServerHandler serverHandler;
        public QueriesForm(ServerHandler serverHandler)
        {
            InitializeComponent();
            this.serverHandler = serverHandler;
        }

        private void Send_btn_Click(object sender, EventArgs e)
        {
            if(TimePlayed_rb.Checked)
            {
                string TimePlayed;
                TimePlayed = serverHandler.GetTimePlayed(queries_tb.Text);
                if (TimePlayed == null)
                    MessageBox.Show("Aquest jugador no existeix o no ha jugat res");
                else
                    MessageBox.Show("El jugador " + queries_tb + "ha jugat el temps següent:" + TimePlayed);
            }
            else if (Ranking_rb.Checked)
            {
                string[][] ranking;
                ranking = serverHandler.GetRanking();
                QueryGrid.DataSource = ranking;
                QueryGrid.Refresh();
            }
            else
            {
                string[][] GameCharacters;
                GameCharacters = serverHandler.GetGameCharacters(queries_tb.Text);
                QueryGrid.DataSource = GameCharacters;
                QueryGrid.Refresh();
            }
        }
    }
}
