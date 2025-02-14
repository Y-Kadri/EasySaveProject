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
    public class AddCryptoExtensionsCommand : ICommand
    {
        private readonly JobService _jobService = new JobService();
        private readonly ConsoleView _consoleView = new ConsoleView();
        private readonly TranslationService translator = TranslationService.GetInstance();
        private readonly string JOB_SETTINGS_KEY = "EncryptedFileExtensions";
        /// <summary>
        /// Executes the command to update the list of file formats that can be encrypted.
        /// Displays the current list and prompts the user to add a new format.
        /// </summary>
        public void Execute()
        {
            // Retrieve the current list of encrypted file extensions
            ConsoleUtil.PrintTextconsole(translator.GetText("retrieve" + JOB_SETTINGS_KEY));
            List<string> currentData = _jobService.GetJobSettingsList(JOB_SETTINGS_KEY);

            _consoleView.ShowCurrentFormatCrypt(currentData);


            string inputData = _consoleView.ElementWrited("promptEnterFileFormat");

            if (!string.IsNullOrWhiteSpace(inputData))
            {
                // Add the new format to the settings
                ConsoleUtil.PrintTextconsole(translator.GetText("addFormatToSettings"));
                _jobService.AddValueToJobSettingsList(JOB_SETTINGS_KEY, inputData);
            }
            else
            {
                ConsoleUtil.PrintTextconsole(translator.GetText("invalidFileFormat"));
            }
        }

        /// <summary>
        /// Displays an instruction message to the user about how to update the encrypted file formats.
        /// </summary>
        public void GetInstruction()
        {
            // Get the translated instruction text for updating crypto extensions
            ConsoleUtil.PrintTextconsole(translator.GetText("enterCryptoExtensionsInstruction"));
        }
    }
}
