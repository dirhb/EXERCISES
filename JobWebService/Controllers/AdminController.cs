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

        // Returns all users — used by Admin Notify page for user search
        [HttpGet]
        public List<User> GetAllUsers()
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.ReadAll();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new List<User>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        // Returns latest notifications — used by the notification bell.
        // When a userId is supplied, the user sees broadcasts plus the
        // notifications targeted at them ("[To User {id}]: ..."), with the
        // targeting prefix stripped off for display. Notifications aimed at
        // other users are filtered out.
        [HttpGet]
        public List<Notification> GetNotifications(string? userId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                List<Notification> all = this.libraryUOW.NotificationRepository.ReadAll()
                    .OrderByDescending(n => n.NotificationDate)
                    .ToList();

                const string targetPrefix = "[To User ";
                List<Notification> visible = new List<Notification>();

                foreach (Notification notification in all)
                {
                    string text = notification.NotificationText ?? string.Empty;

                    if (!text.StartsWith(targetPrefix))
                    {
                        // Broadcast — everyone sees it
                        visible.Add(notification);
                    }
                    else if (!string.IsNullOrWhiteSpace(userId))
                    {
                        // Targeted — only the addressed user sees it
                        string marker = $"[To User {userId}]:";
                        if (text.StartsWith(marker))
                        {
                            notification.NotificationText = text.Substring(marker.Length).Trim();
                            visible.Add(notification);
                        }
                    }
                }

                return visible.Take(10).ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new List<Notification>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }
    }
}