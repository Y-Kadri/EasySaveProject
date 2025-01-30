using System;

namespace EasySave_Project.Model
{
    public class JobModel
    {
        // Propriétés publiques
        public JobSaveStateEnum SaveState { get; set; } = JobSaveStateEnum.INACTIVE;
        public JobSaveTypeEnum SaveType { get; set; }
        
        public string Name { get; set; }
        public string FileSource { get; set; }
        public string FileTarget { get; set; }
        public string FileSize { get; set; } = "0 KB";
        public string FileTransferTime { get; set; } = "0 sec";
        public DateTime Time { get; set; } = DateTime.Now;

        public JobModel(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum)
        {
            this.Name = name;
            this.FileSource = fileSource;
            this.FileTarget = fileTarget;
            this.SaveType = jobSaveTypeEnum;
        }
        
        public override string ToString()
        {
            return $"Job {Name} : De {FileSource} vers {FileTarget} datant du {Time} de type {SaveType}";
        }
    }
}