using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace BaboGameClient
{
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
                if(responseType == DataGridUpdateRequested)
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
           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // instanciem classes
            NotificationWorker notificationWorker = new NotificationWorker();
            ServerHandler serverHandler = new ServerHandler();

            // iniciem el thread de notificacions
            ThreadStart threadStart = delegate { notificationWorker.Start(); };
            ThreadNotificationWorker = new Thread(threadStart);
            ThreadNotificationWorker.Start();

            // instanciem la UI
            LoginMenu loginMenu = new LoginMenu(serverHandler, notificationWorker);
            Application.Run(loginMenu);

            // Parem el notification worker
            ThreadNotificationWorker.Abort();
        }
    }
}
