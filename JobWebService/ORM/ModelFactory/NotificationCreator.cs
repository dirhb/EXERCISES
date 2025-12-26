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
            };
            return Notification;
        }
    }
}
