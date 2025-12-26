using JobModels;
using System;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class ReviewCreator : IModelCreator<Review>
    {
        public Review CreateModel(IDataReader reader)
        {
            Review Review = new Review()
            {
                ReviewID = Convert.ToString(reader["ReviewId"]),
                ReviewText = Convert.ToString(reader["ReviewText"]),
                RatingTitle = Convert.ToInt16(reader["RatingTitle"]),
            };
            return Review;
        }
    }
}