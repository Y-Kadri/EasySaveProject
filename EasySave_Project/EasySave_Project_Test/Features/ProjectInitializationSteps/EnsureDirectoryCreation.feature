Feature: Project Initialization - Ensure Directory Creation - Verify Configuration Directory and Config File Creation

  As a user of the EasySave application,
  I want to ensure that the "easySaveSetting" configuration directory and "jobsSetting.json" file are created automatically,
  So that the application settings are properly saved.

  Scenario: Verify the creation of the configuration directory if it does not exist
    Given the configuration directory "easySaveSetting" does not exist
    When I initialize the project for the first time
    Then the configuration directory "easySaveSetting" should be created in the "Documents" folder

  Scenario: Verify the creation of the jobsSetting.json file with settings
    Given the configuration file "jobsSetting.json" does not exist
    When I initialize the project
    Then the "jobsSetting.json" file should be created with settings:
      """
      {
        "jobs": [],
        "index": 0
      }
      """
