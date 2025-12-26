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
            };
            return Job;
        }
    }
}
