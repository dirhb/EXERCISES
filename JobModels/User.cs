using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class User
    {
        public string? UserID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Password { get; set; }
        public string? Country { get; set; }
        public string? PhoneNum { get; set; }
        public string? CreationDate { get; set; }
        public int? UserTypeID { get; set; }
        public string? Email { get; set; }
    }
}
