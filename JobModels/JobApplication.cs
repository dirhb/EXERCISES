using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class JobApplication
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public int EmployeeId { get; set; }
        public string ResumeSnapshot { get; set; } = string.Empty;
        public string SubmittedAtUTC { get; set; } = string.Empty;
        public string Status { get; set; } = "Submitted";
    }
}
