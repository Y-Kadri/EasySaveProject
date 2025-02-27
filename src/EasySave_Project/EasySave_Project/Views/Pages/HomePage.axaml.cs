using Avalonia.Controls;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Pages;

namespace EasySave_Project.Views.Pages
{
    public partial class HomePage : UserControl, IPage
    {
        private readonly HomePageViewModel _homePageViewModel;

        /// <summary>
        /// Initializes the HomePage, sets up the ViewModel, and assigns the DataContext.
        /// </summary>
        public HomePage()
        {
            InitializeComponent();
            _homePageViewModel = new HomePageViewModel();
            DataContext = _homePageViewModel;
        }

        /// <summary>
        /// Reloads the HomePage by updating the title.
        /// </summary>
        public void Reload()
        {
            _homePageViewModel.UpdateTitle();
        }
    }
}