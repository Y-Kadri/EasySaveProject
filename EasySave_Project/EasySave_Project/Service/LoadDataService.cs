using EasySave_Project.Manager;
using EasySave_Project.Model;
using System.Text.Json;
using EasySave_Project.Dto;

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
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "easySaveSetting", "jobsSetting.json");

            try
            {
                // Vérifier si le fichier existe
                if (File.Exists(filePath))
                {
                    // Lire le fichier JSON
                    string jsonString = File.ReadAllText(filePath);

                    try
                    {
                        // Désérialiser le contenu JSON en JobSettingsDto
                        JobSettingsDto data = JsonSerializer.Deserialize<JobSettingsDto>(jsonString);

                        if (data != null && data.jobs != null)
                        {
                            // Parcourir la liste des jobs dans le DTO
                            foreach (var jobData in data.jobs)
                            {
                                // Créer une instance de JobModel et l'ajouter à la liste de _jobManager
                                JobModel job = new JobModel(jobData.Name, jobData.FileSource, jobData.FileTarget,
                                    jobData.SaveType)
                                {
                                    id = jobData.id,
                                    SaveState = jobData.SaveState,
                                    FileSize = jobData.FileSize,
                                    FileTransferTime = jobData.FileTransferTime,
                                    Time = jobData.Time
                                };

                                _jobManager.AddJob(job);
                            }

                            Console.WriteLine("Jobs chargés avec succès.");
                        }
                        else
                        {
                            Console.WriteLine(
                                "Le fichier JSON ne contient pas de propriété 'jobs' ou la liste des jobs est vide.");
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Erreur de désérialisation du JSON : {ex.Message}");
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
