using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Library_Log.manager;
using EasySave_Project.Dto;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Components;
using EasySave_Project.Views.Layout;

namespace EasySave_Project.Views.Pages
{
    public partial class SettingPage : UserControl, IPage
    {
        private BaseLayout _baseLayout;
        private SettingPageViewModel _settingPageViewModel;
        private readonly TranslationService _translationService = TranslationService.GetInstance();

        /// <summary>
        /// Initializes the settings page, setting up the ViewModel and layout reference.
        /// </summary>
        /// <param name="baseLayout">The main application layout.</param>
        public SettingPage(BaseLayout baseLayout)
        {
            InitializeComponent();
            _baseLayout = baseLayout;
            _settingPageViewModel = new SettingPageViewModel(NotificationContainer);
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
        private void AddPriorityFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PriorityInput.Text))
            {
                _settingPageViewModel.AddPriorityFileExtensions(PriorityInput.Text);
                PriorityInput.Text = "";
            }
            Reload();
        }

        private void RemovePriorityFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var priority = (PriorityExtensionDTO)button.DataContext; 
            _settingPageViewModel.RemovePriorityFileExtensions(priority); 
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

        /// <summary>
        /// Handles the click event for the "Move Up" button for file extensions.
        /// </summary>
        /// <param name="sender">The sender of the event (Button).</param>
        /// <param name="e">The event arguments.</param>
        private void MoveExtensionUp_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; // Get the button that was clicked
            var extension = (PriorityExtensionDTO)button.CommandParameter; // Cast CommandParameter to PriorityExtensionDTO
            var index = extension.Index; // Find the index of the extension in the list

            if (index > 0)
            {
                _settingPageViewModel.MoveExtensionUp(index); // Call the MoveExtensionUp method in the ViewModel with the found index
                Toastr.ShowNotification($"{_translationService.GetText("Extensionmovedup")}.", NotificationContainer, "Success"); // Show success notification
            }
            Reload();
        }

        /// <summary>
        /// Handles the click event for the "Move Down" button for file extensions.
        /// </summary>
        /// <param name="sender">The sender of the event (Button).</param>
        /// <param name="e">The event arguments.</param>
        private void MoveExtensionDown_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; // Get the button that was clicked
            var extension = (PriorityExtensionDTO)button.CommandParameter; // Cast CommandParameter to PriorityExtensionDTO
            var index = extension.Index; // Find the index of the extension in the list

            if (index >= 0 && index < _settingPageViewModel.PriorityExtensionFiles.Count)
            {
                _settingPageViewModel.MoveExtensionDown(index); // Call the MoveExtensionDown method in the ViewModel with the found index
                Toastr.ShowNotification($"{_translationService.GetText("Extensionmoveddown")}.", NotificationContainer, "Success"); // Show success notification
            }
            Reload();
        }

        /// <summary>
        /// Handles the click event for the "Delete" button for file extensions.
        /// </summary>
        /// <param name="sender">The sender of the event (Button).</param>
        /// <param name="e">The event arguments.</param>
        private void MoveSoftwareDown_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; // Get the button that was clicked
            var extension = (PriorityExtensionDTO)button.CommandParameter; // Cast CommandParameter to PriorityExtensionDTO
            var index = _settingPageViewModel.PriorityExtensionFiles.IndexOf(extension); // Find the index of the extension in the list

            if (index > 0)
            {
                //_settingPageViewModel.RemoveExtension(index); // Assuming you have a method to remove the extension
                Toastr.ShowNotification($"{_translationService.GetText("Extensionremoved")}.", NotificationContainer, "Success"); // Show success notification
            }
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