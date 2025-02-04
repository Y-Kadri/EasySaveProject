using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Command
{
    public class ExecuteAllJobCommand : ICommand
    {
        private readonly JobService _jobService = new JobService();
        private readonly ConsoleView _consoleView = new ConsoleView();

        public void Execute()
        {
            // Retrieve the list of all jobs
            List<JobModel> jobsList = _jobService.GetAllJobs();

            // Display the list of all jobs
            _consoleView.ShowJobList(jobsList);

            // Execute all jobs without any user selection
            foreach (JobModel selectedJob in jobsList)
            {
                _jobService.ExecuteOneJob(selectedJob);
            }
        }

        public void GetInstruction()
        {
            //TODO to be considered when refactoring
        }
    }
}
