using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Project.Model;
using EasySave_Project.Server;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Layout;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Components;
using EasySave_Project.Views.Layout;

namespace EasySave_Project.Views.Pages
{
    public partial class ConnexionPage : UserControl, IPage
    {
        private readonly ConnexionViewModel _connexionViewModel;
        private readonly BaseLayout _baseLayout;
        private static TranslationService _translationService = TranslationService.GetInstance();

        /// <summary>
        /// Initializes the ConnexionPage, sets up the ViewModel, assigns the DataContext, and triggers an initial reload.
        /// </summary>
        /// <param name="baseLayout">Reference to the BaseLayout for refreshing UI elements.</param>
        public ConnexionPage(BaseLayout baseLayout)
        {
            InitializeComponent();
            _baseLayout = baseLayout;
            _connexionViewModel = new ConnexionViewModel();
            DataContext = _connexionViewModel;

            Reload();
        }

        /// <summary>
        /// Reloads the connection state and updates the UI accordingly.
        /// </summary>
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
            
            _connexionViewModel.Refresh();
        }

        /// <summary>
        /// Displays the login form when the user is not connected.
        /// </summary>
        private void ShowLoginForm()
        {
            LoginForm.IsVisible = true;
            ConnectedMessage.IsVisible = false;
            ConnectedToMessage.IsVisible = false;
        }

        /// <summary>
        /// Displays the connected state when the user is logged in but not connected to another user.
        /// </summary>
        private void ShowConnectedState()
        {
            ConnectedMessage.IsVisible = true;
            LoginForm.IsVisible = false;
            ConnectedToMessage.IsVisible = false;
        }

        /// <summary>
        /// Displays the state when the user is connected to another user.
        /// </summary>
        private void ShowConnectedToState()
        {
            ConnectedMessage.IsVisible = false;
            LoginForm.IsVisible = false;
            ConnectedToMessage.IsVisible = true;

            ConnectedToTitre.Text = $"\u2705 {_translationService.GetText("Vousêtesconnectéà")} {GlobalDataService.GetInstance().connecteTo.Item2} !";
        }

        /// <summary>
        /// Handles the login button click event and attempts to connect using the provided username.
        /// </summary>
        /// <param name="sender">The UI element that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Connexion(object? sender, RoutedEventArgs e)
        {
            try
            {
                string name = UserName.Text;
                Connexion(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        /// <summary>
        /// Attempts to connect the user asynchronously and displays a notification during the process.
        /// </summary>
        /// <param name="name">The username entered by the user.</param>
        /// <returns>A task representing the asynchronous connection process.</returns>
        public async Task Connexion(string name)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Toastr.ShowServeurNotification($"{_translationService.GetText("Connexionencours")} ...", BaseLayoutViewModel.Instance.NotificationContainer);
            });

            await Task.Run(() =>
            {
                try
                {
                    _connexionViewModel.Connexion(name);
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _baseLayout.Reload();
                        Reload();
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Erreur de connexion Page : {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Handles the event when a user attempts to connect to another user from the list.
        /// </summary>
        /// <param name="sender">The button triggering the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ConnectTo(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is User user)
            {
                _connexionViewModel.ConnexionTo(user.Id, user.Name);
                _baseLayout.Reload();
                Reload();
            }
        }

        /// <summary>
        /// Handles the event when the user disconnects from another user.
        /// </summary>
        /// <param name="sender">The button triggering the event.</param>
        /// <param name="e">Event arguments.</param>
        private void DisconnectTo(object? sender, RoutedEventArgs e)
        {
            _connexionViewModel.DisconnexionTo();
            _baseLayout.Reload();
            Reload();
        }
    }
}
