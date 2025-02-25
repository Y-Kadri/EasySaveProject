using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Components;
using EasySave_Project.Views.Layout;

namespace EasySave_Project.Views.Pages
{
    public partial class SettingPage : UserControl, IPage
    {
        private BaseLayout _baseLayout;
        private SettingPageViewModel _settingPageViewModel;

        /// <summary>
        /// Initializes the settings page, setting up the ViewModel and layout reference.
        /// </summary>
        /// <param name="baseLayout">The main application layout.</param>
        public SettingPage(BaseLayout baseLayout)
        {
            InitializeComponent();
            _baseLayout = baseLayout;
            _settingPageViewModel = new SettingPageViewModel();
            DataContext = _settingPageViewModel;
        }

        /// <summary>
        /// Reloads the settings page, refreshing the ViewModel.
        /// </summary>
        public void Reload()
        {
            _settingPageViewModel.Refresh();
        }

        /// <summary>
        /// Handles language change event, updating the application language based on user selection.
        /// </summary>
        /// <param name="sender">The event source, a RadioButton.</param>
        /// <param name="e">Event arguments.</param>
        private void OnLanguageChanged(object? sender, RoutedEventArgs e)
        {
            if (sender is RadioButton selectedRadioButton)
            {
                LanguageEnum lang;

                if (selectedRadioButton == FirstLangue)
                {
                    lang = LanguageEnum.FR;
                }
                else if (selectedRadioButton == SecondLangue)
                {
                    lang = LanguageEnum.EN;
                }
                else
                {
                    return;
                }
                
                // Appel de ChangeLanguage et décomposition du tuple
                var (message, status) = _settingPageViewModel.ChangeLanguage(lang);

                // Affichage de la notification avec les valeurs récupérées
                Toastr.ShowNotification(message, NotificationContainer, status);
                Update();
            }
        }
        
        /// <summary>
        /// Handles log format change event, updating the log format to JSON or XML based on user selection.
        /// </summary>
        /// <param name="sender">The event source, a RadioButton.</param>
        /// <param name="e">Event arguments.</param>
        private void OnLogsFormatChanged(object? sender, RoutedEventArgs e)
        {
            if (sender is RadioButton selectedRadioButton)
            {
                LogFormatManager.LogFormat logsFormat;

                if (selectedRadioButton == FirstFormat)
                {
                    logsFormat = LogFormatManager.LogFormat.JSON;
                }
                else if (selectedRadioButton == SecondFormat)
                {
                    logsFormat = LogFormatManager.LogFormat.XML;
                }
                else
                {
                    return;
                }
                
                // Appel de ChangeLogsFormat et décomposition du tuple
                var (message, status) = _settingPageViewModel.ChangeLogsFormat(logsFormat);

                // Affichage de la notification avec les valeurs récupérées
                Toastr.ShowNotification(message, NotificationContainer, status);
                Update();
            }
        }

        /// <summary>
        /// Updates the settings page and reloads the main layout.
        /// </summary>
        private void Update()
        {
            Reload();
            _baseLayout.Reload();
        }
        
        /// <summary>
        /// Handles the event for adding a new encrypted file extension to the list.
        /// </summary>
        /// <param name="sender">The event source, a Button.</param>
        /// <param name="e">Event arguments.</param>
        private void AddEncryptedFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ExtensionInput.Text))
            {
                _settingPageViewModel.AddEncryptedFileExtensions(ExtensionInput.Text);
                ExtensionInput.Text = "";
                Reload();
            }
        }

        /// <summary>
        /// Handles the event for removing an encrypted file extension from the list.
        /// </summary>
        /// <param name="sender">The event source, a Button.</param>
        /// <param name="e">Event arguments.</param>
        private void RemoveEncryptedFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var extension = (string)button.DataContext;
            _settingPageViewModel.RemoveEncryptedFileExtensions(extension);
            Reload();
        }

        /// <summary>
        /// Handles the event for adding a new priority business process.
        /// </summary>
        /// <param name="sender">The event source, a Button.</param>
        /// <param name="e">Event arguments.</param>
        private void AddPriorityBusinessProcess_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SoftwareInput.Text))
            {
                _settingPageViewModel.AddPriorityBusinessProcess(SoftwareInput.Text);
                SoftwareInput.Text = "";
                Reload();
            }
        }

        /// <summary>
        /// Handles the event for removing a priority business process.
        /// </summary>
        /// <param name="sender">The event source, a Button.</param>
        /// <param name="e">Event arguments.</param>
        private void RemovePriorityBusinessProcess_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var software = (string)button.DataContext;
            _settingPageViewModel.RemovPriorityBusinessProcess(software);
            Reload();
        }
    }
}