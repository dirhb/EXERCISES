using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class EducationCreator : IModelCreator<Education>
    {
        public Education CreateModel(IDataReader reader)
        {
            Education Education = new Education()
            {
                UserID = Convert.ToString(reader["UserID"]),
                EducationTypeID = Convert.ToString(reader["EducationTypeID"]),
                GenreID = Convert.ToString(reader["GenreID"]),
            };
            return Education;
        }
    }
}
