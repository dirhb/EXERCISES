using ModelLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXERCISES.Models
{
    public class Employee : Model
    {
        string employeeID;
        string employeeFullName;
        int employeePhoneNumber;
        string employeeEducation;

        public Employee()
        {
        }

        public Employee(string employeeID, string employeeFullName, int employeePhoneNumber, string employeeEducation)
        {
            this.employeeID = employeeID;
            this.employeeFullName = employeeFullName;
            this.employeePhoneNumber = employeePhoneNumber;
            this.employeeEducation = employeeEducation;
        }

        public string EmployeeID { get { return employeeID; } set { employeeID = value; } }

        [Required(ErrorMessage = "Employee Full Name is required")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Employee Full Name cannot be less than 2 characters")]
        public string EmployeeFullName
        { 
            get { return employeeFullName; } 
            set { employeeFullName = value; ValidateProperty(value, "EmployeeFullName"); }
        }

        [Required(ErrorMessage = "Employee Phone Number is required")]
        [Phone(ErrorMessage = "Phone Number is invalid")]
        public int EmployeePhoneNumber
        {
            get { return employeePhoneNumber; }
            set { employeePhoneNumber = value; ValidateProperty(value, "EmployeePhoneNumber"); }
        }

        [Required(ErrorMessage = "Employee Education is required")]
        [StringLength(25, MinimumLength = 5, ErrorMessage = "Employee Education cannot be less than 2 characters")]
        public string EmployeeEducation
        {
            get { return employeeEducation; }
            set { employeeEducation = value; ValidateProperty(value, "EmployeeEducation"); }
        }
    }
}
