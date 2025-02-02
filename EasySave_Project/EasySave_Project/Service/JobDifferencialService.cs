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

            // Check for previous full backup
            if (string.IsNullOrEmpty(job.LastFullBackupPath) || !FileUtil.ExistsDirectory(job.LastFullBackupPath))
            {
                Console.WriteLine($"{translator.GetText("noPreviousFullBackup")}");
                job.LastFullBackupPath = null; // Reset last full backup path
                new JobCompleteService().Execute(job, backupDir); // Perform a complete backup
            }
            else
            {
                ExecuteDifferentialSave(job.FileSource, backupDir, job.LastFullBackupPath); // Execute differential backup
            }
        }

        /// <summary>
        /// Implements the logic for performing a differential backup.
        /// This method copies only modified files from the source directory
        /// to the target directory based on the last full backup.
        /// </summary>
        /// <param name="sourceDir">The source directory to backup.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="lastFullBackupDir">The last full backup directory used for comparison.</param>
        private void ExecuteDifferentialSave(string sourceDir, string targetDir, string lastFullBackupDir)
        {
            // Copy modified files from the source directory
            foreach (string sourceFile in FileUtil.GetFiles(sourceDir))
            {
                string fileName = FileUtil.GetFileName(sourceFile);
                string lastFullBackupFile = FileUtil.CombinePath(lastFullBackupDir, fileName);
                string targetFile = FileUtil.CombinePath(targetDir, fileName);

                // Copy file if it doesn't exist in last backup or is modified
                if (!FileUtil.ExistsFile(lastFullBackupFile) ||
                    (FileUtil.GetLastWriteTime(sourceFile) > FileUtil.GetLastWriteTime(lastFullBackupFile)))
                {
                    FileUtil.CopyFile(sourceFile, targetFile, true); // Copy modified file
                }
            }

            // Recursively copy modified subdirectories
            foreach (string subDir in FileUtil.GetDirectories(sourceDir))
            {
                string subDirName = FileUtil.GetDirectoryName(subDir);
                string lastFullBackupSubDir = FileUtil.CombinePath(lastFullBackupDir, subDirName);
                string targetSubDir = FileUtil.CombinePath(targetDir, subDirName);
                FileUtil.CreateDirectory(targetSubDir);
                ExecuteDifferentialSave(subDir, targetSubDir, lastFullBackupSubDir); // Recursive call
            }
        }
    }
}
