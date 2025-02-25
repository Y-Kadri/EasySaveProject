using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using ReactiveUI;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Dto;
using System.Reactive;
using Avalonia.Threading;
using EasySave_Project.Server;

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
        
        /// <summary>
        /// Initializes the ViewModel and loads job data.
        /// </summary>
        private readonly JobService _jobService = new JobService();
        
        private readonly TranslationService _translationService = TranslationService.GetInstance();

        private readonly GlobalDataService _globalDataService = GlobalDataService.GetInstance();
        
        private ObservableCollection<JobModel> _jobs;
        
        private string _allJobs, _addAJob, _run, _name, _source, _destination, _type, _progress, _results, PriorityExtension;
        
        public ObservableCollection<JobModel> Jobs
        {
            get => _jobs;
            private set => this.RaiseAndSetIfChanged(ref _jobs, value);
        }

        public string AllJobs
        {
            get => _allJobs;
            private set => this.RaiseAndSetIfChanged(ref _allJobs, value);
        }

        public string AddAJob
        {
            get => _addAJob;
            private set => this.RaiseAndSetIfChanged(ref _addAJob, value);
        }

        public string Run
        {
            get => _run;
            private set => this.RaiseAndSetIfChanged(ref _run, value);
        }

        public string Name
        {
            get => _name;
            private set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string Source
        {
            get => _source;
            private set => this.RaiseAndSetIfChanged(ref _source, value);
        }

        public string Destination
        {
            get => _destination;
            private set => this.RaiseAndSetIfChanged(ref _destination, value);
        }

        public string Type
        {
            get => _type;
            private set => this.RaiseAndSetIfChanged(ref _type, value);
        }

        public string Progress
        {
            get => _progress;
            private set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public string Results
        {
            get => _results;
            private set => this.RaiseAndSetIfChanged(ref _results, value);
        }

        public JobsPageViewModel()
        {
            Refresh();
        }

        /// <summary>
        /// Loads translated text values for UI elements from the translation service.
        /// </summary>
        private void LoadTranslations()
        {
            AllJobs = _translationService.GetText("AllJobs");
            AddAJob = _translationService.GetText("AddAJob");
            Run = _translationService.GetText("Run");
            Name = _translationService.GetText("Name");
            Source = _translationService.GetText("Source");
            Destination = _translationService.GetText("Destination");
            Type = _translationService.GetText("Type");
            Progress = _translationService.GetText("Progress");
            Results = _translationService.GetText("Results");
            PriorityExtension = _translationService.GetText("PriorityExtension");
        }

        /// <summary>
        /// Refreshes translations and reloads the job list.
        /// </summary>
        public void Refresh()
        {
            LoadTranslations();
            LoadJobs();
        }

        /// <summary>
        /// Loads jobs from the server if connected; otherwise, retrieves jobs from the local job service.
        /// </summary>
        private async void LoadJobs()
        {
            if (_globalDataService.isConnecte && _globalDataService.connecteTo.Item1 != null)
            {
                try
                {
                    var requestData = new 
                    { 
                        command = "GET_JOB_USERS", 
                        id = _globalDataService.connecteTo.Item1 
                    };
                    string jsonString = JsonSerializer.Serialize(requestData);
                    Utils.SendToServer(jsonString);
                    ObservableCollection<JobModel>? jobsReceived = await Utils.WaitForResponse<ObservableCollection<JobModel>>();
                    Jobs = jobsReceived ?? new ObservableCollection<JobModel>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur lors de la récupération des jobs : {ex.Message}");
                    Jobs = new ObservableCollection<JobModel>();
                }
            }
            else
            {
                Jobs = new ObservableCollection<JobModel>(_jobService.GetAllJobs() ?? new List<JobModel>());
            }
        }
        
        /// <summary>
        /// Executes multiple jobs in parallel using a thread pool, providing progress updates and notifications.
        /// </summary>
        /// <param name="jobs">The list of jobs to execute.</param>
        /// <param name="progressCallback">Callback function to update progress.</param>
        /// <param name="showPopup">Function to display notifications.</param>
        public void ExecuteJobsParallelThreadPool(List<JobModel> jobs, Action<JobModel, double> progressCallback, Action<string, string> showPopup)
        {
            int jobCount = jobs.Count;
            int completedJobs = 0;
            bool allSuccess = true;
            object lockObj = new object();

            foreach (var job in jobs)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var (success, message) = _jobService.ExecuteOneJobThreaded(job, progressCallback);

                    string notificationMessage = success
                        ? $"{_translationService.GetText("TheJob")} '{job.Name}' {_translationService.GetText("successfullyCompleted")}"
                        : $"{_translationService.GetText("TheJob")} '{job.Name}' {_translationService.GetText("failed")} : {message}";

                    string notificationType = success ? "Success" : "Error";

                    Dispatcher.UIThread.Post(() => showPopup(notificationMessage, notificationType));

                    lock (lockObj)
                    {
                        if (!success) allSuccess = false;
                        completedJobs++;

                        if (completedJobs == jobCount)
                        {
                            string finalMessage = allSuccess ? "All jobs completed successfully." : "At least one job failed.";
                            Dispatcher.UIThread.Post(() => showPopup(finalMessage, allSuccess ? "Success" : "Error"));
                        }
                    }
                });
            }
        }

        public void CancelJobInActif(JobModel jib, Action<JobModel, double> progressCallback)
        {
            JobService.CanceljobInActif(jib, progressCallback);
        }
    }
}
