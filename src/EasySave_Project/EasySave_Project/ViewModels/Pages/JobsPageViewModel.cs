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
    public class JobsPageViewModel : ReactiveObject
    {
        public ObservableCollection<JobModel> Jobs { get; }

        public JobService JobService { get; } = new JobService();
        public ReactiveCommand<Unit, Unit> ExecuteCommand { get; }
        private List<PriorityExtensionDTO> PriorityExtensionFiles { get; set; }

        public string AllJobs { get; private set; }
        public string AddAJob { get; private set; }
        public string Run { get; private set; }

        public string Name { get; private set; }
        public string Source { get; private set; }
        public string Destination { get; private set; }
        public string Type { get; private set; }
        public string Progress { get; private set; }
        public string Results { get; private set; }
        public string PriorityExtension { get; private set; }

        public JobsPageViewModel()
        {
            Jobs = new ObservableCollection<JobModel>(this.JobService.GetAllJobs());
            AllJobs = TranslationService.GetInstance().GetText("AllJobs");
            AddAJob = TranslationService.GetInstance().GetText("AddAJob");
            Run = TranslationService.GetInstance().GetText("Run");
            Name = TranslationService.GetInstance().GetText("Name");
            Source = TranslationService.GetInstance().GetText("Source");
            Destination = TranslationService.GetInstance().GetText("Destination");
            Type = TranslationService.GetInstance().GetText("Type");
            Progress = TranslationService.GetInstance().GetText("Progress");
            Results = TranslationService.GetInstance().GetText("Results");
            PriorityExtension = TranslationService.GetInstance().GetText("PriorityExtension");
        }

        public void LoadAppSettings()
        {
            string settingsPath = @"C:\path\to\your\appsettings.json";  // Remplace avec le bon chemin
            string json = File.ReadAllText(settingsPath);

            // Désérialiser le fichier JSON avec System.Text.Json
            var appSettings = JsonSerializer.Deserialize<AppSettingDto>(json);

            // Charger les extensions de fichiers prioritaires, triées par index
            PriorityExtensionFiles = appSettings.PriorityExtensionFiles.OrderBy(p => p.Index).ToList();
        }

        public void ExecuteJobsParallelThreadPool(List<JobModel> jobs, Action<JobModel, double> progressCallback, Action<string, string> showPopup)
        {
            int jobCount = jobs.Count; // Total number of jobs to process
            int completedJobs = 0;     // Counter to keep track of how many jobs are completed
            object lockObj = new object(); // Lock object to ensure thread safety when updating shared variables
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
                        ? $"{TranslationService.GetInstance().GetText("TheJob")} '{job.Name}' {TranslationService.GetInstance().GetText("successfullyCompleted")} "
                        : $"{TranslationService.GetInstance().GetText("TheJob")} '{job.Name}' {TranslationService.GetInstance().GetText("failed")} : {message}";

                    string notificationType = success ? "Success" : "Error";

                    // Update the UI using the Dispatcher to show a popup with the result of the job
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => showPopup(notificationMessage, notificationType));

                    lock (lockObj) // Ensure thread-safe updates to shared variables
                    {
                        if (!success) allSuccess = false; // If any job fails, set allSuccess to false
                        completedJobs++; // Increment the completed jobs counter

                        // When all jobs are completed, display a final message indicating the overall result
                        if (completedJobs == jobCount)
                        {
                            string finalMessage = allSuccess
                                ? "All jobs completed successfully."
                                : "At least one job failed.";
                            Avalonia.Threading.Dispatcher.UIThread.Post(() => showPopup(finalMessage, allSuccess ? "Success" : "Error"));
                        }
                    }
                });
            }
        }
    }
}
