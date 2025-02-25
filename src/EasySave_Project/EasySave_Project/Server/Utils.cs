using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EasySave_Project.Dto;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Layout;

namespace EasySave_Project.Server
{
    public static class Utils
    {
        private static readonly JobService _jobService = new JobService();
        
        private static readonly TranslationService _translationService = TranslationService.GetInstance();
        
        private static readonly ConcurrentQueue<string> messageQueue = new();

        /// <summary>
        /// Starts listening for incoming messages from the server through the provided network stream.
        /// Processes received messages, handles specific commands, and enqueues unknown messages.
        /// </summary>
        /// <param name="stream">The network stream used for reading incoming messages.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
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
                            
                            if (jobCommand.command == "RUN_JOB_USERS" && !string.IsNullOrEmpty(jobCommand.id))
                            {
                                BaseLayoutViewModel.Instance.AddNotification($"{_translationService.GetText("UtilisateurLanceJobs")} .");
                                ExecuteJobs(jobCommand.obj);
                            }
                            
                            continue;
                        }
                    }
                    catch (JsonException)
                    {
                    }

                    messageQueue.Enqueue(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"⚠️ Erreur de connexion : {e.Message}");
            }
        }

        /// <summary>
        /// Executes a list of job commands by running each job in a separate thread.
        /// Sends a confirmation message to the server when execution begins.
        /// </summary>
        /// <param name="jobCommand">A list of JobModel instances representing jobs to be executed.</param>
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

        /// <summary>
        /// Sends a message to the server via the provided or default network stream.
        /// Ensures the stream is writable before attempting to send data.
        /// </summary>
        /// <param name="message">The message to send to the server.</param>
        /// <param name="stream">An optional network stream; if null, the global client stream is used.</param>
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

        /// <summary>
        /// Retrieves the next message from the message queue asynchronously.
        /// Waits until a message is available before returning.
        /// </summary>
        /// <returns>The next message from the queue, or null if no message is retrieved.</returns>
        private static async Task<string?> GetNextMessage()
        {
            while (messageQueue.IsEmpty)
            {
                await Task.Delay(100);
            }

            if (messageQueue.TryDequeue(out string message))
            {
                return message;
            }

            return null;
        }

        /// <summary>
        /// Waits for and retrieves a response message from the server, deserializing it into the specified type.
        /// Handles JSON parsing errors gracefully.
        /// </summary>
        /// <typeparam name="T">The expected type of the response object.</typeparam>
        /// <returns>The deserialized response object, or default if parsing fails.</returns>
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