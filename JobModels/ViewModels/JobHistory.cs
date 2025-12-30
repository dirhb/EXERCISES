using JobModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels.ViewModels
{
    public class JobHistory
    {
        public List<Genre> Genres { get; set; }
        public string? GenreID { get; set; }
        // Employer type not found in JobModels; keep as plain strings or remove
        public List<string> Employers { get; set; }
        public string? EmployerID { get; set; }
        public List<Job> JobHistoryList { get; set; }
        public string? JobID { get; set; }
        public int PageNumber { get; set; }
        public int Pages { get; set; }
    }
}
