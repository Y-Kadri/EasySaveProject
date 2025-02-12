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
        private void ExecuteDifferentialSave(JobModel job, string fileSource, string targetDir, string lastFullBackupDir, int totalFiles, long totalSize, ref int processedFiles, ref long processedSize)
        {
            string message;
            TranslationService translator = TranslationService.GetInstance();
            message = $"Starting differential backup for {job.Name}";
            LogManager.Instance.AddMessage(message);
            ConsoleUtil.PrintTextconsole(message);

            bool directoryModified = false; // Flag to check if any file in the directory is modified

            // Copy modified files
            foreach (string sourceFile in FileUtil.GetFiles(fileSource))
            {
                string relativePath = FileUtil.GetRelativePath(fileSource, sourceFile);
                string targetFile = FileUtil.CombinePath(targetDir, relativePath);
                string lastFullBackupFile = FileUtil.CombinePath(lastFullBackupDir, relativePath);

                // Ensure the target directory exists
                string targetFileDirectory = Path.GetDirectoryName(targetFile);
                FileUtil.CreateDirectory(targetFileDirectory);

                // Check if the file needs to be copied
                if (!FileUtil.ExistsFile(lastFullBackupFile) || FileUtil.GetLastWriteTime(sourceFile) > FileUtil.GetLastWriteTime(lastFullBackupFile))
                {
                    long fileSize = HandleFileOperation(sourceFile, targetFile, job);

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

                    // Mark that the directory is modified
                    directoryModified = true;
                }
            }

            // Copy modified subdirectories
            foreach (string subDir in FileUtil.GetDirectories(fileSource))
            {
                string newTargetDir = FileUtil.CombinePath(targetDir, FileUtil.GetFileName(subDir));
                string newLastFullBackupDir = FileUtil.CombinePath(lastFullBackupDir, FileUtil.GetFileName(subDir));


                if (!FileUtil.ExistsDirectory(newLastFullBackupDir) || FileUtil.GetLastWriteTime(subDir) > FileUtil.GetLastWriteTime(newLastFullBackupDir))
                {
                    ExecuteDifferentialSave(job, subDir, newTargetDir, newLastFullBackupDir, totalFiles, totalSize, ref processedFiles, ref processedSize);
                }



                /*string relativePath = FileUtil.GetRelativePath(job.FileSource, subDir);
                string targetSubDir = FileUtil.CombinePath(targetDir, relativePath);
                string lastFullBackupSubDir = FileUtil.CombinePath(lastFullBackupDir, relativePath);

                // Ensure the subdirectory exists before proceeding
                if (!FileUtil.ExistsDirectory(lastFullBackupSubDir))
                {
                    continue; // Skip if the last full backup directory doesn't exist
                }

                // Check if any file or subdirectory has been modified in this directory
                bool subDirModified = false;
                foreach (string subFile in FileUtil.GetFiles(subDir))
                {
                    string lastFullBackupSubFile = FileUtil.CombinePath(lastFullBackupSubDir, FileUtil.GetFileName(subFile));

                    if (!FileUtil.ExistsFile(lastFullBackupSubFile) || FileUtil.GetLastWriteTime(subFile) > FileUtil.GetLastWriteTime(lastFullBackupSubFile))
                    {
                        subDirModified = true;
                        break; // No need to continue checking this subdirectory
                    }
                }

                // Proceed with the subdirectory only if modified
                if (subDirModified)
                {
                    FileUtil.CreateDirectory(targetSubDir);

                    // Proceed with the differential save on the subdirectory
                    ExecuteDifferentialSave(job, targetSubDir, lastFullBackupSubDir, totalFiles, totalSize, ref processedFiles, ref processedSize);
                }
            }

            string endMessage = $"Differential backup {job.Name} completed.";
            ConsoleUtil.PrintTextconsole(endMessage);
            LogManager.Instance.AddMessage(endMessage);*/
            }
        }

    }
}

