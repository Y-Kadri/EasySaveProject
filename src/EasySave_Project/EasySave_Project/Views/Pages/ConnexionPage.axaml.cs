using System;
using System.Collections.Generic;
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
        DataContext = _ConnexionViewModel;

        if (GlobalDataService.GetInstance().isConnecte)
        {
            ConnectedMessage.IsVisible = true;
            LoginForm.IsVisible = false;

            List<User> users = _ConnexionViewModel.GetAllUserConnect();
            Console.WriteLine("");
        }
        else
        {
            LoginForm.IsVisible = true;
            ConnectedMessage.IsVisible = false;
        }
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
}