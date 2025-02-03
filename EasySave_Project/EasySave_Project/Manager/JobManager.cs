using System;
using System.Collections.Generic;
using EasySave_Project.Model;

namespace EasySave_Project.Manager
{
    public sealed class JobManager
    {
        private static JobManager jobManager = null;

        public List<JobModel> Jobs { get; private set; }

        public static JobManager GetInstance()
        {
            if (jobManager == null)
            {
                jobManager = new JobManager();
                jobManager.Jobs = new List<JobModel>();
            }
            return jobManager;
        }

        public void AddJob(JobModel jobModel)
        {
            this.Jobs.Add(jobModel);
        }

        public JobModel CreateAndAddJob(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum)
        {
            try
            {
                // Créer le job via la JobFactory
                JobModel job = JobFactory.CreateJobModel(name, fileSource, fileTarget, jobSaveTypeEnum);
                // Ajouter le job à la liste
                Jobs.Add(job);
                return job;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Erreur lors de la création du job : {ex.Message}");
                return null;
            }
        }

        public JobModel? GetJobById(int jobId)
        {
            return Jobs.Find(job => job.id == jobId);
        }
    }
}