using System;
using EasySave_Project.Service;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using System.Collections.Generic;

namespace EasySave_Project.Command
{
    public class ListJobCommand : ICommand
    {
        private readonly JobService _jobService;

        public ListJobCommand()
        {
            _jobService = new JobService();
        }

        public void Execute()
        {
            List<JobModel> jobs = _jobService.GetJobs();

            if (jobs.Count == 0)
            {
                Console.WriteLine("Aucun job trouvé.");
            }
            else
            {
                Console.WriteLine("Liste des jobs :");
                foreach (var job in jobs)
                {
                    Console.WriteLine($"- {job.Name}");
                }
            }
        }

        public void GetInstruction()
        {
            Console.WriteLine("Affiche la liste des jobs.");
        }
    }
}