using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXERCISES.ViewModels
{
    public class JobTrack
    {
        public List<Jobs> Jobs { get; set; }
        public string? JobID { get; set; }
        public int PageNumber { get; set; }
        public int Pages { get; set; }
        public List<Genre> Genres { get; set; }
        public string? GenreID { get; set; }
    }
}
