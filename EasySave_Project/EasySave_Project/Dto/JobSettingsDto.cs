using EasySave_Project.Model;

namespace EasySave_Project.Dto;

public class JobSettingsDto
{
    public List<JobModel> jobs { get; set; }
    public int index { get; set; }
}