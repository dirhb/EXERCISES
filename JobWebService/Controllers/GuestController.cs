using JobModels;
using JobWebService.ORM.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobWebService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GuestController : ControllerBase
    {
        LibraryUOW libraryUOW;

        public GuestController()
        {
            this.libraryUOW = new LibraryUOW();
        }

        [HttpGet]
        public List<Job> GetAllJobs()
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.JobRepository.ReadAll(); // SORT IT TO PAGES LATER NIGGER
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }
        [HttpGet]
        public Job GetJob(string JobID) // giving a job id, get the job details
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.JobRepository.Read(JobID);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

    }
}
