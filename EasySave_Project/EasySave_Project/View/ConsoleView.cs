using EasySave_Project.Command;
using EasySave_Project.Model;
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

        public void ShowJobList(List<JobModel> jobList)
        {
            Console.WriteLine(TranslationService.GetInstance().GetText("CurrentBackupTasks"));
            for (int i = 0; i < jobList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobList[i].Name} (Source: {jobList[i].FileSource}, Target: {jobList[i].FileTarget})");
            }
        }

        public string ChoiceJob()
        {
            Console.Write(TranslationService.GetInstance().GetText("enterTaskNumbers"));
            return Console.ReadLine();

        }
    }
}
