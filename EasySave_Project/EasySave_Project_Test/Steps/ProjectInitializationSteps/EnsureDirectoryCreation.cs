using NUnit.Framework;
using System;
using System.IO;
using System.Text.Json;
using EasySave_Project.Util;
using EasySave_Project.Dto;
using TechTalk.SpecFlow;

namespace EasySave_Project_Test.Steps.ProjectInitializationSteps
{
    [Binding]
    public class EnsureDirectoryCreation
    {
        private string directoryPath;
        private string fileName = "jobsSetting.json";
        private string filePath;

        [Given(@"the configuration directory ""(.*)"" does not exist")]
        public void GivenTheConfigurationDirectoryDoesNotExist(string directoryName)
        {
            directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), directoryName);

            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true); // Deletes the directory if it exists (for a clean test environment)
            }

            Assert.IsFalse(Directory.Exists(directoryPath),
                $"The directory '{directoryPath}' should not exist before the test.");
        }

        [Given(@"the configuration file ""(.*)"" does not exist")]
        public void GivenTheConfigurationFileDoesNotExist(string fileName)
        {
            directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting");
            filePath = Path.Combine(directoryPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath); // Delete the file if it exists (clean state)
            }

            Assert.IsFalse(File.Exists(filePath),
                $"The file '{filePath}' should not exist before the test.");
        }

        [When(@"I initialize the project for the first time")]
        [When(@"I initialize the project")]
        public void WhenIInitializeTheProjectForTheFirstTime()
        {
            // Call the method responsible for directory and file creation
            EasySave_Project.Util.FileUtil.EnsureDirectoryAndFileExist(fileName);
        }

        [Then(@"the configuration directory ""(.*)"" should be created in the ""(.*)"" folder")]
        public void ThenTheConfigurationDirectoryShouldBeCreatedInTheFolder(string directoryName, string parentFolder)
        {
            string expectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), directoryName);

            Assert.IsTrue(Directory.Exists(expectedPath), $"The directory '{expectedPath}' was not created as expected.");
        }

        [Then(@"the ""(.*)"" file should be created with default settings:")]
        public void ThenTheFileShouldBeCreatedWithDefaultSettings(string fileName, string expectedJson)
        {
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", fileName);

            Assert.IsTrue(File.Exists(filePath), $"The file '{filePath}' was not created as expected.");

            string actualJson = File.ReadAllText(filePath);

            // Deserialize both actual and expected JSON for object comparison
            var actualObject = JsonSerializer.Deserialize<JobSettingsDto>(actualJson);
            var expectedObject = JsonSerializer.Deserialize<JobSettingsDto>(expectedJson);

            // Compare the deserialized objects
            Assert.AreEqual(expectedObject.index, actualObject.index, "The 'index' value does not match.");
            Assert.AreEqual(expectedObject.jobs?.Count, actualObject.jobs?.Count, "The 'jobs' list does not match.");
        }
    }
}