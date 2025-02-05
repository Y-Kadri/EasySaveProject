using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasySave_Project.Model
{
    public class StateModel

    {
        private String Name { get; set; }
        private String SourceFilePath { get; set; }
        private String TargetFilePath { get; set; }

        private JobSaveStateEnum State { get; set; }
        private int TotalFileSizeCopy { get; set; }
        private int TotalFileSize { get; set; }
        private int NbFilesLeftToDo { get; set; }

        private int Progression { get; set; }

        public StateModel(String name, String sourceFilePath, String targetFilePath, JobSaveStateEnum state, int totalFileSizeCopy, int totalFileSize, int nbFilesLeftToDo, int progression)
        {
            Name = name;
            SourceFilePath = sourceFilePath;
            TargetFilePath = targetFilePath;
            State = state;
            TotalFileSizeCopy = totalFileSizeCopy;
            TotalFileSize = totalFileSize;
            NbFilesLeftToDo = nbFilesLeftToDo;
            Progression = progression;
        }
    }
}
