using System;
using System.IO;
using System.Collections.Generic;
using EasySave_Project.Service;

namespace EasySave_Project.Util
{
    /// <summary>
    /// The FileUtil class provides utility methods for file and directory operations.
    /// It includes methods for creating directories, checking existence, copying files,
    /// and retrieving information about files and directories.
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// Creates a directory at the specified path if it does not already exist.
        /// </summary>
        /// <param name="path">The path where the directory will be created.</param>
        public static void CreateDirectory(string path)
        {
            var translator = TranslationService.GetInstance();
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"{translator.GetText("directoryCreated")}: {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{translator.GetText("errorCreatingDirectory")}: '{path}' - {ex.Message}");
            }
        }

        /// <summary>
        /// Checks whether a directory exists at the specified path.
        /// </summary>
        /// <param name="path">The path to check for the existence of a directory.</param>
        /// <returns>True if the directory exists; otherwise, false.</returns>
        public static bool ExistsDirectory(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Copies a file from the source path to the destination path.
        /// </summary>
        /// <param name="sourceFile">The path of the file to copy.</param>
        /// <param name="destinationFile">The path where the file will be copied.</param>
        /// <param name="overwrite">Indicates whether to overwrite the destination file if it exists.</param>
        public static void CopyFile(string sourceFile, string destinationFile, bool overwrite)
        {
            var translator = TranslationService.GetInstance();
            try
            {
                File.Copy(sourceFile, destinationFile, overwrite);
                Console.WriteLine($"{translator.GetText("fileCopied")}: {sourceFile} -> {destinationFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{translator.GetText("errorCopyingFile")}: '{sourceFile}' - {ex.Message}");
            }
        }

        /// <summary>
        /// Checks whether a file exists at the specified path.
        /// </summary>
        /// <param name="path">The path to check for the existence of a file.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        public static bool ExistsFile(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Retrieves the file name from the specified path.
        /// </summary>
        /// <param name="path">The path from which to retrieve the file name.</param>
        /// <returns>The file name, or null if the path is invalid.</returns>
        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Combines two paths into a single path.
        /// </summary>
        /// <param name="path1">The first path.</param>
        /// <param name="path2">The second path.</param>
        /// <returns>The combined path.</returns>
        public static string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        /// <summary>
        /// Retrieves all files in the specified directory.
        /// </summary>
        /// <param name="path">The path of the directory to retrieve files from.</param>
        /// <returns>An enumerable collection of file paths.</returns>
        public static IEnumerable<string> GetFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{TranslationService.GetInstance().GetText("errorCopyingDirectory")}: '{path}' - {ex.Message}");
                return new string[0]; // Returns an empty array in case of error
            }
        }

        /// <summary>
        /// Retrieves all subdirectories in the specified directory.
        /// </summary>
        /// <param name="path">The path of the directory to retrieve subdirectories from.</param>
        /// <returns>An enumerable collection of directory paths.</returns>
        public static IEnumerable<string> GetDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{TranslationService.GetInstance().GetText("errorGettingDirectories")}: '{path}' - {ex.Message}");
                return new string[0]; // Returns an empty array in case of error
            }
        }

        /// <summary>
        /// Retrieves the last write time of the specified file.
        /// </summary>
        /// <param name="path">The path of the file to retrieve the last write time from.</param>
        /// <returns>The DateTime of the last write time.</returns>
        public static DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        /// <summary>
        /// Retrieves the directory name from the specified path.
        /// </summary>
        /// <param name="path">The path from which to retrieve the directory name.</param>
        /// <returns>The directory name, or null if the path is invalid.</returns>
        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }
    }
}
