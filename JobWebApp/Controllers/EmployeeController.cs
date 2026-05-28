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
            List<Job> jobs = await client.GetAsync() ?? new List<Job>();

            // Pass their name to the view so we can say "Welcome back, John!"
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Jobs = jobs;

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

            // We reuse GetJobHistory to also get user info
            // In a real app you'd have a GetUser endpoint
            ViewBag.UserID = userId;
            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);

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
            client.AddParameter("resumeText", resumeText);
            bool success = await client.GetAsync();

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
        public async Task<IActionResult> ApplyToJob(int jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<bool> client = BuildClient<bool>("User", "ApplyToJob");
            client.AddParameter("userId", userId);
            client.AddParameter("jobId", jobId.ToString());
            bool success = await client.GetAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Application submitted successfully!"
                : "Failed to apply. You may have already applied to this job.";

            return RedirectToAction("Home");
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