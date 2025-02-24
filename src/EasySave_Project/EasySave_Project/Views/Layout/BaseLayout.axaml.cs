using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Layout;
using EasySave_Project.Views.Pages;

namespace EasySave_Project.Views.Layout
{
    public partial class BaseLayout : UserControl
    {
        private readonly List<IPage> pages = new();

        public BaseLayout()
        {
            InitializeComponent();
            DataContext = BaseLayoutViewModel.Instance;

            BaseLayoutViewModel.Instance.NotificationContainer = NotificationContainer;

            pages.Add(new HomePage());
            pages.Add(new JobsPage());
            pages.Add(new LogsPage());
            pages.Add(new SettingPage(this));
            pages.Add(new AddJobsPage());
            pages.Add(new ConnexionPage(this));

            ContentArea.Content = pages[0];
        }

        public void Reload()
        {
            Button logsButton = labelLogs;
            Button SettingButton = labelSetting;
            
            BaseLayoutViewModel.RefreshInstance(logsButton, SettingButton);
        }

        public void LoadPage(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int index))
            {
                pages[index].Reload();
                ContentArea.Content = pages[index];
            }
        }

        public void SetContent(Control content)
        {
            ContentArea.Content = content;
        }
    }
}