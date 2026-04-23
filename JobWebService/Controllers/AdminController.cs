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
        public bool UpdateJob(Job job)
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
        public bool DeleteJob(string jobId)
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
        public bool DeleteUser(string userId)
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
        public List<JobApplication> ReviewApplication(string? status)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                if (!string.IsNullOrWhiteSpace(status))
                    return this.libraryUOW.JobApplicationRepository.ReadByStatus(status);

                return this.libraryUOW.JobApplicationRepository.ReadAll()
                    .OrderByDescending(x => x.SubmittedAtUTC)
                    .ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new List<JobApplication>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
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