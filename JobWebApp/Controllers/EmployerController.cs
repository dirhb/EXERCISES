using JobWebApp;
using JobModels;
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

            // Get ALL jobs then filter to only this employer's jobs
            // In a real app you'd have a GetJobsByEmployer endpoint
            string employerId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<List<Job>> client = BuildClient<List<Job>>("Guest", "GetAllJobs");
            List<Job> allJobs = await client.GetAsync() ?? new List<Job>();

            // Filter to only show THIS employer's jobs
            List<Job> myJobs = allJobs
                .Where(j => j.EmployerID == employerId)
                .ToList();

            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Jobs = myJobs;

            return View();
        }

        // ── GET: /Employer/JobHistory ──────────────────────────
        // Shows the jobs this employer has posted (their history)
        public async Task<IActionResult> JobHistory()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            string employerId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<List<Job>> client = BuildClient<List<Job>>("Guest", "GetAllJobs");
            List<Job> allJobs = await client.GetAsync() ?? new List<Job>();

            List<Job> myJobs = allJobs
                .Where(j => j.EmployerID == employerId)
                .ToList();

            ViewBag.Jobs = myJobs;

            return View();
        }

        // ── GET: /Employer/PostJob ─────────────────────────────
        // Shows the form to post a new job
        public IActionResult PostJob()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");
            return View();
        }

        // ── POST: /Employer/PostJob ────────────────────────────
        // Called when the employer submits the post job form
        [HttpPost]
        public async Task<IActionResult> PostJob(Job job)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            // Attach the employer's ID to the job before saving
            job.EmployerID = SessionHelper.GetUserID(HttpContext.Session);
            job.JobStatus = true; // active by default
            job.JobID = Guid.NewGuid().ToString("N"); // generate a unique ID

            ApiClient<Job> client = BuildClient<Job>("Employer", "AddJob");
            bool success = await client.PostAsync(job);

            TempData[success ? "Success" : "Error"] = success
                ? "Job posted successfully!"
                : "Failed to post job. Please try again.";

            return RedirectToAction("Home");
        }

        // ── POST: /Employer/DeleteJob ──────────────────────────
        // Called when the employer clicks delete on a job
        [HttpPost]
        public async Task<IActionResult> DeleteJob(string jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<bool> client = BuildClient<bool>("Employer", "DeleteJob");
            client.AddParameter("jobId", jobId);
            bool success = await client.GetAsync();

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

            ViewBag.Applicants = applicants;
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
            bool success = await client.GetAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Salary updated."
                : "Failed to update salary.";

            return RedirectToAction("Applicants", new { jobId = Request.Form["jobId"] });
        }

        // ── GET: /Employer/Profile ─────────────────────────────
        public IActionResult Profile()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.UserName = SessionHelper.GetUserName(HttpContext.Session);
            ViewBag.UserID = SessionHelper.GetUserID(HttpContext.Session);

            return View();
        }
    }
}