using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Util
{
    public class ConsoleUtil
    {
        public void printTextconsole(string text)
        {
            Console.WriteLine(text);
        }

        public string GetInputString()
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
                    Console.WriteLine("Veuillez entrer un texte valide."); //TODO prendre le translate
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la lecture de la console: {ex.Message}"); //TODO prendre le translate
                }
            }
        }

        public int GetInputInt()
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
                    Console.WriteLine("Veuillez entrer un nombre entier valide.");  //TODO prendre le translate
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la lecture de la console: {ex.Message}");  //TODO prendre le translate
                }
            }
        }
    }
}