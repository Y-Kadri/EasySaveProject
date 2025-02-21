using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace EasySave_Project.Server
{
    public class Client
    {
        public TcpClient client;
        public NetworkStream stream { get; }
        public string name;

        public Client(string name)
        {
            client = Connexion.Conn();
            stream = client.GetStream();
            this.name = name;
            Console.WriteLine("✅ Connecté au serveur !");
            
            Utils.SendToServer(name, stream);

            // Démarre un thread pour recevoir les messages du serveur
            Thread receiveThread = new Thread(() => Utils.ReceiveMessages(stream));
            receiveThread.Start();
        }
        

        public void Start()
        {
            // stream.Close();
            // client.Close();
        }
        
    }
}