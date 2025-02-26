using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave_Project.Model;
using EasySave_Project.Util;

namespace EasySave_Project.Service
{
    public sealed class TranslationService
    {
        private static TranslationService _instance;
        private static readonly object _lock = new object();
        private Dictionary<string, string> translations;
        public static LanguageEnum Language { get; private set; }

        private TranslationService()
        {
            LoadTranslations();
        }

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

        public static void SetLanguage(LanguageEnum language)
        {
            Language = language;
            var instance = GetInstance();
            instance.LoadTranslations();
        }

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
                translations = new Dictionary<string, string>();
            }
        }

        public string GetText(string key)
        {
            return translations.TryGetValue(key, out var value) ? value : key;
        }

        /// <summary>
        /// Initializes application settings by setting the language and log format 
        /// based on the stored configuration.
        /// </summary>
        public static void InitSetting()
        {
            SetLanguage(SettingUtil.GetSetting().language);
            EasySave_Library_Log.manager.LogFormatManager.Instance.SetLogFormat(SettingUtil.GetSetting().logFormat);
        }

        /// <summary>
        /// Replaces each word in a given text with its corresponding translation.
        /// </summary>
        /// <param name="text">The input text to be translated.</param>
        /// <returns>The translated text with each word replaced.</returns>
        public string Replace(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            
            string[] words = text.Split(' ');
            
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = GetText(words[i]);
            }

            return string.Join(" ", words);
        }
        
    }
}
