using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class JobRepository : Repository, IRepository<Job>
    {
        public JobRepository(DBHelperOledb helperOleDb, ModelCreators modelcreators) : base(helperOleDb, modelcreators) { }

        public bool Delete(int id)
        {
            // Delegate to the string overload so child rows are cleared first
            // (referential integrity) regardless of which overload is called.
            return Delete(id.ToString());
        }

        // Alias for Insert to match other repositories naming
        public bool Create(Job model)
        {
            return Insert(model);
        }

        public bool Delete(string id)
        {
            // A Job is referenced by foreign keys in JobApplications, JobGenre
            // and Work. Access enforces referential integrity, so deleting the
            // Job directly fails whenever any of those child rows exist (this is
            // why admin "Delete job" silently failed). Remove the child rows
            // first, then the Job itself.
            DeleteChildren("DELETE FROM JobApplications WHERE JobID=@JobID", id);
            DeleteChildren("DELETE FROM JobGenre WHERE JobID=@JobID", id);
            // [Work] is bracketed because WORK is a reserved word in Access SQL.
            DeleteChildren("DELETE FROM [Work] WHERE JobID=@JobID", id);

            string sql = $"DELETE FROM Job WHERE JobID=@JobID";
            this.helperOleDb.AddParameters("JobID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        // Deletes rows in a child table that point at this JobID.
        private void DeleteChildren(string sql, string jobId)
        {
            this.helperOleDb.AddParameters("JobID", jobId);
            this.helperOleDb.Delete(sql);
        }

        public bool Delete(Job model)
        {
            if (model == null) return false;
            return Delete(model.JobID);
        }

        public bool Insert(Job model)
        {
            string sql = @"INSERT INTO Job(JobTitle,JobDescription,JobType,
                            JobStatus,JobFilter,EmployerID,CountryID,GenreID,Salary)
                            VALUES(@JobTitle,@JobDescription,@JobType,@JobStatus,@JobFilter,
                            @EmployerID,@CountryID,@GenreID,@Salary)";

            this.helperOleDb.AddParameters("@JobTitle", model.JobTitle);
            this.helperOleDb.AddParameters("@JobDescription", model.JobDescription);
            this.helperOleDb.AddParameters("@JobType", model.JobType);
            this.helperOleDb.AddParameters("@JobStatus", model.JobStatus.HasValue ? (model.JobStatus.Value ? "1" : "0") : null);
            this.helperOleDb.AddParameters("@JobFilter", model.JobFilter);
            this.helperOleDb.AddParameters("@EmployerID", model.EmployerID);
            this.helperOleDb.AddParameters("@CountryID", model.CountryID);
            this.helperOleDb.AddParameters("@GenreID", model.GenreID);
            this.helperOleDb.AddParameters("@Salary", model.Salary);
            return this.helperOleDb.Create(sql) > 0;
        }

        public Job Read(object id)
        {
            string sql = "SELECT Job.*, Country.CountryName, Genre.GenreTitle, JT.JobTypeName, (U.FirstName & ' ' & U.LastName) AS EmployerName FROM (((Job LEFT JOIN Country ON Job.CountryID = Country.CountryID) LEFT JOIN Genre ON Job.GenreID = Genre.GenreID) LEFT JOIN JobType AS JT ON Job.JobType = JT.JobTypeID) LEFT JOIN [User] AS U ON Job.EmployerID = U.UserID WHERE Job.JobID=@JobID";
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
            string sql = "SELECT Job.*, Country.CountryName, Genre.GenreTitle, JT.JobTypeName, (U.FirstName & ' ' & U.LastName) AS EmployerName FROM (((Job LEFT JOIN Country ON Job.CountryID = Country.CountryID) LEFT JOIN Genre ON Job.GenreID = Genre.GenreID) LEFT JOIN JobType AS JT ON Job.JobType = JT.JobTypeID) LEFT JOIN [User] AS U ON Job.EmployerID = U.UserID";
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

        // Returns only the jobs posted by a specific employer
        public List<Job> ReadByEmployer(string employerId)
        {
            List<Job> list = new List<Job>();
            string sql = "SELECT Job.*, Country.CountryName, Genre.GenreTitle, JT.JobTypeName, (U.FirstName & ' ' & U.LastName) AS EmployerName FROM (((Job LEFT JOIN Country ON Job.CountryID = Country.CountryID) LEFT JOIN Genre ON Job.GenreID = Genre.GenreID) LEFT JOIN JobType AS JT ON Job.JobType = JT.JobTypeID) LEFT JOIN [User] AS U ON Job.EmployerID = U.UserID WHERE Job.EmployerID=@EmployerID";
            this.helperOleDb.AddParameters("@EmployerID", employerId);
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.JobCreator.CreateModel(dataReader));
            return list;
        }

        public object ReadValue()
        {
            string sql = "SELECT COUNT(*) FROM Job";
            return this.helperOleDb.ReadValue(sql);
        }

        public bool Update(Job model)
        {
            string sql = "UPDATE Job SET JobTitle=@JobTitle,JobDescription=@JobDescription,JobType=@JobType,JobStatus=@JobStatus,JobFilter=@JobFilter,EmployerID=@EmployerID,CountryID=@CountryID,GenreID=@GenreID,Salary=@Salary WHERE JobID=@JobID";
            this.helperOleDb.AddParameters("JobTitle", model.JobTitle);
            this.helperOleDb.AddParameters("JobDescription", model.JobDescription);
            this.helperOleDb.AddParameters("JobType", model.JobType);
            this.helperOleDb.AddParameters("JobStatus", model.JobStatus.HasValue ? (model.JobStatus.Value ? "1" : "0") : null);
            this.helperOleDb.AddParameters("JobFilter", model.JobFilter);
            this.helperOleDb.AddParameters("EmployerID", model.EmployerID);
            this.helperOleDb.AddParameters("CountryID", model.CountryID);
            this.helperOleDb.AddParameters("GenreID", model.GenreID);
            this.helperOleDb.AddParameters("Salary", model.Salary);
            this.helperOleDb.AddParameters("JobID", model.JobID);
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
