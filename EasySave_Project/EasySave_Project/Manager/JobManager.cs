using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave_Project.Model;

namespace EasySave_Project.Manager
{
    public sealed class JobManager
    {
        private static readonly JobManager _instance = new();

        public static List<JobModel> Jobs { get; private set; }


        private JobManager()
        {
            Jobs = new List<JobModel>();
        }
        public static JobManager Instance
        {
            get{return _instance;}
        }
    }
}
