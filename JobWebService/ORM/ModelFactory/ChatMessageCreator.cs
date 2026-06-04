using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class ChatMessageCreator : IModelCreator<ChatMessage>
    {
        public ChatMessage CreateModel(IDataReader reader)
        {
            return new ChatMessage()
            {
                MessageID = Convert.ToInt32(reader["MessageID"]),
                SenderID = Convert.ToString(reader["SenderID"]),
                ReceiverID = Convert.ToString(reader["ReceiverID"]),
                MessageText = Convert.ToString(reader["MessageText"]),
                SentAt = Convert.ToString(reader["SentAt"]),
            };
        }
    }
}
