using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EasySave_Project.Command;
using EasySave_Project.Service;
using EasySave_Project.Util;

namespace EasySave_Project.Controller
{
    public class CommandController
    {
        private readonly ICommand CreateJobCommand;
        private readonly ICommand ExecuteJobCommand;
        private readonly ICommand ExecuteAllJobCommand;
        private readonly ICommand ExitCommand;
        public CommandController()
        {
          
            CreateJobCommand = new CreateJobCommand();
            ExecuteJobCommand = new ExecuteJobCommand();
            ExecuteAllJobCommand = new ExecuteAllJobCommand();
            ExitCommand = new ExitCommand();
        }
        public void LaunchCommand(int choice)
        {

            switch (choice) {
                case 1:
                    CreateJobCommand.Execute();
                    break;
                case 2:
                    ExecuteJobCommand.Execute();
                    break;
                case 3:
                    ExecuteAllJobCommand.Execute();
                    break;
                case 4:
                    ExitCommand.Execute();
                    break;
                default:
                    Console.WriteLine(TranslationService.GetInstance().GetText("invalidChoice"));
                    break;
            }
        }
    }
}
