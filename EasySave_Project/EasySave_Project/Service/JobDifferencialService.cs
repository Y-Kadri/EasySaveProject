using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Strategy for differential backup jobs.
    /// This class implements the IJobStrategy interface and provides
    /// the functionality to execute a differential backup of the specified job.
    /// </summary>
    public class JobDifferencialService : IJobStrategyService
    {
        /// <summary>
        /// Executes the differential backup job for the given JobModel.
        /// If there is no previous full backup, it performs a complete backup instead.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        /// <param name="backupDir">The directory where the backup will be stored.</param>
        public void Execute(JobModel job, string backupDir)
        {
            var translator = TranslationService.GetInstance();

            // Check for the last full backup
            if (string.IsNullOrEmpty(job.LastFullBackupPath) || !FileUtil.ExistsDirectory(job.LastFullBackupPath))
            {
                string message = translator.GetText("noPreviousFullBackup");
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);

                job.LastFullBackupPath = null; // Reset full backup path
                new JobCompleteService().Execute(job, backupDir); // Perform a full backup
            }
            else
            {
                ExecuteDifferentialSave(job, backupDir, job.LastFullBackupPath); // Perform a differential backup
            }
        }

        /// <summary>
        /// Implements the logic for performing a differential backup.
        /// This method copies only modified files from the source directory
        /// to the target directory based on the last full backup.
        /// </summary>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="lastFullBackupDir">The last full backup directory used for comparison.</param>
        private void ExecuteDifferentialSave(JobModel job, string targetDir, string lastFullBackupDir)
        {
            string message;
            message = $"Starting differential backup for {job.Name}";
            LogManager.Instance.AddMessage(message);
            ConsoleUtil.PrintTextconsole(message);

            // Calculate the total number of files and total size
            long totalSize = FileUtil.CalculateTotalSize(job.FileSource);
            int totalFiles = FileUtil.GetFiles(job.FileSource).Count();
            int processedFiles = 0;
            long processedSize = 0;

            // Copy modified files
            foreach (string sourceFile in FileUtil.GetFiles(job.FileSource))
            {
                string fileName = FileUtil.GetFileName(sourceFile);
                string lastFullBackupFile = FileUtil.CombinePath(lastFullBackupDir, fileName);
                string targetFile = FileUtil.CombinePath(targetDir, fileName);

                // Check if the file needs to be copied
                if (!FileUtil.ExistsFile(lastFullBackupFile) || FileUtil.GetLastWriteTime(sourceFile) > FileUtil.GetLastWriteTime(lastFullBackupFile))
                {
                    FileUtil.CopyFile(sourceFile, targetFile, true);

                    long fileSize = FileUtil.GetFileSize(sourceFile);
                    double transferTime = FileUtil.CalculateTransferTime(sourceFile, targetFile);

                    message = $"File {fileName} copied from {sourceFile} to {targetFile}";
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);

                    LogManager.Instance.UpdateState(job.Name, sourceFile, targetFile, fileSize, transferTime);

                    processedFiles++;
                    processedSize += fileSize;

                    // Update state in StateManager
                    StateManager.Instance.UpdateState(new BackupJobState
                    {
                        JobName = job.Name,
                        LastActionTimestamp = DateUtil.GetTodayDate(DateUtil.YYYY_MM_DD_HH_MM_SS),
                        JobStatus = job.SaveState.ToString(),
                        TotalEligibleFiles = totalFiles,
                        TotalFileSize = totalSize,
                        Progress = (double)processedFiles / totalFiles * 100,
                        RemainingFiles = totalFiles - processedFiles,
                        RemainingFileSize = totalSize - processedSize,
                        CurrentSourceFilePath = sourceFile,
                        CurrentDestinationFilePath = targetFile
                    });
                }
            }

            // Recursively copy modified subdirectories
            foreach (string subDir in FileUtil.GetDirectories(job.FileSource))
            {
                string subDirName = FileUtil.GetDirectoryName(subDir);
                string lastFullBackupSubDir = FileUtil.CombinePath(lastFullBackupDir, subDirName);
                string targetSubDir = FileUtil.CombinePath(targetDir, subDirName);

                FileUtil.CreateDirectory(targetSubDir);
                ExecuteDifferentialSave(job, targetSubDir, lastFullBackupSubDir);
            }

            string endMessage = $"Differential backup {job.Name} completed.";
            ConsoleUtil.PrintTextconsole(endMessage);
            LogManager.Instance.AddMessage(endMessage);
        }
    }
}
