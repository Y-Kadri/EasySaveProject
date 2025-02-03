using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Model;
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
                ExecuteDifferentialSave(job, backupDir, job.LastFullBackupPath); // Execute differential backup
            }
        }

        /// <summary>
        /// Implements the logic for performing a differential backup.
        /// This method copies only modified files from the source directory
        /// to the target directory based on the last full backup.
        /// </summary>
        /// <param name="job">The JobModel object representing the job.</param>
        /// <param name="targetDir">The target directory where the backup will be stored.</param>
        /// <param name="lastFullBackupDir">The last full backup directory used for comparison.</param>
        private void ExecuteDifferentialSave(JobModel job, string targetDir, string lastFullBackupDir)
        {
            // Calculer le nombre total de fichiers et la taille totale
            long totalSize = FileUtil.CalculateTotalSize(job.FileSource);
            int totalFiles = FileUtil.GetFiles(job.FileSource).Count();
            int processedFiles = 0;
            long processedSize = 0;

            // Copier les fichiers modifiés du répertoire source
            foreach (string sourceFile in FileUtil.GetFiles(job.FileSource))
            {
                string fileName = FileUtil.GetFileName(sourceFile);
                string lastFullBackupFile = FileUtil.CombinePath(lastFullBackupDir, fileName);
                string targetFile = FileUtil.CombinePath(targetDir, fileName);

                // Copier le fichier s'il n'existe pas dans la dernière sauvegarde ou s'il a été modifié
                if (!FileUtil.ExistsFile(lastFullBackupFile) ||
                    (FileUtil.GetLastWriteTime(sourceFile) > FileUtil.GetLastWriteTime(lastFullBackupFile)))
                {
                    FileUtil.CopyFile(sourceFile, targetFile, true); // Copier le fichier modifié

                    // Calculer la taille du fichier et le temps de transfert
                    long fileSize = FileUtil.GetFileSize(sourceFile);
                    double transferTime = FileUtil.CalculateTransferTime(sourceFile, targetFile);

                    // Journaliser l'opération
                    LogManager.Instance.UpdateState(
                        jobName: job.Name,
                        sourcePath: sourceFile,
                        targetPath: targetFile,
                        fileSize: fileSize,
                        transferTime: transferTime
                    );

                    // Mettre à jour les fichiers et tailles traités
                    processedFiles++;
                    processedSize += fileSize;

                    // Mettre à jour l'état dans le StateManager
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
            }

            // Copier récursivement les sous-répertoires modifiés
            foreach (string subDir in FileUtil.GetDirectories(job.FileSource))
            {
                string subDirName = FileUtil.GetDirectoryName(subDir);
                string lastFullBackupSubDir = FileUtil.CombinePath(lastFullBackupDir, subDirName);
                string targetSubDir = FileUtil.CombinePath(targetDir, subDirName);
                FileUtil.CreateDirectory(targetSubDir);
                ExecuteDifferentialSave(job, targetSubDir, lastFullBackupSubDir); // Appel récursif
            }
        }
    }
}
