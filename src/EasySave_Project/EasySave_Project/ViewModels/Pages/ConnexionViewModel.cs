using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using EasySave_Project.Server;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Layout;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages;

public class ConnexionViewModel : ReactiveObject
{
    public event PropertyChangedEventHandler? PropertyChanged;
        
    public bool IsConnected => GlobalDataService.GetInstance().isConnecte;
    
    public ObservableCollection<User> users { get; set; } = new ObservableCollection<User>();
    
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

    public void GetAllUserConnect()
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
            users = JsonSerializer.Deserialize<ObservableCollection<User>>(jsonResponse);
        }
        catch (Exception ex)
        {
            //PROBLEME ICI
            Console.WriteLine($"⚠️ Erreur lors de la récupération des utilisateurs <ConnexionViewModel> : {ex.Message}");
            users.Clear();
        }
    }

    public void ConnexionTo(string id, string name)
    {
        GlobalDataService.GetInstance().connecteTo = (id, name);

        // Création de l'objet JSON
        var requestData = new
        {
            command = "CONNECTE_USERS",
            id = id
        };

        // Sérialisation en JSON
        string jsonString = JsonSerializer.Serialize(requestData);
        byte[] buffer = Encoding.UTF8.GetBytes(jsonString);

        // Envoi des données au serveur
        GlobalDataService.GetInstance().client.stream.Write(buffer, 0, buffer.Length);
    }

    public void DisconnexionTo()
    {
        
        var requestData = new
        {
            command = "DISCONNECTE_USERS",
            id = GlobalDataService.GetInstance().connecteTo.Item1
        };
        string jsonString = JsonSerializer.Serialize(requestData);
        byte[] buffer = Encoding.UTF8.GetBytes(jsonString);

        // Envoi des données au serveur
        GlobalDataService.GetInstance().client.stream.Write(buffer, 0, buffer.Length);
        GlobalDataService.GetInstance().connecteTo = (null, null);
    }
}