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
            int port = 8912;
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

                
                //Envoyer a tout les clients qu'un nouveau user est la
                BroadCastNewConnection(newUser);
                
                Thread clientThread = new Thread(() => HandleClient(newUser));
                clientThread.Start();
            }
        }

        static void BroadCastNewConnection(User sender)
        {
            string message = "NEW_USER";

            foreach (User user in clients)
            {
                if (user != sender)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    user.TcpClient.GetStream().Write(data, 0, data.Length);
                }
            }
        }

        static User? GetUserById(string id)
        {
            foreach (User user in clients)
            {
                if (user.Id == id)
                    return user;
            }

            return null;
        }

        static void HandleClient(User user)
        {
            try
            {
                while (true)
                {
                    string message = ServerUtils.ReadMessage(user);
                    if (string.IsNullOrEmpty(message)) break;

                    Console.WriteLine($"📩 Reçu du client {user.Id}: {message}");

                    // Vérifier si c'est une requête pour obtenir la liste des utilisateurs
                    if (message == "GET_USERS")
                    {
                        SendClientsList(user);
                        continue; // Passe à l'itération suivante
                    }

                    try
                    {
                        // Désérialisation JSON en un objet dynamique
                        var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(message);

                        // Vérification de la présence de la clé "command"
                        if (requestData != null && requestData.TryGetValue("command", out string command) && requestData.TryGetValue("id", out string id))
                        {
                            if (command == "CONNECTE_USERS" && id != null)
                            {
                                ConnecteUser(user,id);
                            }
                            
                            if (command == "DISCONNECTE_USERS" && id != null)
                            {
                                DisconnecteUser(user, id);
                            }
                            
                            if (command == "GET_JOB_USERS" && id != null)
                            {
                                GetJobsUser(user,id);
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ Erreur: Commande invalide.");
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"❌ Erreur de parsing JSON: {ex.Message}");
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

        private static void GetJobsUser(User user, string id)
        {
            User? userConnect = GetUserById(id);

            if (userConnect != null)
            {
                sendMessage(userConnect, "GET_JOBS");
                string response = ServerUtils.ReadMessage(userConnect);
                sendMessage(user, response);
                return;
            }
            
            sendMessage(user, "get job échoué");
        }

        private static void DisconnecteUser(User user, string id)
        {
            User? disConnectTo = GetUserById(id);

            if (disConnectTo != null)
            {
                user.ConnectTo = null;
                
                sendMessage(disConnectTo,$"L'utilisateur {user.Name} s'est déconnecté à vous !" );
                sendMessage(user, $"Déconnection à {disConnectTo.Name} réussi !");

                return;
            }
            
            sendMessage(user, "Déconnection échoué");
        }

        private static void ConnecteUser(User user, string id)
        {
            User? connectTo = GetUserById(id);

            if (connectTo != null)
            {
                user.ConnectTo = connectTo;
                sendMessage(connectTo, $"L'utilisateur {user.Name} s'est connecté à vous !");
           
                sendMessage(user, $"Connection à {connectTo.Name} réussi !");
                return;
            }
            
            sendMessage(user, "Connection échoué");
        }

        private static void sendMessage(User user, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            NetworkStream stream = user.TcpClient.GetStream();
    
            stream.Write(data, 0, data.Length);
            stream.Flush(); // 🔥 Assurer l'envoi immédiat

            ServerUtils.WriteLog(user, message);
        }


        static void SendClientsList(User user)
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
                sendMessage(user, jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lors de l'envoi de la liste des clients : {ex.Message}");
            }
        }


    }
}
