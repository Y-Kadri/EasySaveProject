using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Layout;
using Server;

namespace EasySave_Project.Server
{
    public class Utils
    {
        public static JobService _jobService = new JobService();
        public static TranslationService _translationService = TranslationService.GetInstance();

        public static event Action<ObservableCollection<User>>? OnUsersReceived;

        // 📌 Queue pour stocker tous les messages reçus
        private static readonly ConcurrentQueue<string> messageQueue = new();

        // 🚀 Démarre la réception des messages en continu
        public static async Task StartListening(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("🔴 Connexion fermée par le serveur.");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Console.WriteLine($"\n[Server]: {message}");

                    // 🎯 Filtrage des messages spéciaux
                    switch (message)
                    {
                        case "NEW_USER":
                            BaseLayoutViewModel.Instance.AddNotification($"{_translationService.GetText("Nouvelutilisateurconnecté")}.");
                            continue;
                        case "GET_JOBS":
                            SendToServer(JsonSerializer.Serialize(JobManager.GetInstance().Jobs));
                            continue;
                    }
                    
                    if(!message.StartsWith("{") && !message.StartsWith("[")) {
                        BaseLayoutViewModel.Instance.AddNotification(message);
                        continue;
                    }

                    try
                    {
                        var jobCommand = JsonSerializer.Deserialize<CommandDTO<JobModel>>(message);
                        if (jobCommand != null)
                        {
                            Console.WriteLine($"✅ Message traité comme JobCommandDTO : {jobCommand.command}");

                            // Implémenter la logique spécifique pour JobCommandDTO ici
                            if (jobCommand.command == "RUN_JOB_USERS" && !string.IsNullOrEmpty(jobCommand.id))
                            {
                                BaseLayoutViewModel.Instance.AddNotification($"{_translationService.GetText("UtilisateurLanceJobs")} .");
                                ExecuteJobs(jobCommand.obj);
                            }
                            
                            continue; // Évite d'exécuter les autres tests
                        }
                    }
                    catch (JsonException)
                    {
                        // On ne fait rien ici, on tente la prochaine désérialisation
                    }

                    // ✅ Stocker dans la file d'attente
                    messageQueue.Enqueue(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"⚠️ Erreur de connexion : {e.Message}");
            }
        }

        private static void ExecuteJobs(List<JobModel> jobCommand)
        {
            SendToServer("Job lancé");
            foreach (JobModel job in jobCommand)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var (success, message) = _jobService.ExecuteOneJobThreaded((JobModel) job, null);
                    if (!success)
                    {
                        SendToServer(message);
                    }
                });
            }
        }

        // 📤 Envoi de messages au serveur
        public static void SendToServer(string message, NetworkStream stream = null)
        {
            Task.Run(() =>
            {
                try
                {
                    stream ??= GlobalDataService.GetInstance().client?.stream;

                    if (stream == null || !stream.CanWrite)
                    {
                        Console.WriteLine("❌ Erreur : Le stream est invalide ou non accessible.");
                        return;
                    }

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    stream.Flush();

                    Console.WriteLine($"📤 Message envoyé : {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Erreur lors de l'envoi : {ex.Message}");
                }
            });
        }

        // 📩 Récupérer un message (bloquant tant qu'il n'y en a pas)
        public static async Task<string?> GetNextMessage()
        {
            while (messageQueue.IsEmpty)
            {
                await Task.Delay(100); // Attente active
            }

            if (messageQueue.TryDequeue(out string message))
            {
                return message;
            }

            return null;
        }

        // 🎯 Attend une réponse spécifique et la retourne sous forme d'objet
        public static async Task<T?> WaitForResponse<T>()
        {
            string? jsonResponse = await GetNextMessage();
            if (jsonResponse == null) return default;

            try
            {
                Console.WriteLine($"📩 Réponse reçue : {jsonResponse}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                return JsonSerializer.Deserialize<T>(jsonResponse, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur de parsing JSON : {ex.Message}");
                return default;
            }
        }
    }
}