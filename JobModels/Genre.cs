using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class Genre
    {
        public string? GenreID { get; set; }
        public string? GenreTitle { get; set; }
        public string? GenreDescription { get; set; }
    }
}
