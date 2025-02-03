using EasySave_Project.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave_Project.Model;

namespace EasySave_Project.Util
{
    public static class ConsoleUtil
    {
        public static void PrintTextconsole(string text)
        {
            string translatedText = TranslationService.GetInstance().GetText(text);
            Console.WriteLine(!string.IsNullOrWhiteSpace(translatedText) ? translatedText : text);
        }
        
        public static JobSaveTypeEnum GetInputJobSaveTypeEnum()
        {
            JobSaveTypeEnum[] enumValues = (JobSaveTypeEnum[])Enum.GetValues(typeof(JobSaveTypeEnum));
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

                Console.WriteLine(TranslationService.GetInstance().GetText("invalidChoice"));
            }
        }

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
                    Console.WriteLine(TranslationService.GetInstance().GetText("messageValidtext"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la lecture de la console: {ex.Message}"); //TODO prendre le translate
                }
            }
        }

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
                    Console.WriteLine(TranslationService.GetInstance().GetText("messageValidInt"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la lecture de la console: {ex.Message}");  //TODO prendre le translate
                }
            }
        }
    }
}