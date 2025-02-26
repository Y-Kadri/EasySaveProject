using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using Avalonia.Threading;
using ReactiveUI;
using EasySave_Project.Dto;
using EasySave_Project.Service;
using EasySave_Library_Log.manager;

namespace EasySave_Project.ViewModels.Pages
{
    public class LogsPageViewModel : ReactiveObject
    {
        private readonly LogManager _logManager = LogManager.Instance; 
        
        private readonly TranslationService _translationService = TranslationService.GetInstance();
        
        private ObservableCollection<LogNode> _nodes;
        
        private string _allLogs, _timestamp, _sourcePath, _targetPath, 
            _fileSize, _transferTime, _encryptionTime;
        
        public ObservableCollection<LogNode> Nodes
        {
            get => _nodes;
            private set => this.RaiseAndSetIfChanged(ref _nodes, value);
        }

        public string AllLogs
        {
            get => _allLogs;
            private set => this.RaiseAndSetIfChanged(ref _allLogs, value);
        }

        public string Timestamp
        {
            get => _timestamp;
            private set => this.RaiseAndSetIfChanged(ref _timestamp, value);
        }

        public string SourcePath
        {
            get => _sourcePath;
            private set => this.RaiseAndSetIfChanged(ref _sourcePath, value);
        }

        public string TargetPath
        {
            get => _targetPath;
            private set => this.RaiseAndSetIfChanged(ref _targetPath, value);
        }

        public string FileSize
        {
            get => _fileSize;
            private set => this.RaiseAndSetIfChanged(ref _fileSize, value);
        }

        public string TransferTime
        {
            get => _transferTime;
            private set => this.RaiseAndSetIfChanged(ref _transferTime, value);
        }

        public string EncryptionTime
        {
            get => _encryptionTime;
            private set => this.RaiseAndSetIfChanged(ref _encryptionTime, value);
        }

        public LogsPageViewModel()
        {
            Nodes = new ObservableCollection<LogNode>();
            Refresh();
        }
        
        /// <summary>
        /// Refreshes translations and reloads the logs.
        /// </summary>
        /// <summary>
        /// Refreshes translations and reloads the logs.
        /// </summary>
        public void Refresh()
        {
            LoadTranslations();
            LoadLogs();
        }
        
        /// <summary>
        /// Loads translated text values for log-related UI elements from the translation service.
        /// </summary>
        private void LoadTranslations()
        {
            Dispatcher.UIThread.Post(() =>
            {
                AllLogs = _translationService.GetText("AllLogs");
                Timestamp = _translationService.GetText("Timestamp");
                SourcePath = _translationService.GetText("SourcePath");
                TargetPath = _translationService.GetText("TargetPath");
                FileSize = _translationService.GetText("FileSize");
                TransferTime = _translationService.GetText("TransferTime");
                EncryptionTime = _translationService.GetText("EncryptionTime");
            });
        }
        
        /// <summary>
        /// Loads log files from the designated directory and processes them into a structured format.
        /// </summary>
        private void LoadLogs()
        {
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "Logs");

            if (!Directory.Exists(logsDirectory))
            {
                Nodes.Clear();
                return;
            }

            var files = Directory.GetFiles(logsDirectory)
                .Where(file => file.EndsWith(".json") || file.EndsWith(".xml"))
                .ToList();

            var newNodes = new ObservableCollection<LogNode>();

            foreach (var logFile in files)
            {
                if (Path.GetFileNameWithoutExtension(logFile) == "state")
                    continue;

                try
                {
                    var node = CreateLogNode(logFile);
                    if (node != null)
                    {
                        newNodes.Add(node);
                    }
                }
                catch (Exception ex)
                {
                    _logManager.AddMessage($"Erreur de lecture du fichier {logFile}: {ex.Message}");
                }
            }

            Dispatcher.UIThread.Post(() => Nodes = newNodes);
        }

        /// <summary>
        /// Creates a log node representation for a given log file.
        /// </summary>
        /// <param name="logFile">The path of the log file.</param>
        /// <returns>A structured log node or null if the log file is invalid.</returns>
        private LogNode CreateLogNode(string logFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(logFile);
            var dateNode = new LogNode(fileName);

            var logs = DeserializeLogs(logFile);
            if (logs == null || !logs.Any())
            {
                _logManager.AddMessage($"Aucun log valide trouvé dans : {logFile}");
                return null;
            }

            foreach (var log in logs)
            {
                var jobNode = CreateJobNode(log);
                dateNode.SubNodes.Add(jobNode);
            }

            return dateNode;
        }

        /// <summary>
        /// Deserializes logs from JSON or XML format.
        /// </summary>
        /// <param name="filePath">The path of the log file.</param>
        /// <returns>A list of log data objects or null if deserialization fails.</returns>
        private List<LogDataDto> DeserializeLogs(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                string extension = Path.GetExtension(filePath).ToLower();

                return extension switch
                {
                    ".json" => JsonSerializer.Deserialize<List<LogDataDto>>(content),
                    ".xml" => DeserializeXmlManually(content),
                    _ => throw new NotSupportedException($"Extension non supportée : {extension}")
                };
            }
            catch (Exception ex)
            {
                _logManager.AddMessage($"Erreur de désérialisation pour {filePath} : {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Manually deserializes log data from an XML string.
        /// </summary>
        /// <param name="xmlContent">The XML content as a string.</param>
        /// <returns>A list of deserialized log entries.</returns>
        private List<LogDataDto> DeserializeXmlManually(string xmlContent)
        {
            var logs = new List<LogDataDto>();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                var logEntries = doc.SelectNodes("//LogEntry");
                if (logEntries == null) return logs;

                foreach (XmlNode entry in logEntries)
                {
                    var log = new LogDataDto
                    {
                        Timestamp = DateTime.Parse(entry.SelectSingleNode("Timestamp")?.InnerText ?? string.Empty),
                        JobName = entry.SelectSingleNode("JobName")?.InnerText ?? string.Empty,
                        SourcePath = entry.SelectSingleNode("SourcePath")?.InnerText ?? string.Empty,
                        TargetPath = entry.SelectSingleNode("TargetPath")?.InnerText ?? string.Empty,
                        FileSize = long.TryParse(entry.SelectSingleNode("FileSize")?.InnerText, out var fileSize) ? fileSize : 0,
                        TransferTime = double.TryParse(entry.SelectSingleNode("TransferTime")?.InnerText, out var transferTime) ? transferTime : 0,
                        EncryptionTime = double.TryParse(entry.SelectSingleNode("EncryptionTime")?.InnerText, out var encryptionTime) ? encryptionTime : 0,
                        Messages = new List<MessageDto>()
                    };

                    var messages = entry.SelectNodes("Messages/Message");
                    if (messages != null)
                    {
                        foreach (XmlNode messageNode in messages)
                        {
                            log.Messages.Add(new MessageDto { Text = messageNode.SelectSingleNode("Text")?.InnerText ?? string.Empty });
                        }
                    }

                    logs.Add(log);
                }
            }
            catch (Exception ex)
            {
                _logManager.AddMessage($"Erreur lors du parsing manuel du XML : {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Creates a job-specific log node containing relevant log details.
        /// </summary>
        /// <param name="log">The log data object.</param>
        /// <returns>A structured log node for the job.</returns>
        private LogNode CreateJobNode(LogDataDto log)
        {
            var jobNode = new LogNode(log.JobName)
            {
                SubNodes =
                {
                    new LogNode($"{Timestamp}: {log.Timestamp}"),
                    new LogNode($"{SourcePath}: {log.SourcePath}"),
                    new LogNode($"{TargetPath}: {log.TargetPath}"),
                    new LogNode($"{FileSize}: {log.FileSize} bytes"),
                    new LogNode($"{TransferTime}: {log.TransferTime} sec"),
                    new LogNode($"{EncryptionTime}: {log.EncryptionTime}"),
                    CreateMessageNode(log.Messages)
                }
            };
            return jobNode;
        }

        /// <summary>
        /// Creates a log node containing messages related to a job.
        /// </summary>
        /// <param name="messages">The list of message data objects.</param>
        /// <returns>A structured log node for messages.</returns>
        private LogNode CreateMessageNode(List<MessageDto> messages)
        {
            var messageNode = new LogNode(_translationService.GetText("Messages"));
            foreach (var message in messages)
            {
                messageNode.SubNodes.Add(new LogNode(message.Text));
            }
            return messageNode;
        }
    }
}
