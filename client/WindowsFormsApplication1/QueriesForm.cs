using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                List<ConnectedUser> connectedList = serverHandler.GetConnected();
                //QueryGrid.DataSource = connectedList;
                //QueryGrid.Columns[0].HeaderText = "Usuari";
                //QueryGrid.Columns[1].HeaderText = "ID";
                QueryGrid.Columns.Add("username", "Usuari");
                QueryGrid.Columns.Add("ID", "ID");


                for (int i = 0; i < connectedList.Count; i++)// array rows
                {
                    string[] row = new string[2];
                    row[0] = connectedList[i].Name;
                    row[1] = connectedList[i].Id.ToString();
                    QueryGrid.Rows.Add(row);
                }
            }
            else if (createGame_rb.Checked)
            {
                if(queries_tb.Text.Length == 0)
                {
                    MessageBox.Show("Escriu el nom de la partida!");
                }
                else
                {
                    string response = serverHandler.CreateGame(queries_tb.Text);
                    MessageBox.Show(response);
                }
            }

            else if (showGames_rb.Checked)
            {
                QueryGrid.Rows.Clear();
                QueryGrid.Columns.Clear();
                List<PreGameState> gameTable = serverHandler.GetGameTable();
                QueryGrid.Columns.Add("ID", "ID");
                QueryGrid.Columns.Add("Nom", "Nom");
                QueryGrid.Columns.Add("Creador", "Creador");
                QueryGrid.Columns.Add("Participants", "Participants");
                QueryGrid.Columns.Add("Estat", "Estat");
                                             
                for (int i = 0; i < gameTable.Count; i++)// array rows
                {
                    string[] row = new string[5];
                    row[0] = gameTable[i].Id.ToString();
                    row[1] = gameTable[i].Name;
                    row[2] = gameTable[i].Creator;
                    row[3] = gameTable[i].UserCount.ToString();
                    if (gameTable[i].Playing == 1)
                    {
                        row[4] = "En Curs";
                    }
                    else
                    {
                        row[4] = "Oberta";
                    }
                    QueryGrid.Rows.Add(row);
                }
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
