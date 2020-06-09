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
using System.IO;
using BaboGame_test_2;

namespace BaboGameClient
{
    
    public partial class QueriesForm : Form
    {
        ServerHandler serverHandler;
        MusicPlayer musicPlayer;
        Thread loadingThread;

        Game1.LocalGameState TrainingState = new Game1.LocalGameState();
        // necessitem una ref. al Notification Worker per modificar el camp 
        // DataGridUpdateRequested segons el data grid
        NotificationWorker notificationWorker;

        PictureBox NotificationIcon;

        //Elements del menú d'entrenament
        Button Return_btn;
        Button Train_btn;
        Button LeftOpponentChar_btn;
        Button RightOpponentChar_btn;
        PictureBox Opponentcharacter_pb;
        Button OpponentSelectChar_btn;
        Button PlayerSelectChar_btn;
        Label OpponentCharName_lbl;
        Label OpponentCharDescription_lbl;
        Panel OpponentListPanel;
        PictureBox DifficultySelected_pb;
        Button LeftDifficulty_btn;
        Button RightDifficulty_btn;

        //Elements del menú dels personatges seleccionats
        DataGridView PlayersSelected_dg;
        TextBox NewPartyName_tb;
        Button CreateParty_btn;
        Button NewPartyBack_btn;
        Label NewPartyName_lbl;

        //Elements del menú de la partida
        Label PartyName_lbl;
        PictureBox character_pb;
        Button LeftChar_btn;
        Button RightChar_btn;
        Button CancelGame_btn;
        Button SelectChar_btn;
        Button StartGame_btn;
        Button QuitGame_btn;
        RichTextBox ChatGame_rtb;
        Panel ChattingPanel;
        Button Chatting_btn;
        TextBox Chatting_tb;
        Panel StickersPanel;
        Button Stickers_btn;
        Label CharName_lbl;
        Label CharDescription_lbl;

        //Elements del menú principal
        Button NewParty_btn;
        Button Training_btn;

        //Elements del menú de queries
        DataGridView MultipleStringSelected_dgv;
        Button AddToMultipleStringSelected_btn;

        string[] characterSelected;
        string[] characterDescription;
        int charSelectedPos;
        int OpponentcharSelectedPos;

        //Variable que diferenciar a quin menú estàs situat
        int ScreenSelected;
        string gameName;
        ToolStripItem notificationSelection;
        ToolStripItem stickerSelector;
        char[] Difficulty = {'N','E','M','D','I' }; //Canvia la dificultat del CPU (E)asy, (M)edium, (D)ifficult, (I)nsane, (N)one
        string[] DifficultyName = { "None.png", "Easy.png", "Medium.png", "Difficult.png", "Insane.png" }; //Canvia la dificultat del CPU (E)asy, (M)edium, (D)ifficult, (I)nsane, (N)one
        int DifficultyPos;

        public QueriesForm(ServerHandler serverHandler, NotificationWorker notificationWorker)
        {


            this.NotificationIcon = new PictureBox();

            //Elements del menú d'entrenament
            Train_btn = new Button();
            Return_btn = new Button();
            LeftOpponentChar_btn = new Button();
            RightOpponentChar_btn = new Button();
            Opponentcharacter_pb = new PictureBox();
            PlayerSelectChar_btn = new Button();
            OpponentSelectChar_btn = new Button();
            OpponentCharName_lbl = new Label(); 
            OpponentCharDescription_lbl = new Label();
            OpponentListPanel = new Panel();
            LeftDifficulty_btn = new Button();
            RightDifficulty_btn = new Button();
            DifficultySelected_pb = new PictureBox();


            //Elements del menú dels personatges seleccionats
            PlayersSelected_dg = new DataGridView();
            NewPartyName_tb = new TextBox();
            CreateParty_btn = new Button();
            NewPartyBack_btn = new Button();
            NewPartyName_lbl = new Label();

            //Elements del menú de la partida
            PartyName_lbl = new Label();
            character_pb = new PictureBox();
            LeftChar_btn = new Button();
            RightChar_btn = new Button();
            CancelGame_btn = new Button();
            SelectChar_btn = new Button();
            StartGame_btn = new Button();
            QuitGame_btn = new Button();
            ChatGame_rtb = new RichTextBox();
            ChattingPanel = new Panel();
            Chatting_btn = new Button();
            Chatting_tb = new TextBox();
            StickersPanel = new Panel();
            Stickers_btn = new Button();
            CharName_lbl = new Label();
            CharDescription_lbl = new Label();

            //Element del menú principal
            NewParty_btn = new Button();
            Training_btn = new Button();

            //Elements del menú de Queries
            MultipleStringSelected_dgv = new DataGridView();
            AddToMultipleStringSelected_btn = new Button();

            characterSelected = new string[] { "Babo", "Limax", "Kaler", "Swalot" };

            characterDescription = new string[] 
            {
            "Una vegada va voler fundar una societat anònima anomenada Babo S.A. ¿Qué puede malir sal?",//Babo
            "De l'espècie Limax Maximus. Li agrada fer jocs de paraules. És un llimac una mica salat.",//Limax
            "És un llimac groc amb un saler controlat per terminal, es podrà descarregar el driver per aptitude?",//Kaler
            "Swalot, pokémon tipus verí. Com no té dents, es traga tot d'un sol cop amb la seva enorme boca. " //Swalot
            };
            charSelectedPos = 0;

            //Variable que diferencia a quin menú estàs situat
            ScreenSelected = 0;


            InitializeComponent();
            this.serverHandler = serverHandler;
            this.notificationWorker = notificationWorker;
            NotificationIcon.ImageLocation = "../../../Pictures/Layouts/Babo down hit.png";
            NotificationIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            NotificationIcon.Load();
            NotificationIcon.Refresh();

            
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //Creació dels objectes del menú de la tria de la llista de connectats (ScreenSelected = 2)
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
            //Creació del menú de selecció de personatges (ScreenSelected = 3)
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            //Label del nom de partida seleccionat
            PartyName_lbl.Location = new Point(25, 35);
            PartyName_lbl.Text = "Partida: Game1";
            PartyName_lbl.Visible = false;
            this.Controls.Add(PartyName_lbl);

            //Buttons per canviar el personatge
            LeftChar_btn.Location = new Point(25,100);
            //LeftChar_btn.Text = "Left";
            LeftChar_btn.Visible = false;
            LeftChar_btn.Size = new Size(64, 64);
            LeftChar_btn.Click += new EventHandler(this.LeftChar_btn_Click);
            LeftChar_btn.Image = System.Drawing.Image.FromFile("../../../Pictures/Layouts/left.png");
            this.Controls.Add(LeftChar_btn);

            RightChar_btn.Location = new Point(240, 100);
            //RightChar_btn.Text = "Right";
            RightChar_btn.Visible = false;
            RightChar_btn.Size = new Size(64, 64);
            RightChar_btn.Click += new EventHandler(this.RightChar_btn_Click);
            RightChar_btn.Image = Image.FromFile("../../../Pictures/Layouts/right.png");
            this.Controls.Add(RightChar_btn);

            //PictureBox de la imatge del personatge
            character_pb.Location = new Point(90, 60);
            character_pb.Size = new Size(140, 140);
            character_pb.ImageLocation = "../../../Pictures/Characters/Babo stop.gif";
            character_pb.Visible = false;
            character_pb.SizeMode = PictureBoxSizeMode.Zoom;
            character_pb.Refresh();
            this.Controls.Add(character_pb);

            //Label del nom del personatge
            CharName_lbl.Location = new Point(25, 200);
            CharName_lbl.Text = "Nom: " + characterSelected[charSelectedPos];
            CharName_lbl.Visible = false;
            this.Controls.Add(CharName_lbl);

            //Label de la descripció del personatge
            CharDescription_lbl.Location = new Point(25, 230);
            CharDescription_lbl.Text = characterDescription[charSelectedPos];
            CharDescription_lbl.Size = new Size(150, 60);
            CharDescription_lbl.TextAlign = ContentAlignment.TopCenter;
            CharDescription_lbl.Visible = false;
            this.Controls.Add(CharDescription_lbl);

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
            ChatGame_rtb.Location = new Point(325,25);
            ChatGame_rtb.ReadOnly = true;
            this.Controls.Add(ChatGame_rtb);
            ChatGame_rtb.Text = "";

            //ChatPanel
            ChattingPanel.Size = new Size(250,270); //Mides del panell
            ChattingPanel.Location = new Point(625,25); //posició del panell
            ChattingPanel.Visible = false;
            this.Controls.Add(ChattingPanel);
            ChattingPanel.BackColor = Color.WhiteSmoke;
            ChattingPanel.AutoScroll = false;
            ChattingPanel.VerticalScroll.Visible = true;
            ChattingPanel.VerticalScroll.Enabled = true;
            ChattingPanel.AutoScroll = true;
            ChattingPanel.Refresh();

            //StickersPanel
            StickersPanel.Size = new Size(250, 120); //Mides del panell
            StickersPanel.Location = new Point(625, 175); //posició del panell
            StickersPanel.Visible = false;
            this.Controls.Add(StickersPanel);
            StickersPanel.BringToFront();
            StickersPanel.BackColor = Color.LightYellow;
            StickersPanel.AutoScroll = false;
            StickersPanel.HorizontalScroll.Enabled = true;
            StickersPanel.HorizontalScroll.Visible = true;
            StickersPanel.AutoScroll = true;
            StickersPanel.Refresh();

            //Chatting_Btn
            Chatting_btn.Location = new Point(625, 300);
            Chatting_btn.Text = "Xateja";
            Chatting_btn.Size = new Size(80, 25);
            Chatting_btn.Visible = false;
            this.Controls.Add(Chatting_btn);
            Chatting_btn.Click += new EventHandler(this.Chatting_btn_Click);

            //Stickers_Btn
            Stickers_btn.Location = new Point(625, 325);
            Stickers_btn.Text = "Adhesius";
            Stickers_btn.Size = new Size(80, 25);
            Stickers_btn.Visible = false;
            this.Controls.Add(Stickers_btn);
            Stickers_btn.Click += new EventHandler(this.Stickers_btn_Click);

            //Chattting_tb
            Chatting_tb.Location = new Point(725, 300);
            Chatting_tb.Visible = false;
            this.Controls.Add(Chatting_tb);

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //Creació del menú principal (ScreenSelected = 0)
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //NewParty_Btn
            NewParty_btn.Location = new Point(225, 200);
            NewParty_btn.Text = "Nova Partida";
            NewParty_btn.Size = new Size(150, 60);
            NewParty_btn.Visible = false;
            this.Controls.Add(NewParty_btn);
            NewParty_btn.Click += new EventHandler(this.NewParty_btn_Click);

            //Training_Btn
            Training_btn.Location = new Point(225, 100);
            Training_btn.Text = "Entrenament";
            Training_btn.Size = new Size(150, 60);
            Training_btn.Visible = false;
            this.Controls.Add(Training_btn);
            Training_btn.Click += new EventHandler(this.Training_btn_Click);

            ScreenSelected = 0;
            UpdateScreen();

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //Creació del menú entrenament (ScreenSelected = -1)
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //Butó per tornar enrere 
            Return_btn.Location = new Point(25, 30);
            Return_btn.Text = "Enrere";
            Return_btn.Size = new Size(80, 30);
            Return_btn.Visible = false;
            this.Controls.Add(Return_btn);
            Return_btn.Click += new EventHandler(this.Return_btn_Click);

            //Butó per començar l'entrenament
            Train_btn.Location = new Point(224, 300);
            Train_btn.Text = "Entrena";
            Train_btn.Size = new Size(80, 30);
            Train_btn.Visible = false;
            this.Controls.Add(Train_btn);
            Train_btn.Click += new EventHandler(this.Train_btn_Click);

            //Butó per veure el personatge esquerre de l'oponent
            LeftOpponentChar_btn.Location = new Point(312, 100);
            //LeftChar_btn.Text = "Left";
            LeftOpponentChar_btn.Visible = false;
            LeftOpponentChar_btn.Size = new Size(64, 64);
            LeftOpponentChar_btn.Click += new EventHandler(this.LeftOpponentChar_btn_Click);
            LeftOpponentChar_btn.Image = System.Drawing.Image.FromFile("../../../Pictures/Layouts/left.png");
            this.Controls.Add(LeftOpponentChar_btn);

            //Butó per veure el personatge dret de l'oponent
            RightOpponentChar_btn.Location = new Point(527, 100);
            //LeftChar_btn.Text = "Left";
            RightOpponentChar_btn.Visible = false;
            RightOpponentChar_btn.Size = new Size(64, 64);
            RightOpponentChar_btn.Click += new EventHandler(this.RightOpponentChar_btn_Click);
            RightOpponentChar_btn.Image = System.Drawing.Image.FromFile("../../../Pictures/Layouts/right.png");
            this.Controls.Add(RightOpponentChar_btn);

            //Picture Box de l'oponent
            Opponentcharacter_pb.Location = new Point(377, 60);
            Opponentcharacter_pb.Size = new Size(140, 140);
            Opponentcharacter_pb.ImageLocation = "../../../Pictures/Characters/Babo stop.gif";
            Opponentcharacter_pb.Visible = false;
            Opponentcharacter_pb.SizeMode = PictureBoxSizeMode.Zoom;
            Opponentcharacter_pb.Refresh();
            this.Controls.Add(Opponentcharacter_pb);

            //Butó per seleccionar el personatge de l'oponent
            OpponentSelectChar_btn.Location = new Point(312, 300);
            OpponentSelectChar_btn.Text = "Selecciona Oponent";
            OpponentSelectChar_btn.Size = new Size(80, 60);
            OpponentSelectChar_btn.Visible = false;
            this.Controls.Add(OpponentSelectChar_btn);
            OpponentSelectChar_btn.Click += new EventHandler(this.OpponentSelectChar_btn_Click);

            //Butó per seleccionar el personatge del jugador
            PlayerSelectChar_btn.Location = new Point(25, 300);
            PlayerSelectChar_btn.Text = "Selecciona Personatge";
            PlayerSelectChar_btn.Size = new Size(80, 60);
            PlayerSelectChar_btn.Visible = false;
            this.Controls.Add(PlayerSelectChar_btn);
            PlayerSelectChar_btn.Click += new EventHandler(this.PlayerSelectChar_btn_Click);

            //Nom del personatge de l'oponent
            OpponentCharName_lbl.Location = new Point(312, 200);
            OpponentCharName_lbl.Text = "Nom: " + characterSelected[OpponentcharSelectedPos];
            OpponentCharName_lbl.Visible = false;
            this.Controls.Add(OpponentCharName_lbl);

            //Descripció del personatge de l'oponent
            OpponentCharDescription_lbl.Location = new Point(312, 230);
            OpponentCharDescription_lbl.Text = characterDescription[OpponentcharSelectedPos];
            OpponentCharDescription_lbl.Size = new Size(150, 60);
            OpponentCharDescription_lbl.TextAlign = ContentAlignment.TopCenter;
            OpponentCharDescription_lbl.Visible = false;
            this.Controls.Add(OpponentCharDescription_lbl);

            //Llista d'oponents
            OpponentListPanel.Size = new Size(250, 240); //Mides del panell
            OpponentListPanel.Location = new Point(625, 25); //posició del panell
            OpponentListPanel.Visible = false;
            this.Controls.Add(OpponentListPanel);
            OpponentListPanel.BackColor = Color.WhiteSmoke;
            OpponentListPanel.AutoScroll = false;
            OpponentListPanel.VerticalScroll.Visible = true;
            OpponentListPanel.VerticalScroll.Enabled = true;
            OpponentListPanel.AutoScroll = true;
            OpponentListPanel.Refresh();

            //Butó per canviar la difficultat al de l'esquerra
            LeftDifficulty_btn.Location = new Point(625, 290);
            //LeftChar_btn.Text = "Left";
            LeftDifficulty_btn.Visible = false;
            LeftDifficulty_btn.Size = new Size(64, 64);
            LeftDifficulty_btn.Click += new EventHandler(this.LeftDifficulty_btn_Click);
            LeftDifficulty_btn.Image = System.Drawing.Image.FromFile("../../../Pictures/Layouts/left.png");
            this.Controls.Add(LeftDifficulty_btn);

            //Butó per canviar la difficultat a la dreta de l'oponent
            RightDifficulty_btn.Location = new Point(811, 290);
            //LeftChar_btn.Text = "Left";
            RightDifficulty_btn.Visible = false;
            RightDifficulty_btn.Size = new Size(64, 64);
            RightDifficulty_btn.Click += new EventHandler(this.RightDifficulty_btn_Click);
            RightDifficulty_btn.Image = System.Drawing.Image.FromFile("../../../Pictures/Layouts/right.png");
            this.Controls.Add(RightDifficulty_btn);

            //Picture Box de la difficultat
            DifficultySelected_pb.Location = new Point(705, 270);
            DifficultySelected_pb.Size = new Size(90, 90);
            DifficultySelected_pb.ImageLocation = "../../../Pictures/Difficulty/None.png";
            DifficultySelected_pb.Visible = false;
            DifficultySelected_pb.SizeMode = PictureBoxSizeMode.Zoom;
            DifficultySelected_pb.Refresh();
            this.Controls.Add(DifficultySelected_pb);
            DifficultyPos = 0;

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //Creació del menú de consultes
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //LLista de múltiples respostes
            MultipleStringSelected_dgv.Visible = false;
            MultipleStringSelected_dgv.Size = new Size(250, 240);
            MultipleStringSelected_dgv.Location = new Point(625, 25);
            MultipleStringSelected_dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            MultipleStringSelected_dgv.Name = "MultipleStringSelected_dgv";
            MultipleStringSelected_dgv.TabIndex = 0;
            MultipleStringSelected_dgv.RowTemplate.Height = 20;
            MultipleStringSelected_dgv.Enabled = true;
            this.Controls.Add(MultipleStringSelected_dgv);
            MultipleStringSelected_dgv.CellClick += new DataGridViewCellEventHandler(this.MultipleStringSelected_dgv_CellClick);

            //Butó per afegir elements a la llista
            AddToMultipleStringSelected_btn.Location = new Point(625, 270);
            AddToMultipleStringSelected_btn.Size = new Size(80, 30);
            AddToMultipleStringSelected_btn.Text = "Afegeix";
            AddToMultipleStringSelected_btn.Visible = false;
            AddToMultipleStringSelected_btn.Click += new EventHandler(this.AddToMultipleSelected_btn_Click);
            this.Controls.Add(AddToMultipleStringSelected_btn);


            TrainingState.Player_ID = new int[] {1,2,3,4,5,6,7,8};
            

        }

        public void UpdateScreen()
        {
            bool MainScreen = false;
            bool TrainningScreen = false;
            bool QueriesScreen = false;
            bool CreatePartyScreen = false;
            bool SelectCharacterOnlineScreen = false;

            //Canvia les variables visibles dels objectes segons el número de pantalla
            if (ScreenSelected == -1)//Menú d'entrenament
                TrainningScreen = true;
            else if (ScreenSelected == 1) //Menú de les Queries
                QueriesScreen = true;
            else if (ScreenSelected == 2) //Menú de crear la partida
                CreatePartyScreen = true;
            else if (ScreenSelected == 3) //Menú de triar el personatge
                SelectCharacterOnlineScreen = true;
            else //Menú principal
                MainScreen = true;

            //~~~~~~~~~~~~~~~~~~~~~~~~~~

            //MainScreen
            NewParty_btn.Visible = MainScreen;
            Training_btn.Visible = MainScreen;
            this.BackColor = Color.LightGreen;

            //Trainning screen            
            Train_btn.Visible = TrainningScreen;
            Return_btn.Visible = TrainningScreen;
            LeftOpponentChar_btn.Visible = TrainningScreen;
            RightOpponentChar_btn.Visible= TrainningScreen;
            Opponentcharacter_pb.Visible = TrainningScreen;
            OpponentSelectChar_btn.Visible = TrainningScreen;
            PlayerSelectChar_btn.Visible = TrainningScreen;
            OpponentCharName_lbl.Visible = TrainningScreen;
            OpponentCharDescription_lbl.Visible = TrainningScreen;
            OpponentListPanel.Visible = TrainningScreen;
            RightDifficulty_btn.Visible = TrainningScreen;
            LeftDifficulty_btn.Visible = TrainningScreen;
            DifficultySelected_pb.Visible = TrainningScreen;


            //QueriesScreen
            QueryGrid.Visible = QueriesScreen||CreatePartyScreen;
            Send_btn.Visible = QueriesScreen;
            Characters_rb.Visible = QueriesScreen;
            ConnectedList_rb.Visible = QueriesScreen;
            //createGame_rb.Visible = QueriesScreen;
            Ranking_rb.Visible = QueriesScreen;
            //showGames_rb.Visible = QueriesScreen;
            TimePlayed_rb.Visible = QueriesScreen;
            queries_tb.Visible = QueriesScreen;
            Opponents_rb.Visible = QueriesScreen;
            AddToMultipleStringSelected_btn.Visible = QueriesScreen;
            MultipleStringSelected_dgv.Visible = QueriesScreen;
            gameResultsWithPlayers_rb.Visible = QueriesScreen;
            TimeInterval1_lbl.Visible = QueriesScreen;
            TimeInterval2_lbl.Visible = QueriesScreen;
            dateTimeEnd.Visible = QueriesScreen;
            dateTimeStart_dt.Visible = QueriesScreen;
            GameListInterval_rb.Visible = QueriesScreen;
            this.BackColor = Color.LightGreen;

            //CreatePartyScreen
            PlayersSelected_dg.Visible = CreatePartyScreen;
            NewPartyName_tb.Visible = CreatePartyScreen;
            NewPartyName_lbl.Visible = CreatePartyScreen;
            NewPartyBack_btn.Visible = CreatePartyScreen;
            CreateParty_btn.Visible = CreatePartyScreen;
            this.BackColor = Color.LightGreen;

            //SelectCharacterOnlineScreen
            character_pb.Visible = SelectCharacterOnlineScreen || TrainningScreen;
            PartyName_lbl.Visible = SelectCharacterOnlineScreen;
            RightChar_btn.Visible = SelectCharacterOnlineScreen||TrainningScreen;
            LeftChar_btn.Visible = SelectCharacterOnlineScreen ||TrainningScreen;
            SelectChar_btn.Visible = SelectCharacterOnlineScreen;
            StartGame_btn.Visible = SelectCharacterOnlineScreen;
            CancelGame_btn.Visible = SelectCharacterOnlineScreen;
            QuitGame_btn.Visible = SelectCharacterOnlineScreen;
            ChatGame_rtb.Visible = SelectCharacterOnlineScreen;
            PartyName_lbl.Text = "Partida: " + gameName;
            Chatting_btn.Visible = SelectCharacterOnlineScreen;
            Chatting_tb.Visible = SelectCharacterOnlineScreen;
            ChattingPanel.Visible = SelectCharacterOnlineScreen;
            StickersPanel.Visible = false;
            Stickers_btn.Visible = SelectCharacterOnlineScreen;
            CharName_lbl.Visible = SelectCharacterOnlineScreen || TrainningScreen;
            CharDescription_lbl.Visible = SelectCharacterOnlineScreen || TrainningScreen;
            this.BackColor = Color.LightGreen;

            if ((SelectCharacterOnlineScreen)||(TrainningScreen))
                this.Width = 900;            
            else
                this.Width = 616;
            
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

        //Actualitzem llista de jugadors en que hem jugat
        public void UpdateOpponent(string[][] opponents)
        {
            QueryGrid.Rows.Clear();
            QueryGrid.Columns.Clear();
            QueryGrid.Columns.Add("username", "Usuari");
            QueryGrid.Columns.Add("ID", "ID");

            for (int i = 0; i < opponents.GetLength(0); i++)// array rows
            {
                string[] row = new string[opponents[i].GetLength(0) - 1];

                for (int j = 0; j < opponents[i].GetLength(0) - 1; j++)
                {
                    if ((opponents[i][2].ToString() == "0") && (j == 0))
                        row[j] = opponents[i][j] + "(Inhabilitat)";
                    else
                        row[j] = opponents[i][j];
                }

                QueryGrid.Rows.Add(row);
            }
            QueryGrid.Refresh();

        }

        //Actualitza llista de partides amb altres jugadors
        public void UpdateGameResultsWithOthers(string[][] games)
        {
            QueryGrid.Rows.Clear();
            QueryGrid.Columns.Clear();
            QueryGrid.Columns.Add("gameName", "Nom de la partida");
            QueryGrid.Columns.Add("ID", "ID");
            QueryGrid.Columns.Add("winner", "Guanyador");

            for (int i = 0; i < games.GetLength(0); i++)// array rows
            {
                string[] row = new string[games[i].GetLength(0)];

                for (int j = 0; j < games[i].GetLength(0); j++)
                {
                        row[j] = games[i][j];
                }

                QueryGrid.Rows.Add(row);
            }
            QueryGrid.Refresh();

        }

        //Actualitza llista de partides amb un interval
        public void UpdateGameInterval(string[][] games)
        {
            QueryGrid.Rows.Clear();
            QueryGrid.Columns.Clear();
            QueryGrid.Columns.Add("gameName", "Nom de la partida");
            QueryGrid.Columns.Add("ID", "ID");
            QueryGrid.Columns.Add("startdate", "Inici de la partida");

            for (int i = 0; i < games.GetLength(0); i++)// array rows
            {
                string[] row = new string[games[i].GetLength(0)];

                for (int j = 0; j < games[i].GetLength(0); j++)
                {
                    row[j] = games[i][j];
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

                ScreenSelected = 3;
                UpdateScreen();
                StartGame_btn.Visible = true;
                CancelGame_btn.Visible = true;
                QuitGame_btn.Visible = false;
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

            ScreenSelected = 3;
            UpdateScreen();

            StartGame_btn.Visible = false;
            CancelGame_btn.Visible = false;
            QuitGame_btn.Visible = true;

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

            serverHandler.SwitchToRealtimeMode();
            MessageBox.Show("Comença la partida");
            musicPlayer.Stop();
            Thread.Sleep(250);
            musicPlayer.Play("Sounds/Music/InGame1.wav");
            //BaboGame_test_2.Game1 BaboGame = new BaboGame_test_2.Game1();
            //BaboGame.Run();

            //Engegar el thread de carregant
            Game1.Loading LoadVariable = new Game1.Loading();
            LoadVariable.Loaded = false;

            ThreadStart LoadingThreadStart = delegate { this.LoadingThread(LoadVariable); };
            loadingThread = new Thread(LoadingThreadStart);
            loadingThread.Start();

            //Engegar el joc
            using (var game = new Game1(this.serverHandler,LoadVariable))
            game.Run();

            //Canviem a la pantalla principal
            ScreenSelected = 0;
            UpdateScreen();
        }

        //Error en no haver escollit tothom personatge
        public void NotAllSelectedPopup()
        {
            MessageBox.Show("Tots els jugadors no han escollit personatge");
        }

        //Popup per cancel·lar el joc
        public void CancelGamePopup(string gameName, string creatorName)
        {
            if(ScreenSelected == 3)
            {
                MessageBox.Show("S'ha cancel·lat la partida");
                ScreenSelected = 0;
                UpdateScreen();
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
            ScreenSelected = 0;
            UpdateScreen();
        }

        int panelcursor = 0;

        public void SentMessageChat(string username, string message)
        {
            if (message.Length > 0)
            {
                bool Image = true;
                string[] messageReceived = message.Split('}');
                if (messageReceived.Length < 2)
                    Image = false;
                if (Image)
                {
                    PictureBox sticker = new PictureBox();
                    sticker.Size = new Size(120, 120);
                    sticker.SizeMode = PictureBoxSizeMode.Zoom;
                    string ImageName = messageReceived[0].Split('{')[1];
                    try
                    {
                        sticker.ImageLocation = "../../../Pictures/Stickers/" + ImageName + ".png";
                    }
                    catch
                    {
                        Image = false;
                    }

                    if (Image)
                    {
                        Label text = new Label();
                        text.Text = username + ":";
                        text.BackColor = Color.PaleGreen;
                        text.MaximumSize = new Size(ChattingPanel.Width, ChattingPanel.Height);
                        text.AutoSize = true;
                        text.Location = new Point(0, panelcursor - ChattingPanel.VerticalScroll.Value);
                        panelcursor += text.Height + 10;
                        ChattingPanel.Controls.Add(text);
                        sticker.Location = new Point(0, panelcursor - ChattingPanel.VerticalScroll.Value);
                        Chatting_tb.Text = "";
                        panelcursor += sticker.Height + 10;
                        ChattingPanel.Controls.Add(sticker);
                        ChattingPanel.Refresh();
                    }
                    else
                    {
                        
                        Label text = new Label();
                        text.Text = username + ": " + message;
                        text.BackColor = Color.PaleGreen;
                        text.MaximumSize = new Size(ChattingPanel.Width, ChattingPanel.Height);
                        text.AutoSize = true;
                        text.Location = new Point(0, panelcursor - ChattingPanel.VerticalScroll.Value);
                        Chatting_tb.Text = "";
                        panelcursor += sticker.Height + 10;
                        ChattingPanel.Controls.Add(text);
                        ChattingPanel.Refresh();
                        
                    }
                }
                else
                {
                    Label text = new Label();
                    text.Text = username + ": " + message;
                    text.BackColor = Color.PaleGreen;
                    text.MaximumSize = new Size(ChattingPanel.Width, ChattingPanel.Height);
                    text.AutoSize = true;
                    text.Location = new Point(0, panelcursor - ChattingPanel.VerticalScroll.Value);
                    Chatting_tb.Text = "";
                    panelcursor += text.Height + 10;
                    ChattingPanel.Controls.Add(text);

                }

            }
            
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
            else if (Opponents_rb.Checked)
            {
                serverHandler.RequestOpponentPlayed();
            }
            else if (gameResultsWithPlayers_rb.Checked)
            {
                string[] players = new string[8];
                for (int i = 0; i < MultipleStringSelected_dgv.RowCount - 1; i++)
                {
                    players[i] = MultipleStringSelected_dgv[0, i].Value.ToString();
                }

                serverHandler.RequestgamePlayedwithPlayers(MultipleStringSelected_dgv.RowCount - 1, players);
            }
            else if (GameListInterval_rb.Checked)
            {
                string startInterval = dateTimeStart_dt.Value.ToString("u");
                string endInterval = dateTimeEnd.Value.ToString("u");

                serverHandler.RequestgameInterval(startInterval, endInterval);
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

        private void Notificacions_btn_Click(object sender, EventArgs e)
        {
            Notificacions_btn.BackColor = Color.LightGray;
        }

        private void Chatting_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Chatting_tb.Text))
            {
                MessageBox.Show("Els camps estan buits!");
                return;
            }
            else
            {
                serverHandler.RequestChatMessage(Chatting_tb.Text);
            }

        }

        private void Stickers_btn_Click(object sender, EventArgs e)
        {
            if(StickersPanel.Visible==true)
            {
                StickersPanel.Visible = false;
            }
            else
            {
                StickersPanel.Visible = true;
                DirectoryInfo stickerFolder = new DirectoryInfo("../../../Pictures/Stickers");
                FileInfo[] stickers = stickerFolder.GetFiles();
                List<string> stickersNames = new List<string>();
                int ImageCursor = 0;
                StickersPanel.HorizontalScroll.Value = 0;
                StickersPanel.Controls.Clear();
                foreach(FileInfo sticker in stickers)
                {
                    stickersNames.Add(sticker.Name);
                }
                foreach(string stickerName in stickersNames)
                {
                    //ChatGame_rtb.Text += stickerName + "\n";
                    string stickerSelected = stickerName.Split('.')[0];
                    PictureBox StickerImage = new PictureBox();
                    StickerImage.Size = new Size(80, 80);
                    StickerImage.Location = new Point(ImageCursor + 10, 10);
                    ImageCursor += 120;
                    StickerImage.ImageLocation = "../../../Pictures/Stickers/" + stickerName;
                    StickerImage.Load();
                    StickerImage.SizeMode = PictureBoxSizeMode.Zoom;
                    StickerImage.Refresh();
                    StickersPanel.Controls.Add(StickerImage);
                    StickerImage.Click += delegate { StickerSelected_Click(sender, e, stickerSelected); };
                    
                }
            }
        }

        private void StickerSelected_Click (object sender, EventArgs e, string StickerName)
        {
            Chatting_tb.Text = "{" + StickerName + "}";
            serverHandler.RequestChatMessage(Chatting_tb.Text);
        }

        private void gameResultsWithPlayers_rb_CheckedChanged(object sender, EventArgs e)
        {
            if (gameResultsWithPlayers_rb.Checked)
            {
                MultipleStringSelected_dgv.Rows.Clear();
                MultipleStringSelected_dgv.Columns.Clear();
                MultipleStringSelected_dgv.Columns.Add("players", "Jugadors");
                this.Width = 900;
            }
            else
            {
                this.Width = 616;
            }
        }

        private void AddToMultipleSelected_btn_Click(object sender,EventArgs e)
        {
            if((!string.IsNullOrWhiteSpace(queries_tb.Text))&&(MultipleStringSelected_dgv.Rows.Count < 8))
                MultipleStringSelected_dgv.Rows.Add(queries_tb.Text);
        }

        private void MultipleStringSelected_dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                MultipleStringSelected_dgv.Rows.RemoveAt(e.RowIndex);
            }
            catch { }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //Menú de selecció de jugadors
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //Botó que obre el menú
        //Fa invisibles els objectes innecesàris i fa aparèixer els objectes necessàris d'aquell menú
        private void NewParty_btn_Click(object sender, EventArgs e)
        {
 
            notificationWorker.DataGridUpdateRequested = 6;
            serverHandler.RequestConnected();
            
            PlayersSelected_dg.Rows.Clear();
            PlayersSelected_dg.Columns.Clear();
            PlayersSelected_dg.Columns.Add("username", "Usuari");
            PlayersSelected_dg.Columns.Add("ID", "ID");
            PlayersSelected_dg.Refresh();

            ScreenSelected = 2;
            UpdateScreen();
        }

        private void Training_btn_Click(object sender, EventArgs e)
        {
            ScreenSelected = -1;
            UpdateScreen();
            
            TrainingState.OpponentCharacter_Selected.Clear();
            OpponentListPanel.Controls.Clear();
            TrainingState.Opponentnum_players = 0;
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
            if((notificationWorker.DataGridUpdateRequested == 6)&&(ScreenSelected == 2))
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
            ScreenSelected = 0;
            UpdateScreen();
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

            character_pb.Image.Dispose();
            character_pb.ImageLocation = "../../../Pictures/Characters/"+ characterSelected[charSelectedPos] + " stop.gif";
            character_pb.Load();
            character_pb.Refresh();
            CharName_lbl.Text = "Nom: " + characterSelected[charSelectedPos];
            CharDescription_lbl.Text = characterDescription[charSelectedPos];           
        }

        public void RightChar_btn_Click(object sender, EventArgs e)
        {
            if (charSelectedPos < 3)
                charSelectedPos++;
            else
                charSelectedPos = 0;
                
            character_pb.Image.Dispose();
            character_pb.ImageLocation = "../../../Pictures/Characters/" + characterSelected[charSelectedPos] + " stop.gif";
            character_pb.Load();
            character_pb.Refresh();
            CharName_lbl.Text = "Nom: " + characterSelected[charSelectedPos];
            CharDescription_lbl.Text = characterDescription[charSelectedPos];
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
                ScreenSelected = 0;
                UpdateScreen();
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

        private void QueriesForm_Load(object sender, EventArgs e)
        {
           musicPlayer = new MusicPlayer();
           musicPlayer.Play("Sounds/Music/StartMenu.wav");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MainMenu_btn_Click(object sender, EventArgs e)
        {
            if (ScreenSelected == 1)
            {
                ScreenSelected = 0;
                UpdateScreen();
            }
        }

        private void QueriesMenu_btn_Click(object sender, EventArgs e)
        {
            if (ScreenSelected == 0)
            {
                ScreenSelected = 1;
                UpdateScreen();
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //Menú d'entrenament
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //Entrena
        public void Train_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TrainingState.PlayerCharacter_Selected))
                MessageBox.Show("No has seleccionat el teu personatge");
            else
            {
                Game1.Loading LoadVariable = new Game1.Loading();
                LoadVariable.Loaded = false;

                ThreadStart LoadingThreadStart = delegate { this.LoadingThread(LoadVariable); };
                loadingThread = new Thread(LoadingThreadStart);
                loadingThread.Start();

                using (var game = new Game1(TrainingState, Difficulty[DifficultyPos], LoadVariable))
                    game.Run();
                TrainingState.OpponentCharacter_Selected.Clear();
                OpponentListPanel.Controls.Clear();
                TrainingState.Opponentnum_players = 0;
            }
        }

        public void LoadingThread(Game1.Loading LoadVariable)
        {
            LoadingForm LoadForm = new LoadingForm(LoadVariable);
            LoadForm.ShowDialog();

            loadingThread.Abort();
        }

        //Escollir el personatge
        public void Return_btn_Click(object sender, EventArgs e)
        {
            ScreenSelected = 0;
            UpdateScreen();

        }
        public void PlayerSelectChar_btn_Click(object sender, EventArgs e)
        {
            TrainingState.PlayerCharacter_Selected= characterSelected[charSelectedPos];            
        }
        public void OpponentSelectChar_btn_Click(object sender, EventArgs e)
        {
            if (TrainingState.Opponentnum_players < 7)
            {
                string OpponentSelected = characterSelected[OpponentcharSelectedPos];
                TrainingState.OpponentCharacter_Selected.Add(OpponentSelected);
                PictureBox Opponent = new PictureBox();
                Opponent.Size = new Size(250, 40);
                Opponent.ImageLocation = "../../../Pictures/Characters/" + characterSelected[OpponentcharSelectedPos] +" look.png";
                Opponent.SizeMode = PictureBoxSizeMode.Zoom;
                Opponent.Location = new Point(0,TrainingState.Opponentnum_players * 40 - OpponentListPanel.VerticalScroll.Value);
                Opponent.Click += delegate { DeselectOpponent_Click(sender, e, OpponentSelected, Opponent); };
                OpponentListPanel.Controls.Add(Opponent);
                TrainingState.Opponentnum_players++;
            }
        }
        public void DeselectOpponent_Click(object sender, EventArgs e, string opponentName, PictureBox opponent)
        {
            TrainingState.OpponentCharacter_Selected.Remove(opponentName);
            TrainingState.Opponentnum_players--;

            for (int i = OpponentListPanel.Controls.IndexOf(opponent); i < OpponentListPanel.Controls.Count - 1; i++)
            {
                OpponentListPanel.Controls[i+1].Location = new Point(0, i * 40 - OpponentListPanel.VerticalScroll.Value);
            }

            OpponentListPanel.Controls.Remove(opponent);
        }

        public void LeftOpponentChar_btn_Click(object sender, EventArgs e)
        {
            if (OpponentcharSelectedPos > 0)
                OpponentcharSelectedPos--;
            else
                OpponentcharSelectedPos = 3;

            Opponentcharacter_pb.Image.Dispose();
            Opponentcharacter_pb.ImageLocation = "../../../Pictures/Characters/" + characterSelected[OpponentcharSelectedPos] + " stop.gif";
            Opponentcharacter_pb.Load();
            Opponentcharacter_pb.Refresh();
            OpponentCharName_lbl.Text = "Nom: " + characterSelected[OpponentcharSelectedPos];
            OpponentCharDescription_lbl.Text = characterDescription[OpponentcharSelectedPos];

            
        }

        public void RightOpponentChar_btn_Click(object sender, EventArgs e)
        {
            if (OpponentcharSelectedPos < 3)
                OpponentcharSelectedPos++;
            else
                OpponentcharSelectedPos = 0;

            Opponentcharacter_pb.Image.Dispose();
            Opponentcharacter_pb.ImageLocation = "../../../Pictures/Characters/" + characterSelected[OpponentcharSelectedPos] + " stop.gif";
            Opponentcharacter_pb.Load();
            Opponentcharacter_pb.Refresh();
            OpponentCharName_lbl.Text = "Nom: " + characterSelected[OpponentcharSelectedPos];
            OpponentCharDescription_lbl.Text = characterDescription[OpponentcharSelectedPos];
        }

        //Selecció de difficultat
        public void LeftDifficulty_btn_Click(object sender, EventArgs e)
        {
            if (DifficultyPos > 0)
                DifficultyPos--;
            else
                DifficultyPos = 4;

            DifficultySelected_pb.Image.Dispose();
            DifficultySelected_pb.ImageLocation = "../../../Pictures/Difficulty/" + DifficultyName[DifficultyPos];
            DifficultySelected_pb.Load();
            DifficultySelected_pb.Refresh();

            
        }

        public void RightDifficulty_btn_Click(object sender, EventArgs e)
        {
            if (DifficultyPos < 4)
                DifficultyPos++;
            else
                DifficultyPos = 0;

            DifficultySelected_pb.Image.Dispose();
            DifficultySelected_pb.ImageLocation = "../../../Pictures/Difficulty/" + DifficultyName[DifficultyPos];
            DifficultySelected_pb.Load();
            DifficultySelected_pb.Refresh();

        }

    }
}
