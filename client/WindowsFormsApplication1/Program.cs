using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Media;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BaboGameClient
{
    public class MusicPlayer
    {
        //private SoundPlayer player;
        private MediaPlayer mediaPlayer;
        private MediaTimeline timeLine;

        public MusicPlayer()
        {
            //this.player = new SoundPlayer();
            this.mediaPlayer = new MediaPlayer();
            this.mediaPlayer.Volume = 0.25;
            timeLine = new MediaTimeline();
        }

        public void Play(string filePath)
        {
            try
            {
                //this.player.SoundLocation = filePath;
                //this.player.PlayLooping();
                var uri = new Uri(filePath, UriKind.Relative);
                //MessageBox.Show(uri.ToString());
                //timeLine.Source = uri;
                //mediaPlayer.Open(uri);
                //timeLine.RepeatBehavior = RepeatBehavior.Forever;

                //mediaPlayer.Clock = timeLine.CreateClock();
                //timeLine.BeginTime = TimeSpan.Zero;
                //timeLine.Duration = TimeSpan.FromMinutes(3);
                //timeLine.
                //mediaPlayer.Clock.Controller.Begin();
                mediaPlayer.Open(uri);
                mediaPlayer.Play();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error playing music");
            }
        }

        public void Stop()
        {
            this.mediaPlayer.Stop();
        }
    }
    // Classe que gestiona les notificacions des de la UI.
    // El mètode Start és el que es passa al Thread de notificacions
    public class NotificationWorker
    {
        //------------------------------------------------
        // DELEGATE METHODS FOR ACCESSING UI ELEMENTS
        //------------------------------------------------

        // el mètode delegat per accedir al data grid view
        delegate void ConnectedListDelegate(List<ConnectedUser> connectedList);

        // el mètode delegat per crear un popup amb el temps jugat
        delegate void TimePlayedPopupDelegate(string TimePlayed);

        // el mètode delegat per accedir al data grid view amb una taula de strings
        delegate void CharacterListDelegate(string[][] characterList);

        //el el mètode delegat per accedir al data grid del Ranking
        delegate void RankingDelegate(string[][] rankingList);

        // el mètode delegat per crear un popup com a resposta a la creació d'un nou usuari
        delegate void SignUpPopup(string response);

        // el mètode delegat per crear un popup com a resposta a la creació de partida
        delegate void CreatePartyPopup(string response);

        // el mètode delegat per la notificació de invitació
        delegate void InvitationMessage(string gameName, string creatorName);

        // el mètode delegat per la notificació de invitació confirmada
        delegate void InvitationMessageConfirmation();

        // el mètode delegat per avisar l'estat de la prepartida
        delegate void PreGameStateUserDelegate(List<PreGameStateUser> connectedList);

        //el mètode delegat pels missatges del xat
        delegate void ChatMessage(string username, string message);
        //------------------------------------------------
        // ATTRIBUTES
        //------------------------------------------------

        // necessitem una referència a queries form per poder modificar la UI    
        public QueriesForm QueriesForm { get; set; }

        // Quina info estem mostrant pel data grid?
        // 6: Llista de connectats
        public int DataGridUpdateRequested { get; set; }

        //------------------------------------------------
        // THREAD WORKER
        //------------------------------------------------
        public void Start()
        {
            while (true)
            {
                // esperem a que el receiver ens indiqui
                ReceiverArgs.notificationSignal.WaitOne();

                // agafem el tipus de missatge del Receiver
                int responseType = ReceiverArgs.responseType;

                // si hem rebut una update de dades (data grid), actualitzem el data grid
                if (responseType == DataGridUpdateRequested)
                {
                    // Actualitzem la llista de connectats
                    if (this.DataGridUpdateRequested == 6)
                    {
                        List<ConnectedUser> connectedList = ReceiverArgs.connectedList;
                        ConnectedListDelegate gridDelegate = new ConnectedListDelegate(this.QueriesForm.UpdateConnectedList);
                        QueriesForm.Invoke(gridDelegate, new object[] { connectedList });
                        connectedList = null;
                    }

                    // TODO: Actualitzem la taula de partides

                }
                // La resta de missatges de moment son les queries:

                // Mostra el temps jugat
                else if (responseType == 1)
                {
                    string time = ReceiverArgs.responseStr;
                    TimePlayedPopupDelegate timeDelegate = new TimePlayedPopupDelegate(this.QueriesForm.TimePlayedPopup);
                    QueriesForm.Invoke(timeDelegate, new object[] { time });
                }

                // TODO: la resta de queries

                //Mostra els personatges utilitzats en una partida
                else if (responseType == 3)
                {
                    string CharacterList = ReceiverArgs.responseStr;
                    CharacterListDelegate characterDelegate = new CharacterListDelegate(this.QueriesForm.UpdateCharactersList);

                    int n_pairs = Convert.ToInt32(CharacterList.Split('/')[0]);
                    string[] playerCharPairs = new string[n_pairs];
                    string[][] playerChars = new string[n_pairs][];
                    for (int i = 0; i < n_pairs; i++)
                    {
                        playerChars[i] = new string[2];
                    }
                    if (n_pairs > 0)
                    {
                        CharacterList = CharacterList.Remove(0, CharacterList.IndexOf("/") + 1); //eliminem el n_chars de la resposta
                        for (int i = 0; i < n_pairs; i++)
                        {
                            playerCharPairs = CharacterList.Split('/');
                            playerChars[i] = playerCharPairs[i].Split('*');
                        }
                    }

                    QueriesForm.Invoke(characterDelegate, new object[] { playerChars });
                }

                //Mostra el ranking
                else if (responseType == 2)
                {
                    string response = ReceiverArgs.responseStr;
                    RankingDelegate rankingDelegate = new RankingDelegate(this.QueriesForm.UpdateRanking);
                    int n_pairs = Convert.ToInt32(response.Split('/')[0]);
                    string[] rankingPairs = new string[n_pairs];
                    string[][] ranking = new string[n_pairs][];
                    for (int i = 0; i < n_pairs; i++)
                    {
                        ranking[i] = new string[2];
                    }
                    if (n_pairs > 0)
                    {
                        response = response.Remove(0, response.IndexOf("/") + 1); //eliminem el n_chars de la resposta
                        for (int i = 0; i < n_pairs; i++)
                        {
                            rankingPairs = response.Split('/');
                            ranking[i] = rankingPairs[i].Split('*');
                        }
                    }

                    QueriesForm.Invoke(rankingDelegate, new object[] { ranking });
                }
                //Crea un compte amb SignUp
                else if (responseType == 5)
                {
                    string response = ReceiverArgs.responseStr;
                    SignUpPopup SignUpDelegate = new SignUpPopup(this.QueriesForm.SignUpPopup);
                    QueriesForm.Invoke(SignUpDelegate, new object[] { response });
                }
                //Mostra la resposta a la partida creada
                else if (responseType == 7)
                {
                    string response = ReceiverArgs.responseStr;
                    CreatePartyPopup createPartyDelegate = new CreatePartyPopup(this.QueriesForm.CreatePartyPopup);
                    QueriesForm.Invoke(createPartyDelegate, new object[] { response });
                }
                //Notificació en invitacions
                else if (responseType == 9)
                {
                    string response = ReceiverArgs.responseStr;
                    string[] splitResponse = response.Split('/');
                    if (splitResponse[0] == "NOTIFY")
                    {
                        InvitationMessage invitationMessageDelegate = new InvitationMessage(this.QueriesForm.InvitationNotificationMessage);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { splitResponse[1], splitResponse[2] });
                    }

                    else if (splitResponse[0] == "ACCEPTED")
                    {
                        InvitationMessageConfirmation invitationMessageDelegate = new InvitationMessageConfirmation(this.QueriesForm.AcceptedGamePopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { });
                    }

                    else if (splitResponse[0] == "REJECTED")
                    {
                        InvitationMessageConfirmation invitationMessageDelegate = new InvitationMessageConfirmation(this.QueriesForm.RejectGamePopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { });
                    }

                    else if (splitResponse[0] == "FAIL")
                    {
                        InvitationMessageConfirmation invitationMessageDelegate = new InvitationMessageConfirmation(this.QueriesForm.FailResponseGamePopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { });
                    }

                    else if (splitResponse[0] == "LOST")
                    {
                        InvitationMessage invitationMessageDelegate = new InvitationMessage(this.QueriesForm.LoseInvitationPopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { splitResponse[1], splitResponse[2] });
                    }
                }

                //Notificació de l'actualització del GameStateUser
                else if (responseType == 10)
                {
                    List<PreGameStateUser> gameState = ReceiverArgs.gameState;
                    PreGameStateUserDelegate userStateDelegate = new PreGameStateUserDelegate(this.QueriesForm.GameStateUpdate);
                    QueriesForm.Invoke(userStateDelegate, new object[] { gameState });
                    gameState = null;
                }
                //Missatges del Xat
                else if (responseType == 11)
                {
                    string response = ReceiverArgs.responseStr;
                    string[] splitResponse = response.Split('/');
                    ChatMessage ChatDelegate = new ChatMessage(this.QueriesForm.SentMessageChat);
                    QueriesForm.Invoke(ChatDelegate, new object[] {splitResponse[0], splitResponse[1]});
                }

                //Notificacions sobre la selecció de personatges, cancel.lar partides i l'inici d'aquestes
                else if (responseType == 12)
                {
                    string response = ReceiverArgs.responseStr;
                    string[] splitResponse = response.Split('/');
                    if (splitResponse[0] == "CHAROK")
                    {
                        InvitationMessageConfirmation invitationMessageDelegate = new InvitationMessageConfirmation(this.QueriesForm.AcceptCharacterPopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { });
                    }
                    else if (splitResponse[0] == "CHARFAIL")
                    {
                        InvitationMessageConfirmation invitationMessageDelegate = new InvitationMessageConfirmation(this.QueriesForm.FailCharacterPopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { });
                    }
                    else if (splitResponse[0] == "START")
                    {
                        InvitationMessageConfirmation invitationMessageDelegate = new InvitationMessageConfirmation(this.QueriesForm.StartGamePopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { });
                    }
                    else if (splitResponse[0] == "NOTALLSELECTED")
                    {
                        InvitationMessageConfirmation invitationMessageDelegate = new InvitationMessageConfirmation(this.QueriesForm.NotAllSelectedPopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { });
                    }
                    else if (splitResponse[0] == "ALONE")
                    {
                        InvitationMessageConfirmation invitationMessageDelegate = new InvitationMessageConfirmation(this.QueriesForm.AlonePlayerPopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { });
                    }
                    else if (splitResponse[0] == "CANCEL")
                    {
                        InvitationMessage invitationMessageDelegate = new InvitationMessage(this.QueriesForm.CancelGamePopup);
                        QueriesForm.Invoke(invitationMessageDelegate, new object[] { splitResponse[1], splitResponse[2] });
                    }
                }
            }
        }

    }

    static class Program
    {
       
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Senyal per notificar al notifcation worker que arriba nou update des del receiver.
            ReceiverArgs.notificationSignal = new AutoResetEvent(false);

            // thread del notification worker
            Thread ThreadNotificationWorker;
            //Thread ThreadMusicPlayer;
           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // instanciem classes
            NotificationWorker notificationWorker = new NotificationWorker();
            ServerHandler serverHandler = new ServerHandler();
            //MusicPlayer musicPlayer = new MusicPlayer();

            // iniciem el thread de notificacions
            ThreadStart threadStart = delegate { notificationWorker.Start(); };
            ThreadNotificationWorker = new Thread(threadStart);
            ThreadNotificationWorker.Start();

            //threadStart = delegate { }

            // instanciem la UI
            LoginMenu loginMenu = new LoginMenu(serverHandler, notificationWorker);
            Application.Run(loginMenu);

            // Parem el notification worker
            ThreadNotificationWorker.Abort();
        }
    }
}
