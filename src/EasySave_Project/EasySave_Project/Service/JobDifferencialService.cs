using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using System.Collections.Generic;
using CryptoSoft;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Tmds.DBus.Protocol;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Strategy for differential backup jobs.
    /// This class implements the IJobStrategy interface and provides
    /// the functionality to execute a differential backup of the specified job.
    /// </summary>
    public class JobDifferencialService : AJobStrategyService
    {
        public event Action<double> OnProgressChanged;
        
        /// <summary>
        /// Executes the differential backup job for the given JobModel.
        /// If there is no previous full backup, it performs a complete backup instead.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        /// <param name="backupDir">The directory where the backup will be stored.</param>
        
        public override void Execute(JobModel job, string backupDir)
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
                ExecuteDifferentialSave(job, job.FileSource, backupDir, job.LastFullBackupPath); // Perform a differential backup
            }

            job.LastSaveDifferentialPath = backupDir;
        }

        /// Starts the differential backup for the given job and performs the backup
        /// by copying only modified files and directories based on the last full backup.
        /// </summary>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="fileSource">The source directory to back up.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="lastFullBackupDir">The directory of the last full backup.</param>
        /// <param name="processedFiles">Reference to the number of processed files.</param>
        /// <param name="processedSize">Reference to the total size of processed files.</param>
        private void ExecuteDifferentialSave(JobModel job, string fileSource, string targetDir, string lastFullBackupDir)
        {
            // Log the start of the backup process
            string message = TranslationService.GetInstance().GetText("startingBackup") + job.Name;
            LogManager.Instance.AddMessage(message);
            ConsoleUtil.PrintTextconsole(message);

            List<string> filesToCopy;
            int processedFiles = 0;
            long processedSize = 0;
            int totalFiles = 0;
            long totalSize = 0;

            if (job.SaveState.Equals(JobSaveStateEnum.PENDING))
            {
                filesToCopy = job.FileInPending.FilesInPending;
                job.SaveState = JobSaveStateEnum.ACTIVE;
                processedFiles = job.FileInPending.ProcessedFiles;
                processedSize = job.FileInPending.ProcessedSize;
                totalFiles = job.FileInPending.TotalFiles;
                totalSize = job.FileInPending.TotalSize;
            } else
            {
                // Retrieve the list of files to copy along with their count and size
                (totalFiles, totalSize, filesToCopy) = CalculateFilesToCopy(fileSource, lastFullBackupDir);
            }

            // Copy modified files using the precomputed list
            CopyModifiedFiles(job, filesToCopy, targetDir, ref processedFiles, ref processedSize, totalFiles, totalSize);

            // Log the completion of the backup
            message = TranslationService.GetInstance().GetText("backupCompleted") + job.Name;
            ConsoleUtil.PrintTextconsole(message);
            LogManager.Instance.AddMessage(message);
        }

        /// <summary>
        /// Calculates the number of files and directories to be copied and their total size, 
        /// based on whether they have been modified or are missing compared to the last full backup.
        /// </summary>
        /// <param name="fileSource">The source directory to scan.</param>
        /// <param name="lastFullBackupDir">The last full backup directory for comparison.</param>
        /// <returns>A tuple containing the count of files and directories to copy and their total size.</returns>
        private (int itemsToCopyCount, long itemsToCopySize, List<string> filesToCopy) CalculateFilesToCopy(string fileSource, string lastFullBackupDir)
        {
            int itemsToCopyCount = 0;
            long itemsToCopySize = 0;
            List<string> filesToCopy = new List<string>();

            // Check files
            foreach (string sourceFile in FileUtil.GetFiles(fileSource))
            {
                string relativePath = FileUtil.GetRelativePath(fileSource, sourceFile);
                string lastFullBackupFile = FileUtil.CombinePath(lastFullBackupDir, relativePath);

                // If the file doesn't exist in the full backup or has been modified, mark it for copying
                if (!FileUtil.ExistsFile(lastFullBackupFile) || FileUtil.GetLastWriteTime(sourceFile) > FileUtil.GetLastWriteTime(lastFullBackupFile))
                {
                    itemsToCopyCount++;
                    itemsToCopySize += FileUtil.GetFileSize(sourceFile);
                    filesToCopy.Add(sourceFile);
                }
            }

            // Recursively check subdirectories
            foreach (string sourceDir in FileUtil.GetDirectories(fileSource))
            {
                string relativePath = FileUtil.GetRelativePath(fileSource, sourceDir);
                string newLastFullBackupSubDir = FileUtil.CombinePath(lastFullBackupDir, relativePath);

                var (itemsToCopyCountChild, itemsToCopySizeChild, filesToCopyChild) = CalculateFilesToCopy(sourceDir, newLastFullBackupSubDir);
                itemsToCopyCount += itemsToCopyCountChild;
                itemsToCopySize += itemsToCopySizeChild;
                filesToCopy.AddRange(filesToCopyChild);
            }

            return (itemsToCopyCount, itemsToCopySize, filesToCopy);
        }


        /// <summary>
        /// Copies the modified files from the precomputed list to the target directory,
        /// updating the processed files and size counters.
        /// </summary>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="filesToCopy">The list of files that need to be copied.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="processedFiles">Reference to the number of processed files.</param>
        /// <param name="processedSize">Reference to the total size of processed files.</param>
        /// <param name="totalFiles">Total number of files to be processed.</param>
        /// <param name="totalSize">Total size of the files to be backed up.</param>
        private void CopyModifiedFiles(JobModel job, List<string> filesToCopy, string targetDir, ref int processedFiles, ref long processedSize, int totalFiles, long totalSize)
        {
            List<string> pathToDelete = new List<string>();

            if (filesToCopy.Count <= 0)
            {
                string message = TranslationService.GetInstance().GetText("notFileDifference") + " " + job.Name;
                LogManager.Instance.AddMessage(message);
                LogManager.Instance.UpdateState(job.Name, job.FileSource, job.FileTarget, 0, 0, 0);
            }
            else
            {
                foreach (string sourceFileWithAbsolutePath in filesToCopy)
                {
                    string relativePath = FileUtil.GetRelativePath(job.FileSource, sourceFileWithAbsolutePath);
                    string targetFile = FileUtil.CombinePath(targetDir, relativePath);

                    // Ensure the target directory exists
                    string targetFileDirectory = Path.GetDirectoryName(targetFile);
                    FileUtil.CreateDirectory(targetFileDirectory);

                    double progressPourcentage = (double)processedFiles / totalFiles * 100;

                    changeJobStateIfBusinessProcessLaunching(job);

                    if (!job.SaveState.Equals(JobSaveStateEnum.PENDING))
                    {
                        // Perform the file copy operation
                        long fileSize = HandleFileOperation(sourceFileWithAbsolutePath, targetFile, job, progressPourcentage);

                        processedFiles++;
                        processedSize += fileSize;

                        // Update the backup state
                        UpdateBackupState(job, processedFiles, processedSize, totalFiles, totalSize, sourceFileWithAbsolutePath, targetFile, progressPourcentage, job.FileInPending.LastDateTimePath);

                        pathToDelete.Add(sourceFileWithAbsolutePath);
                    } else
                    {
                        break;
                    }
                }
                filesToCopy.RemoveAll(path => pathToDelete.Contains(path));
                SaveFileInPending(job, filesToCopy, processedFiles, processedSize, totalFiles, totalSize, job.FileInPending.LastDateTimePath);
            }
        }
    }
}

