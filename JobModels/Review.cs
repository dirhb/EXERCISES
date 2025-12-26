
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class Review
    {
        public string? ReviewID { get; set; }
        public string? ReviewText { get; set; }
        public int? RatingTitle { get; set; }
    }
}
