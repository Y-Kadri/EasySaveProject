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
        private TranslationService TranslationService;
        private readonly ConsoleUtil ConsoleUtil;
        private readonly ICommand CreateJobCommand;
        private readonly ICommand ExecuteJobCommand;
        private readonly ICommand ExecuteAllJobCommand;
        private readonly ICommand ExitCommand;
        public CommandController()
        {
            TranslationService = new TranslationService();
            ConsoleUtil = new ConsoleUtil();
            CreateJobCommand = new CreateJobCommand();
            ExecuteJobCommand = new ExecuteJobCommand();
            ExecuteAllJobCommand = new ExecuteAllJobCommand();
            ExitCommand = new ExitCommand();
        }

        public void ChoiceCommande()
        {
            ConsoleUtil.printTextconsole("1. Créer une tâche de sauvegarde \n2. Exécuter une tâche spécifiquee \n3. Exécuter toutes les tâches \n4. Quitter");

            int choice = ConsoleUtil.GetInputInt();

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
                    Console.WriteLine("invalid_choice"); //TODO translate
                    break;
            }
        }
    }
}
