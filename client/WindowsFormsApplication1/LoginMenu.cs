using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BaboGameClient
{
    public partial class LoginMenu : Form
    {
        // Paràmetres de connexió
        public const int shiva_port = 50084;
        public const string shiva_ip = "147.83.117.22";
        public const string local_ip = "192.168.56.103";
        public const string local2_ip = "192.168.56.101";

        ServerHandler serverHandler;

        // Necessitem el notification worker per passar-li la instància del queries form un cop el creem
        NotificationWorker notificationWorker;

        // A aquest form li passem el Serve Handler i el Notification Worker ja inicalitzats
        public LoginMenu(ServerHandler serverHandler, NotificationWorker notificationWorker)
        {
            this.serverHandler = serverHandler;
            this.notificationWorker = notificationWorker;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //serverHandler = new ServerHandler();
        }

        // arranquem el queries form i el passem al Notification Worker
        public void LoginOk()
        {
            MessageBox.Show("Login OK!");
            QueriesForm queriesForm = new QueriesForm(serverHandler, notificationWorker);
            notificationWorker.QueriesForm = queriesForm;
            queriesForm.ShowDialog();
        }

        // connexió i login directes (sense fer anar al Notification Worker)
        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                MessageBox.Show("Els camps estan buits!");
                return;
            }
            
            int error = serverHandler.Connect("192.168.56.104", shiva_port); //Quim:192.168.56.103  Albert:192.168.56.101 Joel:192.168.56.104
            if (error == -1)
            {
                MessageBox.Show("Connection Error.");
            }
            else if (error == -2)
            {
                MessageBox.Show("Error: server Full. Try again later.");
            }
            else
            {
                error = serverHandler.Login(this.UsernameTextBox.Text, this.PasswordTextBox.Text);
                if (error == 0)
                {
                    // arranquem el queries form
                    this.LoginOk();
                }
                else if (error == -1)
                {
                    MessageBox.Show("Error. usuari / password incorrectes");
                }
                else if (error == -2)
                {
                    MessageBox.Show("Error en el missatge. Comprova que estiguin tots els camps");
                }
            }
        }

        // sign up directe (sense fer anar al Notification Worker
        private void SignupButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                MessageBox.Show("Els camps estan buits!");
                return;
            }
            int error = serverHandler.Connect(local2_ip, shiva_port); //Quim:192.168.56.103  Albert:192.168.56.101 Joel:192.168.56.104
            if (error != 0)
            {
                MessageBox.Show("Connection Error.");

            }
            else
            {
                error = serverHandler.SignUp(this.UsernameTextBox.Text, this.PasswordTextBox.Text);
                if (error == 0)
                {
                    MessageBox.Show("Usuari creat.");

                    // instanciar form consultes
                    this.LoginOk();
                }
                else if (error == -1)
                {
                    MessageBox.Show("Error. aquest usuari ja existeix!");
                }
                else if (error == -2)
                {
                    MessageBox.Show("Error en el missatge");
                }
            }
        }
    }
}
