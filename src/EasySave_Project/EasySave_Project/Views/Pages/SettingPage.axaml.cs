using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Library_Log.Utils;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Components;
using EasySave_Project.Views.Layout;
using EasySave_Project.Util;
using FileUtil = EasySave_Project.Util.FileUtil;

namespace EasySave_Project.Views.Pages;

public partial class SettingPage : UserControl, IPage
{
    private BaseLayout _baseLayout;
    
    public SettingPage(BaseLayout baseLayout)
    {
        InitializeComponent();
        DataContext = new SettingPageViewModel(); 
        _baseLayout = baseLayout;
    }
    
    public void reload()
    {
        DataContext = new SettingPageViewModel();
    }
    
    private void OnLanguageChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton selectedRadioButton)
        {

            LanguageEnum lang;
            
            // Vérifie quelle langue est sélectionnée
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


            if (FileUtil.SettingChangeLanguage(lang))
            {
                TranslationService.SetLanguage(lang);
                string messageSuccess = TranslationService.GetInstance().GetText("LanguageChangeSuccess");
                Toastr.ShowNotification(messageSuccess, NotificationContainer, "Success");

                // Met à jour la langue dans l'interface
                UpdateLanguage(); 
            }
            else
            {
                string messageSuccess = TranslationService.GetInstance().GetText("LanguageChangeError");
                Toastr.ShowNotification(messageSuccess, NotificationContainer);
            }
        }
    }

    public void UpdateLanguage()
    {
        reload();
        _baseLayout.reload();
    }
}