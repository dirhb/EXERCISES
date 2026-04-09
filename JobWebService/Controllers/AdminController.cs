using JobModels;
using JobWebService.ORM.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AdminController : ControllerBase
    {
        LibraryUOW libraryUOW;

        public AdminController()
        {
            this.libraryUOW = new LibraryUOW();
        }

        [HttpPost]
        public bool ManageJobs(Job job)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.JobRepository.Update(job);
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

        [HttpPost]
        public bool DeleteJobs(string jobId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.JobRepository.Delete(jobId);
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

        [HttpPost]
        public bool DeleteUsers(string userId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.Delete(userId);
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

        [HttpGet]
        public List<JobApplicationLog> ReviewApplication(string? status)
        {
            try
            {
                List<JobApplicationLog> result = UseCaseMemoryStore.Applications;

                if (!string.IsNullOrWhiteSpace(status))
                    result = result.Where(x => (x.Status ?? "").Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

                return result.OrderByDescending(x => x.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new List<JobApplicationLog>();
            }
        }

        [HttpPost]
        public bool NotifyAboutWebsiteChanges(string text)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                Notification model = new Notification();
                model.NotificationID = Guid.NewGuid().ToString("N");
                model.NotificationText = text;
                model.NotificationDate = DateTime.UtcNow.ToString("O");

                return this.libraryUOW.NotificationRepository.Insert(model);
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