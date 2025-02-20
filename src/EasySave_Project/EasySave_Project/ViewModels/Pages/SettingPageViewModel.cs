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
        public ObservableCollection<string> PriorityExtension { get; }

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

            EncryptedFileExtensions = new ObservableCollection<string>(SettingUtil.GetList("EncryptedFileExtensions"));
            PriorityBusinessProcess = new ObservableCollection<string>(SettingUtil.GetList("PriorityBusinessProcess"));
            PriorityExtension = new ObservableCollection<string>(SettingUtil.GetList("PriorityBusinessProcess"));

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
        public (string message, string status) ChangeLanguage(LanguageEnum lang)
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

        /// <summary>
        /// Moves the file extension up in the list if it is not already at the top.
        /// </summary>
        /// <param name="index">The index of the item to move.</param>
        public void MoveExtensionUp(int index)
        {
            if (index > 0) // Check if the item is not the first one in the list
            {
                var item = EncryptedFileExtensions[index]; // Get the item at the current index
                EncryptedFileExtensions.RemoveAt(index); // Remove it from the current position
                EncryptedFileExtensions.Insert(index - 1, item); // Insert it one position above
                this.RaisePropertyChanged(nameof(EncryptedFileExtensions)); // Notify the UI to update
                Message = "Extension moved up.";

                SettingUtil.
            }
            else
            {
                Message = "Cannot move up further."; // Error message if it is already at the top
            }
        }

        /// <summary>
        /// Moves the file extension down in the list if it is not already at the bottom.
        /// </summary>
        /// <param name="index">The index of the item to move.</param>
        public void MoveExtensionDown(int index)
        {
            if (index < EncryptedFileExtensions.Count - 1) // Check if the item is not the last one in the list
            {
                var item = EncryptedFileExtensions[index]; // Get the item at the current index
                EncryptedFileExtensions.RemoveAt(index); // Remove it from the current position
                EncryptedFileExtensions.Insert(index + 1, item); // Insert it one position below
                this.RaisePropertyChanged(nameof(EncryptedFileExtensions)); // Notify the UI to update
                Message = "Extension moved down.";
            }
            else
            {
                Message = "Cannot move down further."; // Error message if it is already at the bottom
            }
        }

    }
}
