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
                return this.libraryUOW.UserRepository.UpdateSalary(userId, salary);
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