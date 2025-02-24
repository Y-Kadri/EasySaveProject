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

        public SettingPage(BaseLayout baseLayout)
        {
            InitializeComponent();
            _baseLayout = baseLayout;
            _settingPageViewModel = new SettingPageViewModel();
            DataContext = _settingPageViewModel;
        }

        public void Reload()
        {
            DataContext = new SettingPageViewModel();
        }

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

        private void Update()
        {
            Reload();
            _baseLayout.reload();
        }
        
        private void AddEncryptedFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ExtensionInput.Text))
            {
                _settingPageViewModel.AddEncryptedFileExtensions(ExtensionInput.Text);
                ExtensionInput.Text = "";
                Reload();
            }
        }

        private void RemoveEncryptedFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var extension = (string)button.DataContext;
            _settingPageViewModel.RemoveEncryptedFileExtensions(extension);
            Reload();
        }

        private void AddPriorityBusinessProcess_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SoftwareInput.Text))
            {
                _settingPageViewModel.AddPriorityBusinessProcess(SoftwareInput.Text);
                SoftwareInput.Text = "";
                Reload();
            }
        }

        private void RemovePriorityBusinessProcess_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var software = (string)button.DataContext;
            _settingPageViewModel.RemovPriorityBusinessProcess(software);
            Reload();
        }

        /// <summary>
        /// Handles the click event for updating the maximum large file size setting.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void MaxLargeFileSize_Click(object sender, RoutedEventArgs e)
        {
            // Check if a value is set in the input field
            if (MaxLargeFileSizeInput.Value.HasValue)
            {
                // Convert the decimal value to an integer
                int value = (int)MaxLargeFileSizeInput.Value.Value;

                // Call ChangeMaxLargeFileSize and deconstruct the returned tuple
                var (message, status) = _settingPageViewModel.ChangeMaxLargeFileSize(value);

                // Display the notification with the retrieved values
                Toastr.ShowNotification(message, NotificationContainer, status);

                // Update the UI or settings after the change
                Update();
            }
        }
    }
}