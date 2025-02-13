using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using EasySave_Project.Util;

namespace EasySave_Project.Dto
{
    public class LogDto
    {
        public string Name { get; set; }
        public List<LogDataDto> Logs { get; set; }
    }

    public class LogDataDto
    {
        [JsonConverter(typeof(CustomDateTimeConverterUtil))]
        public DateTime Timestamp { get; set; }
        public string JobName { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public long FileSize { get; set; }
        public double TransferTime { get; set; }
        public List<MessageDto> Messages { get; set; }
    }

    public class MessageDto
    {
        public string Text { get; set; }
    }

    public class LogNode
    {
        public string Title { get; set; }
        public ObservableCollection<LogNode> SubNodes { get; }

        public LogNode(string title)
        {
            Title = title;
            SubNodes = new ObservableCollection<LogNode>();
        }
    }
}