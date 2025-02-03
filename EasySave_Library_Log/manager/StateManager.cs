using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave_Library_Log.Utils;

namespace EasySave_Library_Log
{
    /// <summary>
    /// Singleton responsible for managing the real-time state of backup jobs.
    /// </summary>
    public sealed class StateManager
    {
        private static readonly Lazy<StateManager> instance = new(() => new StateManager());

        private readonly object _lock = new();
        private string stateFilePath;

        /// <summary>
        /// Gets the unique instance of StateManager.
        /// </summary>
        public static StateManager Instance => instance.Value;

        /// <summary>
        /// Private constructor to prevent direct instantiation.
        /// </summary>
        private StateManager()
        {
            // Define the path for the state file
            string statesDirectory = "C:\\Users\\Yanis\\Documents\\TestLogEasySave\\Logs"; // Define your desired path here
            stateFilePath = FileUtil.CombinePaths(statesDirectory, "state.json");
            FileUtil.CreateDirectoryIfNotExists(statesDirectory); // Ensure the directory exists
            FileUtil.CreateFileIfNotExists(stateFilePath, "[]"); // Create the state file if it doesn't exist
        }

        /// <summary>
        /// Updates the state of a backup job.
        /// </summary>
        public void UpdateState(BackupJobState jobState)
        {
            lock (_lock)
            {
                try
                {
                    string jsonString = FileUtil.ReadFromFile(stateFilePath);
                    var states = JsonSerializer.Deserialize<List<BackupJobState>>(jsonString) ?? new List<BackupJobState>();

                    states.Add(jobState);
                    FileUtil.WriteToFile(stateFilePath, JsonSerializer.Serialize(states, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Unable to write to the state file: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Represents the state of a backup job.
    /// </summary>
    public class BackupJobState
    {
        public string JobName { get; set; }
        public string LastActionTimestamp { get; set; }
        public string JobStatus { get; set; } // e.g., Active, Inactive
        public int TotalEligibleFiles { get; set; }
        public long TotalFileSize { get; set; } // Total size of files to transfer
        public double Progress { get; set; } // Progress percentage
        public int RemainingFiles { get; set; } // Number of remaining files
        public long RemainingFileSize { get; set; } // Size of remaining files
        public string CurrentSourceFilePath { get; set; } // Full path of the current source file
        public string CurrentDestinationFilePath { get; set; } // Full path of the current destination file
    }
}
