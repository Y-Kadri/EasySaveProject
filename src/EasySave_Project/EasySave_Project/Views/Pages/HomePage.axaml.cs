using Avalonia.Controls;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Pages;

namespace EasySave_Project.Views.Pages;

public partial class HomePage : UserControl, IPage
{

    public HomePage()
    {
        InitializeComponent();
        DataContext = new HomePageViewModel(); 
    }
    
    public void reload()
    {
        DataContext = new HomePageViewModel();
    }
}