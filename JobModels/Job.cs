
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class Job
    {
        public string? JobID { get; set; }
        public string? JobTitle { get; set; }
        public string? JobDescription { get; set; }
        public string? JobType { get; set; }
        public bool? JobStatus { get; set; }
        public string? JobFilter { get; set; }
        public string? EmployerID { get; set; }
        public string? CountryID { get; set; }
        public string? GenreID { get; set; }
    }
}
