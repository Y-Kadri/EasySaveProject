using System;
using System.Globalization;
using Avalonia.Controls;
using EasySave_Project.Model;
using ReactiveUI;
using EasySave_Project.Service;
using EasySave_Project.Views.Components;

namespace EasySave_Project.ViewModels.Layout
{
    public class BaseLayoutViewModel : ReactiveObject
    {
        private static BaseLayoutViewModel _instance = new BaseLayoutViewModel();
        
        public static BaseLayoutViewModel Instance => _instance;

        private StackPanel _notificationContainer;
        public StackPanel NotificationContainer
        {
            get => _notificationContainer;
            set => this.RaiseAndSetIfChanged(ref _notificationContainer, value);
        }
        
        private bool _isLogsVisible = true; // Visible par défaut
        public bool IsLogsVisible
        {
            get => _isLogsVisible;
            set => this.RaiseAndSetIfChanged(ref _isLogsVisible, value);
        }


        private string _currentDate;
        public string CurrentDate
        {
            get => _currentDate;
            set => this.RaiseAndSetIfChanged(ref _currentDate, value);
        }

        private string _home;
        public string Home
        {
            get => _home;
            set => this.RaiseAndSetIfChanged(ref _home, value);
        }

        private string _jobs;
        public string Jobs
        {
            get => _jobs;
            set => this.RaiseAndSetIfChanged(ref _jobs, value);
        }

        private string _logs;
        public string Logs
        {
            get => _logs;
            set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        private string _settings;
        public string Settings
        {
            get => _settings;
            set => this.RaiseAndSetIfChanged(ref _settings, value);
        }

        private string _conecte;
        public string Conecte
        {
            get => _conecte;
            set => this.RaiseAndSetIfChanged(ref _conecte, value);
        }

        public BaseLayoutViewModel()
        {
            UpdateValues(null, null);
        }

        public void UpdateValues(Button? logsButton, Button? SettingButton)
        {
            CurrentDate = TranslationService.Language == LanguageEnum.FR 
                ? DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("fr-FR")) 
                : DateTime.Now.ToString("MMMM dd yyyy", new CultureInfo("en-US"));

            var translationService = TranslationService.GetInstance();
            Home = translationService.GetText("Home");
            Jobs = translationService.GetText("Jobs");
            Logs = translationService.GetText("Logs");
            Settings = translationService.GetText("Settings");

            var globalData = GlobalDataService.GetInstance();
            Conecte = globalData.isConnecte 
                ? $"Connecté{(globalData.connecteTo.Item2 != null ? " à " + globalData.connecteTo.Item2 : "")}" 
                : "Non connecté";

            if (logsButton != null && SettingButton != null)
            {
                if (globalData.isConnecte && globalData.connecteTo.Item2 != null)
                {
                    SettingButton.IsVisible = false;
                    logsButton.IsVisible = false;
                }
                else
                {
                    SettingButton.IsVisible = true;
                    logsButton.IsVisible = true;
                }
            }
            
        }

        public static void RefreshInstance( Button? logsButton, Button? SettingButton)
        {
            _instance.UpdateValues(logsButton, SettingButton); // 🔥 Met à jour sans recréer une instance
        }
        
        public void AddNotification(string message)
        {
            Toastr.ShowServeurNotification(message, NotificationContainer);
        }
    }
}
