using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using System.Linq;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Strategy for complete backup jobs.
    /// This class implements the IJobStrategy interface and provides
    /// the functionality to execute a full backup of the specified job.
    /// </summary>
    public class JobCompleteService : IJobStrategyService
    {
        /// <summary>
        /// Executes the complete backup job for the given JobModel.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        /// <param name="backupDir">The directory where the backup will be stored.</param>
        public void Execute(JobModel job, string backupDir)
        {
            string message = $"Starting complete backup for {job.Name}";
            ConsoleUtil.PrintTextconsole(message);
            LogManager.Instance.AddMessage(message);

            ExecuteCompleteSave(job.FileSource, backupDir, job); // Perform the complete backup

            job.LastFullBackupPath = backupDir; // Update the last full backup path

            message = $"Complete backup for {job.Name} finished successfully.";
            ConsoleUtil.PrintTextconsole(message);
            LogManager.Instance.AddMessage(message);
        }

        /// <summary>
        /// Implements the logic for performing a complete backup.
        /// This method copies all files and subdirectories from the source directory
        /// to the target directory.
        /// </summary>
        /// <param name="sourceDir">The source directory to back up.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="job">The JobModel representing the backup job.</param>
        private void ExecuteCompleteSave(string sourceDir, string targetDir, JobModel job)
        {
            var files = FileUtil.GetFiles(sourceDir);
            int totalFiles = files.Count();
            int processedFiles = 0;
            long totalSize = FileUtil.CalculateTotalSize(sourceDir); // Use the new method
            long processedSize = 0;

            // Copy all files from the source directory
            foreach (string sourceFile in files)
            {
                string fileName = FileUtil.GetFileName(sourceFile);
                string targetFile = FileUtil.CombinePath(targetDir, fileName);

                FileUtil.CopyFile(sourceFile, targetFile, true); // Copy file to target

                // Calculate file size and transfer time
                long fileSize = FileUtil.GetFileSize(sourceFile);
                double transferTime = FileUtil.CalculateTransferTime(sourceFile, targetFile);

                // Log the operation
                LogManager.Instance.UpdateState(
                    jobName: job.Name,
                    sourcePath: sourceFile,
                    targetPath: targetFile,
                    fileSize: fileSize,
                    transferTime: transferTime
                );

                // Update processed files and sizes
                processedFiles++;
                processedSize += fileSize;

                // Update the state in the StateManager
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

            // Recursively copy all subdirectories
            foreach (string subDir in FileUtil.GetDirectories(sourceDir))
            {
                string subDirName = FileUtil.GetDirectoryName(subDir);
                string targetSubDir = FileUtil.CombinePath(targetDir, subDirName);

                FileUtil.CreateDirectory(targetSubDir);
                ExecuteCompleteSave(subDir, targetSubDir, job); // Recursive call
            }
        }
    }
}
