using System;
using EasySave_Project.Model;

namespace EasySave_Project.Service
{
    /// <summary>
    /// Service responsible for logging changes in observed <see cref="JobModel"/> instances.
    /// Implements <see cref="JobObserverService"/> to receive updates when a job's state changes.
    /// </summary>
    public class LogService : JobObserverService
    {
        /// <summary>
        /// Called when an observed <see cref="JobModel"/> changes.
        /// Logs the change to the console.
        /// </summary>
        /// <param name="job">The <see cref="JobModel"/> instance that has been updated.</param>
        public override void OnNext(JobModel job)
        {
            Console.WriteLine($"[LOG] Job");
        }
    }
}
