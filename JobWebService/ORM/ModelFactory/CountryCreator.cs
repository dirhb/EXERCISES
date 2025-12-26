using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class CountryCreator : IModelCreator<Country>
    {
        public Country CreateModel(IDataReader reader)
        {
            Country Country = new Country()
            {
                CountryID = Convert.ToString(reader["CountryID"]),
                CountryName = Convert.ToString(reader["CountryName"]),
            };
            return Country;
        }
    }
}
