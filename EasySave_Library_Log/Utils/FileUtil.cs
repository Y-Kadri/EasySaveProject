using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave_Library_Log.Utils
{
    public static class FileUtil
    {
        /// <summary>
        /// Creates a directory if it does not exist.
        /// </summary>
        /// <param name="directoryPath">Path of the directory to create.</param>
        public static void CreateDirectoryIfNotExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Reads the content of a file.
        /// </summary>
        /// <param name="filePath">Path of the file to read.</param>
        /// <returns>Content of the file as a string.</returns>
        public static string ReadFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Writes a string to a file.
        /// </summary>
        /// <param name="filePath">Path of the file to write to.</param>
        /// <param name="content">Content to write to the file.</param>
        public static void WriteToFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Combines multiple paths into one.
        /// </summary>
        /// <param name="paths">The paths to combine.</param>
        /// <returns>Combined path.</returns>
        public static string CombinePaths(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// Creates a file if it does not exist, writing initial content to it.
        /// </summary>
        /// <param name="filePath">Path of the file to create.</param>
        /// <param name="initialContent">Initial content to write to the file.</param>
        public static void CreateFileIfNotExists(string filePath, string initialContent)
        {
            if (!File.Exists(filePath))
            {
                WriteToFile(filePath, initialContent);
            }
        }
    }
}
