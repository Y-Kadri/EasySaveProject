using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using EasySave_Library_Log.manager;
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
            initFIles();
            
        }

        private void initFIles()
        {
            string statesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "Logs");
            string fileExtension = LogFormatManager.Instance.Format == LogFormatManager.LogFormat.JSON ? ".json" : ".xml";
            stateFilePath = FileUtil.CombinePaths(statesDirectory, $"state{fileExtension}");
            FileUtil.CreateDirectoryIfNotExists(statesDirectory); // Ensure the directory exists
            FileUtil.CreateFileIfNotExists(stateFilePath, LogFormatManager.Instance.Format == LogFormatManager.LogFormat.JSON ? "[]" : "<States></States>"); // Create the state file if it doesn't exist
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
                    initFIles();
                    if (LogFormatManager.Instance.Format == LogFormatManager.LogFormat.JSON)
                    {
                        string jsonString = FileUtil.ReadFromFile(stateFilePath);
                        var states = JsonSerializer.Deserialize<List<BackupJobState>>(jsonString) ?? new List<BackupJobState>();
                        states.Add(jobState);
                        string logContent = JsonSerializer.Serialize(states, new JsonSerializerOptions { WriteIndented = true });
                        FileUtil.WriteToFile(stateFilePath, logContent);
                    }
                    else // XML
                    {
                        StatesContainer statesContainer;
                        string xmlString = FileUtil.ReadFromFile(stateFilePath);
                        statesContainer = SerializerUtil.DeserializeXml<StatesContainer>(xmlString) ?? new StatesContainer();
                        statesContainer.States.Add(jobState);
                        string logContent = SerializerUtil.SerializeToXml(statesContainer);
                        FileUtil.WriteToFile(stateFilePath, logContent);
                    }
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
    [XmlRoot("BackupJobState")]
    public class BackupJobState
    {
        [XmlElement("JobName")]
        public string JobName { get; set; }

        [XmlElement("LastActionTimestamp")]
        public string LastActionTimestamp { get; set; }

        [XmlElement("JobStatus")]
        public string JobStatus { get; set; } // e.g., Active, Inactive

        [XmlElement("TotalEligibleFiles")]
        public int TotalEligibleFiles { get; set; }

        [XmlElement("TotalFileSize")]
        public long TotalFileSize { get; set; } // Total size of files to transfer

        [XmlElement("Progress")]
        public double Progress { get; set; } // Progress percentage

        [XmlElement("RemainingFiles")]
        public int RemainingFiles { get; set; } // Number of remaining files

        [XmlElement("RemainingFileSize")]
        public long RemainingFileSize { get; set; } // Size of remaining files

        [XmlElement("CurrentSourceFilePath")]
        public string CurrentSourceFilePath { get; set; } // Full path of the current source file

        [XmlElement("CurrentDestinationFilePath")]
        public string CurrentDestinationFilePath { get; set; } // Full path of the current destination file
    }

    /// <summary>
    /// Container for a collection of backup job states.
    /// </summary>
    [XmlRoot("States")]
    public class StatesContainer
    {
        [XmlElement("State")]
        public List<BackupJobState> States { get; set; } = new();
    }
}
