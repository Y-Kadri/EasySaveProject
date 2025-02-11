using System;
using System.Collections.Generic;
using System.Text.Json;
using EasySave_Library_Log;
using EasySave_Library_Log.manager;
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
            string message;

            // Check if the source directory exists
            if (!FileUtil.ExistsDirectory(job.FileSource))
            {
                message = $"{translator.GetText("directorySourceDoNotExist")} : {job.FileSource}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
                return; // Exit if source directory does not exist
            }

            // Create target directory if it doesn't exist
            if (!FileUtil.ExistsDirectory(job.FileTarget))
            {
                FileUtil.CreateDirectory(job.FileTarget);
            }

            // Create a job-specific backup directory
            string jobBackupDir = FileUtil.CombinePath(job.FileTarget, job.Name + "_" + job.Id);
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

            message = $"{translator.GetText("backupCompleted")} : {job.Name}";
            ConsoleUtil.PrintTextconsole(message);
            LogManager.Instance.AddMessage(message);

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

        /// <summary>
        /// Met à jour un job spécifique dans le fichier JSON après modification.
        /// </summary>
        /// <param name="updatedJob">Le job mis à jour.</param>
        private void UpdateJobInFile(JobModel updatedJob)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easysave", "easySaveSetting", "jobsSetting.json");
            var translator = TranslationService.GetInstance();
            string message;

            try
            {
                if (!File.Exists(filePath))
                {
                    message = $"{translator.GetText("jsonFileNotExist")}";
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    return;
                }

                // Lire le JSON existant
                string jsonString = File.ReadAllText(filePath);
                JobSettingsDto data = JsonSerializer.Deserialize<JobSettingsDto>(jsonString);

                if (data == null || data.jobs == null)
                {
                    message = $"{translator.GetText("errorReadingJsonFile")}";
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    return;
                }

                // Rechercher et mettre à jour le job
                var jobToUpdate = data.jobs.Find(j => j.Id == updatedJob.Id);
                if (jobToUpdate != null)
                {
                    jobToUpdate.LastFullBackupPath = updatedJob.LastFullBackupPath;
                    jobToUpdate.LastSaveDifferentialPath = updatedJob.LastSaveDifferentialPath;

                    // Réécrire le JSON avec les nouvelles valeurs
                    string updatedJsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updatedJsonString);
                }
                else
                {
                    message = $"{translator.GetText("indexNotFoundInJson")}";
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                }
            }
            catch (Exception ex)
            {
                message = $"{translator.GetText("errorWritingJsonFile")} {ex.Message}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
            }
        }

        /// <summary>
        /// Retrieves the list of encrypted file extensions from the settings file.
        /// </summary>
        /// <returns>A list of encrypted file extensions (e.g., [".txt", ".pdf"]).</returns>
        public List<string> GetEncryptedFileExtensions()
        {
            try
            {
                // Call the FileUtil method to retrieve encrypted file extensions
                List<string> encryptedExtensions = FileUtil.GetEncryptedFileExtensions();

                return encryptedExtensions; // Return the list of encrypted file extensions
            }
            catch (Exception ex)
            {
                // Handle the exception and print an error message
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorReadingFormat") + ex.Message);
                return new List<string>(); // Return an empty list in case of an error
            }
        }

        /// <summary>
        /// Adds a file format to the list of encrypted file extensions.
        /// </summary>
        /// <param name="format">The file extension to add (e.g., "txt" or "pdf").</param>
        public void AddFormatToEasySaveSettingsForScryptFile(string format)
        {
            try
            {
                // Call the utility method to add the format to settings
                FileUtil.AddFormatToEasySaveSettingsForScryptFile(format);

                // Print success message
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("formatAdded") + format);
            }
            catch (Exception ex)
            {
                // Handle the exception and print an error message
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorAddingFormat") + ex.Message);
            }
        }
    }
}
