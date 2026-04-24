using JobModels;
using JobModels.ViewModels;
using JobWebService.ORM.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        LibraryUOW libraryUOW;

        public UserController()
        {
            this.libraryUOW = new LibraryUOW();
        }

        [HttpPost]
        public bool Register(User user)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.Create(user);
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
        [ActionName("UpdateUser")]
        public bool UpdateUser(User user)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return libraryUOW.UserRepository.Update(user);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
            return false;
        }

        [HttpPost]
        [ActionName("UpdatePassword")]
        public bool UpdatePassword(User user)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return libraryUOW.UserRepository.UpdatePassword(user);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
            return false;
        }

        [HttpGet]
        public bool CheckPassword(string username, string password)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                string password2 = libraryUOW.UserRepository.GetPasswordByUserName(username);
                return password2 != null && password2.Equals(password);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
            return false;
        }

        [HttpGet]
        public bool IsAvailableUserName(string username)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return !libraryUOW.UserRepository.IsExistUserName(username);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
            return false;
        }

        [HttpPost]
        public bool UpdateOnlineResume(string userId, string resumeText)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.UpdateResume(userId, resumeText);
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
        public bool ApplyToJob(int userId, int jobId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                User? user = this.libraryUOW.UserRepository.Read(userId);

                if (user == null)
                    return false;

                JobApplication app = new JobApplication();
                //app.ApplicationId = Guid.NewGuid().ToString("N");
                app.EmployeeId = userId;
                app.JobId = jobId;
                app.ResumeSnapshot = user.ResumeText ?? string.Empty;
                app.Status = "Submitted";
                app.SubmittedAtUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                bool ok = this.libraryUOW.JobApplicationRepository.Create(app);

                return ok;
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
        public JobHistory GetJobHistory(string userId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                List<JobApplication> apps = this.libraryUOW.JobApplicationRepository.ReadByUserId(userId);
                List<Job> historyJobs = new List<Job>();

                foreach (JobApplication app in apps)
                {
                    if (app.JobId <= 0)
                        continue;

                    Job? job = this.libraryUOW.JobRepository.Read(app.JobId);
                    if (job != null)
                        historyJobs.Add(job);
                }

                JobHistory vm = new JobHistory();
                vm.JobHistoryList = historyJobs;
                vm.PageNumber = 1;
                vm.Pages = 1;
                vm.Genres = this.libraryUOW.GenreRepository.ReadAll();
                vm.Employers = historyJobs.Select(x => x.EmployerID ?? "").Distinct().ToList();
                return vm;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new JobHistory
                {
                    JobHistoryList = new List<Job>(),
                    Genres = new List<Genre>(),
                    Employers = new List<string>(),
                    PageNumber = 1,
                    Pages = 1
                };
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }
    }
}