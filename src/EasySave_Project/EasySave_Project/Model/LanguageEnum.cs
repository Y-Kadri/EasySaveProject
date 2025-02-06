﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Model
{
    public enum LanguageEnum
    {
        FR,
        EN
    }

    public static class LanguageEnumExtensions
    {
        public static string GetLanguagePath(this LanguageEnum language)
        {
            // Récupère le chemin du dossier où l'exécutable est lancé
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string resourceFolder = Path.Combine(exeDirectory, "Resource");

            return language switch
            {
                LanguageEnum.FR => Path.Combine(resourceFolder, "fr.json"),
                LanguageEnum.EN => Path.Combine(resourceFolder, "en.json"),
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
            };
        }
    }
}
