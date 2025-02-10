﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave_Library_Log;
using EasySave_Library_Log.manager;
using EasySave_Project.Controller;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Util;
using EasySave_Project.View;

namespace EasySave_Project
{
    class MainEasySave
    {

        static void Main(string[] args)
        {
            FileUtil.EnsureDirectoryAndFileExist("jobsSetting.json");
            ConsoleView consoleView = new();
            CommandController commandController = new();
            LoadDataService loadDataService = new();
            
            loadDataService.LoadJobs();


            
            int choiceLanguage = consoleView.ChooseLanguage();

            switch (choiceLanguage)
            {
                case 1:
                    TranslationService.SetLanguage(LanguageEnum.EN);
                    break;
                case 2:
                    TranslationService.SetLanguage(LanguageEnum.FR);
                    break;
                default:
                    TranslationService.SetLanguage(LanguageEnum.EN);
                    break;
            }
            
            
            
            int choiceLogFormat = consoleView.ChooseLogFormat();

            switch (choiceLogFormat)
            {
                case 1:
                    LogFormatManager.Instance.SetLogFormat(LogFormatManager.LogFormat.JSON);
                    break;
                case 2:
                    LogFormatManager.Instance.SetLogFormat(LogFormatManager.LogFormat.XML);
                    break;
            }

            while (true)
            {
               

                int choice = consoleView.StartJobCommand();

                commandController.LaunchCommand(choice);
            }
        }
    }
}