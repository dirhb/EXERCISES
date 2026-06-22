using JobWebApp;
using JobModels;
using LibraryWSClient;
using Microsoft.AspNetCore.Mvc;

namespace JobWebApp.Controllers
{
    public class AdminController : Controller
    {
        private const string WS_HOST = "localhost";
        private const int WS_PORT = 5015;
        private const string WS_SCHEME = "http";

        // ── Guard: only admins allowed ─────────────────────────
        private bool IsAuthorized()
        {
            return SessionHelper.IsAdmin(HttpContext.Session);
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

        // ── GET: /Admin/Home ───────────────────────────────────
        // Admin dashboard — shows all jobs on the platform
        public async Task<IActionResult> Home()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<List<Job>> client = BuildClient<List<Job>>("Guest", "GetAllJobs");
            List<Job> jobs = await client.GetAsync() ?? new List<Job>();

            ViewBag.FullName = SessionHelper.GetFullName(HttpContext.Session);
            ViewBag.Jobs = jobs;
            ViewBag.TotalJobs = jobs.Count;

            return View();
        }

        // ── GET: /Admin/Applications ───────────────────────────
        // Shows all job applications across the platform
        public async Task<IActionResult> Applications(string? status)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<List<JobApplication>> client = BuildClient<List<JobApplication>>("Admin", "ReviewApplication");

            if (!string.IsNullOrEmpty(status))
                client.AddParameter("status", status);

            List<JobApplication> applications = await client.GetAsync() ?? new List<JobApplication>();

            // Build id→name lookups so the view can show job titles and applicant
            // names instead of raw IDs.
            List<Job> allJobs = await BuildClient<List<Job>>("Guest", "GetAllJobs").GetAsync() ?? new List<Job>();
            List<User> allUsers = await BuildClient<List<User>>("Admin", "GetAllUsers").GetAsync() ?? new List<User>();

            ViewBag.JobTitles = allJobs
                .Where(j => !string.IsNullOrEmpty(j.JobID))
                .GroupBy(j => j.JobID!)
                .ToDictionary(g => g.Key, g => g.First().JobTitle ?? "Untitled job");
            ViewBag.UserNames = allUsers
                .Where(u => !string.IsNullOrEmpty(u.UserID))
                .GroupBy(u => u.UserID!)
                .ToDictionary(g => g.Key, g => $"{g.First().FirstName} {g.First().LastName}".Trim());

            ViewBag.Applications = applications;
            ViewBag.StatusFilter = status;

            return View();
        }

        // ── POST: /Admin/DeleteJob ─────────────────────────────
        // Admin can delete any job on the platform
        [HttpPost]
        public async Task<IActionResult> DeleteJob(string jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<bool> client = BuildClient<bool>("Admin", "DeleteJob");
            client.AddParameter("jobId", jobId);
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Job deleted."
                : "Failed to delete job.";

            return RedirectToAction("Home");
        }

        // ── POST: /Admin/SendNotification ──────────────────────
        // Broadcasts a notification to all users
        [HttpPost]
        public async Task<IActionResult> SendNotification(string text)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<bool> client = BuildClient<bool>("Admin", "NotifyAboutWebsiteChanges");
            client.AddParameter("text", text);
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Notification sent to all users!"
                : "Failed to send notification.";

            return RedirectToAction("Home");
        }

        // ── GET: /Admin/Notify ─────────────────────────────────
        // Notify page: search users and send targeted notifications
        public async Task<IActionResult> Notify(string? userSearch)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            List<User> users = new List<User>();

            if (!string.IsNullOrWhiteSpace(userSearch))
            {
                ApiClient<List<User>> client = BuildClient<List<User>>("Admin", "GetAllUsers");
                List<User> allUsers = await client.GetAsync() ?? new List<User>();
                users = allUsers.Where(u =>
                    (u.FirstName ?? "").Contains(userSearch, StringComparison.OrdinalIgnoreCase) ||
                    (u.LastName ?? "").Contains(userSearch, StringComparison.OrdinalIgnoreCase) ||
                    (u.UserName ?? "").Contains(userSearch, StringComparison.OrdinalIgnoreCase) ||
                    (u.Email ?? "").Contains(userSearch, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            ViewBag.Users = users;
            ViewBag.UserSearch = userSearch;
            return View();
        }

        // ── POST: /Admin/SendNotificationToUser ────────────────
        // Sends a notification targeted at a specific user
        [HttpPost]
        public async Task<IActionResult> SendNotificationToUser(string userId, string text)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<bool> client = BuildClient<bool>("Admin", "NotifyUser");
            client.AddParameter("userId", userId);
            client.AddParameter("text", text);
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? $"Notification sent to user {userId}."
                : "Failed to send notification.";

            return RedirectToAction("Notify");
        }

        // ── GET: /Admin/Reports ────────────────────────────────
        // Shows all reports submitted by users, newest first.
        public async Task<IActionResult> Reports()
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            List<Report> reports = await BuildClient<List<Report>>("Report", "GetAllReports").GetAsync() ?? new List<Report>();
            List<User> allUsers = await BuildClient<List<User>>("Admin", "GetAllUsers").GetAsync() ?? new List<User>();

            ViewBag.UserNames = allUsers
                .Where(u => !string.IsNullOrEmpty(u.UserID))
                .GroupBy(u => u.UserID!)
                .ToDictionary(g => g.Key, g => $"{g.First().FirstName} {g.First().LastName}".Trim());
            ViewBag.Reports = reports;

            return View();
        }

        // ── POST: /Admin/ResolveReport ─────────────────────────
        [HttpPost]
        public async Task<IActionResult> ResolveReport(string reportId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            ApiClient<bool> client = BuildClient<bool>("Report", "ResolveReport");
            client.AddParameter("reportId", reportId);
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? "Report marked as resolved."
                : "Failed to update report.";

            return RedirectToAction("Reports");
        }

        // ── POST: /Admin/BanReportedUser ───────────────────────
        // Bans the reported user, then resolves the report.
        [HttpPost]
        public async Task<IActionResult> BanReportedUser(string reportId, string userId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            bool banned = false;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                ApiClient<bool> banClient = BuildClient<bool>("Admin", "BanUser");
                banClient.AddParameter("userId", userId);
                banned = await banClient.PostAsync();
            }

            await ResolveReportInternal(reportId);

            TempData[banned ? "Success" : "Error"] = banned
                ? "User banned — they can no longer sign in. Report resolved."
                : "Could not ban that user.";

            return RedirectToAction("Reports");
        }

        // ── POST: /Admin/DeleteReportedJob ─────────────────────
        // Deletes the reported job, then resolves the report.
        [HttpPost]
        public async Task<IActionResult> DeleteReportedJob(string reportId, string jobId)
        {
            if (!IsAuthorized()) return RedirectToAction("Home", "Guest");

            bool deleted = false;
            if (!string.IsNullOrWhiteSpace(jobId))
            {
                ApiClient<bool> delClient = BuildClient<bool>("Admin", "DeleteJob");
                delClient.AddParameter("jobId", jobId);
                deleted = await delClient.PostAsync();
            }

            await ResolveReportInternal(reportId);

            TempData[deleted ? "Success" : "Error"] = deleted
                ? "Reported job deleted. Report resolved."
                : "Could not delete that job.";

            return RedirectToAction("Reports");
        }

        private async Task ResolveReportInternal(string reportId)
        {
            if (string.IsNullOrWhiteSpace(reportId)) return;
            ApiClient<bool> resolveClient = BuildClient<bool>("Report", "ResolveReport");
            resolveClient.AddParameter("reportId", reportId);
            await resolveClient.PostAsync();
        }

        // ── GET: /Admin/Profile ────────────────────────────────
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