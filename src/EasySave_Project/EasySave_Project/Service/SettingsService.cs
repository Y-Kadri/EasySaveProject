using System;
using System.IO;
using System.Text.Json;
using EasySave_Project.Dto;

namespace EasySave_Project.Service
{
    public class SettingsService
    {
        private readonly string _filePath = "AppSettings.json"; // Le chemin du fichier JSON

        public AppSettingDto LoadAppSettings()
        {
            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<AppSettingDto>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des paramètres : {ex.Message}");
                return null;
            }
        }
    }
}
