using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXERCISES
{
    public class RatingAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is byte rating && rating >= 1 && rating <= 10)
                return ValidationResult.Success;

            return new ValidationResult("The rating needs to be between 1-10");
        }
    }
    
}
