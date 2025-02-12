using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.IO;
using System.Text.Json;
using EasySave_Project.Dto;
using EasySave_Project.Service;

namespace EasySave_Project.ViewModels.Pages
{
    public class LogsPageViewModel : ReactiveObject
    {
        public ObservableCollection<LogNode> Nodes { get; }
        
        public string AllLogs { get; private set; }
        
        public LogsPageViewModel()
        {
            
            AllLogs = TranslationService.GetInstance().GetText("AllLogs");
            
            Nodes = new ObservableCollection<LogNode>();

            // Chemin vers le dossier Logs
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "easySave", "Logs");

            if (Directory.Exists(logsDirectory))
            {
                // Parcourir tous les fichiers de log
                var logFiles = Directory.GetFiles(logsDirectory, "*.json");

                foreach (var logFile in logFiles)
                {
                    if (Path.GetFileNameWithoutExtension(logFile) == "state")
                    {
                        continue;
                    }
                    
                    string fileName = Path.GetFileNameWithoutExtension(logFile); // Par exemple: "12-09-2025"
                    var dateNode = new LogNode(fileName); // Crée un nœud pour la date

                    // Lire le contenu JSON du fichier
                    var jsonContent = File.ReadAllText(logFile);
                    var logs = JsonSerializer.Deserialize<List<LogDataDto>>(jsonContent);

                    foreach (var log in logs)
                    {
                        // Crée un nœud pour chaque Job
                        var jobNode = new LogNode(log.JobName);

                        // Ajouter les détails du job
                        jobNode.SubNodes.Add(new LogNode($"Timestamp: {log.Timestamp}"));
                        jobNode.SubNodes.Add(new LogNode($"SourcePath: {log.SourcePath}"));
                        jobNode.SubNodes.Add(new LogNode($"TargetPath: {log.TargetPath}"));
                        jobNode.SubNodes.Add(new LogNode($"FileSize: {log.FileSize} bytes"));
                        jobNode.SubNodes.Add(new LogNode($"TransferTime: {log.TransferTime} sec"));

                        // Ajouter les messages
                        var messageNode = new LogNode("Messages");
                        foreach (var message in log.Messages)
                        {
                            messageNode.SubNodes.Add(new LogNode(message.Text));
                        }
                        jobNode.SubNodes.Add(messageNode);

                        // Ajouter le job sous le nœud de la date
                        dateNode.SubNodes.Add(jobNode);
                    }

                    // Ajouter le nœud de la date à la racine de l'arbre
                    Nodes.Add(dateNode);
                }
            }
        }
    }
}
