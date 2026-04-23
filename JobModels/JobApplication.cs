using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class JobApplication
    {
        public string ApplicationId { get; set; } = Guid.NewGuid().ToString("N");
        public string JobId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string ResumeSnapshot { get; set; } = string.Empty;
        public DateTime SubmittedAtUTC { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Submitted";
    }
}
