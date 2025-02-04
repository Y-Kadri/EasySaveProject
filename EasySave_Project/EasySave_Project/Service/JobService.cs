using System;
using System.Collections.Generic;
using System.Text.Json;
using EasySave_Library_Log;
using EasySave_Project.Dto;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Util;
using EasySave_Project.View;

namespace EasySave_Project.Service
{
    public class JobService
    {
        public List<JobModel> GetAllJobs()
        {
            return JobManager.GetInstance().GetAll();
        }

        public void ExecuteOneJob(JobModel job)
        {
            var translator = TranslationService.GetInstance();

            // Check if the source directory exists
            if (!FileUtil.ExistsDirectory(job.FileSource))
            {
                Console.WriteLine($"{translator.GetText("directorySourceDoNotExist")} : {job.FileSource}");
                return; // Exit if source directory does not exist
            }

            Console.WriteLine($"{translator.GetText("startingBackup")} : {job.Name}");

            // Create target directory if it doesn't exist
            if (!FileUtil.ExistsDirectory(job.FileTarget))
            {
                FileUtil.CreateDirectory(job.FileTarget);
            }

            // Create a job-specific backup directory
            string jobBackupDir = FileUtil.CombinePath(job.FileTarget, job.Name);
            if (!FileUtil.ExistsDirectory(jobBackupDir))
            {
                FileUtil.CreateDirectory(jobBackupDir);
            }

            // Create a timestamped subdirectory for the backup
            string timestampedBackupDir = FileUtil.CombinePath(jobBackupDir, DateUtil.GetTodayDate(DateUtil.YYYY_MM_DD_HH_MM_SS));
            FileUtil.CreateDirectory(timestampedBackupDir);

            // Update job state to ACTIVE
            job.SaveState = JobSaveStateEnum.ACTIVE;
            StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, job.FileSource, string.Empty));

            // Select the appropriate strategy based on the job type
            IJobStrategyService strategy = job.SaveType switch
            {
                JobSaveTypeEnum.COMPLETE => new JobCompleteService(),
                JobSaveTypeEnum.DIFFERENTIAL => new JobDifferencialService(),
                _ => throw new InvalidOperationException("Invalid job type")
            };

            // Execute the job using the selected strategy
            strategy.Execute(job, timestampedBackupDir);

            // Update job state to END after execution
            job.SaveState = JobSaveStateEnum.END;
            StateManager.Instance.UpdateState(CreateBackupJobState(job, 100, string.Empty, string.Empty));

            ConsoleUtil.PrintTextconsole($"{translator.GetText("backupCompleted")} : {job.Name}");

            // Update job settings
            UpdateJobInFile(job);
        }

        private BackupJobState CreateBackupJobState(JobModel job, double progress, string currentSourceFilePath, string currentDestinationFilePath)
        {
            long totalSize = FileUtil.CalculateTotalSize(job.FileSource);
            return new BackupJobState
            {
                JobName = job.Name,
                LastActionTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                JobStatus = job.SaveState.ToString(),
                TotalEligibleFiles = FileUtil.GetFiles(job.FileSource).Count(),
                TotalFileSize = totalSize,
                Progress = progress,
                RemainingFiles = FileUtil.GetFiles(job.FileSource).Count() - (progress == 100 ? 0 : 1),
                RemainingFileSize = totalSize, // Modify this as per your logic for progress
                CurrentSourceFilePath = currentSourceFilePath,
                CurrentDestinationFilePath = currentDestinationFilePath
            };
        }

        private void UpdateJobInFile(JobModel updatedJob)
        {
            // Logic for updating job settings in JSON
        }
    }
}
