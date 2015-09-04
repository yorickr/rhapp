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
        private delegate void SetTextCallback(string text);
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
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (o, args) =>
                {
                    while (true)
                    {
                        sendMessage("ST");
                        Thread.Sleep(1000);
                    }
                };
                worker.RunWorkerAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("woops");
            }

        }

        public void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            Thread.Sleep(500);
            string indata = sp.ReadExisting();
            string[] search = new string[] { "ACK", "ERROR", "RUN" };

            bool toUI = true;
            foreach (string s in search)
            {
                if (!s.Contains(indata))
                {
                    toUI = false;
                    break;
                }
                else
                {
                    toUI = true; 
                    
                }
;
                
               
            }
            if (!toUI)
            {
                UpdateStatus(indata);
            }
            else
            {
                this.BeginInvoke(new SetTextDeleg(DisplayToUI), new object[] { indata });
            }

        }

        public void UpdateStatus(string indata)
        {

            if (label5.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(UpdateStatus);
                Invoke(d, new object[] { indata });
            }
            else
            {
                label5.Text = indata.Replace("\t", "    ");
            }
            
        }

        private void DisplayToUI(string displayData)
        {
            richTextBox1.AppendText(displayData);
            richTextBox1.ScrollToCaret();

        }

        private void sendMessage(string s)
        {
            port.WriteLine(s);
        }

        private void sendMultipleMessages(string[] s)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, args) =>
            {
                foreach (string str in s)
                {
                    sendMessage(str);
                    Thread.Sleep(500);
                }
            };
            worker.RunWorkerAsync();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sendMessage("RS");
            richTextBox1.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sendMessage("ST");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sendMultipleMessages(new string[] { "CM", "PW " + textBox1.Text });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sendMultipleMessages(new string[] { "CM", "PT  " + textBox2.Text });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            sendMultipleMessages(new string[] { "CM", "PD  " +  Convert.ToDouble( textBox3.Text)*10 });
        }

        private void button6_Click(object sender, EventArgs e)
        {
            sendMultipleMessages(new string[] { "CM", "PE  " + textBox4.Text });
        }

    }
}
