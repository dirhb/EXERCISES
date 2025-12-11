using System.Data;

namespace JobWebService
{
    public interface IDatabaseHelper
    {
        void OpenConnection();

        void CloseConnection();

        //datareader זה אובייקט של recordset
        IDataReader Select(string sql);

        //CRUD פעולות
        int Update(string sql);
        int Insert(string sql);
        int Delete(string sql);

        void OpenTransaction();
        void Commit();
        void RollBack();
    }
}
