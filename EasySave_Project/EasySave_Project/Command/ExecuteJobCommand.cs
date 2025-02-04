using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Util;
using EasySave_Project.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasySave_Project.Command
{
    /// <summary>
    /// Command to execute jobs based on user selection.
    /// </summary>
    public class ExecuteJobCommand : ICommand
    {
        private readonly JobService _jobService = new JobService();
        private readonly ConsoleView _consoleView = new ConsoleView();

        /// <summary>
        /// Executes the job command, displaying job list and executing selected jobs.
        /// </summary>
        public void Execute()
        {
            List<JobModel> jobsList = _jobService.GetAllJobs();
            _consoleView.ShowJobList(jobsList);

            GetInstruction();

            string choiceTypeExecute = _consoleView.ChoiceJob();
            List<JobModel> tasksToExecute = ParseTaskSelection(choiceTypeExecute, jobsList);

            foreach (JobModel selectedJob in tasksToExecute)
            {
                _jobService.ExecuteOneJob(selectedJob);
            }
        }

        /// <summary>
        /// Displays instructions for executing a job.
        /// </summary>
        public void GetInstruction()
        {
            ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("executeJobCommand"));
        }

        /// <summary>
        /// Parses user input to select jobs for execution.
        /// </summary>
        /// <param name="input">User input string</param>
        /// <param name="jobsList">List of available jobs</param>
        /// <returns>List of selected JobModel instances</returns>
        private List<JobModel> ParseTaskSelection(string input, List<JobModel> jobsList)
        {
            List<JobModel> selectedJobs = new List<JobModel>();
            HashSet<int> invalidJobs = new HashSet<int>();

            // Split input into separate selections
            string[] parts = input.Split(';');
            foreach (string part in parts)
            {
                if (IsRangeFormat(part))
                {
                    ProcessRangeSelection(part, jobsList, selectedJobs, invalidJobs);
                }
                else
                {
                    ProcessSingleSelection(part, jobsList, selectedJobs, invalidJobs);
                }
            }

            // Display invalid job numbers
            ShowInvalidJobs(invalidJobs);

            return selectedJobs.Distinct().ToList();
        }

        /// <summary>
        /// Checks if the given input matches a range format (e.g., "1-3").
        /// </summary>
        /// <param name="input">User input string</param>
        /// <returns>True if input is a range format, false otherwise</returns>
        private bool IsRangeFormat(string input)
        {
            return Regex.IsMatch(input, @"^\d+-\d+$");
        }

        /// <summary>
        /// Processes a range selection (e.g., "1-3") and adds valid jobs to the list.
        /// </summary>
        /// <param name="rangeInput">User input range string</param>
        /// <param name="jobsList">List of available jobs</param>
        /// <param name="selectedJobs">List of selected jobs</param>
        /// <param name="invalidJobs">Set of invalid job numbers</param>
        private void ProcessRangeSelection(string rangeInput, List<JobModel> jobsList, List<JobModel> selectedJobs, HashSet<int> invalidJobs)
        {
            string[] range = rangeInput.Split('-');

            if (int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
            {
                if (start > end)
                    (start, end) = (end, start); // Swap values if reversed

                for (int i = start; i <= end; i++)
                {
                    AddJobToList(i, jobsList, selectedJobs, invalidJobs);
                }
            }
        }

        /// <summary>
        /// Processes a single job selection (e.g., "2") and adds it to the list if valid.
        /// </summary>
        /// <param name="input">User input string</param>
        /// <param name="jobsList">List of available jobs</param>
        /// <param name="selectedJobs">List of selected jobs</param>
        /// <param name="invalidJobs">Set of invalid job numbers</param>
        private void ProcessSingleSelection(string input, List<JobModel> jobsList, List<JobModel> selectedJobs, HashSet<int> invalidJobs)
        {
            if (int.TryParse(input, out int jobNumber))
            {
                AddJobToList(jobNumber, jobsList, selectedJobs, invalidJobs);
            }
        }

        /// <summary>
        /// Adds a job to the selected list if it exists, otherwise marks it as invalid.
        /// </summary>
        /// <param name="jobNumber">Job number</param>
        /// <param name="jobsList">List of available jobs</param>
        /// <param name="selectedJobs">List of selected jobs</param>
        /// <param name="invalidJobs">Set of invalid job numbers</param>
        private void AddJobToList(int jobNumber, List<JobModel> jobsList, List<JobModel> selectedJobs, HashSet<int> invalidJobs)
        {
            if (jobNumber >= 1 && jobNumber <= jobsList.Count)
            {
                selectedJobs.Add(jobsList[jobNumber - 1]);
            }
            else
            {
                invalidJobs.Add(jobNumber);
            }
        }

        /// <summary>
        /// Displays a message for each invalid job selection.
        /// </summary>
        /// <param name="invalidJobs">Set of invalid job numbers</param>
        private void ShowInvalidJobs(HashSet<int> invalidJobs)
        {
            foreach (int invalidJob in invalidJobs)
            {
                Console.WriteLine($"Job {invalidJob} does not exist.");
            }
        }
    }
}