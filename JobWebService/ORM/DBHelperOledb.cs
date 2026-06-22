using System.Data;
using System.Data.OleDb;

namespace JobWebService
{
    public class DBHelperOledb : IDatabaseHelper
    {
        // The Microsoft ACE (Access) OLE DB provider is not safe to use from
        // several threads at once. Each web request creates its own helper +
        // connection, so without a gate two requests can hit the same .accdb
        // simultaneously and the native provider faults — crashing the whole
        // service process (exit code -1, an uncatchable AccessViolation).
        //
        // This single process-wide lock is taken in OpenConnection() and
        // released in CloseConnection(), so a whole DB "session" (open → run
        // queries → close) runs to completion before the next one starts.
        // Every controller brackets its DB use in try { OpenConnection… }
        // finally { CloseConnection() }, so the lock is always released.
        private static readonly object DbLock = new object();

        // The chat-table check only needs to happen once for the whole app,
        // not on every single request. Guarded by DbLock; volatile so the
        // fast-path read in EnsureChatMessagesTableExists() sees the update.
        private static volatile bool schemaInitialized = false;

        // Tracks whether THIS instance currently holds DbLock, so
        // CloseConnection() only releases when OpenConnection() actually took it.
        private bool lockHeld = false;

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
            // Close the connection first, then always release the lock if we
            // took it — even if Close() throws — so the gate can never get stuck.
            try
            {
                this.oleDbConnection.Close();
            }
            finally
            {
                if (this.lockHeld)
                {
                    this.lockHeld = false;
                    Monitor.Exit(DbLock);
                }
            }
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
            // Take the process-wide gate before touching the database, so only
            // one request uses the Access provider at a time. Released in
            // CloseConnection() (always called from a finally block).
            Monitor.Enter(DbLock);
            this.lockHeld = true;
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
            // Release the lock if we still hold it, then free the connection.
            // (Previously threw NotImplementedException, which could crash the
            // process if anything ever disposed this helper.)
            if (this.lockHeld)
            {
                this.lockHeld = false;
                Monitor.Exit(DbLock);
            }
            this.oleDbCommand?.Dispose();
            this.oleDbConnection?.Dispose();
        }

        public void RollBack()
        {
            this.oleDbTransaction?.Rollback();
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
            // Run this at most once for the whole app, serialized behind the
            // same gate as every other DB access. Previously this ran inside
            // the constructor on EVERY request, so a burst of requests opened
            // many concurrent connections to Access at once — the main trigger
            // for the provider crash.
            if (schemaInitialized)
                return;

            lock (DbLock)
            {
                if (schemaInitialized)
                    return;

                EnsureChatMessagesTableExistsCore();
                EnsureColumnExists("Job", "Salary", "ALTER TABLE Job ADD COLUMN Salary CURRENCY");
                EnsureColumnExists("Notification", "RecipientUserID", "ALTER TABLE Notification ADD COLUMN RecipientUserID VARCHAR(50)");
                EnsureTableExists("Report", "SELECT TOP 1 ReportID FROM [Report]",
                    "CREATE TABLE [Report] (ReportID COUNTER PRIMARY KEY, ReporterUserID VARCHAR(50), TargetType VARCHAR(50), TargetID VARCHAR(50), Category VARCHAR(100), Subject VARCHAR(255), ReportText MEMO, Status VARCHAR(50), ReportDate VARCHAR(50))");
                EnsureColumnExists("Report", "TargetType", "ALTER TABLE [Report] ADD COLUMN TargetType VARCHAR(50)");
                EnsureColumnExists("Report", "TargetID", "ALTER TABLE [Report] ADD COLUMN TargetID VARCHAR(50)");
                EnsureColumnExists("Review", "EmployerID", "ALTER TABLE [Review] ADD COLUMN EmployerID VARCHAR(50)");
                EnsureColumnExists("Review", "ReviewDate", "ALTER TABLE [Review] ADD COLUMN ReviewDate VARCHAR(50)");
                EnsureColumnExists("User", "IsBanned", "ALTER TABLE [User] ADD COLUMN IsBanned INTEGER");
                EnsureColumnExists("JobApplications", "OfferedSalary", "ALTER TABLE JobApplications ADD COLUMN OfferedSalary CURRENCY");
                EnsureColumnExists("User", "PreferredCurrency", "ALTER TABLE [User] ADD COLUMN PreferredCurrency VARCHAR(10)");
                EnsureTableExists("SavedJob", "SELECT TOP 1 SavedJobID FROM [SavedJob]",
                    "CREATE TABLE [SavedJob] (SavedJobID COUNTER PRIMARY KEY, UserID VARCHAR(50), JobID VARCHAR(50), SavedDate VARCHAR(50))");
                schemaInitialized = true;
            }
        }

        // Creates a table if the probe query fails (table missing), so a copy of
        // the .accdb without it gets it on first run — same idea as ChatMessages.
        private void EnsureTableExists(string table, string probeSql, string createSql)
        {
            bool tableExists = false;
            try
            {
                this.oleDbConnection.Open();
                using (var cmd = this.oleDbConnection.CreateCommand())
                {
                    cmd.CommandText = probeSql;
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
                        cmd.CommandText = createSql;
                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"Created table {table} successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating table {table}: " + ex.Message);
                }
                finally
                {
                    this.oleDbConnection.Close();
                }
            }
        }

        // Adds a column if it's missing, so an older copy of the .accdb upgrades
        // itself on first run (same self-healing idea as the ChatMessages table).
        private void EnsureColumnExists(string table, string column, string addColumnSql)
        {
            bool columnExists = false;
            try
            {
                this.oleDbConnection.Open();
                using (var cmd = this.oleDbConnection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT TOP 1 [{column}] FROM [{table}]";
                    using (var reader = cmd.ExecuteReader()) { }
                    columnExists = true;
                }
            }
            catch
            {
                columnExists = false;
            }
            finally
            {
                this.oleDbConnection.Close();
            }

            if (!columnExists)
            {
                try
                {
                    this.oleDbConnection.Open();
                    using (var cmd = this.oleDbConnection.CreateCommand())
                    {
                        cmd.CommandText = addColumnSql;
                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"Added column {table}.{column} successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding column {table}.{column}: " + ex.Message);
                }
                finally
                {
                    this.oleDbConnection.Close();
                }
            }
        }

        private void EnsureChatMessagesTableExistsCore()
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