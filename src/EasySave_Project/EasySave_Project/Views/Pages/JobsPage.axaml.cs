using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using EasySave_Project.Model;
using EasySave_Project.Server;
using EasySave_Project.Service;
using EasySave_Project.ViewModels.Pages;
using EasySave_Project.Views.Components;
using EasySave_Project.Views.Layout;

namespace EasySave_Project.Views.Pages
{
    public partial class JobsPage : UserControl, IPage
    {
        private readonly JobsPageViewModel _viewModel;
        private readonly TranslationService _translationService;

        /// <summary>
        /// Initializes the JobsPage, sets up the ViewModel, and assigns the DataContext.
        /// </summary>
        public JobsPage()
        {
            InitializeComponent();
            _viewModel = new JobsPageViewModel();
            DataContext = _viewModel;
            _translationService = TranslationService.GetInstance();
        }

        /// <summary>
        /// Executes selected jobs, either locally or by sending a request to the server.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void Execute(object sender, RoutedEventArgs e)
        {
            var selectedJobs = DataGrid.GetVisualDescendants()
                .OfType<DataGridRow>()
                .Where(row => row.GetVisualDescendants().OfType<CheckBox>().FirstOrDefault()?.IsChecked == true)
                .Select(row => row.DataContext as JobModel)
                .Where(job => job != null)
                .ToList();

            if (selectedJobs.Count == 0)
            {
                Toastr.ShowNotification(_translationService.GetText("ErrorSelectOneJobMin"), NotificationContainer);
                return;
            }
            
            if (GlobalDataService.GetInstance().isConnecte && GlobalDataService.GetInstance().connecteTo.Item1 != null)
            {
                // 🔵 Construire la requête JSON
                var requestData = new 
                { 
                    command = "RUN_JOB_USERS", 
                    id = GlobalDataService.GetInstance().connecteTo.Item1,
                    obj = selectedJobs
                };
                string jsonString = JsonSerializer.Serialize(requestData);
                
                Toastr.ShowServeurNotification($"{_translationService.GetText("Envoiedesjobsàrun")} ...", NotificationContainer);
                Utils.SendToServer(jsonString);
            }
            else
            {
                _viewModel.ExecuteJobsParallelThreadPool(selectedJobs, UpdateJobProgress, (msg, type) =>
                {
                    Dispatcher.UIThread.Post(() => Toastr.ShowNotification(msg, NotificationContainer, type));
                });
            }
        }
        
        /// <summary>
        /// Handles the click event to navigate to the Add Job page.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The routed event arguments.</param>
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

        // <summary>
        /// Updates the progress bar of a job in the UI.
        /// </summary>
        /// <param name="job">The job whose progress is being updated.</param>
        /// <param name="progress">The current progress value.</param>
        private void UpdateJobProgress(JobModel job, double progress)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var row = DataGrid.GetVisualDescendants()
                    .OfType<DataGridRow>()
                    .FirstOrDefault(r => r.DataContext == job);

                row?.GetVisualDescendants().OfType<ProgressBar>().FirstOrDefault()?.SetValue(ProgressBar.ValueProperty, progress);
            });
        }

        /// <summary>
        /// Reloads the job page, adjusting UI elements based on connection status.
        /// </summary>
        public void Reload()
        {
            if (GlobalDataService.GetInstance().isConnecte && GlobalDataService.GetInstance().connecteTo.Item1 != null)
            {
                buttonAdd.IsVisible = false;
            }
            else
            {
                buttonAdd.IsVisible = true;
            }
            _viewModel.Refresh(); 
        }
    }
}
