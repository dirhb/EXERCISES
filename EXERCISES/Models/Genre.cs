using ModelLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXERCISES
{
    public class Genre : Model
    {
        string genreID;
        string genreTitle;
        string genreDescription;

        public Genre(string genreID, string genreTitle, string genreDescription)
        {
            this.genreID = genreID;
            this.genreTitle = genreTitle;
            this.genreDescription = genreDescription;
        }

        public string GenreID { get { return genreID; } set { genreID = value; ValidateProperty(value, "ReviewsID"); } }

        [Required(ErrorMessage = "Genre Title is required")]
        [StringLength(40, MinimumLength = 4, ErrorMessage = "Genre Title cannot be less than 4 characters")]
        public string GenreTitle { get { return genreTitle; } set { genreTitle = value; ValidateProperty(value, "GenreTitle"); } }

        [Required(ErrorMessage = "Genre Description is required")]
        [StringLength(40, MinimumLength = 10, ErrorMessage = "Genre Description cannot be less than 2 characters")]
        public string GenreDescription { get { return genreDescription; } set { genreDescription = value; ValidateProperty(value, "GenreDescription"); } }
    }
}
