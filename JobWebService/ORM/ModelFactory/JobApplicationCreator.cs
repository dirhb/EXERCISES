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
                ApplicationId = Convert.ToInt16(reader["ApplicationID"]),
                JobId = Convert.ToInt16(reader["JobID"]),
                EmployeeId = Convert.ToInt16(reader["UserID"]),
                ResumeSnapshot = Convert.ToString(reader["ResumeText"]),
                Status = Convert.ToString(reader["Status"]),
                SubmittedAtUTC = Convert.ToString(reader["CreatedAt"]),
            };
            return jobApplication;
        }
    }
}
