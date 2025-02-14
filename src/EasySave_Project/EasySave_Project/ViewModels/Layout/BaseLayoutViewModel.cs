using System;
using System.Globalization;
using ReactiveUI;
using System.Reactive;
using EasySave_Project.Model;
using EasySave_Project.Service;

namespace EasySave_Project.ViewModels.Layout
{
    public class BaseLayoutViewModel : ReactiveObject
    {
        private string _currentDate = string.Empty;
        
        public string Home { get; private set; }
        public string Jobs { get; private set; }
        public string Logs { get; private set; }
        public string Settings { get; private set; }

        public string CurrentDate
        {
            get => _currentDate;
            set => this.RaiseAndSetIfChanged(ref _currentDate, value);
        }

        public BaseLayoutViewModel()
        {
            
            if (TranslationService.Language == LanguageEnum.FR)
            {
                CurrentDate = DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("fr-FR"));
            }
            else
            {
                CurrentDate = DateTime.Now.ToString("MMMM dd yyyy", new CultureInfo("en-US"));
            }

            
            
            Home = TranslationService.GetInstance().GetText("Home");
            Jobs = TranslationService.GetInstance().GetText("Jobs");
            Logs = TranslationService.GetInstance().GetText("Logs");
            Settings = TranslationService.GetInstance().GetText("Settings");

        }
    }
}