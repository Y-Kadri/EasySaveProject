using ReactiveUI;
using EasySave_Project.Service;

namespace EasySave_Project.ViewModels.Pages
{
    public class AddJobsPageViewModel : ReactiveObject
    {
        private string _enterName;
        public string EnterName
        {
            get => _enterName;
            set => this.RaiseAndSetIfChanged(ref _enterName, value);
        }

        private string _complete;
        public string Complete
        {
            get => _complete;
            set => this.RaiseAndSetIfChanged(ref _complete, value);
        }

        private string _differential;
        public string Differential
        {
            get => _differential;
            set => this.RaiseAndSetIfChanged(ref _differential, value);
        }

        private string _chooseASourceFolder;
        public string ChooseASourceFolder
        {
            get => _chooseASourceFolder;
            set => this.RaiseAndSetIfChanged(ref _chooseASourceFolder, value);
        }

        private string _chooseADestinationFolder;
        public string ChooseADestinationFolder
        {
            get => _chooseADestinationFolder;
            set => this.RaiseAndSetIfChanged(ref _chooseADestinationFolder, value);
        }

        private string _noFolderSelected;
        public string NoFolderSelected
        {
            get => _noFolderSelected;
            set => this.RaiseAndSetIfChanged(ref _noFolderSelected, value);
        }

        private string _add;
        public string Add
        {
            get => _add;
            set => this.RaiseAndSetIfChanged(ref _add, value);
        }

        private string _addAJob;
        public string AddAJob
        {
            get => _addAJob;
            set => this.RaiseAndSetIfChanged(ref _addAJob, value);
        }

        public AddJobsPageViewModel()
        {
            LoadTranslations();
        }

        public void LoadTranslations()
        {
            var translationService = TranslationService.GetInstance();
            EnterName = translationService.GetText("EnterName");
            AddAJob = translationService.GetText("AddAJob");
            Complete = translationService.GetText("Complete");
            Differential = translationService.GetText("Differential");
            ChooseASourceFolder = translationService.GetText("ChooseASourceFolder");
            ChooseADestinationFolder = translationService.GetText("ChooseADestinationFolder");
            NoFolderSelected = translationService.GetText("NoFolderSelected");
            Add = translationService.GetText("Add");
        }

        public void RefreshTranslations()
        {
            LoadTranslations();
        }
    }
}
