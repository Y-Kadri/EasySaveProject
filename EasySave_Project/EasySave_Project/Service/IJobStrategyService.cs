using System;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Defines the contract for backup job strategies.
    /// Implementations of this interface must provide a concrete 
    /// execution method for different types of backup jobs.
    /// </summary>
    public interface IJobStrategyService
    {
        /// <summary>
        /// Executes the backup job using the specified job model and target backup directory.
        /// </summary>
        /// <param name="job">The JobModel object representing the job to execute.</param>
        /// <param name="backupDir">The directory where the backup will be stored.</param>
        void Execute(JobModel job, string backupDir);
    }
}
