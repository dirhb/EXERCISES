using ModelLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXERCISES.Models
{
    public class Notification : Model
    {
        string notificationID;
        string notificationText;
        string notificationDate;

        public Notification(string notificationID, string notificationText, string notificationDate)
        {
            this.notificationID = notificationID;
            this.notificationText = notificationText;
            this.notificationDate = notificationDate;
        }
        public string NotificationID { get { return notificationID; } set { notificationID = value; ValidateProperty(value, "NotificationID"); } }


        [Required(ErrorMessage = "Notification Text is required")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Notification Text cannot be less than 4 characters")]
        public string NotificationText { get { return notificationText; } set { notificationText = value; ValidateProperty(value, "NotificationText"); } }
        public string NotificationDate
        { 
            get { return notificationDate; } 
            set { notificationDate = value; ValidateProperty(value, "NotificationDate"); }
        }
    }
}
