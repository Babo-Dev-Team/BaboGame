using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

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

    public class PreGameStateUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string CharName { get; set; }
        public int UserState { get; set; }
    }

    //Informació del joc
    
    public class GameState
    {
        public int gameID { get; set; }
        public int playable { get; set; }
        public int nPlayers { get; set; }
        public List<CharacterState> characterStatesList { get; set; }
        public List<projectileState> projectileStates { get; set; }
    }

    public class CharacterState
    {
        //string charName;
        public int charID { get; set; }
        public int posX { get; set; }
        public int posY { get; set; }
        public int velX { get; set; }
        public int velY { get; set; }
        public float dirX { get; set; }
        public float dirY { get; set; }
        public int health { get; set; }
    }

    public class initState
    {
        public string gameName { get; set; }
        public int nPlayers { get; set; }
        public user thisUser { get; set; }
        public List<user> users { get; set; }
    }

    public class user
    {
        public int userId { get; set; }
        public int charId { get; set; }
        public string userName { get; set; }
        public string charName { get; set; }
    }

    public class projectileState
    {
        public int projectileID { get; set; }
        public int shooterID { get; set; }
        public char projectileType { get; set; }
        public int posX { get; set; }
        public int posY { get; set; }
        public float directionX { get; set; }
        public float directionY { get; set; }
        public float LinearVelocity { get; set; }
        public int hitCount { get; set; }
        public int targetX { get; set; }
        public int targetY { get; set; }

        /*

        public projectileState(int projectileID, int shooterID, char projectileType, int posX, int posY, float directionX, float directionY, float LinearVelocity, int hitCount, int targetX, int targetY)
        {
            this.projectileID = projectileID;
            this.shooterID = shooterID;
            this.projectileType = projectileType;
            this.posX = posX;
            this.posY = posY;
            this.directionX = directionX;
            this.directionY = directionY;
            this.LinearVelocity = LinearVelocity;
            this.hitCount = hitCount;
            this.targetX = targetX;
            this.targetY = targetY;
        }
        */
    }

    public class playerUpdate
    {
        public CharacterState characterState { get; set; }
        public List<projectileState> projectileStates { get; set; }
    }

    public class GenericResponse
    {
        public GenericResponse(int number, string response)
        {
            this.responseType = number;
            this.responseStr = response;
        }

        public GenericResponse()
        {

        }

        public int responseType;
        public string responseStr;
    }
    

    // arguments estàtics per passar informació entre el thread del Receiver i el thread principal.
    // Les dades les agafarà el Notification Worker directament, i pel monogame la idea és fer anar 
    // el Server Handler i mode realtime al receiver
    public static class ReceiverArgs
    {
        public static List<ConnectedUser> connectedList;    // llistes per parsejar el JSON
        public static List<PreGameState> gameTable;
        public static List<PreGameStateUser> gameState;
        public static string responseStr;                   // resposta per string
        public static int newDataFromServer;                // Flag pel mode Realtime
        public static Socket server;                        // el socket per llegir
        public static int responseType;                     // indica el número de resposta
        public static AutoResetEvent notificationSignal;    // senyal pel mode Notificacions
        public static Queue<GenericResponse> responseFifo;
        public static GenericResponse realtimeResponse;

    }

    public class ServerReceiver
    {  
        private string response;
        //private Queue<int, string> responseFifo;
        //private int responseFifoWrittable;

        public ServerReceiver()
        {

        }

        //------------------------------------------------
        // THREAD WORKERS (2)
        //------------------------------------------------

        // Gestió Event Driven.
        // el receiver, cada cop que rep un update, assenyala amb notificationSignal
        // al NotificationWorker de la GUI que té notificacions pendents per processar.
        // el NotificationWorker crida el mètode Read*** segons les dades que s'han rebut
        // No s'utilitza el flag newDataFromServer.
        public void StartNotificationMode()
        {
            ReceiverArgs.newDataFromServer = -1; // indiquem que no l'utilitzem
            while (true)
            {
                // agafem la resposta del server
                response = this.ReceiveReponse();
                
                this.ProcessData();

                // Permetem al NotificationWorker agafar les dades de la llista
                //this.notificationSignal.Set();
                ReceiverArgs.notificationSignal.Set();
                // Donem temps a notificationWorker per a que agafi les dades
                // TODO: implementar un lock segons aquest exemple: 
                // https://docs.microsoft.com/en-us/dotnet/api/system.threading.eventresetmode?view=netcore-3.1#System_Threading_EventResetMode_ManualReset
                Thread.Sleep(100);
            }
        }

        // Gestió Real Time.
        // el receiver, cada cop que rep un update, posa el flag newDataFromServer a 1.
        // d'aquesta manera, la propera vegada que el monogame demani si hi ha canvis, 
        // el serverhandler li passarà l'actualització llegint de ReceiverArgs.
        // i tornarà a posar el flag a 0. Amb aquest mode pot ser que es descartin updates
        // si no es processen abans que arribi el següent update.
        // no s'utilitza el senyal notificationSignal.
        public void StartRealtimeMode()
        {
            const int responseArraySize = 1024;
            char[] separator = new char[] { '/' };
            ReceiverArgs.responseFifo = new Queue<GenericResponse>();
            ReceiverArgs.realtimeResponse = new GenericResponse();
            string[] responseSplitByMessage;
            ReceiverArgs.newDataFromServer = 0;
            bool backOffRequested = false;
            int lastAvailable = 0;

            GenericResponse[] responseArray = new GenericResponse[responseArraySize];
            for (int i = 0; i < responseArray.Count(); i++)
            {
                responseArray[i] = new GenericResponse();
            }
            bool incompleteLastMessage = false;
            int messageSkipper;
            while (true)
            {
                messageSkipper = 0;
                // acumulem a la cua
                if (ReceiverArgs.newDataFromServer == 0)
                {
                    byte[] responseBytes = new byte[65536];
                    ReceiverArgs.server.Receive(responseBytes);
                    if(backOffRequested)
                    {
                        backOffRequested = false;
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes("103/RESUME");
                        ReceiverArgs.server.Send(msg);
                    }

                    responseSplitByMessage = Encoding.ASCII.GetString(responseBytes).Split('\0')[0].Split('|');
                    if (responseSplitByMessage.Count() > responseArraySize)
                    {
                        Exception ex = new Exception("Error: massa missatges a la cua del socket!");
                        throw ex;
                    }
                    if (incompleteLastMessage)
                    {
                        messageSkipper = 1;
                    }

                    if (!string.IsNullOrWhiteSpace(responseSplitByMessage[responseSplitByMessage.Count() - 1]))
                    {
                        incompleteLastMessage = true;
                        //Exception ex = new Exception("Error: missatge incomplert al Server Receiver!");
                        //throw ex;
                        Console.WriteLine("Warning: missatge incomplert al Server Receiver!");

                    }
                    else incompleteLastMessage = false;

                    for (int i = messageSkipper; i < responseSplitByMessage.Count() - 1; i++)
                    {
                        try
                        {
                            string[] responseSplitByNumber = responseSplitByMessage[i].Split(separator, 2);
                            responseArray[i].responseType = Convert.ToInt32(responseSplitByNumber[0]);
                            responseArray[i].responseStr = responseSplitByNumber[1];
                        }
                        catch (FormatException ex)
                        {
                            throw ex;
                        }
                    }

                    bool realtimeMessageFound = false;
                    bool otherMessageFound = false;

                    // mirar que no hagi de restar 2 en comptes d'1 per l'index -> DONE
                    for (int i = responseSplitByMessage.Count() - 2; i >= messageSkipper; i--)
                    {

                        if (responseArray[i].responseType == 103 && !realtimeMessageFound)
                        {
                            realtimeMessageFound = true;
                            ReceiverArgs.realtimeResponse.responseStr = responseArray[i].responseStr;
                            ReceiverArgs.realtimeResponse.responseType = responseArray[i].responseType;
                        }
                        else if (responseArray[i].responseType != 0 && responseArray[i].responseType != 103)
                        {
                            otherMessageFound = true;
                            GenericResponse queueElement = new GenericResponse(responseArray[i].responseType, responseArray[i].responseStr);
                            ReceiverArgs.responseFifo.Enqueue(queueElement);
                        }
                    }

                    if (realtimeMessageFound && !otherMessageFound)
                    {
                        ReceiverArgs.newDataFromServer = 103;
                    }
                    else if (!realtimeMessageFound && otherMessageFound)
                    {
                        ReceiverArgs.newDataFromServer = 1;
                    }
                    else if (realtimeMessageFound && otherMessageFound)
                    {
                        ReceiverArgs.newDataFromServer = 1103;
                    }
                       
                }

                else
                {
                    // esperem a que el monogame buidi la cua
                    while (ReceiverArgs.newDataFromServer != 0)
                    {
                        int data = ReceiverArgs.server.Available;
                        if (data > 16384 && (!backOffRequested || data > lastAvailable))
                        {
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("103/BACK-OFF");
                            ReceiverArgs.server.Send(msg);
                            backOffRequested = true;
                        }
                        else if (data > 32768)
                        {
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("103/BACK-OFF");
                            ReceiverArgs.server.Send(msg);
                            backOffRequested = true;
                        }
                        lastAvailable = data;
                        Thread.Sleep(2);
                    }
                }

                // esperem a rebre una resposta del servidor
                /*response = this.ReceiveReponse();

                while (ReceiverArgs.newDataFromServer != 0)
                {
                    Thread.Sleep(3);
                }
                // si podem escriure perquè l'update anterior s'ha processat, ho fem.
                // si no podem escriure, descartem l'update i seguim escoltant.
                if (ReceiverArgs.newDataFromServer == 0)
                {
                    this.ProcessData();

                    // avisem que hi ha un nou update
                    ReceiverArgs.newDataFromServer = 1;
                }*/
            }
        }

        //------------------------------------------------
        // INTERNAL METHODS
        //------------------------------------------------

        // funció per processar les respostes que es reben.
        private void ProcessData()
        {
            // arranquem l'id de resposta
            string[] splitResponse = response.Split('/');
            string num = splitResponse[0];
            int responseType = Convert.ToInt32(num);

            // indiquem el tipus de dades que s'han rebut
            ReceiverArgs.responseType = responseType;

            switch (responseType)
            {
                // temps total jugador
                case 1:
                    if (response == "00:00:00")
                        response = null;
                    ReceiverArgs.responseStr = splitResponse[1]; // passem update
                    break;
                case 2:
                    //Ranking dels jugadors
                    string Ranking = splitResponse[1];
                    for (int i = 2; i < splitResponse.Length; i++)
                    {
                        Ranking = Ranking + "/" + splitResponse[i];

                    }
                    ReceiverArgs.responseStr = Ranking; // passem update
                    break;
                case 3:
                    //Entrega els personantges que han jugat una partida
                    string CharactersList = splitResponse[1];
                    for (int i = 2; i < splitResponse.Length; i++)
                    {
                        CharactersList = CharactersList + "/" + splitResponse[i];

                    }
                    ReceiverArgs.responseStr = CharactersList;
                    break;
                case 4:
                    // només copiem la resposta si és vàlida
                    if (splitResponse[1] == "OK" || splitResponse[1] == "FAIL")
                    {
                        ReceiverArgs.responseStr = splitResponse[1];
                    }
                    break;
                case 5:
                    //Crea un compte
                    ReceiverArgs.responseStr = splitResponse[1];
                    break;
                // llista de connectats
                case 6:
                    // passem la nova llista a les estructures de dades compartides
                    ReceiverArgs.connectedList = JsonSerializer.Deserialize<List<ConnectedUser>>(splitResponse[1]);
                    break;
                case 7:
                    ReceiverArgs.responseStr = splitResponse[1];
                    break;

                // taula de partides
                case 8:
                    ReceiverArgs.gameTable = JsonSerializer.Deserialize<List<PreGameState>>(splitResponse[1]);
                    break;

                // notificació de invitacions
                case 9:
                    string Invitation = splitResponse[1];
                    for (int i = 2; i < splitResponse.Length; i++)
                    {
                        Invitation = Invitation + "/" + splitResponse[i];

                    }
                    ReceiverArgs.responseStr = Invitation;
                    break;

                // notificació d'avís en el joc
                case 10:
                    ReceiverArgs.gameState = JsonSerializer.Deserialize<List<PreGameStateUser>>(splitResponse[1]);
                    break;

                //Missatges del xat de la partida
                case 11:
                    string message = splitResponse[1];
                    for (int i = 2; i < splitResponse.Length; i++)
                    {
                        message = message + "/" + splitResponse[i];

                    }
                    ReceiverArgs.responseStr = message;
                    break;

                // notificacions d'estat del joc
                case 12:
                    string gameStr = splitResponse[1];
                    for (int i = 2; i < splitResponse.Length; i++)
                    {
                        gameStr = gameStr + "/" + splitResponse[i];

                    }
                    ReceiverArgs.responseStr = gameStr;
                    break;
                case 101:
                    ReceiverArgs.responseStr = splitResponse[1];
                    break;
                case 102:
                    ReceiverArgs.responseStr = splitResponse[1];
                    break;
                case 103:
                    ReceiverArgs.responseStr = splitResponse[1];
                    break;
                default:
                    response = null;
                    break;
            }
        }

        // escoltem el servidor i ens quedem només amb el primer missatge (partim per |)
        private string ReceiveReponse()
        {
            //Recibimos la respuesta del servidor
            byte[] response = new byte[32768];
            ReceiverArgs.server.Receive(response);
            return Encoding.ASCII.GetString(response).Split('|')[0];
        }
    }

    public class ServerHandler
    {
        // la instància del receiver
        private ServerReceiver Receiver;
        Thread threadReceiver;

        private const int SERVER_RSP_LEN = 32768;
        private Socket server;
        private IPAddress serverIP; //= IPAddress.Parse("192.168.56.103");
        private IPEndPoint serverIPEP; //= new IPEndPoint(direc, 9092);

        public ServerHandler()
        {

        }

        //------------------------------------------------
        // NEW METHODS (USE WITH NOTIFICATION WORKER)
        //------------------------------------------------

        public void RequestTimePlayed(string username)
        {
            this.SendRequest("1/" + username + "/");
        }

        public void RequestRanking()
        {
            this.SendRequest("2/");
        }

        public void RequestGameCharacters(string partyID)
        {
            this.SendRequest("3/" + partyID + "/");
        }

        public void RequestSignUp(string username, string password)
        {
            this.SendRequest("5/" + username + "/" + password + "/");
        }

        public void RequestConnected()
        {
            this.SendRequest("6/");
        }

        public void RequestCreateParty(string name,string[] players)
        {
            string CreatePartyMsg = "7/ " + name + "/" + (players.Length-1) + "/";
            for(int i=0; i< players.Length;i++)
            {
                CreatePartyMsg += players[i] + "/";
            }

            this.SendRequest(CreatePartyMsg);
        }

        //Request de les notificacions de invitació
        public void RequestAcceptInvitation(string gameName)
        {
            this.SendRequest("9/ACCEPT/" + gameName + "/");
        }

        public void RequestRejectInvitation(string gameName)
        {
            this.SendRequest("9/REJECT/" + gameName + "/");
        }

        public void RequestChatMessage(string message)
        {
            this.SendRequest("11/" + message + "/");
        }

        public void RequestCancelGame(string gameName)
        {
            this.SendRequest("12/CANCEL/");
        }

        public void RequestStartGame(string gameName)
        {
            this.SendRequest("12/START/");
        }

        public void RequestSelectCharacter(string gameName, string character)
        {
            this.SendRequest("12/CHARACTER/" + character + "/");
        }

        //Request pel mode joc online
        public void RequestInitState()
        {
            this.SendRequest("101/HELLO/");
        }

        public void RequestRealTimeUpdate(playerUpdate playerState)
        {
            string playerInform = JsonSerializer.Serialize(playerState);
            this.SendRequest("104/" + playerInform + "|");
        }

        public void SwitchToRealtimeMode()
        {
            threadReceiver.Abort();
            ThreadStart threadStart = delegate { this.Receiver.StartRealtimeMode(); };
            threadReceiver = new Thread(threadStart);
            threadReceiver.Start();
        }

        public void SwitchToNotificationMode()
        {
            threadReceiver.Abort();
            Console.WriteLine("Attempting to leave the game...");
            //Thread.Sleep(1000);
            this.SendRequest("103/BACK-OFF");
            Thread.Sleep(100);
            this.SendRequest("103/BACK-OFF");
            Thread.Sleep(100);
            this.SendRequest("103/BACK-OFF");
            Thread.Sleep(100);
            this.SendRequest("101/LEAVE");
            int counter = 0;
            bool success = false;
            while (!success)
            {
                counter = 0;
                Console.WriteLine("Reached the counter");
                while (this.server.Available == 0 && counter < 50)
                { 
                 
                    ++counter;
                    Thread.Sleep(10);
                }
                if(server.Available > 0)
                {
                    Console.WriteLine("Threre is data in socket");
                    string response = this.ReceiveReponse();
                    Console.WriteLine("Response received: " + response);
                    if (response != "101/GOODBYE")
                    {
                        this.SendRequest("103/BACK-OFF");
                        Thread.Sleep(50);
                        this.SendRequest("101/LEAVE");
                    }
                    else
                    {
                        success = true;
                    }
                }
                else
                {
                    Console.WriteLine("Nothing available in server");
                    this.SendRequest("103/BACK-OFF");
                    Thread.Sleep(100);
                    this.SendRequest("101/LEAVE");
                }
            }       
            Console.WriteLine("Successfully left the game. Now entering Notifications mode...");
            ThreadStart threadStart = delegate { this.Receiver.StartNotificationMode(); };
            threadReceiver = new Thread(threadStart);
            threadReceiver.Start();
        }
        // TODO: Adaptar els metodes deprecated (al final del document) a metodes nous de tipus Request***

        // Aquests mètodes no s'han d'actualitzar ja que es segueixen fent servir igual 
        //------------------------------------------------
        // OLD METHODS (OK)
        //------------------------------------------------
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

                ReceiverArgs.server = this.server;
                this.Receiver = new ServerReceiver();

                ThreadStart threadStart = delegate { this.Receiver.StartNotificationMode(); };
                threadReceiver = new Thread(threadStart);



            }
            catch (SocketException)
            {
                //Si hay excepcion imprimimos error y salimos del programa con return 
                error = -1;
                return error;
            }
            //return error;

            string response = ReceiveReponse();
            if (response == "OK")
            {
                error = 0;
            }
            else if (response == "FULL")
                error = -2;
            return error;
        }

        public void Disconnect()
        {
            threadReceiver.Abort();
            // Nos desconectamos
            string request = "0/DISCONNECT";
            this.SendRequest(request);

            // parem el receiver
            //threadReceiver.Abort();

            server.Shutdown(SocketShutdown.Both);
            server.Close();
        }

        // login convencional (sense fer servir el Notification Worker ni el Server Receiver
        public int Login (string username, string password)
        {
            int error;
            if ((username == null) || (password == null))
                return -2;
            error = this.SendRequest("4/" + username + "/" + password + "/");
            if (error != 0)
                return error;

            string response = this.ReceiveReponse();
            if (response == "4/OK")
            {
                error = 0;
                threadReceiver.Start();
            }
            else if (response == "4/FAIL")
            {
                error = -1;
            }
            else error = -2;
            return error;
        }

        // sign up convencional (sense fer servir el Notification Worker ni el Server Receiver
        public int SignUp(string username, string password)
        {
            int error;
            if ((username == null) || (password == null))
                return -2;
            error = this.SendRequest("5/" + username + "/" + password + "/");
            if (error != 0)
                return error;

            string response = this.ReceiveReponse();
            if (response == "5/OK")
            {
                error = 0;
                threadReceiver.Start();
            }
            else if (response == "5/USED")
            {
                error = -1;
            }
            else error = -2;
            return error;
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
            byte[] response = new byte[SERVER_RSP_LEN];
            this.server.Receive(response);
            return Encoding.ASCII.GetString(response).Split('|')[0];
        }

        //------------------------------------------------
        // OLD METHODS (DEPRECATED)
        //------------------------------------------------ 

        // retorna el temps en format HH:MM:SS
        public string GetTimePlayed (string username)
        {
            this.SendRequest("1/" + username + "/");


            bool responseReceived = false;
            while (!responseReceived)
            {
                // esperem a rebre resposta
                if (ReceiverArgs.newDataFromServer == 0)
                {
                    Thread.Sleep(1);
                }

                // descartem totes les respostes que no siguin al login
                else if (ReceiverArgs.newDataFromServer == 1 && ReceiverArgs.responseType != 1)
                {
                    ReceiverArgs.newDataFromServer = 0;
                }

                else responseReceived = true;
            }                                       //*******************************
            ReceiverArgs.newDataFromServer = 0;
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

            // wait
            while (ReceiverArgs.newDataFromServer == 0)
            {
                Thread.Sleep(1);
            }

            bool responseReceived = false;
            while (!responseReceived)
            {
                // esperem a rebre resposta
                if (ReceiverArgs.newDataFromServer == 0)
                {
                    Thread.Sleep(1);
                }

                // descartem totes les respostes que no siguin al login
                else if (ReceiverArgs.newDataFromServer == 1 && ReceiverArgs.responseType != 2)
                {
                    ReceiverArgs.newDataFromServer = 0;
                }

                else responseReceived = true;
            }
            
            
            ReceiverArgs.newDataFromServer = 0;
            string response = ReceiverArgs.responseStr;
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
            ReceiverArgs.newDataFromServer = 0;
            return ranking;
        }

        // retorna una matriu com GetRanking amb les parelles
        // username - character per la partida consultada
        public string[][] GetGameCharacters(string game)
        {
            this.SendRequest("3/" + game + "/");


            bool responseReceived = false;
            while (!responseReceived)
            {
                // esperem a rebre resposta
                if (ReceiverArgs.newDataFromServer == 0)
                {
                    Thread.Sleep(1);
                }

                // descartem totes les respostes que no siguin al login
                else if (ReceiverArgs.newDataFromServer == 1 && ReceiverArgs.responseType != 3)
                {
                    ReceiverArgs.newDataFromServer = 0;
                }

                else responseReceived = true;
            }

            string response = ReceiverArgs.responseStr;
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
            ReceiverArgs.newDataFromServer = 0;
            return playerChars;
        }

        // retorna el temps en format HH:MM:SS
        public string  CreateGame(string gameName)
        {
            this.SendRequest("7/" + gameName + "/");

            // wait
            while (ReceiverArgs.newDataFromServer == 0)
            {
                Thread.Sleep(1);
            }


            bool responseReceived = false;
            while (!responseReceived)
            {
                // esperem a rebre resposta
                if (ReceiverArgs.newDataFromServer == 0)
                {
                    Thread.Sleep(1);
                }

                // descartem totes les respostes que no siguin al login
                else if (ReceiverArgs.newDataFromServer == 1 && ReceiverArgs.responseType != 7)
                {
                    ReceiverArgs.newDataFromServer = 0;
                }

                else responseReceived = true;
            }

            string response = ReceiverArgs.responseStr;
            ReceiverArgs.newDataFromServer = 0;
            return response;
        }

    

        //retorna una matriu el qual només retorna els usuaris connectats
        //Només té una columna de connectats i no necessita entrades
        public List<ConnectedUser> ReadConnected()
        {
            // esperem a rebre resposta
            if (ReceiverArgs.newDataFromServer == 0 || ReceiverArgs.responseType != 6)
            {
                return null;
            }
            else
            {
                List<ConnectedUser> connectedList = ReceiverArgs.connectedList;
                ReceiverArgs.newDataFromServer = 0;
                return connectedList;
            }

        }

        public List<PreGameState> GetGameTable()
        {
            this.SendRequest("8/");


            bool responseReceived = false;
            while (!responseReceived)
            {
                // esperem a rebre resposta
                if (ReceiverArgs.newDataFromServer == 0)
                {
                    Thread.Sleep(1);
                }

                // descartem totes les respostes que no siguin al login
                else if (ReceiverArgs.newDataFromServer == 1 && ReceiverArgs.responseType != 8)
                {
                    ReceiverArgs.newDataFromServer = 0;
                }

                else responseReceived = true;
            }

            List<PreGameState> gameTable = ReceiverArgs.gameTable;
            ReceiverArgs.newDataFromServer = 0;
            return gameTable;
        }
    }
}

