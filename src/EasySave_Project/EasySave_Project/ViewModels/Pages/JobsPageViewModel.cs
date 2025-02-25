using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Avalonia.Controls;
using ReactiveUI;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Dto;
using System.Reactive;

namespace EasySave_Project.ViewModels.Pages
{
    /// <summary>
    /// ViewModel for the Jobs Page.
    /// Manages the display and execution of backup jobs.
    /// </summary>
    public class JobsPageViewModel : ReactiveObject
    {
        /// <summary>
        /// Collection of backup jobs displayed in the UI.
        /// </summary>
        public ObservableCollection<JobModel> Jobs { get; }

        /// <summary>
        /// Service for managing backup jobs.
        /// </summary>

        public JobService JobService { get; } = new JobService();

        /// <summary>
        /// UI text labels retrieved from the translation service.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ExecuteCommand { get; }
        private List<PriorityExtensionDTO> PriorityExtensionFiles { get; set; }

        public string AllJobs { get; private set; }
        public string AddAJob { get; private set; }
        public string Run { get; private set; }
        public string Name { get; private set; }
        public string Source { get; private set; }
        public string Destination { get; private set; }
        public string Type { get; private set; }
        public string Action { get; private set; }
        public string Progress { get; private set; }
        public string Results { get; private set; }
        public string PriorityExtension { get; private set; }

        /// <summary>
        /// Initializes the ViewModel and loads job data.
        /// </summary>
        public JobsPageViewModel()
        {
            Jobs = new ObservableCollection<JobModel>(this.JobService.GetAllJobs());

            // Retrieve UI labels from the translation service
            AllJobs = TranslationService.GetInstance().GetText("AllJobs");
            AddAJob = TranslationService.GetInstance().GetText("AddAJob");
            Run = TranslationService.GetInstance().GetText("Run");
            Name = TranslationService.GetInstance().GetText("Name");
            Source = TranslationService.GetInstance().GetText("Source");
            Destination = TranslationService.GetInstance().GetText("Destination");
            Type = TranslationService.GetInstance().GetText("Type");
            Action = TranslationService.GetInstance().GetText("Action");
            Progress = TranslationService.GetInstance().GetText("Progress");
            Results = TranslationService.GetInstance().GetText("Results");
            PriorityExtension = TranslationService.GetInstance().GetText("PriorityExtension");
        }

        /// <summary>
        /// Executes a list of backup jobs in parallel using a thread pool.
        /// </summary>
        /// <param name="jobs">The list of jobs to execute.</param>
        /// <param name="progressCallback">Callback function to update job progress.</param>
        /// <param name="showPopup">Callback function to display status messages.</param>
        public void ExecuteJobsParallelThreadPool(List<JobModel> jobs, Action<JobModel, double> progressCallback, Action<string, string> showPopup)
        {
            int jobCount = jobs.Count; // Total number of jobs to process
            int completedJobs = 0;     // Counter to keep track of completed jobs
            object lockObj = new object(); // Lock object for thread safety
            bool allSuccess = true;    // Flag to track if all jobs were successful

            // Iterate over each job and add it to the ThreadPool for parallel execution
            foreach (var job in jobs)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    // Execute the job in a separate thread and get the result
                    var (success, message) = JobService.ExecuteOneJobThreaded(job, progressCallback);

                    // Prepare the message to display when the job is finished
                    string notificationMessage = success
                        ? $"{message} "
                        : $"{TranslationService.GetInstance().GetText("TheJob")} '{job.Name}' {TranslationService.GetInstance().GetText("failed")} : {message}";

                    string notificationType = success ? "Success" : "Error";

                    // Update the UI using the Dispatcher to show a popup with the job result
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => showPopup(notificationMessage, notificationType));

                    // Ensure thread-safe updates to shared variables
                    lock (lockObj)
                    {
                        if (!success) allSuccess = false; // If any job fails, mark allSuccess as false
                        completedJobs++; // Increment completed job count
                    }
                });
            }
        }
    }
}
