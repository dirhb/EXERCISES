using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class JobApplicationCreator : IModelCreator<JobApplication>
    {
        public JobApplication CreateModel(IDataReader reader)
        {
            JobApplication jobApplication = new JobApplication()
            {
                ApplicationId = Convert.ToString(reader["ApplicationID"]),
                JobId = Convert.ToString(reader["JobID"]),
                EmployeeId = Convert.ToString(reader["UserID"]),
                ResumeSnapshot = Convert.ToString(reader["ResumeText"]),
                Status = Convert.ToString(reader["Status"]),
                SubmittedAtUTC = Convert.ToDateTime(reader["CreatedAt"]),
            };
            return jobApplication;
        }
    }
}
