using JobModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels.ViewModels
{
    public class HomePage
    {
        public List<Job>? JobMatches { get; set; }
        public List<Job>? PopularJobs { get; set; }
        public List<Review>? RecentReviews { get; set; }
        

    }
}
