using System;
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

        private void SendRequest(string request)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(request);
            server.Send(msg);
        }
    }
}
