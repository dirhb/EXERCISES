using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class UserCreator : IModelCreator<User>
    {
        public User CreateModel(IDataReader reader)
        {
            User User = new User()
            {
                UserID = Convert.ToString(reader["UserID"]),
                FirstName = Convert.ToString(reader["FirstName"]),
                LastName = Convert.ToString(reader["LastName"]),
                Password = Convert.ToString(reader["Password"]),
                Country = Convert.ToString(reader["Country"]),
                PhoneNum = Convert.ToString(reader["PhoneNum"]),
                CreationDate = Convert.ToString(reader["CreationDate"]),
                UserTypeID = Convert.ToInt16(reader["UserTypeID"]),
                Email = Convert.ToString(reader["Email"]),
            };
            return User;
        }
    }
}
