using System;
using System.IO;
using EasySave_Project.Model;

namespace EasySave_Project.Manager
{
    public static class JobFactory
    {
        public static JobModel CreateJobModel(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum)
        {
            // Vérification des répertoires
            if (Directory.Exists(fileSource) && Directory.Exists(fileTarget))
            {
                Console.WriteLine($"Répertoires valides : Source = {fileSource}, Cible = {fileTarget}");
                return new JobModel(name, fileSource, fileTarget, jobSaveTypeEnum);
            }
            else
            {
                // Si un répertoire est invalide, afficher des détails
                Console.WriteLine($"Erreur : Source = {fileSource}, Cible = {fileTarget}");
                throw new ArgumentException("Le répertoire source ou le répertoire de destination n'existent pas.");
            }
        }
    }
}