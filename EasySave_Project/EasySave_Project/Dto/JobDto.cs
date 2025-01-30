namespace EasySave_Project.Dto;

public class JobDto
{
    public int id { get; set; }
    public string SaveState { get; set; }
    public string SaveType { get; set; }
    public string Name { get; set; }
    public string FileSource { get; set; }
    public string FileTarget { get; set; }
    public string FileSize { get; set; }
    public string FileTransferTime { get; set; }
    public string Time { get; set; }
}