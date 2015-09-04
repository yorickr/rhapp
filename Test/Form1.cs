using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace Test
{
    public partial class Form1 : Form
    {
        private delegate void SetTextDeleg(string data);
        private SerialPort port;

        public Form1()
        {
            InitializeComponent();
            SettingRS232();
        }

        public void SettingRS232()
        {
            try
            {
                port = new SerialPort("COM1");

                port.BaudRate = 9600;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.DataBits = 8;
                port.Handshake = Handshake.None;
                port.ReadTimeout = 2000;
                port.WriteTimeout = 500;

                port.DtrEnable = true;
                port.RtsEnable = true;

                port.Open();

                port.DataReceived += DataReceivedHandler;

            }
            catch (Exception ex)
            {
                Console.WriteLine("woops");
            }

        }

        public void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            System.Threading.Thread.Sleep(500);
            string indata = sp.ReadExisting();
            this.BeginInvoke(new SetTextDeleg(DisplayToUI), new object[] { indata });

        }

        private void DisplayToUI(string displayData)
        {
            richTextBox1.Text += displayData;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            port.WriteLine("RS");
            richTextBox1.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            port.WriteLine("ST");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            port.WriteLine("CM\nPM " + textBox1.Text);
            richTextBox1.Text += "PM " + textBox1.Text;
        }

    }
}
