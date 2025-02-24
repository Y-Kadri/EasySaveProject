using EasySave_Project.Service;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages
{
    public class HomePageViewModel : ReactiveObject
    {
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

        public void UpdateTitle()
        {
            Title = TranslationService.GetInstance().GetText("HomePageTitle");
        }
    }
}