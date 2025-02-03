Feature: Project Initialization - Ensure Jobs Creation - Load Jobs from jobsSetting.json

  As a user of the EasySave application,
  I want to ensure that the jobs are correctly loaded from the "jobsSetting.json" file,
  So that the application can properly manage the job data.

  Scenario: Verify that jobs are loaded from the jobsSetting.json file if it exists
    Given the "jobsSetting.json" file exists in the "easySaveSetting" directory with the following content:
      """
      {
        "jobs": [
          {
            "id": 1,
            "SaveState": "INACTIVE",
            "SaveType": "COMPLETE",
            "Name": "Backup Documents",
            "FileSource": "C:\\Documents\\myfile.txt",
            "FileTarget": "D:\\Backup\\myfile.txt",
            "FileSize": "1024",
            "FileTransferTime": "30",
            "Time": "2025-02-03T12:00:00"
          }
        ],
        "index": 1
      }
      """
    When I load the jobs from the "jobsSetting.json" file
    Then the job with id 1 should be loaded with the following details:
      | Property          | Value                      |
      | id               | 1                           |
      | SaveState        | INACTIVE                    |
      | SaveType         | COMPLETE                    |
      | Name             | Backup Documents            |
      | FileSource       | C:\Documents\myfile.txt     |
      | FileTarget       | D:\Backup\myfile.txt        |
      | FileSize         | 1024                        |
      | FileTransferTime | 30                          |
      | Time             | 2025-02-03T12:00:00         |
