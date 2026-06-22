using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class ReportRepository : Repository, IRepository<Report>
    {
        public ReportRepository(DBHelperOledb helperOleDb, ModelCreators modelcreators) : base(helperOleDb, modelcreators) { }

        public bool Insert(Report model)
        {
            string sql = "INSERT INTO [Report](ReporterUserID,TargetType,TargetID,Category,Subject,ReportText,Status,ReportDate) VALUES(@ReporterUserID,@TargetType,@TargetID,@Category,@Subject,@ReportText,@Status,@ReportDate)";
            this.helperOleDb.AddParameters("ReporterUserID", model.ReporterUserID);
            this.helperOleDb.AddParameters("TargetType", model.TargetType);
            this.helperOleDb.AddParameters("TargetID", model.TargetID);
            this.helperOleDb.AddParameters("Category", model.Category);
            this.helperOleDb.AddParameters("Subject", model.Subject);
            this.helperOleDb.AddParameters("ReportText", model.ReportText);
            this.helperOleDb.AddParameters("Status", model.Status);
            this.helperOleDb.AddParameters("ReportDate", model.ReportDate);
            return this.helperOleDb.Create(sql) > 0;
        }

        public List<Report> ReadAll()
        {
            List<Report> list = new List<Report>();
            // Newest first (ReportID is an AutoNumber = insertion order).
            string sql = "SELECT * FROM [Report] ORDER BY ReportID DESC";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.ReportCreator.CreateModel(dataReader));
            return list;
        }

        public Report Read(object id)
        {
            string sql = "SELECT * FROM [Report] WHERE ReportID=@ReportID";
            this.helperOleDb.AddParameters("ReportID", id.ToString());
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.ReportCreator.CreateModel(dataReader);
            }
        }

        public object ReadValue()
        {
            string sql = "SELECT COUNT(*) FROM [Report]";
            return this.helperOleDb.ReadValue(sql);
        }

        // Used by the admin to flip a report between Open / Resolved.
        public bool Update(Report model)
        {
            string sql = "UPDATE [Report] SET Status=@Status WHERE ReportID=@ReportID";
            this.helperOleDb.AddParameters("Status", model.Status);
            this.helperOleDb.AddParameters("ReportID", model.ReportID);
            return this.helperOleDb.Update(sql) > 0;
        }

        public bool Delete(int id)
        {
            return Delete(id.ToString());
        }

        public bool Delete(string id)
        {
            string sql = "DELETE FROM [Report] WHERE ReportID=@ReportID";
            this.helperOleDb.AddParameters("ReportID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(Report model)
        {
            if (model == null) return false;
            return Delete(model.ReportID);
        }
    }
}
