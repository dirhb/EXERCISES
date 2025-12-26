using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class UserTypeCreator : IModelCreator<UserType>
    {
        public UserType CreateModel(IDataReader reader)
        {
            UserType UserType = new UserType()
            {
                UserTypeID = Convert.ToString(reader["UserTypeID"]),
                UserTypeName = Convert.ToString(reader["UserTypeName"]),
            };
            return UserType;
        }
    }
}
