using JobWebService.ORM.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SavedJobController : ControllerBase
    {
        LibraryUOW libraryUOW;

        public SavedJobController()
        {
            this.libraryUOW = new LibraryUOW();
        }

        [HttpPost]
        public bool SaveJob(string userId, string jobId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(jobId)) return false;
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.SavedJobRepository.Save(userId, jobId);
            }
            catch (Exception ex) { Trace.WriteLine(ex); return false; }
            finally { this.libraryUOW.HelperOledb.CloseConnection(); }
        }

        [HttpPost]
        public bool UnsaveJob(string userId, string jobId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(jobId)) return false;
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.SavedJobRepository.Unsave(userId, jobId);
            }
            catch (Exception ex) { Trace.WriteLine(ex); return false; }
            finally { this.libraryUOW.HelperOledb.CloseConnection(); }
        }

        [HttpGet]
        public List<string> GetSavedJobIds(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId)) return new List<string>();
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.SavedJobRepository.GetJobIds(userId);
            }
            catch (Exception ex) { Trace.WriteLine(ex); return new List<string>(); }
            finally { this.libraryUOW.HelperOledb.CloseConnection(); }
        }
    }
}
