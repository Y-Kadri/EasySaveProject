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
        private readonly TranslationService _translationService = TranslationService.GetInstance();
        
        private readonly GlobalDataService _globalData = GlobalDataService.GetInstance();
        
        private static BaseLayoutViewModel _instance = new BaseLayoutViewModel();
        
        public static BaseLayoutViewModel Instance => _instance;
        
        private bool _isLogsVisible = true;
        
        private StackPanel _notificationContainer;
        
        private string _currentDate, _home, _jobs, _logs, _settings, _conecte;
        
        public StackPanel NotificationContainer
        {
            get => _notificationContainer;
            set => this.RaiseAndSetIfChanged(ref _notificationContainer, value);
        }
        
        public bool IsLogsVisible
        {
            get => _isLogsVisible;
            set => this.RaiseAndSetIfChanged(ref _isLogsVisible, value);
        }

        public string CurrentDate
        {
            get => _currentDate;
            set => this.RaiseAndSetIfChanged(ref _currentDate, value);
        }

        public string Home
        {
            get => _home;
            set => this.RaiseAndSetIfChanged(ref _home, value);
        }

        public string Jobs
        {
            get => _jobs;
            set => this.RaiseAndSetIfChanged(ref _jobs, value);
        }

        public string Logs
        {
            get => _logs;
            set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        public string Settings
        {
            get => _settings;
            set => this.RaiseAndSetIfChanged(ref _settings, value);
        }

        public string Conecte
        {
            get => _conecte;
            set => this.RaiseAndSetIfChanged(ref _conecte, value);
        }

        public BaseLayoutViewModel()
        {
            UpdateValues(null, null);
        }

        /// <summary>
        /// Updates the UI elements based on the current language and connection status.
        /// </summary>
        /// <param name="logsButton">The button associated with logs, which may be hidden or shown based on connection status.</param>
        /// <param name="SettingButton">The button associated with settings, which may be hidden or shown based on connection status.</param>
        public void UpdateValues(Button? logsButton, Button? SettingButton)
        {
            CurrentDate = TranslationService.Language == LanguageEnum.FR 
                ? DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("fr-FR")) 
                : DateTime.Now.ToString("MMMM dd yyyy", new CultureInfo("en-US"));

            
            Home = _translationService.GetText("Home");
            Jobs = _translationService.GetText("Jobs");
            Logs = _translationService.GetText("Logs");
            Settings = _translationService.GetText("Settings");

            
            Conecte = _globalData.isConnecte 
                ? $"{_translationService.GetText("Connecté")} {(_globalData.connecteTo.Item2 != null ? $" {_translationService.GetText("à")} " + _globalData.connecteTo.Item2 : "")}" 
                : $"{_translationService.GetText("Nonconnecté")}";

            if (logsButton != null && SettingButton != null)
            {
                if (_globalData.isConnecte && _globalData.connecteTo.Item2 != null)
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

        /// <summary>
        /// Refreshes the instance of the UI by updating values related to buttons and translations.
        /// </summary>
        /// <param name="logsButton">The button associated with logs.</param>
        /// <param name="SettingButton">The button associated with settings.</param>
        public static void RefreshInstance( Button? logsButton, Button? SettingButton)
        {
            _instance.UpdateValues(logsButton, SettingButton);
        }
        
        /// <summary>
        /// Adds a notification message, translating it if necessary.
        /// </summary>
        /// <param name="message">The notification message key to be translated and displayed.</param>
        public void AddNotification(string message)
        {
            string newmessage = _translationService.GetText(message);
            if (newmessage == message)
            {
                newmessage = _translationService.Replace(message);
            }
            Toastr.ShowServeurNotification(newmessage, NotificationContainer);
        }
    }
}
