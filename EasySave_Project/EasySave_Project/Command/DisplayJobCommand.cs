using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.View;

namespace EasySave_Project.Command
{
    public class DisplayJobCommand : ICommand
    {

        private JobManager _jobManager;

        public DisplayJobCommand()
        {
            _jobManager = JobManager.GetInstance();
        }
        
        public void Execute()
        {
            if (_jobManager.Jobs.Count == 0)
            {
                Util.ConsoleUtil.PrintTextconsole("listEmpty");
                return;
            }
            foreach (JobModel job in _jobManager.Jobs)
            {
                Util.ConsoleUtil.PrintTextconsole(job.ToString());
            }
        }

        public void GetInstruction()
        {
            Console.WriteLine("Create Job Command");
        }
    }
}
