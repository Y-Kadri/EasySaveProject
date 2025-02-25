using System;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave_Project.Server;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Layout;
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
        
        private string _connexion;
        public string connexion
        {
            get => _connexion;
            private set => this.RaiseAndSetIfChanged(ref _connexion, value);
        }
        
        private string _entrerVotreNom;
        public string entrerVotreNom
        {
            get => _entrerVotreNom;
            private set => this.RaiseAndSetIfChanged(ref _entrerVotreNom, value);
        }
        
        private string _vousetesconnecte;
        public string vousetesconnecte
        {
            get => _vousetesconnecte;
            private set => this.RaiseAndSetIfChanged(ref _vousetesconnecte, value);
        }
        
        private string _seconnecter;
        public string seconnecter
        {
            get => _seconnecter;
            private set => this.RaiseAndSetIfChanged(ref _seconnecter, value);
        }
        
        private string _sedeconnecter;
        public string sedeconnecter
        {
            get => _sedeconnecter;
            private set => this.RaiseAndSetIfChanged(ref _sedeconnecter, value);
        }


        public ConnexionViewModel()
        {
            Refresh();
        }

        public void Refresh()
        {
            IsConnected = GlobalDataService.GetInstance().isConnecte;
            BaseLayoutViewModel.RefreshInstance(null, null);
            connexion = TranslationService.GetInstance().GetText("Connexion");
            entrerVotreNom = TranslationService.GetInstance().GetText("EntrerVotreNom");
            vousetesconnecte = $"\u2705 {TranslationService.GetInstance().GetText("Vousêtesconnecté")} !";
            seconnecter = TranslationService.GetInstance().GetText("Seconnecter");
            sedeconnecter = TranslationService.GetInstance().GetText("Sedeconnecter");
        }
        
        public async Task Connexion(string name)
        {
            try
            {
                var globalService = GlobalDataService.GetInstance();
                globalService.client = new Client(name);
                if (globalService.client.client != null)
                {
                    globalService.client.Start();
                    globalService.isConnecte = true;
                    IsConnected = true;
                    Refresh();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur de connexion : {ex.Message}");
                // Gérer l'erreur (par exemple, notifier l'utilisateur via une notification)
            }
        }


        public async void GetAllUserConnect()
        {
            try
            {
                Utils.SendToServer("GET_USERS");
                Users = await Utils.WaitForResponse<ObservableCollection<User>>() ?? new ObservableCollection<User>();
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
            Refresh();
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
            
            Refresh();
        }
    }
}
