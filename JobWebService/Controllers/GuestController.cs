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
        public bool Login(string username, string password)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                string stored = this.libraryUOW.UserRepository.GetPasswordByUserName(username);
                return PasswordHasher.Verify(password, stored)
                    || (!PasswordHasher.LooksHashed(stored) && stored != null && stored == password);
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

        [HttpGet]
        public User? GetByCredentials(string email, string password)
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();

                User user = this.libraryUOW.UserRepository.ReadByEmail(email);
                if (user == null) return null;

                // Banned accounts can't sign in.
                if (user.IsBanned == true) return null;

                string stored = user.Password ?? string.Empty;

                // Normal case: verify the supplied password against the stored hash.
                if (PasswordHasher.Verify(password, stored))
                {
                    user.Password = null;
                    return user;
                }

                // Legacy plaintext password: accept the correct one once, then
                // upgrade it to a hash so it's never stored in clear text again.
                if (!PasswordHasher.LooksHashed(stored) && stored == password)
                {
                    user.Password = PasswordHasher.Hash(password);
                    this.libraryUOW.UserRepository.UpdatePassword(user);
                    user.Password = null;
                    return user;
                }

                return null;
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

        // Returns all genres from the database
        // Used by the frontend to populate dropdowns and filter sidebars
        [HttpGet]
        public List<Genre> GetAllGenres()
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.GenreRepository.ReadAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Genre>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        // Returns all job types from the database
        // Used to populate the Job Type dropdown when posting a job
        [HttpGet]
        public List<JobType> GetAllJobTypes()
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.JobTypeRepository.ReadAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<JobType>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        [HttpGet]
        public List<Country> GetAllCountries()
        {
            try
            {
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.CountryRepository.ReadAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Country>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }
    }
}
