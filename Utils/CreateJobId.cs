using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreCommander.Utils
{
    internal class CreateJobId   
    {
        public static async Task<string> GenerateAsync()
        {
            string jobid = $"commander-job-{await SequenceGenerator.NextValue()}-{Guid.NewGuid():N}";
            await SimpleLogger.Log("JobId Created:" + jobid);
            return jobid;
        }
    }
}
