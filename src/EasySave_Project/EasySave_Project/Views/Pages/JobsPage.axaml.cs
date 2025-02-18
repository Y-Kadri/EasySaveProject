using System;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Components;
using EasySave_Project.Views.Layout;

namespace EasySave_Project.Views.Pages;

public partial class JobsPage : UserControl, IPage
{

    private TranslationService _translationService;

    public JobsPage()
    {
        InitializeComponent();
        DataContext = new JobsPageViewModel();
        _translationService = TranslationService.GetInstance();
    }

    private void Execute(object sender, RoutedEventArgs e)
    {
        var selectedJobs = new List<JobModel>();

        // Récupération des jobs sélectionnés
        foreach (var row in DataGrid.GetVisualDescendants().OfType<DataGridRow>())
        {
            var checkBox = row.GetVisualDescendants().OfType<CheckBox>().FirstOrDefault();
            if (checkBox?.IsChecked == true && row.DataContext is JobModel job)
            {
                selectedJobs.Add(job);
            }
        }

        // Vérifier si des jobs sont sélectionnés
        if (selectedJobs.Count == 0)
        {
            string message = _translationService.GetText("ErrorSelectOneJobMin");
            Toastr.ShowNotification(message, NotificationContainer);
            return;
        }

        if (DataContext is JobsPageViewModel viewModel)
        {
            viewModel.ExecuteJobsParallelThreadPool(selectedJobs,
            (job, progress) => UpdateJobProgress(job, progress),
            (msg, type) => Dispatcher.UIThread.Post(() =>
            {
                Toastr.ShowNotification(msg, NotificationContainer, type);
            }));
        }
    }

    private void UpdateJobProgress(JobModel job, double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var row = DataGrid.GetVisualDescendants()
                .OfType<DataGridRow>()
                .FirstOrDefault(r => r.DataContext == job);

            if (row != null)
            {
                var progressBar = row.GetVisualDescendants().OfType<ProgressBar>().FirstOrDefault();
                if (progressBar != null)
                {
                    progressBar.Value = progress;
                }
            }
        });
    }
    
    private void OnAddJobClick(object sender, RoutedEventArgs e)
    {
        var mainWindow = this.GetVisualRoot() as Window;

        if (mainWindow?.FindControl<BaseLayout>("MainLayout") is BaseLayout baseLayout)
        {
            // Créer un bouton temporaire avec le Tag approprié pour utiliser LoadPage
            var tempButton = new Button { Tag = "4" };
            baseLayout.LoadPage(tempButton, e);
        }
    }
    
    public void Reload()
    {
        DataContext = new JobsPageViewModel();
    }
}