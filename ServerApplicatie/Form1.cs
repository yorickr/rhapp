using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace ServerApplicatie
{
    public partial class Form1 : Form
    {
        private TcpListener server;
        private List<Client> clients;

        public Form1()
        {
            InitializeComponent();
            clients = new List<Client>();
            CreateFolder();
        }

        public List<Client> GetClients()
        {
            return clients;
        }

        private void StartServer(object sender, EventArgs e)
        {
            if (server == null) {
                IPAddress ip = IPAddress.Parse(GetGateway());
                server = new TcpListener(IPAddress.Any, 1338);
                DisplayOnScreen("Server is running...");
                UpdateIP(ip.ToString());
                server.Start();
                Thread thread = new Thread(new ThreadStart(ServerRunning));
                thread.Start();
            }
        }

        private void StopServer(object sender, EventArgs e)
        {
            foreach (Client c in clients)
            {
                c.SafeData();
            }
            server.Stop();
            DisplayOnScreen("Server stopped!");
            server = null;
        }

        private void CreateFolder()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDoc‌​uments), "clientdata");
            bool exists = System.IO.Directory.Exists(path);
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(path);
                DisplayOnScreen("Nieuwe folder aan gemaakt!");
            }
        }

        private void ServerRunning()
        {
            DisplayOnScreen("Accepting clients!");
            while (true) {
                try {
                    Client client = new Client(server.AcceptTcpClient(), this);
                    this.clients.Add(client);
                    DisplayOnScreen("New client added!");
                } catch (Exception e)
                {

                }
            }
        }


        private delegate void SetTextCallback(string msg);

        public void UpdateIP(String text) {
            if (label3.InvokeRequired)
            {
                Invoke(new SetTextCallback(UpdateIP), new object[] { text });
            }
            else
            { label3.Text = text; }
        }


        private String GetGateway()
        {
            string url = "http://checkip.dyndns.org";
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] AllData = response.Split(':');
            string dataString = AllData[1].Substring(1);
            string[] data = dataString.Split('<');
            string ip = data[0];
            return ip;
        }

        public void DisplayOnScreen(string displayData)
        {
            if (richTextBox1.InvokeRequired)
            {
                Invoke(new SetTextCallback(DisplayOnScreen), new object[] { displayData });
            }
            else {
                richTextBox1.AppendText( "\n" + displayData);
                richTextBox1.ScrollToCaret();
            }
        }

        public void SendClientsToDoctors()
        {
            foreach (Client client in clients)
            {
                if (client.IsDoctor())
                {
                    foreach(Client c in clients)
                    {
                        if (!c.IsDoctor())
                        {
                            client.WriteMessage("05" + c.getClientname());
                        }
                    }
                }
            }
        }

        public void RemoveClient(Client client)
        {
            if (clients.Contains(client))
            {
                clients.Remove(client);
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

    public class Client
    {
        private TcpClient client;
        private String clientname;
        private StreamReader reader;
        private StreamWriter writer;
        private List<String> data;
        private Form1 application;
        private Thread thread;
        private bool isAlive = true;
        private List<Client> races;

        public Client(TcpClient client, Form1 application)
        {
            this.client = client;
            this.application = application;
            reader = new StreamReader(client.GetStream(), Encoding.ASCII);
            writer = new StreamWriter(client.GetStream(), Encoding.ASCII);
            thread = new Thread(HandleClient);
            thread.Start();
            data = new List<String>();
            races = new List<Client>();
        }

        public Boolean IsDoctor()
        {
            if (clientname.Equals("DOCTOR"))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public void AddRace(Client c)
        {
            races.Add(c);
        }

        private void HandleClient()
        {
            while (true)
            {
                try {
                    HandleData(reader.ReadLine());
                } catch (Exception e)
                {
                    if (isAlive) { 
                        application.DisplayOnScreen("Error on client " + clientname + "! Closing client!");
                        StopConnection();
                    }
                }
            }
        }

        private void AddData(String data)
        {
            this.data.Add(data);
            foreach(Client client in races)
            {
                client.WriteMessage("06" + data);
            }
        }

        public void WriteMessage(String message)
        {
            writer.WriteLine(message);
            writer.Flush();
        }

        public String getClientname()
        {
            return clientname;
        }

        private void HandleData(String data)
        {
            application.DisplayOnScreen(data);
            switch (data.Substring(0, 2))
            {
                case "00": NameClient(data); break;
                case "01": AddData(data.Substring(2)); break;
                case "02": StreamData(data.Substring(2));  break;
                case "03": StopConnection();  break;
                case "04": Chat(data.Substring(2)); break;
                case "06": Race(data.Substring(2)); break;
                default: application.DisplayOnScreen("Incorrect message send!"); break;
            }
        }

        private void Race(String data)
        {
            String[] clientnames = data.Split(':');
            Client client1 = null;
            Client client2 = null;
            foreach(Client c in application.GetClients())
            {
                if (c.clientname == clientnames[0])
                {
                    client1 = c;
                }
                if (c.clientname == clientnames[1])
                {
                    client2 = c;
                }
            }
            client1.AddRace(client2);
            client2.AddRace(client1);
        }

        private void Chat(String data)
        {
            if (IsDoctor())
            {
                String[] splitData = data.Split(':');
                foreach(Client c in application.GetClients())
                {
                    if (c.clientname == splitData[0])
                    {
                        c.WriteMessage("04" + splitData[1]);
                    }
                }
            } else
            {
                foreach(Client client in application.GetClients())
                {
                    if (client.IsDoctor())
                    {
                        client.WriteMessage("04" + clientname + ":" + data);
                    }
                }
            }
        }

        private void StreamData(String data)
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDoc‌​uments), data + ".dat");
            if (File.Exists(path))
            {
                String[] lines = File.ReadAllLines(path);
                foreach (String line in lines)
                {
                    WriteMessage("02"+ line);
                }
            }
        }

        private void NameClient(String data)
        {
            clientname = data.Substring(2);
            application.SendClientsToDoctors();
        }

        public void SafeData()
        {
            if (clientname != "")
            {
                try {
                    string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDoc‌​uments), "clientdata", clientname + ".dat");
                    if (File.Exists(path))
                    {
                        String[] lines = System.IO.File.ReadAllLines(path);
                        StreamWriter tw = new StreamWriter(path);
                        foreach (String line in lines)
                        {
                            tw.WriteLine(line.ToString());
                        }
                        tw.WriteLine("----------------------------------------");
                        foreach (String line in data)
                        {
                            tw.WriteLine(line.ToString());
                        }
                        tw.Close();
                    }
                    else
                    {
                        File.Create(path).Close();
                        StreamWriter tw = new StreamWriter(path);
                        foreach (String line in data)
                        {
                            tw.WriteLine(line.ToString());
                        }
                        tw.Close();
                    }
                } catch(Exception e)
                {
                    application.DisplayOnScreen("Couldn't safe the data of " + clientname);
                }
            }
        }

        private void StopConnection()
        {
            SafeData();
            application.DisplayOnScreen("Connection closed with " + clientname);
            isAlive = false;
            this.thread.Abort();
            application.RemoveClient(this);
        }

    }
}
