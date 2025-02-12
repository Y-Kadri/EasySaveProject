using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;

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
        
        public void ExecuteJobs(List<JobModel> jobs)
        {
            foreach (var job in jobs)
            {
                this.JobService.ExecuteOneJob(job);
            }
        }
        
        public async Task ExecuteJobsAsync(List<JobModel> jobs, Action<JobModel, double> progressCallback)
        {
            foreach (var job in jobs)
            {
                await JobService.ExecuteOneJobAsync(job, progressCallback);
            }
        }

    }
}
