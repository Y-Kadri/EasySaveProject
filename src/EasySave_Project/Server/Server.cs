
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server
{
    class Server
    {
        private static List<User> clients = new List<User>();
        private static TcpListener listener;
        public static Dictionary<string, string> pendingResponses = new Dictionary<string, string>();

        static void Main()
        {
            // int port = 8912;
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

                    // Vérifier si c'est une requête spéciale "GET_USERS"
                    if (message == "GET_USERS")
                    {
                        SendClientsList(user);
                        continue; // Passe à l'itération suivante
                    }

                    // Tenter la désérialisation en Dictionary<string, string>
                    try
                    {
                        var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(message);
                        if (requestData != null && requestData.TryGetValue("command", out string command) && requestData.TryGetValue("id", out string id))
                        {
                            Console.WriteLine($"✅ Message traité comme Dictionary<string, string> : {command}");

                            switch (command)
                            {
                                case "CONNECTE_USERS":
                                    ConnecteUser(user, id);
                                    break;
                                case "DISCONNECTE_USERS":
                                    DisconnecteUser(user, id);
                                    break;
                                case "GET_JOB_USERS":
                                    GetJobsUser(user, id);
                                    break;
                                default:
                                    Console.WriteLine("❌ Commande invalide.");
                                    break;
                            }

                            continue; // Évite d'ajouter aux pendingResponses
                        }
                    }
                    catch (JsonException)
                    {
                        // On ne fait rien ici, on passe à l'étape suivante
                    }
                    
                    // Tenter la désérialisation en JobCommandDTO
                    try
                    {
                        var jobCommand = JsonSerializer.Deserialize<CommandDTO>(message);
                        if (jobCommand != null)
                        {
                            Console.WriteLine($"✅ Message traité comme JobCommandDTO : {jobCommand.command}");

                            // Implémenter la logique spécifique pour JobCommandDTO ici
                            if (jobCommand.command == "RUN_JOB_USERS" && !string.IsNullOrEmpty(jobCommand.id))
                            {
                                Console.WriteLine($"📤 Exécution des jobs pour l'utilisateur {jobCommand.id}");
                                ExecuteJobs(user, jobCommand);
                            }
                            
                            continue; // Évite d'exécuter les autres tests
                        }
                    }
                    catch (JsonException)
                    {
                        // On ne fait rien ici, on tente la prochaine désérialisation
                    }

                    // 3️⃣ Si aucun des deux formats ne correspond, stocker le message dans pendingResponses
                    lock (pendingResponses)
                    {
                        pendingResponses[user.Id] = message;
                    }

                    Console.WriteLine($"⚠️ Message non reconnu, ajouté à pendingResponses.");
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

        private static async Task ExecuteJobs(User user, CommandDTO command)
        {
            User? userConnect = GetUserById(command.id);

            if (userConnect != null)
            {
                // 📤 Demande les jobs au Client 2
                sendMessage(userConnect, JsonSerializer.Serialize(command));
                Console.WriteLine($"📤 Demande envoyée à {userConnect.Name} pour ses jobs.");
                
                try
                {
                    // ⏳ Attente de la réponse avec limite de temps
                    string? response = await ServerUtils.WaitForResponse(userConnect, 5000); // Max 5s d'attente

                    if (!string.IsNullOrEmpty(response))
                    {
                        Console.WriteLine($"📩 Réponse reçue de {userConnect.Name} : {response}");
                        sendMessage(user, response); // 📤 Envoi des jobs au Client 1
                    }
                    else
                    {
                        Console.WriteLine("❌ Aucun job reçu après plusieurs tentatives.");
                        sendMessage(user, "Execute job à échoué");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur lors de la réception des jobs : {ex.Message}");
                    sendMessage(user, "Execute job à échoué");
                }
                return;
            }
            sendMessage(user, "Execute job à échoué");
        }

        private static async Task GetJobsUser(User user, string id)
        {
            User? userConnect = GetUserById(id);

            if (userConnect != null)
            {
                // 📤 Demande les jobs au Client 2
                sendMessage(userConnect, "GET_JOBS");
                Console.WriteLine($"📤 Demande envoyée à {userConnect.Name} pour ses jobs.");

                try
                {
                    // ⏳ Attente de la réponse avec limite de temps
                    string? response = await ServerUtils.WaitForResponse(userConnect, 5000); // Max 5s d'attente

                    if (!string.IsNullOrEmpty(response))
                    {
                        Console.WriteLine($"📩 Réponse reçue de {userConnect.Name} : {response}");
                        sendMessage(user, response); // 📤 Envoi des jobs au Client 1
                    }
                    else
                    {
                        Console.WriteLine("❌ Aucun job reçu après plusieurs tentatives.");
                        sendMessage(user, "get job échoué");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur lors de la réception des jobs : {ex.Message}");
                    sendMessage(user, "get job échoué");
                }
                return;
            }

            // 📢 Si l'utilisateur demandé n'est pas trouvé
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
            stream.Flush();

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
