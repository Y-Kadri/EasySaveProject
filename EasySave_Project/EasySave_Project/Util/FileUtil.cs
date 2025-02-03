using System;
using System.IO;
using System.Text.Json;
using EasySave_Project.Dto;
using EasySave_Project.Model;
using EasySave_Project.Service;

namespace EasySave_Project.Util
{
    public class FileUtil
    {

        public static void CreateDirectory(string path)
        {
            var translator = TranslationService.GetInstance();
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"{translator.GetText("directoryCreated")}: {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{translator.GetText("errorCreatingDirectory")}: '{path}' - {ex.Message}");
            }
        }

        /// <summary>
        /// Checks whether a directory exists at the specified path.
        /// </summary>
        /// <param name="path">The path to check for the existence of a directory.</param>
        /// <returns>True if the directory exists; otherwise, false.</returns>
        public static bool ExistsDirectory(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Ensures that the specified directory and file exist. 
        /// Creates them if they do not exist.
        /// </summary>
        /// <param name="fileName">The name of the file to check/create.</param>
        public static void EnsureDirectoryAndFileExist(string fileName)
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting");
            string filePath = Path.Combine(directoryPath, fileName);

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(directoryPath);
                    ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("directoryCreated"));
                }

                // Check if the file exists
                if (!File.Exists(filePath))
                {
                    // Create the file and dispose of the file handle
                    File.Create(filePath).Dispose();
                    ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("fileCreated"));
                    // Create the default JSON file structure
                    CreateDefaultJsonFile(filePath);
                }
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorCreatingDirectoryOrFile") + ex.Message);
            }
        }


        /// <summary>
        /// Copies a file from the source path to the destination path.
        /// </summary>
        /// <param name="sourceFile">The path of the file to copy.</param>
        /// <param name="destinationFile">The path where the file will be copied.</param>
        /// <param name="overwrite">Indicates whether to overwrite the destination file if it exists.</param>
        public static void CopyFile(string sourceFile, string destinationFile, bool overwrite)
        {
            var translator = TranslationService.GetInstance();
            try
            {
                File.Copy(sourceFile, destinationFile, overwrite);
                Console.WriteLine($"{translator.GetText("fileCopied")}: {sourceFile} -> {destinationFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{translator.GetText("errorCopyingFile")}: '{sourceFile}' - {ex.Message}");
            }
        }

        /// <summary>
        /// Checks whether a file exists at the specified path.
        /// </summary>
        /// <param name="path">The path to check for the existence of a file.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        public static bool ExistsFile(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Retrieves the file name from the specified path.
        /// </summary>
        /// <param name="path">The path from which to retrieve the file name.</param>
        /// <returns>The file name, or null if the path is invalid.</returns>
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Creates a default JSON file with an initial structure.
        /// </summary>
        /// <param name="filePath">The path of the file to create.</param>
        private static void CreateDefaultJsonFile(string filePath)
        {
            // Initial data to be written in the JSON file
            var data = new
            {
                jobs = new string[] { },
                index = 0
            };

            // Serialize the data to JSON format
            string jsonString = JsonSerializer.Serialize(data);

            try
            {
                // Write the JSON string to the file
                File.WriteAllText(filePath, jsonString);
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("jsonFileCreated"));
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs during file writing
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorWritingJsonFile") + ex.Message);
            }
        }

        /// <summary>
        /// Combines two paths into a single path.
        /// </summary>
        /// <param name="path1">The first path.</param>
        /// <param name="path2">The second path.</param>
        /// <returns>The combined path.</returns>
        public static string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        /// <summary>
        /// Gets the current job index from the JSON file and increments it for a new job.
        /// </summary>
        /// <returns>The incremented job index.</returns>
        public static int GetJobIndex()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", "jobsSetting.json");

            try
            {
                // Check if the JSON file exists
                if (File.Exists(filePath))
                {
                    // Read the content of the JSON file
                    string jsonString = File.ReadAllText(filePath);
                    JsonDocument doc = JsonDocument.Parse(jsonString);
                    JsonElement root = doc.RootElement;

                    // Try to get the job index from the JSON
                    if (root.TryGetProperty("index", out JsonElement indexElement))
                    {
                        int currentIndex = indexElement.GetInt32();
                        return currentIndex + 1; // Return the incremented index
                    }
                    else
                    {
                        ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("indexNotFoundInJson"));
                        return 1; // Default to 1 if index is not found
                    }
                }
                else
                {
                    ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("jsonFileNotExist"));
                    return 1; // Default to 1 if file does not exist
                }
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs during reading
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorReadingJsonFile") + ex.Message);
                return -1; // Return -1 to indicate an error
            }
        }

        /// <summary>
        /// Retrieves all files in the specified directory.
        /// </summary>
        /// <param name="path">The path of the directory to retrieve files from.</param>
        /// <returns>An enumerable collection of file paths.</returns>
        public static IEnumerable<string> GetFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{TranslationService.GetInstance().GetText("errorCopyingDirectory")}: '{path}' - {ex.Message}");
                return new string[0]; // Returns an empty array in case of error
            }
        }

        /// <summary>
        /// Adds a new job to the JSON file.
        /// </summary>
        /// <param name="name">The name of the job.</param>
        /// <param name="fileSource">The source file path.</param>
        /// <param name="fileTarget">The target file path.</param>
        /// <param name="jobSaveTypeEnum">The type of the save job.</param>
        public static void AddJobInFile(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", "jobsSetting.json");

            try
            {
                // Check if the JSON file exists
                if (File.Exists(filePath))
                {
                    // Read the content of the JSON file
                    string jsonString = File.ReadAllText(filePath);
                    JobSettingsDto data = JsonSerializer.Deserialize<JobSettingsDto>(jsonString);

                    int newJobId = GetJobIndex(); // Get the new job index
                    IncrementJobIndex(); // Increment the job index

                    // Set the initial state for the new job
                    JobSaveStateEnum saveState;
                    if (!Enum.TryParse<JobSaveStateEnum>("INACTIVE", out saveState))
                    {
                        ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorConvertingSaveState"));
                        return;
                    }

                    // Create a new job model
                    var newJob = new JobModel(name, fileSource, fileTarget, jobSaveTypeEnum)
                    {
                        id = newJobId,
                        SaveState = saveState,
                        FileSize = "0 KB",
                        FileTransferTime = "0 sec",
                        Time = DateTime.Now
                    };

                    data.jobs.Add(newJob); // Add the new job to the list

                    // Serialize the updated data back to JSON format
                    string updatedJsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updatedJsonString); // Write the updated JSON to the file

                    ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("jobCree"));
                }
                else
                {
                    ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("jsonFileNotExist"));
                }
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs during adding the job
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorAddingJobToJson") + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all subdirectories in the specified directory.
        /// </summary>
        /// <param name="path">The path of the directory to retrieve subdirectories from.</param>
        /// <returns>An enumerable collection of directory paths.</returns>
        public static IEnumerable<string> GetDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{TranslationService.GetInstance().GetText("errorGettingDirectories")}: '{path}' - {ex.Message}");
                return new string[0]; // Returns an empty array in case of error
            }
        }

        /// <summary>
        /// Increments the job index in the JSON file.
        /// </summary>
        public static void IncrementJobIndex()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", "jobsSetting.json");

            try
            {
                // Check if the JSON file exists
                if (File.Exists(filePath))
                {
                    // Read the content of the JSON file
                    string jsonString = File.ReadAllText(filePath);
                    JobSettingsDto data = JsonSerializer.Deserialize<JobSettingsDto>(jsonString);

                    data.index++; // Increment the job index

                    // Serialize the updated data back to JSON format
                    string updatedJsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updatedJsonString); // Write the updated JSON to the file

                    ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("indexIncremented") + data.index);
                }
                else
                {
                    ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("jsonFileNotExist"));
                }
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs during index incrementing
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorIncrementingIndex") + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the last write time of the specified file.
        /// </summary>
        /// <param name="path">The path of the file to retrieve the last write time from.</param>
        /// <returns>The DateTime of the last write time.</returns>
        public static DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        /// <summary>
        /// Retrieves the directory name from the specified path.
        /// </summary>
        /// <param name="path">The path from which to retrieve the directory name.</param>
        /// <returns>The directory name, or null if the path is invalid.</returns>
        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }
    }
}
