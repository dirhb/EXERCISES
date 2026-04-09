using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class ManagedEmployee
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string Resume { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
