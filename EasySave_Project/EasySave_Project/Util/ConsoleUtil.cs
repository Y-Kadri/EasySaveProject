using EasySave_Project.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Util
{
    public static class ConsoleUtil
    {
        public static void printTextconsole(string text)
        {
            Console.WriteLine(text);
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