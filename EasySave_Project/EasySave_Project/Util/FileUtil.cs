using System;
using System.IO;
using System.Text.Json;
using EasySave_Project.Dto;
using EasySave_Project.Model;

namespace EasySave_Project.Util
{
    public class FileUtil
    {
        // Méthode pour s'assurer que le dossier et le fichier existent
        public static void EnsureDirectoryAndFileExist(string fileName)
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting");
            string filePath = Path.Combine(directoryPath, fileName);

            try
            {
                // Vérification de l'existence du dossier
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Console.WriteLine($"Dossier créé à l'emplacement : {directoryPath}");
                }

                // Vérification de l'existence du fichier
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose(); // Crée un fichier vide si il n'existe pas encore
                    Console.WriteLine($"Fichier créé à l'emplacement : {filePath}");
                    
                    // Appeler la méthode pour créer et écrire le fichier JSON
                    CreateDefaultJsonFile(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création du fichier/dossier : {ex.Message}");
            }
        }

        // Méthode pour créer un fichier JSON avec la structure définie
        private static void CreateDefaultJsonFile(string filePath)
        {
            // Créer un objet anonyme avec la structure désirée
            var data = new
            {
                jobs = new string[] { }, // Liste vide
                index = 0
            };

            // Sérialisation de l'objet en JSON
            string jsonString = JsonSerializer.Serialize(data);

            // Écriture du fichier JSON
            try
            {
                File.WriteAllText(filePath, jsonString);
                Console.WriteLine($"Le fichier JSON a été créé avec succès à : {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'écriture du fichier JSON : {ex.Message}");
            }
        }

        public static int GetJobIndex()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", "jobsSetting.json");

            try
            {
                // Vérifier si le fichier existe
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    JsonDocument doc = JsonDocument.Parse(jsonString);
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("index", out JsonElement indexElement))
                    {
                        int currentIndex = indexElement.GetInt32();
                        return currentIndex + 1;
                    }
                    else
                    {
                        Console.WriteLine("'index' n'a pas été trouvé dans le fichier JSON.");
                        return 1;
                    }
                }
                else
                {
                    // Si le fichier n'existe pas
                    Console.WriteLine("Le fichier JSON n'existe pas.");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la lecture du fichier JSON : {ex.Message}");
                return -1;
            }
        }

        public static void AddJobInFile(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", "jobsSetting.json");

            try
            {
                // Vérifier si le fichier existe
                if (File.Exists(filePath))
                {
                    // Lire le fichier JSON
                    string jsonString = File.ReadAllText(filePath);

                    // Désérialiser le JSON en un objet de type JobSettingsDto
                    JobSettingsDto data = JsonSerializer.Deserialize<JobSettingsDto>(jsonString);

                    // Créer un nouveau job à ajouter
                    int newJobId = GetJobIndex();
                    IncrementJobIndex();

                    // Gérer l'énumération SaveState en utilisant Enum.TryParse
                    JobSaveStateEnum saveState;
                    if (!Enum.TryParse<JobSaveStateEnum>("INACTIVE", out saveState))
                    {
                        // Si l'énumération échoue, gérer l'erreur
                        Console.WriteLine("Erreur lors de la conversion de SaveState.");
                        return;
                    }

                    var newJob = new JobModel(name, fileSource, fileTarget, jobSaveTypeEnum)
                    {
                        id = newJobId,
                        SaveState = saveState,
                        FileSize = "0 KB",
                        FileTransferTime = "0 sec",
                        Time = DateTime.Now
                    };

                    // Ajouter le job à la liste des jobs
                    data.jobs.Add(newJob);

                    // Sérialiser l'objet mis à jour en JSON
                    string updatedJsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

                    // Réécrire le fichier avec les données mises à jour
                    File.WriteAllText(filePath, updatedJsonString);

                    Console.WriteLine("Le job a été ajouté avec succès.");
                }
                else
                {
                    Console.WriteLine("Le fichier JSON n'existe pas.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout du job dans le fichier JSON : {ex.Message}");
            }
        }
        
        public static void IncrementJobIndex()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", "jobsSetting.json");

            try
            {
                // Vérifier si le fichier existe
                if (File.Exists(filePath))
                {
                    // Lire le contenu du fichier JSON
                    string jsonString = File.ReadAllText(filePath);

                    // Désérialiser le JSON en un objet de type JobSettings
                    JobSettingsDto data = JsonSerializer.Deserialize<JobSettingsDto>(jsonString);

                    // Incrémenter l'index
                    data.index++;

                    // Sérialiser l'objet mis à jour en JSON
                    string updatedJsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

                    // Réécrire le fichier avec les données mises à jour
                    File.WriteAllText(filePath, updatedJsonString);

                    Console.WriteLine($"L'index a été incrémenté à {data.index}.");
                }
                else
                {
                    Console.WriteLine("Le fichier JSON n'existe pas.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'incrémentation de l'index dans le fichier JSON : {ex.Message}");
            }
        }
    }
}