using System;
using System.IO;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using System.Text.Json;
using EasySave_Project.Dto;
using EasySave_Project.Util;

namespace EasySave_Project.Service
{
    public class LoadDataService
    {
        private readonly JobManager _jobManager = JobManager.GetInstance();
        
        private readonly ConfigurationService _configurationService = ConfigurationService.GetInstance();

        public LoadDataService()
        {
        }

        /// <summary>
        /// Loads job configurations from a JSON file and initializes job instances.
        /// </summary>
        public void LoadJobs()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _configurationService.GetStringSetting("Path:ProjectFile"),
                _configurationService.GetStringSetting("Path:SettingFolder"),
                _configurationService.GetStringSetting("Path:JobSettingFile"));


            try
            {
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);

                    try
                    {
                        // Deserialize the JSON content into JobSettingsDto
                        JobSettingsDto data = JsonSerializer.Deserialize<JobSettingsDto>(jsonString);

                        if (data != null && data.jobs != null)
                        {
                            // Iterate through the list of jobs in the DTO
                            foreach (var jobData in data.jobs)
                            {
                                // Create a JobModel instance and add it to the _jobManager list
                                JobModel job = new JobModel(jobData.Name, jobData.FileSource, jobData.FileTarget,
                                    jobData.SaveType, jobData.LastSaveDifferentialPath, jobData.LastFullBackupPath)
                                {
                                    Id = jobData.Id,
                                    SaveState = jobData.SaveState,
                                    FileSize = jobData.FileSize,
                                    FileTransferTime = jobData.FileTransferTime,
                                    Time = jobData.Time,
                                    LastFullBackupPath = jobData.LastFullBackupPath,
                                    LastSaveDifferentialPath = jobData.LastSaveDifferentialPath,
                                    FileInPending = jobData.FileInPending
                                };

                                _jobManager.AddJob(job);
                            }

                            ConsoleUtil.PrintTextconsole("Jobs successfully loaded.");
                        }
                        else
                        {
                            ConsoleUtil.PrintTextconsole(
                                "The JSON file does not contain a 'jobs' property or the job list is empty.");
                        }
                    }
                    catch (JsonException ex)
                    {
                        ConsoleUtil.PrintTextconsole($"JSON deserialization error: {ex.Message}");
                    }
                }
                else
                {
                    ConsoleUtil.PrintTextconsole("The JSON file does not exist.");
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"General error while loading jobs from the JSON file: {ex.Message}");
            }
        }
    }
}
