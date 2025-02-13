using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using System.Diagnostics;
using System.Linq;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Strategy for complete backup jobs.
    /// This class implements the IJobStrategy interface and provides
    /// the functionality to execute a full backup of the specified job.
    /// </summary>
    public class JobCompleteService : AJobStrategyService
    {
        /// <summary>
        /// Executes the complete backup job for the given JobModel.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        /// <param name="backupDir">The directory where the backup will be stored.</param>
        override
        public void Execute(JobModel job, string backupDir)
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

            // Execute full backup process
            ExecuteCompleteSave(job.FileSource, backupDir, job, totalFiles, totalSize, ref processedFiles, ref processedSize);

            // Update the last full backup path
            job.LastFullBackupPath = backupDir;

            message = TranslationService.GetInstance().GetText("backupComplet") + " " + job.Name;
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
        /// <param name="totalFiles">Total number of files to be backed up.</param>
        /// <param name="totalSize">Total size of all files to be backed up.</param>
        /// <param name="processedFiles">Reference to the counter for processed files.</param>
        /// <param name="processedSize">Reference to the counter for processed size.</param>
        private void ExecuteCompleteSave(string sourceDir, string targetDir, JobModel job, int totalFiles, long totalSize, ref int processedFiles, ref long processedSize)
        {
            var files = FileUtil.GetFiles(sourceDir);
            TranslationService translator = TranslationService.GetInstance();

            foreach (string sourceFile in files)
            {
                string fileName = FileUtil.GetFileName(sourceFile);
                string targetFile = FileUtil.CombinePath(targetDir, fileName);
                long fileSize = HandleFileOperation(sourceFile, targetFile, job);

                // Update processed file count and total processed size
                processedFiles++;
                processedSize += fileSize;

                UpdateBackupState(job, processedFiles, processedSize, totalFiles, totalSize, sourceFile, targetFile);
            }

            // Recursively process subdirectories
            foreach (string subDir in FileUtil.GetDirectories(sourceDir))
            {
                string fileName = FileUtil.GetFileName(subDir);
                string targetSubDir = FileUtil.CombinePath(targetDir, fileName);
                FileUtil.CreateDirectory(targetSubDir);

                ExecuteCompleteSave(subDir, targetSubDir, job, totalFiles, totalSize, ref processedFiles, ref processedSize);
            }
        }
    }
}
