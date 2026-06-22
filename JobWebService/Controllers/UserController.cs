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
                user.Password = PasswordHasher.Hash(user.Password);

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

        // Updates the user's preferred display currency.
        [HttpPost]
        public bool UpdateCurrency(string userId, string currency)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId)) return false;
                string code = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();

                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.UpdateCurrency(userId, code);
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

        // Updates profile fields (name, email, phone, country) — not the password.
        [HttpPost]
        public bool UpdateProfile([FromBody] User user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.UserID)) return false;
                if (string.IsNullOrWhiteSpace(user.Email)) return false;

                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.UserRepository.UpdateProfile(user);
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
        [ActionName("UpdatePassword")]
        public bool UpdatePassword(User user)
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Password)) return false;
                user.Password = PasswordHasher.Hash(user.Password);

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

        // Resets a password after verifying the email + username pair. Used by
        // the "Forgot your password?" flow (no email infrastructure required).
        [HttpPost]
        public bool ResetPassword(string email, string username, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email)
                    || string.IsNullOrWhiteSpace(username)
                    || string.IsNullOrWhiteSpace(newPassword))
                    return false;

                this.libraryUOW.HelperOledb.OpenConnection();

                User user = this.libraryUOW.UserRepository.ReadByEmail(email.Trim());
                if (user == null) return false;
                if (!string.Equals(user.UserName, username.Trim(), StringComparison.OrdinalIgnoreCase)) return false;

                user.Password = PasswordHasher.Hash(newPassword);
                return this.libraryUOW.UserRepository.UpdatePassword(user);
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
        public bool CheckPassword(string username, string password)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                string stored = libraryUOW.UserRepository.GetPasswordByUserName(username);
                return PasswordHasher.Verify(password, stored)
                    || (!PasswordHasher.LooksHashed(stored) && stored != null && stored == password);
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

                // Don't insert duplicate applications. If this user already
                // applied to this job, treat it as success — it's already in
                // their job history — rather than piling up identical rows.
                List<JobApplication> existing = this.libraryUOW.JobApplicationRepository.ReadByUserId(userId);
                if (existing.Any(a => a.JobId == jobIdInt))
                    return true;

                JobApplication app = new JobApplication();
                app.EmployeeId = employeeIdInt;
                app.JobId = jobIdInt;
                app.ResumeSnapshot = resumeUrl ?? user.ResumeText ?? string.Empty;
                app.Status = "Submitted";
                app.SubmittedAtUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                bool ok = this.libraryUOW.JobApplicationRepository.Create(app);

                // Trigger-based notification: tell the employer about the new application.
                if (ok)
                {
                    Job? job = this.libraryUOW.JobRepository.Read(jobId);
                    if (job != null && !string.IsNullOrWhiteSpace(job.EmployerID))
                    {
                        Notification note = new Notification
                        {
                            NotificationText = $"New application received for '{job.JobTitle}'.",
                            NotificationDate = DateTime.UtcNow.ToString("O"),
                            RecipientUserID = job.EmployerID
                        };
                        this.libraryUOW.NotificationRepository.Insert(note);
                    }
                }

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
                List<AppliedJob> appliedJobs = new List<AppliedJob>();

                foreach (JobApplication app in apps)
                {
                    if (app.JobId <= 0)
                        continue;

                    Job? job = this.libraryUOW.JobRepository.Read(app.JobId);
                    if (job != null)
                    {
                        historyJobs.Add(job);
                        appliedJobs.Add(new AppliedJob
                        {
                            Job = job,
                            Status = app.Status,
                            AppliedAt = app.SubmittedAtUTC,
                            OfferedSalary = app.OfferedSalary
                        });
                    }
                }

                JobHistory vm = new JobHistory();
                vm.JobHistoryList = historyJobs;
                vm.AppliedJobs = appliedJobs;
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
                    AppliedJobs = new List<AppliedJob>(),
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