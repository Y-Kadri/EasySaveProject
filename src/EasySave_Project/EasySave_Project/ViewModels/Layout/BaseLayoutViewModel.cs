using System;
using System.Globalization;
using Avalonia.Controls;
using ReactiveUI;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Views.Components;

namespace EasySave_Project.ViewModels.Layout
{
    public class BaseLayoutViewModel : ReactiveObject
    {
        private static BaseLayoutViewModel _instance;

        public StackPanel NotificationContainer { get; set; }
        
        public static BaseLayoutViewModel GetInstance()
        {
            return _instance ??= new BaseLayoutViewModel();
        }

        private string _currentDate = string.Empty;
        public string CurrentDate
        {
            get => _currentDate;
            set => this.RaiseAndSetIfChanged(ref _currentDate, value);
        }
        
        public static void NewInstance()
        {
            _instance = new BaseLayoutViewModel();
        }

        public string Home { get; private set; }
        public string Jobs { get; private set; }
        public string Logs { get; private set; }
        public string Settings { get; private set; }
        public string Conecte { get; set; }

        public BaseLayoutViewModel()
        {
            // 📆 Format de la date en fonction de la langue
            if (TranslationService.Language == LanguageEnum.FR)
            {
                CurrentDate = DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("fr-FR"));
            }
            else
            {
                CurrentDate = DateTime.Now.ToString("MMMM dd yyyy", new CultureInfo("en-US"));
            }

            // 🌍 Récupération des traductions
            Home = TranslationService.GetInstance().GetText("Home");
            Jobs = TranslationService.GetInstance().GetText("Jobs");
            Logs = TranslationService.GetInstance().GetText("Logs");
            Settings = TranslationService.GetInstance().GetText("Settings");

            // ✅ Statut de connexion
            Conecte = GlobalDataService.GetInstance().isConnecte ? "Connecté" : "Non connecté";

            if (GlobalDataService.GetInstance().isConnecte && GlobalDataService.GetInstance().connecteTo.Item2 != null)
            {
                Conecte = $"Connecté à {GlobalDataService.GetInstance().connecteTo.Item2}";
            }
        }

        // ✅ Ajout d'une notification
        public void AddNotification(string message)
        {
            Toastr.ShowServeurNotification(message, NotificationContainer);
        }
    }
}
