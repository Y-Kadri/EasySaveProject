using EasySave_Project.Manager;
using EasySave_Project.Model;
using System.Text.Json;

namespace EasySave_Project.Service
{
    public class LoadDataService
    {
        private JobManager _jobManager;
        private string _filePath;

        public LoadDataService()
        {
            _jobManager = JobManager.GetInstance();
            _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySaveSetting", "jobsSetting.json");
        }

        public void LoadJobs()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Console.WriteLine("Le fichier JSON n'existe pas.");
                    return;
                }

                string jsonString = ReadJsonFile();
                if (string.IsNullOrEmpty(jsonString)) return;

                List<JobModel> jobs = ParseJson(jsonString);
                foreach (var job in jobs)
                {
                    _jobManager.AddJob(job);
                }

                Console.WriteLine("Jobs chargés avec succès.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des jobs : {ex.Message}");
            }
        }

        private string ReadJsonFile()
        {
            try
            {
                return File.ReadAllText(_filePath);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Erreur de lecture du fichier : {ex.Message}");
                return string.Empty;
            }
        }

        private List<JobModel> ParseJson(string jsonString)
        {
            var jobs = new List<JobModel>();

            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("jobs", out JsonElement jobsArray))
                {
                    foreach (JsonElement jobData in jobsArray.EnumerateArray())
                    {
                        JobModel? job = CreateJobFromJson(jobData);
                        if (job != null)
                        {
                            jobs.Add(job);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Le fichier JSON ne contient pas de propriété 'jobs'.");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erreur de désérialisation JSON : {ex.Message}");
            }

            return jobs;
        }

        private JobModel? CreateJobFromJson(JsonElement jobData)
        {
            try
            {
                int id = jobData.GetProperty("id").GetInt32();
                string name = jobData.GetProperty("Name").GetString() ?? "Unnamed";
                string fileSource = jobData.GetProperty("FileSource").GetString() ?? string.Empty;
                string fileTarget = jobData.GetProperty("FileTarget").GetString() ?? string.Empty;
                string fileSize = jobData.GetProperty("FileSize").GetString() ?? "0";
                string fileTransferTime = jobData.GetProperty("FileTransferTime").GetString() ?? "0";

                if (!Enum.TryParse(jobData.GetProperty("SaveState").GetString(), true, out JobSaveStateEnum saveState))
                {
                    Console.WriteLine($"Erreur de conversion de SaveState : '{jobData.GetProperty("SaveState").GetString()}'");
                    return null;
                }

                if (!Enum.TryParse(jobData.GetProperty("SaveType").GetString(), true, out JobSaveTypeEnum saveType))
                {
                    Console.WriteLine($"Erreur de conversion de SaveType : '{jobData.GetProperty("SaveType").GetString()}'");
                    return null;
                }

                if (!DateTime.TryParse(jobData.GetProperty("Time").GetString(), out DateTime time))
                {
                    Console.WriteLine($"Erreur de conversion de DateTime : '{jobData.GetProperty("Time").GetString()}'");
                    return null;
                }

                return new JobModel(name, fileSource, fileTarget, saveType)
                {
                    id = id,
                    SaveState = saveState,
                    FileSize = fileSize,
                    FileTransferTime = fileTransferTime,
                    Time = time
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création du job : {ex.Message}");
                return null;
            }
        }
    }
}
