using EasySave_Project.Manager;
using EasySave_Project.Model;
using System.Text.Json;

namespace EasySave_Project.Service
{
    public class LoadDataService
    {
        private JobManager _jobManager;

        public LoadDataService()
        {
            this._jobManager = JobManager.GetInstance();
        }

        public void LoadJobs()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", "jobsSetting.json");

            try
            {
                // Vérifier si le fichier existe
                if (File.Exists(filePath))
                {
                    // Lire le fichier JSON
                    string jsonString = File.ReadAllText(filePath);

                    try
                    {
                        // Analyser le JSON
                        using (JsonDocument doc = JsonDocument.Parse(jsonString))
                        {
                            // Accéder à la racine du document (l'objet contenant "jobs")
                            JsonElement root = doc.RootElement;

                            // Vérifier que "jobs" existe
                            if (root.TryGetProperty("jobs", out JsonElement jobsArray))
                            {
                                // Parcourir la liste des jobs dans le tableau "jobs"
                                foreach (JsonElement jobData in jobsArray.EnumerateArray())
                                {
                                    // Extraire les propriétés du job
                                    int id = jobData.GetProperty("id").GetInt32();

                                    // Convertir SaveState (une chaîne en énumération)
                                    JobSaveStateEnum saveState;
                                    if (Enum.TryParse<JobSaveStateEnum>(jobData.GetProperty("SaveState").GetString(), ignoreCase: true, out saveState))
                                    {
                                        Console.WriteLine($"SaveState '{jobData.GetProperty("SaveState").GetString()}' converti avec succès en {saveState}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Erreur de conversion de SaveState: '{jobData.GetProperty("SaveState").GetString()}'");
                                        continue; // Passer à l'itération suivante si la conversion échoue
                                    }

                                    // Convertir SaveType (une chaîne en énumération)
                                    JobSaveTypeEnum saveType;
                                    if (Enum.TryParse<JobSaveTypeEnum>(jobData.GetProperty("SaveType").GetString(), ignoreCase: true, out saveType))
                                    {
                                        Console.WriteLine($"SaveType '{jobData.GetProperty("SaveType").GetString()}' converti avec succès en {saveType}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Erreur de conversion de SaveType: '{jobData.GetProperty("SaveType").GetString()}'");
                                        continue; // Passer à l'itération suivante si la conversion échoue
                                    }

                                    string name = jobData.GetProperty("Name").GetString();
                                    string fileSource = jobData.GetProperty("FileSource").GetString();
                                    string fileTarget = jobData.GetProperty("FileTarget").GetString();
                                    string fileSize = jobData.GetProperty("FileSize").GetString();
                                    string fileTransferTime = jobData.GetProperty("FileTransferTime").GetString();

                                    // Convertir la date (en vérifiant si elle est bien au format correct)
                                    if (DateTime.TryParse(jobData.GetProperty("Time").GetString(), out DateTime time))
                                    {
                                        Console.WriteLine($"DateTime '{jobData.GetProperty("Time").GetString()}' converti avec succès en {time}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Erreur de conversion de DateTime: '{jobData.GetProperty("Time").GetString()}'");
                                        continue; // Passer à l'itération suivante si la conversion échoue
                                    }

                                    // Créer une instance de JobModel et l'ajouter à la liste de _jobManager
                                    JobModel job = new JobModel(name, fileSource, fileTarget, saveType)
                                    {
                                        id = id,
                                        SaveState = saveState,
                                        FileSize = fileSize,
                                        FileTransferTime = fileTransferTime,
                                        Time = time
                                    };

                                    _jobManager.AddJob(job);
                                }

                                Console.WriteLine("Jobs chargés avec succès.");
                            }
                            else
                            {
                                Console.WriteLine("Le fichier JSON ne contient pas de propriété 'jobs'.");
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Erreur de désérialisation manuelle : {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Le fichier JSON n'existe pas.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur générale lors du chargement des jobs depuis le fichier JSON : {ex.Message}");
            }
        }
    }
}
