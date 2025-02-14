using EasySave_Library_Log.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace EasySave_Library_Log.manager
{
    /// <summary>
    /// Singleton class responsible for managing log entries and file operations.
    /// </summary>
    public sealed class LogManager
    {
        private static readonly Lazy<LogManager> instance = new(() => new LogManager());
        private readonly object _lock = new();
        private string logFilePath;
        private List<string> messageBuffer = new();

        /// <summary>
        /// Gets the single instance of LogManager.
        /// </summary>
        public static LogManager Instance => instance.Value;

        /// <summary>
        /// Private constructor to initialize the log manager and set up the log file path.
        /// </summary>
        private LogManager()
        {
           initFolders();
        }

        private void initFolders()
        {
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "Logs");
            string fileExtension = LogFormatManager.Instance.Format == LogFormatManager.LogFormat.JSON ? ".json" : ".xml";
            logFilePath = FileUtil.CombinePaths(logsDirectory, $"{DateTime.Now:yyyy-MM-dd}{fileExtension}");
            FileUtil.CreateDirectoryIfNotExists(logsDirectory);
            FileUtil.CreateFileIfNotExists(logFilePath, LogFormatManager.Instance.Format == LogFormatManager.LogFormat.JSON ? "[]" : "<Logs></Logs>");
        }

        /// <summary>
        /// Adds a message to the message buffer for logging purposes.
        /// </summary>
        /// <param name="message">The message to add to the buffer.</param>
        public void AddMessage(string message)
        {
            lock (_lock)
            {
                messageBuffer.Add(message);
            }
        }

        /// <summary>
        /// Updates the log state with new information about a job and clears the message buffer.
        /// </summary>
        /// <param name="jobName">The name of the backup job.</param>
        /// <param name="sourcePath">The source file path.</param>
        /// <param name="targetPath">The target file path.</param>
        /// <param name="fileSize">The size of the file being copied.</param>
        /// <param name="transferTime">The time taken to transfer the file.</param>
        public void UpdateState(string jobName, string sourcePath, string targetPath, long fileSize, double transferTime, double? encryptionTime = null)
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
                    Messages = messageBuffer.ConvertAll(m => new LogMessage { Text = m }),
                    EncryptionTime = encryptionTime ?? 0
                };

                SaveToFile(logEntry);
                messageBuffer.Clear();
            }
        }

        /// <summary>
        /// Saves the log entry to the specified log file in the appropriate format (JSON or XML).
        /// </summary>
        /// <param name="logEntry">The log entry to save.</param>
        private void SaveToFile(LogEntry logEntry)
        {
            lock (_lock)
            {
                try
                {
                    initFolders();
                    if (LogFormatManager.Instance.Format == LogFormatManager.LogFormat.JSON)
                    {
                        string jsonString = FileUtil.ReadFromFile(logFilePath);
                        var logs = SerializerUtil.DeserializeJson<List<LogEntry>>(jsonString) ?? new List<LogEntry>();
                        logs.Add(logEntry);
                        string logContent = SerializerUtil.SerializeToJson(logs);
                        FileUtil.WriteToFile(logFilePath, logContent);
                    }
                    else // XML
                    {
                        LogsContainer logs;
                        string xmlString = FileUtil.ReadFromFile(logFilePath);
                        logs = SerializerUtil.DeserializeXml<LogsContainer>(xmlString) ?? new LogsContainer();
                        logs.LogEntries.Add(logEntry);
                        string logContent = SerializerUtil.SerializeToXml(logs);
                        FileUtil.WriteToFile(logFilePath, logContent);
                    }
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
        [XmlElement("Timestamp")]
        public string Timestamp { get; set; }

        [XmlElement("JobName")]
        public string JobName { get; set; }

        [XmlElement("SourcePath")]
        public string SourcePath { get; set; }

        [XmlElement("TargetPath")]
        public string TargetPath { get; set; }

        [XmlElement("FileSize")]
        public long FileSize { get; set; }

        [XmlElement("TransferTime")]
        public double TransferTime { get; set; }

        [XmlElement("EncryptionTime")]
        public double EncryptionTime { get; set; }

        [XmlArray("Messages")]
        [XmlArrayItem("Message")]
        public List<LogMessage> Messages { get; set; } = new();
    }

    /// <summary>
    /// Container for a collection of log entries.
    /// </summary>
    [XmlRoot("Logs")]
    public class LogsContainer
    {
        [XmlElement("LogEntry")]
        public List<LogEntry> LogEntries { get; set; } = new();
    }

    /// <summary>
    /// Model for storing a log message.
    /// </summary>
    public class LogMessage
    {
        [XmlElement("Text")]
        public string Text { get; set; }
    }
}
