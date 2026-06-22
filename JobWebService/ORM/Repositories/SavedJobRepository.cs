using System.Data;

namespace JobWebService.ORM.Repositories
{
    // Bookmarked jobs (employee saves a job to revisit later).
    public class SavedJobRepository : Repository
    {
        public SavedJobRepository(DBHelperOledb helperOleDb, ModelCreators modelcreators) : base(helperOleDb, modelcreators) { }

        public bool IsSaved(string userId, string jobId)
        {
            string sql = "SELECT COUNT(*) FROM [SavedJob] WHERE UserID=@UserID AND JobID=@JobID";
            this.helperOleDb.AddParameters("UserID", userId);
            this.helperOleDb.AddParameters("JobID", jobId);
            object val = this.helperOleDb.ReadValue(sql);
            return val != null && Convert.ToInt32(val) > 0;
        }

        public bool Save(string userId, string jobId)
        {
            if (IsSaved(userId, jobId)) return true;   // already saved — idempotent
            string sql = "INSERT INTO [SavedJob](UserID,JobID,SavedDate) VALUES(@UserID,@JobID,@SavedDate)";
            this.helperOleDb.AddParameters("UserID", userId);
            this.helperOleDb.AddParameters("JobID", jobId);
            this.helperOleDb.AddParameters("SavedDate", DateTime.UtcNow.ToString("O"));
            return this.helperOleDb.Create(sql) > 0;
        }

        public bool Unsave(string userId, string jobId)
        {
            string sql = "DELETE FROM [SavedJob] WHERE UserID=@UserID AND JobID=@JobID";
            this.helperOleDb.AddParameters("UserID", userId);
            this.helperOleDb.AddParameters("JobID", jobId);
            return this.helperOleDb.Delete(sql) > 0;
        }

        public List<string> GetJobIds(string userId)
        {
            List<string> ids = new List<string>();
            string sql = "SELECT JobID FROM [SavedJob] WHERE UserID=@UserID";
            this.helperOleDb.AddParameters("UserID", userId);
            using (IDataReader dr = this.helperOleDb.Read(sql))
                while (dr.Read())
                    ids.Add(Convert.ToString(dr["JobID"]));
            return ids;
        }
    }
}
