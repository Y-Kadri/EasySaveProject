using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave_Project.Model;

namespace EasySave_Project.Service
{
    public sealed class TranslationService
    {
        private static TranslationService _instance;
        private static readonly object _lock = new object();
        private Dictionary<string, string> translations;

        // Propriété statique pour stocker la langue choisie
        public static LanguageEnum Language { get; private set; }

        // Constructeur privé
        private TranslationService()
        {
            LoadTranslations();
        }

        // Méthode pour obtenir l'instance du singleton
        public static TranslationService GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new TranslationService();
                    }
                }
            }
            return _instance;
        }

        // Méthode pour définir la langue
        public static void SetLanguage(LanguageEnum language)
        {
            Language = language;
            // Recharger les traductions chaque fois que la langue est définie
            var instance = GetInstance();
            instance.LoadTranslations();
        }

        // Charger les traductions à partir du fichier JSON
        private void LoadTranslations()
        {
            try
            {
                string json = File.ReadAllText(LanguageEnumExtensions.GetLanguagePath(Language));
                translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des traductions : {ex.Message}");
                translations = new Dictionary<string, string>(); // Initialiser à vide en cas d'erreur
            }
        }

        // Récupérer le texte en fonction de la clé
        public string GetText(string key)
        {
            return translations.TryGetValue(key, out var value) ? value : key; // Renvoie la clé si elle n'existe pas
        }
    }
}
