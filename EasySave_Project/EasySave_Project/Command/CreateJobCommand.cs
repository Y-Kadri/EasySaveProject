using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave_Project.Manager;
using EasySave_Project.Model;

namespace EasySave_Project.Command
{
    public class CreateJobCommand : ICommand
    {
        public void Execute()
        {
            string fileSource = @"/tmp/source";
            string fileTarget = @"/tmp/target";

            JobManager jobMana = JobManager.GetInstance();
            jobMana.CreateAndAddJob("Backup Job", fileSource, fileTarget, JobSaveTypeEnum.COMPLETE);
            Console.WriteLine("la");
        }

        public void GetInstruction()
        {
            Console.WriteLine("Create Job Command");
        }
    }
}
