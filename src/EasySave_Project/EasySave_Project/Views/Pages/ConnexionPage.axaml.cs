using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Project.Model;
using EasySave_Project.Server;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Layout;

namespace EasySave_Project.Views.Pages;

public partial class ConnexionPage : UserControl, IPage
{
    private ConnexionViewModel _ConnexionViewModel;
    private BaseLayout _baseLayout;
    
    public ConnexionPage(BaseLayout baseLayout)
    {
        InitializeComponent();
        _baseLayout = baseLayout;
        _ConnexionViewModel = new ConnexionViewModel();
        DataContext = _ConnexionViewModel;
    }
    
    public void Reload()
    {

        _ConnexionViewModel = new ConnexionViewModel();
        if (GlobalDataService.GetInstance().isConnecte && GlobalDataService.GetInstance().connecteTo.Item2 == null)
        {
            ConnectedMessage.IsVisible = true;
            LoginForm.IsVisible = false;
            ConnectedToMessage.IsVisible = false;
            _ConnexionViewModel.GetAllUserConnect();
        }
        else if (GlobalDataService.GetInstance().isConnecte && GlobalDataService.GetInstance().connecteTo.Item2 != null)
        {
            ConnectedMessage.IsVisible = false;
            LoginForm.IsVisible = false;
            ConnectedToMessage.IsVisible = true;

            ConnectedToTitre.Text = $"\u2705 Vous êtes connecté à {GlobalDataService.GetInstance().connecteTo.Item2} !";
        }
        else
        {
            LoginForm.IsVisible = true;
            ConnectedMessage.IsVisible = false;
            ConnectedToMessage.IsVisible = false;
        }
        DataContext = _ConnexionViewModel;
    }

    private void Connexion(object? sender, RoutedEventArgs e)
    {
        string name = UserName.Text;

        try
        {
            _ConnexionViewModel.Connexion(name);
            _baseLayout.reload();
            Reload();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void ConnectTo(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is User user)
        {
            string id = user.Id;
            string name = user.Name;

            _ConnexionViewModel.ConnexionTo(id, name);
            _baseLayout.reload();
            Reload();
        }
    }

    private void DisconnectTo(object? sender, RoutedEventArgs e)
    {
        _ConnexionViewModel.DisconnexionTo();
        _baseLayout.reload();
        Reload();
    }
}