using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using System.Linq;

namespace EasySave_Project.Service
{
    public class JobCompleteService : IJobStrategyService
    {
        public void Execute(JobModel job, string backupDir)
        {
            ExecuteCompleteSave(job.FileSource, backupDir, job); // Perform the complete backup
            job.LastFullBackupPath = backupDir; // Update the last full backup path
        }

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
                    LastActionTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
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
