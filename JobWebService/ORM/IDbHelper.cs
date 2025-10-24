using System.Data;

namespace JobWebService
{
    public interface IDbHelper
    {
        void OpenConnection();
        void CloseConnection();
        //dataReader is an object of recordSet
        IDataReader Select(string sql);
        // CRUD 
        int Update(string sql);
        int Insert(string sql);
        int Delete(string sql);

        void OpenTransaction();

        void Commit();

        void Rollback();
    }
}
