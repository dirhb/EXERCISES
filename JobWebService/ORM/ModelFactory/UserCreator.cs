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
                FirstName = Convert.ToString(reader["FirstName"]),
                LastName = Convert.ToString(reader["LastName"]),
                UserName = Convert.ToString(reader["UserName"]),
                Password = Convert.ToString(reader["Password"]),
                Country = Convert.ToString(reader["Country"]),
                PhoneNum = Convert.ToString(reader["PhoneNum"]),
                CreationDate = Convert.ToString(reader["CreationDate"]),
                UserTypeID = Convert.ToInt16(reader["UserTypeID"]),
                Email = Convert.ToString(reader["Email"]),
                ResumeText = reader["ResumeText"] == DBNull.Value ? null : Convert.ToString(reader["ResumeText"]),
                Salary = reader["Salary"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Salary"])
            };
            return User;
        }
    }
}
