using Avalonia.Controls;
using EasySave_Project.Service;
using ReactiveUI;

namespace EasySave_Project.ViewModels.Pages
{
    public class SettingPageViewModel : ReactiveObject
    {
        public string SelectLanguage { get; private set; }
        public string French { get; private set; }
        public string English { get; private set; }
        public string ChooseLogsFormat { get; private set; }
        public string Json { get; private set; }
        public string Xml { get; private set; }
        
        public SettingPageViewModel()
        {
            SelectLanguage = TranslationService.GetInstance().GetText("SelectLanguage");
            French = TranslationService.GetInstance().GetText("French");
            English = TranslationService.GetInstance().GetText("English");
            ChooseLogsFormat = TranslationService.GetInstance().GetText("ChooseLogsFormat");
            Json = TranslationService.GetInstance().GetText("Json");
            Xml = TranslationService.GetInstance().GetText("Xml");
        }
    }
}
