using EasySave_Project.Command;
using EasySave_Project.Service;
using EasySave_Project.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.View
{
    public class ConsoleView
    {

        private readonly TranslationService TranslationService;
        private readonly ConsoleUtil ConsoleUtil;

        public ConsoleView()
        {
            ConsoleUtil = new ConsoleUtil();
        }

        public void LaunchApp()
        {
            Console.WriteLine(TranslationService.GetText("welcome"));
        }

        public int ChooseLanguage()
        {
            Console.WriteLine("Choose your language / Choisissez votre langue: \n1. English\n2. Français\nEnter your choice:");
            return ConsoleUtil.GetInputInt();
        }

        public int StartJobCommand()
        {
            ConsoleUtil.printTextconsole(TranslationService.GetInstance().GetText("option1")
                + "\n" + TranslationService.GetInstance().GetText("option2")
                + "\n" + TranslationService.GetInstance().GetText("option3")
                + "\n" + TranslationService.GetInstance().GetText("option4"));
            return ConsoleUtil.GetInputInt();
        }
    }
}
