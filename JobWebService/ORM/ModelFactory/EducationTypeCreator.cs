using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class EducationTypeCreator : IModelCreator<EducationType>
    {
        public EducationType CreateModel(IDataReader reader)
        {
            EducationType EducationType = new EducationType()
            {
                EducationTypeID = Convert.ToString(reader["EducationTypeId"]),
                EducationLevel = Convert.ToString(reader["EducationLevel"]),
            };
            return EducationType;
        }
    }
}
