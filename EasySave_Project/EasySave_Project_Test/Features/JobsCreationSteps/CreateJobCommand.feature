Feature: Jobs Creation - Create Job Command - Create new backup job

  As a user of the EasySave application,
  I want to create a new backup job
  So that I can save files with the desired configurations

  # Scenario 1: Create a new job with valid inputs
  Scenario: Create a new job with valid name, source, target, and job type
    Given the configuration file "jobsSetting.json" contains:
    """
    {
      "jobs": [],
      "index": 0
    }
    """
    And I am on the job creation screen
    When I enter "Backup Project" as the job name
    And I enter "/tmp" as the source path
    And I enter "/tmp" as the target path
    And I choose "COMPLETE" as the job type
    Then I should see a success message "Job created successfully"
    And the job should be added to the job list

  # Scenario 2: Create a job with an invalid job name
  Scenario: Create a job with an invalid name (empty string)
    Given the configuration file "jobsSetting.json" contains:
    """
    {
      "jobs": [],
      "index": 0
    }
    """
    And I am on the job creation screen
    When I enter "" as the job name
    And I enter "/tmp" as the source path
    And I enter "/tmp" as the target path
    And I choose "COMPLETE" as the job type
    Then the job should not be created

  # Scenario 3: Create a job with an invalid source file path
  Scenario: Create a job with an invalid source file path
    Given the configuration file "jobsSetting.json" contains:
    """
    {
      "jobs": [],
      "index": 0
    }
    """
    And I am on the job creation screen
    When I enter "Backup Project" as the job name
    And I enter "C:/InvalidPath/project" as the source path
    And I enter "/tmp" as the target path
    And I choose "COMPLETE" as the job type
    Then the job should not be created


  # Scenario 4: Create a job with an invalid target file path
  Scenario: Create a job with an invalid target file path
    Given the configuration file "jobsSetting.json" contains:
    """
    {
      "jobs": [],
      "index": 0
    }
    """
    And I am on the job creation screen
    When I enter "Backup Project" as the job name
    And I enter "/tmp" as the source path
    And I enter "D:/InvalidPath/project" as the target path
    And I choose "COMPLETE" as the job type
    Then the job should not be created

  # Scenario 5: Create a job with an invalid job type
  Scenario: Create a job with an invalid job type
    Given the configuration file "jobsSetting.json" contains:
    """
    {
      "jobs": [],
      "index": 0
    }
    """
    And I am on the job creation screen
    When I enter "Backup Project" as the job name
    And I enter "/tmp" as the source path
    And I enter "/tmp" as the target path
    And I choose "INVALID" as the job type
    Then the job should not be created

  # Scenario 6: Create a job and check if it appears in the job list
  Scenario: Verify the job appears in the job list after creation
    Given the configuration file "jobsSetting.json" contains:
    """
    {
      "jobs": [],
      "index": 0
    }
    """
    And I am on the job creation screen
    When I enter "Backup Project" as the job name
    And I enter "/tmp" as the source path
    And I enter "/tmp" as the target path
    And I choose "COMPLETE" as the job type
    Then I should see a success message "Job created successfully"
    And I should see "Backup Project" in the job list
    
  # Scenario 7: Create a job and check if it appears in the jobsSetting.json file and was index was good
  Scenario: Verify the job appears in the config file and the index is correct
    Given the configuration file "jobsSetting.json" contains:
    """
    {
      "jobs": [],
      "index": 0
    }
    """
    And I am on the job creation screen
    When I enter "Backup Project" as the job name
    And I enter "/tmp" as the source path
    And I enter "/tmp" as the target path
    And I choose "COMPLETE" as the job type
    Then I should see a success message "Job created successfully"
    And I should see "Backup Project" in the job list
    And the configuration file "jobsSetting.json" should contain:
    """
    {
      "jobs": [
        {
          "id": 1,
          "SaveState": "INACTIVE",
          "SaveType": "COMPLETE",
          "Name": "Backup Project",
          "FileSource": "/tmp",
          "FileTarget": "/tmp",
          "FileSize": "0 KB",
          "FileTransferTime": "0 sec",
          "Time": "2025-02-03T14:14:12.695169+01:00"
        }
      ],
      "index": 1
    }
    """
  # Scenario 8: Verify the system limits the number of jobs to 5
  Scenario Outline: Create up to 5 jobs successfully and fail on the 6th
    Given the configuration file "jobsSetting.json" contains:
    """
    {
      "jobs": [],
      "index": 0
    }
    """
    And I am on the job creation screen

    When I enter "<JobName>" as the job name
    And I enter "/tmp" as the source path
    And I enter "/tmp" as the target path
    And I choose "COMPLETE" as the job type

    Then <ExpectedResult>

    Examples:
      | JobName          | ExpectedResult                                 |
      | Backup Project 1 | I should see a success message "Job created successfully" |
      | Backup Project 2 | I should see a success message "Job created successfully" |
      | Backup Project 3 | I should see a success message "Job created successfully" |
      | Backup Project 4 | I should see a success message "Job created successfully" |
      | Backup Project 5 | I should see a success message "Job created successfully" |
      | Backup Project 6 | I should see an error message "Cannot create more than 5 jobs" |
