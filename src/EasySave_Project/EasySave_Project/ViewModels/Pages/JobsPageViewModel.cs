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
        
        public JobsPageViewModel()
        {
            Jobs = new ObservableCollection<JobModel>(this.JobService.GetAllJobs());
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
