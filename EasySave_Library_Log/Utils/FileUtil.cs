using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave_Library_Log.Utils
{
    public static class FileUtil
    {
        /// <summary>
        /// Crée un répertoire si celui-ci n'existe pas.
        /// </summary>
        /// <param name="directoryPath">Chemin du répertoire à créer.</param>
        public static void CreateDirectoryIfNotExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Lit le contenu d'un fichier.
        /// </summary>
        /// <param name="filePath">Chemin du fichier à lire.</param>
        /// <returns>Contenu du fichier sous forme de chaîne.</returns>
        public static string ReadFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Écrit une chaîne dans un fichier.
        /// </summary>
        /// <param name="filePath">Chemin du fichier dans lequel écrire.</param>
        /// <param name="content">Contenu à écrire dans le fichier.</param>
        public static void WriteToFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Combine plusieurs chemins en un seul.
        /// </summary>
        /// <param name="paths">Les chemins à combiner.</param>
        /// <returns>Chemin combiné.</returns>
        public static string CombinePaths(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// Crée un fichier s'il n'existe pas, en y écrivant du contenu initial.
        /// </summary>
        public static void CreateFileIfNotExists(string filePath, string initialContent)
        {
            if (!File.Exists(filePath))
            {
                WriteToFile(filePath, initialContent);
            }
        }
    }
}
