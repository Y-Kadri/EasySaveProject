using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EasySave_Project.Dto;
using EasySave_Project.Service;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using EasySave_Library_Log.manager;

namespace EasySave_Project.ViewModels.Pages
{
    public class LogsPageViewModel
    {
        public ObservableCollection<LogNode> Nodes { get; }
        public string AllLogs { get; }
        public string Timestamp { get; }
        public string SourcePath { get; }
        public string TargetPath { get; }
        public string FileSize { get; }
        public string TransferTime { get; }
        public string EncryptionTime { get; }

        public LogsPageViewModel()
        {
            var translationService = TranslationService.GetInstance();

            AllLogs = translationService.GetText("AllLogs");
            Timestamp = translationService.GetText("Timestamp");
            SourcePath = translationService.GetText("SourcePath");
            TargetPath = translationService.GetText("TargetPath");
            FileSize = translationService.GetText("FileSize");
            TransferTime = translationService.GetText("TransferTime");
            EncryptionTime = translationService.GetText("EncryptionTime");

            Nodes = new ObservableCollection<LogNode>();

            LoadLogs();
        }

        private void LoadLogs()
        {
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "Logs");

            if (!Directory.Exists(logsDirectory))
                return;

            var files = Directory.GetFiles(logsDirectory)
                .Where(file => file.EndsWith(".json") || file.EndsWith(".xml"))
                .ToList();

            foreach (var logFile in files)
            {
                if (Path.GetFileNameWithoutExtension(logFile) == "state")
                    continue;

                try
                {
                    AddLogFileToTree(logFile);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.AddMessage($"{TranslationService.GetInstance().GetText("errorReadingJsonFile")} {logFile}: {ex.Message}");
                }
            }
        }

        private void AddLogFileToTree(string logFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(logFile);
            var dateNode = new LogNode(fileName);

            var logs = DeserializeLogs(logFile);
            if (logs == null || !logs.Any())
            {
                LogManager.Instance.AddMessage($"{TranslationService.GetInstance().GetText("noEncryptedFormatsSet")} : {logFile}");
                return;
            }

            foreach (var log in logs)
            {
                var jobNode = CreateJobNode(log);
                dateNode.SubNodes.Add(jobNode);
            }

            Nodes.Add(dateNode);
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
                    _ => throw new NotSupportedException($"{TranslationService.GetInstance().GetText("errorFormatLanguage")} {extension}")
                };
            }
            catch (Exception ex)
            {
                LogManager.Instance.AddMessage($"{TranslationService.GetInstance().GetText("errorReadingJsonFile")} {filePath} : {ex.Message}");
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
                LogManager.Instance.AddMessage($"{TranslationService.GetInstance().GetText("errorReadingJsonFile")} : {ex.Message}");
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
            var messageNode = new LogNode(TranslationService.GetInstance().GetText("Messages"));
            foreach (var message in messages)
            {
                messageNode.SubNodes.Add(new LogNode(message.Text));
            }
            return messageNode;
        }
    }
}
