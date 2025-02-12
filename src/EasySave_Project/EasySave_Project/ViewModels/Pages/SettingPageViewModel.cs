using System;
using Avalonia.Controls;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Util;
using EasySave_Project.Views.Components;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages
{
    public class SettingPageViewModel : ReactiveObject
    {
        private readonly TranslationService _translationService;

        private string _message;
        private string _status;

        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public string SelectLanguage { get; private set; }
        public string French { get; private set; }
        public string English { get; private set; }
        public string ChooseLogsFormat { get; private set; }
        public string Json { get; private set; }
        public string Xml { get; private set; }

        // Constructeur qui prend un callback pour notifier la vue
        public SettingPageViewModel()
        {
            _translationService = TranslationService.GetInstance();
            
            SelectLanguage = _translationService.GetText("SelectLanguage");
            French = _translationService.GetText("French");
            English = _translationService.GetText("English");
            ChooseLogsFormat = _translationService.GetText("ChooseLogsFormat");
            Json = _translationService.GetText("Json");
            Xml = _translationService.GetText("Xml");
        }

        // Méthode pour changer la langue et appeler la notification
        public  (string message, string status) ChangeLanguage(LanguageEnum lang)
        {
            if (FileUtil.SettingChangeLanguage(lang))
            {
                TranslationService.SetLanguage(lang);
                _message = _translationService.GetText("LanguageChangeSuccess");
                _status = "Success";
            }
            else
            {
                _message = _translationService.GetText("LanguageChangeError");
                _status = "Error";
            }

            // Appeler le callback pour afficher la notification dans la vue
            return (_message, _status);
        }
    }
}
