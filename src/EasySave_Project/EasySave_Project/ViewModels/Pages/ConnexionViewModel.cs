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

    public async void GetAllUserConnect()
    {
        try
        {
            Utils.SendToServer("GET_USERS");
            users = Utils.ReceiveFromServer<ObservableCollection<User>>();
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
        Utils.SendToServer(jsonString);
    }

    public void DisconnexionTo()
    {
        
        var requestData = new
        {
            command = "DISCONNECTE_USERS",
            id = GlobalDataService.GetInstance().connecteTo.Item1
        };
        string jsonString = JsonSerializer.Serialize(requestData);
        Utils.SendToServer(jsonString);
        GlobalDataService.GetInstance().connecteTo = (null, null);
    }
}