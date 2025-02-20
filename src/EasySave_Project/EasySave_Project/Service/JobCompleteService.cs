﻿using DynamicData;
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

            List<string> allFiles;

            int processedFiles = 0;
            long processedSize = 0;
            int totalFiles = 0;
            long totalSize = 0;

            if (job.SaveState.Equals(JobSaveStateEnum.PENDING))
            {
                allFiles = job.FileInPending.FilesInPending;
                job.SaveState = JobSaveStateEnum.ACTIVE;
                processedFiles = job.FileInPending.ProcessedFiles;
                processedSize = job.FileInPending.ProcessedSize;
                totalFiles = job.FileInPending.TotalFiles;
                totalSize = job.FileInPending.TotalSize;
            }
            else
            {
                // Retrieve total number of files and total size before starting the backup
                allFiles = FileUtil.GetAllFilesAndDirectories(job.FileSource);
                totalFiles = allFiles.Count;
                totalSize = allFiles.Sum(FileUtil.GetFileSize);
            }

            // Execute full backup process
            ExecuteCompleteSave(allFiles, backupDir, job, totalFiles, totalSize, ref processedFiles, ref processedSize);  
            
            // Update the last full backup path
            job.LastFullBackupPath = backupDir;
        }

        /// <summary>
        /// Executes a full backup by copying all specified files to the target directory.
        /// This method iterates through the list of files to copy, processes each file,
        /// updates the backup state, and tracks progress. It also manages pending files
        /// in case of an interrupted backup.
        /// </summary>
        /// <param name="filesToCopyPath">List of file paths to be backed up.</param>
        /// <param name="targetDir">The target directory where files will be copied.</param>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="totalFiles">The total number of files to be backed up.</param>
        /// <param name="totalSize">The total size of all files in the backup.</param>
        /// <param name="processedFiles">Reference to the counter tracking processed files.</param>
        /// <param name="processedSize">Reference to the counter tracking processed file size.</param>
        private void ExecuteCompleteSave(List<string> filesToCopyPath, string targetDir, JobModel job, int totalFiles, long totalSize, ref int processedFiles, ref long processedSize)
        {
            // Queue to store normal-sized files to be copied first
            var files = new Queue<string>(filesToCopyPath);
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
            List<string> pathToDelete = new List<string>();

            // Copy all files from the source directory
            foreach (string sourceFileWithAbsolutePath in filesToCopyPath)
            {
                string sourceFile = sourceFileWithAbsolutePath.Split(job.FileSource + "\\")[1];

                string fileName = FileUtil.GetFileName(sourceFile);
                string targetFile = FileUtil.CombinePath(targetDir, fileName);
                double progressPourcentage = (double)processedFiles / totalFiles * 100;

                changeJobStateIfBusinessProcessLaunching(job);

                if (!job.SaveState.Equals(JobSaveStateEnum.PENDING))
                {
                    string targetPathComplete = FileUtil.CombinePath(targetDir, sourceFile);

                    long fileSize = HandleFileOperation(sourceFileWithAbsolutePath, targetPathComplete, job, progressPourcentage);

                    // Update processed file count and total processed size
                    processedFiles++;
                    processedSize += fileSize;
                    UpdateBackupState(job, processedFiles, processedSize, totalFiles, totalSize, sourceFile, targetFile, progressPourcentage);

                    pathToDelete.Add(sourceFileWithAbsolutePath);   
                }
                else
                {
                    break;
                }
            }

            filesToCopyPath.RemoveAll(path => pathToDelete.Contains(path));
            SaveFileInPending(job, filesToCopyPath, processedFiles, processedSize, totalFiles, totalSize);
        }
    }
}
