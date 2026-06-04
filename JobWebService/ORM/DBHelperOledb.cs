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
            this.oleDbCommand = new OleDbCommand();

            string databasePath = ResolveDatabasePath();
            this.oleDbConnection.ConnectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={databasePath};Persist Security Info=True";
            Console.WriteLine("DB Path: " + databasePath);
            this.oleDbCommand.Connection = this.oleDbConnection;
            EnsureChatMessagesTableExists();
        }

        private static string ResolveDatabasePath()
        {
            const string databaseFileName = "JobFindUltimate.accdb";

            foreach (string startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            {
                DirectoryInfo? directory = new DirectoryInfo(startPath);

                while (directory != null)
                {
                    string candidate = Path.Combine(directory.FullName, "App_Data", databaseFileName);

                    if (File.Exists(candidate))
                        return candidate;

                    candidate = Path.Combine(directory.FullName, "JobWebService", "App_Data", databaseFileName);

                    if (File.Exists(candidate))
                        return candidate;

                    directory = directory.Parent;
                }
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "App_Data", databaseFileName);
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
            return ChangeDb(sql);
        }

        public int Insert(string sql)
        {
            return ChangeDb(sql);

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

        // Compatibility wrappers used by repositories
        public void ClearParameters()
        {
            this.oleDbCommand.Parameters.Clear();
        }

        public int ExecuteNonQuery(string sql)
        {
            return ChangeDb(sql);
        }

        public object ExecuteScalar(string sql)
        {
            this.oleDbCommand.CommandText = sql;
            return this.oleDbCommand.ExecuteScalar();
        }

        public void BeginTransaction()
        {
            this.OpenTransaction();
            this.oleDbCommand.Transaction = this.oleDbTransaction;
        }

        public void CommitTransaction()
        {
            this.Commit();
        }

        public void RollbackTransaction()
        {
            this.Rollback();
        }

        public IDataReader Select(string sql)
        {
            this.oleDbCommand.CommandText = sql;
            return this.oleDbCommand.ExecuteReader();
        }

        public int Update(string sql)
        {
            return ChangeDb(sql);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void RollBack()
        {
            throw new NotImplementedException();
        }

        public void AddParameters(string name, object value)
        {
            this.oleDbCommand.Parameters.Add(new OleDbParameter(name, value ?? DBNull.Value));
        }

        public IDataReader Read(string sql)
        {
            this.oleDbCommand.CommandText = sql;
            IDataReader datareader = this.oleDbCommand.ExecuteReader(); //collects the data
            oleDbCommand.Parameters.Clear();
            return datareader;
        }

        public object ReadValue(string sql)
        {
            this.oleDbCommand.CommandText = sql;
            try
            {
                return this.oleDbCommand.ExecuteScalar(); //collects the single "drop" of data
            }
            finally
            {
                this.oleDbCommand.Parameters.Clear();
            }
        }

        public int Create(string sql)
        {
            return ChangeDb(sql);
        }

        private int ChangeDb(string sql)
        {
            this.oleDbCommand.CommandText = sql;
            try
            {
                return this.oleDbCommand.ExecuteNonQuery(); //Changes columms in the database, and returns the number of columms it changed
            }
            finally
            {
                this.oleDbCommand.Parameters.Clear();
            }
        }

        public void CreateCommand()
        {
            this.oleDbCommand = this.oleDbConnection.CreateCommand();
        }

        private void EnsureChatMessagesTableExists()
        {
            bool tableExists = false;
            try
            {
                this.oleDbConnection.Open();
                using (var cmd = this.oleDbConnection.CreateCommand())
                {
                    cmd.CommandText = "SELECT TOP 1 MessageID FROM ChatMessages";
                    using (var reader = cmd.ExecuteReader()) { }
                    tableExists = true;
                }
            }
            catch
            {
                tableExists = false;
            }
            finally
            {
                this.oleDbConnection.Close();
            }

            if (!tableExists)
            {
                try
                {
                    this.oleDbConnection.Open();
                    using (var cmd = this.oleDbConnection.CreateCommand())
                    {
                        cmd.CommandText = "CREATE TABLE ChatMessages (MessageID COUNTER PRIMARY KEY, SenderID VARCHAR(100), ReceiverID VARCHAR(100), MessageText MEMO, SentAt VARCHAR(50))";
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Created table ChatMessages successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error creating ChatMessages table: " + ex.Message);
                }
                finally
                {
                    this.oleDbConnection.Close();
                }
            }
        }
    }
}