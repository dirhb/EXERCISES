using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class JobFilters
    {
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public decimal? MinimumSalary { get; set; }
        public decimal? MaximumSalary { get; set; }
        public int? ExperienceLevel { get; set; }
        public string? Flexibility { get; set; }
    }
}
