using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class ReviewCreator : IModelCreator<Review>
    {
        public Review CreateModel(IDataReader reader)
        {
            return new Review()
            {
                ReviewID = Convert.ToString(reader["ReviewID"]),
                // NB: the text column is named "ReviewsText" in the Access table.
                ReviewText = reader["ReviewsText"] == DBNull.Value ? null : Convert.ToString(reader["ReviewsText"]),
                RatingTitle = reader["RatingTitle"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["RatingTitle"]),
                UserID = HasColumn(reader, "UserID") && reader["UserID"] != DBNull.Value ? Convert.ToString(reader["UserID"]) : null,
                EmployerID = HasColumn(reader, "EmployerID") && reader["EmployerID"] != DBNull.Value ? Convert.ToString(reader["EmployerID"]) : null,
                ReviewDate = HasColumn(reader, "ReviewDate") && reader["ReviewDate"] != DBNull.Value ? Convert.ToString(reader["ReviewDate"]) : null,
            };
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
