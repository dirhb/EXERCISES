
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class Notification
    {
        public string? NotificationID {  get; set; }
        public string? NotificationText { get; set; }
        public string? NotificationDate { get; set; }
    }
}
