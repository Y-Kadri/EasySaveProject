using EasySave_Project.Dto;
using EasySave_Project.Manager;
using EasySave_Project.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;

namespace EasySave_Project_Test.Steps.JobsCreationSteps;

[Binding]
public class CreateJobCommand
{
    private string _jobName;
    private string _fileSource;
    private string _fileTarget;
    private JobSaveTypeEnum _jobSaveTypeEnum;
    private JobManager _jobManager;
    private string _successMessage;
    private string _errorMessage;
    private bool _isJobCreated;


    [BeforeScenario]
    public void ClearJobsBeforeScenario()
    {
        _jobManager = JobManager.GetInstance();

        // Clear the jobs list before most scenarios
        if (ScenarioContext.Current.ScenarioInfo.Title != "Create up to 5 jobs successfully and fail on the 6th")
        {
            _jobManager.Jobs.Clear(); // Clear the job list for most scenarios
        }
    }

    // Step definition: Given I am on the job creation screen
    [Given(@"I am on the job creation screen")]
    public void GivenIAmOnTheJobCreationScreen()
    {
        // Logic to simulate being on the job creation screen.
        // Could be initializing necessary objects if needed
        _jobManager = JobManager.GetInstance();
    }

    // Step definition: When I enter "(.*)" as the job name
    [When(@"I enter ""(.*)"" as the job name")]
    public void WhenIEnterAsTheJobName(string jobName)
    {
        _jobName = jobName;
    }

    // Step definition: And I enter "(.*)" as the source path
    [When(@"I enter ""(.*)"" as the source path")]
    public void WhenIEnterAsTheSourcePath(string fileSource)
    {
        _fileSource = fileSource;
    }

    // Step definition: And I enter "(.*)" as the target path
    [When(@"I enter ""(.*)"" as the target path")]
    public void WhenIEnterAsTheTargetPath(string fileTarget)
    {
        _fileTarget = fileTarget;
    }

    // Step definition: And I choose "(.*)" as the job type
    [When(@"I choose ""(.*)"" as the job type")]
    public void WhenIChooseAsTheJobType(string jobType)
    {
        if (Enum.TryParse<JobSaveTypeEnum>(jobType, out JobSaveTypeEnum parsedJobType))
        {
            _jobSaveTypeEnum = parsedJobType;
        }
    }
    
    // Step definition: Then I should see a success message "(.*)"
    [Then(@"I should see a success message ""(.*)""")]
    public void ThenIShouldSeeASuccessMessage(string expectedMessage)
    {
        // Simulate job creation process
        var createdJob = _jobManager.CreateAndAddJob(_jobName, _fileSource, _fileTarget, _jobSaveTypeEnum);
        _isJobCreated = createdJob != null;

        // Logic to verify the success message
        if (_isJobCreated)
        {
            _successMessage = "Job created successfully";
        }

        // Assert the message matches the expected one
        if (_isJobCreated)
        {
            if (_successMessage == expectedMessage)
            {
                Console.WriteLine("Success: " + expectedMessage);
            }
            else
            {
                throw new Exception("Expected message does not match actual: " + _successMessage);
            }
        }
        else
        {
            throw new Exception("Job creation failed.");
        }
    }

    // Step definition: And the job should be added to the job list
    [Then(@"the job should be added to the job list")]
    public void ThenTheJobShouldBeAddedToTheJobList()
    {
        if (!_isJobCreated)
        {
            throw new Exception("Job creation failed, the job was not added to the job list.");
        }

        // Verify the job has been added to the JobManager
        var jobList = _jobManager.Jobs;
        bool jobExists = jobList.Exists(job => job.Name == _jobName);

        if (jobExists)
        {
            Console.WriteLine("The job was successfully added to the job list.");
        }
        else
        {
            throw new Exception("Job was not found in the job list.");
        }
    }

    [Then(@"the job should not be created")]
    public void ThenTheJobShouldNotBeCreated()
    {
        // Vérifie si le job a été créé
        if (_isJobCreated)
        {
            throw new Exception("Job creation failed, the job should not have been created.");
        }

        // Vérifie si le job n'a pas été ajouté à la liste
        var jobList = _jobManager.Jobs;
        bool jobExists = jobList.Exists(job => job.Name == _jobName);

        if (jobExists)
        {
            throw new Exception("The job was unexpectedly found in the job list.");
        }
        else
        {
            Console.WriteLine("The job was not created as expected.");
        }
    }

    [Then(@"I should see ""(.*)"" in the job list")]
    public void ThenIShouldSeeInTheJobList(string name)
    {
        var jobList = _jobManager.Jobs;
        bool jobExists = jobList.Exists(job => job.Name == name);
        if (!jobExists)
        {
            throw new Exception("The job was not found in the job list.");
        }
        else
        {
            Console.WriteLine("The job was in the job list.");
        }
    }
    
    [Then(@"The job was in the config file")]
    public void ThenTheJobWasInTheConfigFile()
    {
        string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting/jobsSetting.json");
        
        if (!File.Exists(configFilePath))
        {
            throw new Exception("The configuration file does not exist.");
        }

        try
        {
            // Lire le contenu du fichier de configuration
            string jsonContent = File.ReadAllText(configFilePath);
        
            // Désérialiser le contenu JSON en liste de jobs
            var jobsFromConfig = JsonConvert.DeserializeObject<JobSettingsDto>(
                jsonContent, 
                new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore }
            );

            // Vérifier si le job existe dans la liste
            bool jobExistsInConfig = jobsFromConfig.jobs != null && jobsFromConfig.jobs.Exists(job => job.Name == _jobName);

            if (jobExistsInConfig)
            {
                Console.WriteLine("The job was found in the configuration file.");
            }
            else
            {
                throw new Exception($"The job '{_jobName}' was not found in the configuration file.");
            }
        }
        catch (JsonException ex)
        {
            throw new Exception("Error while reading the configuration file: " + ex.Message);
        }
    }
    
    [Then(@"the configuration file \""(.*)\"" should contain:")]
    public void ThenTheConfigurationFileShouldContain(string configFileName, string expectedJson)
    {
        string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting/" + configFileName);

        if (!File.Exists(configFilePath))
        {
            throw new Exception("The configuration file does not exist.");
        }

        try
        {
            // Lire le contenu du fichier de configuration
            string jsonContent = File.ReadAllText(configFilePath);

            // Désérialiser le contenu JSON
            JobSettingsDto actualConfig = JsonConvert.DeserializeObject<JobSettingsDto>(jsonContent);
            JobSettingsDto expectedConfig = JsonConvert.DeserializeObject<JobSettingsDto>(expectedJson);

            if (actualConfig.index != expectedConfig.index)
            {
                throw new Exception("Index are different");
            }

            if (actualConfig.jobs.Count != expectedConfig.jobs.Count)
            {
                throw new Exception("Jobs count are different");
            }
            
            // Comparer les propriétés statiques
            for (int i = 0; i < actualConfig.jobs.Count; i++)
            {
                JobModel actualJobModel = actualConfig.jobs[i];
                JobModel expectedJobModel = expectedConfig.jobs[i];
                
                if (actualJobModel.Id != expectedJobModel.Id ||
                    actualJobModel.SaveState != expectedJobModel.SaveState ||
                    actualJobModel.SaveType != expectedJobModel.SaveType ||
                    actualJobModel.Name != expectedJobModel.Name ||
                    actualJobModel.FileSource != expectedJobModel.FileSource ||
                    actualJobModel.FileTarget != expectedJobModel.FileTarget ||
                    actualJobModel.FileSize != expectedJobModel.FileSize ||
                    actualJobModel.FileTransferTime != expectedJobModel.FileTransferTime)
                {
                    throw new Exception($"Mismatch found at job index {i}. " +
                                        $"Expected: {expectedJobModel}, " +
                                        $"Actual: {actualJobModel}");
                }
            }

            Console.WriteLine("The configuration file matches the expected content.");
        }
        catch (JsonException ex)
        {
            throw new Exception("Error while reading the configuration file: " + ex.Message);
        }
    }
    
    [Then(@"the \""(.*)\"" file should be created with settings:")]
    public void ThenTheFileShouldBeCreatedWithSettings(string fileName, string expectedJson)
    {
        // Construction du chemin vers le fichier
        string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", fileName);

        // Vérifier si le fichier existe
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException($"Le fichier {fileName} n'existe pas à l'emplacement attendu : {configFilePath}");
        }

        // Lire le contenu du fichier
        string actualJson = File.ReadAllText(configFilePath).Trim();
        expectedJson = expectedJson.Trim();

        // Comparer directement les chaînes de caractères
        if (actualJson != expectedJson)
        {
            throw new Exception($"Le contenu du fichier ne correspond pas au contenu attendu.\nAttendu : {expectedJson}\nObtenu : {actualJson}");
        }

        Console.WriteLine($"Le fichier {fileName} correspond au contenu attendu.");
    }
    
    [Then(@"I should see an error message ""(.*)""")]
    public void ThenIShouldSeeAnErrorMessage(string expectedErrorMessage)
    {
        // Simulate job creation process that leads to an error
        var createdJob = _jobManager.CreateAndAddJob(_jobName, _fileSource, _fileTarget, _jobSaveTypeEnum);
        _isJobCreated = createdJob != null;

        // Logic to verify the error message
        if (!_isJobCreated)
        {
            _errorMessage = "Cannot create more than 5 jobs"; // Customize the error message as needed
        }

        // Assert the error message matches the expected one
        if (!_isJobCreated)
        {
            if (_errorMessage == expectedErrorMessage)
            {
                Console.WriteLine("Error: " + expectedErrorMessage);
            }
            else
            {
                throw new Exception("Expected error message does not match actual: " + _errorMessage);
            }
        }
        else
        {
            throw new Exception("Job creation succeeded when failure was expected.");
        }
    }


}