using EasySave_Library_Log;
using EasySave_Library_Log.manager;
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
                List<string> encryptedFormats = FileUtil.GetEncryptedFileExtensions();

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
        public long HandleFileOperation(string sourceFile, string targetFile, JobModel job)
        {
            TranslationService translator = TranslationService.GetInstance();

            // Copy file to target
            FileUtil.CopyFile(sourceFile, targetFile, true);

            string formatFile = FileUtil.GetFileExtension(sourceFile);
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
                    FileUtil.EncryptFile(targetFile, "Cesi2004@+");
                    message = $"{translator.GetText("fileCopiedAndEncrypted")}: {sourceFile} -> {targetFile}";
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
                message = $"{translator.GetText("fileCopied")}: {sourceFile} -> {targetFile}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
            }

            // Calculate file size and transfer time
            long fileSize = FileUtil.GetFileSize(sourceFile);
            double transferTime = FileUtil.CalculateTransferTime(sourceFile, targetFile);

            // Update state in LogManager
            LogManager.Instance.UpdateState(job.Name, sourceFile, targetFile, fileSize, transferTime, elapsedTime);

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
        protected void UpdateBackupState(JobModel job, int processedFiles, long processedSize, int totalFiles, long totalSize, string currentSourceFilePath, string currentDestinationFilePath)
        {
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
                CurrentSourceFilePath = currentSourceFilePath,
                CurrentDestinationFilePath = currentDestinationFilePath
            });
        }
    }
}
