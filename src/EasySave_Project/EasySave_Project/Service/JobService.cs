using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Dto;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Util;

namespace EasySave_Project.Service
{
    public class JobService
    {
        
        private ConfigurationService _configurationService = ConfigurationService.GetInstance();
        
        public List<JobModel> GetAllJobs()
        {
            return JobManager.GetInstance().GetAll();
        }

        public (bool, string) ExecuteOneJobThreaded(JobModel job, Action<JobModel, double> progressCallback)
        {
            var translator = TranslationService.GetInstance();
            string message;

            // Check if the source directory of the job exists
            if (!FileUtil.ExistsDirectory(job.FileSource))
            {
                message = $"{translator.GetText("directorySourceDoNotExist")} : {job.FileSource}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);

                // Update the job status to CANCELLED
                LogManager.Instance.UpdateState(job.Name, job.FileSource, job.FileTarget, 0, 0, 0);
                job.SaveState = JobSaveStateEnum.CANCEL;
                StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, job.FileSource, string.Empty));

                return (false, "source directory does not exist");
            }

            // Check if the target directory exists, create it if not
            if (!FileUtil.ExistsDirectory(job.FileTarget))
            {
                FileUtil.CreateDirectory(job.FileTarget);
            }

            // Check if a priority process is currently running
            var processes = FileUtil.GetAppSettingsList("PriorityBusinessProcess");
            (bool isRunning, string PName) = ProcessUtil.IsProcessRunning(processes);
            if (isRunning)
            {
                string Pmessage = TranslationService.GetInstance().GetText("interuptJob") + " " + PName;
                ConsoleUtil.PrintTextconsole(Pmessage);

                // Update the job status to SKIPPED
                job.SaveState = JobSaveStateEnum.SKIP;
                StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, job.FileSource, string.Empty));

                return (false, "A priority program killed the process");
            }

            // Create a backup directory specific to the job
            string jobBackupDir = FileUtil.CombinePath(job.FileTarget, job.Name + "_" + job.Id);
            if (!FileUtil.ExistsDirectory(jobBackupDir))
            {
                FileUtil.CreateDirectory(jobBackupDir);
            }

            // Create a timestamped subdirectory to differentiate backup executions
            string timestampedBackupDir = FileUtil.CombinePath(jobBackupDir, DateUtil.GetTodayDate(DateUtil.YYYY_MM_DD_HH_MM_SS));
            FileUtil.CreateDirectory(timestampedBackupDir);

            // Mark the job as ACTIVE
            job.SaveState = JobSaveStateEnum.ACTIVE;
            StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, job.FileSource, string.Empty));

            // Notify the UI that the progress starts at 0%
            if (progressCallback != null)
                progressCallback(job, 0);

            // Select the backup strategy based on the job type
            IJobStrategyService strategy = job.SaveType switch
            {
                JobSaveTypeEnum.COMPLETE => new JobCompleteService(),       // Full backup
                JobSaveTypeEnum.DIFFERENTIAL => new JobDifferencialService(), // Differential backup
                _ => throw new InvalidOperationException("Invalid job type") // Handle unknown job types
            };

            // Handle the job progress
            strategy.OnProgressChanged += (progress) =>
            {
                // Update the progress in real-time
                if (progressCallback != null)
                    progressCallback(job, progress);
                StateManager.Instance.UpdateState(CreateBackupJobState(job, progress, string.Empty, string.Empty));
            };

            // Execute the backup with the selected strategy
            strategy.Execute(job, timestampedBackupDir);

            // Update the job status after execution
            job.SaveState = JobSaveStateEnum.END;
            if (progressCallback != null)
                progressCallback(job, 100); // Indicate the job is finished at 100%
            StateManager.Instance.UpdateState(CreateBackupJobState(job, 100, string.Empty, string.Empty));

            // Generate a message indicating the job is complete
            message = $"{translator.GetText("backupCompleted")} : {job.Name}";
            ConsoleUtil.PrintTextconsole(message);
            LogManager.Instance.AddMessage(message);

            // Update the stored parameters for the job
            UpdateJobInFile(job);

            return (true, "success"); // Return success if everything went as planned
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
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _configurationService.GetStringSetting("Path:ProjectFile"),
                _configurationService.GetStringSetting("Path:SettingFolder"),
                _configurationService.GetStringSetting("Path:JobSettingFile"));
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
        /// Retrieves a list from the settings file based on the provided key.
        /// </summary>
        /// <param name="key">The key to retrieve the list for (e.g., "EncryptedFileExtensions" or "PriorityBusinessProcess").</param>
        /// <returns>A list of values (e.g., file extensions, business processes).</returns>
        public List<string> GetJobSettingsList(string key)
        {
            try
            {
                // Call the utility method to retrieve the list based on the key
                List<string> settingsList = FileUtil.GetAppSettingsList(key);

                return settingsList; // Return the list
            }
            catch (Exception ex)
            {
                // Handle the exception and print an error message
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("error" + key) + ex.Message);
                return new List<string>(); // Return an empty list in case of an error
            }
        }

        // /// <summary>
        // /// Adds a file format to the list of encrypted file extensions.
        // /// </summary>
        // /// <param name="key">The file extension to add (e.g., "txt" or "pdf").</param>
        // public void AddValueToJobSettingsList(string key, string value)
        // {
        //     try
        //     {
        //         // Call the utility method to add the format to settings
        //         FileUtil.AddValueToJobSettingsList(key, value);
        //
        //         // Print success message
        //         ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("elementAdded") + " " + value);
        //     }
        //     catch (Exception ex)
        //     {
        //         // Handle the exception and print an error message
        //         ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorAddingElement") + ex.Message);
        //     }
        // }

    }
}
