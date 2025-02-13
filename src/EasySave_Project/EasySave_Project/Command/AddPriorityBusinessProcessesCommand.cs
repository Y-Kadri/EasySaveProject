using EasySave_Project.Dto;
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
    public class AddPriorityBusinessProcessesCommand : ICommand
    {
        private readonly JobService _jobService = new JobService();
        private readonly ConsoleView _consoleView = new ConsoleView();
        private readonly TranslationService translator = TranslationService.GetInstance();
        private readonly string JOB_SETTINGS_KEY = "PriorityBusinessProcess";

        /// <summary>
        /// Executes the command to update the list of process
        /// Displays the current list and prompts the user to add a new process.
        /// </summary>
        public void Execute()
        {
            List<string> currentData = _jobService.GetJobSettingsList(JOB_SETTINGS_KEY);
            _consoleView.ShowCurrentPriorityBusinessProcess(currentData);
            string inputData = _consoleView.ElementWrited("promptEnterBusinessProcess");

            if (!string.IsNullOrWhiteSpace(inputData))
            {
                // Add the new format to the settings
                ConsoleUtil.PrintTextconsole(translator.GetText("addProcessToSettings"));
                _jobService.AddValueToJobSettingsList(JOB_SETTINGS_KEY, inputData);
            }
            else
            {
                ConsoleUtil.PrintTextconsole(translator.GetText("invalidProcess"));
            }
        }

        /// <summary>
        /// Displays an instruction message to the user about how to update the process.
        /// </summary>
        public void GetInstruction()
        {
            // Get the translated instruction text for updating crypto extensions
            ConsoleUtil.PrintTextconsole(translator.GetText("enterBusinessProcessInstruction"));
        }
    }
}
