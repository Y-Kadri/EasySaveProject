using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.View;

namespace EasySave_Project.Command
{
    public class CreateJobCommand : ICommand
    {

        private ConsoleView _consoleView;

        public CreateJobCommand()
        {
            _consoleView = new ConsoleView();
        }
        
        public void Execute()
        {
            while (true)
            {
                Util.ConsoleUtil.PrintTextconsole("entrerNom");
                string name = Util.ConsoleUtil.GetInputString();
                
                Util.ConsoleUtil.PrintTextconsole("entrerFileSource");
                string fileSource = Util.ConsoleUtil.GetInputString();
                
                Util.ConsoleUtil.PrintTextconsole("entrerFileTarget");
                string fileTarget = Util.ConsoleUtil.GetInputString();
                
                Util.ConsoleUtil.PrintTextconsole("entrerJobType");
                JobSaveTypeEnum jobSaveTypeEnum = Util.ConsoleUtil.GetInputJobSaveTypeEnum();
                JobManager jobMana = JobManager.GetInstance();
                
                jobMana.CreateAndAddJob(name, fileSource, fileTarget, jobSaveTypeEnum);
                Util.ConsoleUtil.PrintTextconsole("jobCree");
                break;
            }
        }

        public void GetInstruction()
        {
            Console.WriteLine("Create Job Command");
        }
    }
}
