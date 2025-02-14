using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using EasySave_Project.Util;

namespace EasySave_Project.Dto
{
    public class LogDto
    {
        public string Name { get; set; }
        public List<LogDataDto> Logs { get; set; }
    }
    
    [XmlRoot("Logs")]
    public class LogsDto
    {
        [XmlElement("LogEntry")]
        public List<LogDataDto> Entries { get; set; }
    }

    [XmlRoot("LogDataDto")]
    public class LogDataDto
    {
        [JsonConverter(typeof(CustomDateTimeConverterUtil))]
        [XmlElement("Timestamp")]
        public DateTime Timestamp { get; set; }

        [XmlElement("JobName")]
        public string JobName { get; set; }

        [XmlElement("SourcePath")]
        public string SourcePath { get; set; }

        [XmlElement("TargetPath")]
        public string TargetPath { get; set; }

        [XmlElement("FileSize")]
        public long FileSize { get; set; }

        [XmlElement("TransferTime")]
        public double TransferTime { get; set; }

        [XmlElement("Messages")]
        public List<MessageDto> Messages { get; set; }

        [XmlElement("EncryptionTime")]
        public double EncryptionTime { get; set; }
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