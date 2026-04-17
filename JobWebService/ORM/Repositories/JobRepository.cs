using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class JobRepository : Repository, IRepository<Job>
    {
        public JobRepository(DBHelperOledb helperOleDb, ModelCreators modelcreators) : base(helperOleDb, modelcreators) { }

        public bool Delete(int id)
        {
            string sql = $"DELETE FROM Job WHERE JobID=@JobID";
            this.helperOleDb.AddParameters("JobID", id.ToString());
            return this.helperOleDb.Delete(sql) > 0;
        }

        // Alias for Insert to match other repositories naming
        public bool Create(Job model)
        {
            return Insert(model);
        }

        public bool Delete(string id)
        {
            string sql = $"DELETE FROM Job WHERE JobID=@JobID";
            this.helperOleDb.AddParameters("JobID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(Job model)
        {
            if (model == null) return false;
            return Delete(model.JobID);
        }

        public bool Insert(Job model)
        {
            string sql = $@"INSERT INTO Job(JobTitle,JobDescription,JobType,
                            JobStatus,JobFilter,EmployerID,CountryID,GenreID) 
                            VALUES(@JobTitle,@JobDescription,@JobType,@JobStatus,@JobFilter,
                            @EmployerID,@CountryID,@GenreID)";
           
            this.helperOleDb.AddParameters("@JobTitle", model.JobTitle);
            this.helperOleDb.AddParameters("@JobDescription", model.JobDescription);
            this.helperOleDb.AddParameters("@JobType", model.JobType);
            this.helperOleDb.AddParameters("@JobStatus", model.JobStatus.HasValue ? (model.JobStatus.Value ? "1" : "0") : null);
            this.helperOleDb.AddParameters("@JobFilter", model.JobFilter);
            this.helperOleDb.AddParameters("@EmployerID", model.EmployerID);
            this.helperOleDb.AddParameters("@CountryID", model.CountryID);
            this.helperOleDb.AddParameters("@GenreID", model.GenreID);
            return this.helperOleDb.Create(sql) > 0;
        }

        public Job Read(object id)
        {
            string sql = $"SELECT * FROM Job WHERE JobID=@JobID";
            this.helperOleDb.AddParameters("JobID", id.ToString());
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.JobCreator.CreateModel(dataReader);
            }
        }

        public List<Job> ReadAll()
        {
            List<Job> list = new List<Job>();
            string sql = "SELECT * FROM Job";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.JobCreator.CreateModel(dataReader));
            return list;
        }

        // Optional paging implementation
        public List<Job> GetPaged(int pageNumber, int pageSize)
        {
            List<Job> jobs = new List<Job>();
            int offset = (pageNumber - 1) * pageSize;
            string sql = $"SELECT * FROM Job ORDER BY JobID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            this.helperOleDb.AddParameters("Offset", offset.ToString());
            this.helperOleDb.AddParameters("PageSize", pageSize.ToString());
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    jobs.Add(this.modelCreators.JobCreator.CreateModel(dataReader));
            return jobs;
        }

        public object ReadValue()
        {
            string sql = "SELECT COUNT(*) FROM Job";
            return this.helperOleDb.ReadValue(sql);
        }

        public bool Update(Job model)
        {
            string sql = "UPDATE Job SET JobTitle=@JobTitle,JobDescription=@JobDescription,JobType=@JobType,JobStatus=@JobStatus,JobFilter=@JobFilter,EmployerID=@EmployerID,CountryID=@CountryID,GenreID=@GenreID WHERE JobID=@JobID";
            this.helperOleDb.AddParameters("JobID", model.JobID);
            this.helperOleDb.AddParameters("JobTitle", model.JobTitle);
            this.helperOleDb.AddParameters("JobDescription", model.JobDescription);
            this.helperOleDb.AddParameters("JobType", model.JobType);
            this.helperOleDb.AddParameters("JobStatus", model.JobStatus.HasValue ? (model.JobStatus.Value ? "1" : "0") : null);
            this.helperOleDb.AddParameters("JobFilter", model.JobFilter);
            this.helperOleDb.AddParameters("EmployerID", model.EmployerID);
            this.helperOleDb.AddParameters("CountryID", model.CountryID);
            this.helperOleDb.AddParameters("GenreID", model.GenreID);
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
