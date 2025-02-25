using EasySave_Project.Service;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages
{
    public class HomePageViewModel : ReactiveObject
    {
        private readonly TranslationService _translationService = TranslationService.GetInstance();
        
        private string _title;
        
        public string Title
        {
            get => _title;
            private set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public HomePageViewModel()
        {
            UpdateTitle();
        }

        /// <summary>
        /// Update the title display on the page
        /// </summary>
        public void UpdateTitle()
        {
            Title = _translationService.GetText("HomePageTitle");
        }
    }
}