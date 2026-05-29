using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels.ViewModels
{
    public class PostJobViewModel
    {
        public List<Genre> Genres { get; set; }
        public List<JobType> JobTypes { get; set; }
    }
}

