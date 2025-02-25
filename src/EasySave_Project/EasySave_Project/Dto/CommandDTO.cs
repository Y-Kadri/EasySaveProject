using System;
using System.Collections.Generic;

namespace EasySave_Project.Dto;

public class CommandDTO<T>
{
    public string command { get; set; }
    public string id { get; set; }
    public List<T> obj { get; set; }
}