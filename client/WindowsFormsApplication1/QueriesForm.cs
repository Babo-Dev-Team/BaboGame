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
                QueryGrid.Columns.Add("username", "Usuari");
                QueryGrid.Columns.Add("Wins", "Partides guanyades");
                string[][] ranking;
                ranking = serverHandler.GetRanking();

                for (int i = 0; i < ranking.GetLength(0); i++)// array rows
                {
                    string[] row = new string[ranking[i].GetLength(0)];

                    for (int j = 0; j < ranking[i].GetLength(0); j++)
                    {
                        row[j] = ranking[i][j];
                    }

                    QueryGrid.Rows.Add(row);
                }
                QueryGrid.Refresh();
            }
            else
            {
                string[][] gameCharacters;
                gameCharacters = serverHandler.GetGameCharacters(queries_tb.Text);
                QueryGrid.Columns.Add("username", "Usuari");
                QueryGrid.Columns.Add("character", "Personatge");


                for (int i = 0; i < gameCharacters.GetLength(0); i++)// array rows
                {
                    string[] row = new string[gameCharacters[i].GetLength(0)];

                    for (int j = 0; j < gameCharacters[i].GetLength(0); j++)
                    {
                        row[j] = gameCharacters[i][j];
                    }

                    QueryGrid.Rows.Add(row);
                }
                QueryGrid.Refresh();
            }
        }
    }
}
