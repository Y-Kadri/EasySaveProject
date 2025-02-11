using Avalonia.Controls;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Pages;

namespace EasySave_Project.Views.Pages;

public partial class SettingPage : UserControl, IPage
{

    public SettingPage()
    {
        InitializeComponent();
        DataContext = new SettingPageViewModel(); 
    }
    
    public void reload()
    {
        DataContext = new SettingPageViewModel();
    }
}