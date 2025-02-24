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
        private ObservableCollection<LogNode> _nodes;
        public ObservableCollection<LogNode> Nodes
        {
            get => _nodes;
            private set => this.RaiseAndSetIfChanged(ref _nodes, value);
        }

        private string _allLogs;
        public string AllLogs
        {
            get => _allLogs;
            private set => this.RaiseAndSetIfChanged(ref _allLogs, value);
        }

        private string _timestamp;
        public string Timestamp
        {
            get => _timestamp;
            private set => this.RaiseAndSetIfChanged(ref _timestamp, value);
        }

        private string _sourcePath;
        public string SourcePath
        {
            get => _sourcePath;
            private set => this.RaiseAndSetIfChanged(ref _sourcePath, value);
        }

        private string _targetPath;
        public string TargetPath
        {
            get => _targetPath;
            private set => this.RaiseAndSetIfChanged(ref _targetPath, value);
        }

        private string _fileSize;
        public string FileSize
        {
            get => _fileSize;
            private set => this.RaiseAndSetIfChanged(ref _fileSize, value);
        }

        private string _transferTime;
        public string TransferTime
        {
            get => _transferTime;
            private set => this.RaiseAndSetIfChanged(ref _transferTime, value);
        }

        private string _encryptionTime;
        public string EncryptionTime
        {
            get => _encryptionTime;
            private set => this.RaiseAndSetIfChanged(ref _encryptionTime, value);
        }

        private readonly TranslationService _translationService = TranslationService.GetInstance();

        public LogsPageViewModel()
        {
            Nodes = new ObservableCollection<LogNode>();
            Refresh();
        }

        public void Refresh()
        {
            LoadTranslations();
            LoadLogs();
        }

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
                    LogManager.Instance.AddMessage($"Erreur de lecture du fichier {logFile}: {ex.Message}");
                }
            }

            Dispatcher.UIThread.Post(() => Nodes = newNodes);
        }

        private LogNode CreateLogNode(string logFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(logFile);
            var dateNode = new LogNode(fileName);

            var logs = DeserializeLogs(logFile);
            if (logs == null || !logs.Any())
            {
                LogManager.Instance.AddMessage($"Aucun log valide trouvé dans : {logFile}");
                return null;
            }

            foreach (var log in logs)
            {
                var jobNode = CreateJobNode(log);
                dateNode.SubNodes.Add(jobNode);
            }

            return dateNode;
        }

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
                LogManager.Instance.AddMessage($"Erreur de désérialisation pour {filePath} : {ex.Message}");
                return null;
            }
        }

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
                LogManager.Instance.AddMessage($"Erreur lors du parsing manuel du XML : {ex.Message}");
            }

            return logs;
        }

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

        private LogNode CreateMessageNode(List<MessageDto> messages)
        {
            var messageNode = new LogNode("Messages");
            foreach (var message in messages)
            {
                messageNode.SubNodes.Add(new LogNode(message.Text));
            }
            return messageNode;
        }
    }
}
