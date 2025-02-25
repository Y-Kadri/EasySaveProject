using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <summary>
    /// The JobService class is responsible for managing backup jobs, including executing them,
    /// adding/removing file extensions, and handling job states.
    /// </summary>
    public class JobService
    {
        /// <summary>
        /// Instance of ConfigurationService to manage application configurations.
        /// </summary>
        private ConfigurationService _configurationService = ConfigurationService.GetInstance();

        /// <summary>
        /// Instance of TranslationService to provide localized text.
        /// </summary>
        private TranslationService translator = TranslationService.GetInstance();

        /// <summary>
        /// Retrieves a list of all backup jobs.
        /// </summary>
        /// <returns>A list of JobModel objects representing all backup jobs.</returns>
        public List<JobModel> GetAllJobs()
        {
            return JobManager.GetInstance().GetAll();
        }

        /// <summary>
        /// Executes a backup job in a separate thread and reports progress through a callback.
        /// </summary>
        /// <param name="job">The JobModel object representing the backup job to execute.</param>
        /// <param name="progressCallback">Callback function to update job progress.</param>
        /// <returns>A tuple indicating success and a message regarding the job execution.</returns>
        public (bool, string) ExecuteOneJobThreaded(JobModel job, Action<JobModel, double> progressCallback)
        {
            string message;

            // Check if the source directory exists.
            if (!FileUtil.ExistsDirectory(job.FileSource))
            {
                message = $"{translator.GetText("directorySourceDoNotExist")} : {job.FileSource}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);

                // Update job state to CANCEL
                LogManager.Instance.UpdateState(job.Name, job.FileSource, job.FileTarget, 0, 0, 0);
                job.SaveState = JobSaveStateEnum.CANCEL;
                StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, job.FileSource, string.Empty));

                return (false, translator.GetText("sourceDirectoryNotExist"));
            }

            // Check if the target directory exists, create it if not.
            if (!FileUtil.ExistsDirectory(job.FileTarget))
            {
                FileUtil.CreateDirectory(job.FileTarget);
            }

            // Create a specific backup directory for the job.
            string jobBackupDir = FileUtil.CombinePath(job.FileTarget, job.Name + "_" + job.Id);
            if (!FileUtil.ExistsDirectory(jobBackupDir))
            {
                FileUtil.CreateDirectory(jobBackupDir);
            }

            string timestampedBackupDir;
            if (!job.SaveState.Equals(JobSaveStateEnum.PENDING))
            {
                timestampedBackupDir = FileUtil.CombinePath(jobBackupDir, DateUtil.GetTodayDate(DateUtil.YYYY_MM_DD_HH_MM_SS));
                FileUtil.CreateDirectory(timestampedBackupDir);
                job.FileInPending.LastDateTimePath = timestampedBackupDir;
                job.SaveState = JobSaveStateEnum.ACTIVE;
                job.FileInPending.Progress = 0;
            }
            else
            {
                timestampedBackupDir = job.FileInPending.LastDateTimePath;
            }

            StateManager.Instance.UpdateState(CreateBackupJobState(job, job.FileInPending.Progress, job.FileSource, string.Empty));
            progressCallback(job, job.FileInPending.Progress);

            // Select the backup strategy based on job type.
            IJobStrategyService strategy = job.SaveType switch
            {
                JobSaveTypeEnum.COMPLETE => new JobCompleteService(),
                JobSaveTypeEnum.DIFFERENTIAL => new JobDifferencialService(),
                _ => throw new InvalidOperationException("Invalid job type")
            };

            strategy.OnProgressChanged += (progress) =>
            {
                progressCallback(job, job.FileInPending.Progress);
            };

            // Execute the backup.
            strategy.Execute(job, timestampedBackupDir);

            string messageForPopup;
            if (job.SaveState.Equals(JobSaveStateEnum.PENDING))
            {
                messageForPopup = $"{translator.GetText("backupPending")} : {job.Name}";
            }
            else if (job.SaveState.Equals(JobSaveStateEnum.CANCEL))
            {
                messageForPopup = $"{translator.GetText("backupCancel")} : {job.Name}";
                StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, string.Empty, string.Empty));
                InitJobDefaultValues(job, progressCallback);
            }
            else
            {
                messageForPopup = $"{translator.GetText("backupComplet")} : {job.Name}";
                job.SaveState = JobSaveStateEnum.END;
                StateManager.Instance.UpdateState(CreateBackupJobState(job, 100, string.Empty, string.Empty));
                InitJobDefaultValues(job, progressCallback);
            }

            ConsoleUtil.PrintTextconsole(messageForPopup);
            LogManager.Instance.AddMessage(messageForPopup);
            LogManager.Instance.UpdateState(job.Name, job.FileSource, job.FileTarget, 0, 0);
            UpdateJobInFile(job);

            return (true, messageForPopup);
        }

        /// <summary>
        /// Initializes default values for the job state.
        /// </summary>
        /// <param name="job">The JobModel object to initialize.</param>
        /// <param name="progressCallback">Callback function to update job progress.</param>
        private void InitJobDefaultValues(JobModel job, Action<JobModel, double> progressCallback)
        {
            job.FileInPending.Progress = 0;
            job.FileInPending.ProcessedFiles = 0;
            job.FileInPending.ProcessedSize = 0;
            job.FileInPending.TotalFiles = 0;
            job.FileInPending.TotalSize = 0;
            progressCallback(job, 0);
        }

        /// <summary>
        /// Cancels an active job and resets its state.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to cancel.</param>
        /// <param name="progressCallback">Callback function to update job progress.</param>
        public void CanceljobInActif(JobModel job, Action<JobModel, double> progressCallback)
        {
            string messageForPopup = $"{translator.GetText("backupCancel")} : {job.Name}";
            ConsoleUtil.PrintTextconsole(messageForPopup);
            LogManager.Instance.AddMessage(messageForPopup);
            LogManager.Instance.UpdateState(job.Name, job.FileSource, job.FileTarget, 0, 0);
            StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, string.Empty, string.Empty));
            InitJobDefaultValues(job, progressCallback);
            job.FileInPending.LastDateTimePath = null;
            UpdateJobInFile(job);
        }

        /// <summary>
        /// Updates a specific job in the JSON file after modification.
        /// </summary>
        /// <param name="updatedJob">The updated job.</param>
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

                // Read the existing JSON
                string jsonString = File.ReadAllText(filePath);
                JobSettingsDto data = JsonSerializer.Deserialize<JobSettingsDto>(jsonString);

                if (data == null || data.jobs == null)
                {
                    message = $"{translator.GetText("errorReadingJsonFile")}";
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    return;
                }

                // Find and update the job
                var jobToUpdate = data.jobs.Find(j => j.Id == updatedJob.Id);
                if (jobToUpdate != null)
                {
                    jobToUpdate.LastFullBackupPath = updatedJob.LastFullBackupPath;
                    jobToUpdate.LastSaveDifferentialPath = updatedJob.LastSaveDifferentialPath;
                    jobToUpdate.FileInPending = updatedJob.FileInPending;
                    jobToUpdate.SaveState = updatedJob.SaveState;

                    // Rewrite the JSON with the new values
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
        /// Adds a new encrypted file extension to the configuration.
        /// </summary>
        /// <param name="extension">The file extension to add.</param>
        public void AddEncryptedFileExtension(string extension)
        {
            try
            {
                if (!FileUtil.GetAppSettingsList("EncryptedFileExtensions").Contains(extension))
                {
                    FileUtil.AddValueToJobSettingsList("EncryptedFileExtensions", extension);
                    ConsoleUtil.PrintTextconsole($"{translator.GetText("fileExtensionAdded")} : {extension}");
                    LogManager.Instance.AddMessage($"{translator.GetText("fileExtensionAdded")} : {extension}");
                }
                else
                {
                    ConsoleUtil.PrintTextconsole($"{translator.GetText("fileExtensionAlreadyExists")} : {extension}");
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"{translator.GetText("errorAddingFileExtension")} : {ex.Message}");
            }
        }

        /// <summary>
        /// Removes an encrypted file extension from the configuration.
        /// </summary>
        /// <param name="extension">The file extension to remove.</param>
        public void RemoveEncryptedFileExtension(string extension)
        {
            try
            {
                if (FileUtil.GetAppSettingsList("EncryptedFileExtensions").Contains(extension))
                {
                    FileUtil.RemoveValueFromJobSettingsList("EncryptedFileExtensions", extension);
                    ConsoleUtil.PrintTextconsole($"{translator.GetText("fileExtensionRemoved")} : {extension}");
                    LogManager.Instance.AddMessage($"{translator.GetText("fileExtensionRemoved")} : {extension}");
                }
                else
                {
                    ConsoleUtil.PrintTextconsole($"{translator.GetText("fileExtensionNotFound")} : {extension}");
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"{translator.GetText("errorRemovingFileExtension")} : {ex.Message}");
            }
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
        /// Adds a priority business software to the configuration.
        /// </summary>
        /// <param name="software">The name of the software to add.</param>
        public void AddPriorityBusinessSoftware(string software)
        {
            try
            {
                if (!FileUtil.GetAppSettingsList("PriorityBusinessProcess").Contains(software))
                {
                    FileUtil.AddValueToJobSettingsList("PriorityBusinessProcess", software);
                    ConsoleUtil.PrintTextconsole($"{translator.GetText("prioritySoftwareAdded")} : {software}");
                    LogManager.Instance.AddMessage($"{translator.GetText("prioritySoftwareAdded")} : {software}");
                }
                else
                {
                    ConsoleUtil.PrintTextconsole($"{translator.GetText("prioritySoftwareAlreadyExists")} : {software}");
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"{translator.GetText("errorAddingPrioritySoftware")} : {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a priority business software from the configuration.
        /// </summary>
        /// <param name="software">The name of the software to remove.</param>
        public void RemovePriorityBusinessSoftware(string software)
        {
            try
            {
                if (FileUtil.GetAppSettingsList("PriorityBusinessProcess").Contains(software))
                {
                    FileUtil.RemoveValueFromJobSettingsList("PriorityBusinessProcess", software);
                    ConsoleUtil.PrintTextconsole($"{translator.GetText("prioritySoftwareRemoved")} : {software}");
                    LogManager.Instance.AddMessage($"{translator.GetText("prioritySoftwareRemoved")} : {software}");
                }
                else
                {
                    ConsoleUtil.PrintTextconsole($"{translator.GetText("prioritySoftwareNotFound")} : {software}");
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"{translator.GetText("errorRemovingPrioritySoftware")} : {ex.Message}");
            }
        }
    }
}
