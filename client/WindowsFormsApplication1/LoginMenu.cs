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
            int error = serverHandler.Connect("192.168.56.103", 9094);
            if (error != 0)
            {
                MessageBox.Show("Connection Error.");
            }
            error = serverHandler.Login(this.UsernameTextBox.Text, this.PasswordTextBox.Text);
            if(error == 0)
            {
                MessageBox.Show("Login OK!");
            }
            else if (error == -1)
            {
                MessageBox.Show("Error. usuari / password incorrectes");
            }
        }

        private void SignupButton_Click(object sender, EventArgs e)
        {
            int error = serverHandler.Connect("192.168.56.103", 9094);
            if (error != 0)
            {
                MessageBox.Show("Connection Error.");
                // instanciar form consultes
            }
            error = serverHandler.SignUp(this.UsernameTextBox.Text, this.PasswordTextBox.Text);
            if (error == 0)
            {
                MessageBox.Show("Usuari creat.");
                // instanciar form consultes
            }
            else if (error == -1)
            {
                MessageBox.Show("Error. aquest usuari ja existeix!");
            }
        }
    }
}
