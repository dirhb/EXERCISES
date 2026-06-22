using JobModels;
using JobModels.ViewModels;
using JobWebService.ORM.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class EmployerController : ControllerBase
    {
        LibraryUOW libraryUOW;

        public EmployerController()
        {
            this.libraryUOW = new LibraryUOW();
        }

        [HttpPost]
        public bool AddJob([FromBody] Job job)
        {
            try
            {
                if (job == null
                    || string.IsNullOrWhiteSpace(job.JobTitle)
                    || string.IsNullOrWhiteSpace(job.JobDescription)
                    || string.IsNullOrWhiteSpace(job.JobType)
                    || string.IsNullOrWhiteSpace(job.EmployerID)
                    || string.IsNullOrWhiteSpace(job.CountryID)
                    || string.IsNullOrWhiteSpace(job.GenreID))
                {
                    Console.WriteLine("AddJob failed: missing required job fields.");
                    return false;
                }

                job.JobTitle = job.JobTitle.Trim();
                job.JobDescription = job.JobDescription.Trim();
                job.JobType = job.JobType.Trim();
                job.CountryID = job.CountryID.Trim();
                job.GenreID = job.GenreID.Trim();
                job.JobFilter = job.JobFilter?.Trim();
                job.JobStatus ??= true;

                this.libraryUOW.HelperOledb.OpenConnection();
                bool result = this.libraryUOW.JobRepository.Create(job);
                Console.WriteLine($"AddJob result: {result}");
                return result;
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

        [HttpGet]
        public List<JobApplication> Applications(string jobId, string? resumeWord)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                List<JobApplication> result = this.libraryUOW.JobApplicationRepository.ReadByJobId(jobId);

                if (!string.IsNullOrWhiteSpace(resumeWord))
                    result = result.Where(x => (x.ResumeSnapshot ?? "").Contains(resumeWord, StringComparison.OrdinalIgnoreCase)).ToList();

                return result.OrderByDescending(x => x.SubmittedAtUTC).ToList();
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
        public bool UpdateApplicationStatus(string applicationId, string status)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                bool ok = this.libraryUOW.JobApplicationRepository.UpdateStatus(applicationId, status);

                // Trigger-based notification: tell the applicant their status changed.
                if (ok)
                {
                    JobApplication app = this.libraryUOW.JobApplicationRepository.Read(applicationId);
                    if (app != null)
                    {
                        Job? job = this.libraryUOW.JobRepository.Read(app.JobId.ToString());
                        string title = job?.JobTitle ?? "a job";
                        Notification note = new Notification
                        {
                            NotificationText = $"Your application for '{title}' was {(status ?? "").ToLower()}.",
                            NotificationDate = DateTime.UtcNow.ToString("O"),
                            RecipientUserID = app.EmployeeId.ToString()
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

        [HttpPost]
        public bool FireEmployee(string userId)
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

        [HttpPost]
        public bool UpdateSalary(string userId, decimal salary)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                bool updated = this.libraryUOW.UserRepository.UpdateSalary(userId, salary);

                // Trigger-based notification: let the employee know their offer changed.
                if (updated)
                {
                    Notification note = new Notification();
                    note.NotificationText = $"Your offered salary was updated to {salary:0.##} USD.";
                    note.NotificationDate = DateTime.UtcNow.ToString("O");
                    note.RecipientUserID = userId;
                    this.libraryUOW.NotificationRepository.Insert(note);
                }

                return updated;
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
        // Sets the salary offered for one specific application and notifies the applicant.
        [HttpPost]
        public bool UpdateOfferedSalary(string applicationId, decimal salary)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                bool ok = this.libraryUOW.JobApplicationRepository.UpdateOfferedSalary(applicationId, salary);

                if (ok)
                {
                    JobApplication app = this.libraryUOW.JobApplicationRepository.Read(applicationId);
                    if (app != null)
                    {
                        Job? job = this.libraryUOW.JobRepository.Read(app.JobId.ToString());
                        string title = job?.JobTitle ?? "a job";
                        Notification note = new Notification
                        {
                            NotificationText = $"You received a salary offer of {salary:0.##} USD for '{title}'.",
                            NotificationDate = DateTime.UtcNow.ToString("O"),
                            RecipientUserID = app.EmployeeId.ToString()
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

        // All Accepted applications across this employer's jobs (the "hired" list).
        [HttpGet]
        public List<JobApplication> GetAcceptedApplications(string employerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employerId)) return new List<JobApplication>();
                this.libraryUOW.HelperOledb.OpenConnection();

                HashSet<string> jobIds = this.libraryUOW.JobRepository.ReadByEmployer(employerId)
                    .Select(j => j.JobID)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase)!;

                return this.libraryUOW.JobApplicationRepository.ReadAll()
                    .Where(a => jobIds.Contains(a.JobId.ToString())
                                && string.Equals(a.Status, "Accepted", StringComparison.OrdinalIgnoreCase))
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

        // Total number of applications across all of this employer's jobs.
        [HttpGet]
        public int CountApplicants(string employerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employerId)) return 0;
                this.libraryUOW.HelperOledb.OpenConnection();

                HashSet<string> jobIds = this.libraryUOW.JobRepository.ReadByEmployer(employerId)
                    .Select(j => j.JobID)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase)!;

                return this.libraryUOW.JobApplicationRepository.ReadAll()
                    .Count(a => jobIds.Contains(a.JobId.ToString()));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return 0;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        [HttpGet]
        public List<Job> GetMyJobs(string employerId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.JobRepository.ReadByEmployer(employerId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new List<Job>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        [HttpPost]
        public bool ToggleJobStatus(string jobId)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                Job? job = this.libraryUOW.JobRepository.Read(jobId);
                if (job == null) return false;
                job.JobStatus = !(job.JobStatus ?? false);
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
    }
}