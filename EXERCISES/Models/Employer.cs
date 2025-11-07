using ModelLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXERCISES.Models
{
    public class Employer : Model
    {
        string employerID;
        string employerName;
        int employerPhoneNumber;
        string employerIndustry;

        public Employer(string employerID, string employerName, int employerPhoneNumber, string employerIndustry)
        {
            this.employerID = employerID;
            this.employerName = employerName;
            this.employerPhoneNumber = employerPhoneNumber;
            this.employerIndustry = employerIndustry;
        }
        public string EmployerID { get { return employerID; } set { employerID = value; ValidateProperty(value, "EmployerID"); } }


        [Required(ErrorMessage = "Employer Name is required")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Employer Name cannot be less than 2 characters")]
        public string EmployerName
        {
            get { return employerName; }
            set { employerName = value; ValidateProperty(value, "EmployerName"); }
        }

        [Required(ErrorMessage = "Employer Phone Number is required")]
        [Phone(ErrorMessage = "Phone Number is invalid")]
        public int EmployerPhoneNumber
        {
            get { return employerPhoneNumber; }
            set { employerPhoneNumber = value; ValidateProperty(value, "EmployerPhoneNumber"); }
        }

        [Required(ErrorMessage = "Employer Industry is required")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Employer Industry cannot be less than 2 characters")]
        public string EmployerIndustry
        {
            get { return employerIndustry; }
            set { employerIndustry = value; ValidateProperty(value, "EmployerIndustry"); }
        }
    }
}
