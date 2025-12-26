using System.Data;
using System.Data.OleDb;

namespace JobWebService
{
    public class DBHelperOledb : IDatabaseHelper
    {
        //יוצר קשר עם מוסד הנתונים
        OleDbConnection oleDbConnection;
        //אחראי לשלוח פקודות למוסד נתונים ומחזיר תשובה ממוסד נתונים
        OleDbCommand oleDbCommand;
        
        OleDbTransaction oleDbTransaction;


        public DBHelperOledb()
        {

            this.oleDbConnection = new OleDbConnection();
            this.oleDbConnection.ConnectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\stam\EXERCISES\JobWebService\App_Data\JobFindDataBase.accdb;Persist Security Info=True";
            this.oleDbCommand = new OleDbCommand();
            //this.oleDbConnection.ConnectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\App_Data\JobFindDataBase.accdb;Persist Security Info=True";
            this.oleDbCommand = new OleDbCommand();
            this.oleDbCommand.Connection = this.oleDbConnection;
        }
        public void CloseConnection()
        {
            this.oleDbConnection.Close();
        }

        public void Commit()
        {
            this.oleDbTransaction.Commit();
        }

        public int Delete(string sql)
        {
            this.oleDbCommand.CommandText = sql;
            return this.oleDbCommand.ExecuteNonQuery();
        }

        public int Insert(string sql)
        {
            this.oleDbCommand.CommandText = sql;
            return this.oleDbCommand.ExecuteNonQuery();

        }

        public void OpenConnection()
        {
           this.oleDbConnection.Open();
        }

        public void OpenTransaction()
        {
            this.oleDbTransaction = this.oleDbConnection.BeginTransaction();
        }

        public void Rollback()
        {
            this.oleDbTransaction.Rollback();
        }

        public IDataReader Select(string sql)
        {
            this.oleDbCommand.CommandText = sql;
            return this.oleDbCommand.ExecuteReader();
        }

        public int Update(string sql)
        {
            this.oleDbCommand.CommandText = sql;
            return this.oleDbCommand.ExecuteNonQuery();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void RollBack()
        {
            throw new NotImplementedException();
        }
    }
}
