using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class JobTypeRepository : Repository, IRepository<JobType>
    {
        public JobTypeRepository(DBHelperOledb helperOleDb, ModelCreators modelcreators) : base(helperOleDb, modelcreators) { }

        public bool Delete(int id)
        {
            string sql = $"DELETE FROM JobType WHERE JobTypeID=@JobTypeID";
            this.helperOleDb.AddParameters("JobTypeID", id.ToString());
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = $"DELETE FROM JobType WHERE JobTypeID=@JobTypeID";
            this.helperOleDb.AddParameters("JobTypeID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(JobType model)
        {
            if (model == null) return false;
            return Delete(model.JobTypeID);
        }

        public bool Insert(JobType model)
        {
            string sql = $"INSERT INTO JobType(JobTypeName,JobTypeDescription) VALUES(@JobTypeName,@JobTypeDescription)";
            this.helperOleDb.AddParameters("JobTypeName", model.JobTypeName);
            this.helperOleDb.AddParameters("JobTypeDescription", model.JobTypeDescription);
            return this.helperOleDb.Create(sql) > 0;
        }

        public JobType Read(object id)
        {
            string sql = $"SELECT * FROM JobType WHERE JobTypeID=@JobTypeID";
            this.helperOleDb.AddParameters("JobTypeID", id.ToString());
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.JobTypeCreator.CreateModel(dataReader);
            }
        }

        public List<JobType> ReadAll()
        {
            List<JobType> list = new List<JobType>();
            string sql = "SELECT * FROM JobType";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.JobTypeCreator.CreateModel(dataReader));
            return list;
        }

        public object ReadValue()
        {
            string sql = "SELECT COUNT(*) FROM JobType";
            return this.helperOleDb.ReadValue(sql);
        }

        public bool Update(JobType model)
        {
            string sql = "UPDATE JobType SET JobTypeName=@JobTypeName,JobTypeDescription=@JobTypeDescription WHERE JobTypeID=@JobTypeID";
            this.helperOleDb.AddParameters("JobTypeName", model.JobTypeName);
            this.helperOleDb.AddParameters("JobTypeDescription", model.JobTypeDescription);
            this.helperOleDb.AddParameters("JobTypeID", model.JobTypeID);
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
