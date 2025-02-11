using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
        
        public async Task ExecuteOneJobAsync(JobModel job, Action<JobModel, double> progressCallback)
        {
            await Task.Run(() =>
            {
                var translator = TranslationService.GetInstance();
                string message;

                // Vérification du répertoire source
                if (!FileUtil.ExistsDirectory(job.FileSource))
                {
                    message = $"{translator.GetText("directorySourceDoNotExist")} : {job.FileSource}";
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    return;
                }

                // Crée les répertoires nécessaires
                if (!FileUtil.ExistsDirectory(job.FileTarget))
                    FileUtil.CreateDirectory(job.FileTarget);

                string jobBackupDir = FileUtil.CombinePath(job.FileTarget, $"{job.Name}_{job.Id}");
                if (!FileUtil.ExistsDirectory(jobBackupDir))
                    FileUtil.CreateDirectory(jobBackupDir);

                string timestampedBackupDir = FileUtil.CombinePath(jobBackupDir, DateUtil.GetTodayDate(DateUtil.YYYY_MM_DD_HH_MM_SS));
                FileUtil.CreateDirectory(timestampedBackupDir);

                // Mettre à jour l'état du job
                job.SaveState = JobSaveStateEnum.ACTIVE;
                StateManager.Instance.UpdateState(CreateBackupJobState(job, 0, job.FileSource, string.Empty));
                progressCallback(job, 0);  // Initialiser à 0%

                // Sélection de la stratégie
                IJobStrategyService strategy = job.SaveType switch
                {
                    JobSaveTypeEnum.COMPLETE => new JobCompleteService(),
                    JobSaveTypeEnum.DIFFERENTIAL => new JobDifferencialService(),
                    _ => throw new InvalidOperationException("Invalid job type")
                };

                // Suivre la progression pendant l'exécution
                strategy.OnProgressChanged += (progress) =>
                {
                    progressCallback(job, progress);
                    StateManager.Instance.UpdateState(CreateBackupJobState(job, progress, string.Empty, string.Empty));
                };

                // Exécuter le job
                strategy.Execute(job, timestampedBackupDir);

                // Marquer comme terminé
                job.SaveState = JobSaveStateEnum.END;
                progressCallback(job, 100);  // Progression à 100%
                StateManager.Instance.UpdateState(CreateBackupJobState(job, 100, string.Empty, string.Empty));

                message = $"{translator.GetText("backupCompleted")} : {job.Name}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);

                UpdateJobInFile(job);
            });
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
    }
}
