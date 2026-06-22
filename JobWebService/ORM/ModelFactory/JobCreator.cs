using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class JobCreator : IModelCreator<Job>
    {
        public Job CreateModel(IDataReader reader)
        {
            Job Job = new Job()
            {
                JobID = Convert.ToString(reader["JobID"]),
                JobTitle = Convert.ToString(reader["JobTitle"]),
                JobDescription = Convert.ToString(reader["JobDescription"]),
                JobType = Convert.ToString(reader["JobType"]),
                JobStatus = Convert.ToBoolean(reader["JobStatus"]),
                JobFilter = Convert.ToString(reader["JobFilter"]),
                EmployerID = Convert.ToString(reader["EmployerID"]),
                CountryID = Convert.ToString(reader["CountryID"]),
                GenreID = Convert.ToString(reader["GenreID"]),
                Salary = HasColumn(reader, "Salary") && reader["Salary"] != DBNull.Value ? Convert.ToDecimal(reader["Salary"]) : (decimal?)null,
                CountryName = HasColumn(reader, "CountryName") && reader["CountryName"] != DBNull.Value ? Convert.ToString(reader["CountryName"]) : null,
                GenreTitle = HasColumn(reader, "GenreTitle") && reader["GenreTitle"] != DBNull.Value ? Convert.ToString(reader["GenreTitle"]) : null,
                JobTypeName = HasColumn(reader, "JobTypeName") && reader["JobTypeName"] != DBNull.Value ? Convert.ToString(reader["JobTypeName"]) : null,
                EmployerName = HasColumn(reader, "EmployerName") && reader["EmployerName"] != DBNull.Value ? BlankToNull(Convert.ToString(reader["EmployerName"])) : null,
            };
            return Job;
        }

        // Employer name is built in SQL as "First & ' ' & Last"; if the employer
        // had no name it comes back as just a space — treat that as no name.
        private static string? BlankToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
