using System;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using EasySave_Project.Model;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Components;
using EasySave_Project.Views.Layout;

namespace EasySave_Project.Views.Pages;

public partial class JobsPage : UserControl, IPage
{

    public JobsPage()
    {
        InitializeComponent();
        DataContext = new JobsPageViewModel(); 
    }

    private async void Execute(object sender, RoutedEventArgs e)
    {
        var selectedJobs = new List<JobModel>();

        // Parcourt les descendants visuels pour trouver les DataGridRow
        foreach (var row in DataGrid.GetVisualDescendants().OfType<DataGridRow>())
        {
            var checkBox = row.GetVisualDescendants().OfType<CheckBox>().FirstOrDefault();

            if (checkBox != null && checkBox.IsChecked == true)
            {
                if (row.DataContext is JobModel job)
                {
                    selectedJobs.Add(job);
                }
            }
        }

        if (selectedJobs.Count == 0)
        {
            Toastr.ShowNotification("Erreur : Veuillez sélectionner au moins un job.", NotificationContainer);
            return;
        }

        if (DataContext is JobsPageViewModel viewModel)
        {
            await viewModel.ExecuteJobsAsync(selectedJobs, (job, progress) =>
            {
                // Mettre à jour la ProgressBar dans le DataGrid pour chaque job
                UpdateJobProgress(job, progress);
            });

            Toastr.ShowNotification($"{selectedJobs.Count} job(s) exécuté(s) avec succès.",  NotificationContainer, "Succès");
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