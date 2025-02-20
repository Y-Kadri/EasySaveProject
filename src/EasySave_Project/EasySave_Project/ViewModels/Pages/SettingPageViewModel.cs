using System.Collections.ObjectModel;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Util;
using ReactiveUI;
using System.Windows.Input;

namespace EasySave_Project.ViewModels.Pages
{
    public class SettingPageViewModel : ReactiveObject
    {
        private readonly TranslationService _translationService;

        public ObservableCollection<string> EncryptedFileExtensions { get; }
        public ObservableCollection<string> PriorityBusinessProcess { get; }
        public ObservableCollection<string> PriorityFileExtensions { get; }

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

        public ICommand AddEncryptedFileExtensionCommand { get; }
        public ICommand RemoveEncryptedFileExtensionCommand { get; }
        public ICommand MoveExtensionUpCommand { get; }
        public ICommand MoveExtensionDownCommand { get; }

        public ICommand AddPriorityBusinessProcessCommand { get; }
        public ICommand RemovePriorityBusinessProcessCommand { get; }
        public ICommand MoveSoftwareUpCommand { get; }
        public ICommand MoveSoftwareDownCommand { get; }

        public string SelectLanguage { get; private set; }
        public string French { get; private set; }
        public string English { get; private set; }
        public string ChooseLogsFormat { get; private set; }
        public string Json { get; private set; }
        public string Xml { get; private set; }

        public string Add { get; private set; }
        public string FileExtensionsToEncrypt { get; private set; }
        public string MonitoredBusinessSoftware { get; private set; }

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

            EncryptedFileExtensions = new ObservableCollection<string>();
            PriorityBusinessProcess = new ObservableCollection<string>();
            PriorityFileExtensions = new ObservableCollection<string>();

            AddEncryptedFileExtensionCommand = ReactiveCommand.Create<string>(AddEncryptedFileExtension);
            RemoveEncryptedFileExtensionCommand = ReactiveCommand.Create<string>(RemoveEncryptedFileExtension);
            MoveExtensionUpCommand = ReactiveCommand.Create<string>(MoveExtensionUp);
            MoveExtensionDownCommand = ReactiveCommand.Create<string>(MoveExtensionDown);

            AddPriorityBusinessProcessCommand = ReactiveCommand.Create<string>(AddPriorityBusinessProcess);
            RemovePriorityBusinessProcessCommand = ReactiveCommand.Create<string>(RemovePriorityBusinessProcess);
            MoveSoftwareUpCommand = ReactiveCommand.Create<string>(MoveSoftwareUp);
            MoveSoftwareDownCommand = ReactiveCommand.Create<string>(MoveSoftwareDown);
        }

        public void AddEncryptedFileExtension(string extension)
        {
            EncryptedFileExtensions.Add(extension);
        }

        public void RemoveEncryptedFileExtension(string extension)
        {
            EncryptedFileExtensions.Remove(extension);
        }

        public void MoveExtensionUp(string extension)
        {
            var index = EncryptedFileExtensions.IndexOf(extension);
            if (index > 0)
            {
                EncryptedFileExtensions.Move(index, index - 1);
            }
        }

        public void MoveExtensionDown(string extension)
        {
            var index = EncryptedFileExtensions.IndexOf(extension);
            if (index < EncryptedFileExtensions.Count - 1)
            {
                EncryptedFileExtensions.Move(index, index + 1);
            }
        }

        public void AddPriorityBusinessProcess(string software)
        {
            PriorityBusinessProcess.Add(software);
        }

        public void RemovePriorityBusinessProcess(string software)
        {
            PriorityBusinessProcess.Remove(software);
        }

        public void MoveSoftwareUp(string software)
        {
            var index = PriorityBusinessProcess.IndexOf(software);
            if (index > 0)
            {
                PriorityBusinessProcess.Move(index, index - 1);
            }
        }

        public void MoveSoftwareDown(string software)
        {
            var index = PriorityBusinessProcess.IndexOf(software);
            if (index < PriorityBusinessProcess.Count - 1)
            {
                PriorityBusinessProcess.Move(index, index + 1);
            }
        }
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
