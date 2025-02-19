using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
            
            SendName();

            // // Démarre un thread pour recevoir les messages du serveur
            // Thread receiveThread = new Thread(() => Utils.ReceiveMessages(stream));
            // receiveThread.Start();
        }
        
        private void SendName()
        {
            byte[] data = Encoding.UTF8.GetBytes(name);
            stream.Write(data, 0, data.Length);
        }

        public void Start()
        {
            // stream.Close();
            // client.Close();
        }

        public List<string> GetAllUserConnect()
        {
            List<string> users = new List<string>();

            try
            {
                // 🔥 Envoyer la requête pour obtenir la liste des utilisateurs
                byte[] requestData = Encoding.UTF8.GetBytes("GET_USERS");
                stream.Write(requestData, 0, requestData.Length);

                // 🔥 Lire la réponse du serveur
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // 🔥 Convertir la réponse en liste
                users.AddRange(response.Split('\n', StringSplitOptions.RemoveEmptyEntries));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lors de la récupération des utilisateurs : {ex.Message}");
            }

            return users;
        }
        
    }
}