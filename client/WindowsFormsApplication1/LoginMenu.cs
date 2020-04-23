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

namespace BaboGameClient
{
    public partial class LoginMenu : Form
    {
        ServerHandler serverHandler;
        public LoginMenu()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            serverHandler = new ServerHandler();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                MessageBox.Show("Els camps estan buits!");
                return;
            }
            int error = serverHandler.Connect("147.83.117.22", 50084); //Quim:192.168.56.103  Albert:192.168.56.101 Joel:192.168.56.104 Shiva: 147.83.117.22
            if (error != 0)
            {
                MessageBox.Show("Connection Error.");
            }
            else
            {
                error = serverHandler.Login(this.UsernameTextBox.Text, this.PasswordTextBox.Text);
                if (error == 0)
                {
                    MessageBox.Show("Login OK!");
                    QueriesForm queriesForm = new QueriesForm(serverHandler);
                    queriesForm.ShowDialog();
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

        private void SignupButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                MessageBox.Show("Els camps estan buits!");
                return;
            }
            int error = serverHandler.Connect("147.83.117.22", 50084); //Quim:192.168.56.103  Albert:192.168.56.101 Joel:192.168.56.104 Shiva: 147.83.117.22
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
                    QueriesForm queriesForm = new QueriesForm(serverHandler);
                    queriesForm.ShowDialog();
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
