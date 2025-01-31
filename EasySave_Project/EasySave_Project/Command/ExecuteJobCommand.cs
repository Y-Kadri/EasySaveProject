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
            List<JobModel> jobsList = new List<JobModel>
            {
                new JobModel("Save1", "C:\\Users\\Yanis\\Desktop\\CESI_A3_S5\\GenieLogicielle\\testpourcopy", "C:\\Users\\Yanis\\Desktop\\CESI_A3_S5\\GenieLogicielle\\copy", JobSaveTypeEnum.COMPLETE),
                new JobModel("Save2", "C:/source2", "D:/backup2", JobSaveTypeEnum.COMPLETE),
                new JobModel("Save3", "C:/source3", "D:/backup3", JobSaveTypeEnum.COMPLETE),
                new JobModel("Save4", "C:/source4", "D:/backup4", JobSaveTypeEnum.COMPLETE),
                new JobModel("Save5", "C:/source5", "D:/backup5", JobSaveTypeEnum.DIFFERENTIAL)
            };

            MethASupp(jobsList); //TODO utiliser getistruction

            string choiceTypeExecute = _consoleView.ChoiceJob();
            List<JobModel> tasksToExecute = ParseTaskSelection(choiceTypeExecute, jobsList);

            foreach (JobModel selectedJob in tasksToExecute)
            {
                Console.WriteLine(selectedJob.Name);
                _jobService.ExecuteOneJob(selectedJob);
            }
        }

        public void MethASupp(List<JobModel> jobList) //TODO méthode a supprimer
        {
            _consoleView.ShowJobList(jobList);
        }

        public void GetInstruction()
        {
           // _consoleView.ShowJobList(jobList);
        }

        private List<JobModel> ParseTaskSelection(string input, List<JobModel> jobsList)
        {
            List<JobModel> selectedJobs = new List<JobModel>();
            HashSet<int> invalidJobs = new HashSet<int>();
            int maxJobs = jobsList.Count;

            string[] parts = input.Split(';');
            foreach (string part in parts)
            {
                if (Regex.IsMatch(part, "^\\d+-\\d+$")) // Range like 1-3
                {
                    string[] range = part.Split('-');
                    if (int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                    {
                        if (start > end)
                        {
                            (start, end) = (end, start); // Swap if reversed
                        }

                        for (int i = start; i <= end; i++)
                        {
                            if (i >= 1 && i <= maxJobs)
                            {
                                selectedJobs.Add(jobsList[i - 1]);
                            }
                            else
                            {
                                invalidJobs.Add(i);
                            }
                        }
                    }
                }
                else if (int.TryParse(part, out int jobNumber)) // Single number
                {
                    if (jobNumber >= 1 && jobNumber <= maxJobs)
                    {
                        selectedJobs.Add(jobsList[jobNumber - 1]);
                    }
                    else
                    {
                        invalidJobs.Add(jobNumber);
                    }
                }
            }

            foreach (int invalidJob in invalidJobs)
            {
                Console.WriteLine($"Save {invalidJob} n'existe pas.");
            }

            return selectedJobs.Distinct().ToList();
        }
    }
}
