using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class EducationTypeRepository : Repository, IRepository<EducationType>
    {
        public EducationTypeRepository(DBHelperOledb helperOleDb) : base(helperOleDb) { }

        public bool Delete(int id)
        {
            string sql = $"DELETE FROM EducationTypes WHERE EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", id.ToString());
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = $"DELETE FROM EducationTypes WHERE EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(EducationType model)
        {
            if (model == null) return false;
            return Delete(model.EducationTypeID);
        }

        public bool Insert(EducationType model)
        {
            string sql = $"INSERT INTO EducationTypes(EducationTypeID,EducationLevel) VALUES(@EducationTypeID,@EducationLevel)";
            this.helperOleDb.AddParameters("EducationTypeID", model.EducationTypeID);
            this.helperOleDb.AddParameters("EducationLevel", model.EducationLevel);
            return this.helperOleDb.Create(sql) > 0;
        }

        public EducationType Read(object id)
        {
            string sql = $"SELECT * FROM EducationTypes WHERE EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", id.ToString());
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.EducationTypeCreator.CreateModel(dataReader);
            }
        }

        public List<EducationType> ReadAll()
        {
            List<EducationType> list = new List<EducationType>();
            string sql = "SELECT * FROM EducationTypes";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    list.Add(this.modelCreators.EducationTypeCreator.CreateModel(dataReader));
            return list;
        }

        public object ReadValue()
        {
            throw new NotImplementedException();
        }

        public bool Update(EducationType model)
        {
            string sql = "UPDATE EducationTypes SET EducationLevel=@EducationLevel WHERE EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", model.EducationTypeID);
            this.helperOleDb.AddParameters("EducationLevel", model.EducationLevel);
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
