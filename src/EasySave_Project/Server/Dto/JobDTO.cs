namespace Server;

public class JobDTO
{
    public int Id { get; set; }
    public string SaveState { get; set; }
    public string SaveType { get; set; }
    public string Name { get; set; }
    public string FileSource { get; set; }
    public string FileTarget { get; set; }
    public string FileSize { get; set; }
    public string FileTransferTime { get; set; }
    public string? LastFullBackupPath { get; set; }
    public string? LastSaveDifferentialPath { get; set; }
    public DateTime Time { get; set; }
}