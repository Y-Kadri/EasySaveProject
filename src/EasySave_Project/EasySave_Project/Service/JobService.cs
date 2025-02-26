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


            // Create a backup directory specific to the job
            string jobBackupDir = FileUtil.CombinePath(job.FileTarget, job.Name + "_" + job.Id);
            if (!FileUtil.ExistsDirectory(jobBackupDir))
            {
                FileUtil.CreateDirectory(jobBackupDir);
            }

            string timestampedBackupDir;

            if (!job.SaveState.Equals(JobSaveStateEnum.PENDING))
            {
                // Create a timestamped subdirectory to differentiate backup executions
                timestampedBackupDir = FileUtil.CombinePath(jobBackupDir, DateUtil.GetTodayDate(DateUtil.YYYY_MM_DD_HH_MM_SS));
                FileUtil.CreateDirectory(timestampedBackupDir);
                job.FileInPending.LastDateTimePath = timestampedBackupDir;
                // Mark the job as ACTIVE
                job.SaveState = JobSaveStateEnum.ACTIVE;
                // Notify the UI that the progress starts at 0%
                job.FileInPending.Progress = 0;
            } else
            {
                timestampedBackupDir = job.FileInPending.LastDateTimePath;
            }

            StateManager.Instance.UpdateState(CreateBackupJobState(job, job.FileInPending.Progress, job.FileSource, string.Empty));

            progressCallback(job, job.FileInPending.Progress);

            IJobStrategyService strategy;

            if (job.SaveType.Equals(JobSaveTypeEnum.COMPLETE))
            {
                strategy = new JobCompleteService();
            } else if (job.SaveType.Equals(JobSaveTypeEnum.DIFFERENTIAL))
            {
                if (string.IsNullOrEmpty(job.LastFullBackupPath) || !FileUtil.ExistsDirectory(job.LastFullBackupPath))
                {
                    strategy = new JobCompleteService();
                    message = translator.GetText("noPreviousFullBackup");
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    job.LastFullBackupPath = null; // Reset full backup pat
                } else
                {
                    strategy = new JobDifferencialService();
                }
            } else
            {
                throw new InvalidOperationException("Invalid job type");
            }

            // Handle the job progress
            strategy.OnProgressChanged += (progress) =>
            {
                // Update the progress in real-time
                progressCallback(job, job.FileInPending.Progress);
            };

            // Execute the backup with the selected strategy
            strategy.Execute(job, timestampedBackupDir);

            string messageForPopup;

            if (job.SaveState.Equals(JobSaveStateEnum.PENDING))
            {
                // Generate a message indicating the job is pending
                messageForPopup = $"{translator.GetText("backupPending")} : {job.Name}";
                ConsoleUtil.PrintTextconsole(messageForPopup);
                LogManager.Instance.AddMessage(messageForPopup);
            } else if (job.SaveState.Equals(JobSaveStateEnum.CANCEL))
            {
                messageForPopup = $"{translator.GetText("backupCancel")} : {job.Name}";
                ConsoleUtil.PrintTextconsole(messageForPopup);
                LogManager.Instance.AddMessage(messageForPopup);
                StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, string.Empty, string.Empty));
                InitJobDefaultValues(job, progressCallback);
            }
            else {
                messageForPopup = TranslationService.GetInstance().GetText("backupComplet") + " " + job.Name;
                job.SaveState = JobSaveStateEnum.END;
                ConsoleUtil.PrintTextconsole(messageForPopup);
                LogManager.Instance.AddMessage(messageForPopup);
                StateManager.Instance.UpdateState(CreateBackupJobState(job, 100, string.Empty, string.Empty));
                InitJobDefaultValues(job, progressCallback);
            }

            // Update the stored parameters for the job
            UpdateJobInFile(job);

            return (true, messageForPopup); // Return success if everything went as planned
        }

        private void InitJobDefaultValues(JobModel job, Action<JobModel, double> progressCallback)
        {
            job.FileInPending.Progress = 0;
            job.FileInPending.ProcessedFiles = 0;
            job.FileInPending.ProcessedSize = 0;
            job.FileInPending.TotalFiles = 0;
            job.FileInPending.TotalSize = 0;
            progressCallback(job, 0);
        }

        public void CanceljobInActif(JobModel job, Action<JobModel, double> progressCallback)
        {
            string messageForPopup = $"{TranslationService.GetInstance().GetText("backupCancel")} : {job.Name}";
            ConsoleUtil.PrintTextconsole(messageForPopup);
            StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, string.Empty, string.Empty));
            InitJobDefaultValues(job, progressCallback);
            job.FileInPending.LastDateTimePath = null;
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
                    jobToUpdate.FileInPending = updatedJob.FileInPending;
                    jobToUpdate.SaveState = updatedJob.SaveState;

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
        /// Retrieve a list from the settings file based on the provided key.
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

public void AddEncryptedFileExtension(string extension)
{
    try
    {
        // Vérifie si l'extension n'est pas déjà présente dans la liste
        if (!FileUtil.GetAppSettingsList("EncryptedFileExtensions").Contains(extension))
        {
            FileUtil.AddValueToJobSettingsList("EncryptedFileExtensions", extension); // Appel à la méthode FileUtil
            ConsoleUtil.PrintTextconsole($"Extension de fichier {extension} ajoutée.");
            LogManager.Instance.AddMessage($"Extension de fichier {extension} ajoutée.");
        }
        else
        {
            ConsoleUtil.PrintTextconsole($"L'extension {extension} est déjà présente.");
        }
    }
    catch (Exception ex)
    {
        ConsoleUtil.PrintTextconsole($"Erreur lors de l'ajout de l'extension : {ex.Message}");
    }
}


        public void RemoveEncryptedFileExtension(string extension)
        {
            try
            {
                if (FileUtil.GetAppSettingsList("EncryptedFileExtensions").Contains(extension))
                {
                    FileUtil.RemoveValueFromJobSettingsList("EncryptedFileExtensions", extension);
                    ConsoleUtil.PrintTextconsole($"Extension de fichier {extension} supprimée.");
                    LogManager.Instance.AddMessage($"Extension de fichier {extension} supprimée.");
                }
                else
                {
                    ConsoleUtil.PrintTextconsole($"L'extension {extension} n'existe pas.");
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"Erreur lors de la suppression de l'extension : {ex.Message}");
            }
        }

        public void AddPriorityBusinessSoftware(string software)
        {
            try
            {
                if (!FileUtil.GetAppSettingsList("PriorityBusinessProcess").Contains(software))
                {
                    FileUtil.AddValueToJobSettingsList("PriorityBusinessProcess", software);
                    ConsoleUtil.PrintTextconsole($"Logiciel prioritaire {software} ajouté.");
                    LogManager.Instance.AddMessage($"Logiciel prioritaire {software} ajouté.");
                }
                else
                {
                    ConsoleUtil.PrintTextconsole($"Le logiciel {software} est déjà présent.");
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"Erreur lors de l'ajout du logiciel : {ex.Message}");
            }
        }

        public void RemovePriorityBusinessSoftware(string software)
        {
            try
            {
                if (FileUtil.GetAppSettingsList("PriorityBusinessProcess").Contains(software))
                {
                    FileUtil.RemoveValueFromJobSettingsList("PriorityBusinessProcess", software);
                    ConsoleUtil.PrintTextconsole($"Logiciel prioritaire {software} supprimé.");
                    LogManager.Instance.AddMessage($"Logiciel prioritaire {software} supprimé.");
                }
                else
                {
                    ConsoleUtil.PrintTextconsole($"Le logiciel {software} n'existe pas.");
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"Erreur lors de la suppression du logiciel : {ex.Message}");
            }
        }
    }
}
