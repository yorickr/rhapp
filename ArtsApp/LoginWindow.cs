using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArtsApp
{
    public partial class LoginWindow : Form
    {
        public bool done { get; set; } = false;
        

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


        public String returnUsername()
        {
            return username.ToString();
        }

        public String returnPassWord()
        {
            return password.ToString();
        }

        private void Login_Click(object sender, EventArgs e)
        {
            done = true;

            this.Close();
        }
    }
}
