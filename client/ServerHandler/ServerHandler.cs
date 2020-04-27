using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaboGameClient
{
    public class ConnectedUser
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class PreGameState
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        public int UserCount { get; set; }
        public int Playing { get; set; }
    }

    public static class ReceiverArgs
    {
        public static List<ConnectedUser> connectedList;
        public static List<PreGameState> gameTable;
        public static string responseStr;
        public static int newDataFromServer;
        public static Socket server;
    }

    public class ServerReceiver
    {
        private bool discardUpdates;
        private string response;
        //private ReceiverArgs receiverArgs;
        public ServerReceiver()
        {
            //this.receiverArgs = args; // passa per referencia
            //this.discardUpdates = false;
        }

        // Garanteix la integritat de les dades. 
        // Si un update no es pot escriure a les estructures de dades,
        // no rebrem més updates fins que ho poguem fer.
        // Recomanat quan es reben dades només un cop i no les volem perdre
        public void IntegrityMode()
        {
            this.discardUpdates = false;
        }

        // Garanteix un temps d'execució màxim per la recepció i escriptura d'un update.
        // Si en el moment de rebre un update no el podem escriure, es descarta i seguim escoltant.
        // Garanteix que sempre es disposa de l'update més recent i es recomana per l'execució de la partida.
        public void PerformanceMode()
        {
            this.discardUpdates = true;
        }

        public void Init()
        {
            //ReceiverArgs. = args;
            this.discardUpdates = false;
        }

        public void Start()
        {
            bool allowNextUpdate = false;
            while (true)
            {
                // agafem la resposta del server
                response = this.ReceiveReponse();
                allowNextUpdate = false;
                while(!allowNextUpdate)
                {
                    // comprovem que tenim accés exclusiu garantit a les estructures de dades
                    // el flag a 0 indica que ServerHandler ja ha entregat l'update anterior,
                    // per tant, no tornarà a accedir a les dades fins que no escrivim el nou update
                    if (ReceiverArgs.newDataFromServer == 0)
                    {
                        // arranquem l'id de resposta
                        string num = response.Split('/')[0];
                        int responseType = Convert.ToInt32(num);

                        switch (responseType)
                        {
                            // temps total jugador
                            case 1:
                                if (response == "00:00:00")
                                    response = null;
                                ReceiverArgs.responseStr = response; // passem update
                                break;
                            case 4:
                                // només copiem la resposta si és vàlida
                                if (response == "OK" || response == "FAIL")
                                {
                                    ReceiverArgs.responseStr = response;
                                }
                                break;
                            // llista de connectats
                            case 6:
                                // passem la nova llista a les estructures de dades compartides
                                ReceiverArgs.connectedList = JsonSerializer.Deserialize<List<ConnectedUser>>(response);
                                break;
                            
                            // llista de partides
                            case 8:
                                List<PreGameState> gameTable = JsonSerializer.Deserialize<List<PreGameState>>(response);
                                break;

                            default:
                                response = null;
                                break;
                        }
                        // indiquem que hi ha noves dades i seguim escoltant
                        ReceiverArgs.newDataFromServer = 1;
                        allowNextUpdate = true;
                    }
                    else
                    {
                        // si estem en mode performance i no hem pogut escriure l'update, el descartem
                        // per seguir escoltant.
                        if (this.discardUpdates)
                        {
                            allowNextUpdate = true;
                        }
                    }
                }
                
            }         
        }

        private string ReceiveReponse()
        {
            //Recibimos la respuesta del servidor
            byte[] response = new byte[8192];
            ReceiverArgs.server.Receive(response);
            return Encoding.ASCII.GetString(response).Split('\0')[0];
        }
    }

    public class ServerHandler
    {
        private Socket server;
        private IPAddress serverIP; //= IPAddress.Parse("192.168.56.103");
        private IPEndPoint serverIPEP; //= new IPEndPoint(direc, 9092);

        // estructures de dades internes per a que actualitzi el Receiver
        //private List<ConnectedUser> connectedList;
        //private List<PreGameState> gameTable;
        //private string responseStr;

        // flag d'escriptura de les dades
        //private int newDataFromServer;

        // crear la subclasse del receiver

        private ServerReceiver Receiver;
        //private ReceiverArgs receiverArgs;

        // crear el thread per la sublasse
        // mirar com passar-li accés a les dades i al flac (classe estatica rollo struct?)


        public ServerHandler()
        {
            //receiverArgs = new ReceiverArgs();
            //receiverArgs.connectedList = this.connectedList;
            //receiverArgs.gameTable = this.gameTable;
            //receiverArgs.server = this.server;

        }

        public int Connect(string ip, int port)
        {
            int error = 0;
            this.serverIP = IPAddress.Parse(ip);
            this.serverIPEP = new IPEndPoint(this.serverIP, port);

            //Creamos el socket 
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(this.serverIPEP); //Intentamos conectar el socket
                
                // inicialitzem el Receiver passant-li les estructures de dades
                ReceiverArgs.newDataFromServer = 0;
                ReceiverArgs.server = this.server;
                this.Receiver = new ServerReceiver();
                this.Receiver.Init();
            }
            catch (SocketException)
            {
                //Si hay excepcion imprimimos error y salimos del programa con return 
                error = -1;
            }
            return error;
        }

        public void Disconnect()
        {
            // Nos desconectamos
            string request = "/0";
            this.SendRequest(request);
            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }

        public int Login (string username, string password)
        {
            int error;
            if ((username == null) || (password == null))
                return -2;
            error = this.SendRequest("4/" + username + "/" + password + "/");
            if (error != 0)
                return error;
            //string response = this.ReceiveReponse();

            this.Receiver.Start();
            int i = 0;
            while (ReceiverArgs.newDataFromServer == 0)
            {
                i++;
            }

            if (ReceiverArgs.responseStr == "OK")
            {
                error = 0;
            }
            else if (ReceiverArgs.responseStr == "FAIL")
            {
                error = -1;
            }
            else error = -2;
            return error;
        }

        public int SignUp(string username, string password)
        {
            int error;
            if ((username == null) || (password == null))
                return -2;
            error = this.SendRequest("5/" + username + "/" + password + "/");
            if (error != 0)
                return error;
            string response = this.ReceiveReponse();
            if (response == "OK")
            {
                error = 0;
            }
            else if (response == "USED")
            {
                error = -1;
            }
            else error = -2;
            return error;
        }

        // retorna el temps en format HH:MM:SS
        public string GetTimePlayed (string username)
        {
            this.Receiver.Start();
            this.SendRequest("1/" + username + "/");

            // wait
            int i = 0;
            while (ReceiverArgs.newDataFromServer == 0)
            {
                i++;
            }

            return ReceiverArgs.responseStr;

            //string response = this.ReceiveReponse();
            //if (response == "00:00:00")
            //    return null;
            //else
            //    return response;
        }

        // retorna una matriu amb tantes files com usuaris i 2 columnes
        // la 0 pel username i la 1 pel nombre de partides guanyades
        public string[][] GetRanking()
        {
            this.SendRequest("2/");
            string response = this.ReceiveReponse();
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
            return ranking;
        }

        // retorna una matriu com GetRanking amb les parelles
        // username - character per la partida consultada
        public string[][] GetGameCharacters(string game)
        {
            this.SendRequest("3/" + game + "/");
            string response = this.ReceiveReponse();
            int n_pairs = Convert.ToInt32(response.Split('/')[0]);
            string[] playerCharPairs = new string[n_pairs];
            string[][] playerChars = new string[n_pairs][];
            for (int i = 0; i < n_pairs; i++)
            {
                playerChars[i] = new string[2];
            }
            if (n_pairs > 0)
            {
                response = response.Remove(0, response.IndexOf("/") + 1); //eliminem el n_chars de la resposta
                for (int i = 0; i < n_pairs; i++)
                {
                    playerCharPairs = response.Split('/');
                    playerChars[i] = playerCharPairs[i].Split('*');
                }
            }
            return playerChars;
        }

        // retorna el temps en format HH:MM:SS
        public string  CreateGame(string gameName)
        {
            this.SendRequest("7/" + gameName + "/");
            string response = this.ReceiveReponse();
            return response;
        }

        //retorna una matriu el qual només retorna els usuaris connectats
        //Només té una columna de connectats i no necessita entrades
        public List<ConnectedUser> GetConnected()
        {
            this.SendRequest("6/");
            string response = this.ReceiveReponse();
            List<ConnectedUser> connectedList = JsonSerializer.Deserialize<List<ConnectedUser>>(response);
            return connectedList;
        }

        public List<PreGameState> GetGameTable()
        {
            this.SendRequest("8/");
            string response = this.ReceiveReponse();
            List<PreGameState> gameTable = JsonSerializer.Deserialize<List<PreGameState>>(response);
            return gameTable;
        }
        
        private int SendRequest(string request)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(request);
            try
            {
                server.Send(msg);
                return 0;
            }
            catch
            {
                return -2;
            }
        }

        private string ReceiveReponse()
        {
            //Recibimos la respuesta del servidor
            byte[] response = new byte[8192];
            this.server.Receive(response);
            return Encoding.ASCII.GetString(response).Split('\0')[0];
        }
    }
}

