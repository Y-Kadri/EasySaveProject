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
            string sourceDir = job.FileSource;
            string destinationDir = job.FileTarget;

            // Vérifier que le dossier source existe
            if (!Directory.Exists(sourceDir))
            {
                Console.WriteLine($"Erreur : Le dossier source '{sourceDir}' n'existe pas.");
                return;
            }

            // Copier le dossier (avec écrasement uniquement si c'est une sauvegarde complète)
            FileUtil.CopyDirectory(sourceDir, destinationDir, job.SaveType == JobSaveTypeEnum.COMPLETE);
        }
    }
}
