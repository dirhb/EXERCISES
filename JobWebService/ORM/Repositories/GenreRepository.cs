using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class GenreRepository : Repository, IRepository<Genre>
    {
        public GenreRepository(DBHelperOledb helperOleDb) : base(helperOleDb) { }

        public bool Delete(int id)
        {
            string sql = $"DELETE FROM Genres WHERE GenreID=@GenreID";
            this.helperOleDb.AddParameters("GenreID", id.ToString());
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = $"DELETE FROM Genres WHERE GenreID=@GenreID";
            this.helperOleDb.AddParameters("GenreID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(Genre model)
        {
            if (model == null) return false;
            return Delete(model.GenreID);
        }

        public bool Insert(Genre model)
        {
            string sql = $"INSERT INTO Genres(GenreID,GenreTitle,GenreDescription) VALUES(@GenreID,@GenreTitle,@GenreDescription)";
            this.helperOleDb.AddParameters("GenreID", model.GenreID);
            this.helperOleDb.AddParameters("GenreTitle", model.GenreTitle);
            this.helperOleDb.AddParameters("GenreDescription", model.GenreDescription);
            return this.helperOleDb.Create(sql) > 0;
        }

        public Genre Read(object id)
        {
            string sql = $"SELECT * FROM Genres WHERE GenreID=@GenreID";
            this.helperOleDb.AddParameters("GenreID", id.ToString());
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.GenreCreator.CreateModel(dataReader);
            }
        }

        public List<Genre> ReadAll()
        {
            List<Genre> list = new List<Genre>();
            string sql = "SELECT * FROM Genres";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.GenreCreator.CreateModel(dataReader));
            return list;
        }

        public object ReadValue()
        {
            throw new NotImplementedException();
        }

        public bool Update(Genre model)
        {
            string sql = "UPDATE Genres SET GenreTitle=@GenreTitle,GenreDescription=@GenreDescription WHERE GenreID=@GenreID";
            this.helperOleDb.AddParameters("GenreID", model.GenreID);
            this.helperOleDb.AddParameters("GenreTitle", model.GenreTitle);
            this.helperOleDb.AddParameters("GenreDescription", model.GenreDescription);
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
