using Avalonia.Controls;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Pages;

namespace EasySave_Project.Views.Pages;

public partial class LogsPage : UserControl, IPage
{

    public LogsPage()
    {
        InitializeComponent();
        DataContext = new LogsPageViewModel(); 
    }
    
    public void reload()
    {
        DataContext = new LogsPageViewModel();
    }
}