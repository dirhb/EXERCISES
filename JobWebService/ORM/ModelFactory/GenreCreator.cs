using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class GenreCreator : IModelCreator<Genre>
    {
        public Genre CreateModel(IDataReader reader)
        {
            Genre Genre = new Genre()
            {
                GenreID = Convert.ToString(reader["GenreID"]),
                GenreTitle = Convert.ToString(reader["GenreTitle"]),
                GenreDescription = Convert.ToString(reader["GenreDescription"]),
            };
            return Genre;
        }
    }
}
