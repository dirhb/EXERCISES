using System.Data;

namespace JobWebService
{
    public interface IDatabaseHelper
    {
        void OpenConnection();

        void CloseConnection();

        //datareader זה אובייקט של recordset
        IDataReader Select(string sql);

        //Read
        IDataReader Read(string sql);
        object ReadValue(string sql);

        //CRUD פעולות
        int Update(string sql);
        int Insert(string sql);
        int Delete(string sql);

        //Create
        int Create(string sql);

        void OpenTransaction();
        void Commit();
        void RollBack();
        void CreateCommand();
    }
}
