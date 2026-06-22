using JobModels;
using System.Data;

namespace JobWebService.ORM.ModelFactory
{
    public class ReportCreator : IModelCreator<Report>
    {
        public Report CreateModel(IDataReader reader)
        {
            Report report = new Report()
            {
                ReportID = Convert.ToString(reader["ReportID"]),
                ReporterUserID = reader["ReporterUserID"] == DBNull.Value ? null : Convert.ToString(reader["ReporterUserID"]),
                TargetType = HasColumn(reader, "TargetType") && reader["TargetType"] != DBNull.Value ? Convert.ToString(reader["TargetType"]) : null,
                TargetID = HasColumn(reader, "TargetID") && reader["TargetID"] != DBNull.Value ? Convert.ToString(reader["TargetID"]) : null,
                Category = reader["Category"] == DBNull.Value ? null : Convert.ToString(reader["Category"]),
                Subject = reader["Subject"] == DBNull.Value ? null : Convert.ToString(reader["Subject"]),
                ReportText = reader["ReportText"] == DBNull.Value ? null : Convert.ToString(reader["ReportText"]),
                Status = reader["Status"] == DBNull.Value ? null : Convert.ToString(reader["Status"]),
                ReportDate = reader["ReportDate"] == DBNull.Value ? null : Convert.ToString(reader["ReportDate"]),
            };
            return report;
        }

        private static bool HasColumn(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
