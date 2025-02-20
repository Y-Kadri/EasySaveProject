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
                _settingPageViewModel.AddEncryptedFileExtension(ExtensionInput.Text);
                ExtensionInput.Clear(); // Efface le texte après l'ajout
            }
        }

        private void RemoveEncryptedFileExtensions_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var extension = (string)button.DataContext; // Récupère l'extension à supprimer
            _settingPageViewModel.RemoveEncryptedFileExtension(extension);
        }

        private void MoveExtensionUp_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var extension = (string)button.DataContext;
            _settingPageViewModel.MoveExtensionUp(extension);
        }

        private void MoveExtensionDown_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var extension = (string)button.DataContext;
            _settingPageViewModel.MoveExtensionDown(extension);
        }

        private void AddPriorityBusinessProcess_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SoftwareInput.Text))
            {
                _settingPageViewModel.AddPriorityBusinessProcess(SoftwareInput.Text);
                SoftwareInput.Clear(); // Efface le texte après l'ajout
            }
        }

        private void RemovePriorityBusinessProcess_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var software = (string)button.DataContext; // Récupère le logiciel à supprimer
            _settingPageViewModel.RemovePriorityBusinessProcess(software);
        }

        private void MoveSoftwareUp_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var software = (string)button.DataContext;
            _settingPageViewModel.MoveSoftwareUp(software);
        }

        private void MoveSoftwareDown_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var software = (string)button.DataContext;
            _settingPageViewModel.MoveSoftwareDown(software);
        }


    }
}