using Avalonia.Controls;
using Avalonia.Interactivity;
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

            // Passer la méthode NotifyCallback au ViewModel
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

        public void Update()
        {
            Reload();
            _baseLayout.reload();
        }
    }
}