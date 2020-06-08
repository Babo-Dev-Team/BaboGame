using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaboGame_test_2;

namespace BaboGameClient
{
    public partial class LoadingForm : Form
    {
        Game1.Loading LoadVariable;
        public LoadingForm(Game1.Loading LoadVariable)
        {
            InitializeComponent();
            Loading_pb.ImageLocation = "../../../Pictures/Layouts/Loading.gif";
            Loading_pb.Load();
            Loading_pb.Refresh();
            this.LoadVariable = LoadVariable;
            timer1.Interval = 100;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(LoadVariable.Loaded)
            {
                timer1.Stop();
                this.Close();
            }
        }
    }
}
