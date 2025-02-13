using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EasySave_Project.Dto;
using EasySave_Project.Service;
using System.IO;
using System.Text.Json;
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

        public LogsPageViewModel()
        {
            AllLogs = TranslationService.GetInstance().GetText("AllLogs");
            Timestamp = TranslationService.GetInstance().GetText("Timestamp");
            SourcePath = TranslationService.GetInstance().GetText("SourcePath");
            TargetPath = TranslationService.GetInstance().GetText("TargetPath");
            FileSize = TranslationService.GetInstance().GetText("FileSize");
            TransferTime = TranslationService.GetInstance().GetText("TransferTime");
            Nodes = new ObservableCollection<LogNode>();

            LoadLogs();
        }

        private void LoadLogs()
        {
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "Logs");

            if (!Directory.Exists(logsDirectory))
                return;

            foreach (var logFile in Directory.GetFiles(logsDirectory, "*.json"))
            {
                if (Path.GetFileNameWithoutExtension(logFile) == "state")
                    continue;

                try
                {
                    AddLogFileToTree(logFile);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.AddMessage($"Erreur de lecture du fichier {logFile}: {ex.Message}");
                }
            }
        }

        private void AddLogFileToTree(string logFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(logFile);
            var dateNode = new LogNode(fileName);

            var jsonContent = File.ReadAllText(logFile);
            var logs = JsonSerializer.Deserialize<List<LogDataDto>>(jsonContent);

            if (logs == null)
                return;

            foreach (var log in logs)
            {
                var jobNode = CreateJobNode(log);
                dateNode.SubNodes.Add(jobNode);
            }

            Nodes.Add(dateNode);
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
