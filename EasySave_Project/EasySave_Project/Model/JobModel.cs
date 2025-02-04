using EasySave_Project.Model;
using System;
using System.Text.Json.Serialization;
using EasySave_Project.Util;
using System;
using System.Collections.Generic;
using EasySave_Project.Service;

namespace EasySave_Project.Model
{
    /// <summary>
    /// Represents a backup job that can be observed for changes.
    /// </summary>
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

        /// <summary>
        /// Mandatory constructor for .NET JSON Deserialization use
        /// </summary>
        public JobModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobModel"/> class.
        /// </summary>
        /// <param name="name">The name of the job.</param>
        /// <param name="fileSource">The source file path.</param>
        /// <param name="fileTarget">The target file path.</param>
        /// <param name="jobSaveTypeEnum">The type of save operation.</param>
        public JobModel(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum, string LastFullBackupPath, string LastSaveDifferentialPath)
        {
            this.Name = name;
            this.FileSource = fileSource;
            this.FileTarget = fileTarget;
            this.SaveType = jobSaveTypeEnum;
            this.LastFullBackupPath = LastFullBackupPath;
            this.LastSaveDifferentialPath = LastSaveDifferentialPath;

        }

        /// <summary>
        /// Returns a string representation of the job.
        /// </summary>
        /// <returns>A formatted string describing the job.</returns>
        public override string ToString()
        {
            return $"Job {Name} : From {FileSource} to {FileTarget}, created on {Time}, type: {SaveType}";
        }
    }
}
