using System.Collections.ObjectModel;
using System.Linq;
using EasySave_Library_Log.manager;
using EasySave_Project.Dto;
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
        public ObservableCollection<PriorityExtensionDTO> PriorityExtensionFiles { get; }

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
            PriorityExtensionFiles = new ObservableCollection<PriorityExtensionDTO>(SettingUtil.GetPriorityExtensionFilesList("PriorityExtensionFiles"));
            SortPriorityExtensions();
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

        public void AddPriorityFileExtensions(string extensionfile)
        {
            if (SettingUtil.AddPriorityExtension(extensionfile))
            {
                this.RaisePropertyChanged(nameof(PriorityExtensionFiles));
                Message = "Priorité ajouté avec succès.";
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

        public void RemovePriorityFileExtensions(PriorityExtensionDTO priority)
        {
            if (SettingUtil.RemovePriorityExtension(priority.ExtensionFile))
            {
                PriorityExtensionFiles.Remove(priority);
                this.RaisePropertyChanged(nameof(PriorityExtensionFiles));
                Message = "Priorité supprimé avec succès.";
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
                SettingUtil.MovePriorityExtensionFileUp(index);

                // Sort the list after moving the item
                SortPriorityExtensions();  // Call the method to sort the list

                Message = "Extension moved up.";
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
            if (index <= PriorityExtensionFiles.Count) // Check if the item is not the last one in the list
            {
                SettingUtil.MovePriorityExtensionFileDown(index);

                // Sort the list after moving the item
                SortPriorityExtensions();  // Call the method to sort the list

                Message = "Extension moved down.";
            }
            else
            {
                Message = "Cannot move down further."; // Error message if it is already at the bottom
            }
        }

        /// <summary>
        /// Sorts the list of priority extensions by their index.
        /// This method orders the extensions in the PriorityExtensionFiles collection 
        /// based on the 'Index' property, and updates the collection to reflect the sorted order.
        /// </summary>
        private void SortPriorityExtensions()
        {
            // Sort the list of extensions by the 'Index' property
            // This assumes that PriorityExtensionDTO has an 'Index' property that is used to determine priority
            var sortedList = PriorityExtensionFiles.OrderBy(p => p.Index).ToList();

            // Clear the existing list in the ObservableCollection
            PriorityExtensionFiles.Clear();

            // Add the sorted extensions back to the ObservableCollection
            foreach (var extension in sortedList)
            {
                PriorityExtensionFiles.Add(extension);
            }

            // Notify the view that the ObservableCollection has been updated and the list has changed
            // This triggers a re-render of the UI elements bound to PriorityExtensionFiles
            this.RaisePropertyChanged(nameof(PriorityExtensionFiles));
        }
    }
}
