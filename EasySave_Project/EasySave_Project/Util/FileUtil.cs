using System;
using System.IO;
using System.Linq;

namespace EasySave_Project.Util
{
    public static class FileUtil
    {
        public static void CreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"Dossier créé : {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création du dossier '{path}' : {ex.Message}");
            }
        }

        public static bool ExistsDirectory(string path)
        {
            return Directory.Exists(path);
        }

        public static void CopyFile(string sourceFile, string destinationFile, bool overwrite)
        {
            try
            {
                File.Copy(sourceFile, destinationFile, overwrite);
                Console.WriteLine($"Copie du fichier : {sourceFile} -> {destinationFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la copie du fichier '{sourceFile}': {ex.Message}");
            }
        }

        public static void CopyDirectoryComplete(string sourceDir, string targetDir, bool overwrite = false)
        {
            try
            {
                // Vérifier si le répertoire source existe
                if (!ExistsDirectory(sourceDir))
                {
                    Console.WriteLine($"Erreur : Le répertoire source '{sourceDir}' n'existe pas.");
                    return;
                }

                // Créer le répertoire cible s'il n'existe pas
                CreateDirectory(targetDir);

                // Parcourir tous les fichiers du répertoire source
                foreach (string sourceFile in Directory.GetFiles(sourceDir))
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string targetFile = Path.Combine(targetDir, fileName);

                    // Vérifier si le fichier existe dans la destination
                    if (File.Exists(targetFile))
                    {
                        // Si le fichier existe déjà, comparer les dates
                        DateTime sourceFileWriteTime = File.GetLastWriteTime(sourceFile);
                        DateTime targetFileWriteTime = File.GetLastWriteTime(targetFile);

                        // Copier uniquement si le fichier source est plus récent que la destination
                        if (sourceFileWriteTime > targetFileWriteTime)
                        {
                            Console.WriteLine($"Mise à jour du fichier : {sourceFile} vers {targetFile}");
                            CopyFile(sourceFile, targetFile, overwrite);
                        }
                        else
                        {
                            Console.WriteLine($"Le fichier n'a pas été modifié : {sourceFile} n'est pas plus récent que {targetFile}");
                        }
                    }
                    else
                    {
                        // Copier le fichier s'il n'existe pas dans la destination
                        Console.WriteLine($"Copie du nouveau fichier : {sourceFile} vers {targetFile}");
                        CopyFile(sourceFile, targetFile, overwrite);
                    }
                }

                // Gérer les sous-répertoires
                foreach (string subDir in Directory.GetDirectories(sourceDir))
                {
                    string subDirName = Path.GetFileName(subDir);
                    string targetSubDir = Path.Combine(targetDir, subDirName);

                    // Appel récursif pour les sous-répertoires
                    CopyDirectoryComplete(subDir, targetSubDir, overwrite);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la copie complète du dossier '{sourceDir}': {ex.Message}");
            }
        }

        public static void CopyDirectoryDifferential(string sourceDir, string targetDir, string lastFullBackupDir, bool overwrite = false)
        {
            try
            {
                // Vérifier si le répertoire source existe
                if (!ExistsDirectory(lastFullBackupDir))
                {
                    Console.WriteLine($"Erreur : Le répertoire de la dernière sauvegarde '{sourceDir}' n'existe plus.");
                    return;
                }

                // Parcourir tous les fichiers du répertoire source
                foreach (string sourceFile in Directory.GetFiles(sourceDir))
                {




                    // Obtenir le nom du fichier et le chemin dans la dernière sauvegarde
                    string fileName = Path.GetFileName(sourceFile);
                    string lastFullBackupFile = Path.Combine(lastFullBackupDir, fileName);
                    string targetFile = Path.Combine(targetDir, fileName);

                    DateTime sourceFileDateModif = File.GetLastWriteTime(sourceFile);
                    DateTime lastFullBackupFileModif = File.GetLastWriteTime(lastFullBackupFile);


                    // Vérifier si le fichier existe dans la dernière sauvegarde
                    if (!File.Exists(lastFullBackupFile) ||
                        (File.GetLastWriteTime(sourceFile) > File.GetLastWriteTime(lastFullBackupFile)))
                    {
                        // Copier le fichier modifié ou nouveau
                        CopyFile(sourceFile, targetFile, overwrite);
                    }
                }

                // Gérer les sous-répertoires
                foreach (string subDir in Directory.GetDirectories(sourceDir))
                {
                    string subDirName = Path.GetFileName(subDir);
                    string lastFullBackupSubDir = Path.Combine(lastFullBackupDir, subDirName);
                    string targetSubDir = Path.Combine(targetDir, subDirName);

                    // Appel récursif pour les sous-répertoires
                    CopyDirectoryDifferential(subDir, targetSubDir, lastFullBackupSubDir, overwrite);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la copie différentielle du dossier '{sourceDir}': {ex.Message}");
            }
        }
    }
}
