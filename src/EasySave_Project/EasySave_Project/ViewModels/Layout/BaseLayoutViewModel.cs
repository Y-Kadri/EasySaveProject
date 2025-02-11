using System;
using ReactiveUI;
using System.Reactive;

namespace EasySave_Project.ViewModels.Layout
{
    public class BaseLayoutViewModel : ReactiveObject
    {
        private string _currentDate = string.Empty;

        public string CurrentDate
        {
            get => _currentDate;
            set => this.RaiseAndSetIfChanged(ref _currentDate, value);
        }

        public BaseLayoutViewModel()
        {
            // Met à jour la date actuelle
            CurrentDate = DateTime.Now.ToString("dd MMMM yyyy");
        }
    }
}