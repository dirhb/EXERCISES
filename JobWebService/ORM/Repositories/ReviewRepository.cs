using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class ReviewRepository : Repository, IRepository<Review>
    {
        public ReviewRepository(DBHelperOledb helperOleDb) : base(helperOleDb) { }

        public bool Delete(int id)
        {
            string sql = $"DELETE FROM Reviews WHERE ReviewID=@ReviewID";
            this.helperOleDb.AddParameters("ReviewID", id.ToString());
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = $"DELETE FROM Reviews WHERE ReviewID=@ReviewID";
            this.helperOleDb.AddParameters("ReviewID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(Review model)
        {
            if (model == null) return false;
            return Delete(model.ReviewID);
        }

        public bool Insert(Review model)
        {
            string sql = $"INSERT INTO Reviews(ReviewID,ReviewText,RatingTitle) VALUES(@ReviewID,@ReviewText,@RatingTitle)";
            this.helperOleDb.AddParameters("ReviewID", model.ReviewID);
            this.helperOleDb.AddParameters("ReviewText", model.ReviewText);
            this.helperOleDb.AddParameters("RatingTitle", model.RatingTitle.HasValue ? model.RatingTitle.Value.ToString() : null);
            return this.helperOleDb.Create(sql) > 0;
        }

        // Alias for Insert
        public bool Create(Review model)
        {
            return Insert(model);
        }

        public Review Read(object id)
        {
            string sql = $"SELECT * FROM Reviews WHERE ReviewID=@ReviewID";
            this.helperOleDb.AddParameters("ReviewID", id.ToString());
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.ReviewsCreator.CreateModel(dataReader);
            }
        }

        public List<Review> ReadAll()
        {
            List<Review> list = new List<Review>();
            string sql = "SELECT * FROM Reviews";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.ReviewsCreator.CreateModel(dataReader));
            return list;
        }

        public object ReadValue()
        {
            throw new NotImplementedException();
        }

        public bool Update(Review model)
        {
            string sql = "UPDATE Reviews SET ReviewText=@ReviewText,RatingTitle=@RatingTitle WHERE ReviewID=@ReviewID";
            this.helperOleDb.AddParameters("ReviewID", model.ReviewID);
            this.helperOleDb.AddParameters("ReviewText", model.ReviewText);
            this.helperOleDb.AddParameters("RatingTitle", model.RatingTitle.HasValue ? model.RatingTitle.Value.ToString() : null);
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
