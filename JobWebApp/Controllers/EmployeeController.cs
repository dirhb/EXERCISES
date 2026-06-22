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

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<List<Job>> client = BuildClient<List<Job>>("Guest", "GetAllJobs");
            List<Job> jobs = (await client.GetAsync() ?? new List<Job>())
                .Where(job => job.JobStatus != false)
                .ToList();

            // ── Real dashboard stats ──
            ApiClient<JobHistory> historyClient = BuildClient<JobHistory>("User", "GetJobHistory");
            historyClient.AddParameter("userId", userId);
            JobHistory history = await historyClient.GetAsync() ?? new JobHistory();

            ApiClient<List<User>> convClient = BuildClient<List<User>>("Chat", "GetConversations");
            convClient.AddParameter("userId", userId);
            int messageCount = (await convClient.GetAsync() ?? new List<User>()).Count;

            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Jobs = jobs.Take(6).ToList();
            ViewBag.TotalJobs = jobs.Count;
            ViewBag.ApplicationCount = history.AppliedJobs?.Count ?? 0;
            ViewBag.SavedCount = (await FetchSavedJobIds(userId)).Count;
            ViewBag.MessageCount = messageCount;

            return View();
        }


        // ── GET: /Employee/JobCatalog ─────────────────────────
        // Shows all jobs with search, employer/genre filters, and paging.
        public async Task<IActionResult> JobCatalog(string? search, string? employerId, string? genreId, string? jobType, decimal? minSalary = null, decimal? maxSalary = null, bool showActiveOnly = false, int page = 1)
        {
            // Open to everyone — guests may browse; actions (apply/message) require login.

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

            // Salary range filter — jobs without a salary are excluded once a bound is set.
            if (minSalary.HasValue)
                filteredJobs = filteredJobs.Where(job => job.Salary.HasValue && job.Salary.Value >= minSalary.Value);

            if (maxSalary.HasValue)
                filteredJobs = filteredJobs.Where(job => job.Salary.HasValue && job.Salary.Value <= maxSalary.Value);

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
            ViewBag.MinSalary = minSalary;
            ViewBag.MaxSalary = maxSalary;
            ViewBag.ShowActiveOnly = showActiveOnly;
            ViewBag.Employers = employers;
            ViewBag.Genres = genres;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalJobs = results.Count;
            ViewBag.Jobs = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            HashSet<string> savedJobIds = new HashSet<string>();
            if (SessionHelper.IsEmployee(HttpContext.Session))
                savedJobIds = (await FetchSavedJobIds(SessionHelper.GetUserID(HttpContext.Session)!)).ToHashSet();
            ViewBag.SavedJobIds = savedJobIds;

            return View();
        }

        // ── GET: /Employee/JobDetails ─────────────────────────
        // Dedicated individual job page, reached by clicking a catalog card.
        public async Task<IActionResult> JobDetails(string jobId)
        {
            // Open to everyone — guests may view job details; applying requires login.

            ApiClient<Job> client = BuildClient<Job>("Guest", "GetJob");
            client.AddParameter("JobID", jobId);
            Job? job = await client.GetAsync();

            if (job == null)
            {
                TempData["Error"] = "That job could not be found.";
                return RedirectToAction("JobCatalog");
            }

            await PopulateEmployerRating(job);

            bool isSaved = false;
            if (SessionHelper.IsEmployee(HttpContext.Session))
                isSaved = (await FetchSavedJobIds(SessionHelper.GetUserID(HttpContext.Session)!)).Contains(jobId);
            ViewBag.IsSaved = isSaved;

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
        // Saves the employee's online resume as plain text.
        [HttpPost]
        public async Task<IActionResult> UpdateResume(string resumeText)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<bool> client = BuildClient<bool>("User", "UpdateOnlineResume");
            client.AddParameter("userId", userId);
            client.AddParameter("resumeText", resumeText ?? string.Empty);
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Resume saved!"
                : "Couldn't save your resume. Please try again.";

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
                : "Sorry — something went wrong submitting your application. Please try again.";

            return RedirectToReturnUrl(returnUrl);
        }

        private IActionResult RedirectToReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction("Home");
        }

        private async Task<List<string>> FetchSavedJobIds(string userId)
        {
            ApiClient<List<string>> client = BuildClient<List<string>>("SavedJob", "GetSavedJobIds");
            client.AddParameter("userId", userId);
            return await client.GetAsync() ?? new List<string>();
        }

        // ── POST: /Employee/ToggleSavedJob ────────────────────
        // Saves the job if it isn't saved yet, otherwise un-saves it.
        [HttpPost]
        public async Task<IActionResult> ToggleSavedJob(string jobId, string? returnUrl)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;
            bool isSaved = (await FetchSavedJobIds(userId)).Contains(jobId);

            ApiClient<bool> client = BuildClient<bool>("SavedJob", isSaved ? "UnsaveJob" : "SaveJob");
            client.AddParameter("userId", userId);
            client.AddParameter("jobId", jobId);
            bool ok = await client.PostAsync();

            TempData[ok ? "Success" : "Error"] = ok
                ? (isSaved ? "Removed from saved jobs." : "Job saved.")
                : "Couldn't update your saved jobs.";

            return RedirectToReturnUrl(returnUrl);
        }

        // ── GET: /Employee/SavedJobs ──────────────────────────
        public async Task<IActionResult> SavedJobs()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;
            List<string> savedIds = await FetchSavedJobIds(userId);
            List<Job> allJobs = await BuildClient<List<Job>>("Guest", "GetAllJobs").GetAsync() ?? new List<Job>();

            ViewBag.Jobs = allJobs.Where(j => savedIds.Contains(j.JobID ?? string.Empty)).ToList();
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            return View();
        }

        // Loads the employer's reviews + average rating into ViewBag for the detail page.
        private async Task PopulateEmployerRating(Job job)
        {
            List<Review> reviews = new List<Review>();
            if (!string.IsNullOrEmpty(job.EmployerID))
            {
                ApiClient<List<Review>> reviewClient = BuildClient<List<Review>>("Review", "GetReviewsForEmployer");
                reviewClient.AddParameter("employerId", job.EmployerID);
                reviews = await reviewClient.GetAsync() ?? new List<Review>();
            }

            ViewBag.EmployerReviews = reviews;
            ViewBag.EmployerRatingCount = reviews.Count;
            ViewBag.EmployerRatingAvg = reviews.Count > 0
                ? Math.Round(reviews.Where(r => r.RatingTitle.HasValue).Select(r => (double)r.RatingTitle!.Value).DefaultIfEmpty(0).Average(), 1)
                : 0.0;

            // An employee may review this employer only after being accepted by them.
            bool canReview = false;
            if (SessionHelper.IsEmployee(HttpContext.Session) && !string.IsNullOrEmpty(job.EmployerID))
            {
                ApiClient<bool> canReviewClient = BuildClient<bool>("Review", "CanReview");
                canReviewClient.AddParameter("userId", SessionHelper.GetUserID(HttpContext.Session));
                canReviewClient.AddParameter("employerId", job.EmployerID);
                canReview = await canReviewClient.GetAsync();
            }
            ViewBag.CanReview = canReview;
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
        public async Task<IActionResult> Profile()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;
            ApiClient<User> userClient = BuildClient<User>("User", "GetUser");
            userClient.AddParameter("userId", userId);
            User? user = await userClient.GetAsync();
            List<Country> countries = await BuildClient<List<Country>>("Guest", "GetAllCountries").GetAsync() ?? new List<Country>();

            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.UserName = SessionHelper.GetUserName(HttpContext.Session);
            ViewBag.UserID = userId;
            ViewBag.User = user;
            ViewBag.Countries = countries;

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