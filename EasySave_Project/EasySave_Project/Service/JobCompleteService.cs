using EasySave_Library_Log;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using EasySave_Library_Log;
using EasySave_Library_Log.manager;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Strategy for complete backup jobs.
    /// This class implements the IJobStrategy interface and provides
    /// the functionality to execute a complete backup of the specified job.
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
            ExecuteCompleteSave(job.FileSource, backupDir); // Perform the complete backup
            job.LastFullBackupPath = backupDir; // Update the last full backup path
        }

        /// <summary>
        /// Implements the logic for performing a complete backup.
        /// This method copies all files and subdirectories from the source directory
        /// to the target directory recursively.
        /// </summary>
        /// <param name="sourceDir">The source directory to backup.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        private void ExecuteCompleteSave(string sourceDir, string targetDir)
        {
            // Copy all files from the source directory
            foreach (string sourceFile in FileUtil.GetFiles(sourceDir))
            {
                string fileName = FileUtil.GetFileName(sourceFile);
                string targetFile = FileUtil.CombinePath(targetDir, fileName);
                FileUtil.CopyFile(sourceFile, targetFile, true); // Copy file to target

                // Calculate file size and transfer time
                long fileSize = FileUtil.GetFileSize(sourceFile);
                double transferTime = FileUtil.CalculateTransferTime(sourceFile, targetFile);

                // Log the operation
                LogManager.Instance.UpdateState(
                    jobName: "test",
                    sourcePath: sourceFile,
                    targetPath: targetFile,
                    fileSize: fileSize,
                    transferTime: transferTime
                );
            }

            // Recursively copy all subdirectories
            foreach (string subDir in FileUtil.GetDirectories(sourceDir))
            {
                string subDirName = FileUtil.GetDirectoryName(subDir);
                string targetSubDir = FileUtil.CombinePath(targetDir, subDirName);
                FileUtil.CreateDirectory(targetSubDir);
                ExecuteCompleteSave(subDir, targetSubDir); // Recursive call
            }
        }
    }
}
