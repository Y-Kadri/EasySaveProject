using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasySave_Project.Command
{
    public class ExecuteJobCommand : ICommand
    {
        private readonly JobService _jobService = new JobService();
        private readonly ConsoleView _consoleView = new ConsoleView();

        public void Execute()
        {
            List<JobModel> jobsList = new List<JobModel> //TODO mettre dans le service
            {
                new JobModel("Save1", "C:\\Users\\Yanis\\Desktop\\CESI_A3_S5\\GenieLogicielle\\testpourcopy", "C:\\Users\\Yanis\\Desktop\\CESI_A3_S5\\GenieLogicielle\\copy", JobSaveTypeEnum.DIFFERENTIAL, null),
                new JobModel("Save2", "C:\\Users\\Yanis\\Desktop\\CESI_A3_S5\\GenieLogicielle\\testpourcopy", "C:\\Users\\Yanis\\Desktop\\CESI_A3_S5\\GenieLogicielle\\copy", JobSaveTypeEnum.DIFFERENTIAL, "C:\\Users\\Yanis\\Desktop\\CESI_A3_S5\\GenieLogicielle\\copy\\2025_01_31_11_19_02"),
                new JobModel("Save3", "C:\\Users\\Yanis\\Desktop\\CESI_A3_S5\\GenieLogicielle\\testpourcopy", "C:\\Users\\Yanis\\Desktop\\CESI_A3_S5\\GenieLogicielle\\copyComplete", JobSaveTypeEnum.COMPLETE, null),
                new JobModel("Save4", "C:/source4", "D:/backup4", JobSaveTypeEnum.COMPLETE, null),
                new JobModel("Save5", "C:/source5", "D:/backup5", JobSaveTypeEnum.DIFFERENTIAL, null)
            };
            //TODO enlever code mort

            //List<JobModel> jobsList = _jobService.getAllJob();
            _consoleView.ShowJobList(jobsList);


            string choiceTypeExecute = _consoleView.ChoiceJob();
            List<JobModel> tasksToExecute = ParseTaskSelection(choiceTypeExecute, jobsList);

            foreach (JobModel selectedJob in tasksToExecute)
            {
                _jobService.ExecuteOneJob(selectedJob);
            }
        }
        public void GetInstruction()
        {
           //TODO a prendre en compte au moment de la refacto
        }

        private List<JobModel> ParseTaskSelection(string input, List<JobModel> jobsList)
        {
            List<JobModel> selectedJobs = new List<JobModel>();
            HashSet<int> invalidJobs = new HashSet<int>();
            int maxJobs = jobsList.Count;

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
        private bool IsRangeFormat(string input)
        {
            return Regex.IsMatch(input, @"^\d+-\d+$");
        }

        /// <summary>
        /// Processes a range selection (e.g., "1-3") and adds valid jobs to the list.
        /// </summary>
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
        private void ShowInvalidJobs(HashSet<int> invalidJobs)
        {
            foreach (int invalidJob in invalidJobs)
            {
                Console.WriteLine($"Job {invalidJob} does not exist.");
            }
        }
    }
}
