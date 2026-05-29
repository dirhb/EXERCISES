// GuestController.cs
// This controller handles everything a non-logged-in user can do:
// - View the homepage
// - View job listings
// - Login
// - Register
//
// URL pattern: /Guest/Home, /Guest/Login etc.

using JobWebApp;
using JobModels;
using LibraryWSClient;
using Microsoft.AspNetCore.Mvc;
using JobModels.ViewModels;

namespace JobWebApp.Controllers
{
    public class GuestController : Controller
    {
        // The base URL of your web service
        private const string WS_HOST = "localhost";
        private const int WS_PORT = 5015;
        private const string WS_SCHEME = "http";

        // ── Helper: builds an ApiClient pointed at the right place ──
        // Instead of repeating the host/port/scheme setup in every method,
        // we have one private helper that does it for us.
        private ApiClient<T> BuildClient<T>(string controller, string action)
        {
            ApiClient<T> client = new ApiClient<T>();
            client.Scheme = WS_SCHEME;
            client.Host = WS_HOST;
            client.Port = WS_PORT;
            client.Path = $"api/{controller}/{action}";
            return client;
        }

        // ── GET: /Guest/Home ───────────────────────────────────
        // The main homepage — shows featured jobs and stats
        // Anyone can see this (guest, employee, employer)
        public async Task<IActionResult> Home(string? search)
        {
            // If user is already logged in, redirect them to their dashboard
            if (SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                return RedirectBasedOnRole();
            }

            // Call the web service to get all jobs
            ApiClient<List<Job>> client = BuildClient<List<Job>>("Guest", "GetAllJobs");
            List<Job> jobs = await client.GetAsync() ?? new List<Job>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                jobs = jobs
                    .Where(job => MatchesJobSearch(job, search))
                    .ToList();
            }

            // Pass featured jobs to the view
            ViewBag.Search = search;
            ViewBag.Jobs = jobs.Take(6).ToList();
            ViewBag.TotalJobs = jobs.Count;

            return View();
        }

        // ── GET: /Guest/Login ──────────────────────────────────
        // Shows the login/register page
        public IActionResult Login()
        {
            // If already logged in, no need to be here
            if (SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectBasedOnRole();

            return View();
        }

        // ── POST: /Guest/Login ─────────────────────────────────
        // Called when the user submits the login form
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            ApiClient<User> client = BuildClient<User>("Guest", "GetByCredentials");
            client.AddParameter("email", email);
            client.AddParameter("password", password);
            User user = await client.GetAsync();

            if (user == null)
            {
                TempData["Error"] = "Invalid email or password.";
                return View();
            }

            // Save user info to session
            SessionHelper.SetUser(
                HttpContext.Session,
                user.UserID!,
                user.UserTypeID ?? 1,
                user.UserName!,
                $"{user.FirstName} {user.LastName}"
            );

            return RedirectBasedOnRole();
        }

        // ── POST: /Guest/Register ──────────────────────────────
        // Called when the user submits the register form
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            // Debug — check what data we're sending
            Console.WriteLine($"Sending register: {user.UserName}, {user.Email}, {user.UserTypeID}");

            user.CreationDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            ApiClient<User> client = BuildClient<User>("User", "Register");
            bool success = await client.PostAsync(user);

            Console.WriteLine($"Register response: {success}");

            if (!success)
            {
                TempData["Error"] = "Registration failed. Please check that the username/email are not already used and try again.";
                return RedirectToAction("Login");
            }

            TempData["Success"] = "Account created! Please sign in.";
            return RedirectToAction("Login");
        }

        // ── GET: /Guest/Logout ─────────────────────────────────
        // Clears the session and sends the user back to homepage
        public IActionResult Logout()
        {
            SessionHelper.ClearUser(HttpContext.Session);
            return RedirectToAction("Home");
        }

        private static bool MatchesJobSearch(Job job, string search)
        {
            return (job.JobTitle ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.JobDescription ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.JobType ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.JobFilter ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.EmployerID ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase)
                || (job.CountryID ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        // ── Private helper: redirect based on role ─────────────
        // After login, send the user to the right homepage for their role
        private IActionResult RedirectBasedOnRole()
        {
            if (SessionHelper.IsEmployee(HttpContext.Session))
                return RedirectToAction("Home", "Employee");

            if (SessionHelper.IsEmployer(HttpContext.Session))
                return RedirectToAction("Home", "Employer");

            if (SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToAction("Home", "Admin");

            // Default — guest
            return RedirectToAction("Home", "Guest");
        }
    }
}