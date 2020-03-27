using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace BaboGameClient
{
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
            this.SendRequest("4/" + username + "/" + password + "/");
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
            this.SendRequest("5/" + username + "/" + password + "/");
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

        public string GetTimePlayed (string username)
        {
            this.SendRequest("1/" + username + "/");
            string response = this.ReceiveReponse();
            return response;
        }

        public string[][] GetRanking(string game)
        {
            this.SendRequest("2/" + game + "/");
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
                response.Remove(0, 2); //eliminem el n_chars de la resposta
                for (int i = 0; i < n_pairs; i++)
                {
                    rankingPairs = response.Split('/');
                    ranking[i] = rankingPairs[i].Split('*');
                }
            }
            return ranking;
        }

        public string[][] getGameCharacters(string game)
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
                response.Remove(0, 2); //eliminem el n_chars de la resposta
                for (int i = 0; i < n_pairs; i++)
                {
                    playerCharPairs = response.Split('/');
                    playerChars[i] = playerCharPairs[i].Split('*');
                }
            }
            return playerChars;
        }



        public 

        private void SendRequest(string request)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(request);
            server.Send(msg);
        }

        private string ReceiveReponse()
        {
            //Recibimos la respuesta del servidor
            byte[] response = new byte[200];
            this.server.Receive(response);
            return Encoding.ASCII.GetString(response).Split('\0')[0];
        }
    }
}

