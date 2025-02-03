using System;
using System.Collections.Generic;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Util;

namespace EasySave_Project.Service
{
    /// <summary>
    /// The JobService class provides methods for managing and executing backup jobs.
    /// </summary>
    public class JobService
    {

        /// <summary>
        /// Retrieves a list of all backup jobs configured in the system.
        /// Currently, the jobs are hardcoded for demonstration purposes.
        /// This method will be modified in the future to retrieve jobs from a persistent storage once the job recovery method is implemented.
        /// </summary>
        /// <returns>
        /// A list of <see cref="JobModel"/> objects representing the configured backup jobs. 
        /// Each job includes the name, source directory, target directory, type of save (differential or complete), 
        /// and the path to the last full backup if applicable.
        /// </returns>
        public List<JobModel> GetAllJob()
        {
            return JobManager.GetInstance().GetAll();
        }

        /// <summary>
        /// Executes a specific backup job.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        public void ExecuteOneJob(JobModel job)
        {
            var translator = TranslationService.GetInstance();

            // Check if the source directory exists
            if (!FileUtil.ExistsDirectory(job.FileSource))
            {
                Console.WriteLine($"{translator.GetText("directorySourceDoNotExist")} : {job.FileSource}");
                return; // Exit the method if the source directory does not exist
            }

            Console.WriteLine($"{translator.GetText("startingBackup")} : {job.Name}");

            // Create the target directory if it doesn't exist
            if (!FileUtil.ExistsDirectory(job.FileTarget))
            {
                FileUtil.CreateDirectory(job.FileTarget);
            }

            // Create a directory for the job inside the target directory
            string jobBackupDir = FileUtil.CombinePath(job.FileTarget, job.Name);
            if (!FileUtil.ExistsDirectory(jobBackupDir))
            {
                FileUtil.CreateDirectory(jobBackupDir);
            }

            // Create a directory named with job name and ID for this backup
            string backupDirectoryName = $"{job.Name}_{job.id}";
            string backupDir = FileUtil.CombinePath(jobBackupDir, backupDirectoryName);
            FileUtil.CreateDirectory(backupDir);

            // Create a timestamped subdirectory for the backup inside the job directory
            string timestampedBackupDir = FileUtil.CombinePath(backupDir, DateUtil.GetTodayDate(DateUtil.YYYY_MM_DD_HH_MM_SS));
            FileUtil.CreateDirectory(timestampedBackupDir);

            // Select the appropriate strategy based on the job type
            IJobStrategyService strategy = job.SaveType switch
            {
                JobSaveTypeEnum.COMPLETE => new JobCompleteService(), // Use complete backup strategy
                JobSaveTypeEnum.DIFFERENTIAL => new JobDifferencialService(), // Use differential backup strategy
                _ => throw new InvalidOperationException("Invalid job type") // Throw exception for invalid job type
            };

            // Execute the job using the selected strategy
            strategy.Execute(job, timestampedBackupDir);
            // Notify observers that the job has been completed
            job.NotifyObservers();
            Console.WriteLine($"{translator.GetText("backupCompleted")} : {job.Name}");
        }
    }
}
