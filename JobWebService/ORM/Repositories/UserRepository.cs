using JobModels;
using System.Data;

namespace JobWebService.ORM.Repositories
{
    public class UserRepository : Repository, IRepository<User>
    {
        public UserRepository(DBHelperOledb helperOleDb, ModelCreators modelcreators) : base(helperOleDb, modelcreators) { }

        // Checks whether username exists (returns true if available)
        public bool IsAvailableUserName(string userName)
        {
            string sql = "SELECT 1 FROM Users WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", userName);
            using (IDataReader dr = this.helperOleDb.Read(sql))
            {
                return !dr.Read();
            }
        }

        //Read password hash and salt for a user
        public String GetPasswordByUserId(string userId)
        {
            string sql = "SELECT Password, '' AS Salt FROM Users WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", userId);
            using (IDataReader dr = this.helperOleDb.Read(sql))
            {
                if (dr == null || !dr.Read()) return null;
                return Convert.ToString(dr["Password"]);/*, Convert.ToString(dr["Salt"]);*/
            }
        }

        // Return UserType for given user id
        public UserType UserTypeByUserId(string userId)
        {
            string sql = "SELECT ut.UserTypeID, ut.UserTypeName FROM UserTypes ut INNER JOIN Users u ON ut.UserTypeID = u.UserTypeID WHERE u.UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", userId);
            using (IDataReader dr = this.helperOleDb.Read(sql))
            {
                if (dr == null || !dr.Read()) return null;
                return this.modelCreators.UserTypeCreator.CreateModel(dr);
            }
        }

        // minimal 2FA projection (returns subset of user fields)
        public User GetUser2FAByUserName(string userName)
        {
            string sql = "SELECT UserID, UserTypeID, Email FROM Users WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", userName);
            using (IDataReader dr = this.helperOleDb.Read(sql))
            {
                if (dr == null || !dr.Read()) return null;
                User u = new User()
                {
                    UserID = Convert.ToString(dr["UserID"]),
                    UserTypeID = dr["UserTypeID"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["UserTypeID"]),
                    Email = Convert.ToString(dr["Email"])
                };
                return u;
            }
        }

        public bool Delete(int id)
        {
            string sql = $"DELETE FROM Users WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", id.ToString());
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = $"DELETE FROM Users WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", id);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public bool Delete(User model)
        {
            if (model == null) return false;
            return Delete(model.UserID);
        }

        public bool Insert(User model)
        {
            string sql = $"INSERT INTO Users(UserID,FirstName,LastName,Password,Country,PhoneNum,CreationDate,UserTypeID,Email) VALUES(@UserID,@FirstName,@LastName,@Password,@Country,@PhoneNum,@CreationDate,@UserTypeID,@Email)";
            this.helperOleDb.AddParameters("UserID", model.UserID);
            this.helperOleDb.AddParameters("FirstName", model.FirstName);
            this.helperOleDb.AddParameters("LastName", model.LastName);
            this.helperOleDb.AddParameters("Password", model.Password);
            this.helperOleDb.AddParameters("Country", model.Country);
            this.helperOleDb.AddParameters("PhoneNum", model.PhoneNum);
            this.helperOleDb.AddParameters("CreationDate", model.CreationDate);
            this.helperOleDb.AddParameters("UserTypeID", model.UserTypeID.HasValue ? model.UserTypeID.Value.ToString() : null);
            this.helperOleDb.AddParameters("Email", model.Email);
            return this.helperOleDb.Create(sql) > 0;
        }

        // alias matching other repos
        public bool Create(User model) => Insert(model);

        public User Read(object id)
        {
            string sql = $"SELECT * FROM Users WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", id.ToString());
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.UserCreator.CreateModel(dataReader);
            }
        }

        public List<User> ReadAll()
        {
            List<User> users = new List<User>();
            string sql = "SELECT * FROM Users";
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
                while (dataReader.Read())
                    users.Add(this.modelCreators.UserCreator.CreateModel(dataReader));
            return users;
        }

        public object ReadValue()
        {
            string sql = "SELECT COUNT(*) FROM Users";
            return this.helperOleDb.ReadValue(sql);
        }

        public bool Update(User model)
        {
            string sql = "UPDATE Users SET FirstName=@FirstName,LastName=@LastName,Password=@Password,Country=@Country,PhoneNum=@PhoneNum,CreationDate=@CreationDate,UserTypeID=@UserTypeID,Email=@Email WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", model.UserID);
            this.helperOleDb.AddParameters("FirstName", model.FirstName);
            this.helperOleDb.AddParameters("LastName", model.LastName);
            this.helperOleDb.AddParameters("Password", model.Password);
            this.helperOleDb.AddParameters("Country", model.Country);
            this.helperOleDb.AddParameters("PhoneNum", model.PhoneNum);
            this.helperOleDb.AddParameters("CreationDate", model.CreationDate);
            this.helperOleDb.AddParameters("UserTypeID", model.UserTypeID.HasValue ? model.UserTypeID.Value.ToString() : null);
            this.helperOleDb.AddParameters("Email", model.Email);
            return this.helperOleDb.Update(sql) > 0;
        }

        // Additional helpers inspired by referenced implementation
        public User ReadByEmail(string email)
        {
            string sql = "SELECT * FROM Users WHERE Email=@Email";
            this.helperOleDb.AddParameters("Email", email);
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.UserCreator.CreateModel(dataReader);
            }
        }
        //checks if email exists in db
        public bool ExistsByEmail(string email)
        {
            string sql = "SELECT COUNT(*) FROM Users WHERE Email=@Email";
            this.helperOleDb.AddParameters("Email", email);
            var val = this.helperOleDb.ReadValue(sql);
            return val != null && Convert.ToInt32(val) > 0;
        }

        public User GetByCredentials(string email, string password)
        {
            string sql = "SELECT * FROM Users WHERE Email=@Email AND Password=@Password";
            this.helperOleDb.AddParameters("Email", email);
            this.helperOleDb.AddParameters("Password", password);
            using (IDataReader dataReader = this.helperOleDb.Read(sql))
            {
                if (dataReader == null) return null;
                if (!dataReader.Read()) return null;
                return this.modelCreators.UserCreator.CreateModel(dataReader);
            }
        }

        // Compatibility helpers used by controllers
        public bool IsExistUserName(string userName)
        {
            string sql = "SELECT 1 FROM Users WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", userName);
            using (IDataReader dr = this.helperOleDb.Read(sql))
            {
                return dr != null && dr.Read();
            }
        }

        public bool UpdatePassword(User user)
        {
            if (user == null) return false;
            string sql = "UPDATE Users SET Password=@Password WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("Password", user.Password);
            this.helperOleDb.AddParameters("UserID", user.UserID);
            return this.helperOleDb.Update(sql) > 0;
        }
    }
}
