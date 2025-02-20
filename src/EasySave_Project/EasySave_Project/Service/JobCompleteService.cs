using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Strategy for complete backup jobs.
    /// This class implements the IJobStrategy interface and provides
    /// the functionality to execute a full backup of the specified job.
    /// </summary>
    public class JobCompleteService : AJobStrategyService
    {
        public event Action<double> OnProgressChanged;

        /// <summary>
        /// Executes the complete backup job for the given JobModel.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        /// <param name="backupDir">The directory where the backup will be stored.</param>
        public override void Execute(JobModel job, string backupDir)
        {
            string message = TranslationService.GetInstance().GetText("startingBackUpComplete") + job.Name;

            ConsoleUtil.PrintTextconsole(message);
            LogManager.Instance.AddMessage(message);

            // Retrieve total number of files and total size before starting the backup
            var allFiles = FileUtil.GetFilesRecursively(job.FileSource);
            int totalFiles = allFiles.Count;
            long totalSize = allFiles.Sum(FileUtil.GetFileSize);

            int processedFiles = 0;
            long processedSize = 0;

            // Execute the full backup process
            ExecuteCompleteSave(job.FileSource, backupDir, job, totalFiles, totalSize, ref processedFiles, ref processedSize);

            // Update the last full backup path
            job.LastFullBackupPath = backupDir;

            message = TranslationService.GetInstance().GetText("backupComplet") + " " + job.Name;
            ConsoleUtil.PrintTextconsole(message);
            LogManager.Instance.AddMessage(message);
        }

        /// <summary>
        /// Recursively copies all files and directories from the source directory to the target directory.
        /// </summary>
        /// <param name="sourceDir">The source directory to back up.</param>
        /// <param name="targetDir">The destination directory for the backup.</param>
        /// <param name="job">The job configuration.</param>
        /// <param name="totalFiles">Total number of files in the backup.</param>
        /// <param name="totalSize">Total size of all files in the backup.</param>
        /// <param name="processedFiles">Counter for processed files (used for progress tracking).</param>
        /// <param name="processedSize">Counter for processed data size (used for progress tracking).</param>
        private void ExecuteCompleteSave(string sourceDir, string targetDir, JobModel job, int totalFiles, long totalSize, ref int processedFiles, ref long processedSize)
        {
            // Queue to store normal-sized files to be copied first
            var files = new Queue<string>(FileUtil.GetFiles(sourceDir));
            // Queue for large files that will be processed later
            var fallbackQueue = new Queue<string>();

            // Process the queued files
            ProcessFilesInQueue(files, fallbackQueue, targetDir, job, ref processedFiles, ref processedSize);
            // Process large files after regular ones
            ProcessFallbackQueue(fallbackQueue, targetDir, job, ref processedFiles, ref processedSize);
            // Recursively process subdirectories
            ProcessSubdirectories(sourceDir, targetDir, job, totalFiles, totalSize, ref processedFiles, ref processedSize);
        }

        /// <summary>
        /// Recursively processes subdirectories and performs a full backup for each.
        /// </summary>
        /// <param name="sourceDir">The source directory.</param>
        /// <param name="targetDir">The target backup directory.</param>
        /// <param name="job">The job configuration.</param>
        /// <param name="totalFiles">Total number of files in the backup.</param>
        /// <param name="totalSize">Total size of all files in the backup.</param>
        /// <param name="processedFiles">Counter for processed files (used for progress tracking).</param>
        /// <param name="processedSize">Counter for processed data size (used for progress tracking).</param>
        private void ProcessSubdirectories(string sourceDir, string targetDir, JobModel job, int totalFiles, long totalSize, ref int processedFiles, ref long processedSize)
        {
            foreach (string subDir in FileUtil.GetDirectories(sourceDir))
            {
                // Get the subdirectory name and construct the target path
                string subDirName = FileUtil.GetFileName(subDir);
                string targetSubDir = FileUtil.CombinePath(targetDir, subDirName);

                // Create the subdirectory in the target location
                FileUtil.CreateDirectory(targetSubDir);

                // Recursively back up the subdirectory
                ExecuteCompleteSave(subDir, targetSubDir, job, totalFiles, totalSize, ref processedFiles, ref processedSize);
            }
        }
    }
}
