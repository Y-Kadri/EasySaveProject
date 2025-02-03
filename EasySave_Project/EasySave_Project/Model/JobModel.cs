using EasySave_Project.Model;

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
    public string LastFullBackupPath { get; set; } = null;
    public string LastSaveDifferentialPath { get; set; } = null;

    public JobModel(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum, string lastFullBackupPath)
    {
        this.Name = name;
        this.FileSource = fileSource;
        this.FileTarget = fileTarget;
        this.SaveType = jobSaveTypeEnum;
        this.LastFullBackupPath = lastFullBackupPath; //TODO enlever cette variable du constructeur
    }
}
