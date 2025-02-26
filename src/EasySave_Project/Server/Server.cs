
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Server.Dto;
using Server.Models;
using Server.Utils;

namespace Server
{
    internal static class Server
    {
        private static List<User> clients = new List<User>();
        
        private static TcpListener listener;
        
        public static Dictionary<string, string> pendingResponses = new Dictionary<string, string>();

        /// <summary>
        /// Main entry point for the server application.
        /// Initializes and starts the TCP listener, accepts client connections,
        /// and manages client interactions in separate threads.
        /// </summary>
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

                User newUser = new User(tcpClient) { Name = clientName };
                clients.Add(newUser);

                Console.WriteLine($"🔵 Nouveau client connecté: {newUser.Name} ({newUser.IPAddress}:{newUser.Port})");
                
                BroadCastNewConnection(newUser);
                
                Thread clientThread = new Thread(() => HandleClient(newUser));
                clientThread.Start();
            }
        }

        /// <summary>
        /// Broadcasts a new user connection to all connected clients except the sender.
        /// </summary>
        /// <param name="sender">The user who just connected.</param>
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

        /// <summary>
        /// Retrieves a connected user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The corresponding User object if found, otherwise null.</returns>
        static User? GetUserById(string id)
        {
            foreach (User user in clients)
            {
                if (user.Id == id)
                    return user;
            }

            return null;
        }

        /// <summary>
        /// Handles client communication, processes messages, executes commands,
        /// and manages client disconnections.
        /// </summary>
        /// <param name="user">The user to handle.</param>
        static void HandleClient(User user)
        {
            try
            {
                while (true)
                {
                    string message = ServerUtils.ReadMessage(user);
                    if (string.IsNullOrEmpty(message)) break;

                    Console.WriteLine($"📩 Reçu du client {user.Id}: {message}");

                    if (message == "GET_USERS")
                    {
                        SendClientsList(user);
                        continue;
                    }

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

                            continue;
                        }
                    }
                    catch (JsonException)
                    {
                        
                    }
                    
                    try
                    {
                        var jobCommand = JsonSerializer.Deserialize<CommandDTO>(message);
                        if (jobCommand != null)
                        {
                            Console.WriteLine($"✅ Message traité comme JobCommandDTO : {jobCommand.command}");

                            if (jobCommand.command == "RUN_JOB_USERS" && !string.IsNullOrEmpty(jobCommand.id))
                            {
                                Console.WriteLine($"📤 Exécution des jobs pour l'utilisateur {jobCommand.id}");
                                ExecuteJobs(user, jobCommand);
                            }
                            
                            continue;
                        }
                    }
                    catch (JsonException)
                    {
                        
                    }

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

        /// <summary>
        /// Executes jobs for a specified user by forwarding the command
        /// and awaiting a response from the client.
        /// </summary>
        /// <param name="user">The requesting user.</param>
        /// <param name="command">The job execution command.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private static async Task ExecuteJobs(User user, CommandDTO command)
        {
            User? userConnect = GetUserById(command.id);

            if (userConnect != null)
            {
                
                sendMessage(userConnect, JsonSerializer.Serialize(command));
                Console.WriteLine($"📤 Demande envoyée à {userConnect.Name} pour ses jobs.");
                
                try
                {
                    
                    string? response = await ServerUtils.WaitForResponse(userConnect, 5000);

                    if (!string.IsNullOrEmpty(response))
                    {
                        Console.WriteLine($"📩 Réponse reçue de {userConnect.Name} : {response}");
                        sendMessage(user, response);
                    }
                    else
                    {
                        Console.WriteLine("❌ Aucun job reçu après plusieurs tentatives.");
                        sendMessage(user, "Executejobàéchoué");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur lors de la réception des jobs : {ex.Message}");
                    sendMessage(user, "Executejobàéchoué");
                }
                return;
            }
            sendMessage(user, "Executejobàéchoué");
        }

        /// <summary>
        /// Requests the list of jobs from a specified user and forwards the response.
        /// </summary>
        /// <param name="user">The requesting user.</param>
        /// <param name="id">The identifier of the target user.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private static async Task GetJobsUser(User user, string id)
        {
            User? userConnect = GetUserById(id);

            if (userConnect != null)
            {
                sendMessage(userConnect, "GET_JOBS");
                Console.WriteLine($"📤 Demande envoyée à {userConnect.Name} pour ses jobs.");

                try
                {
                    string? response = await ServerUtils.WaitForResponse(userConnect, 5000);

                    if (!string.IsNullOrEmpty(response))
                    {
                        Console.WriteLine($"📩 Réponse reçue de {userConnect.Name} : {response}");
                        sendMessage(user, response);
                    }
                    else
                    {
                        Console.WriteLine("❌ Aucun job reçu après plusieurs tentatives.");
                        sendMessage(user, "getjobéchoué");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur lors de la réception des jobs : {ex.Message}");
                    sendMessage(user, "getjobéchoué");
                }
                return;
            }
            
            sendMessage(user, "getjobéchoué");
        }

        /// <summary>
        /// Disconnects a user from another user they are connected to.
        /// </summary>
        /// <param name="user">The user initiating the disconnection.</param>
        /// <param name="id">The ID of the user to disconnect from.</param>
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
            
            sendMessage(user, "Déconnectionéchoué");
        }

        /// <summary>
        /// Connects a user to another user by establishing a link between them.
        /// </summary>
        /// <param name="user">The user initiating the connection.</param>
        /// <param name="id">The ID of the target user to connect to.</param>
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
            
            sendMessage(user, "Connectionéchoué");
        }

        /// <summary>
        /// Sends a message to a specified user over their network stream.
        /// Also logs the message for record-keeping.
        /// </summary>
        /// <param name="user">The recipient user.</param>
        /// <param name="message">The message to send.</param>
        private static void sendMessage(User user, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            NetworkStream stream = user.TcpClient.GetStream();
    
            stream.Write(data, 0, data.Length);
            stream.Flush();

            ServerUtils.WriteLog(user, message);
        }

        /// <summary>
        /// Sends the list of currently connected clients to a specified user.
        /// </summary>
        /// <param name="user">The user requesting the client list.</param>
        static void SendClientsList(User user)
        {
            try
            {
                var clientDTOs = clients.Select(c => new ClientDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    IPAddress = c.IPAddress,
                    Port = c.Port
                }).ToList();

                string jsonData = JsonSerializer.Serialize(clientDTOs);
                
                sendMessage(user, jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lors de l'envoi de la liste des clients : {ex.Message}");
            }
        }
    }
}
