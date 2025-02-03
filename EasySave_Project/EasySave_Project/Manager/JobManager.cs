using System;
using System.Collections.Generic;
using EasySave_Project.Model;
using EasySave_Project.Util; // Import ConsoleUtil
using EasySave_Project.Service; // Import TranslationService

namespace EasySave_Project.Manager
{
    /// <summary>
    /// Singleton class that manages a list of JobModel instances.
    /// Ensures only one instance of JobManager exists throughout the application.
    /// </summary>
    public sealed class JobManager
    {
        // Static instance of JobManager (Singleton pattern)
        private static JobManager jobManager = null;
 
        /// <summary>
        /// List containing all backup jobs.
        /// </summary>
        public List<JobModel> Jobs { get; private set; }

        /// <summary>
        /// Retrieves the single instance of JobManager.
        /// If the instance does not exist, it is created.
        /// </summary>
        /// <returns>The singleton instance of JobManager.</returns>
        public static JobManager GetInstance()
        {
            if (jobManager == null) // Check if instance is null
            {
                jobManager = new JobManager(); // Create instance
                jobManager.Jobs = new List<JobModel>(); // Initialize the job list
            }
            return jobManager; // Return the singleton instance
        }

        /// <summary>
        /// Adds a new job to the job list.
        /// </summary>
        /// <param name="jobModel">The job to add.</param>
        public void AddJob(JobModel jobModel)
        {
            this.Jobs.Add(jobModel); // Add job to the list
        }

        /// <summary>
        /// Creates a new job using the JobFactory and adds it to the job list.
        /// </summary>
        /// <param name="name">Job name.</param>
        /// <param name="fileSource">Source directory.</param>
        /// <param name="fileTarget">Target directory.</param>
        /// <param name="jobSaveTypeEnum">Type of backup (complete or differential).</param>
        /// <returns>The created job, or null if an error occurs.</returns>
        public JobModel CreateAndAddJob(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum)
        {
            try
            {
                // Create the job using the JobFactory
                JobModel job = JobFactory.CreateJobModel(name, fileSource, fileTarget, jobSaveTypeEnum);

                // Subscribe to the job with the LogService and StateService observers
                job.Subscribe(new LogService());
                job.Subscribe(new StateService());

                // Add the job to the list
                Jobs.Add(job);

                // Display success message
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("jobCree"));
                return job; // Return the created job
            }
            catch (ArgumentException ex)
            {
                // Display error message
                ConsoleUtil.PrintTextconsole($"{TranslationService.GetInstance().GetText("invalidChoice")} : {ex.Message}");
                return null; // Return null in case of failure
            }
        }

        /// <summary>
        /// Retrieves all jobs from the list.
        /// If the job list is empty, it displays a message and initializes the list.
        /// </summary>
        /// <returns>List of all jobs.</returns>
        public List<JobModel> GetAll()
        {
            if (Jobs == null || Jobs.Count == 0) // Check if Jobs list is null or empty
            {
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("listEmpty"));
                jobManager.Jobs = new List<JobModel>(); // Initialize the list if needed
            }
            return Jobs; // Return the job list
        }

        public JobModel? GetJobById(int jobId)
        {
            return Jobs.Find(job => job.Id == jobId);
        }
    }
}
