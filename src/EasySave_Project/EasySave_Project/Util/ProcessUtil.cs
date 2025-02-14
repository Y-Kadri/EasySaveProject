using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave_Project.Util
{
    public class ProcessUtil
    {
        //// <summary>
        /// Checks if any of the given process names are currently running.
        /// </summary>
        /// <param name="partialNames">One or multiple process names to check.</param>
        /// <returns>A tuple containing a boolean (isRunning) and the matching process name.</returns>
        public static (bool isRunning, string matchedProcess) IsProcessRunning(List<string> partialNames)
        {
            if (partialNames == null || partialNames.Count == 0)
                return (false, null);

            var runningProcesses = Process.GetProcesses().Select(p => p.ProcessName.ToLower()).ToList();
            foreach (var name in partialNames)
            {
                var matchedProcess = runningProcesses.FirstOrDefault(proc => proc.Contains(name.ToLower()));
                if (matchedProcess != null)
                    return (true, matchedProcess);
            }
            return (false, null);
        }
    }
}
