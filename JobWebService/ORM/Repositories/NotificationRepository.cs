using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class NotificationRepository : Repository, IRepository<Notification>
    {
        public NotificationRepository(DBHelperOledb helperOleDb) : base(helperOleDb) { }

        public bool Delete(int id)
        {
            string sql = $"DELETE FROM Notifications WHERE NotificationID=@NotificationID";
            this.helperOleDb.AddParameters("NotificationID", id.ToString());
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = $"DELETE FROM Notifications WHERE NotificationID=@NotificationID";
            this.helperOleDb.AddParameters("NotificationID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(Notification model)
        {
            if (model == null) return false;
            return Delete(model.NotificationID);
        }

        public bool Insert(Notification model)
        {
            string sql = $"INSERT INTO Notifications(NotificationID,NotificationText,NotificationDate) VALUES(@NotificationID,@NotificationText,@NotificationDate)";
            this.helperOleDb.AddParameters("NotificationID", model.NotificationID);
            this.helperOleDb.AddParameters("NotificationText", model.NotificationText);
            this.helperOleDb.AddParameters("NotificationDate", model.NotificationDate);
            return this.helperOleDb.Create(sql) > 0;
        }

        public Notification Read(object id)
        {
            string sql = $"SELECT * FROM Notifications WHERE NotificationID=@NotificationID";
            this.helperOleDb.AddParameters("NotificationID", id.ToString());
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.NotificationCreator.CreateModel(dataReader);
            }
        }

        public List<Notification> ReadAll()
        {
            List<Notification> list = new List<Notification>();
            string sql = "SELECT * FROM Notifications";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.NotificationCreator.CreateModel(dataReader));
            return list;
        }

        public object ReadValue()
        {
            throw new NotImplementedException();
        }

        public bool Update(Notification model)
        {
            string sql = "UPDATE Notifications SET NotificationText=@NotificationText,NotificationDate=@NotificationDate WHERE NotificationID=@NotificationID";
            this.helperOleDb.AddParameters("NotificationID", model.NotificationID);
            this.helperOleDb.AddParameters("NotificationText", model.NotificationText);
            this.helperOleDb.AddParameters("NotificationDate", model.NotificationDate);
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
