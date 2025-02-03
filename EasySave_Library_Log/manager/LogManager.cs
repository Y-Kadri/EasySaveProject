using EasySave_Library_Log.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EasySave_Library_Log.manager
{
    /// <summary>
    /// Singleton responsable de la gestion des logs journaliers.
    /// </summary>
    public sealed class LogManager
    {
        private static readonly Lazy<LogManager> instance = new(() => new LogManager());

        private readonly object _lock = new();
        private string logFilePath;

        /// <summary>
        /// Obtient l'instance unique de LogManager.
        /// </summary>
        public static LogManager Instance => instance.Value;

        /// <summary>
        /// Constructeur privé pour empêcher l'instanciation directe.
        /// </summary>
        private LogManager()
        {
            // Définit le chemin du fichier log
            string logsDirectory = "C:\\Users\\Yanis\\Documents\\TestLogEasySave\\Logs";
            logFilePath = FileUtil.CombinePaths(logsDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            FileUtil.CreateDirectoryIfNotExists(logsDirectory);
            // Crée le fichier log si nécessaire
            FileUtil.CreateFileIfNotExists(logFilePath, "[]");
        }

        /// <summary>
        /// Met à jour l'état du log avec de nouvelles informations.
        /// </summary>
        public void UpdateState(string jobName, string sourcePath, string targetPath, long fileSize, double transferTime)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                JobName = jobName,
                SourcePath = sourcePath,
                TargetPath = targetPath,
                FileSize = fileSize,
                TransferTime = transferTime
            };

            SaveToFile(logEntry);
        }

        /// <summary>
        /// Sauvegarde une entrée de log dans le fichier JSON journalier.
        /// </summary>
        private void SaveToFile(LogEntry logEntry)
        {
            lock (_lock)
            {
                try
                {
                    // Lire le contenu actuel du fichier log ou initialiser une nouvelle liste
                    string jsonString = FileUtil.ReadFromFile(logFilePath);
                    var logs = JsonSerializer.Deserialize<List<LogEntry>>(jsonString) ?? new List<LogEntry>();

                    // Ajouter la nouvelle entrée de log
                    logs.Add(logEntry);

                    // Écrire les logs mis à jour dans le fichier
                    FileUtil.WriteToFile(logFilePath, JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Impossible d'écrire dans le fichier log : {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Modèle représentant une entrée de log.
    /// </summary>
    public class LogEntry
    {
        public string Timestamp { get; set; }
        public string JobName { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public long FileSize { get; set; }
        public double TransferTime { get; set; }
    }
}
