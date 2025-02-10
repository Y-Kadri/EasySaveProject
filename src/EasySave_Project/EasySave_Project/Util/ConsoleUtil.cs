using EasySave_Project.Service;
using System;
using EasySave_Project.Model;

namespace EasySave_Project.Util
{
    /// <summary>
    /// Utility class for handling console input and output.
    /// Provides methods for printing translated messages and retrieving user input.
    /// </summary>
    public static class ConsoleUtil
    {
        /// <summary>
        /// Prints a translated message to the console.
        /// If no translation is available, it prints the original text.
        /// </summary>
        /// <param name="textKey">The translation key to fetch the translated message.</param>
        public static void PrintTextconsole(string textKey)
        {
            Console.WriteLine(TranslationService.GetInstance().GetText(textKey));
        }

        /// <summary>
        /// Displays a list of job save types and retrieves the user's choice.
        /// Ensures valid input before returning the selected enum value.
        /// </summary>
        /// <returns>The selected <see cref="JobSaveTypeEnum"/> value.</returns>
        public static JobSaveTypeEnum GetInputJobSaveTypeEnum()
        {
            JobSaveTypeEnum[] enumValues = (JobSaveTypeEnum[])Enum.GetValues(typeof(JobSaveTypeEnum));

            // Display available job save types
            for (int i = 0; i < enumValues.Length; i++)
            {
                Console.WriteLine($"{i}. {enumValues[i]}");
            }

            while (true)
            {
                int choice = GetInputInt();
                if (choice >= 0 && choice < enumValues.Length)
                {
                    return enumValues[choice];
                }

                PrintTextconsole("invalidChoice");
            }
        }

        /// <summary>
        /// Reads a valid non-empty string input from the console.
        /// Repeats the prompt if the input is empty or invalid.
        /// </summary>
        /// <returns>A valid non-empty string.</returns>
        public static string GetInputString()
        {
            while (true)
            {
                try
                {
                    string input = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        return input;
                    }
                    PrintTextconsole("messageValidtext");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{TranslationService.GetInstance().GetText("errorReadingJsonFile")} {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Reads a valid integer input from the console.
        /// Ensures that the user enters a numeric value before returning it.
        /// </summary>
        /// <returns>A valid integer input from the user.</returns>
        public static int GetInputInt()
        {
            while (true)
            {
                try
                {
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out int result))
                    {
                        return result;
                    }
                    PrintTextconsole("messageValidInt");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{TranslationService.GetInstance().GetText("errorReadingJsonFile")} {ex.Message}");
                }
            }
        }
    }
}
