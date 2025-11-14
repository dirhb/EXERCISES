using ModelLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXERCISES.Models
{
    public class Reviews : Model
    {
        string reviewsID;
        string reviewsText;
        int reviewsRating;
        public Reviews(string ReviewsID, string ReviewsText, int ReviewsRating)
        {
            this.reviewsID = ReviewsID;
            this.reviewsText = ReviewsText;
            this.reviewsRating = ReviewsRating;
        }
        public string ReviewsID { get { return reviewsID; } set { reviewsID = value; ValidateProperty(value, "ReviewsID"); } }

        [Required(ErrorMessage = "Reviews Text is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Reviews Text cannot be less than 1 characters")]
        public string ReviewsText { get { return reviewsText; } set { reviewsText = value; ValidateProperty(value, "ReviewsText"); } }

        [Required(ErrorMessage = "Rating Number is needed")]
        [Rating]
        public int ReviewsRating { get { return reviewsRating; } set { reviewsRating = value; ValidateProperty(value, "ReviewsRating"); } }
    }
}
