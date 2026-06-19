using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class JobTypeCreator : IModelCreator<JobType>
    {
        public JobType CreateModel(IDataReader reader)
        {
            JobType jobType = new JobType()
            {
                JobTypeID = Convert.ToString(reader["JobTypeID"]),
                JobTypeName = Convert.ToString(reader["JobTypeName"]),
                JobTypeDescription = Convert.ToString(reader["JobTypeDescription"]),
            };
            return jobType;
        }
    }
}
