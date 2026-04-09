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
        public bool CheckPassword(string userId, string password)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                string password2 = libraryUOW.UserRepository.GetPasswordByUserId(userId);
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
                UseCaseMemoryStore.UserResumes[userId] = resumeText;
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }

        [HttpPost]
        public JobApplicationLog? ApplyToJob(string userId, string jobId)
        {
            try
            {
                JobApplicationLog app = new JobApplicationLog();
                app.ApplicationID = Guid.NewGuid().ToString("N");
                app.UserID = userId;
                app.JobID = jobId;
                app.ResumeText = UseCaseMemoryStore.UserResumes.GetValueOrDefault(userId, string.Empty);
                app.Status = "Submitted";
                app.CreatedAt = DateTime.UtcNow.ToString("O");

                UseCaseMemoryStore.Applications.Add(app);

                if (!UseCaseMemoryStore.UserJobHistory.ContainsKey(userId))
                    UseCaseMemoryStore.UserJobHistory[userId] = new List<string>();

                if (!UseCaseMemoryStore.UserJobHistory[userId].Contains(jobId))
                    UseCaseMemoryStore.UserJobHistory[userId].Add(jobId);

                return app;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        [HttpGet]
        public JobHistory GetJobHistory(string userId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                List<string> ids = UseCaseMemoryStore.UserJobHistory.GetValueOrDefault(userId, new List<string>());
                List<Job> historyJobs = new List<Job>();

                foreach (string id in ids)
                {
                    Job? job = this.libraryUOW.JobRepository.Read(id);
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