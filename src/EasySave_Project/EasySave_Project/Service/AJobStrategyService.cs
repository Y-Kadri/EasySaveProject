using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Dto;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Abstract class defining a backup strategy.
    /// Implements the IJobStrategyService interface and enforces the implementation 
    /// of the Execute method in derived classes.
    /// </summary>
    public abstract class AJobStrategyService : IJobStrategyService
    {
        /// <summary>
        /// Executes the backup job using the specified job model and target backup directory.
        /// This method must be implemented by derived classes.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        /// <param name="backupDir">The directory where the backup will be stored.</param>
        public abstract void Execute(JobModel job, string backupDir);

        public event Action<double>? OnProgressChanged;

        /// <summary>
        /// Checks if a given file format is in the list of encrypted file extensions.
        /// </summary>
        /// <param name="format">The file format to check (e.g., ".txt").</param>
        /// <returns>True if the format is present, otherwise False.</returns>
        public bool IsEncryptedFileFormat(string format)
        {
            try
            {
                // Retrieve the list of encrypted file formats
                List<string> encryptedFormats = FileUtil.GetAppSettingsList("EncryptedFileExtensions");

                // Check if the format exists in the list
                return encryptedFormats != null && encryptedFormats.Contains(format);
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorReadingFormat") + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Common method to handle file copying, encryption, and logging.
        /// </summary>
        /// <param name="sourceFile">The source file to copy.</param>
        /// <param name="targetDir">The target directory where the file will be copied.</param>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        /// <returns>Elapsed time for encryption or 0 if no encryption occurred.</returns>
        public long HandleFileOperation(string sourcePath, string targetPath, JobModel job, double progress)
        {
            TranslationService translator = TranslationService.GetInstance();

            // Copy file to target
            FileUtil.CopyFile(sourcePath, targetPath, true);

            string formatFile = FileUtil.GetFileExtension(sourcePath);
            bool shouldEncrypt = IsEncryptedFileFormat(formatFile);


            Stopwatch stopwatch = new Stopwatch();
            double elapsedTime = 0;

            string message;

            // If encryption option is enabled, encrypt the file after copying
            if (shouldEncrypt)
            {
                try
                {
                    stopwatch.Start();
                    FileUtil.EncryptFile(targetPath, ConfigurationService.GetInstance().GetStringSetting("EncryptKey"));
                    message = $"{translator.GetText("fileCopiedAndEncrypted")}: {sourcePath} -> {targetPath}";
                    stopwatch.Stop();
                    elapsedTime = stopwatch.ElapsedMilliseconds;

                    // Log message after encryption
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                }
                catch (Exception ex)
                {
                    message = $"{translator.GetText("errorEncrypting")}: {ex.Message}";
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    elapsedTime = -1;
                }
            }
            else
            {
                message = $"{translator.GetText("fileCopied")}: {sourcePath} -> {targetPath}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
            }

            // Calculate file size and transfer time
            long fileSize = FileUtil.GetFileSize(sourcePath);
            double transferTime = FileUtil.CalculateTransferTime(sourcePath, targetPath);

            // Update state in LogManager
            LogManager.Instance.UpdateState(job.Name, sourcePath, targetPath, fileSize, transferTime, elapsedTime);

            job.FileInPending.Progress = progress;

            OnProgressChanged?.Invoke(progress);

            return fileSize;
        }

        /// <summary>
        /// Updates the backup state in the StateManager during the backup process.
        /// </summary>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="processedFiles">The current number of processed files.</param>
        /// <param name="processedSize">The current total size of processed files.</param>
        /// <param name="totalFiles">Total number of files to be processed.</param>
        /// <param name="totalSize">Total size of the files to be backed up.</param>
        /// <param name="currentSourceFilePath">The path of the current source file being processed.</param>
        /// <param name="currentDestinationFilePath">The path of the destination file being processed.</param>
        protected void UpdateBackupState(JobModel job, int processedFiles, long processedSize, int totalFiles, long totalSize, string currentSourceFilePath, string currentDestinationFilePath, double progressPourcentage)
        {
            StateManager.Instance.UpdateState(new BackupJobState
            {
                JobName = job.Name,
                LastActionTimestamp = DateUtil.GetTodayDate(DateUtil.YYYY_MM_DD_HH_MM_SS),
                JobStatus = job.SaveState.ToString(),
                TotalEligibleFiles = totalFiles,
                TotalFileSize = totalSize,
                Progress = progressPourcentage,
                RemainingFiles = totalFiles - processedFiles,
                RemainingFileSize = totalSize - processedSize,
                CurrentSourceFilePath = currentSourceFilePath,
                CurrentDestinationFilePath = currentDestinationFilePath
            });
            job.FileInPending.Progress = progressPourcentage;
            job.FileInPending.ProcessedFiles = processedFiles;
            job.FileInPending.ProcessedSize = processedSize;
            job.FileInPending.TotalFiles = totalFiles;
            job.FileInPending.TotalSize = totalSize;
        }

        /// <summary>
        /// Saves the current state of pending files in a backup job.
        /// This method updates the JobModel with details about the files yet to be processed,
        /// along with progress metrics such as the number of processed files, processed size,
        /// total files, and total size.
        /// </summary>
        /// <param name="job">The JobModel representing the backup job.</param>
        /// <param name="filesToSave">The list of file paths that are still pending processing.</param>
        /// <param name="processedFiles">The number of files that have been processed so far.</param>
        /// <param name="processedSize">The total size of the files that have been processed.</param>
        /// <param name="totalFiles">The total number of files in the backup job.</param>
        /// <param name="totalSize">The total size of all files in the backup job.</param>
        protected void SaveFileInPending(JobModel job, List<string> filesToSave, int processedFiles, long processedSize, int totalFiles, long totalSize)
        {
            FileInPendingJobDTO fileInPendingJobDTO = new FileInPendingJobDTO();
            fileInPendingJobDTO.FilesInPending = filesToSave;
            fileInPendingJobDTO.Progress = job.FileInPending.Progress;
            fileInPendingJobDTO.ProcessedFiles = processedFiles;
            fileInPendingJobDTO.ProcessedSize = processedSize; 
            fileInPendingJobDTO.TotalFiles = totalFiles;
            fileInPendingJobDTO.TotalSize = totalSize;
            job.FileInPending = fileInPendingJobDTO;
        }
    }
}
