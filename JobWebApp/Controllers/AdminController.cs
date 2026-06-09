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

            ApiClient<bool> client = BuildClient<bool>("Admin", "NotifyAboutWebsiteChanges");
            client.AddParameter("text", $"[To User {userId}]: {text}");
            bool success = await client.PostAsync();

            TempData[success ? "Success" : "Error"] = success
                ? $"Notification sent to user {userId}."
                : "Failed to send notification.";

            return RedirectToAction("Notify");
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