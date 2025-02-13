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
                int processedFiles = 0;
                long processedSize = 0;

                ExecuteDifferentialSave(job, job.FileSource, backupDir, job.LastFullBackupPath, ref processedFiles, ref processedSize); // Perform a differential backup
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
        private void ExecuteDifferentialSave(JobModel job, string fileSource, string targetDir, string lastFullBackupDir, ref int processedFiles, ref long processedSize)
        {
            // Log the start of the backup process
            string message = $"Starting differential backup for {job.Name}";
            LogManager.Instance.AddMessage(message);
            ConsoleUtil.PrintTextconsole(message);

            // Retrieve the list of files to copy along with their count and size
            (int filesToCopyCount, long filesToCopySize, List<string> filesToCopy) = CalculateFilesToCopy(fileSource, lastFullBackupDir);

            // Copy modified files using the precomputed list
            CopyModifiedFiles(job, filesToCopy, targetDir, ref processedFiles, ref processedSize, filesToCopyCount, filesToCopySize);

            // Log the completion of the backup
            string endMessage = $"Differential backup {job.Name} completed.";
            ConsoleUtil.PrintTextconsole(endMessage);
            LogManager.Instance.AddMessage(endMessage);
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
            foreach (string sourceFile in filesToCopy)
            {
                string relativePath = FileUtil.GetRelativePath(job.FileSource, sourceFile);
                string targetFile = FileUtil.CombinePath(targetDir, relativePath);

                // Ensure the target directory exists
                string targetFileDirectory = Path.GetDirectoryName(targetFile);
                FileUtil.CreateDirectory(targetFileDirectory);

                // Perform the file copy operation
                long fileSize = HandleFileOperation(sourceFile, targetFile, job);

                processedFiles++;
                processedSize += fileSize;

                // Update the backup state
                UpdateBackupState(job, processedFiles, processedSize, totalFiles, totalSize, sourceFile, targetFile);
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
       /* private void HandleSubdirectories(JobModel job, string fileSource, string targetDir, string lastFullBackupDir, ref int processedFiles, ref long processedSize, int totalFiles, long totalSize)
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
        }*/

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

