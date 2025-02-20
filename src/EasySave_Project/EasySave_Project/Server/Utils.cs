using System;
using System.Net.Sockets;
using System.Text;
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
                }
                
                // 🔥 Ajout du message dans les notifications du BaseLayoutViewModel
                BaseLayoutViewModel.GetInstance().AddNotification(message);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"⚠️ Erreur de connexion : {e.Message}");
        }
    }
}