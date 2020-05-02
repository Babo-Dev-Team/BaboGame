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
using System.Threading;

namespace BaboGameClient
{
    public partial class QueriesForm : Form
    {
        ServerHandler serverHandler;

        // necessitem una ref. al Notification Worker per modificar el camp 
        // DataGridUpdateRequested segons el data grid
        NotificationWorker notificationWorker;

        public QueriesForm(ServerHandler serverHandler, NotificationWorker notificationWorker)
        {
            InitializeComponent();
            this.serverHandler = serverHandler;
            this.notificationWorker = notificationWorker;
        }

        //------------------------------------------------
        // DELEGATE METHODS (USE FROM NOTIFICATION WORKER)
        //------------------------------------------------

        public void UpdateConnectedList(List<ConnectedUser> connectedList)
        {
            this.QueryGrid.Rows.Clear();
            this.QueryGrid.Columns.Clear();
            this.QueryGrid.Columns.Add("username", "Usuari");
            this.QueryGrid.Columns.Add("ID", "ID");

            for (int i = 0; i < connectedList.Count; i++)// array rows
            {
                string[] row = new string[2];
                row[0] = connectedList[i].Name;
                row[1] = connectedList[i].Id.ToString();
                this.QueryGrid.Rows.Add(row);
            }
        }

        public void UpdateCharactersList(string[][] gameCharacters)
        {
            this.QueryGrid.Rows.Clear();
            this.QueryGrid.Columns.Clear();

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

        public void TimePlayedPopup(string TimePlayed)
        {
            if (TimePlayed == null)
                MessageBox.Show("Aquest jugador no existeix o no ha jugat res");
            else
                MessageBox.Show("El jugador " + queries_tb.Text + " ha jugat el temps següent:" + TimePlayed);
        }

        public void CreatePartyPopup(string response)
        {
            if (response == "OK")
                MessageBox.Show("La partida s'ha creat correctament");
            else
                MessageBox.Show(response);
        }
        //------------------------------------------------
        // REGULAR METHODS, USE FROM UI
        //------------------------------------------------

        private void Send_btn_Click(object sender, EventArgs e)
        {
            // Demanem al server handler que envii el request.
            // En aquest cas el server handler ja no l'hem de cridar més
            // perquè l'actualització ens arribarà pel Notification Worker
            if (TimePlayed_rb.Checked)
            {
                if (string.IsNullOrWhiteSpace(queries_tb.Text))
                {
                    MessageBox.Show("Els camps estan buits!");
                    return;
                }
                serverHandler.RequestTimePlayed(queries_tb.Text);
            }

            // TODO: Modificar la resta de queries com la primera
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
            else if (Characters_rb.Checked) //Modificat a la nova versió - Albert
            {
                notificationWorker.DataGridUpdateRequested = 3;
                if (string.IsNullOrWhiteSpace(queries_tb.Text))
                {
                    MessageBox.Show("Els camps estan buits!");
                    return;
                }
                try
                {
                    Convert.ToInt32(queries_tb.Text); //Detecta que hagi posat un nombre i sinò fa saltar el try catch
                    serverHandler.RequestGameCharacters(queries_tb.Text);   
                }
                catch
                {
                    MessageBox.Show("Introdueix una ID de partida al quadre de text");
                }
            }

            // Aquesta ja està modificada: demanem al server handler que ens fagi el request
            // i indiquem al Notification Worker el tipus d'info que esperem rebre per al data grid
            else if (ConnectedList_rb.Checked)
            {
                notificationWorker.DataGridUpdateRequested = 6;
                serverHandler.RequestConnected();
            }

            else if (createGame_rb.Checked) //Modificat a la nova versió - Albert
            {
                if(queries_tb.Text.Length == 0)
                {
                    MessageBox.Show("Escriu el nom de la partida!");
                }
                else
                {
                    serverHandler.RequestCreateParty(queries_tb.Text);
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
            notificationWorker.DataGridUpdateRequested = 0;
            serverHandler.Disconnect();
        }
    }
}
