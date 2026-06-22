using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class ReviewRepository : Repository, IRepository<Review>
    {
        public ReviewRepository(DBHelperOledb helperOleDb, ModelCreators modelcreators) : base(helperOleDb, modelcreators) { }

        public bool Insert(Review model)
        {
            // ReviewID is an AutoNumber, so it isn't supplied here.
            string sql = "INSERT INTO [Review](ReviewsText,RatingTitle,UserID,EmployerID,ReviewDate) VALUES(@ReviewsText,@RatingTitle,@UserID,@EmployerID,@ReviewDate)";
            this.helperOleDb.AddParameters("ReviewsText", model.ReviewText);
            this.helperOleDb.AddParameters("RatingTitle", model.RatingTitle.HasValue ? model.RatingTitle.Value.ToString() : null);
            this.helperOleDb.AddParameters("UserID", model.UserID);
            this.helperOleDb.AddParameters("EmployerID", model.EmployerID);
            this.helperOleDb.AddParameters("ReviewDate", model.ReviewDate);
            return this.helperOleDb.Create(sql) > 0;
        }

        public bool Create(Review model) => Insert(model);

        public Review Read(object id)
        {
            string sql = "SELECT * FROM [Review] WHERE ReviewID=@ReviewID";
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
            string sql = "SELECT * FROM [Review] ORDER BY ReviewID DESC";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.ReviewsCreator.CreateModel(dataReader));
            return list;
        }

        // All reviews written about one employer, newest first.
        public List<Review> ReadByEmployer(string employerId)
        {
            List<Review> list = new List<Review>();
            string sql = "SELECT * FROM [Review] WHERE EmployerID=@EmployerID ORDER BY ReviewID DESC";
            this.helperOleDb.AddParameters("EmployerID", employerId);
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.ReviewsCreator.CreateModel(dataReader));
            return list;
        }

        public object ReadValue()
        {
            string sql = "SELECT COUNT(*) FROM [Review]";
            return this.helperOleDb.ReadValue(sql);
        }

        public bool Update(Review model)
        {
            string sql = "UPDATE [Review] SET ReviewsText=@ReviewsText,RatingTitle=@RatingTitle WHERE ReviewID=@ReviewID";
            this.helperOleDb.AddParameters("ReviewsText", model.ReviewText);
            this.helperOleDb.AddParameters("RatingTitle", model.RatingTitle.HasValue ? model.RatingTitle.Value.ToString() : null);
            this.helperOleDb.AddParameters("ReviewID", model.ReviewID);
            return this.helperOleDb.Update(sql) > 0;
        }

        public bool Delete(int id) => Delete(id.ToString());

        public bool Delete(string id)
        {
            string sql = "DELETE FROM [Review] WHERE ReviewID=@ReviewID";
            this.helperOleDb.AddParameters("ReviewID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(Review model)
        {
            if (model == null) return false;
            return Delete(model.ReviewID);
        }
    }
}
