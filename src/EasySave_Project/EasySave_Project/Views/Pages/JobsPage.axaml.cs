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

    /// <summary>
    /// Handles the execution of selected backup jobs.
    /// </summary>
    /// <param name="sender">The event sender (usually a button).</param>
    /// <param name="e">Event arguments.</param>
    private void Execute(object sender, RoutedEventArgs e)
    {
        var selectedJobs = new List<JobModel>();

        // Retrieve selected jobs from the DataGrid
        foreach (var row in DataGrid.GetVisualDescendants().OfType<DataGridRow>())
        {
            var checkBox = row.GetVisualDescendants().OfType<CheckBox>().FirstOrDefault();

            // Ensure DataContext is a JobModel
            if (row.DataContext is JobModel job)
            {
                // If the checkbox is checked, add the job to the selected list
                if (checkBox?.IsChecked == true)
                {
                    if (job.SaveState.Equals(JobSaveStateEnum.ACTIVE))
                    {
                        Toastr.ShowNotification($"The job {job.Name} is already running", NotificationContainer, "warning");
                    }
                    else
                    {
                        checkBox.IsChecked = false;
                        selectedJobs.Add(job);
                    }
                }
            }
        }

        // Ensure at least one job is selected
        if (selectedJobs.Count == 0)
        {
            string message = _translationService.GetText("ErrorSelectOneJobMin");
            Toastr.ShowNotification(message, NotificationContainer);
            return;
        }

        executeJobList(selectedJobs);
    }

    /// <summary>
    /// Updates the progress bar of a specific job in the UI.
    /// </summary>
    /// <param name="job">The JobModel representing the job being updated.</param>
    private void UpdateJobProgress(JobModel job)
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
                    progressBar.Value = job.FileInPending.Progress;
                }
            }
        });
    }

    /// <summary>
    /// Handles the event when the "Add Job" button is clicked.
    /// Loads the job creation page.
    /// </summary>
    /// <param name="sender">The event sender (usually a button).</param>
    /// <param name="e">Event arguments.</param>
    private void OnAddJobClick(object sender, RoutedEventArgs e)
    {
        var mainWindow = this.GetVisualRoot() as Window;

        if (mainWindow?.FindControl<BaseLayout>("MainLayout") is BaseLayout baseLayout)
        {
            // Create a temporary button with the appropriate Tag to use LoadPage
            var tempButton = new Button { Tag = "4" };
            baseLayout.LoadPage(tempButton, e);
        }
    }

    /// <summary>
    /// Reloads the page by resetting the DataContext.
    /// </summary>
    public void Reload()
    {
        DataContext = new JobsPageViewModel();
    }

    /// <summary>
    /// Sets the state of a job to "Pending" when the corresponding button is clicked.
    /// </summary>
    /// <param name="sender">The event sender (usually a button).</param>
    /// <param name="e">Event arguments.</param>
    private void OnPendingJob(object sender, RoutedEventArgs e)
    {
        // Retrieve the clicked button
        var button = sender as Button;

        // Ensure the CommandParameter is a JobModel
        if (button?.CommandParameter is JobModel job)
        {
            job.SaveState = JobSaveStateEnum.PENDING;
        }
    }

    /// <summary>
    /// Executes a list of selected jobs using a thread pool for parallel execution.
    /// </summary>
    /// <param name="selectedJobs">The list of jobs to execute.</param>
    private void executeJobList(List<JobModel> selectedJobs)
    {
        if (DataContext is JobsPageViewModel viewModel)
        {
            viewModel.ExecuteJobsParallelThreadPool(selectedJobs,
            (job, progress) => UpdateJobProgress(job),
            (msg, type) => Dispatcher.UIThread.Post(() =>
            {
                Toastr.ShowNotification(msg, NotificationContainer, type);
            }));
        }
    }

    /// <summary>
    /// Resumes a paused job when the corresponding button is clicked.
    /// </summary>
    /// <param name="sender">The event sender (usually a button).</param>
    /// <param name="e">Event arguments.</param>
    private void OnResumeJob(object sender, RoutedEventArgs e)
    {
        // Retrieve the clicked button
        var button = sender as Button;

        // Ensure the CommandParameter is a JobModel
        if (button?.CommandParameter is JobModel job)
        {
            Toastr.ShowNotification(TranslationService.GetInstance().GetText("resumejob") + job.Name, NotificationContainer, "Success");
            executeJobList(new List<JobModel> { job });
        }
    }

    /// <summary>
    /// Stop job when the corresponding button is clicked.
    /// </summary>
    /// <param name="sender">The event sender (usually a button).</param>
    /// <param name="e">Event arguments.</param>
    private void OnCancelJob(object sender, RoutedEventArgs e)
    {
        // Retrieve the clicked button
        var button = sender as Button;

        // Ensure the CommandParameter is a JobModel
        if (button?.CommandParameter is JobModel job)
        {
            job.SaveState = JobSaveStateEnum.CANCEL;
            if (DataContext is JobsPageViewModel viewModel)
            {
                viewModel.CancelJobInActif(job, (job, progress) => UpdateJobProgress(job));
            }
            
        }
    }
}
