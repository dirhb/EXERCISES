using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    internal class JobType
    {
        [Required]
        public string JobTypeID { get; set; }
        public string JobTypeName { get; set; }
        public string JobTypeDescription { get; set; } 
    }
}
