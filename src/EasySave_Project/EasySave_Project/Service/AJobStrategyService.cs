using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
        private static readonly SemaphoreSlim _largeFileSemaphore = new SemaphoreSlim(1, 1);
        private static int LargeFileThreshold = 1024 * 1000000; // 100 KB configurable threshold

        /// <summary>
        /// Checks if a given file format is in the list of encrypted file extensions.
        /// </summary>
        /// <param name="format">The file format to check (e.g., ".txt").</param>
        /// <returns>True if the format is present, otherwise False.</returns>
        public bool IsEncryptedFileFormat(string format)
        {
            try
            {
                // Retrieve the list of encrypted file formats from application settings
                List<string> encryptedFormats = FileUtil.GetAppSettingsList("EncryptedFileExtensions");

                // Check if the format exists in the list
                return encryptedFormats != null && encryptedFormats.Contains(format);
            }
            catch (Exception ex)
            {
                // Log an error message if an exception occurs
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorReadingFormat") + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Handles file copying, optional encryption, and logging.
        /// </summary>
        public long HandleFileOperation(string sourceFile, string targetFile, JobModel job, double progress)
        {
            TranslationService translator = TranslationService.GetInstance();
            FileUtil.CopyFile(sourceFile, targetFile, true);
            string formatFile = FileUtil.GetFileExtension(sourceFile);
            bool shouldEncrypt = IsEncryptedFileFormat(formatFile);
            Stopwatch stopwatch = new Stopwatch();
            double elapsedTime = 0;
            string message;

            if (shouldEncrypt)
            {
                try
                {
                    stopwatch.Start();
                    FileUtil.EncryptFile(targetFile, ConfigurationService.GetInstance().GetStringSetting("EncryptKey"));
                    message = $"{translator.GetText("fileCopiedAndEncrypted")}: {sourceFile} -> {targetFile}";
                    stopwatch.Stop();
                    elapsedTime = stopwatch.ElapsedMilliseconds;
                }
                catch (Exception ex)
                {
                    message = $"{translator.GetText("errorEncrypting")}: {ex.Message}";
                    elapsedTime = -1;
                }
            }
            else
            {
                message = $"{translator.GetText("fileCopied")}: {sourceFile} -> {targetFile}";
            }

            ConsoleUtil.PrintTextconsole(message);
            LogManager.Instance.AddMessage(message);
            long fileSize = FileUtil.GetFileSize(sourceFile);
            double transferTime = FileUtil.CalculateTransferTime(sourceFile, targetFile);
            LogManager.Instance.UpdateState(job.Name, sourceFile, targetFile, fileSize, transferTime, elapsedTime);
            OnProgressChanged?.Invoke(progress);
            return fileSize;
        }

        /// <summary>
        /// Updates the backup state in the StateManager during the backup process.
        /// </summary>
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
        }

        /// <summary>
        /// Copies a file while managing large file operations with a semaphore.
        /// </summary>
        protected void CopyFileWithSemaphore(string sourceFile, string targetDir, JobModel job, bool isLargeFile, ref int processedFiles, ref long processedSize)
        {
            string fileName = FileUtil.GetFileName(sourceFile);
            string targetFile = FileUtil.CombinePath(targetDir, fileName);
            long fileSize = HandleFileOperation(sourceFile, targetFile, job, 0);
            if (isLargeFile)
            {
                _largeFileSemaphore.Release();
            }
            processedFiles++;
            processedSize += fileSize;
        }

        /// <summary>
        /// Processes a queue of files, managing large files separately.
        /// </summary>
        protected void ProcessFilesInQueue(Queue<string> files, Queue<string> fallbackQueue, string targetDir, JobModel job, ref int processedFiles, ref long processedSize)
        {
            while (files.Count > 0)
            {
                string file = files.Dequeue();
                long fileSize = FileUtil.GetFileSize(file);
                bool isLargeFile = fileSize > LargeFileThreshold;
                if (isLargeFile && !_largeFileSemaphore.Wait(0))
                {
                    fallbackQueue.Enqueue(file);
                }
                else
                {
                    CopyFileWithSemaphore(file, targetDir, job, isLargeFile, ref processedFiles, ref processedSize);
                }
            }
        }

        /// <summary>
        /// Processes files in the fallback queue, retrying large files that were deferred earlier.
        /// </summary>
        protected void ProcessFallbackQueue(Queue<string> fallbackQueue, string targetDir, JobModel job, ref int processedFiles, ref long processedSize)
        {
            while (fallbackQueue.Count > 0)
            {
                string waitingFile = fallbackQueue.Dequeue();
                if (_largeFileSemaphore.Wait(0))
                {
                    CopyFileWithSemaphore(waitingFile, targetDir, job, true, ref processedFiles, ref processedSize);
                }
                else
                {
                    fallbackQueue.Enqueue(waitingFile);
                    Thread.Sleep(500); // Wait before retrying to avoid excessive CPU usage
                }
            }
        }
    }
}
