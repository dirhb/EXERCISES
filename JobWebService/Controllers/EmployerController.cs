using JobModels;
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
        public bool AddJob(Job job)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.JobRepository.Create(job);
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
        public List<JobApplicationLog> Applications(string jobId, string? resumeWord)
        {
            try
            {
                List<JobApplicationLog> result = UseCaseMemoryStore.Applications
                    .Where(x => x.JobID == jobId)
                    .ToList();

                if (!string.IsNullOrWhiteSpace(resumeWord))
                    result = result.Where(x => (x.ResumeText ?? "").Contains(resumeWord, StringComparison.OrdinalIgnoreCase)).ToList();

                return result.OrderByDescending(x => x.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new List<JobApplicationLog>();
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
                // salary field is not in current Users table, so keep it in memory for now
                UseCaseMemoryStore.UserSalaries[userId] = salary;
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }
    }
}