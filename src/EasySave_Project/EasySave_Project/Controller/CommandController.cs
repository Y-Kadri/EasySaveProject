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
        private readonly List<ICommand> Commands;
        public CommandController()
        {

            Commands = new List<ICommand>();
            Commands.Add(new DisplayJobCommand());
            Commands.Add(new CreateJobCommand());
            Commands.Add(new ExecuteJobCommand());
            Commands.Add(new ExecuteAllJobCommand());
            Commands.Add(new ExitCommand());
        }
        public void LaunchCommand(int choice)
        {
            (choice > 0 && choice-1 < Commands.Count && Commands[choice-1] != null
                ? (Action)(() => Commands[choice-1].Execute())
                : () => Console.WriteLine(TranslationService.GetInstance().GetText("invalidChoice"))
            )();
        }

    }
}
