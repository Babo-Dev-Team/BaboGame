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
