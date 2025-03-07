﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CryptoSoft;
using DynamicData;
using EasySave_Library_Log.manager;
using EasySave_Project.Dto;
using EasySave_Project.Model;
using EasySave_Project.Service;
using static System.Collections.Specialized.BitVector32;

namespace EasySave_Project.Util
{
    public class FileUtil
    {
        public static void CreateDirectory(string path)
        {
            var translator = TranslationService.GetInstance();
            string message;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    message = $"{translator.GetText("directoryCreated")}: {path}";
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                }
            }
            catch (Exception ex)
            {
                message = $"{translator.GetText("errorCreatingDirectory")}: '{path}' - {ex.Message}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
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
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "easySaveSetting");
            string filePath = Path.Combine(directoryPath, fileName);
            string message;

            try
            {
                // Check if the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(directoryPath);
                    message = TranslationService.GetInstance().GetText("directoryCreated");
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                }

                // Check if the file exists
                if (!File.Exists(filePath))
                {
                    // Create the file and dispose of the file handle
                    File.Create(filePath).Dispose();
                    message = TranslationService.GetInstance().GetText("fileCreated");
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    // Create the default JSON file structure
                    CreateDefaultJsonFile(filePath);
                }
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs
                message = TranslationService.GetInstance().GetText("errorCreatingDirectoryOrFile") + ex.Message;
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
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
            string message;

            try
            {
                // Ensure the destination directory exists
                string destinationDirectory = Path.GetDirectoryName(destinationFile);
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // Copy the file to the destination, depending on the 'overwrite' parameter
                File.Copy(sourceFile, destinationFile, overwrite);
            }
            catch (Exception ex)
            {
                // Log and display any error that occurs during the copy process
                message = $"{translator.GetText("errorCopyingFile")}: '{sourceFile}' - {ex.Message}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
            }
        }


        public static void EncryptFile(string filePath, string key)
        {
            try
            {
                var fileManager = new FileManager(filePath, key);
                fileManager.TransformFile();
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"Erreur lors du cryptage du fichier: {ex.Message}");
                LogManager.Instance.AddMessage($"Erreur lors du cryptage du fichier: {ex.Message}");
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
            string message;

            try
            {
                // Write the JSON string to the file
                File.WriteAllText(filePath, jsonString);
                message = TranslationService.GetInstance().GetText("jsonFileCreated");
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs during file writing
                message = TranslationService.GetInstance().GetText("errorWritingJsonFile") + ex.Message;
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
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
            ConfigurationService _configurationService = ConfigurationService.GetInstance();
            
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _configurationService.GetStringSetting("Path:ProjectFile"),
                _configurationService.GetStringSetting("Path:SettingFolder"),
                _configurationService.GetStringSetting("Path:JobSettingFile"));
            string message;

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
                        message = TranslationService.GetInstance().GetText("indexNotFoundInJson");
                        ConsoleUtil.PrintTextconsole(message);
                        LogManager.Instance.AddMessage(message);
                        return 1; // Default to 1 if index is not found
                    }
                }
                else
                {
                    message = TranslationService.GetInstance().GetText("jsonFileNotExist");
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    return 1; // Default to 1 if file does not exist
                }
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs during reading
                message = TranslationService.GetInstance().GetText("errorReadingJsonFile") + ex.Message;
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
                return -1; // Return -1 to indicate an error
            }
        }

        /// <summary>
        /// Gets the current job index from the JSON file.
        /// </summary>
        /// <returns>The current job index, or 0 if not found.</returns>
        public static int GetCurrentJobIndex()
        { 
            ConfigurationService _configurationService = ConfigurationService.GetInstance();
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _configurationService.GetStringSetting("Path:ProjectFile"),
                _configurationService.GetStringSetting("Path:SettingFolder"),
                _configurationService.GetStringSetting("Path:JobSettingFile"));
            string message;

            try
            {
                // Check if the JSON file exists
                if (!File.Exists(filePath))
                {
                    message = TranslationService.GetInstance().GetText("jsonFileNotExist");
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    return 0; // Return 0 if file does not exist
                }

                // Read the content of the JSON file
                string jsonString = File.ReadAllText(filePath);
                JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;

                // Try to get the "index" property
                if (root.TryGetProperty("index", out JsonElement indexElement))
                {
                    return indexElement.GetInt32(); // Return the current index
                }
                else
                {
                    message = TranslationService.GetInstance().GetText("indexNotFoundInJson");
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                    return 0; // Default to 0 if index is not found
                }
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs during reading
                message = TranslationService.GetInstance().GetText("errorReadingJsonFile") + ex.Message;
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
                return -1; // Return -1 to indicate an error
            }
        }


        /// <summary>
        /// Retrieves all files in the specified directory.
        /// </summary>
        /// <param name="path">The path of the directory to retrieve files from.</param>
        /// <returns>An enumerable collection of file paths.</returns>
        public static List<string> GetFiles(string path)
        {
            string message;
            try
            {
                return Directory.GetFiles(path).ToList();
            }
            catch (Exception ex)
            {
                return new List<string>(); // Returns an empty array in case of error
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
            ConfigurationService _configurationService = ConfigurationService.GetInstance();
            
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _configurationService.GetStringSetting("Path:ProjectFile"),
                _configurationService.GetStringSetting("Path:SettingFolder"),
                _configurationService.GetStringSetting("Path:JobSettingFile"));
            string message;

            try
            {
                // Check if the JSON file exists
                if (File.Exists(filePath))
                {
                    // Read the content of the JSON file
                    string jsonString = File.ReadAllText(filePath);
                    JobSettingsDto data = JsonSerializer.Deserialize<JobSettingsDto>(jsonString);

                    int newJobId = GetJobIndex(); // Get the new job index
                    IncrementJobIndex(data); // Increment the job index

                    // Set the initial state for the new job
                    JobSaveStateEnum saveState;
                    if (!Enum.TryParse<JobSaveStateEnum>("INACTIVE", out saveState))
                    {
                        message = TranslationService.GetInstance().GetText("errorConvertingSaveState");
                        ConsoleUtil.PrintTextconsole(message);
                        LogManager.Instance.AddMessage(message);
                        return;
                    }

                    FileInPendingJobDTO fileInPendingJobDTO = new FileInPendingJobDTO();
                    fileInPendingJobDTO.Progress = 0.0;

                    // Create a new job model
                    var newJob = new JobModel(name, fileSource, fileTarget, jobSaveTypeEnum, null, null)
                    {
                        Id = newJobId,
                        SaveState = saveState,
                        FileSize = "0 KB",
                        FileTransferTime = "0 sec",
                        Time = DateTime.Now,
                        FileInPending = fileInPendingJobDTO
                    };

                    data.jobs.Add(newJob); // Add the new job to the list

                    // Serialize the updated data back to JSON format
                    string updatedJsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updatedJsonString); // Write the updated JSON to the file

                    message = TranslationService.GetInstance().GetText("jobCree");
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                }
                else
                {
                    message = TranslationService.GetInstance().GetText("jsonFileNotExist");
                    ConsoleUtil.PrintTextconsole(message);
                    LogManager.Instance.AddMessage(message);
                }
            }
            catch (Exception ex)
            {
                // Print an error message if an exception occurs during adding the job
                message = TranslationService.GetInstance().GetText("errorAddingJobToJson") + ex.Message;
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
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
                string message = $"{TranslationService.GetInstance().GetText("errorGettingDirectories")}: '{path}' - {ex.Message}";
                ConsoleUtil.PrintTextconsole(message);
                LogManager.Instance.AddMessage(message);
                return new string[0]; // Returns an empty array in case of error
            }
        }

        /// <summary>
        /// Increments the job index in the JSON file.
        /// </summary>
        public static void IncrementJobIndex(JobSettingsDto data)
        {
            ConfigurationService _configurationService = ConfigurationService.GetInstance();
            
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _configurationService.GetStringSetting("Path:ProjectFile"),
                _configurationService.GetStringSetting("Path:SettingFolder"),
                _configurationService.GetStringSetting("Path:JobSettingFile"));

            try
            {
                // Check if the JSON file exists
                if (File.Exists(filePath))
                {
                    // Read the content of the JSON file
                    string jsonString = File.ReadAllText(filePath);
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

        /// <summary>
        /// Obtient la taille d'un fichier en octets.
        /// </summary>
        /// <param name="filePath">Le chemin complet du fichier.</param>
        /// <returns>La taille du fichier en octets, ou -1 si le fichier n'existe pas.</returns>
        public static long GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length; // Renvoie la taille en octets
            }
            return -1; // Indique que le fichier n'existe pas
        }

        /// <summary>
        /// Calcule le temps de transfert entre le fichier source et le fichier cible.
        /// </summary>
        /// <param name="sourceFile">Le chemin complet du fichier source.</param>
        /// <param name="targetFile">Le chemin complet du fichier cible.</param>
        /// <returns>Le temps de transfert en millisecondes, ou -1 en cas d'erreur.</returns>
        public static double CalculateTransferTime(string sourceFile, string targetFile)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); // Démarre le chronomètre
                stopwatch.Stop(); // Arrête le chronomètre
                return stopwatch.Elapsed.TotalMilliseconds; // Renvoie le temps écoulé en millisecondes
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du calcul du temps de transfert : {ex.Message}");
                return -1; // Indique une erreur
            }
        }

        /// <summary>
        /// Calculates the total size of all files in the specified directory.
        /// </summary>
        /// <param name="directory">The directory to calculate the total size for.</param>
        /// <returns>The total size in bytes.</returns>
        public static long CalculateTotalSize(string directory)
        {
            long size = 0;
            foreach (string file in GetFiles(directory))
            {
                size += GetFileSize(file);
            }
            return size;
        }

        /// <summary>
        /// Ensures that the EncryptedFileExtensions key exists in the JobSettingsDto object.
        /// If the key is missing, it initializes it with an empty list and updates the JSON file.
        /// </summary>
        /// <param name="data">The JobSettingsDto object to check and update.</param>
        /// <param name="settingFilePath">The path to the settings JSON file.</param>
        private static void EnsureKeyExists(AppSettingDto data, string settingFilePath, string key)
        {
            var property = typeof(AppSettingDto).GetProperty(key);
            if (property != null && property.GetValue(data) == null)
            {
                property.SetValue(data, new List<string>());
                string updatedJsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingFilePath, updatedJsonString); // Write the updated JSON to the file
            }
        }


        /// <summary>
        /// Retrieves a specified list from the JobSettingsDto object.
        /// If the list is not present or empty, it returns an empty list.
        /// </summary>
        /// <param name="key">The property name of the list (e.g., "EncryptedFileExtensions" or "PriorityBusinessProcess").</param>
        /// <returns>A list of values stored in the specified property.</returns>
        public static List<string> GetAppSettingsList(string key)
        {
            // Define the file path for the settings JSON file
            string settingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "easySave", "easySaveSetting", "appSetting.json");

            List<string> retrievedList = [];

            try
            {
                // Check if the JSON file exists
                if (File.Exists(settingFilePath))
                {
                    // Read the content of the JSON file
                    string jsonString = File.ReadAllText(settingFilePath);
                    AppSettingDto data = JsonSerializer.Deserialize<AppSettingDto>(jsonString);
                    // Ensure the specified list key exists
                    EnsureKeyExists(data, settingFilePath, key);

                    // Retrieve the list dynamically
                    var property = typeof(AppSettingDto).GetProperty(key);
                    if (property != null && property.GetValue(data) is List<string> list)
                    {
                        retrievedList = list;
                    }
                }
            }
            catch (Exception ex)
            {
                // Print error message if an exception occurs
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorReadingFormat") + ex.Message);
            }
            return retrievedList;
        }


        /// <summary>
        /// Retrieves the file extension from the given file path.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The file extension, or an empty string if the file has no extension.</returns>
        public static string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath); // Returns the extension (including the dot)
        }

        /// <summary>
        /// Retrieves all files from a directory, including its subdirectories recursively.
        /// </summary>
        /// <param name="directoryPath">The path of the source directory.</param>
        /// <returns>A list of full file paths found in the directory and its subdirectories.</returns>
        public static List<string> GetAllFilesAndDirectories(string rootPath)
        {
            List<string> allPaths = new List<string>();

            try
            {

                allPaths.AddRange(Directory.GetFiles(rootPath));


                foreach (string directory in Directory.GetDirectories(rootPath))
                {
                    allPaths.AddRange(GetAllFilesAndDirectories(directory));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des fichiers/dossiers : {ex.Message}");
            }

            return allPaths;
        }

        /// <summary>
        /// Gets the relative path of a file or directory based on the provided root directory.
        /// </summary>
        /// <param name="rootDir">The root directory to base the relative path on.</param>
        /// <param name="fullPath">The full path of the file or directory.</param>
        /// <returns>The relative path from the root directory to the specified file or directory.</returns>
        public static string GetRelativePath(string rootDir, string fullPath)
        {
            // Ensure the paths are normalized and absolute
            var rootDirectoryInfo = new DirectoryInfo(rootDir);
            var fullPathInfo = new FileInfo(fullPath);

            // Ensure the full path belongs to the root directory
            if (!fullPathInfo.FullName.StartsWith(rootDirectoryInfo.FullName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The full path must be within the root directory.", nameof(fullPath));
            }

            // Calculate the relative path
            Uri rootUri = new Uri(rootDirectoryInfo.FullName + Path.DirectorySeparatorChar);
            Uri fullPathUri = new Uri(fullPathInfo.FullName);
            Uri relativeUri = rootUri.MakeRelativeUri(fullPathUri);

            return Uri.UnescapeDataString(relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static void AddValueToJobSettingsList(string key, string value)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easysave", "easySaveSetting", "settings.json");

            try
            {
                // Lire le fichier JSON existant
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);

                    if (settings == null)
                    {
                        settings = new Dictionary<string, List<string>>();
                    }

                    // Vérifier si la clé existe déjà, sinon l'ajouter
                    if (!settings.ContainsKey(key))
                    {
                        settings[key] = new List<string>();
                    }

                    // Ajouter la valeur si elle n'est pas déjà présente
                    if (!settings[key].Contains(value))
                    {
                        settings[key].Add(value);
                    }

                    // Réécrire le fichier JSON avec la nouvelle valeur ajoutée
                    string updatedJsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updatedJsonString);
                }
                else
                {
                    // Créer un fichier avec une nouvelle clé si le fichier n'existe pas
                    var settings = new Dictionary<string, List<string>>()
            {
                { key, new List<string> { value } }
            };

                    string updatedJsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updatedJsonString);
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"Erreur lors de l'ajout de la valeur dans le fichier JSON : {ex.Message}");
            }
        }
        public static void RemoveValueFromJobSettingsList(string key, string value)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easysave", "easySaveSetting", "settings.json");

            try
            {
                if (File.Exists(filePath))
                {
                    // Lire le fichier JSON existant
                    string jsonString = File.ReadAllText(filePath);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);

                    if (settings == null || !settings.ContainsKey(key))
                    {
                        ConsoleUtil.PrintTextconsole($"Clé '{key}' non trouvée dans le fichier JSON.");
                        return;
                    }

                    // Supprimer la valeur si elle existe
                    if (settings[key].Contains(value))
                    {
                        settings[key].Remove(value);

                        // Réécrire le fichier JSON avec la valeur supprimée
                        string updatedJsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(filePath, updatedJsonString);
                    }
                    else
                    {
                        ConsoleUtil.PrintTextconsole($"La valeur '{value}' n'existe pas pour la clé '{key}' dans le fichier JSON.");
                    }
                }
                else
                {
                    ConsoleUtil.PrintTextconsole($"Le fichier {filePath} n'existe pas.");
                }
            }
            catch (Exception ex)
            {
                ConsoleUtil.PrintTextconsole($"Erreur lors de la suppression de la valeur du fichier JSON : {ex.Message}");
            }
        }


        /// <summary>
        /// Retrieves a specified integer value from the AppSettingDto object.
        /// If the key is not present or invalid, it returns a default value.
        /// </summary>
        /// <param name="key">The property name of the integer value (e.g., "MaxLargeFileSize").</param>
        /// <returns>The integer value stored in the specified property, or 0 if not found.</returns>
        public static int GetAppSettingsInt(string key)
        {
            // Define the file path for the settings JSON file
            string settingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "easySave", "easySaveSetting", "appSetting.json");

            int retrievedValue = 0; // Valeur par défaut

            try
            {
                // Check if the JSON file exists
                if (File.Exists(settingFilePath))
                {
                    // Read the content of the JSON file
                    string jsonString = File.ReadAllText(settingFilePath);
                    AppSettingDto data = JsonSerializer.Deserialize<AppSettingDto>(jsonString);

                    // Vérifier si la clé existe
                    EnsureKeyExists(data, settingFilePath, key);

                    // Retrieve the integer value dynamically
                    var property = typeof(AppSettingDto).GetProperty(key);
                    if (property != null && property.GetValue(data) is int value)
                    {
                        retrievedValue = value;
                    }
                }
            }
            catch (Exception ex)
            {
                // Print error message if an exception occurs
                ConsoleUtil.PrintTextconsole(TranslationService.GetInstance().GetText("errorReadingFormat") + ex.Message);
            }

            return retrievedValue;
        }

    }
}
