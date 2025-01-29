using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Model
{
    public class LogModel
    {
        private String Name { get; set; }
        private String SourceFile { get; set; }
        private String TargetFile { get; set; }
        private int FileSize { get; set; }
        private float FileTransferTime { get; set; }
        private DateTime Date { get; set; }

        public LogModel(String name, String sourceFile, String targetFile, int fileSize, float fileTransferTime, DateTime date)
        {
            Name = name;
            SourceFile = sourceFile;
            TargetFile = targetFile;
            FileSize = fileSize;
            FileTransferTime = fileTransferTime;
            Date = date;
        }
    }
}
