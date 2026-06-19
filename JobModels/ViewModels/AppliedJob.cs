using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels.ViewModels
{
    // Pairs a job the employee applied to with the status and date of
    // that application, so the Job History page can show both together.
    public class AppliedJob
    {
        public Job Job { get; set; }
        public string Status { get; set; }
        public string AppliedAt { get; set; }
    }
}
