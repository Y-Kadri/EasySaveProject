using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Util;
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

        /// <summary>
        /// Executes all jobs automatically without requiring user input.
        /// </summary>
        public void Execute()
        {
            // Retrieve the list of all jobs
            List<JobModel> jobsList = _jobService.GetAllJobs();

            // Display the list of all jobs in the console
            _consoleView.ShowJobList(jobsList);

            // Execute all jobs without any user selection
            foreach (JobModel selectedJob in jobsList)
            {
                // Execute each job individually
                _jobService.ExecuteOneJob(selectedJob);
            }
        }

        /// <summary>
        /// Displays an instruction message to the user about the execution of all jobs.
        /// </summary>
        public void GetInstruction()
        {
            // Get the translated instruction text for the execute all jobs command
            ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("executeallJobCommand"));
        }
    }
}
