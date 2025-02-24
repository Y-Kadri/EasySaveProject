using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading;
using Avalonia.Threading;
using ReactiveUI;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Server;

namespace EasySave_Project.ViewModels.Pages
{
    public class JobsPageViewModel : ReactiveObject
    {
        private ObservableCollection<JobModel> _jobs;
        public ObservableCollection<JobModel> Jobs
        {
            get => _jobs;
            private set => this.RaiseAndSetIfChanged(ref _jobs, value);
        }

        private readonly JobService _jobService = new JobService();
        private readonly TranslationService _translationService = TranslationService.GetInstance();

        private string _allJobs, _addAJob, _run, _name, _source, _destination, _type, _progress, _results;

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
            // Initialisation et écoute des changements de langue
            Refresh();
        }

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
        }

        public void Refresh()
        {
            LoadTranslations();
            LoadJobs();
        }

        public async void LoadJobs()
        {
            if (GlobalDataService.GetInstance().isConnecte && GlobalDataService.GetInstance().connecteTo.Item1 != null)
            {
                try
                {
                    // 🔵 Construire la requête JSON
                    var requestData = new 
                    { 
                        command = "GET_JOB_USERS", 
                        id = GlobalDataService.GetInstance().connecteTo.Item1 
                    };
                    string jsonString = JsonSerializer.Serialize(requestData);

                    // 📤 Envoyer la requête au serveur
                    Utils.SendToServer(jsonString);

                    // ⏳ Attendre la réponse du serveur
                    ObservableCollection<JobModel>? jobsReceived = await Utils.WaitForResponse<ObservableCollection<JobModel>>();

                    // ✅ Mise à jour de la liste des jobs
                    Jobs = jobsReceived ?? new ObservableCollection<JobModel>();

                    Console.WriteLine($"✅ {Jobs.Count} jobs reçus !");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur lors de la récupération des jobs : {ex.Message}");
                    Jobs = new ObservableCollection<JobModel>();
                }
            }
            else
            {
                // 🛠 Charger les jobs localement si non connecté
                Jobs = new ObservableCollection<JobModel>(_jobService.GetAllJobs() ?? new List<JobModel>());
            }
        }

        public void ExecuteJobsParallelThreadPool(List<JobModel> jobs, Action<JobModel, double> progressCallback, Action<string, string> showPopup)
        {
            if (jobs == null || jobs.Count == 0) return;

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
    }
}
