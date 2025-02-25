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
        public string PriorityFileManagement { get; private set; }
        public string PriorityExtendions { get; private set; }

        // Constructor
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
            MaxLargeFileSize = FileUtil.GetAppSettingsInt("MaxLargeFileSize");
            PriorityFileManagement = _translationService.GetText("PriorityFileManagement");
            PriorityExtendions = _translationService.GetText("PriorityExtendions");

            EncryptedFileExtensions = new ObservableCollection<string>(SettingUtil.GetList("EncryptedFileExtensions"));
            PriorityBusinessProcess = new ObservableCollection<string>(SettingUtil.GetList("PriorityBusinessProcess"));
            PriorityExtensionFiles = new ObservableCollection<PriorityExtensionDTO>(SettingUtil.GetPriorityExtensionFilesList("PriorityExtensionFiles"));
            SortPriorityExtensions();
        }

        public (string message, string status) AddEncryptedFileExtensions(string extension)
        {
            string _message;
            if (SettingUtil.AddToList("EncryptedFileExtensions", extension))
            {
                EncryptedFileExtensions.Add(extension);
                this.RaisePropertyChanged(nameof(EncryptedFileExtensions));
                _message = _translationService.GetText("ExtensionAddedSuccessfully");
                _status = "Success";
            }
            else
            {
                _message = _translationService.GetText("ErrorAddingExtension");
                _status = "Error";
            }
            return (_message, _status);
        }

        public void AddPriorityBusinessProcess(string software)
        {
            if (SettingUtil.AddToList("PriorityBusinessProcess", software))
            {
                PriorityBusinessProcess.Add(software);
                this.RaisePropertyChanged(nameof(PriorityBusinessProcess));
            }
        }

        public (string message, string status) AddPriorityFileExtensions(string extensionfile)
        {
            string _message;
            if (SettingUtil.AddPriorityExtension(extensionfile))
            {
                this.RaisePropertyChanged(nameof(PriorityExtensionFiles));
                _message = _translationService.GetText("PriorityAddedSuccessfully");
                _status = "Success";
            }
            else
            {
                _message = _translationService.GetText("ErrorAddingPriority");
                _status = "Error";
            }
            return (_message, _status);
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

        public (string message, string status) RemovPriorityBusinessProcess(string software)
        {
            if (SettingUtil.RemoveFromList("PriorityBusinessProcess", software))
            {
                PriorityBusinessProcess.Remove(software);
                this.RaisePropertyChanged(nameof(PriorityBusinessProcess));
                _message = _translationService.GetText("SoftwareRemovedSuccessfully");
                _status = "Success";
            }
            else
            {
                _message = _translationService.GetText("ErrorRemovingSoftware");
                _status = "Error";
            }
            return (_message, _status);
        }

        public (string message, string status) RemoveEncryptedFileExtensions(string extension)
        {
            string _message;
            if (SettingUtil.RemoveFromList("EncryptedFileExtensions", extension))
            {
                EncryptedFileExtensions.Remove(extension);
                this.RaisePropertyChanged(nameof(EncryptedFileExtensions));
                _message = _translationService.GetText("ExtensionRemovedSuccessfully");
                _status = "Success";
            }
            else
            {
                _message = _translationService.GetText("ErrorRemovingExtension");
                _status = "Error";
            }
            return (_message, _status);
        }

        public (string message, string status) RemovePriorityFileExtensions(PriorityExtensionDTO priority)
        {
            string _message;
            if (SettingUtil.RemovePriorityExtension(priority.ExtensionFile))
            {
                PriorityExtensionFiles.Remove(priority);
                this.RaisePropertyChanged(nameof(PriorityExtensionFiles));
                _message = _translationService.GetText("PriorityRemovedSuccessfully");
                _status = "Success";
            }
            else
            {
                _message = _translationService.GetText("ErrorRemovingPriority");
                _status = "Error";
            }
            return (_message, _status);
        }

        // Method to change the language and call the notification
        public (string message, string status) ChangeLanguage(LanguageEnum lang)
        {
            string _message;
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

            // Call the callback to display the notification in the view
            return (_message, _status);
        }

        public (string message, string status) ChangeLogsFormat(LogFormatManager.LogFormat logsFormat)
        {
            string _message;
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
        public (string message, string status) MoveExtensionUp(int index)
        {
            string _message;
            if (index > 0) // Check if the item is not the first one in the list
            {
                SettingUtil.MovePriorityExtensionFileUp(index);

                // Sort the list after moving the item
                SortPriorityExtensions();  // Call the method to sort the list

                _message = _translationService.GetText("ExtensionMovedUp");
                _status = "Success";
            }
            else
            {
                _message = _translationService.GetText("CannotMoveUpFurther"); // Error message if it is already at the top
                _status = "Error";
            }
            return (_message, _status);
        }

        /// <summary>
        /// Moves the file extension down in the list if it is not already at the bottom.
        /// </summary>
        /// <param name="index">The index of the item to move.</param>
        public (string message, string status) MoveExtensionDown(int index)
        {
            string _message;
            if (index < PriorityExtensionFiles.Count - 1) // Check if the item is not the last one in the list
            {
                SettingUtil.MovePriorityExtensionFileDown(index);

                // Sort the list after moving the item
                SortPriorityExtensions();  // Call the method to sort the list

                _message = _translationService.GetText("ExtensionMovedDown");
                _status = "Success";
            }
            else
            {
                _message = _translationService.GetText("CannotMoveDownFurther"); // Error message if it is already at the bottom
                _status = "Error";
            }
            return (_message, _status);
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
