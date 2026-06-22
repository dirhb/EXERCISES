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
                OfferedSalary = HasColumn(reader, "OfferedSalary") && reader["OfferedSalary"] != DBNull.Value ? Convert.ToDecimal(reader["OfferedSalary"]) : (decimal?)null,
            };
            return jobApplication;
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
