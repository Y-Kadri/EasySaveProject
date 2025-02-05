using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Util;
using EasySave_Project.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Command
{
    public class ExitCommand : ICommand
    {
        public void Execute()
        {
            // Display the instruction message first
            GetInstruction();

            // Exit the application
            Environment.Exit(0);
        }

        public void GetInstruction()
        {
            // Display instruction for the exit command
            ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("exitApplicationCommand"));
        }
    }
}
