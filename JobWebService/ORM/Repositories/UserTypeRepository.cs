using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class UserTypeRepository : Repository, IRepository<UserType>
    {
        public UserTypeRepository(DBHelperOledb helperOledb) : base(helperOledb) { }

        public bool Delete(int id)
        {
            string sql = $"DELETE FROM UserTypes WHERE UserTypeID=@UserTypeID";
            this.helperOleDb.AddParameters("UserTypeID", id.ToString()); //prevents SQL Injection
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(string id)
        {
            return false;
        }

        public bool Delete(UserType model)
        {
            string sql = $"DELETE FROM UserTypes WHERE UserTypeId=@UserTypeID";
            this.helperOleDb.AddParameters("UserTypeID", model.UserTypeID.ToString()); //prevents SQL Injection
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Insert(UserType model)
        {
            string sql = $"INSERT INTO UserTypes(UserTypeID,UserTypeName) VALUES(@UserTypeID,@UserTypeName)";
            this.helperOleDb.AddParameters("UserTypeID", model.UserTypeID.ToString()); //prevents SQL Injection
            this.helperOleDb.AddParameters("UserTypeName", model.UserTypeName); //prevents SQL Injection
            return this.helperOleDb.Create(sql) > 0;
        }

        public UserType Read(object id)
        {
            string sql = $"SELECT FROM UserTypes WHERE UserTypeID=@UserTypeID";
            this.helperOleDb.AddParameters("UserTypeID", id.ToString()); //prevents SQL Injection
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                dataReader.Read();
                return this.modelCreators.UserTypeCreator.CreateModel(dataReader);
            }
            //returns Menu
        }

        public List<UserType> ReadAll()
        {
            List<UserType> UserTypes = new List<UserType>();
            string sql = "SELECT * FROM UserTypes";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read() == true)
                    UserTypes.Add(this.modelCreators.UserTypeCreator.CreateModel(dataReader));
            return UserTypes;
        }

        public object ReadValue()
        {
            throw new NotImplementedException();
        }

        public bool Update(UserType model)
        {
            string sql = "UPDATE UserTypes SET UserTypeName=@UserTypeName where UserTypeID=@UserTypeID";
            this.helperOleDb.AddParameters("UserTypeID", model.UserTypeID.ToString()); //prevents SQL Injection
            this.helperOleDb.AddParameters("UserTypeName", model.UserTypeName); //prevents SQL Injection
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
