using System.Collections.ObjectModel;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Util;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages
{
    public class SettingPageViewModel : ReactiveObject
    {
        private readonly TranslationService _translationService;
        
        public ObservableCollection<string> EncryptedFileExtensions { get; }
        public ObservableCollection<string> PriorityBusinessProcess { get; }

        private string _message, _status, _selectLanguage, _french, _english,
            _chooseLogsFormat, _Json, _Xml, _add, _fileExtensionsToEncrypt, _monitoredBusinessSoftware;

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
        
        public string SelectLanguage
        {
            get => _selectLanguage;
            set => this.RaiseAndSetIfChanged(ref _selectLanguage, value);
        }
        
        public string French
        {
            get => _french;
            set => this.RaiseAndSetIfChanged(ref _french, value);
        }
        
        public string English
        {
            get => _english;
            set => this.RaiseAndSetIfChanged(ref _english, value);
        }
        
        public string ChooseLogsFormat
        {
            get => _chooseLogsFormat;
            set => this.RaiseAndSetIfChanged(ref _chooseLogsFormat, value);
        }
        
        public string Json
        {
            get => _Json;
            set => this.RaiseAndSetIfChanged(ref _Json, value);
        }
        
        public string Xml
        {
            get => _Xml;
            set => this.RaiseAndSetIfChanged(ref _Xml, value);
        }
        
        public string Add
        {
            get => _add;
            set => this.RaiseAndSetIfChanged(ref _add, value);
        }
        
        public string FileExtensionsToEncrypt
        {
            get => _fileExtensionsToEncrypt;
            set => this.RaiseAndSetIfChanged(ref _fileExtensionsToEncrypt, value);
        }
        
        public string MonitoredBusinessSoftware
        {
            get => _monitoredBusinessSoftware;
            set => this.RaiseAndSetIfChanged(ref _monitoredBusinessSoftware, value);
        }
        
        // Constructeur qui prend un callback pour notifier la vue
        public SettingPageViewModel()
        {
            _translationService = TranslationService.GetInstance();
            EncryptedFileExtensions = new ObservableCollection<string>(SettingUtil.GetList("EncryptedFileExtensions"));
            PriorityBusinessProcess = new ObservableCollection<string>(SettingUtil.GetList("PriorityBusinessProcess"));
        }

        public void Refresh()
        {
            SelectLanguage = _translationService.GetText("SelectLanguage");
            French = _translationService.GetText("French");
            English = _translationService.GetText("English");
            ChooseLogsFormat = _translationService.GetText("ChooseLogsFormat");
            Json = _translationService.GetText("Json");
            Xml = _translationService.GetText("Xml");
            Add = _translationService.GetText("Add");
            FileExtensionsToEncrypt = _translationService.GetText("FileExtensionsToEncrypt");
            MonitoredBusinessSoftware = _translationService.GetText("MonitoredBusinessSoftware");
        }
        
        public void AddEncryptedFileExtensions(string extension)
        {
            if (SettingUtil.AddToList("EncryptedFileExtensions", extension))
            {
                EncryptedFileExtensions.Add(extension);
                this.RaisePropertyChanged(nameof(EncryptedFileExtensions));
                Message = "Extension ajoutée avec succès.";
            }
            else
            {
                Message = "Erreur lors de l'ajout.";
            }
        }
        
        public void AddPriorityBusinessProcess(string software)
        {
            if (SettingUtil.AddToList("PriorityBusinessProcess", software))
            {
                PriorityBusinessProcess.Add(software);
                this.RaisePropertyChanged(nameof(PriorityBusinessProcess));
                Message = "Logiciel ajouté avec succès.";
            }
            else
            {
                Message = "Erreur lors de l'ajout.";
            }
        }

        public void RemovPriorityBusinessProcess(string software)
        {
            if (SettingUtil.RemoveFromList("PriorityBusinessProcess", software))
            {
                PriorityBusinessProcess.Remove(software);
                this.RaisePropertyChanged(nameof(PriorityBusinessProcess));
                Message = "Logiciel supprimé avec succès.";
            }
            else
            {
                Message = "Erreur lors de la suppression.";
            }
        }

        public void RemoveEncryptedFileExtensions(string extension)
        {
            if (SettingUtil.RemoveFromList("EncryptedFileExtensions", extension))
            {
                EncryptedFileExtensions.Remove(extension);
                this.RaisePropertyChanged(nameof(EncryptedFileExtensions));
                Message = "Extension supprimée avec succès.";
            }
            else
            {
                Message = "Erreur lors de la suppression.";
            }
        }

        // Méthode pour changer la langue et appeler la notification
        public  (string message, string status) ChangeLanguage(LanguageEnum lang)
        {
            if (SettingUtil.SettingChangeLanguage(lang))
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

        public (string message, string status) ChangeLogsFormat(LogFormatManager.LogFormat logsFormat)
        {
            if (SettingUtil.SettingChangeFormat(logsFormat))
            {
                _message = _translationService.GetText("LogsFormatChangeSuccess");
                _status = "Success";
            }
            else
            {
                _message = _translationService.GetText("LogsFormatChangeError");
                _status = "Error";
            }
            
            LogFormatManager.Instance.SetLogFormat(logsFormat);
            return (_message, _status);
        }
    }
}
