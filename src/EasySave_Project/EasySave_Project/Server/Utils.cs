using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave_Project.Manager;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Layout;

namespace EasySave_Project.Server;

public class Utils
{
    public static void ReceiveMessages(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // 🔴 Si la connexion est fermée, on arrête la boucle
                if (bytesRead == 0)
                {
                    Console.WriteLine("🔴 Connexion fermée par le serveur.");
                    break;
                }

                // ✅ Convertit les données lues en string
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"\n[Server]: {message}");

                switch (message)
                {
                    case "NEW_USER":
                        message = "Un nouveau utilisateur est connecté";
                        break;
                    case "GET_JOBS":
                        message = null;
                        //envoi mes jobs au serveur
                        var jobs = JobManager.GetInstance().Jobs;

                        // Options pour une sérialisation plus lisible (indente le JSON)
                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true, // Pour un JSON bien formaté
                            PropertyNameCaseInsensitive = true
                        };

                        string jsonJobs = JsonSerializer.Serialize(jobs, options);
                        SendToServer(jsonJobs);
                        break;
                }
                
                // 🔥 Ajout du message dans les notifications du BaseLayoutViewModel
                if (message != null)
                    BaseLayoutViewModel.Instance.AddNotification(message);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"⚠️ Erreur de connexion : {e.Message}");
        }
    }


    public static void SendToServer(string message, NetworkStream stream = null)
    {
        try
        {
            stream ??= GlobalDataService.GetInstance().client.stream;

            if (stream == null || !stream.CanWrite)
            {
                Console.WriteLine("❌ Erreur : Le stream est invalide ou non accessible en écriture.");
                return;
            }

            // Convertir le message en bytes
            byte[] data = Encoding.UTF8.GetBytes(message);

            // Envoyer les données au serveur
            stream.Write(data, 0, data.Length);
            stream.Flush(); // Assurer l'envoi immédiat

            Console.WriteLine($"📤 Message envoyé : {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erreur lors de l'envoi au serveur : {ex.Message}");
        }
    }
    
    public static T? ReceiveFromServer<T>()
    {
        try
        {
            NetworkStream stream = GlobalDataService.GetInstance().client.stream;

            if (stream == null || !stream.CanRead)
            {
                Console.WriteLine("❌ Erreur : Le stream est invalide ou non accessible en lecture.");
                return default;
            }

            // 🔥 Lire la réponse du serveur
            byte[] buffer = new byte[4096]; // Augmenter la taille si nécessaire
            
            
            int bytesRead  = stream.Read(buffer, 0, buffer.Length);
            string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            Console.WriteLine($"📩 Réponse reçue : {jsonResponse}");

            // 🔥 Désérialisation générique
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
            Console.WriteLine($"⚠️ Erreur lors de la réception : {ex.Message}");
            return default;
        }
    }
}