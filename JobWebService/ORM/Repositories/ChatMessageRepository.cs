using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class ChatMessageRepository : Repository, IRepository<ChatMessage>
    {
        public ChatMessageRepository(DBHelperOledb helperOleDb, ModelCreators modelcreators) : base(helperOleDb, modelcreators) { }

        public bool Insert(ChatMessage model)
        {
            string sql = "INSERT INTO ChatMessages (SenderID, ReceiverID, MessageText, SentAt) VALUES (@SenderID, @ReceiverID, @MessageText, @SentAt)";
            this.helperOleDb.AddParameters("@SenderID", model.SenderID);
            this.helperOleDb.AddParameters("@ReceiverID", model.ReceiverID);
            this.helperOleDb.AddParameters("@MessageText", model.MessageText);
            this.helperOleDb.AddParameters("@SentAt", model.SentAt);
            return this.helperOleDb.Create(sql) > 0;
        }

        public bool Create(ChatMessage model) => Insert(model);

        public List<ChatMessage> ReadAll()
        {
            List<ChatMessage> list = new List<ChatMessage>();
            string sql = "SELECT * FROM ChatMessages ORDER BY MessageID ASC";
            using (IDataReader dr = this.helperOleDb.Read(sql))
            {
                while (dr.Read())
                {
                    list.Add(this.modelCreators.ChatMessageCreator.CreateModel(dr));
                }
            }
            return list;
        }

        public ChatMessage Read(object id)
        {
            string sql = "SELECT * FROM ChatMessages WHERE MessageID=@MessageID";
            this.helperOleDb.AddParameters("MessageID", id.ToString());
            using (IDataReader dr = this.helperOleDb.Read(sql))
            {
                if (dr == null || !dr.Read()) return null;
                return this.modelCreators.ChatMessageCreator.CreateModel(dr);
            }
        }

        public object ReadValue()
        {
            string sql = "SELECT COUNT(*) FROM ChatMessages";
            return this.helperOleDb.ReadValue(sql);
        }

        public bool Update(ChatMessage model)
        {
            string sql = "UPDATE ChatMessages SET SenderID=@SenderID, ReceiverID=@ReceiverID, MessageText=@MessageText, SentAt=@SentAt WHERE MessageID=@MessageID";
            this.helperOleDb.AddParameters("@SenderID", model.SenderID);
            this.helperOleDb.AddParameters("@ReceiverID", model.ReceiverID);
            this.helperOleDb.AddParameters("@MessageText", model.MessageText);
            this.helperOleDb.AddParameters("@SentAt", model.SentAt);
            this.helperOleDb.AddParameters("@MessageID", model.MessageID);
            return this.helperOleDb.Update(sql) > 0;
        }

        public bool Delete(int id) => Delete(id.ToString());

        public bool Delete(string id)
        {
            string sql = "DELETE FROM ChatMessages WHERE MessageID=@MessageID";
            this.helperOleDb.AddParameters("MessageID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(ChatMessage model)
        {
            if (model == null) return false;
            return Delete(model.MessageID);
        }

        public List<ChatMessage> ReadHistory(string user1, string user2)
        {
            List<ChatMessage> list = new List<ChatMessage>();
            // Access uses ? positional placeholders in OleDb, but named parameters work with our helper because it clears and registers correctly.
            // Let's query messages between the two users.
            string sql = "SELECT * FROM ChatMessages WHERE (SenderID=@Sender1 AND ReceiverID=@Receiver1) OR (SenderID=@Sender2 AND ReceiverID=@Receiver2) ORDER BY MessageID ASC";
            this.helperOleDb.AddParameters("@Sender1", user1);
            this.helperOleDb.AddParameters("@Receiver1", user2);
            this.helperOleDb.AddParameters("@Sender2", user2);
            this.helperOleDb.AddParameters("@Receiver2", user1);

            using (IDataReader dr = this.helperOleDb.Read(sql))
            {
                while (dr.Read())
                {
                    list.Add(this.modelCreators.ChatMessageCreator.CreateModel(dr));
                }
            }
            return list;
        }

        public List<string> ReadConversationPartnerIds(string userId)
        {
            List<string> partnerIds = new List<string>();
            string sql = "SELECT DISTINCT SenderID AS PartnerID FROM ChatMessages WHERE ReceiverID=@UserID1 UNION SELECT DISTINCT ReceiverID AS PartnerID FROM ChatMessages WHERE SenderID=@UserID2";
            this.helperOleDb.AddParameters("@UserID1", userId);
            this.helperOleDb.AddParameters("@UserID2", userId);

            using (IDataReader dr = this.helperOleDb.Read(sql))
            {
                while (dr.Read())
                {
                    string id = Convert.ToString(dr["PartnerID"]);
                    if (!string.IsNullOrEmpty(id) && id != userId)
                    {
                        partnerIds.Add(id);
                    }
                }
            }
            return partnerIds;
        }
    }
}
