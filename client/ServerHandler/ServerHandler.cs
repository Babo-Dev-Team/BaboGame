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

    public class ServerHandler
    {
        private Socket server;
        private IPAddress serverIP; //= IPAddress.Parse("192.168.56.103");
        private IPEndPoint serverIPEP; //= new IPEndPoint(direc, 9092);

        public ServerHandler()
        {

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
                server.Connect(this.serverIPEP);//Intentamos conectar el socket
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
            string response = this.ReceiveReponse();
            if (response == "OK")
            {
                error = 0;
            }
            else if (response == "FAIL")
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
            this.SendRequest("1/" + username + "/");
            string response = this.ReceiveReponse();
            if (response == "00:00:00")
                return null;
            else
                return response;
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

