using JobModels;
using JobWebService.ORM.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ReportController : ControllerBase
    {
        LibraryUOW libraryUOW;

        public ReportController()
        {
            this.libraryUOW = new LibraryUOW();
        }

        // Submit a new report. The reporter and text are required; the server
        // stamps the date and sets the initial status.
        [HttpPost]
        public bool SubmitReport([FromBody] Report report)
        {
            try
            {
                if (report == null
                    || string.IsNullOrWhiteSpace(report.ReporterUserID)
                    || string.IsNullOrWhiteSpace(report.TargetType)
                    || string.IsNullOrWhiteSpace(report.TargetID))
                {
                    Console.WriteLine("SubmitReport failed: missing reporter or target.");
                    return false;
                }

                report.TargetType = report.TargetType.Trim();
                report.TargetID = report.TargetID.Trim();
                report.Category = string.IsNullOrWhiteSpace(report.Category) ? "Other" : report.Category.Trim();
                report.Subject = report.Subject?.Trim();
                report.ReportText = report.ReportText?.Trim();
                report.Status = "Open";
                report.ReportDate = DateTime.UtcNow.ToString("O");

                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.ReportRepository.Insert(report);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        // All reports, newest first — used by the admin Reports page.
        [HttpGet]
        public List<Report> GetAllReports()
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.ReportRepository.ReadAll();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new List<Report>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        // Admin action: mark a report as resolved.
        [HttpPost]
        public bool ResolveReport(string reportId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportId)) return false;
                this.libraryUOW.HelperOledb.OpenConnection();
                Report report = new Report { ReportID = reportId, Status = "Resolved" };
                return this.libraryUOW.ReportRepository.Update(report);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }
    }
}
