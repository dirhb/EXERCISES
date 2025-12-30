using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class CountryCreator : IModelCreator<JobModels.Country>
    {
        public Country CreateModel(IDataReader reader)
        {
            Country country = new Country
            {
                CountryID = Convert.ToString(reader["CountryID"]),
                CountryName = Convert.ToString(reader["CountryName"]),
            };
            return country;
        }
    }
}
