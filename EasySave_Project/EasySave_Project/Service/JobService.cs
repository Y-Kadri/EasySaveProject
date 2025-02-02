using System;
using System.Collections.Generic;
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
                FileUtil.CreateDirectory(job.FileTarget); // Create target directory
            }

            // Create a timestamped subdirectory for the backup
            string timestampedBackupDir = FileUtil.CombinePath(job.FileTarget, DateUtil.GetTodayDate(DateUtil.YYYY_MM_DD_HH_MM_SS));
            FileUtil.CreateDirectory(timestampedBackupDir); // Create backup subdirectory

            // Select the appropriate strategy based on the job type
            IJobStrategy strategy = job.SaveType switch
            {
                JobSaveTypeEnum.COMPLETE => new JobCompleteService(), // Use complete backup strategy
                JobSaveTypeEnum.DIFFERENTIAL => new JobDifferencialService(), // Use differential backup strategy
                _ => throw new InvalidOperationException("Invalid job type") // Throw exception for invalid job type
            };

            // Execute the job using the selected strategy
            strategy.Execute(job, timestampedBackupDir);

            Console.WriteLine($"{translator.GetText("backupCompleted")} : {job.Name}");
        }
    }
}
