using ReactiveUI;
using EasySave_Project.Service;

namespace EasySave_Project.ViewModels.Pages
{
    public class AddJobsPageViewModel : ReactiveObject
    {
        private readonly TranslationService _translationService = TranslationService.GetInstance();
            
        private string _enterName, _complete, _differential, 
            _chooseASourceFolder, _chooseADestinationFolder, _noFolderSelected, 
            _add, _addAJob;
        
        public string EnterName
        {
            get => _enterName;
            set => this.RaiseAndSetIfChanged(ref _enterName, value);
        }

        public string Complete
        {
            get => _complete;
            set => this.RaiseAndSetIfChanged(ref _complete, value);
        }

        public string Differential
        {
            get => _differential;
            set => this.RaiseAndSetIfChanged(ref _differential, value);
        }

        public string ChooseASourceFolder
        {
            get => _chooseASourceFolder;
            set => this.RaiseAndSetIfChanged(ref _chooseASourceFolder, value);
        }

        public string ChooseADestinationFolder
        {
            get => _chooseADestinationFolder;
            set => this.RaiseAndSetIfChanged(ref _chooseADestinationFolder, value);
        }

        public string NoFolderSelected
        {
            get => _noFolderSelected;
            set => this.RaiseAndSetIfChanged(ref _noFolderSelected, value);
        }

        public string Add
        {
            get => _add;
            set => this.RaiseAndSetIfChanged(ref _add, value);
        }

        public string AddAJob
        {
            get => _addAJob;
            set => this.RaiseAndSetIfChanged(ref _addAJob, value);
        }
        
        public AddJobsPageViewModel()
        {
            LoadTranslations();
        }

        /// <summary>
        /// Loads translated text for UI elements from the translation service.
        /// </summary>
        public void LoadTranslations()
        {
            EnterName = _translationService.GetText("EnterName");
            AddAJob = _translationService.GetText("AddAJob");
            Complete = _translationService.GetText("Complete");
            Differential = _translationService.GetText("Differential");
            ChooseASourceFolder = _translationService.GetText("ChooseASourceFolder");
            ChooseADestinationFolder = _translationService.GetText("ChooseADestinationFolder");
            NoFolderSelected = _translationService.GetText("NoFolderSelected");
            Add = _translationService.GetText("Add");
        }

        /// <summary>
        /// Refreshes translations when the language setting changes.
        /// </summary>
        public void RefreshTranslations()
        {
            LoadTranslations();
        }
    }
}
