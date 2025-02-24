using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Project.Model;
using EasySave_Project.Server;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Layout;

namespace EasySave_Project.Views.Pages
{
    public partial class ConnexionPage : UserControl, IPage
    {
        private readonly ConnexionViewModel _connexionViewModel;
        private readonly BaseLayout _baseLayout;

        public ConnexionPage(BaseLayout baseLayout)
        {
            InitializeComponent();
            _baseLayout = baseLayout;
            _connexionViewModel = new ConnexionViewModel();
            DataContext = _connexionViewModel;

            Reload();
        }

        public void Reload()
        {
            if (GlobalDataService.GetInstance().isConnecte)
            {
                if (GlobalDataService.GetInstance().connecteTo.Item2 == null)
                {
                    ShowConnectedState();
                    _connexionViewModel.GetAllUserConnect();
                }
                else
                {
                    ShowConnectedToState();
                }
            }
            else
            {
                ShowLoginForm();
            }
        }

        private void ShowLoginForm()
        {
            LoginForm.IsVisible = true;
            ConnectedMessage.IsVisible = false;
            ConnectedToMessage.IsVisible = false;
        }

        private void ShowConnectedState()
        {
            ConnectedMessage.IsVisible = true;
            LoginForm.IsVisible = false;
            ConnectedToMessage.IsVisible = false;
        }

        private void ShowConnectedToState()
        {
            ConnectedMessage.IsVisible = false;
            LoginForm.IsVisible = false;
            ConnectedToMessage.IsVisible = true;

            ConnectedToTitre.Text = $"\u2705 Vous êtes connecté à {GlobalDataService.GetInstance().connecteTo.Item2} !";
        }

        private void Connexion(object? sender, RoutedEventArgs e)
        {
            try
            {
                string name = UserName.Text;
                _connexionViewModel.Connexion(name);
                _baseLayout.Reload();
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
                _connexionViewModel.ConnexionTo(user.Id, user.Name);
                _baseLayout.Reload();
                Reload();
            }
        }

        private void DisconnectTo(object? sender, RoutedEventArgs e)
        {
            _connexionViewModel.DisconnexionTo();
            _baseLayout.Reload();
            Reload();
        }
    }
}
