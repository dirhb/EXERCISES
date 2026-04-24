using JobModels;
using JobWebService;
using JobWebService.ORM;
using JobWebService.ORM.Repositories;
using System.Data;

public class JobApplicationRepository : Repository, IRepository<JobApplication>
{
    public JobApplicationRepository(DBHelperOledb helperOleDb, ModelCreators modelcreators) : base(helperOleDb, modelcreators) { }

    public bool Insert(JobApplication model)
    {
        string sql = "INSERT INTO\r\n    JobApplications (JobID, UserID, ResumeText, Status, CreatedAt)\r\nVALUES\r\n    (@JobID, @UserID, @ResumeText, @Status, CreatedAt);";
        this.helperOleDb.AddParameters("@JobID", model.JobId);
        this.helperOleDb.AddParameters("@UserID", model.EmployeeId);
        this.helperOleDb.AddParameters("@ResumeText", model.ResumeSnapshot);
        this.helperOleDb.AddParameters("@Status", model.Status);
        this.helperOleDb.AddParameters("@CreatedAt", model.SubmittedAtUTC);
        return this.helperOleDb.Create(sql) > 0;
    }

    public bool Create(JobApplication model) => Insert(model);

    public JobApplication Read(object id)
    {
        string sql = "SELECT * FROM JobApplications WHERE ApplicationID=@ApplicationID";
        this.helperOleDb.AddParameters("ApplicationID", id.ToString());
        using (IDataReader dataReader = this.helperOleDb.Read(sql))
        {
            if (dataReader == null) return null;
            if (!dataReader.Read()) return null;
            return this.modelCreators.JobApplicationCreator.CreateModel(dataReader);
        }
    }

    public List<JobApplication> ReadAll()
    {
        List<JobApplication> list = new List<JobApplication>();
        string sql = "SELECT * FROM JobApplications";
        using (IDataReader dataReader = this.helperOleDb.Read(sql))
            while (dataReader.Read())
                list.Add(this.modelCreators.JobApplicationCreator.CreateModel(dataReader));
        return list;
    }

    public List<JobApplication> ReadByUserId(string userId)
    {
        List<JobApplication> list = new List<JobApplication>();
        string sql = "SELECT * FROM JobApplications WHERE UserID=@UserID ORDER BY CreatedAt DESC";
        this.helperOleDb.AddParameters("UserID", userId);
        using (IDataReader dataReader = this.helperOleDb.Read(sql))
            while (dataReader.Read())
                list.Add(this.modelCreators.JobApplicationCreator.CreateModel(dataReader));
        return list;
    }

    public List<JobApplication> ReadByJobId(string jobId)
    {
        List<JobApplication> list = new List<JobApplication>();
        string sql = "SELECT * FROM JobApplications WHERE JobID=@JobID ORDER BY CreatedAt DESC";
        this.helperOleDb.AddParameters("JobID", jobId);
        using (IDataReader dataReader = this.helperOleDb.Read(sql))
            while (dataReader.Read())
                list.Add(this.modelCreators.JobApplicationCreator.CreateModel(dataReader));
        return list;
    }

    public List<JobApplication> ReadByStatus(string status)
    {
        List<JobApplication> list = new List<JobApplication>();
        string sql = "SELECT * FROM JobApplications WHERE Status=@Status ORDER BY CreatedAt DESC";
        this.helperOleDb.AddParameters("Status", status);
        using (IDataReader dataReader = this.helperOleDb.Read(sql))
            while (dataReader.Read())
                list.Add(this.modelCreators.JobApplicationCreator.CreateModel(dataReader));
        return list;
    }

    public object ReadValue()
    {
        string sql = "SELECT COUNT(*) FROM JobApplications";
        return this.helperOleDb.ReadValue(sql);
    }

    public bool Update(JobApplication model)
    {
        string sql = "UPDATE JobApplications SET JobID=@JobID,UserID=@UserID,ResumeText=@ResumeText,Status=@Status,CreatedAt=@CreatedAt WHERE ApplicationID=@ApplicationID";
        this.helperOleDb.AddParameters("ApplicationID", model.ApplicationId);
        this.helperOleDb.AddParameters("JobID", model.JobId);
        this.helperOleDb.AddParameters("UserID", model.EmployeeId);
        this.helperOleDb.AddParameters("ResumeText", model.ResumeSnapshot);
        this.helperOleDb.AddParameters("Status", model.Status);
        this.helperOleDb.AddParameters("CreatedAt", model.SubmittedAtUTC);
        return this.helperOleDb.Update(sql) > 0;
    }

    public bool Delete(int id)
    {
        return Delete(id.ToString());
    }

    public bool Delete(string id)
    {
        string sql = "DELETE FROM JobApplications WHERE ApplicationID=@ApplicationID";
        this.helperOleDb.AddParameters("ApplicationID", id);
        return this.helperOleDb.Delete(sql) > 0;
    }

    public bool Delete(JobApplication model)
    {
        if (model == null) return false;
        return Delete(model.ApplicationId);
    }
}
