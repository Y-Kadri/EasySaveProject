using Avalonia.Controls;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Pages;

namespace EasySave_Project.Views.Pages
{
    public partial class HomePage : UserControl, IPage
    {
        private readonly HomePageViewModel _homePageViewModel;

        public HomePage()
        {
            InitializeComponent();
            _homePageViewModel = new HomePageViewModel();
            DataContext = _homePageViewModel;
        }

        public void Reload()
        {
            _homePageViewModel.UpdateTitle(); // Mise à jour dynamique
        }
    }
}