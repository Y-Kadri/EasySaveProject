using System;
using System.IO;
using EasySave_Project.Model;
using EasySave_Project.Util;

namespace EasySave_Project.Service
{
    public class JobService
    {
        public void ExecuteOneJob(JobModel job)
        {
            // Vérifier si le répertoire source existe
            if (!FileUtil.ExistsDirectory(job.FileSource))
            {
                Console.WriteLine($"Erreur : Le répertoire source '{job.FileSource}' n'existe pas.");
                return;
            }

            Console.WriteLine($"Démarrage de la sauvegarde : {job.Name}");

            // Crée le répertoire de destination s'il n'existe pas
            if (!FileUtil.ExistsDirectory(job.FileTarget))
            {
                FileUtil.CreateDirectory(job.FileTarget);
            }

            // Crée un répertoire avec la date et l'heure au format aaaa_mm_jj_hh_mm_ss pour la nouvelle sauvegarde
            string timestampedBackupDir = Path.Combine(job.FileTarget, DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
            FileUtil.CreateDirectory(timestampedBackupDir); // Crée le répertoire de destination

            // Vérifiez si une sauvegarde complète précédente existe
            if (job.SaveType == JobSaveTypeEnum.DIFFERENTIAL)
            {
                // Vérifier si LastFullBackupPath est valide
                if (string.IsNullOrEmpty(job.LastFullBackupPath) || !FileUtil.ExistsDirectory(job.LastFullBackupPath))
                {
                    Console.WriteLine("Aucune sauvegarde complète précédente trouvée, exécution d'une sauvegarde complète.");
                    job.LastFullBackupPath = null; // Mettre à null car la sauvegarde complète n'existe pas
                    FileUtil.CopyDirectoryComplete(job.FileSource, timestampedBackupDir);
                    job.LastFullBackupPath = timestampedBackupDir; // Mettre à jour le chemin de la dernière sauvegarde complète
                }
                else
                {
                    // Effectuer une sauvegarde différentielle
                    FileUtil.CopyDirectoryDifferential(job.FileSource, timestampedBackupDir, job.LastFullBackupPath);
                }
            }
            else
            {
                // Effectuer une sauvegarde complète
                FileUtil.CopyDirectoryComplete(job.FileSource, timestampedBackupDir);
                job.LastFullBackupPath = timestampedBackupDir; // Mettre à jour le chemin de la dernière sauvegarde complète
            }

            Console.WriteLine($"Sauvegarde terminée : {job.Name}");
        }

    }
}
