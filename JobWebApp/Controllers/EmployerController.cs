using JobWebApp;
using JobModels;
using JobModels.ViewModels;
using LibraryWSClient;
using Microsoft.AspNetCore.Mvc;

namespace JobWebApp.Controllers
{
    public class EmployerController : Controller
    {
        private const string WS_HOST = "localhost";
        private const int WS_PORT = 5015;
        private const string WS_SCHEME = "http";

        // ── Guard: only employers allowed ─────────────────────
        private bool IsAuthorized()
        {
            return SessionHelper.IsEmployer(HttpContext.Session);
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

        // ── GET: /Employer/Home ────────────────────────────────
        // Shows the employer's homepage — their posted jobs
        public async Task<IActionResult> Home()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string employerId = SessionHelper.GetUserID(HttpContext.Session)!;

            // Call the dedicated endpoint that returns only this employer's jobs
            ApiClient<List<Job>> client = BuildClient<List<Job>>("Employer", "GetMyJobs");
            client.AddParameter("employerId", employerId);
            List<Job> myJobs = await client.GetAsync() ?? new List<Job>();

            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Jobs = myJobs;

            return View();
        }

        // ── GET: /Employer/YourJobs ────────────────────────────
        // Shows the jobs this employer has posted, paged for the larger cards.
        public async Task<IActionResult> YourJobs(int page = 1)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            const int pageSize = 6;

            string employerId = SessionHelper.GetUserID(HttpContext.Session)!;

            // Call the dedicated endpoint — no client-side filtering needed
            ApiClient<List<Job>> client = BuildClient<List<Job>>("Employer", "GetMyJobs");
            client.AddParameter("employerId", employerId);
            List<Job> myJobs = await client.GetAsync() ?? new List<Job>();

            int totalPages = Math.Max(1, (int)Math.Ceiling(myJobs.Count / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalJobs = myJobs.Count;
            ViewBag.Jobs = myJobs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return View();
        }

        // ── GET: /Employer/JobDetails ──────────────────────────
        // Individual job page for the employer's own posting.
        public async Task<IActionResult> JobDetails(string jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<Job> client = BuildClient<Job>("Guest", "GetJob");
            client.AddParameter("JobID", jobId);
            Job? job = await client.GetAsync();

            // Only let an employer open the detail page for jobs they own.
            string employerId = SessionHelper.GetUserID(HttpContext.Session)!;
            if (job == null || !string.Equals(job.EmployerID, employerId, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "That job could not be found.";
                return RedirectToAction("YourJobs");
            }

            ViewBag.Job = job;
            ViewBag.Role = "Employer";
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            return View("~/Views/Shared/JobDetails.cshtml");
        }

        // ── GET: /Employer/PostJob ─────────────────────────────
        // Shows the form to post a new job — loads genres from the database
        [HttpGet]
        public async Task<IActionResult> PostJob()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            // Fetch genres from the database so the dropdown is populated live
            ApiClient<List<Genre>> genreClient = BuildClient<List<Genre>>("Guest", "GetAllGenres");
            List<Genre> genres = await genreClient.GetAsync() ?? new List<Genre>();

            // Fetch job types so their IDs (not names) can be posted to the DB
            ApiClient<List<JobType>> jobTypeClient = BuildClient<List<JobType>>("Guest", "GetAllJobTypes");
            List<JobType> jobTypes = await jobTypeClient.GetAsync() ?? new List<JobType>();

            PostJobViewModel model = new PostJobViewModel();
            model.Genres = genres;
            model.JobTypes = jobTypes;

            return View(model);
        }

        // ── POST: /Employer/PostJob ────────────────────────────
        // Called when the employer submits the post job form
        [HttpPost]
        public async Task<IActionResult> PostAJob(Job job)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            // Attach the employer's ID to the job before saving
            job.EmployerID = SessionHelper.GetUserID(HttpContext.Session);
            job.JobStatus = true; // active by default
            // JobID is generated by the database when the job is inserted.

            ApiClient<Job> client = BuildClient<Job>("Employer", "AddJob");
            bool success = await client.PostAsync(job);

            TempData[success ? "Success" : "Error"] = success
                ? "Job posted successfully!"
                : "Failed to post job. Please check all fields (Title, Description, Job Type, Genre, and Location) are filled correctly.";

            return success
                ? RedirectToAction("Home")
                : RedirectToAction("PostJob");
        }

        // ── POST: /Employer/DeleteJob ──────────────────────────
        // Called when the employer clicks delete on a job
        [HttpPost]
        public async Task<IActionResult> DeleteJob(string jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<bool> client = BuildClient<bool>("Employer", "DeleteJob");
            client.AddParameter("jobId", jobId);
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Job deleted."
                : "Failed to delete job.";

            return RedirectToAction("Home");
        }

        // ── GET: /Employer/Applicants ──────────────────────────
        // Shows all applicants for a specific job
        public async Task<IActionResult> Applicants(string jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<List<JobApplication>> client = BuildClient<List<JobApplication>>("Employer", "Applications");
            client.AddParameter("jobId", jobId);
            List<JobApplication> applicants = await client.GetAsync() ?? new List<JobApplication>();

            // Fetch user profiles for each applicant
            Dictionary<string, User> users = new Dictionary<string, User>();
            foreach (var app in applicants)
            {
                string userId = app.EmployeeId.ToString();
                ApiClient<User> userClient = BuildClient<User>("User", "GetUser");
                userClient.AddParameter("userId", userId);
                User? user = await userClient.GetAsync();
                if (user != null)
                {
                    users[userId] = user;
                }
            }

            ViewBag.Applicants = applicants;
            ViewBag.Users = users;
            ViewBag.JobID = jobId;

            return View();
        }

        // ── POST: /Employer/UpdateSalary ───────────────────────
        // Updates an employee's salary
        [HttpPost]
        public async Task<IActionResult> UpdateSalary(string userId, decimal salary)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<bool> client = BuildClient<bool>("Employer", "UpdateSalary");
            client.AddParameter("userId", userId);
            client.AddParameter("salary", salary.ToString());
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Salary updated."
                : "Failed to update salary.";

            return RedirectToAction("Applicants", new { jobId = Request.Form["jobId"] });
        }

        // ── POST: /Employer/UpdateApplicationStatus ───────────
        // Accept or reject a candidate's application for one of this employer's jobs
        [HttpPost]
        public async Task<IActionResult> UpdateApplicationStatus(string applicationId, string status, string jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<bool> client = BuildClient<bool>("Employer", "UpdateApplicationStatus");
            client.AddParameter("applicationId", applicationId);
            client.AddParameter("status", status);
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? $"Application marked as {status}."
                : "Failed to update application.";

            return RedirectToAction("Applicants", new { jobId });
        }

        // ── GET: /Employer/Profile ─────────────────────────────
        public async Task<IActionResult> Profile()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string employerId = SessionHelper.GetUserID(HttpContext.Session)!;

            // Fetch job count for this employer
            ApiClient<List<Job>> jobClient = BuildClient<List<Job>>("Employer", "GetMyJobs");
            jobClient.AddParameter("employerId", employerId);
            List<Job> myJobs = await jobClient.GetAsync() ?? new List<Job>();

            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.UserName = SessionHelper.GetUserName(HttpContext.Session);
            ViewBag.JobCount = myJobs.Count;

            return View();
        }

        // ── POST: /Employer/ToggleJobStatus ───────────────────
        [HttpPost]
        public async Task<IActionResult> ToggleJobStatus(string jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<bool> client = BuildClient<bool>("Employer", "ToggleJobStatus");
            client.AddParameter("jobId", jobId);
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Job status updated."
                : "Failed to update job status.";

            return RedirectToAction("YourJobs");
        }

        // ── GET: /Employer/Chat ───────────────────────────────
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