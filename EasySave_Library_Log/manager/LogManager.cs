using EasySave_Library_Log.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EasySave_Library_Log.manager
{
    /// <summary>
    /// Singleton responsible for managing daily logs.
    /// </summary>
    public sealed class LogManager
    {
        private static readonly Lazy<LogManager> instance = new(() => new LogManager());
        private readonly object _lock = new();
        private string logFilePath;
        private List<string> messageBuffer = new(); // Buffer to store console messages

        /// <summary>
        /// Gets the single instance of LogManager.
        /// </summary>
        public static LogManager Instance => instance.Value;

        /// <summary>
        /// Private constructor to prevent direct instantiation.
        /// </summary>
        private LogManager()
        {
            // Defines the log file path
            string logsDirectory = "C:\\Users\\Yanis\\Documents\\TestLogEasySave\\Logs";
            logFilePath = FileUtil.CombinePaths(logsDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            FileUtil.CreateDirectoryIfNotExists(logsDirectory);
            FileUtil.CreateFileIfNotExists(logFilePath, "[]"); // Initializes the file if it does not exist
        }

        /// <summary>
        /// Adds a message to the message buffer.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public void AddMessage(string message)
        {
            lock (_lock)
            {
                messageBuffer.Add(message);
            }
        }

        /// <summary>
        /// Updates the log state with new information and console messages.
        /// </summary>
        /// <param name="jobName">The name of the backup job.</param>
        /// <param name="sourcePath">The source file path.</param>
        /// <param name="targetPath">The target file path.</param>
        /// <param name="fileSize">The size of the file being copied.</param>
        /// <param name="transferTime">The time taken to transfer the file.</param>
        public void UpdateState(string jobName, string sourcePath, string targetPath, long fileSize, double transferTime)
        {
            lock (_lock)
            {
                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    JobName = jobName,
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    FileSize = fileSize,
                    TransferTime = transferTime,
                    Messages = messageBuffer.ConvertAll(m => new LogMessage { Text = m }) // Add messages
                };

                SaveToFile(logEntry);
                messageBuffer.Clear(); // Reset after saving
            }
        }

        /// <summary>
        /// Saves a log entry to the daily JSON log file.
        /// </summary>
        /// <param name="logEntry">The log entry to save.</param>
        private void SaveToFile(LogEntry logEntry)
        {
            lock (_lock)
            {
                try
                {
                    string jsonString = FileUtil.ReadFromFile(logFilePath);
                    var logs = JsonSerializer.Deserialize<List<LogEntry>>(jsonString) ?? new List<LogEntry>();

                    logs.Add(logEntry);

                    FileUtil.WriteToFile(logFilePath, JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Unable to write to the log file: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Model representing a log entry.
    /// </summary>
    public class LogEntry
    {
        public string Timestamp { get; set; }
        public string JobName { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public long FileSize { get; set; }
        public double TransferTime { get; set; }
        public List<LogMessage> Messages { get; set; } = new();
    }

    /// <summary>
    /// Model for storing a log message.
    /// </summary>
    public class LogMessage
    {
        public string Text { get; set; }
    }
}
