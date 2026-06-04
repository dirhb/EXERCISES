using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobModels
{
    public class ChatMessage
    {
        public int MessageID { get; set; }
        public string? SenderID { get; set; }
        public string? ReceiverID { get; set; }
        public string? MessageText { get; set; }
        public string? SentAt { get; set; }
    }
}
