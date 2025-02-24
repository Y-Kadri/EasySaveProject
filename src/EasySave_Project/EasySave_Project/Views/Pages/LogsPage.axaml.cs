using Avalonia.Controls;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Pages;

namespace EasySave_Project.Views.Pages;

public partial class LogsPage : UserControl, IPage
{

    private LogsPageViewModel _LogsPageViewModel;

    public LogsPage()
    {
        _LogsPageViewModel = new LogsPageViewModel(); 
        InitializeComponent();
        DataContext = _LogsPageViewModel;
    }
    
    public void Reload()
    {
        _LogsPageViewModel.Refresh();
    }
}