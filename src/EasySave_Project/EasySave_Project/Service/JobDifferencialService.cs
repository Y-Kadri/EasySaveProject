using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using CryptoSoft;
using System.Diagnostics;
namespace EasySave_Project.Service
{
    /// <summary>
    /// Strategy for differential backup jobs.
    /// This class implements the IJobStrategy interface and provides
    /// the functionality to execute a differential backup of the specified job.
    /// </summary>
    public class JobDifferencialService : AJobStrategyService
    {
        /// <summary>
        /// Executes the differential backup job for the given JobModel.
        /// If there is no previous full backup, it performs a complete backup instead.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        /// <param name="backupDir">The directory where the backup will be stored.</param>
        override
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
                // Retrieve total number of files and total size before starting the backup
                var allFiles = FileUtil.GetFilesRecursively(job.FileSource);
                int totalFiles = allFiles.Count;
                long totalSize = allFiles.Sum(FileUtil.GetFileSize);

                int processedFiles = 0;
                long processedSize = 0;

                ExecuteDifferentialSave(job, job.FileSource, backupDir, job.LastFullBackupPath, totalFiles, totalSize, ref processedFiles, ref processedSize); // Perform a differential backup
            }

            job.LastSaveDifferentialPath = backupDir;
        }

        /// <summary>
        /// Implements the logic for performing a differential backup.
        /// This method copies only modified files from the source directory
        /// to the target directory based on the last full backup.
        /// </summary>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="lastFullBackupDir">The last full backup directory used for comparison.</param>
        /// <param name="totalFiles">Total number of files to be processed.</param>
        /// <param name="totalSize">Total size of the files to be backed up.</param>
        /// <param name="processedFiles">Reference to the number of processed files.</param>
        /// <param name="processedSize">Reference to the total size of processed files.</param>
        /// <summary>
        /// Starts the differential backup for the given job and performs the backup
        /// by copying only modified files and directories based on the last full backup.
        /// </summary>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="fileSource">The source directory to back up.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="lastFullBackupDir">The directory of the last full backup.</param>
        /// <param name="totalFiles">Total number of files to be processed.</param>
        /// <param name="totalSize">Total size of the files to be backed up.</param>
        /// <param name="processedFiles">Reference to the number of processed files.</param>
        /// <param name="processedSize">Reference to the total size of processed files.</param>
        private void ExecuteDifferentialSave(JobModel job, string fileSource, string targetDir, string lastFullBackupDir, int totalFiles, long totalSize, ref int processedFiles, ref long processedSize)
        {
            // Log the start of the backup
            string message = $"Starting differential backup for {job.Name}";
            LogManager.Instance.AddMessage(message);
            ConsoleUtil.PrintTextconsole(message);

            // Calculate total files and size to be copied
            (int filesToCopyCount, long filesToCopySize) = CalculateFilesToCopy(fileSource, lastFullBackupDir);

            // Update total files and size variables
            totalFiles = filesToCopyCount;
            totalSize = filesToCopySize;

            // Copy modified files
            CopyModifiedFiles(job, fileSource, targetDir, lastFullBackupDir, ref processedFiles, ref processedSize, totalFiles, totalSize);

            // Handle subdirectories
            HandleSubdirectories(job, fileSource, targetDir, lastFullBackupDir, ref processedFiles, ref processedSize, totalFiles, totalSize);

            // Log completion of the backup
            string endMessage = $"Differential backup {job.Name} completed.";
            ConsoleUtil.PrintTextconsole(endMessage);
            LogManager.Instance.AddMessage(endMessage);
        }

        /// <summary>
        /// Calculates the number of files to be copied and their total size, based on whether the files have been modified
        /// or are missing compared to the last full backup.
        /// </summary>
        /// <param name="fileSource">The source directory to scan for files.</param>
        /// <param name="lastFullBackupDir">The directory of the last full backup for comparison.</param>
        /// <returns>A tuple containing the count of files to copy and their total size.</returns>
        private (int filesToCopyCount, long filesToCopySize) CalculateFilesToCopy(string fileSource, string lastFullBackupDir)
        {
            int filesToCopyCount = 0;
            long filesToCopySize = 0;

            foreach (string sourceFile in FileUtil.GetFiles(fileSource))
            {
                string relativePath = FileUtil.GetRelativePath(fileSource, sourceFile);
                string lastFullBackupFile = FileUtil.CombinePath(lastFullBackupDir, relativePath);

                // Check if the file needs to be copied (modified or missing)
                if (!FileUtil.ExistsFile(lastFullBackupFile) || FileUtil.GetLastWriteTime(sourceFile) > FileUtil.GetLastWriteTime(lastFullBackupFile))
                {
                    // Increment the count and size for files that need to be copied
                    filesToCopyCount++;
                    filesToCopySize += FileUtil.GetFileSize(sourceFile);
                }
            }

            return (filesToCopyCount, filesToCopySize);
        }

        /// <summary>
        /// Copies the modified files from the source directory to the target directory,
        /// and updates the processed files and size counts.
        /// </summary>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="fileSource">The source directory to back up.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="lastFullBackupDir">The directory of the last full backup.</param>
        /// <param name="processedFiles">Reference to the number of processed files.</param>
        /// <param name="processedSize">Reference to the total size of processed files.</param>
        /// <param name="totalFiles">Total number of files to be processed.</param>
        /// <param name="totalSize">Total size of the files to be backed up.</param>
        private void CopyModifiedFiles(JobModel job, string fileSource, string targetDir, string lastFullBackupDir, ref int processedFiles, ref long processedSize, int totalFiles, long totalSize)
        {
            foreach (string sourceFile in FileUtil.GetFiles(fileSource))
            {
                string relativePath = FileUtil.GetRelativePath(fileSource, sourceFile);
                string targetFile = FileUtil.CombinePath(targetDir, relativePath);
                string lastFullBackupFile = FileUtil.CombinePath(lastFullBackupDir, relativePath);

                // Ensure the target directory exists
                string targetFileDirectory = Path.GetDirectoryName(targetFile);
                FileUtil.CreateDirectory(targetFileDirectory);

                // Only copy the file if it is modified or doesn't exist in the full backup
                if (!FileUtil.ExistsFile(lastFullBackupFile) || FileUtil.GetLastWriteTime(sourceFile) > FileUtil.GetLastWriteTime(lastFullBackupFile))
                {
                    long fileSize = HandleFileOperation(sourceFile, targetFile, job);

                    processedFiles++;
                    processedSize += fileSize;

                    // Update state in StateManager
                    UpdateBackupState(job, processedFiles, processedSize, totalFiles, totalSize, sourceFile, targetFile);
                }
            }
        }

        /// <summary>
        /// Handles the subdirectories of the source directory, checking and copying modified subdirectories and their files.
        /// </summary>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="fileSource">The source directory to back up.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="lastFullBackupDir">The directory of the last full backup.</param>
        /// <param name="processedFiles">Reference to the number of processed files.</param>
        /// <param name="processedSize">Reference to the total size of processed files.</param>
        /// <param name="totalFiles">Total number of files to be processed.</param>
        /// <param name="totalSize">Total size of the files to be backed up.</param>
        private void HandleSubdirectories(JobModel job, string fileSource, string targetDir, string lastFullBackupDir, ref int processedFiles, ref long processedSize, int totalFiles, long totalSize)
        {
            foreach (string subDir in FileUtil.GetDirectories(fileSource))
            {
                string newTargetDir = FileUtil.CombinePath(targetDir, FileUtil.GetFileName(subDir));
                string newLastFullBackupDir = FileUtil.CombinePath(lastFullBackupDir, FileUtil.GetFileName(subDir));

                // Check if the last full backup directory exists
                if (FileUtil.ExistsDirectory(newLastFullBackupDir))
                {
                    // Check for any modified files in the subdirectory
                    bool subDirModified = CheckSubdirectoryModification(subDir, newLastFullBackupDir);

                    // If the subdirectory has any modified files or the directory itself is modified
                    if (subDirModified || FileUtil.GetLastWriteTime(subDir) > FileUtil.GetLastWriteTime(newLastFullBackupDir))
                    {
                        ExecuteDifferentialSave(job, subDir, newTargetDir, newLastFullBackupDir, totalFiles, totalSize, ref processedFiles, ref processedSize);
                    }
                }
                else
                {
                    // If the last full backup does not exist for this subdir, copy it completely
                    ExecuteDifferentialSave(job, subDir, newTargetDir, string.Empty, totalFiles, totalSize, ref processedFiles, ref processedSize);
                }
            }
        }

        /// <summary>
        /// Checks if any files in a subdirectory have been modified compared to the last full backup.
        /// </summary>
        /// <param name="subDir">The subdirectory to check.</param>
        /// <param name="lastFullBackupSubDir">The corresponding subdirectory in the last full backup.</param>
        /// <returns>True if any files in the subdirectory are modified, otherwise false.</returns>
        private bool CheckSubdirectoryModification(string subDir, string lastFullBackupSubDir)
        {
            foreach (string subFile in FileUtil.GetFiles(subDir))
            {
                string lastFullBackupSubFile = FileUtil.CombinePath(lastFullBackupSubDir, FileUtil.GetFileName(subFile));
                if (!FileUtil.ExistsFile(lastFullBackupSubFile) || FileUtil.GetLastWriteTime(subFile) > FileUtil.GetLastWriteTime(lastFullBackupSubFile))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

