using JobWebApp;
using JobModels;
using LibraryWSClient;
using Microsoft.AspNetCore.Mvc;
using JobModels.ViewModels;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace JobWebApp.Controllers
{
    public class EmployeeController : Controller
    {
        private const string WS_HOST = "localhost";
        private const int WS_PORT = 5015;
        private const string WS_SCHEME = "http";

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
        public async Task<IActionResult> JobCatalog(string? search, string? employerId, string? genreId, string? jobType, bool showActiveOnly = false, int page = 1)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            // Smaller page size now that each job is shown as a large, detailed card.
            const int pageSize = 8;

            ApiClient<List<Job>> client = BuildClient<List<Job>>("Guest", "GetAllJobs");
            List<Job> allJobs = await client.GetAsync() ?? new List<Job>();

            List<string> employers = allJobs
                .Select(job => job.EmployerID ?? string.Empty)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(id => id)
                .ToList();

            // Fetch genres live from the database so filter values match real GenreIDs
            ApiClient<List<Genre>> genreClient = BuildClient<List<Genre>>("Guest", "GetAllGenres");
            List<Genre> genres = await genreClient.GetAsync() ?? new List<Genre>();

            IEnumerable<Job> filteredJobs = allJobs;

            if (!string.IsNullOrWhiteSpace(search))
                filteredJobs = filteredJobs.Where(job => MatchesJobSearch(job, search));

            if (!string.IsNullOrWhiteSpace(employerId))
                filteredJobs = filteredJobs.Where(job => string.Equals(job.EmployerID, employerId, StringComparison.OrdinalIgnoreCase));

            // Genre filter: match by GenreID (numeric key) OR GenreTitle (text) to handle
            // Access DB type differences. Trim both sides to eliminate whitespace mismatches.
            if (!string.IsNullOrWhiteSpace(genreId))
                filteredJobs = filteredJobs.Where(job =>
                    string.Equals((job.GenreID ?? "").Trim(), genreId.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    string.Equals((job.GenreTitle ?? "").Trim(), genreId.Trim(), StringComparison.OrdinalIgnoreCase));

            // Job type filter (Full-time, Part-time, Contract, Internship, Remote)
            if (!string.IsNullOrWhiteSpace(jobType))
                filteredJobs = filteredJobs.Where(job => string.Equals((job.JobType ?? "").Trim(), jobType.Trim(), StringComparison.OrdinalIgnoreCase));

            if (showActiveOnly)
                filteredJobs = filteredJobs.Where(job => job.JobStatus == true);

            List<Job> results = filteredJobs.ToList();
            int totalPages = Math.Max(1, (int)Math.Ceiling(results.Count / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Search = search;
            ViewBag.SelectedEmployer = employerId;
            ViewBag.SelectedGenre = genreId;
            ViewBag.SelectedJobType = jobType;
            ViewBag.ShowActiveOnly = showActiveOnly;
            ViewBag.Employers = employers;
            ViewBag.Genres = genres;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalJobs = results.Count;
            ViewBag.Jobs = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return View();
        }

        // ── GET: /Employee/JobDetails ─────────────────────────
        // Dedicated individual job page, reached by clicking a catalog card.
        public async Task<IActionResult> JobDetails(string jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<Job> client = BuildClient<Job>("Guest", "GetJob");
            client.AddParameter("JobID", jobId);
            Job? job = await client.GetAsync();

            if (job == null)
            {
                TempData["Error"] = "That job could not be found.";
                return RedirectToAction("JobCatalog");
            }

            ViewBag.Job = job;
            ViewBag.Role = "Employee";
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            return View("~/Views/Shared/JobDetails.cshtml");
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
            // Use CountryName (populated by SQL JOIN in UserRepository) instead of the raw Country ID
            ViewBag.CountryName = user?.CountryName ?? user?.Country ?? string.Empty;

            return View();
        }

        // ── POST: /Employee/UpdateResume ───────────────────────
        // Called when the employee saves their resume
        [HttpPost]
        public async Task<IActionResult> UpdateResume(IFormFile resumeFile)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            if (resumeFile == null || resumeFile.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction("Resume");
            }

            // Server-side validation
            var extension = Path.GetExtension(resumeFile.FileName).ToLower();
            if (extension != ".pdf")
            {
                TempData["Error"] = "Only PDF files are allowed.";
                return RedirectToAction("Resume");
            }

            if (resumeFile.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "File size must be less than 5MB.";
                return RedirectToAction("Resume");
            }

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;
            bool success = false;
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(resumeFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await resumeFile.CopyToAsync(fileStream);
                }

                var fileUrl = $"/uploads/{uniqueFileName}";

                ApiClient<bool> client = BuildClient<bool>("User", "UpdateOnlineResume");
                client.AddParameter("userId", userId);
                client.AddParameter("resumeText", fileUrl);
                success = await client.PostAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred during file upload: {ex.Message}";
                return RedirectToAction("Resume");
            }

            TempData[success ? "Success" : "Error"] = success
                ? "Resume uploaded successfully!"
                : "Failed to save resume to database. Please try again.";

            return RedirectToAction("Resume");
        }

        // ── POST: /Employee/ApplyToJob ─────────────────────────
        // Called when the employee clicks "Apply" on a job
        [HttpPost]
        public async Task<IActionResult> ApplyToJob(int jobId, IFormFile? resumeFile, string? returnUrl)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;
            string? resumeUrl = null;

            if (resumeFile != null && resumeFile.Length > 0)
            {
                // Server-side validation
                var extension = Path.GetExtension(resumeFile.FileName).ToLower();
                if (extension != ".pdf")
                {
                    TempData["Error"] = "Only PDF files are allowed.";
                    return RedirectToReturnUrl(returnUrl);
                }

                if (resumeFile.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "File size must be less than 5MB.";
                    return RedirectToReturnUrl(returnUrl);
                }

                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(resumeFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await resumeFile.CopyToAsync(fileStream);
                    }

                    resumeUrl = $"/uploads/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"An error occurred during file upload: {ex.Message}";
                    return RedirectToReturnUrl(returnUrl);
                }
            }

            ApiClient<bool> client = BuildClient<bool>("User", "ApplyToJob");
            client.AddParameter("userId", userId);
            client.AddParameter("jobId", jobId.ToString());
            if (resumeUrl != null)
            {
                client.AddParameter("resumeUrl", resumeUrl);
            }
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Application submitted successfully!"
                : "Failed to apply. You may have already applied to this job.";

            return RedirectToReturnUrl(returnUrl);
        }

        private IActionResult RedirectToReturnUrl(string? returnUrl)
        {
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

        // ── GET: /Employee/Chat ───────────────────────────────
        // Shared chat view for messaging
        public IActionResult Chat(string? partnerId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ViewBag.CurrentUserID = SessionHelper.GetUserID(HttpContext.Session);
            ViewBag.PartnerID = partnerId;
            ViewBag.UserTypeID = SessionHelper.GetUserTypeID(HttpContext.Session);
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);

            return View("~/Views/Shared/Chat.cshtml");
        }
    }
}