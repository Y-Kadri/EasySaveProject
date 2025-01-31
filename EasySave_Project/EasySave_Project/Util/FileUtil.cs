using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Util
{
    public static class FileUtil
    {
        public static void CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                Console.WriteLine($"Dossier créé : {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création du dossier '{path}' : {ex.Message}");
            }
        }

        public static void CopyDirectory(string sourceDir, string targetDir, bool overwrite = false)
        {
            try
            {
                if (!Directory.Exists(sourceDir))
                {
                    Console.WriteLine($"Erreur : Le répertoire source '{sourceDir}' n'existe pas.");
                    return;
                }

                // Inclure le nom du dossier source dans la destination
                string finalTargetDir = Path.Combine(targetDir, Path.GetFileName(sourceDir));
                Directory.CreateDirectory(finalTargetDir); // Créer le dossier principal s'il n'existe pas

                foreach (string file in Directory.GetFiles(sourceDir))
                    File.Copy(file, Path.Combine(finalTargetDir, Path.GetFileName(file)), overwrite);

                foreach (string subDir in Directory.GetDirectories(sourceDir))
                    CopyDirectory(subDir, finalTargetDir, overwrite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la copie du dossier '{sourceDir}': {ex.Message}");
            }
        }
    }
}
