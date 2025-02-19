using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using EasySave_Project.Server;
using EasySave_Project.Service;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages;

public class ConnexionViewModel : ReactiveObject
{
    public event PropertyChangedEventHandler? PropertyChanged;
        
    public bool IsConnected => GlobalDataService.GetInstance().isConnecte;
    
    public ConnexionViewModel()
    {
    }

    public void Connexion(string name)
    {
        GlobalDataService.GetInstance().client = new Client(name);
        GlobalDataService.GetInstance().client.Start();
        GlobalDataService.GetInstance().isConnecte = true;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
    }

    public List<User> GetAllUserConnect()
    {
        try
        {
            // 🔥 Envoyer la requête pour obtenir la liste des utilisateurs
            byte[] requestData = Encoding.UTF8.GetBytes("GET_USERS");
            GlobalDataService.GetInstance().client.stream.Write(requestData, 0, requestData.Length);

            // 🔥 Lire la réponse du serveur
            byte[] buffer = new byte[4096]; // Augmenter la taille si nécessaire
            int bytesRead = GlobalDataService.GetInstance().client.stream.Read(buffer, 0, buffer.Length);
            string jsonResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // 🔥 Désérialiser le JSON en liste d'objets `User`
            return JsonSerializer.Deserialize<List<User>>(jsonResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erreur lors de la récupération des utilisateurs : {ex.Message}");
            return new List<User>(); // Retourne une liste vide en cas d'erreur
        }
    }

}