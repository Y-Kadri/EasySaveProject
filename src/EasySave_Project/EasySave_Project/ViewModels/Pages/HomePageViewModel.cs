using Avalonia.Controls;
using EasySave_Project.Service;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages
{
    public class HomePageViewModel : ReactiveObject
    {
        
        public string Title { get; set; }
        
        public HomePageViewModel()
        {
            Title = TranslationService.GetInstance().GetText("HomePageTitle");
        }
    }
}
