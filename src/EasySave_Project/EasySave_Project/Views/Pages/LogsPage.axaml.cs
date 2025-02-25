using Avalonia.Controls;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Pages;

namespace EasySave_Project.Views.Pages;

public partial class LogsPage : UserControl, IPage
{

    private LogsPageViewModel _LogsPageViewModel;

    /// <summary>
    /// Initializes the LogsPage, sets up the ViewModel, and assigns the DataContext.
    /// </summary>
    public LogsPage()
    {
        _LogsPageViewModel = new LogsPageViewModel(); 
        InitializeComponent();
        DataContext = _LogsPageViewModel;
    }
    
    /// <summary>
    /// Reloads the LogsPage by refreshing its ViewModel data.
    /// </summary>
    public void Reload()
    {
        _LogsPageViewModel.Refresh();
    }
}