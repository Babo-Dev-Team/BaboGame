using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.Json;

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
            if (TimePlayed_rb.Checked)
            {
            if (string.IsNullOrWhiteSpace(queries_tb.Text))
                {
                    MessageBox.Show("Els camps estan buits!");
                    return;
                }
                string TimePlayed;
                TimePlayed = serverHandler.GetTimePlayed(queries_tb.Text);
                if (TimePlayed == null)
                    MessageBox.Show("Aquest jugador no existeix o no ha jugat res");
                else
                    MessageBox.Show("El jugador " + queries_tb.Text + " ha jugat el temps següent:" + TimePlayed);
            }
            else if (Ranking_rb.Checked)
            {
                QueryGrid.Rows.Clear();
                QueryGrid.Columns.Clear();
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
            else if (Characters_rb.Checked)
            {
                
                if (string.IsNullOrWhiteSpace(queries_tb.Text))
                {
                    MessageBox.Show("Els camps estan buits!");
                    return;
                }
                QueryGrid.Rows.Clear();
                QueryGrid.Columns.Clear();
                string[][] gameCharacters;
                try
                {
                    Convert.ToInt32(queries_tb.Text); //Detecta que hagi posat un nombre i sinò fa saltar el try catch
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
                catch
                {
                    MessageBox.Show("Introdueix una ID de partida al quadre de text");
                }
            }
            else if (ConnectedList_rb.Checked)
            {
                QueryGrid.Rows.Clear();
                QueryGrid.Columns.Clear();
                QueryGrid.Columns.Add("Users Connected", "Usuaris Connectats");
                string[] connected;
                connected = serverHandler.GetConnected();

                for (int i = 0; i < connected.GetLength(0); i++)// array rows
                {
                    string row = connected[i];

                    QueryGrid.Rows.Add(row);
                }
                QueryGrid.Refresh();
            }
            else
                MessageBox.Show("Selecciona alguna opció");
        }

        private void QueriesForm_FormClosing(object sender, EventArgs args)
        {
            MessageBox.Show("Desconnectant-se...");
            serverHandler.Disconnect();
        }
    }
}
