﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using EasySave_Project.Service;
using EasySave_Project.Util;
using EasySave_Project.View;

namespace EasySave_Project.Command
{
    /// <summary>
    /// Command class responsible for creating a new backup job.
    /// Implements the ICommand interface.
    /// </summary>
    public class CreateJobCommand : ICommand
    {
        /// <summary>
        /// Executes the command to create a new job.
        /// It prompts the user for job details and adds the job to JobManager.
        /// </summary>
        public void Execute()
        {
            while (true) // Infinite loop to ensure correct input
            {
                // Prompt user for the job name
                ConsoleUtil.PrintTextconsole("entrerNom");
                string name = ConsoleUtil.GetInputString();

                // Prompt user for the source file path
                ConsoleUtil.PrintTextconsole("entrerFileSource");
                string fileSource = ConsoleUtil.GetInputString();

                // Prompt user for the target file path
                ConsoleUtil.PrintTextconsole("entrerFileTarget");
                string fileTarget = ConsoleUtil.GetInputString();

                // Prompt user for the job type (Complete or Differential)
                ConsoleUtil.PrintTextconsole("entrerJobType");
                JobSaveTypeEnum jobSaveTypeEnum = ConsoleUtil.GetInputJobSaveTypeEnum();

                // Get the singleton instance of JobManager
                JobManager jobMana = JobManager.GetInstance();

                // Create and add the job using the provided user input
                jobMana.CreateAndAddJob(name, fileSource, fileTarget, jobSaveTypeEnum);

                // Notify user that the job was successfully created
                ConsoleUtil.PrintTextconsole("jobCree");

                break; // Exit the loop after successfully creating the job
            }
        }

        /// <summary>
        /// Displays the command instruction.
        /// </summary>
        public void GetInstruction()
        {
            ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("createJobCommand"));
        }
    }
}
