using EasySave_Project.Model;
using System;
using System.Text.Json.Serialization;
using EasySave_Project.Util;

namespace EasySave_Project.Model
{
    public class JobModel
    {
        // Propriétés publiques
        public int Id { get; set; }
        
        [JsonConverter(typeof(EnumConverter.JsonEnumConverter<JobSaveStateEnum>))]
        public JobSaveStateEnum SaveState { get; set; } = JobSaveStateEnum.INACTIVE;
        
        [JsonConverter(typeof(EnumConverter.JsonEnumConverter<JobSaveTypeEnum>))]
        public JobSaveTypeEnum SaveType { get; set; }
        
        public string Name { get; set; }
        public string FileSource { get; set; }
        public string FileTarget { get; set; }
        public string FileSize { get; set; } = "0 KB";
        public string FileTransferTime { get; set; } = "0 sec";
        public string LastFullBackupPath { get; set; } = null;
        public string LastSaveDifferentialPath { get; set; } = null;
        public DateTime Time { get; set; } = DateTime.Now;
        
        public JobModel() {}

        public JobModel(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum, string LastFullBackupPath, string LastSaveDifferentialPath)
        {
            this.Name = name;
            this.FileSource = fileSource;
            this.FileTarget = fileTarget;
            this.SaveType = jobSaveTypeEnum;
            this.LastFullBackupPath = LastFullBackupPath;
            this.LastSaveDifferentialPath = LastSaveDifferentialPath;

        }
        
        public override string ToString()
        {
            return $"Job {Name} : De {FileSource} vers {FileTarget} datant du {Time} de type {SaveType}";
        }
    }
}
