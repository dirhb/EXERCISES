using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class EducationRepository : Repository, IRepository<Education>
    {
        public EducationRepository(DBHelperOledb helperOledb) : base(helperOledb) { }
        public bool Delete(int id)
        {
            string sql = $"DELETE FROM Educations WHERE EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", id.ToString()); //prevents SQL Injection
            return this.helperOleDb.Delete(sql) > 0;
        }
        public bool Delete(string id)
        {
            return false;
        }
        public bool Delete(Education model)
        {
            string sql = $"DELETE FROM Educations WHERE EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", model.EducationTypeID.ToString()); //prevents SQL Injection
            return this.helperOleDb.Delete(sql) > 0;
        }
        public bool Insert(Education model)
        {
            string sql = $"INSERT INTO Educations(EducationTypeID,GenreID) VALUES(@EducationTypeID,@GenreID)";
            this.helperOleDb.AddParameters("EducationTypeID", model.EducationTypeID.ToString()); //prevents SQL Injection
            this.helperOleDb.AddParameters("GenreID", model.GenreID); //prevents SQL Injection
            return this.helperOleDb.Create(sql) > 0;
        }
        public Education Read(object id)
        {
            string sql = $"SELECT * FROM Educations WHERE EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", id.ToString()); //prevents SQL Injection
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.EducationCreator.CreateModel(dataReader);
            }
            //returns Education
        }
        public List<Education> ReadAll()
        {
            List<Education> Educations = new List<Education>();
            string sql = "SELECT * FROM Educations";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read() == true)
                    Educations.Add(this.modelCreators.EducationCreator.CreateModel(dataReader));
            return Educations;
        }
        public object ReadValue()
        {
            throw new NotImplementedException();
        }
        public bool Update(Education model)
        {
            // Update the Genre for the given user and education type
            string sql = $"UPDATE Educations SET GenreID=@GenreID WHERE UserID=@UserID AND EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("UserID", model.UserID ?? string.Empty);
            this.helperOleDb.AddParameters("EducationTypeID", model.EducationTypeID ?? string.Empty);
            this.helperOleDb.AddParameters("GenreID", model.GenreID ?? string.Empty);
            return this.helperOleDb.Update(sql) > 0;
        }
        public bool UpdatePartial(Education model, List<string> columns)
        {
            List<string> setStatements = new List<string>();
            if (columns.Contains("GenreID"))
                setStatements.Add("GenreID=@GenreID");
            string sql = $"UPDATE Educations SET {string.Join(",", setStatements)} WHERE UserID=@UserID AND EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("UserID", model.UserID ?? string.Empty);
            this.helperOleDb.AddParameters("EducationTypeID", model.EducationTypeID ?? string.Empty);
            if (columns.Contains("GenreID"))
                this.helperOleDb.AddParameters("GenreID", model.GenreID ?? string.Empty);
            return this.helperOleDb.Update(sql) > 0;
        }
        public bool Exists(int id)
        {
            string sql = $"SELECT COUNT(*) FROM Educations WHERE EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", id.ToString()); //prevents SQL Injection
            int count = Convert.ToInt32(this.helperOleDb.ReadValue(sql));
            return count > 0;
        }
        public bool Exists(string name)
        {
            // treat 'name' here as the UserID to check whether that user has education entries
            string sql = $"SELECT COUNT(*) FROM Educations WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", name); //prevents SQL Injection
            int count = Convert.ToInt32(this.helperOleDb.ReadValue(sql));
            return count > 0;
        }
        public int GetIdByName(string name)
        {
            // return the EducationTypeID for the given UserID
            string sql = $"SELECT EducationTypeID FROM Educations WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", name); //prevents SQL Injection
            var val = this.helperOleDb.ReadValue(sql);
            return val == null || val == DBNull.Value ? 0 : Convert.ToInt32(val);
        }
        public string GetNameById(int id)
        {
            // return the GenreID for the given EducationTypeID
            string sql = $"SELECT GenreID FROM Educations WHERE EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", id.ToString()); //prevents SQL Injection
            var val = this.helperOleDb.ReadValue(sql);
            return val == null || val == DBNull.Value ? null : val.ToString();
        }
        public int GetNextId()
        {
            // Not applicable because EducationTypeID is not necessarily numeric. Implement if needed.
            throw new NotImplementedException();
        }
        public void BulkInsert(List<Education> educations)
        {
            foreach (var education in educations)
            {
                this.Insert(education);
            }
        }
        public void BulkUpdate(List<Education> educations)
        {
            foreach (var education in educations)
            {
                this.Update(education);
            }
        }
        public void BulkDelete(List<Education> educations)
        {
            foreach (var education in educations)
            {
                this.Delete(education);
            }
        }
        public void Truncate()
        {
            string sql = "DELETE FROM Educations";
            this.helperOleDb.ExecuteNonQuery(sql);
        }
        public int Count()
        {
            string sql = "SELECT COUNT(*) FROM Educations";
            return (int)this.helperOleDb.ReadValue(sql);
        }
        public List<Education> FindByName(string name)
        {
            List<Education> educations = new List<Education>();
            // find by UserID pattern
            string sql = "SELECT * FROM Educations WHERE UserID LIKE @UserID";
            this.helperOleDb.AddParameters("UserID", "%" + name + "%"); //prevents SQL Injection
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read() == true)
                    educations.Add(this.modelCreators.EducationCreator.CreateModel(dataReader));
            return educations;
        }
        public List<Education> FindByPartialId(int partialId)
        {
            List<Education> educations = new List<Education>();
            string sql = "SELECT * FROM Educations WHERE EducationTypeID LIKE @EducationTypeID";
            this.helperOleDb.AddParameters("EducationTypeID", "%" + partialId.ToString() + "%"); //prevents SQL Injection
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read() == true)
                    educations.Add(this.modelCreators.EducationCreator.CreateModel(dataReader));
            return educations;
        }
        public List<Education> GetPaged(int pageNumber, int pageSize)
        {
            List<Education> educations = new List<Education>();
            int offset = (pageNumber - 1) * pageSize;
            string sql = $"SELECT * FROM Educations ORDER BY EducationID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            this.helperOleDb.AddParameters("Offset", offset.ToString()); //prevents SQL Injection
            this.helperOleDb.AddParameters("PageSize", pageSize.ToString()); //prevents SQL Injection
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read() == true)
                    educations.Add(this.modelCreators.EducationCreator.CreateModel(dataReader));
            return educations;
        }
        public void Dispose()
        {
            this.helperOleDb.Dispose();
        }
        public Education ReadByName(string name)
        {
            string sql = $"SELECT * FROM Educations WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", name); //prevents SQL Injection
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.EducationCreator.CreateModel(dataReader);
            }
            //returns Education
        }
        public int GetIdByEducation(Education education)
        {
            string sql = $"SELECT EducationTypeID FROM Educations WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", education.UserID); //prevents SQL Injection
            var val = this.helperOleDb.ReadValue(sql);
            return val == null || val == DBNull.Value ? 0 : Convert.ToInt32(val);
        }
        public string GetNameByEducation(Education education)
        {
            string sql = $"SELECT GenreID FROM Educations WHERE UserID=@UserID AND EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("UserID", education.UserID); //prevents SQL Injection
            this.helperOleDb.AddParameters("EducationTypeID", education.EducationTypeID); //prevents SQL Injection
            var val = this.helperOleDb.ReadValue(sql);
            return val == null || val == DBNull.Value ? null : val.ToString();
        }
        public bool Exists(Education education)
        {
            string sql = $"SELECT COUNT(*) FROM Educations WHERE UserID=@UserID AND EducationTypeID=@EducationTypeID";
            this.helperOleDb.AddParameters("UserID", education.UserID);
            this.helperOleDb.AddParameters("EducationTypeID", education.EducationTypeID);
            int count = Convert.ToInt32(this.helperOleDb.ReadValue(sql));
            return count > 0;
        }
        public List<Education> ReadByIds(List<int> ids)
        {
            List<Education> educations = new List<Education>();
            string sql = $"SELECT * FROM Educations WHERE EducationTypeID IN ({string.Join(",", ids)})";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read() == true)
                    educations.Add(this.modelCreators.EducationCreator.CreateModel(dataReader));
            return educations;
        }
        public List<Education> ReadByNames(List<string> names)
        {
            List<Education> educations = new List<Education>();
            string sql = $"SELECT * FROM Educations WHERE UserID IN ({string.Join(",", names.Select((n, i) => "@p" + i))})";
            for (int i = 0; i < names.Count; i++)
            {
                this.helperOleDb.AddParameters("p" + i, names[i]); //prevents SQL Injection
            }
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read() == true)
                    educations.Add(this.modelCreators.EducationCreator.CreateModel(dataReader));
            return educations;
        }
        // Keep only necessary wrappers
        public void ClearParameters() => this.helperOleDb.ClearParameters();

        public DBHelperOledb GetDBHelper() => this.helperOleDb; // consider removing if not used externally
    }
}
