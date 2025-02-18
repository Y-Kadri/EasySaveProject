using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using Avalonia.Controls.Notifications;

namespace EasySave_Project.ViewModels.Pages
{
    
    public class JobsPageViewModel : ReactiveObject
    {
        
        public ObservableCollection<JobModel> Jobs { get; }
        
        public JobService JobService { get; } = new JobService();
        
        public string AllJobs { get; private set; }
        public string AddAJob { get; private set; }
        public string Run { get; private set; }
        
        public string Name { get; private set; }
        public string Source { get; private set; }
        public string Destination { get; private set; }
        public string Type { get; private set; }
        public string Progress { get; private set; }
        public string Results { get; private set; }
        
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
        }
        
        public async Task<(bool, string)> ExecuteJobsAsync(List<JobModel> jobs, Action<JobModel, double> progressCallback, Action<string, string> showPopup)
        {
            bool allSuccess = true;



            foreach (var job in jobs)
            {
                var (success, message) = await JobService.ExecuteOneJobAsync(job, progressCallback);

                // Affiche un pop-up dès que le job est terminé
                string notificationMessage = success
                    ? $"{TranslationService.GetInstance().GetText("TheJob")} '{job.Name}' {TranslationService.GetInstance().GetText("successfullyCompleted")} "
                    : $"{TranslationService.GetInstance().GetText("TheJob")} '{job.Name}' {TranslationService.GetInstance().GetText("failed")} : {message}";

                string notificationType = success ? "Success" : "Error";

                // Afficher le pop-up en temps réel
                showPopup(notificationMessage, notificationType);

                // Mettre à jour le statut global
                if (!success) allSuccess = false;
            }

            return (allSuccess, allSuccess ? "Tous les jobs ont réussi." : "Au moins un job a échoué.");
        }

    }
}
