using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Dto
{
    public class FileInPendingJobDTO
    {
        public List<string> FilesInPending {  get; set; }
        public double Progress { get; set; }
        public string LastDateTimePath { get; set; }
        public int ProcessedFiles { get; set; }
        public long ProcessedSize { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
    }
}
