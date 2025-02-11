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
    public class UpdateCryptoExtensionsCommand : ICommand
    {
        private readonly JobService _jobService = new JobService();
        private readonly ConsoleView _consoleView = new ConsoleView();

        /// <summary>
        /// Executes the command to update the list of file formats that can be encrypted.
        /// Displays the current list and prompts the user to add a new format.
        /// </summary>
        public void Execute()
        {
            // Retrieve the current list of encrypted file extensions
            List<string> currentFormats = _jobService.GetEncryptedFileExtensions();

            // Display the current formats if not empty
            if (currentFormats != null && currentFormats.Count > 0)
            {
                ConsoleUtil.PrintTextconsole("Current encrypted file formats:");
                foreach (var format in currentFormats)
                {
                    ConsoleUtil.PrintTextconsole(format);
                }
            }
            else
            {
                ConsoleUtil.PrintTextconsole("No encrypted file formats are currently set.");
            }

            // Prompt the user for a new format to add
            ConsoleUtil.PrintTextconsole("Enter the file format to add (e.g., 'txt' or 'pdf'):");
            string inputFormat = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(inputFormat))
            {
                // Add the new format to the settings
                _jobService.AddFormatToEasySaveSettingsForScryptFile(inputFormat);
            }
            else
            {
                ConsoleUtil.PrintTextconsole("Invalid format. Please enter a valid file extension.");
            }
        }

        /// <summary>
        /// Displays an instruction message to the user about how to update the encrypted file formats.
        /// </summary>
        public void GetInstruction()
        {
            // Get the translated instruction text for updating crypto extensions
            ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("enterLogFormatChoose"));
        }
    }
}
