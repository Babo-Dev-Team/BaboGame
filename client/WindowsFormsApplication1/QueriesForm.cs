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

        PictureBox NotificationIcon = new PictureBox();

        //Elements del menú dels personatges seleccionats
        DataGridView PlayersSelected_dg = new DataGridView();
        TextBox NewPartyName_tb = new TextBox();
        Button CreateParty_btn = new Button();
        Button NewPartyBack_btn = new Button();
        Label NewPartyName_lbl = new Label();

        //Elements del menú de la partida
        Label PartyName_lbl = new Label();
        PictureBox character_pb = new PictureBox();
        Button LeftChar_btn = new Button();
        Button RightChar_btn = new Button();
        Button CancelGame_btn = new Button();
        Button SelectChar_btn = new Button();
        Button StartGame_btn = new Button();
        Button QuitGame_btn = new Button();
        RichTextBox ChatGame_rtb = new RichTextBox();

        string[] characterSelected = { "Babo", "Limax", "Quim", "Swalot" };
        int charSelectedPos = 0;

        //Variable que diferenciar a quin menú estàs situat
        int ScreenSelected = 0;
        string gameName;
        ToolStripItem notificationSelection;

        public QueriesForm(ServerHandler serverHandler, NotificationWorker notificationWorker)
        {
            InitializeComponent();
            this.serverHandler = serverHandler;
            this.notificationWorker = notificationWorker;
            NotificationIcon.ImageLocation = "../../../Pictures/Layouts/Babo down hit.png";
            NotificationIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            NotificationIcon.Load();
            NotificationIcon.Refresh();

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //Creació dels objectes del menú de la tria de la llista de connectats
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            //Grid dels jugadors seleccionats
            PlayersSelected_dg.Size = new Size(250, 200);
            PlayersSelected_dg.Location = new Point(25, 75);
            PlayersSelected_dg.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            PlayersSelected_dg.Name = "PlayerSelected_dg";
            PlayersSelected_dg.TabIndex = 0;
            PlayersSelected_dg.RowTemplate.Height = 20;
            PlayersSelected_dg.Enabled = true;
            PlayersSelected_dg.Visible = false;
            this.Controls.Add(PlayersSelected_dg);
            PlayersSelected_dg.CellClick += new DataGridViewCellEventHandler(this.PlayersSelected_dg_CellClick);

            //TextBox del nom de la partida
            NewPartyName_tb.Location = new Point(125, 35);
            NewPartyName_tb.Visible = false;
            this.Controls.Add(NewPartyName_tb);

            //Label del nom de la partida
            NewPartyName_lbl.Location = new Point(25, 35);
            NewPartyName_lbl.Visible = false;
            NewPartyName_lbl.Text = "Nom de la partida:";
            this.Controls.Add(NewPartyName_lbl);

            //Button de crear la partida
            CreateParty_btn.Location = new Point(125, 300);
            CreateParty_btn.Text = "Crea Partida";
            CreateParty_btn.Size = new Size(120, 60);
            CreateParty_btn.Visible = false;
            this.Controls.Add(CreateParty_btn);
            CreateParty_btn.Click += new EventHandler(this.CreateParty_btn_Click);

            //Button per sortir del menú de seleccionar jugadors
            NewPartyBack_btn.Location = new Point(25, 300);
            NewPartyBack_btn.Text = "Surt";
            NewPartyBack_btn.Size = new Size(80, 60);
            NewPartyBack_btn.Visible = false;
            this.Controls.Add(NewPartyBack_btn);
            NewPartyBack_btn.Click += new EventHandler(this.NewPartyBack_btn_Click);

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //Creació del menú de selecció de personatges
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            //Label del nom de partida seleccionat
            PartyName_lbl.Location = new Point(25, 35);
            PartyName_lbl.Text = "Partida: Game1";
            PartyName_lbl.Visible = false;
            this.Controls.Add(PartyName_lbl);

            //Buttons per canviar el personatge
            LeftChar_btn.Location = new Point(25,100);
            LeftChar_btn.Text = "Left";
            LeftChar_btn.Visible = false;
            LeftChar_btn.Size = new Size(60, 60);
            this.Controls.Add(LeftChar_btn);
            LeftChar_btn.Click += new EventHandler(this.LeftChar_btn_Click);

            RightChar_btn.Location = new Point(240, 100);
            RightChar_btn.Text = "Right";
            RightChar_btn.Visible = false;
            RightChar_btn.Size = new Size(60, 60);
            this.Controls.Add(RightChar_btn);
            RightChar_btn.Click += new EventHandler(this.RightChar_btn_Click);

            //PictureBox de la imatge del personatge
            character_pb.Location = new Point(90, 60);
            character_pb.Size = new Size(140, 140);
            character_pb.ImageLocation = "../../../Pictures/Characters/Babo stop.gif";
            character_pb.Visible = false;
            character_pb.SizeMode = PictureBoxSizeMode.Zoom;
            character_pb.Refresh();
            this.Controls.Add(character_pb);

            //Button per cancellar la partida
            CancelGame_btn.Location = new Point(225, 300);
            CancelGame_btn.Text = "Cancel·lar";
            CancelGame_btn.Size = new Size(80, 25);
            CancelGame_btn.Visible = false;
            this.Controls.Add(CancelGame_btn);
            CancelGame_btn.Click += new EventHandler(this.CancelGame_btn_Click);

            //Button per acceptar la partida
            StartGame_btn.Location = new Point(225, 330);
            StartGame_btn.Text = "Acceptar";
            StartGame_btn.Size = new Size(80, 30);
            StartGame_btn.Visible = false;
            this.Controls.Add(StartGame_btn);
            StartGame_btn.Click += new EventHandler(this.StartGame_btn_Click);

            //Button per seleccionar el personatge
            SelectChar_btn.Location = new Point(25, 300);
            SelectChar_btn.Text = "Seleccionar Personatge";
            SelectChar_btn.Size = new Size(80, 60);
            SelectChar_btn.Visible = false;
            this.Controls.Add(SelectChar_btn);
            SelectChar_btn.Click += new EventHandler(this.SelectChar_btn_Click);

            //Button per sortir de la partida
            QuitGame_btn.Location = new Point(225, 300);
            QuitGame_btn.Text = "Sortir";
            QuitGame_btn.Size = new Size(80, 25);
            QuitGame_btn.Visible = false;
            this.Controls.Add(QuitGame_btn);
            QuitGame_btn.Click += new EventHandler(this.QuitGame_btn_Click);

            //RichTextBox
            ChatGame_rtb.Visible = false;
            ChatGame_rtb.Size = new Size(250,350);
            ChatGame_rtb.Location = new Point(300,25);
            this.Controls.Add(ChatGame_rtb);
            ChatGame_rtb.Text = "";
        }

        //------------------------------------------------
        // DELEGATE METHODS (USE FROM NOTIFICATION WORKER)
        //------------------------------------------------

        //Actualitza la llista de connectats
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

            //Borra els usuaris de la llista de usuaris seleccionats que s'hagin desconnectat
            if ((ScreenSelected == 1) && (notificationWorker.DataGridUpdateRequested == 6))
            {
                int i = 0;
                for (i = 0; i < PlayersSelected_dg.RowCount - 1; i++)
                {
                    bool found = false;
                    int j = 0;
                    for (j = 0; j < QueryGrid.RowCount-1; j++)
                    {
                        if ((QueryGrid[0, j].Value.ToString() == PlayersSelected_dg[0, i].Value.ToString()) &&
                            (QueryGrid[1, j].Value.ToString() == PlayersSelected_dg[1, i].Value.ToString()))
                            found = true;
                    }
                    if (!found)
                    {
                        try
                        {
                            PlayersSelected_dg.Rows.RemoveAt(i);
                        }
                        catch { }
                    }
                }
            }
        }

        //Actualitza la llista de personatges en una partida
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

        //Actualitza el ranking
        public void UpdateRanking(string[][] ranking)                   
        {
            QueryGrid.Rows.Clear();
            QueryGrid.Columns.Clear();
            QueryGrid.Columns.Add("username", "Usuari");
            QueryGrid.Columns.Add("Wins", "Partides guanyades");

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

        //Envia el missatge del temps jugat
        public void TimePlayedPopup(string TimePlayed)
        {
            if (TimePlayed == null)
                MessageBox.Show("Aquest jugador no existeix o no ha jugat res");
            else
                MessageBox.Show("El jugador " + queries_tb.Text + " ha jugat el temps següent:" + TimePlayed);
        }

        //Enviar el missatge de crear correctament
        public void SignUpPopup(string response)
        {
            if (response == "OK")
                MessageBox.Show("L'usuari s'ha creat correctament");
            else
                MessageBox.Show(response);
        }
            
        //Enviar el missatge de crear la partida
        public void CreatePartyPopup(string response)
        {
            if (response == "OK")
            {
                MessageBox.Show("La partida s'ha creat correctament");

                //Fa apareixer els objectes del menú principal
                QueryGrid.Visible = false;
                ScreenSelected = 2;
                Send_btn.Visible = false;
                Characters_rb.Visible = false;
                ConnectedList_rb.Visible = false;
                createGame_rb.Visible = false;
                Ranking_rb.Visible = false;
                showGames_rb.Visible = false;
                TimePlayed_rb.Visible = false;
                queries_tb.Visible = false;
                NewParty_btn.Visible = false;

                //Desactiva els objectes del menú anterior
                PlayersSelected_dg.Visible = false;
                NewPartyName_tb.Visible = false;
                NewPartyName_lbl.Visible = false;
                NewPartyBack_btn.Visible = false;
                CreateParty_btn.Visible = false;

                //Desactiva els objectes de la selecció del personatge
                character_pb.Visible = true;
                PartyName_lbl.Visible = true;
                RightChar_btn.Visible = true;
                LeftChar_btn.Visible = true;
                SelectChar_btn.Visible = true;
                StartGame_btn.Visible = true;
                CancelGame_btn.Visible = true;
                QuitGame_btn.Visible = false;
                ChatGame_rtb.Visible = true;
                PartyName_lbl.Text = "Partida: " + gameName;
            }
            else if (response == "ALONE")
                MessageBox.Show("No has escollit a ningú a part de tú. Les partides són multijugadors");
            else
                MessageBox.Show(response);
        }

        //Envia la notificació sobre que t'han convidat
        public void InvitationNotificationMessage (string gameName, string creatorName)
        {
            ToolStripItem[] InvitationSelection = new ToolStripItem[2];
            InvitationSelection[0] = new ToolStripButton("Acceptar");
            InvitationSelection[1] = new ToolStripButton("Rebutjar");
            ToolStripItem Invitation = new ToolStripMenuItem("'" + creatorName + "' t'ha invitat a la partida '" + gameName + "'", NotificationIcon.Image, InvitationSelection);
            InvitationSelection[0].Click += delegate(object sender,EventArgs e) { AcceptInvitation(sender, e, gameName, Invitation); };
            InvitationSelection[1].Click += delegate (object sender, EventArgs e) { RejectInvitation(sender, e, gameName, Invitation); };
            Invitation.Image = NotificationIcon.Image;
            Invitation.Tag = gameName;
            Notificacions_btn.DropDownItems.Add(Invitation);
            Notificacions_btn.BackColor = Color.LightGreen;
        }

        //S'ha confirmat l'acceptació de la partida
        public void AcceptedGamePopup()
        {
            MessageBox.Show("Has entrat a la partida");

            //Fa apareixer els objectes del menú principal
            QueryGrid.Visible = false;
            ScreenSelected = 2;
            Send_btn.Visible = false;
            Characters_rb.Visible = false;
            ConnectedList_rb.Visible = false;
            createGame_rb.Visible = false;
            Ranking_rb.Visible = false;
            showGames_rb.Visible = false;
            TimePlayed_rb.Visible = false;
            queries_tb.Visible = false;
            NewParty_btn.Visible = false;

            //Desactiva els objectes del menú anterior
            PlayersSelected_dg.Visible = false;
            NewPartyName_tb.Visible = false;
            NewPartyName_lbl.Visible = false;
            NewPartyBack_btn.Visible = false;
            CreateParty_btn.Visible = false;

            //Desactiva els objectes de la selecció del personatge
            character_pb.Visible = true;
            PartyName_lbl.Visible = true;
            RightChar_btn.Visible = true;
            LeftChar_btn.Visible = true;
            SelectChar_btn.Visible = true;
            StartGame_btn.Visible = false;
            CancelGame_btn.Visible = false;
            QuitGame_btn.Visible = true;
            ChatGame_rtb.Visible = true;
            PartyName_lbl.Text = "Partida: " + gameName;
            Notificacions_btn.DropDownItems.Remove(notificationSelection);
        }

        //S'ha confirmat el rebuig a la partida
        public void RejectGamePopup()
        {
            MessageBox.Show("Has rebutjat correctament la partida");
            Notificacions_btn.DropDownItems.Remove(notificationSelection);
        }

        //Hi ha hagut algún error rebutjant la partida
        public void FailResponseGamePopup()
        {
            MessageBox.Show("No s'ha pogut acceptar/rebutjar la invitació");
        }

        //Han començat sense tú
        public void LoseInvitationPopup(string gameName, string creatorName)
        {
            MessageBox.Show("Has perdut la oportunitat de jugar a una partida");
            ToolStripItem[] InvitationSelection = new ToolStripItem[2];
            InvitationSelection[0] = new ToolStripButton("Acceptar");
            InvitationSelection[1] = new ToolStripButton("Rebutjar");
            ToolStripItem Invitation = new ToolStripMenuItem("'" + creatorName + "' t'ha invitat a la partida '" + gameName + "'", NotificationIcon.Image, InvitationSelection);
            InvitationSelection[0].Click += delegate (object sender, EventArgs e) { AcceptInvitation(sender, e, gameName, Invitation); };
            InvitationSelection[1].Click += delegate (object sender, EventArgs e) { RejectInvitation(sender, e, gameName, Invitation); };
            Invitation.Image = NotificationIcon.Image;
            Invitation.Tag = gameName;
            Notificacions_btn.DropDownItems.Remove(Invitation);
        }

        //Resposta de l'usuari per acceptar la invitació
        private void AcceptInvitation(object sender, EventArgs e, string gameName, ToolStripItem invitation)
        {
            serverHandler.RequestAcceptInvitation(gameName);
            this.gameName = gameName;
            notificationSelection = invitation;
        }

        //Resposta de l'usuari per rebutjar la invitació
        private void RejectInvitation(object sender, EventArgs e, string gameName, ToolStripItem invitation)
        {
            serverHandler.RequestRejectInvitation(gameName);
            notificationSelection = invitation;
        }

        //Actualització de l'estat de la partida
        public void GameStateUpdate(List<PreGameStateUser> gameState)
        {
            ChatGame_rtb.Text += "ACTUALITZACIÓ \n\n";
            for(int i=0; i < gameState.Count; i++)
            {
                ChatGame_rtb.Text += "ID = " + gameState[i].Id + "\n";
                ChatGame_rtb.Text += "Usuari = " + gameState[i].UserName + "\n";
                if (gameState[i].CharName == null)
                    ChatGame_rtb.Text += "Personatge = no seleccionat\n";
                else
                    ChatGame_rtb.Text += "Character = " + gameState[i].CharName + "\n";
                if (gameState[i].UserState == 0)
                    ChatGame_rtb.Text += "Estat = pendent d'acceptar";
                else if (gameState[i].UserState == 1)
                    ChatGame_rtb.Text += "Estat = Acceptat";
                else if (gameState[i].UserState == -1)
                    ChatGame_rtb.Text += "Estat = Rebutjat";

                ChatGame_rtb.Text += "\n";
            }
        }

        //Acceptar personatge
        public void AcceptCharacterPopup()
        {
            MessageBox.Show("El teu personatge s'ha seleccionat correctament");
        }

        //Error en seleccionar el personatge
        public void FailCharacterPopup()
        {
            MessageBox.Show("El teu personatge no s'ha pogut seleccionar");
        }

        //Error en seleccionar el personatge
        public void StartGamePopup()
        {
            MessageBox.Show("Comença la partida");
        }

        //Error en no haver escollit tothom personatge
        public void NotAllSelectedPopup()
        {
            MessageBox.Show("Tots els jugadors no han escollit personatge");
        }

        //Popup per cancel·lar el joc
        public void CancelGamePopup(string gameName, string creatorName)
        {
            if(ScreenSelected == 2)
            {
                MessageBox.Show("S'ha cancel·lat la partida");
                //Fa apareixer els objectes del menú principal
                QueryGrid.Visible = true;
                ScreenSelected = 0;
                Send_btn.Visible = true;
                Characters_rb.Visible = true;
                ConnectedList_rb.Visible = true;
                createGame_rb.Visible = true;
                Ranking_rb.Visible = true;
                showGames_rb.Visible = true;
                TimePlayed_rb.Visible = true;
                queries_tb.Visible = true;
                NewParty_btn.Visible = true;

                //Desactiva els objectes del menú anterior
                PlayersSelected_dg.Visible = false;
                NewPartyName_tb.Visible = false;
                NewPartyName_lbl.Visible = false;
                NewPartyBack_btn.Visible = false;
                CreateParty_btn.Visible = false;

                //Desactiva els objectes de la selecció del personatge
                character_pb.Visible = false;
                PartyName_lbl.Visible = false;
                RightChar_btn.Visible = false;
                LeftChar_btn.Visible = false;
                SelectChar_btn.Visible = false;
                StartGame_btn.Visible = false;
                CancelGame_btn.Visible = false;
                QuitGame_btn.Visible = false;
                ChatGame_rtb.Visible = false;
            }
            else
            {
                MessageBox.Show("La partida " + gameName + " s'ha cancel·lat");
                ToolStripItem[] InvitationSelection = new ToolStripItem[2];
                InvitationSelection[0] = new ToolStripButton("Acceptar");
                InvitationSelection[1] = new ToolStripButton("Rebutjar");
                ToolStripItem Invitation = new ToolStripMenuItem("'" + creatorName + "' t'ha invitat a la partida '" + gameName + "'", NotificationIcon.Image, InvitationSelection);
                InvitationSelection[0].Click += delegate (object sender, EventArgs e) { AcceptInvitation(sender, e, gameName, Invitation); };
                InvitationSelection[1].Click += delegate (object sender, EventArgs e) { RejectInvitation(sender, e, gameName, Invitation); };
                Invitation.Image = NotificationIcon.Image;
                Invitation.Tag = gameName;
                Notificacions_btn.DropDownItems.Remove(Invitation);
            }
        }

        //Et fa fora per quedar-te sol en la partida
        public void AlonePlayerPopup()
        {
            MessageBox.Show("T'has quedat sol en la partida, la partida s'ha cancel·lat");
            //Fa apareixer els objectes del menú principal
            QueryGrid.Visible = true;
            ScreenSelected = 0;
            Send_btn.Visible = true;
            Characters_rb.Visible = true;
            ConnectedList_rb.Visible = true;
            createGame_rb.Visible = true;
            Ranking_rb.Visible = true;
            showGames_rb.Visible = true;
            TimePlayed_rb.Visible = true;
            queries_tb.Visible = true;
            NewParty_btn.Visible = true;

            //Desactiva els objectes del menú anterior
            PlayersSelected_dg.Visible = false;
            NewPartyName_tb.Visible = false;
            NewPartyName_lbl.Visible = false;
            NewPartyBack_btn.Visible = false;
            CreateParty_btn.Visible = false;

            //Desactiva els objectes de la selecció del personatge
            character_pb.Visible = false;
            PartyName_lbl.Visible = false;
            RightChar_btn.Visible = false;
            LeftChar_btn.Visible = false;
            SelectChar_btn.Visible = false;
            StartGame_btn.Visible = false;
            CancelGame_btn.Visible = false;
            QuitGame_btn.Visible = false;
            ChatGame_rtb.Visible = false;
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
                
                serverHandler.RequestRanking();
            }
            else if (Characters_rb.Checked) //Modificat a la nova versió - Albert
            {
                //notificationWorker.DataGridUpdateRequested = 3;
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

            /*
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
            */
            else
                MessageBox.Show("Selecciona alguna opció");
        }

        private void QueriesForm_FormClosing(object sender, EventArgs args)
        {
            MessageBox.Show("Desconnectant-se...");
            notificationWorker.DataGridUpdateRequested = 0;
            serverHandler.Disconnect();
        }

        private void Notificacions_btn_Click(object sender, EventArgs e)
        {
            Notificacions_btn.BackColor = Color.LightGray;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //Menú de selecció de jugadors
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //Botó que obre el menú
        //Fa invisibles els objectes innecesàris i fa aparèixer els objectes necessàris d'aquell menú
        private void NewParty_btn_Click(object sender, EventArgs e)
        {
            //Amaga els objectes anteriors
            QueryGrid.Visible = true;
            notificationWorker.DataGridUpdateRequested = 6;
            ScreenSelected = 1;
            serverHandler.RequestConnected();
            Send_btn.Visible = false;
            Characters_rb.Visible = false;
            ConnectedList_rb.Visible = false;
            createGame_rb.Visible = false;
            Ranking_rb.Visible = false;
            showGames_rb.Visible = false;
            TimePlayed_rb.Visible = false;
            queries_tb.Visible = false;
            NewParty_btn.Visible = false;

            //Activa el grid dels jugadors seleccionats
            PlayersSelected_dg.Visible = true;
            PlayersSelected_dg.Rows.Clear();
            PlayersSelected_dg.Columns.Clear();
            PlayersSelected_dg.Columns.Add("username", "Usuari");
            PlayersSelected_dg.Columns.Add("ID", "ID");
            PlayersSelected_dg.Refresh();

            //Activa el NewPartyName_tb, el NewPartyName_lbl, el NewPartyBack_btn i el CreateParty_btn
            NewPartyName_tb.Visible = true;
            NewPartyName_lbl.Visible = true;
            NewPartyBack_btn.Visible = true;
            CreateParty_btn.Visible = true;

            //Desactiva els objectes de la selecció del personatge
            character_pb.Visible = false;
            PartyName_lbl.Visible = false;
            RightChar_btn.Visible = false;
            LeftChar_btn.Visible = false;
            SelectChar_btn.Visible = false;
            StartGame_btn.Visible = false;
            CancelGame_btn.Visible = false;
            QuitGame_btn.Visible = false;
            ChatGame_rtb.Visible = false;
        }

        //Selecció de un element de la llista dels jugadors elegits
        //Quan cliques un jugador el borres
        private void PlayersSelected_dg_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                PlayersSelected_dg.Rows.RemoveAt(e.RowIndex);
            }
            catch { }
        }

        //Selecció d'un elemeny de la llista de connectats
        //Quan cliques una cel·la passes l'element a la llista d'elegits
        private void QueryGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if((notificationWorker.DataGridUpdateRequested == 6)&&(ScreenSelected == 1))
            {
                if (e.RowIndex >= 0)
                {
                    try
                    {
                        string[] rowPlayer = { QueryGrid[0, e.RowIndex].Value.ToString(), QueryGrid[1, e.RowIndex].Value.ToString() };
                        string[] rowPlayerInList = new string[2];
                        bool found = false;
                        int i = 0;
                        while ((i < PlayersSelected_dg.RowCount) && (!found))
                        {
                            rowPlayerInList[0] = QueryGrid[0, i].Value.ToString();
                            rowPlayerInList[1] = QueryGrid[1, i].Value.ToString();
                            if (rowPlayer == rowPlayerInList)
                                found = true;
                            i++;
                        }
                        if (!found)
                        {
                            PlayersSelected_dg.Rows.Add(rowPlayer);
                        }
                    }
                    catch
                    { }
                }
            }
        }

        //Selecciona el botó de crear partida
        private void CreateParty_btn_Click(object sender, EventArgs e)
        {
            if (NewPartyName_tb.Text.Length == 0)
            {
                MessageBox.Show("Escriu el nom de la partida!");
            }
            else if (PlayersSelected_dg.RowCount <= 1)
            {
                MessageBox.Show("No pots jugar sol. Invita algú de la llista clicant a una cel·la de la fila del jugador que vols seleccionar");
            }
            else
            {
                string[] Players = new string[PlayersSelected_dg.RowCount];
                for(int i=0; i < PlayersSelected_dg.RowCount - 1 ;i++)
                {
                    Players[i] = PlayersSelected_dg[0, i].Value.ToString();
                }
                serverHandler.RequestCreateParty(NewPartyName_tb.Text, Players);
                gameName = NewPartyName_tb.Text;
            }
        }

        //Selecciona el botó de sortir del menú de selecció de jugadors
        private void NewPartyBack_btn_Click(object sender, EventArgs e)
        {
            //Fa apareixer els objectes del menú principal
            QueryGrid.Visible = true;
            ScreenSelected = 0;
            Send_btn.Visible = true;
            Characters_rb.Visible = true;
            ConnectedList_rb.Visible = true;
            createGame_rb.Visible = true;
            Ranking_rb.Visible = true;
            showGames_rb.Visible = true;
            TimePlayed_rb.Visible = true;
            queries_tb.Visible = true;
            NewParty_btn.Visible = true;

            //Desactiva els objectes del menú anterior
            PlayersSelected_dg.Visible = false;
            NewPartyName_tb.Visible = false;
            NewPartyName_lbl.Visible = false;
            NewPartyBack_btn.Visible = false;
            CreateParty_btn.Visible = false;

            //Desactiva els objectes de la selecció del personatge
            character_pb.Visible = false;
            PartyName_lbl.Visible = false;
            RightChar_btn.Visible = false;
            LeftChar_btn.Visible = false;
            SelectChar_btn.Visible = false;
            StartGame_btn.Visible = false;
            CancelGame_btn.Visible = false;
            QuitGame_btn.Visible = false;
            ChatGame_rtb.Visible = false;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //Menú de la selecció dels personatges
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //Escollir el personatge
        public void LeftChar_btn_Click(object sender, EventArgs e)
        {
            if (charSelectedPos > 0)
                charSelectedPos--;
            else
                charSelectedPos = 3;

            character_pb.ImageLocation = "../../../Pictures/Characters/"+ characterSelected[charSelectedPos] + " stop.gif";
            character_pb.Load();
            character_pb.Refresh();
        }

        public void RightChar_btn_Click(object sender, EventArgs e)
        {
            if (charSelectedPos < 3)
                charSelectedPos++;
            else
                charSelectedPos = 0;

            character_pb.ImageLocation = "../../../Pictures/Characters/" + characterSelected[charSelectedPos] + " stop.gif";
            character_pb.Load();
            character_pb.Refresh();
        }

        //Començar/Cancel·lar la partida
        public void StartGame_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                MessageBox.Show("No s'ha seleccionat cap partida per començar");
            else
                serverHandler.RequestStartGame(gameName);
        }
        public void CancelGame_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                MessageBox.Show("No s'ha seleccionat cap partida per començar");
            else
                serverHandler.RequestCancelGame(gameName);
        }

        //QuitGame
        public void QuitGame_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                MessageBox.Show("No s'ha seleccionat cap partida per començar");
            else
            {
                serverHandler.RequestRejectInvitation(gameName);
                //Fa apareixer els objectes del menú principal
                QueryGrid.Visible = true;
                ScreenSelected = 0;
                Send_btn.Visible = true;
                Characters_rb.Visible = true;
                ConnectedList_rb.Visible = true;
                createGame_rb.Visible = true;
                Ranking_rb.Visible = true;
                showGames_rb.Visible = true;
                TimePlayed_rb.Visible = true;
                queries_tb.Visible = true;
                NewParty_btn.Visible = true;

                //Desactiva els objectes del menú anterior
                PlayersSelected_dg.Visible = false;
                NewPartyName_tb.Visible = false;
                NewPartyName_lbl.Visible = false;
                NewPartyBack_btn.Visible = false;
                CreateParty_btn.Visible = false;

                //Desactiva els objectes de la selecció del personatge
                character_pb.Visible = false;
                PartyName_lbl.Visible = false;
                RightChar_btn.Visible = false;
                LeftChar_btn.Visible = false;
                SelectChar_btn.Visible = false;
                StartGame_btn.Visible = false;
                CancelGame_btn.Visible = false;
                QuitGame_btn.Visible = false;
                ChatGame_rtb.Visible = false;
            }
        }

        //Enviar el personatge escollit
        public void SelectChar_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                MessageBox.Show("No s'ha seleccionat cap partida per començar");
            else
                serverHandler.RequestSelectCharacter(gameName, characterSelected[charSelectedPos]);
        }
    }
}
