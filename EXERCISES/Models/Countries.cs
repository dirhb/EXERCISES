using ModelLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EXERCISES.Models
{
    public class Countries : Model
    {
        string countryID;
        string countryName;

        public Countries(string countryID, string countryName)
        {
            this.countryID = countryID;
            this.countryName = countryName;
        }

        public string CountryID
        {
            get { return this.countryID; }
            set
            {
                countryID = value;
            }
        }


        [Required(ErrorMessage = "Country Name is required")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Country Name cannot be less than 5 characters")]
        public string CountryName
        {
            get { return this.countryName; }
            set { this.countryName = value; ValidateProperty(value, "CountryName"); }
        }
    }
}
