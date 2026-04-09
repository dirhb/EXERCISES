using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class ManagedJob
    {
        public string JobId { get; set; } = Guid.NewGuid().ToString("N");
        public string EmployerId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public int ExperienceNeeded { get; set; }
        public string Location { get; set; } = string.Empty;
        public string BenefitsBonuses { get; set; } = string.Empty;
        public string Flexibility { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}