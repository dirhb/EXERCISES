using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class CountryRepository : Repository, IRepository<Country>
    {
        public CountryRepository(DBHelperOledb helperOledb) : base(helperOledb) { }

        public bool Delete(int id)
        {
            string sql = $"DELETE FROM Countries WHERE CountryID=@CountryID";
            this.helperOleDb.AddParameters("CountryID", id.ToString()); //prevents SQL Injection
            return this.helperOleDb.Delete(sql) > 0;
        }
        public bool Delete(string id)
        {
            return false;
        }
        public bool Delete(Country model)
        {
            string sql = $"DELETE FROM Countries WHERE CountryID=@CountryID";
            this.helperOleDb.AddParameters("CountryID", model.CountryID.ToString()); //prevents SQL Injection
            return this.helperOleDb.Delete(sql) > 0;
        }
        public bool Insert(Country model)
        {
            string sql = $"INSERT INTO Countries(CountryID,CountryName) VALUES(@CountryID,@CountryName)";
            this.helperOleDb.AddParameters("CountryID", model.CountryID.ToString()); //prevents SQL Injection
            this.helperOleDb.AddParameters("CountryName", model.CountryName); //prevents SQL Injection
            return this.helperOleDb.Create(sql) > 0;
        }
        public Country Read(object id)
        {
            string sql = $"SELECT * FROM Countries WHERE CountryID=@CountryID";
            this.helperOleDb.AddParameters("CountryID", id.ToString()); //prevents SQL Injection
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                dataReader.Read();
                return this.modelCreators.CountryCreator.CreateModel(dataReader);
            }
            //returns Country
        }
        public List<Country> ReadAll()
        {
            List<Country> Countries = new List<Country>();
            string sql = "SELECT * FROM Countries";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read() == true)
                    Countries.Add(this.modelCreators.CountryCreator.CreateModel(dataReader));
            return Countries;
        }
        public object ReadValue()
        {
            throw new NotImplementedException();
        }
        public bool Update(Country model)
        {
            string sql = $"UPDATE Countries SET CountryName=@CountryName WHERE CountryID=@CountryID";
            this.helperOleDb.AddParameters("CountryID", model.CountryID.ToString()); //prevents SQL Injection
            this.helperOleDb.AddParameters("CountryName", model.CountryName); //prevents SQL Injection
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
