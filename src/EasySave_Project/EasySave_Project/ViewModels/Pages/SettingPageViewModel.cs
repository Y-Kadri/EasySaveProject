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
        public string Add { get; private set; }
        public string FileExtensionsToEncrypt { get; private set; }
        public string MonitoredBusinessSoftware { get; private set; }
        private int _maxLargeFileSize;
        public int MaxLargeFileSize
        {
            get => _maxLargeFileSize;
            set => this.RaiseAndSetIfChanged(ref _maxLargeFileSize, value);
        }
        public string MaxLargeFileSizeText { get; private set; }

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
            Add = _translationService.GetText("Add");
            FileExtensionsToEncrypt = _translationService.GetText("FileExtensionsToEncrypt");
            MonitoredBusinessSoftware = _translationService.GetText("MonitoredBusinessSoftware");
            MaxLargeFileSizeText = _translationService.GetText("MaxLargeFileSize");

            EncryptedFileExtensions = new ObservableCollection<string>(SettingUtil.GetList("EncryptedFileExtensions"));
            PriorityBusinessProcess = new ObservableCollection<string>(SettingUtil.GetList("PriorityBusinessProcess"));
            MaxLargeFileSize = FileUtil.GetAppSettingsInt("MaxLargeFileSize");
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

        /// <summary>
        /// Updates the maximum large file size setting if the new value is different.
        /// </summary>
        /// <param name="value">The new maximum large file size in bytes.</param>
        /// <returns>
        /// A tuple containing a message and status:
        /// - message: Success or error message.
        /// - status: "Success" if the update was successful, otherwise "Error".
        /// </returns>
        public (string message, string status) ChangeMaxLargeFileSize(int value)
        {
            // Check if the new value is different from the current one and update it in the settings
            if (MaxLargeFileSize != value && SettingUtil.SettingChangeMaxLargeFileSize(value))
            {
                // Update the MaxLargeFileSize variable
                MaxLargeFileSize = value;
                // Notify the UI that the property has changed
                this.RaisePropertyChanged(nameof(MaxLargeFileSize));
                // Set the success message and status
                _message = _translationService.GetText("AddMaxLargeFileSizeToSettings");
                _status = "Success";
            }
            else
            {
                // Set the error message and status if the update fails
                _message = _translationService.GetText("ErrorAddMaxLargeFileSizeToSettings");
                _status = "Error";
            }

            // Return the message and status to trigger a notification in the view
            return (_message, _status);
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
