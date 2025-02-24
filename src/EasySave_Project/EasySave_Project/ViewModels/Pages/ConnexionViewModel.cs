using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using EasySave_Project.Server;
using EasySave_Project.Service;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages
{
    public class ConnexionViewModel : ReactiveObject
    {
        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            private set => this.RaiseAndSetIfChanged(ref _isConnected, value);
        }

        private ObservableCollection<User> _users = new();
        public ObservableCollection<User> Users
        {
            get => _users;
            private set => this.RaiseAndSetIfChanged(ref _users, value);
        }

        public ConnexionViewModel()
        {
            IsConnected = GlobalDataService.GetInstance().isConnecte;
        }

        public void Connexion(string name)
        {
            var globalService = GlobalDataService.GetInstance();
            globalService.client = new Client(name);
            globalService.client.Start();
            globalService.isConnecte = true;
            
            IsConnected = true; // 🔥 ReactiveUI mettra à jour l'UI automatiquement
        }

        public void GetAllUserConnect()
        {
            try
            {
                Utils.SendToServer("GET_USERS");
                Users = Utils.ReceiveFromServer<ObservableCollection<User>>() ?? new ObservableCollection<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur lors de la récupération des utilisateurs : {ex.Message}");
                Users.Clear();
            }
        }

        public void ConnexionTo(string id, string name)
        {
            GlobalDataService.GetInstance().connecteTo = (id, name);

            var requestData = new { command = "CONNECTE_USERS", id };
            string jsonString = JsonSerializer.Serialize(requestData);
            Utils.SendToServer(jsonString);
        }

        public void DisconnexionTo()
        {
            var globalService = GlobalDataService.GetInstance();
            if (globalService.connecteTo.Item1 != null)
            {
                var requestData = new { command = "DISCONNECTE_USERS", id = globalService.connecteTo.Item1 };
                string jsonString = JsonSerializer.Serialize(requestData);
                Utils.SendToServer(jsonString);
                
                globalService.connecteTo = (null, null);
            }
        }
    }
}
