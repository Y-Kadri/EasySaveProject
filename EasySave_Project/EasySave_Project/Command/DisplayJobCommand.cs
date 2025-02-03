using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.View;

namespace EasySave_Project.Command
{
    /// <summary>
    /// Command class responsible for displaying the list of backup jobs.
    /// Implements the ICommand interface.
    /// </summary>
    public class DisplayJobCommand : ICommand
    {
        // Instance of JobManager to access the list of jobs
        private JobManager _jobManager;

        /// <summary>
        /// Constructor initializes the JobManager instance.
        /// </summary>
        public DisplayJobCommand()
        {
            _jobManager = JobManager.GetInstance(); // Get the singleton instance of JobManager
        }

        /// <summary>
        /// Executes the command to display all jobs.
        /// If the job list is empty, it informs the user.
        /// </summary>
        public void Execute()
        {
            // Check if the job list is empty
            if (_jobManager.Jobs.Count == 0)
            {
                // Notify user that the job list is empty
                Util.ConsoleUtil.PrintTextconsole("listEmpty");
                return; // Exit the method if there are no jobs
            }

            // Iterate through the list of jobs and display each job
            foreach (JobModel job in _jobManager.Jobs)
            {
                Util.ConsoleUtil.PrintTextconsole(job.ToString()); // Print the job details
            }
        }

        /// <summary>
        /// Displays the command instruction.
        /// </summary>
        public void GetInstruction()
        {
            Console.WriteLine("Display Job Command"); // Indicate the purpose of this command
        }
    }
}
