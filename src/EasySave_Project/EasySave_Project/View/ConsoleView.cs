﻿using EasySave_Project.Command;
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
    /// <summary>
    /// The ConsoleView class handles user interactions in the console for the EasySave application.
    /// It provides methods to display messages, get user input, and present job lists.
    /// </summary>
    public class ConsoleView
    {

        /// <summary>
        /// Launches the application by displaying a welcome message.
        /// </summary>
        public void LaunchApp()
        {
            ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("welcome"));
        }

        /// <summary>
        /// Prompts the user to choose a language.
        /// </summary>
        /// <returns>An integer representing the user's language choice.</returns>
        public int ChooseLanguage()
        {
            ConsoleUtil.PrintTextconsole("Choose your language / Choisissez votre langue: \n1. English\n2. Français\nEnter your choice:");
            return ConsoleUtil.GetInputInt();
        }

        /// <summary>
        /// Displays available job commands to the user and returns the user's choice.
        /// </summary>
        /// <returns>An integer representing the user's command choice.</returns>
        public int StartJobCommand()
        {
            ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("option1")
                + "\n" + TranslationService.GetInstance().GetText("option2")
                + "\n" + TranslationService.GetInstance().GetText("option3")
                + "\n" + TranslationService.GetInstance().GetText("option4")
                + "\n" + TranslationService.GetInstance().GetText("option5")
                + "\n" + TranslationService.GetInstance().GetText("option6"));
            return ConsoleUtil.GetInputInt();
        }

        /// <summary>
        /// Displays the list of current backup jobs to the user.
        /// </summary>
        /// <param name="jobList">A list of JobModel objects representing backup jobs.</param>
        public void ShowJobList(List<JobModel> jobList)
        {
            ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("CurrentBackupTasks"));
            for (int i = 0; i < jobList.Count; i++)
            {
                ConsoleUtil.PrintTextconsole($"{i + 1}. {jobList[i].Name} (Source: {jobList[i].FileSource}, Target: {jobList[i].FileTarget})");
            }
        }

        /// <summary>
        /// Prompts the user to enter the task numbers for the jobs they wish to execute.
        /// </summary>
        /// <returns>A string representing the user's input.</returns>
        public string ChoiceJob()
        {
            Console.Write(TranslationService.GetInstance().GetText("enterTaskNumbers"));
            return Console.ReadLine();
        }

        public int ChooseLogFormat()
        {
            ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("enterLogFormatChoose") + " (1 : JSON, 2 : XML)");
            return ConsoleUtil.GetInputInt();
        }
    }
}
