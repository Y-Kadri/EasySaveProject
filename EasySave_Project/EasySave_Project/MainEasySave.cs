using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave_Project.Controller;
using EasySave_Project.Util;
using EasySave_Project.View;

namespace EasySave_Project
{
    class MainEasySave
    {

        static void Main(string[] args)
        {
            ConsoleView consoleView = new();
            CommandController commandController = new();
            ConsoleUtil consoleUtil = new();

            consoleView.ChooseLanguage(); //TODO faire la mathode

            while (true)
            {



               commandController.ChoiceCommande();


            }
        }
    }
}