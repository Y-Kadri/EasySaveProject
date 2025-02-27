using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using EasySave_Library_Log.manager;
using EasySave_Project.Dto;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Util;
using EasySave_Project.Views.Components;
using EasySave_Project.Views.Layout;
using Microsoft.VisualBasic.CompilerServices;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages
{
    public class SettingPageViewModel : ReactiveObject
    {
        private readonly TranslationService _translationService;
        
        public ObservableCollection<string> EncryptedFileExtensions { get; }
        public ObservableCollection<string> PriorityBusinessProcess { get; }
        
        private ObservableCollection<PriorityExtensionDTO> _priorityExtensionFiles;
        
        private string _message, _status, _selectLanguage, _french, _english,
            _chooseLogsFormat, _Json, _Xml, _add, _fileExtensionsToEncrypt, 
            _monitoredBusinessSoftware, _maxLargeFileSizeText, _priorityFileManagement,
            _PriorityExtendions;
        
        private int _maxLargeFileSize;

        private StackPanel NotificationContainer;

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
        
        public string MaxLargeFileSizeText
        {
            get => _maxLargeFileSizeText;
            set => this.RaiseAndSetIfChanged(ref _maxLargeFileSizeText, value);
        }
        
        public int MaxLargeFileSize
        {
            get => _maxLargeFileSize;
            set => this.RaiseAndSetIfChanged(ref _maxLargeFileSize, value);
        }
        
        public ObservableCollection<PriorityExtensionDTO> PriorityExtensionFiles
        {
            get => _priorityExtensionFiles;
            set => this.RaiseAndSetIfChanged(ref _priorityExtensionFiles, value);
        }

        public string PriorityFileManagement
        {
            get => _priorityFileManagement;
            set => this.RaiseAndSetIfChanged(ref _priorityFileManagement, value);
        }
        
        public string PriorityExtendions
        {
            get => _PriorityExtendions;
            set => this.RaiseAndSetIfChanged(ref _PriorityExtendions, value);
        }

        public SettingPageViewModel(StackPanel notificationContainer)
        {
            _translationService = TranslationService.GetInstance();
            EncryptedFileExtensions = new ObservableCollection<string>(SettingUtil.GetList("EncryptedFileExtensions"));
            PriorityBusinessProcess = new ObservableCollection<string>(SettingUtil.GetList("PriorityBusinessProcess"));
            
            PriorityExtensionFiles = new ObservableCollection<PriorityExtensionDTO>(
                SettingUtil.GetPriorityExtensionFilesList("PriorityExtensionFiles")
                    .OrderBy(p => p.Index));
            MaxLargeFileSize = FileUtil.GetAppSettingsInt("MaxLargeFileSize");

            NotificationContainer = notificationContainer;
        }
        
        

        /// <summary>
        /// Refreshes the translations for various UI elements related to settings.
        /// </summary>
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
            MaxLargeFileSizeText = _translationService.GetText("MaxLargeFileSize");
            PriorityFileManagement = _translationService.GetText("PriorityFileManagement");
            PriorityExtendions = _translationService.GetText("PriorityExtendions");
        }
        
        /// <summary>
        /// Adds a new file extension to the encrypted file extensions list.
        /// </summary>
        /// <param name="extension">The file extension to be added.</param>
        public void AddEncryptedFileExtensions(string extension)
        {
            if (SettingUtil.AddToList("EncryptedFileExtensions", extension))
            {
                EncryptedFileExtensions.Add(extension);
                this.RaisePropertyChanged(nameof(EncryptedFileExtensions));
                Message = $"{_translationService.GetText("Extensionajoutéesuccès")}.";
                Toastr.ShowNotification(Message, NotificationContainer, "Success");
            }
            else
            {
                Message = $"{_translationService.GetText("Erreurajout")}.";
                Toastr.ShowNotification(Message, NotificationContainer);
            }
        }
        
        public void RemovePriorityFileExtensions(PriorityExtensionDTO priority)
        {
            if (SettingUtil.RemovePriorityExtension(priority.ExtensionFile))
            {
                PriorityExtensionFiles.Remove(priority);
                this.RaisePropertyChanged(nameof(PriorityExtensionFiles));
                Message = $"{_translationService.GetText("Prioritésupprimésuccès")}.";
                Toastr.ShowNotification(Message, NotificationContainer, "Success");
            }
            else
            {
                Message = $"{_translationService.GetText("Erreursuppression")}.";
                Toastr.ShowNotification(Message, NotificationContainer);
            }
        }
        
        /// <summary>
        /// Adds a software process to the priority business process list.
        /// </summary>
        /// <param name="software">The name of the software process.</param>
        public void AddPriorityBusinessProcess(string software)
        {
            if (SettingUtil.AddToList("PriorityBusinessProcess", software))
            {
                PriorityBusinessProcess.Add(software);
                this.RaisePropertyChanged(nameof(PriorityBusinessProcess));
                Message = $"{_translationService.GetText("Logicielajoutésuccès")}.";
                Toastr.ShowNotification(Message, NotificationContainer, "Success");
            }
            else
            {
                Message = $"{_translationService.GetText("Erreurajout")}.";
                Toastr.ShowNotification(Message, NotificationContainer);
            }
        }

        /// <summary>
        /// Removes a software process from the priority business process list.
        /// </summary>
        /// <param name="software">The name of the software process to be removed.</param>
        public void AddPriorityFileExtensions(string extensionfile)
        {
            (bool reponse, PriorityExtensionDTO pdo) = SettingUtil.AddPriorityExtension(extensionfile);
            if (reponse)
            {
                PriorityExtensionFiles.Add(pdo);
                this.RaisePropertyChanged(nameof(PriorityExtensionFiles));
                Message = $"{_translationService.GetText("Prioritéajoutésuccès")}.";
                Toastr.ShowNotification(Message, NotificationContainer, "Success");
            }
            else
            {
                Message = $"{_translationService.GetText("Erreurajout")}.";
                Toastr.ShowNotification(Message, NotificationContainer);
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
            if (SettingUtil.SettingChangeMaxLargeFileSize(value))
            {
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

        /// <summary>
        /// Removes a file extension from the encrypted file extensions list.
        /// </summary>
        /// <param name="extension">The file extension to be removed.</param>
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
        
        /// <summary>
        /// Changes the application's language setting.
        /// </summary>
        /// <param name="lang">The new language to be set.</param>
        /// <returns>A tuple containing a message and status indicating success or failure.</returns>
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

            return (_message, _status);
        }

        /// <summary>
        /// Changes the log format used for storing logs.
        /// </summary>
        /// <param name="logsFormat">The new log format.</param>
        /// <returns>A tuple containing a message and status indicating success or failure.</returns>
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
            if (index > 0)
            {
                SettingUtil.MovePriorityExtensionFileUp(index);
                SortPriorityExtensions(index, true);
                Message = "Extension moved up.";
            }
            else
            {
                Message = "Cannot move up further.";
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
                SortPriorityExtensions(index, false);  // Call the method to sort the list

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
        private void SortPriorityExtensions(int index, bool up)
        {
            if (index < 0 || index >= PriorityExtensionFiles.Count)
                return; // Vérification pour éviter les erreurs

            int newIndex = up ? index - 1 : index + 1;

            if (newIndex < 0 || newIndex >= PriorityExtensionFiles.Count)
                return; // Vérification pour éviter les dépassements

            // Échanger les éléments
            var temp = PriorityExtensionFiles[index];
            PriorityExtensionFiles[index] = PriorityExtensionFiles[newIndex];
            PriorityExtensionFiles[newIndex] = temp;

            // Mettre à jour les indices
            PriorityExtensionFiles[index].Index = index;
            PriorityExtensionFiles[newIndex].Index = newIndex;

            this.RaisePropertyChanged(nameof(PriorityExtensionFiles));
        }
    }
}
