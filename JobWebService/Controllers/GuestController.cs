using JobModels;
using JobModels.ViewModels;
using JobWebService.ORM.Repositories;
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
                return this.libraryUOW.JobRepository.ReadAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Job>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        [HttpGet]
        public Job? GetJob(string JobID)
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

        [HttpGet]
        public HomePage GetHomePage()
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                List<Job> jobs = this.libraryUOW.JobRepository.ReadAll();
                List<Review> reviews = this.libraryUOW.ReviewRepository.ReadAll();

                HomePage vm = new HomePage();
                vm.PopularJobs = jobs.Take(10).ToList();
                vm.JobMatches = jobs.Take(10).ToList();
                vm.RecentReviews = reviews.Take(10).ToList();
                return vm;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new HomePage
                {
                    PopularJobs = new List<Job>(),
                    JobMatches = new List<Job>(),
                    RecentReviews = new List<Review>()
                };
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        [HttpGet]
        public JobCatalog GetJobCatalog(string? location, string? employmentType, string? salaryRange, string? experienceLevel, string? flexibility)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                List<Job> jobs = this.libraryUOW.JobRepository.ReadAll();

                if (!string.IsNullOrWhiteSpace(location))
                    jobs = jobs.Where(x => x.CountryID == location).ToList();

                if (!string.IsNullOrWhiteSpace(employmentType))
                    jobs = jobs.Where(x => x.JobType == employmentType).ToList();

                if (!string.IsNullOrWhiteSpace(experienceLevel))
                    jobs = jobs.Where(x => (x.JobFilter ?? "").Contains(experienceLevel, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrWhiteSpace(flexibility))
                    jobs = jobs.Where(x => (x.JobFilter ?? "").Contains(flexibility, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!string.IsNullOrWhiteSpace(salaryRange))
                    jobs = jobs.Where(x => (x.JobFilter ?? "").Contains(salaryRange, StringComparison.OrdinalIgnoreCase)).ToList();

                JobCatalog vm = new JobCatalog();
                vm.Jobs = jobs;
                vm.PageNumber = 1;
                vm.Pages = 1;
                vm.Genres = this.libraryUOW.GenreRepository.ReadAll();
                vm.Employers = jobs.Select(x => x.EmployerID ?? "").Distinct().ToList();
                return vm;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new JobCatalog
                {
                    Jobs = new List<Job>(),
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

        [HttpGet]
        public bool Login(string userId, string password)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                string passInDb = this.libraryUOW.UserRepository.GetPasswordByUserId(userId);
                return passInDb != null && passInDb.Equals(password);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        [HttpGet]
        public string NavigationTool(string pageName)
        {
            return "Navigated to " + pageName;
        }
    }
}