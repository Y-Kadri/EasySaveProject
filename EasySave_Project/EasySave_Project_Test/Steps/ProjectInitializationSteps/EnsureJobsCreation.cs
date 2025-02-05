using EasySave_Project.Model;
using EasySave_Project.Service;
using TechTalk.SpecFlow;

namespace EasySave_Project_Test.Steps.ProjectInitializationSteps;

[Binding]
public class EnsureJobsCreation
{
    
    private string directoryPath;
    private string fileName = "jobsSetting.json";
    private string filePath;
    private LoadDataService loadDataService;
    
    public EnsureJobsCreation()
    {
        loadDataService = new LoadDataService();
    }
    
    [Given(@"the ""(.*)"" file exists in the ""(.*)"" directory with the following content:")]
    public void GivenTheFileExistsInTheDirectoryWithTheFollowingContent(string fileName, string directoryName, string jsonContent)
    {
        directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), directoryName);
        filePath = Path.Combine(directoryPath, fileName);

        // Ensure the directory exists
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Write the content to the file
        File.WriteAllText(filePath, jsonContent);

        Assert.IsTrue(File.Exists(filePath), $"The file '{filePath}' should exist after writing.");
    }

    [When(@"I load the jobs from the ""(.*)"" file")]
    public void WhenILoadTheJobsFromTheFile(string fileName)
    {
        // Load jobs using the service method
        loadDataService.LoadJobs();
    }

    [Then(@"the job with id (.*) should be loaded with the following details:")]
    public void ThenTheJobWithIdShouldBeLoadedWithTheFollowingDetails(int jobId, Table table)
    {
        // Assuming the job manager holds the loaded jobs in some accessible collection
        var jobManager = EasySave_Project.Manager.JobManager.GetInstance();
        var job = jobManager.GetJobById(jobId);

        Assert.IsNotNull(job, $"Job with ID {jobId} should be found.");

        // Compare each property in the table
        foreach (var row in table.Rows)
        {
            string property = row["Property"];
            string expectedValue = row["Value"];

            switch (property)
            {
                case "Id":
                    Assert.AreEqual(int.Parse(expectedValue), job.Id, $"The 'id' value does not match.");
                    break;
                case "SaveState":
                    Assert.AreEqual(Enum.Parse<JobSaveStateEnum>(expectedValue, true), job.SaveState, $"The 'SaveState' value does not match.");
                    break;
                case "SaveType":
                    Assert.AreEqual(Enum.Parse<JobSaveTypeEnum>(expectedValue, true), job.SaveType, $"The 'SaveType' value does not match.");
                    break;
                case "Name":
                    Assert.AreEqual(expectedValue, job.Name, $"The 'Name' value does not match.");
                    break;
                case "FileSource":
                    Assert.AreEqual(expectedValue, job.FileSource, $"The 'FileSource' value does not match.");
                    break;
                case "FileTarget":
                    Assert.AreEqual(expectedValue, job.FileTarget, $"The 'FileTarget' value does not match.");
                    break;
                case "FileSize":
                    Assert.AreEqual(expectedValue, job.FileSize, $"The 'FileSize' value does not match.");
                    break;
                case "FileTransferTime":
                    Assert.AreEqual(expectedValue, job.FileTransferTime, $"The 'FileTransferTime' value does not match.");
                    break;
                case "Time":
                    Assert.AreEqual(DateTime.Parse(expectedValue), job.Time, $"The 'Time' value does not match.");
                    break;
                default:
                    Assert.Fail($"Unknown property '{property}'.");
                    break;
            }
        }
    }
}