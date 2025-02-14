using Avalonia.Controls;
using EasySave_Project.Service;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages
{
    public class AddJobsPageViewModel : ReactiveObject
    {
        public string EnterName { get; private set; }
        public string Complete { get; private set; }
        public string Differential { get; private set; }
        public string ChooseASourceFolder { get; private set; }
        public string ChooseADestinationFolder { get; private set; }
        public string NoFolderSelected { get; private set; }
        public string Add { get; private set; }
        public string AddAJob { get; private set; }
        
        public AddJobsPageViewModel()
        {
            EnterName = TranslationService.GetInstance().GetText("EnterName");
            AddAJob = TranslationService.GetInstance().GetText("AddAJob");
            Complete = TranslationService.GetInstance().GetText("Complete");
            Differential = TranslationService.GetInstance().GetText("Differential");
            ChooseASourceFolder = TranslationService.GetInstance().GetText("ChooseASourceFolder");
            ChooseADestinationFolder = TranslationService.GetInstance().GetText("ChooseADestinationFolder");
            NoFolderSelected = TranslationService.GetInstance().GetText("NoFolderSelected");
            Add = TranslationService.GetInstance().GetText("Add");
        }
    }
}
