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
        private readonly GlobalDataService _globalDataService = GlobalDataService.GetInstance();
        
        private readonly TranslationService _translationService = TranslationService.GetInstance();
        
        private bool _isConnected;
        
        private ObservableCollection<User> _users = new();
        
        private string _connexion, _entrerVotreNom, _vousetesconnecte, 
            _seconnecter, _sedeconnecter;
        
        public bool IsConnected
        {
            get => _isConnected;
            private set => this.RaiseAndSetIfChanged(ref _isConnected, value);
        }
       
        public ObservableCollection<User> Users
        {
            get => _users;
            private set => this.RaiseAndSetIfChanged(ref _users, value);
        }
        
        public string connexion
        {
            get => _connexion;
            private set => this.RaiseAndSetIfChanged(ref _connexion, value);
        }
        
        public string entrerVotreNom
        {
            get => _entrerVotreNom;
            private set => this.RaiseAndSetIfChanged(ref _entrerVotreNom, value);
        }
        
        public string vousetesconnecte
        {
            get => _vousetesconnecte;
            private set => this.RaiseAndSetIfChanged(ref _vousetesconnecte, value);
        }
        
        public string seconnecter
        {
            get => _seconnecter;
            private set => this.RaiseAndSetIfChanged(ref _seconnecter, value);
        }
        
        public string sedeconnecter
        {
            get => _sedeconnecter;
            private set => this.RaiseAndSetIfChanged(ref _sedeconnecter, value);
        }


        public ConnexionViewModel()
        {
            Refresh();
        }

        /// <summary>
        /// Refreshes the connection status and updates UI text translations.
        /// </summary>
        public void Refresh()
        {
            IsConnected = _globalDataService.isConnecte;
            BaseLayoutViewModel.RefreshInstance(null, null);
            connexion = _translationService.GetText("Connexion");
            entrerVotreNom = _translationService.GetText("EntrerVotreNom");
            vousetesconnecte = $"\u2705 {_translationService.GetText("Vousêtesconnecté")} !";
            seconnecter = _translationService.GetText("Seconnecter");
            sedeconnecter = _translationService.GetText("Sedeconnecter");
        }
        
        /// <summary>
        /// Establishes a connection with the server using the provided username.
        /// </summary>
        /// <param name="name">The username for the connection.</param>
        public async Task Connexion(string name)
        {
            try
            {
                
                _globalDataService.client = new Client(name);
                if (_globalDataService.client.client != null)
                {
                    _globalDataService.isConnecte = true;
                    IsConnected = true;
                    Refresh();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Erreur de connexion : {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the list of all connected users from the server.
        /// </summary>
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

        /// <summary>
        /// Connects to a specific user by sending a request to the server.
        /// </summary>
        /// <param name="id">The ID of the user to connect to.</param>
        /// <param name="name">The name of the user to connect to.</param>
        public void ConnexionTo(string id, string name)
        {
            _globalDataService.connecteTo = (id, name);
            var requestData = new { command = "CONNECTE_USERS", id };
            string jsonString = JsonSerializer.Serialize(requestData);
            Utils.SendToServer(jsonString);
            Refresh();
        }

        /// <summary>
        /// Disconnects from the currently connected user and updates the UI.
        /// </summary>
        public void DisconnexionTo()
        {
            if (_globalDataService.connecteTo.Item1 != null)
            {
                var requestData = new { command = "DISCONNECTE_USERS", id = _globalDataService.connecteTo.Item1 };
                string jsonString = JsonSerializer.Serialize(requestData);
                Utils.SendToServer(jsonString);
                _globalDataService.connecteTo = (null, null);
            }
            Refresh();
        }
    }
}
