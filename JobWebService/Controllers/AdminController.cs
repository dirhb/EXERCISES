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

        // Ban a user (they can no longer log in). Reversible via UnbanUser.
        [HttpPost]
        public bool BanUser(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId)) return false;
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.SetBanned(userId, true);
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
        public bool UnbanUser(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId)) return false;
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.SetBanned(userId, false);
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

        // Broadcast — visible to every user (RecipientUserID left null).
        [HttpPost]
        public bool NotifyAboutWebsiteChanges(string text)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                Notification model = new Notification();
                model.NotificationText = text;
                model.NotificationDate = DateTime.UtcNow.ToString("O");
                model.RecipientUserID = null;

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

        // Targeted — only the addressed user will see this notification.
        [HttpPost]
        public bool NotifyUser(string userId, string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                this.libraryUOW.HelperOledb.OpenConnection();
                Notification model = new Notification();
                model.NotificationText = text;
                model.NotificationDate = DateTime.UtcNow.ToString("O");
                model.RecipientUserID = userId;

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

        // Returns the latest notifications for the bell: broadcasts plus the
        // ones targeted at this user. Targeting is done with the real
        // RecipientUserID column, so other users' notifications never leak.
        [HttpGet]
        public List<Notification> GetNotifications(string? userId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                // ReadForUser already returns newest-first (ordered by NotificationID).
                return this.libraryUOW.NotificationRepository.ReadForUser(userId ?? string.Empty)
                    .Take(10)
                    .ToList();
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