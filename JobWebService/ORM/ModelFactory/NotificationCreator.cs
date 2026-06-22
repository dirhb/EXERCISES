using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class NotificationCreator : IModelCreator<Notification>
    {
        public Notification CreateModel(IDataReader reader)
        {
            Notification Notification = new Notification()
            {
                NotificationID = Convert.ToString(reader["NotificationID"]),
                NotificationText = Convert.ToString(reader["NotificationText"]),
                NotificationDate = Convert.ToString(reader["NotificationDate"]),
                RecipientUserID = HasColumn(reader, "RecipientUserID") && reader["RecipientUserID"] != DBNull.Value ? Convert.ToString(reader["RecipientUserID"]) : null,
            };
            return Notification;
        }

        private static bool HasColumn(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
