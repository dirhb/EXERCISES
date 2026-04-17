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
        public List<JobApplicationLog> ReviewApplication(string? status)
        {
            List<JobApplicationLog> result = UseCaseMemoryStore.Applications;
            List<JobApplicationLog> newList = new List<JobApplicationLog>();
            try
            {
                if (status != null)
                {
                    foreach (var item in result)
                    {
                        if (item.Status != null)
                        {
                            if (item.Status.ToLower() == status.ToLower())
                            {
                                newList.Add(item);
                            }
                        }
                    }

                    result = newList;
                }
                return result.OrderByDescending(x => x.CreatedAt).ToList(); //ORDER BY in SQL instead
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
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