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
        public bool Register([FromBody] User user)
        {
            try
            {
                Console.WriteLine($"Registering user: {user.UserName}, Email: {user.Email}, TypeID: {user.UserTypeID}");
                this.libraryUOW.HelperOledb.OpenConnection();

                user.UserName = user.UserName?.Trim();
                user.Email = user.Email?.Trim();

                if (string.IsNullOrWhiteSpace(user.UserName)
                    || string.IsNullOrWhiteSpace(user.Email)
                    || string.IsNullOrWhiteSpace(user.Password))
                {
                    Console.WriteLine("Register failed: missing username, email, or password.");
                    return false;
                }

                if (this.libraryUOW.UserRepository.IsExistUserName(user.UserName))
                {
                    Console.WriteLine("Register failed: username already exists.");
                    return false;
                }

                if (this.libraryUOW.UserRepository.ExistsByEmail(user.Email))
                {
                    Console.WriteLine("Register failed: email already exists.");
                    return false;
                }

                user.UserTypeID ??= 2;
                user.CreationDate ??= DateTime.UtcNow.ToString("yyyy-MM-dd");

                bool result = this.libraryUOW.UserRepository.Create(user);
                Console.WriteLine($"Register result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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

        [HttpGet]
        public User? GetUser(string userId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.Read(userId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
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
        public bool ApplyToJob(string userId, string jobId, string? resumeUrl = null)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                User? user = this.libraryUOW.UserRepository.Read(userId);

                if (user == null)
                    return false;

                // Parse string IDs to int for the JobApplication model
                int.TryParse(userId, out int employeeIdInt);
                int.TryParse(jobId, out int jobIdInt);

                JobApplication app = new JobApplication();
                app.EmployeeId = employeeIdInt;
                app.JobId = jobIdInt;
                app.ResumeSnapshot = resumeUrl ?? user.ResumeText ?? string.Empty;
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