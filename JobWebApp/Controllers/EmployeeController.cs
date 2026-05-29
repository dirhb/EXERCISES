using JobWebApp;
using JobModels;
using LibraryWSClient;
using Microsoft.AspNetCore.Mvc;
using JobModels.ViewModels;

namespace JobWebApp.Controllers
{
    public class EmployeeController : Controller
    {
        private const string WS_HOST = "localhost";
        private const int WS_PORT = 5015;
        private const string WS_SCHEME = "http";

        private static readonly List<string> CatalogGenres = new List<string>
        {
            "Art",
            "Law",
            "Information Technology",
            "Healthcare",
            "Education",
            "Business, Management, and Finance",
            "STEM",
            "Agriculture, Food, and Natural Resources",
            "Government",
            "Communication"
        };

        // ── Guard: make sure only employees can access this ────
        // This is called at the start of every method.
        // If the user isn't an employee, send them away.
        // Think of it like a bouncer at the employee-only door.
        private bool IsAuthorized()
        {
            return SessionHelper.IsEmployee(HttpContext.Session);
        }

        private ApiClient<T> BuildClient<T>(string controller, string action)
        {
            ApiClient<T> client = new ApiClient<T>();
            client.Scheme = WS_SCHEME;
            client.Host = WS_HOST;
            client.Port = WS_PORT;
            client.Path = $"api/{controller}/{action}";
            return client;
        }

        // ── GET: /Employee/Home ────────────────────────────────
        // Shows the homepage with job listings the employee can apply to
        public async Task<IActionResult> Home()
        {
            // Not an employee? Send them back to guest homepage
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<List<Job>> client = BuildClient<List<Job>>("Guest", "GetAllJobs");
            List<Job> jobs = (await client.GetAsync() ?? new List<Job>())
                .Where(job => job.JobStatus != false)
                .ToList();

            // Pass their name to the view so we can say "Welcome back, John!"
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Jobs = jobs.Take(6).ToList();

            return View();
        }


        // ── GET: /Employee/JobCatalog ─────────────────────────
        // Shows all jobs with search, employer/genre filters, and paging.
        public async Task<IActionResult> JobCatalog(string? search, string? employerId, string? genreId, int page = 1)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            const int pageSize = 18;

            ApiClient<List<Job>> client = BuildClient<List<Job>>("Guest", "GetAllJobs");
            List<Job> allJobs = (await client.GetAsync() ?? new List<Job>())
                .Where(job => job.JobStatus != false)
                .ToList();

            List<string> employers = allJobs
                .Select(job => job.EmployerID ?? string.Empty)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(id => id)
                .ToList();

            List<string> genres = CatalogGenres;

            IEnumerable<Job> filteredJobs = allJobs;

            if (!string.IsNullOrWhiteSpace(search))
                filteredJobs = filteredJobs.Where(job => MatchesJobSearch(job, search));

            if (!string.IsNullOrWhiteSpace(employerId))
                filteredJobs = filteredJobs.Where(job => string.Equals(job.EmployerID, employerId, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(genreId))
                filteredJobs = filteredJobs.Where(job => string.Equals(job.GenreID, genreId, StringComparison.OrdinalIgnoreCase));

            List<Job> results = filteredJobs.ToList();
            int totalPages = Math.Max(1, (int)Math.Ceiling(results.Count / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Search = search;
            ViewBag.SelectedEmployer = employerId;
            ViewBag.SelectedGenre = genreId;
            ViewBag.Employers = employers;
            ViewBag.Genres = genres;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalJobs = results.Count;
            ViewBag.Jobs = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return View();
        }

        // ── GET: /Employee/JobHistory ──────────────────────────
        // Shows all the jobs this employee has applied to
        public async Task<IActionResult> JobHistory()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            // Get the logged-in employee's ID from the session
            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<JobHistory> client = BuildClient<JobHistory>("User", "GetJobHistory");
            client.AddParameter("userId", userId);
            JobHistory history = await client.GetAsync() ?? new JobHistory();

            ViewBag.History = history;

            return View();
        }

        // ── GET: /Employee/Resume ──────────────────────────────
        // Shows the employee's online resume with their saved text
        public async Task<IActionResult> Resume()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<User> client = BuildClient<User>("User", "GetUser");
            client.AddParameter("userId", userId);
            User? user = await client.GetAsync();

            ViewBag.UserID = userId;
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.ResumeText = user?.ResumeText ?? string.Empty;
            ViewBag.Email = user?.Email ?? string.Empty;
            ViewBag.PhoneNum = user?.PhoneNum ?? string.Empty;
            ViewBag.Country = user?.Country ?? string.Empty;

            return View();
        }

        // ── POST: /Employee/UpdateResume ───────────────────────
        // Called when the employee saves their resume
        [HttpPost]
        public async Task<IActionResult> UpdateResume(string resumeText)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<bool> client = BuildClient<bool>("User", "UpdateOnlineResume");
            client.AddParameter("userId", userId);
            client.AddParameter("resumeText", resumeText ?? string.Empty);
            bool success = await client.PostAsync();

            // TempData is like a sticky note — it lasts for exactly one page load
            // Perfect for "saved successfully!" messages
            TempData[success ? "Success" : "Error"] = success
                ? "Resume saved successfully!"
                : "Failed to save resume. Please try again.";

            return RedirectToAction("Resume");
        }

        // ── POST: /Employee/ApplyToJob ─────────────────────────
        // Called when the employee clicks "Apply" on a job
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> ApplyToJob(int jobId, string? returnUrl)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<bool> client = BuildClient<bool>("User", "ApplyToJob");
            client.AddParameter("userId", userId);
            client.AddParameter("jobId", jobId.ToString());
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Application submitted successfully!"
                : "Failed to apply. You may have already applied to this job.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Home");
        }

        private static bool MatchesJobSearch(Job job, string search)
        {
            return (job.JobTitle ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.JobDescription ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.JobType ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.JobFilter ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.EmployerID ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.CountryID ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.GenreID ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        // ── GET: /Employee/Profile ─────────────────────────────
        // Shows the employee's profile page
        public IActionResult Profile()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            // Pass session info to the view
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.UserName = SessionHelper.GetUserName(HttpContext.Session);
            ViewBag.UserID = SessionHelper.GetUserID(HttpContext.Session);

            return View();
        }
    }
}