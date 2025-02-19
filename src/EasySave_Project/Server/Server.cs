using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Server
{
    class Server
    {
        private static List<User> clients = new List<User>();
        private static TcpListener listener;

        static void Main()
        {
            int port = 8080;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"✅ Serveur démarré sur le port {port}");

            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                NetworkStream stream = tcpClient.GetStream();
                
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string clientName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                // Créer un nouvel objet Client avec le nom reçu
                User newUser = new User(tcpClient) { Name = clientName };
                clients.Add(newUser);

                Console.WriteLine($"🔵 Nouveau client connecté: {newUser.Name} ({newUser.IPAddress}:{newUser.Port})");

                Thread clientThread = new Thread(() => HandleClient(newUser));
                clientThread.Start();
            }
        }

        static void HandleClient(User user)
        {
            NetworkStream stream = user.TcpClient.GetStream();

            try
            {
                while (true)
                {
                    string message = ServerUtils.ReadMessage(stream);
                    if (string.IsNullOrEmpty(message)) break;

                    Console.WriteLine($"📩 Reçu du client {user.Id}: {message}");

                    if (message == "GET_USERS")
                    {
                        SendClientsList(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur avec le client {user.Id}: {ex.Message}");
            }
            finally
            {
                clients.Remove(user);
                user.TcpClient.Close();
            }
        }

        

        static void SendClientsList(NetworkStream stream)
        {
            try
            {
                // 🔥 Convertir les objets `Client` en `ClientDTO`
                var clientDTOs = clients.Select(c => new ClientDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    IPAddress = c.IPAddress,
                    Port = c.Port
                }).ToList();

                // 🔥 Sérialiser en JSON
                string jsonData = JsonSerializer.Serialize(clientDTOs);

                // 🔥 Envoyer les données au client
                byte[] data = Encoding.UTF8.GetBytes(jsonData);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lors de l'envoi de la liste des clients : {ex.Message}");
            }
        }
        
        static void SendJobToClient(NetworkStream stream, User sender)
        {
            // 🔹 Afficher la liste des clients connectés
            string clientList = "\n👥 Clients connectés:\n";
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i] != sender) // Empêcher d'envoyer un job à soi-même
                    clientList += $"{i + 1} - {clients[i]}\n";
            }

            clientList += "\n🛠️ Choisis un client cible (Numéro) : ";
            byte[] data = Encoding.UTF8.GetBytes(clientList);
            stream.Write(data, 0, data.Length);

            // 🔹 Demander au client d'entrer un numéro
            byte[] inputRequest = Encoding.UTF8.GetBytes("INPUT_REQUEST");
            stream.Write(inputRequest, 0, inputRequest.Length);
            string clientChoice = ServerUtils.ReadMessage(stream);

            if (!int.TryParse(clientChoice, out int clientIndex) || clientIndex < 1 || clientIndex > clients.Count || clients[clientIndex - 1] == sender)
            {
                string errorMsg = "⚠️ Numéro invalide. Opération annulée.";
                data = Encoding.UTF8.GetBytes(errorMsg);
                stream.Write(data, 0, data.Length);
                return;
            }

            User targetUser = clients[clientIndex - 1];
            Console.WriteLine($"🎯 Client cible sélectionné : {targetUser}");

            // 🔹 Demander le nom du job
            string jobRequest = "\n💼 Entrez le nom du job à envoyer : ";
            data = Encoding.UTF8.GetBytes(jobRequest);
            stream.Write(data, 0, data.Length);

            stream.Write(inputRequest, 0, inputRequest.Length);
            string jobName = ServerUtils.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(jobName))
            {
                string errorMsg = "⚠️ Nom de job invalide. Opération annulée.";
                data = Encoding.UTF8.GetBytes(errorMsg);
                stream.Write(data, 0, data.Length);
                return;
            }

            Console.WriteLine($"📤 Envoi du job '{jobName}' à {targetUser}");

            // 🔹 Envoyer le job au client cible
            NetworkStream targetStream = targetUser.TcpClient.GetStream();
            string jobMessage = $"📩 Nouveau job reçu de {sender}: {jobName}";
            data = Encoding.UTF8.GetBytes(jobMessage);
            targetStream.Write(data, 0, data.Length);
        }


    }
}
