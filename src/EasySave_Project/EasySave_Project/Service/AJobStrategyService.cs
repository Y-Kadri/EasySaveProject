using EasySave_Project.Model;
using EasySave_Project.Util;
using System;
using System.Collections.Generic;
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
    }
}
