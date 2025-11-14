using EXERCISES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXERCISES.ViewModels
{
    public class HomePage
    {
        public List<Jobs>? JobMatches { get; set; }
        public List<Jobs>? PopularJobs { get; set; }
        public List<Reviews>? RecentReviews { get; set; }
        

    }
}
