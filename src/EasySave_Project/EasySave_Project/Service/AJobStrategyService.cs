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
        private static int LargeFileThreshold = 100 * 1024 * 1024;

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
        protected void UpdateBackupState(JobModel job, int processedFiles, long processedSize, int totalFiles, long totalSize, string currentSourceFilePath, string currentDestinationFilePath, double progressPourcentage, string lastDateTimePath)
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
            job.FileInPending.LastDateTimePath = lastDateTimePath;
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
        protected void SaveFileInPending(JobModel job, List<string> filesToSave, int processedFiles, long processedSize, int totalFiles, long totalSize, string lastDateTimePath)
        {
            FileInPendingJobDTO fileInPendingJobDTO = new FileInPendingJobDTO();
            fileInPendingJobDTO.FilesInPending = filesToSave;
            fileInPendingJobDTO.Progress = job.FileInPending.Progress;
            fileInPendingJobDTO.ProcessedFiles = processedFiles;
            fileInPendingJobDTO.ProcessedSize = processedSize; 
            fileInPendingJobDTO.TotalFiles = totalFiles;
            fileInPendingJobDTO.TotalSize = totalSize;
            fileInPendingJobDTO.LastDateTimePath = lastDateTimePath;
            job.FileInPending = fileInPendingJobDTO;
        }

        protected void changeJobStateIfBusinessProcessLaunching(JobModel job)
        {
            // Check if a priority process is currently running
            var processes = FileUtil.GetAppSettingsList("PriorityBusinessProcess");
            (bool isRunning, string PName) = ProcessUtil.IsProcessRunning(processes);
            if (isRunning)
            {
                string Pmessage = TranslationService.GetInstance().GetText("interuptJob") + " " + PName;
                ConsoleUtil.PrintTextconsole(Pmessage);

                // Update the job status to SKIPPED
                job.SaveState = JobSaveStateEnum.PENDING;
            }
        }

        protected void ProcessFilesInQueue(Queue<string> files, Queue<string> fallbackQueue, string targetDir, JobModel job, ref int processedFiles, ref long processedSize, List<string> filesToCopyPath, int totalFiles, long totalSize)
        {
            bool end = false;

            while (files.Count > 0 && !end)
            {
                if (job.SaveState.Equals(JobSaveStateEnum.CANCEL))
                {
                    end = true;
                } else
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
                        end = CopyFileWithSemaphore(
                            file,
                            targetDir,
                            job,
                            isLargeFile,
                            ref processedFiles,
                            ref processedSize,
                            filesToCopyPath,
                            totalFiles,
                            totalSize);
                    }
                }
            }
        }

        protected void ProcessFallbackQueue(Queue<string> fallbackQueue, string targetDir, JobModel job, ref int processedFiles, ref long processedSize, List<string> filesToCopyPath, int totalFiles, long totalSize)
        {
            bool end = false;

            while (fallbackQueue.Count > 0 && !end)
            {
                if (job.SaveState.Equals(JobSaveStateEnum.CANCEL))
                {
                    end = true;
                } else
                {
                    string waitingFile = fallbackQueue.Dequeue();
                    if (_largeFileSemaphore.Wait(0))
                    {
                        end = CopyFileWithSemaphore(
                            waitingFile,
                            targetDir,
                            job,
                            true,
                            ref processedFiles,
                            ref processedSize,
                            filesToCopyPath,
                            totalFiles,
                            totalSize);
                    }
                    else
                    {
                        fallbackQueue.Enqueue(waitingFile);
                        Thread.Sleep(500);
                    }
                }
            }
        }

        protected bool CopyFileWithSemaphore(string sourceFileWithAbsolutePath, string targetDir, JobModel job, bool isLargeFile, ref int processedFiles, ref long processedSize, List<string> filesToCopyPath, int totalFiles, long totalSize)
        {
            string sourceFile = sourceFileWithAbsolutePath.Split(job.FileSource + "\\")[1];
            string fileName = FileUtil.GetFileName(sourceFile);
            string targetFile = FileUtil.CombinePath(targetDir, fileName);
            double progressPourcentage = (double)processedFiles / totalFiles * 100;
            bool end = false;

            List<string> pathToDelete = new List<string>();

            changeJobStateIfBusinessProcessLaunching(job);

            if (!job.SaveState.Equals(JobSaveStateEnum.PENDING) && !job.SaveState.Equals(JobSaveStateEnum.CANCEL))
            {
                string targetPathComplete = FileUtil.CombinePath(targetDir, sourceFile);

                long fileSize = HandleFileOperation(sourceFileWithAbsolutePath, targetPathComplete, job, progressPourcentage);

                processedFiles++;
                processedSize += fileSize;
                pathToDelete.Add(sourceFileWithAbsolutePath);

            } else {
                end = true;
            }
            UpdateBackupState(job, processedFiles, processedSize, totalFiles, totalSize, sourceFile, targetFile, progressPourcentage, job.FileInPending.LastDateTimePath);

            if (isLargeFile)
            {
                _largeFileSemaphore.Release();
            }

            filesToCopyPath.RemoveAll(path => pathToDelete.Contains(path));
            SaveFileInPending(job, filesToCopyPath, processedFiles, processedSize, totalFiles, totalSize, job.FileInPending.LastDateTimePath);

            return end;
        }
    }
}
